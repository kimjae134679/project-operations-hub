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
    }
}
