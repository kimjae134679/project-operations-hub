using System.Collections.Concurrent;
using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed record JobResult(string Id, string ProjectId, string ProgramId, string Command, string State, DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt, int? ExitCode, string LogPath);

/// <summary>Owns only processes started here. A project has one writer/runner at a time.</summary>
public sealed class JobManager
{
    private readonly ConcurrentDictionary<string, (Process Process, CancellationTokenSource Cancellation, WindowsProcessGroup Group)> _running = new();
    private readonly ConcurrentDictionary<string, byte> _projects = new();
    private readonly SemaphoreSlim _slots = new(3, 3);
    public event Action<string>? Log;
    public int RunningCount => _running.Count;
    public int BusyProjectCount => _projects.Count;
    public bool IsProjectBusy(string projectId) => _projects.ContainsKey(projectId);
    public bool IsRunning(string programId) => _running.ContainsKey(programId);
    public async Task<JobResult> RunAsync(ProgramItem program, ProgramCommand command, CancellationToken cancellationToken = default, string? standardInput = null)
    {
        if (command.TimeoutSeconds is < 0 or > 86400) throw new ArgumentOutOfRangeException(nameof(command.TimeoutSeconds), "timeoutSeconds는 0~86400입니다.");
        if (!_projects.TryAdd(program.ProjectId, 0)) throw new InvalidOperationException("이 프로젝트에는 실행 중인 작업이 있습니다.");
        var acquired = false;
        var id = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..8];
        var started = DateTimeOffset.UtcNow;
        var directory = Path.Combine(ControlTowerSettings.DataDirectory, "runs", id);
        var logPath = Path.Combine(directory, "output.log");
        var result = new JobResult(id, program.ProjectId, program.Id, command.Name, "queued", started, null, null, logPath);
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
            if (!Directory.Exists(program.WorkingDirectory)) throw new DirectoryNotFoundException("실행 폴더가 없습니다: " + program.WorkingDirectory);
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
            _running.TryRemove(program.Id, out _);
            _projects.TryRemove(program.ProjectId, out _);
            if (acquired) _slots.Release();
            group?.Dispose();
            process?.Dispose();
        }
        Save(directory, result);
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
    private static void Save(string directory, JobResult result)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "result.json");
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
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
