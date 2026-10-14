using System.Collections.Concurrent;
using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed record JobResult(string Id, string ProjectId, string ProgramId, string Command, string State, DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt, int? ExitCode, string LogPath);

/// <summary>Owns only processes started here. Default project-exclusive; explicitly declared Jev workspaces may run independently.</summary>
public sealed class JobManager
{
    private readonly string _dataDirectory;
    public JobManager() : this(ControlTowerSettings.DataDirectory) { }
    public JobManager(string dataDirectory) => _dataDirectory = Path.GetFullPath(dataDirectory);
    private readonly ConcurrentDictionary<string, (Process Process, CancellationTokenSource Cancellation, WindowsProcessGroup Group)> _running = new(StringComparer.OrdinalIgnoreCase);
    private sealed record Reservation(string ProgramId, string ProjectId, string Workspace, bool Independent);
    private readonly object _admissionGate = new();
    private readonly Dictionary<string, Reservation> _reservations = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _slots = new(3, 3);
    public event Action<string>? Log;
    public int RunningCount => _running.Count;
    public int BusyProjectCount { get { lock (_admissionGate) return _reservations.Values.Select(r => r.ProjectId).Distinct(StringComparer.OrdinalIgnoreCase).Count(); } }
    public bool IsProjectBusy(string projectId) { lock (_admissionGate) return _reservations.Values.Any(r => r.ProjectId.Equals(projectId, StringComparison.OrdinalIgnoreCase)); }
    public bool IsRunning(string programId) => _running.ContainsKey(programId);
    private static Reservation Candidate(ProgramItem program, string? independentWorkspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(program.Id) || string.IsNullOrWhiteSpace(program.ProjectId)) throw new InvalidOperationException("등록 실행 ID가 필요합니다.");
        if (independentWorkspaceRoot is not null && program.Kind != "Jev") throw new InvalidOperationException("독립 작업 폴더는 명시한 Jev 작업만 사용합니다.");
        var workspace = independentWorkspaceRoot is null ? JevExecutionWorkspace.Normalize(program.WorkingDirectory) : JevExecutionWorkspace.Validate(independentWorkspaceRoot, program.WorkingDirectory);
        return new(program.Id, program.ProjectId, workspace, independentWorkspaceRoot is not null);
    }
    private bool Conflicts(Reservation candidate) => _reservations.ContainsKey(candidate.ProgramId)
        || _reservations.Values.Any(r => (r.ProjectId.Equals(candidate.ProjectId, StringComparison.OrdinalIgnoreCase) && (!r.Independent || !candidate.Independent))
            || JevExecutionWorkspace.Overlaps(r.Workspace, candidate.Workspace));
    public bool CanRun(ProgramItem program, string? independentWorkspaceRoot = null)
    {
        try { var candidate = Candidate(program, independentWorkspaceRoot); lock (_admissionGate) return !Conflicts(candidate); }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException) { return false; }
    }
    public async Task<JobResult> RunAsync(ProgramItem program, ProgramCommand command, CancellationToken cancellationToken = default, string? standardInput = null, AiCompletionContract? aiContract = null, string? independentWorkspaceRoot = null)
    {
        if (command.TimeoutSeconds is < 0 or > 86400) throw new ArgumentOutOfRangeException(nameof(command.TimeoutSeconds), "timeoutSeconds는 0~86400입니다.");
        var reservation = Candidate(program, independentWorkspaceRoot);
        lock (_admissionGate)
        {
            if (Conflicts(reservation)) throw new InvalidOperationException("같은 실행 ID·공유 프로젝트·겹치는 작업 폴더에 소유 작업이 있습니다.");
            _reservations.Add(program.Id, reservation); // Queued work owns its reservation until durable completion too.
        }
        var acquired = false;
        var id = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..8];
        var started = DateTimeOffset.UtcNow;
        var directory = Path.Combine(_dataDirectory, "runs", id);
        var logPath = Path.Combine(directory, "output.log");
        var result = new JobResult(id, program.ProjectId, program.Id, command.Name, "queued", started, null, null, logPath);
        AiCompletionEvaluator? ai = null;
        AiCompletionReceipt? aiReceipt = null;
        using var manualCancellation = new CancellationTokenSource();
        using var timeoutCancellation = new CancellationTokenSource();
        if (command.TimeoutSeconds > 0) timeoutCancellation.CancelAfter(TimeSpan.FromSeconds(command.TimeoutSeconds));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, manualCancellation.Token, timeoutCancellation.Token);
        Process? process = null;
        WindowsProcessGroup? group = null;
        var processStarted = false;
        try
        {
            Directory.CreateDirectory(directory);
            Save(directory, result);
            await _slots.WaitAsync(linked.Token).ConfigureAwait(false);
            acquired = true;
            // A queued declaration must still resolve to the same non-link workspace at execution time.
            if (independentWorkspaceRoot is not null) JevExecutionWorkspace.Validate(independentWorkspaceRoot, program.WorkingDirectory);
            if (!Directory.Exists(program.WorkingDirectory)) throw new DirectoryNotFoundException("실행 폴더가 없습니다: " + program.WorkingDirectory);
            if (aiContract is not null) ai = new AiCompletionEvaluator(aiContract, program.WorkingDirectory, id, started);
            if (ai?.PreflightFailure is not null) throw new InvalidOperationException("AI 완료 계약 사전 검사 실패 · " + ai.PreflightFailure);
            var executable = ResolveExecutable(command.FileName, program.WorkingDirectory);
            var info = new ProcessStartInfo(executable) { WorkingDirectory = program.WorkingDirectory, UseShellExecute = false,
                CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8, StandardErrorEncoding = System.Text.Encoding.UTF8 };
            foreach (var argument in command.Arguments) info.ArgumentList.Add(argument);
            if (program.Kind == "Jev")
                foreach (var name in new[] { "JEV_API_KEY", "TYPESAFE_API_KEY" })
                {
                    var key = Environment.GetEnvironmentVariable(name) ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
                    if (!string.IsNullOrWhiteSpace(key)) info.Environment[name] = key;
                }
            process = new Process { StartInfo = info };
            process.Start();
            processStarted = true;
            group = new WindowsProcessGroup(process);
            _running[program.Id] = (process, manualCancellation, group);
            result = result with { State = "running" };
            Save(directory, result);
            Emit(program.Name + " · 실행 시작");
            using var writer = new StreamWriter(logPath, false, System.Text.Encoding.UTF8);
            using var drainCancellation = new CancellationTokenSource();
            var writeLock = new SemaphoreSlim(1, 1);
            async Task Pump(StreamReader reader, string prefix)
            {
                if (ai is not null)
                {
                    // Bounded capture before sanitization; stderr can never supply AI completion.
                    var buffer = new char[2048]; var pending = new System.Text.StringBuilder(); var truncated = false;
                    while (true)
                    {
                        int count;
                        try { count = await reader.ReadAsync(buffer.AsMemory(), drainCancellation.Token).ConfigureAwait(false); }
                        catch (OperationCanceledException) { return; }
                        if (count == 0) break;
                        for (var index = 0; index < count; index++)
                        {
                            var character = buffer[index];
                            if (character == '\n')
                            {
                                if (prefix.Length == 0) ai.ObserveStdout(truncated ? new string('x', AiCompletionEvaluator.MaxLineCharacters + 1) : pending.ToString().TrimEnd('\r'));
                                pending.Clear(); truncated = false;
                            }
                            else if (pending.Length < AiCompletionEvaluator.MaxLineCharacters) pending.Append(character);
                            else truncated = true;
                        }
                    }
                    if (prefix.Length == 0 && (pending.Length > 0 || truncated))
                        ai.ObserveStdout(truncated ? new string('x', AiCompletionEvaluator.MaxLineCharacters + 1) : pending.ToString().TrimEnd('\r'));
                    return; // Never expose declared-AI raw lines/report bodies to log/UI.
                }
                while (true)
                {
                    string? line;
                    try { line = await reader.ReadLineAsync(drainCancellation.Token).ConfigureAwait(false); }
                    catch (OperationCanceledException) { return; }
                    if (line is null) return;
                    var safe = SanitizeOutput(line);
                    if (string.IsNullOrWhiteSpace(safe)) continue;
                    await writeLock.WaitAsync().ConfigureAwait(false);
                    try { await writer.WriteLineAsync(prefix + safe).ConfigureAwait(false); await writer.FlushAsync().ConfigureAwait(false); }
                    finally { writeLock.Release(); }
                    Emit(prefix + safe);
                }
            }
            var output = Pump(process.StandardOutput, "");
            var error = Pump(process.StandardError, "[stderr] ");
            try
            {
                if (standardInput is not null) await process.StandardInput.WriteAsync(standardInput.AsMemory(), linked.Token).ConfigureAwait(false);
                process.StandardInput.Close();
                await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
                await group.WaitForExitAsync(linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                group.Terminate();
                if (!process.HasExited) process.Kill(true);
                await process.WaitForExitAsync().ConfigureAwait(false);
                result = result with { State = cancellationToken.IsCancellationRequested || manualCancellation.IsCancellationRequested ? "cancelled" : "timed_out" };
            }
            catch
            {
                group.Terminate();
                if (!process.HasExited) process.Kill(true);
                await process.WaitForExitAsync().ConfigureAwait(false);
                throw;
            }
            finally
            {
                process.StandardInput.Close();
                drainCancellation.CancelAfter(TimeSpan.FromSeconds(3));
                await Task.WhenAll(output, error).ConfigureAwait(false);
            }
            result = result with { ExitCode = process.ExitCode, FinishedAt = DateTimeOffset.UtcNow,
                State = result.State == "running" ? process.ExitCode == 0 ? "succeeded" : "failed" : result.State };
            if (ai?.OutputLimitExceeded == true && result.State == "succeeded") result = result with { State = "output_limit" };
        }
        catch (OperationCanceledException)
        {
            group?.Terminate();
            if (processStarted && process is { HasExited: false }) { process.Kill(true); await process.WaitForExitAsync().ConfigureAwait(false); }
            result = result with { State = "cancelled", FinishedAt = DateTimeOffset.UtcNow };
        }
        catch (Exception ex)
        {
            group?.Terminate();
            if (processStarted && process is { HasExited: false }) { process.Kill(true); await process.WaitForExitAsync().ConfigureAwait(false); }
            Emit("실행 실패 · " + ProcessRunner.Sanitize(ex.Message));
            result = result with { State = "failed", FinishedAt = DateTimeOffset.UtcNow };
        }
        finally
        {
            try
            {
                // Keep project ownership until semantic completion and its durable receipt are final.
                if (ai is not null)
                {
                    aiReceipt = ai.Complete(result.ExitCode, result.State);
                    if (aiReceipt.Status != "succeeded" && result.State == "succeeded") result = result with { State = "failed" };
                }
                Save(directory, result, aiReceipt);
            }
            finally
            {
                _running.TryRemove(program.Id, out _);
                lock (_admissionGate) _reservations.Remove(program.Id);
                if (acquired) _slots.Release();
                group?.Dispose();
                process?.Dispose();
            }
        }
        Emit(program.Name + " · " + result.State + (result.ExitCode is not null ? " · exit " + result.ExitCode : ""));
        return result;
    }
    public bool Stop(string programId)
    {
        if (!_running.TryGetValue(programId, out var job)) return false;
        try { job.Cancellation.Cancel(); job.Group.Terminate(); }
        catch (ObjectDisposedException) { return false; }
        return true;
    }
    public void StopAllOwned()
    {
        foreach (var job in _running.Values)
        {
            try { job.Cancellation.Cancel(); job.Group.Terminate(); }
            catch (ObjectDisposedException) { continue; }
            // Shutdown must not depend on a dispatcher continuation to terminate a child.
            try { if (!job.Process.HasExited) { job.Process.Kill(true); } }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception) { }
        }
    }
    public static string ResolveExecutable(string name, string workingDirectory)
    {
        var path = name.Contains('/') || name.Contains('\\') ? Path.GetFullPath(Path.Combine(workingDirectory, name)) : EnvironmentProbe.FindCommand(name);
        if (path is null || !File.Exists(path)) throw new FileNotFoundException("실행 파일을 찾지 못했습니다: " + name);
        var extension = Path.GetExtension(path);
        if (extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase) || extension.Equals(".bat", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("셸 스크립트 대신 실행 파일과 arguments 배열을 등록하세요. 예: node.exe + 스크립트 경로");
        return path;
    }
    public static string SanitizeOutput(string value)
    {
        // Codex JSON includes prompts/tool arguments. Keep useful status and usage only.
        if (value.TrimStart().StartsWith('{'))
        {
            try
            {
                using var json = JsonDocument.Parse(value);
                if (json.RootElement.TryGetProperty("type", out var type))
                {
                    var eventType = type.GetString();
                    if (eventType == "turn.completed" && json.RootElement.TryGetProperty("usage", out var usage)) return "usage " + usage.GetRawText();
                    if (eventType == "item.completed" && json.RootElement.TryGetProperty("item", out var item)
                        && item.TryGetProperty("type", out var itemType)) return "item.completed · " + itemType.GetString();
                    return eventType ?? "event";
                }
            }
            catch (JsonException) { }
        }
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"(?i)(api[_-]?key|authorization|bearer\s|secret|password|access[_-]?token|refresh[_-]?token)")) return "[민감 정보 생략]";
        return ProcessRunner.Sanitize(value);
    }
    private void Emit(string message) => Log?.Invoke(DateTime.Now.ToString("HH:mm:ss") + "  " + message);
    private static void Save(string directory, JobResult result, AiCompletionReceipt? aiReceipt = null)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "result.json");
        var json = JsonSerializer.SerializeToNode(result)!.AsObject();
        if (aiReceipt is not null) json["aiReceipt"] = JsonSerializer.SerializeToNode(aiReceipt,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllText(path + ".tmp", json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(path + ".tmp", path, true);
    }
    public static void RecoverInterruptedRuns()
    {
        var root = Path.Combine(ControlTowerSettings.DataDirectory, "runs");
        if (!Directory.Exists(root)) return;
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            try
            {
                var path = Path.Combine(directory, "result.json");
                if (!File.Exists(path)) continue;
                var result = JsonSerializer.Deserialize<JobResult>(File.ReadAllText(path));
                if (result is { State: "running" or "queued" }) Save(directory, result with { State = "interrupted", FinishedAt = DateTimeOffset.UtcNow });
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }
        }
    }
}
