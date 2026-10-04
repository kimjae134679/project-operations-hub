using AIControlTower.Models;
using AIControlTower.Services;
namespace AIControlTower.Tests;
public sealed class JobManagerTests
{
    private static ProgramItem Program(string id) => new() { Id = id + "/program", ProjectId = id, Name = "검증", WorkingDirectory = Path.GetTempPath() };
    private static ProgramCommand Command(string script, int timeout = 10) => new() { Name = "검증", FileName = "powershell.exe", Arguments = ["-NoProfile", "-Command", "$OutputEncoding=[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false); " + script], TimeoutSeconds = timeout };
    private static void Cleanup(JobResult result)
    { var folder = Path.GetDirectoryName(result.LogPath)!; if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    [Fact]
    public async Task CapturesRealExitCodeAndDurableResult()
    {
        var result = await new JobManager().RunAsync(Program(Guid.NewGuid().ToString()), Command("Write-Output '완료'; exit 7"));
        try { Assert.Equal("failed", result.State); Assert.Equal(7, result.ExitCode); Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(result.LogPath)!, "result.json"))); Assert.Contains("완료", await File.ReadAllTextAsync(result.LogPath)); }
        finally { Cleanup(result); }
    }
    [Fact]
    public async Task RejectsSecondWriterAndStopsOnlyOwnedProcess()
    {
        var manager = new JobManager(); var program = Program(Guid.NewGuid().ToString());
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        manager.Log += line => { if (line.Contains("실행 시작")) started.TrySetResult(); };
        var running = manager.RunAsync(program, Command("Start-Sleep -Seconds 20", 30));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.RunAsync(program, Command("exit 0")));
        Assert.True(manager.Stop(program.Id)); Assert.False(manager.Stop("unowned"));
        var result = await running;
        try { Assert.Equal("cancelled", result.State); Assert.Equal(0, manager.RunningCount); }
        finally { Cleanup(result); }
    }
    [Fact]
    public async Task TimeoutIsDistinctFromCancellation()
    {
        var result = await new JobManager().RunAsync(Program(Guid.NewGuid().ToString()), Command("Start-Sleep -Seconds 20", 1));
        try { Assert.Equal("timed_out", result.State); Assert.NotNull(result.ExitCode); }
        finally { Cleanup(result); }
    }
    [Fact]
    public void LogDropsPromptBodiesAndSecretsButKeepsUsage()
    {
        Assert.Equal("item.completed · command_execution", JobManager.SanitizeOutput("{\"type\":\"item.completed\",\"item\":{\"type\":\"command_execution\",\"command\":\"SECRET PROMPT\"}}"));
        Assert.DoesNotContain("abc", JobManager.SanitizeOutput("Authorization: Bearer abc"));
        Assert.Contains("input_tokens", JobManager.SanitizeOutput("{\"type\":\"turn.completed\",\"usage\":{\"input_tokens\":12}}"));
    }    [Fact]
    public async Task StdinCancellationKillsTheOwnedProcess()
    {
        var manager = new JobManager(); var program = Program(Guid.NewGuid().ToString());
        var pid = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        manager.Log += line => { var match = System.Text.RegularExpressions.Regex.Match(line, @"OWNED:(\d+)"); if (match.Success) pid.TrySetResult(int.Parse(match.Groups[1].Value)); };
        var task = manager.RunAsync(program, Command("Write-Output ('OWNED:'+$PID); Start-Sleep -Seconds 20", 30), standardInput: new string('x', 5_000_000));
        var processId = await pid.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(manager.Stop(program.Id));
        var result = await task.WaitAsync(TimeSpan.FromSeconds(10));
        try { Assert.Equal("cancelled", result.State); Assert.False(IsAlive(processId)); }
        finally { Cleanup(result); }
    }
    [Fact]
    public async Task ParentExitDoesNotLoseChildOwnershipOrTimeout()
    {
        var manager = new JobManager();
        var result = await manager.RunAsync(Program(Guid.NewGuid().ToString()), Command("$p=Start-Process powershell.exe -ArgumentList '-NoProfile','-Command','Start-Sleep -Seconds 20' -NoNewWindow -PassThru; Write-Output ('CHILD:'+$p.Id); exit 0", 2));
        try
        {
            Assert.Equal("timed_out", result.State);
            var text = await File.ReadAllTextAsync(result.LogPath);
            var match = System.Text.RegularExpressions.Regex.Match(text, @"CHILD:(\d+)");
            Assert.True(match.Success); Assert.False(IsAlive(int.Parse(match.Groups[1].Value)));
        }
        finally { Cleanup(result); }
    }
    [Fact]
    public async Task ExcessiveTimeoutDoesNotReserveProject()
    {
        var manager = new JobManager(); var program = Program(Guid.NewGuid().ToString());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => manager.RunAsync(program, Command("exit 0", int.MaxValue)));
        var result = await manager.RunAsync(program, Command("exit 0"));
        try { Assert.Equal("succeeded", result.State); }
        finally { Cleanup(result); }
    }
    [Fact]
    public async Task ShutdownStopsOwnedJobWithoutDependingOnDispatcher()
    {
        var manager = new JobManager(); var program = Program(Guid.NewGuid().ToString());
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        manager.Log += line => { if (line.Contains("실행 시작")) started.TrySetResult(); };
        var task = manager.RunAsync(program, Command("Start-Sleep -Seconds 20", 30));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10)); manager.StopAllOwned();
        var result = await task.WaitAsync(TimeSpan.FromSeconds(10));
        try { Assert.Equal("cancelled", result.State); Assert.Equal(0, manager.RunningCount); }
        finally { Cleanup(result); }
    }
    private static bool IsAlive(int pid)
    {
        try { using var process = System.Diagnostics.Process.GetProcessById(pid); return !process.HasExited; }
        catch (ArgumentException) { return false; }
    }

}
