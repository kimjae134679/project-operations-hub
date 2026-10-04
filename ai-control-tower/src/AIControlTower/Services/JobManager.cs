using System.Collections.Concurrent;
using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed record JobResult(string Id, string ProjectId, string ProgramId, string Command, string State, DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt, int? ExitCode, string LogPath);

/// <summary>Owns only processes started here. A project has one writer/runner at a time.</summary>
public sealed class JobManager
{
    private readonly ConcurrentDictionary<string, (Process Process, CancellationTokenSource Cancellation)> _running = new();
    private readonly ConcurrentDictionary<string, byte> _projects = new();
    private readonly SemaphoreSlim _slots = new(3, 3);
    public event Action<string>? Log;
    public int RunningCount => _running.Count;
    public int BusyProjectCount => _projects.Count;
    public bool IsProjectBusy(string projectId) => _projects.ContainsKey(projectId);
    public async Task<JobResult> RunAsync(ProgramItem program, ProgramCommand command, CancellationToken cancellationToken = default, string? standardInput = null)
    {
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
        var processStarted = false;
        try
        {
            Directory.CreateDirectory(directory);
            Save(directory, result);
            await _slots.WaitAsync(linked.Token);
            acquired = true;
            if (!Directory.Exists(program.WorkingDirectory)) throw new DirectoryNotFoundException("실행 폴더가 없습니다: " + program.WorkingDirectory);
            var executable = ResolveExecutable(command.FileName, program.WorkingDirectory);
            var info = new ProcessStartInfo(executable) { WorkingDirectory = program.WorkingDirectory, UseShellExecute = false,
                CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true };
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
            _running[program.Id] = (process, manualCancellation);
            result = result with { State = "running" };
            Save(directory, result);
            Emit(program.Name + " · 실행 시작");
            using var writer = new StreamWriter(logPath, false, System.Text.Encoding.UTF8);
            var writeLock = new SemaphoreSlim(1, 1);
            async Task Pump(StreamReader reader, string prefix)
            {
                while (await reader.ReadLineAsync() is { } line)
                {
                    var safe = SanitizeOutput(line);
                    if (string.IsNullOrWhiteSpace(safe)) continue;
                    await writeLock.WaitAsync();
                    try { await writer.WriteLineAsync(prefix + safe); await writer.FlushAsync(); }
                    finally { writeLock.Release(); }
                    Emit(prefix + safe);
                }
            }
            var output = Pump(process.StandardOutput, "");
            var error = Pump(process.StandardError, "[stderr] ");
            if (standardInput is not null) await process.StandardInput.WriteAsync(standardInput.AsMemory(), linked.Token);
            process.StandardInput.Close();
            try { await process.WaitForExitAsync(linked.Token); }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(true);
                await process.WaitForExitAsync();
                result = result with { State = cancellationToken.IsCancellationRequested || manualCancellation.IsCancellationRequested ? "cancelled" : "timed_out" };
            }
            await Task.WhenAll(output, error);
            result = result with { ExitCode = process.ExitCode, FinishedAt = DateTimeOffset.UtcNow,
                State = result.State == "running" ? process.ExitCode == 0 ? "succeeded" : "failed" : result.State };
        }
        catch (OperationCanceledException) { result = result with { State = "cancelled", FinishedAt = DateTimeOffset.UtcNow }; }
        catch (Exception ex)
        {
            if (processStarted && process is { HasExited: false }) { process.Kill(true); await process.WaitForExitAsync(); }
            Emit("실행 실패 · " + ProcessRunner.Sanitize(ex.Message));
            result = result with { State = "failed", FinishedAt = DateTimeOffset.UtcNow };
        }
        finally
        {
            _running.TryRemove(program.Id, out _);
            _projects.TryRemove(program.ProjectId, out _);
            if (acquired) _slots.Release();
            process?.Dispose();
        }
        Save(directory, result);
        Emit(program.Name + " · " + result.State + (result.ExitCode is not null ? " · exit " + result.ExitCode : ""));
        return result;
    }
    public bool Stop(string programId)
    {
        if (!_running.TryGetValue(programId, out var job)) return false;
        job.Cancellation.Cancel();
        return true;
    }
    public void StopAllOwned() { foreach (var job in _running.Values) job.Cancellation.Cancel(); }
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
