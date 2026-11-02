using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AIControlTower.Services;

public static class OwnedManualExitProtocol
{
    public const string DescriptorPath = @"D:\A_KJ\AI\ControlTowerData\manual-control\manual-exit-owner.json";
    public const int MaximumBytes = 4096;
    public static bool RequestsExit(IEnumerable<string> args) => args.Any(a => a.StartsWith("--request-manual-exit", StringComparison.OrdinalIgnoreCase));
    public static bool AcceptsArguments(IEnumerable<string> args) => args.SequenceEqual(["--request-manual-exit"], StringComparer.Ordinal);
    public static bool IsProductionHost(string executablePath, string baseDirectory)
    {
        try
        {
            const string versions = @"D:\A_KJ\AI\Applications\AIControlTower\versions";
            if (!Path.IsPathFullyQualified(executablePath) || !Path.IsPathFullyQualified(baseDirectory)) return false;
            var executable = Path.GetFullPath(executablePath);
            if (!string.Equals(executable, executablePath, StringComparison.OrdinalIgnoreCase)) return false;
            var relative = Path.GetRelativePath(versions, executable).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (relative.Length != 2 || !System.Text.RegularExpressions.Regex.IsMatch(relative[0], @"\A[A-Za-z0-9][A-Za-z0-9._-]{0,127}\z")
                || !relative[1].Equals("AIControlTower.exe", StringComparison.OrdinalIgnoreCase)) return false;
            return string.Equals(Path.GetDirectoryName(executable), Path.TrimEndingDirectorySeparator(Path.GetFullPath(baseDirectory)), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException) { return false; }
    }
    public static bool IsValidRequest(string json, string nonce, string owner)
    {
        try
        {
            using var document = Parse(json, "action", "nonce", "owner"); var e = document.RootElement;
            return e.GetProperty("action").GetString() == "graceful_exit" && Token(nonce) && Token(owner)
                && e.GetProperty("nonce").GetString() == nonce && e.GetProperty("owner").GetString() == owner;
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or InvalidOperationException) { return false; }
    }
    internal static bool Token(string? value) => value?.Length == 64 && value.All(Uri.IsHexDigit);
    internal static string Nonce() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    internal static JsonDocument Parse(string json, params string[] names)
    {
        if (Encoding.UTF8.GetByteCount(json) > MaximumBytes) throw new InvalidDataException("oversized");
        var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 4 });
        try
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object) throw new InvalidDataException("invalid_object");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
                if (!names.Contains(property.Name, StringComparer.Ordinal) || !keys.Add(property.Name)) throw new InvalidDataException("invalid_fields");
            if (keys.Count != names.Length) throw new InvalidDataException("missing_fields");
            return document;
        }
        catch { document.Dispose(); throw; }
    }
    public static int ExitCode(string status) => status switch
    {
        "graceful_exit_accepted" => 0, "invalid_request" => 2,
        "held_jobs" or "held_timeout" or "held_unknown" or "held_request_pending" or "pending_writes" => 3,
        "unsupported" => 4, "identity_rejected" => 5, _ => 6
    };
    public static void WriteOutcome(string status) => RegisteredRecordPublishing.WriteConsoleOutcome(new { schemaVersion = 1, status, reason = status, exitCode = ExitCode(status) });
}
