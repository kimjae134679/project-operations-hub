using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AIControlTower.Services;

public sealed record AiCompletionContract(string Adapter, string RequestedModel, string? RequiredReportPath = null);
public sealed record AiCompletionReceipt(int SchemaVersion, string Adapter, string ExecutionId, string RequestedModel,
    string Status, int? ProcessExitCode, bool TerminalObserved, bool ReportObserved, bool ReportRequired,
    bool? ArtifactVerified, string? ArtifactSha256, string? FailureCode);

/// <summary>Explicit adapter only. No stderr, arbitrary command output, or report body is persisted.</summary>
public sealed class AiCompletionEvaluator
{
    public const int MaxLineCharacters = 65536;
    public const int MaxOutputBytes = 512 * 1024;
    public const int MaxArtifactBytes = 1024 * 1024;
    private readonly AiCompletionContract _contract;
    private readonly string _root, _executionId;
    private readonly DateTimeOffset _startedAt;
    private string? _report, _failure;
    private int _bytes, _wrappers, _terminals;
    private bool _terminal, _reportObserved;
    public string? PreflightFailure { get; }
    public bool OutputLimitExceeded { get; private set; }

    public AiCompletionEvaluator(AiCompletionContract contract, string approvedWorkingRoot, string executionId, DateTimeOffset startedAt)
    {
        _contract = contract; _root = Path.GetFullPath(approvedWorkingRoot); _executionId = executionId; _startedAt = startedAt;
        if (contract.Adapter is not ("codex-jsonl-v1" or "wrapper-json-v1") || !JevExecutionPolicy.IsExplicitAllowedModel(contract.RequestedModel))
            _failure = "invalid_contract";
        try
        {
            if (contract.RequiredReportPath is not null)
            {
                _report = SafeReportPath(contract.RequiredReportPath);
                if (File.Exists(_report) || Directory.Exists(_report)) _failure ??= "report_preexisting";
            }
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException or NotSupportedException)
        { _failure ??= "report_path_invalid"; }
        PreflightFailure = _failure;
    }
    public void ObserveStdout(string line)
    {
        var lineBytes = Encoding.UTF8.GetByteCount(line);
        if (line.Length > MaxLineCharacters || lineBytes > MaxLineCharacters || _bytes >= MaxOutputBytes - lineBytes)
        { OutputLimitExceeded = true; _failure ??= "output_limit"; return; }
        _bytes += lineBytes + 1;
        if (string.IsNullOrWhiteSpace(line)) return;
        try
        {
            using var document = JsonDocument.Parse(line, new JsonDocumentOptions { MaxDepth = 32 });
            var json = document.RootElement;
            if (json.ValueKind != JsonValueKind.Object || HasDuplicateProperties(json)) { _failure ??= "invalid_json"; return; }
            if (_contract.Adapter == "wrapper-json-v1")
            {
                _wrappers++;
                var state = String(json, "state");
                _terminal = state is "completed" or "succeeded" or "failed";
                _reportObserved = Boolean(json, "reportPresent");
                if (_wrappers != 1 || state is not ("completed" or "succeeded") || !json.TryGetProperty("exitCode", out var exit)
                    || exit.ValueKind != JsonValueKind.Number || !exit.TryGetInt32(out var code) || code != 0 || !_reportObserved
                    || String(json, "model") != _contract.RequestedModel)
                    _failure ??= "wrapper_failed";
                return;
            }
            var type = String(json, "type");
            if (type is "error" or "turn.failed" || json.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
                _failure ??= "ai_failed";
            // Explicit item errors are semantic failures; an ordinary tool's nonzero
            // exit/status alone may be recovered by the model and is not a turn error.
            if (type is not null && type.StartsWith("item.", StringComparison.Ordinal)
                && json.TryGetProperty("item", out var errorItem) && errorItem.ValueKind == JsonValueKind.Object
                && (String(errorItem, "type") == "error"
                    || errorItem.TryGetProperty("error", out var nestedError) && nestedError.ValueKind != JsonValueKind.Null))
                _failure ??= "ai_failed";
            if (type == "turn.completed")
            {
                _terminal = true;
                if (++_terminals != 1) _failure ??= "contradictory_terminal";
            }
            if (type == "item.completed" && json.TryGetProperty("item", out var item) && item.ValueKind == JsonValueKind.Object
                && String(item, "type") == "agent_message" && !string.IsNullOrWhiteSpace(String(item, "text"))) _reportObserved = true;
            if (type is null) _failure ??= "invalid_event";
            if (json.TryGetProperty("model", out var model) && model.ValueKind == JsonValueKind.String && model.GetString() != _contract.RequestedModel)
                _failure ??= "model_mismatch";
        }
        catch (JsonException) { _failure ??= "invalid_json"; }
    }
    public AiCompletionReceipt Complete(int? exitCode, string processState)
    {
        bool? artifactVerified = _contract.RequiredReportPath is null ? null : false;
        string? hash = null;
        if (exitCode != 0 || processState != "succeeded") _failure = "process_" + processState;
        if (!_terminal) _failure ??= "terminal_missing";
        if (!_reportObserved) _failure ??= "report_missing";
        if (_report is not null && _failure is null)
        {
            try
            {
                var path = SafeReportPath(_contract.RequiredReportPath!);
                var before = new FileInfo(path);
                if (!before.Exists || before.Length is < 1 or > MaxArtifactBytes || before.LastWriteTimeUtc < _startedAt.UtcDateTime)
                    _failure = "artifact_missing_stale_or_oversized";
                else
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (stream.Length != before.Length) _failure = "artifact_changed";
                    else
                    {
                        hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
                        var after = new FileInfo(SafeReportPath(_contract.RequiredReportPath!));
                        if (after.Length != before.Length || after.LastWriteTimeUtc != before.LastWriteTimeUtc) _failure = "artifact_changed";
                        else artifactVerified = true;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            { _failure = "artifact_unreadable"; }
        }
        if (_failure is not null) { hash = null; if (artifactVerified.HasValue) artifactVerified = false; }
        return new(1, _contract.Adapter, _executionId, _contract.RequestedModel, _failure is null ? "succeeded" : "failed",
            exitCode, _terminal, _reportObserved, _contract.RequiredReportPath is not null, artifactVerified, hash, _failure);
    }
    private string SafeReportPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Split(['/', '\\']).Any(p => p == ".." || p.Equals("Desktop", StringComparison.OrdinalIgnoreCase) || p == "바탕화면"))
            throw new ArgumentException("invalid report path");
        var path = Path.GetFullPath(value, _root);
        if (!path.StartsWith(_root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || path.AsSpan(Path.GetPathRoot(path)!.Length).Contains(':')) throw new ArgumentException("report outside approved root");
        for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("reparse report path");
        return path;
    }
    private static bool HasDuplicateProperties(JsonElement json)
    {
        if (json.ValueKind == JsonValueKind.Object)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in json.EnumerateObject()) if (!seen.Add(property.Name) || HasDuplicateProperties(property.Value)) return true;
        }
        else if (json.ValueKind == JsonValueKind.Array) foreach (var item in json.EnumerateArray()) if (HasDuplicateProperties(item)) return true;
        return false;
    }
    private static string? String(JsonElement json, string name) => json.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static bool Boolean(JsonElement json, string name) => json.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
}
