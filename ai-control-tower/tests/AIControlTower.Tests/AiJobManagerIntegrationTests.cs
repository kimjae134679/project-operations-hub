using AIControlTower.Models;
using AIControlTower.Services;
using System.Reflection;
using System.Text.Json;

namespace AIControlTower.Tests;

public sealed class AiJobManagerIntegrationTests
{
    private static readonly string FixtureRoot = @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks";
    private static (JobManager Manager, string Root) Fixture()
    {
        var constructor = typeof(JobManager).GetConstructor([typeof(string)]);
        Assert.NotNull(constructor); // RED before scoped instance data-directory seam exists.
        var root = Path.Combine(FixtureRoot, "ai-owned-job-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return ((JobManager)constructor!.Invoke([root]), root);
    }
    private static ProgramItem Program(string root) => new() { Id = "fixture/task", ProjectId = "fixture", Name = "fixture", WorkingDirectory = root };
    private static ProgramCommand Command(string script) => new() { Name = "fixture", FileName = "powershell.exe",
        Arguments = ["-NoProfile", "-Command", "$OutputEncoding=[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false); " + script], TimeoutSeconds = 10 };
    private static AiCompletionContract Contract(string adapter = "codex-jsonl-v1") => new(adapter, "gpt-6.1-sol");
    private static void Cleanup(string root)
    {
        // Only exact fixture descendants, nonrecursive deletes; never installed app data.
        Assert.StartsWith(Path.GetFullPath(FixtureRoot) + Path.DirectorySeparatorChar, Path.GetFullPath(root));
        foreach (var folder in Directory.EnumerateDirectories(Path.Combine(root, "runs")))
        { foreach (var file in Directory.EnumerateFiles(folder)) File.Delete(file); Directory.Delete(folder); }
        Directory.Delete(Path.Combine(root, "runs")); Directory.Delete(root);
    }
    [Fact]
    public void ParameterlessConstructionRemainsAvailable()
    { Assert.NotNull(typeof(JobManager).GetConstructor(Type.EmptyTypes)); }
    [Fact]
    public async Task ExitZeroFailedWrapperPersistsFailedTypedReceiptAndReleasesOwnedReservation()
    {
        var (manager, root) = Fixture();
        try
        {
            var result = await manager.RunAsync(Program(root), Command("Write-Output '{\"state\":\"failed\",\"exitCode\":1,\"reportPresent\":false,\"model\":\"gpt-6.1-sol\"}'; exit 0"), aiContract: Contract("wrapper-json-v1"));
            Assert.Equal(0, result.ExitCode); Assert.Equal("failed", result.State);
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(result.LogPath)!, "result.json")));
            var receipt = json.RootElement.GetProperty("aiReceipt");
            Assert.Equal(result.Id, receipt.GetProperty("executionId").GetString());
            Assert.Equal("failed", receipt.GetProperty("status").GetString());
            Assert.False(manager.IsProjectBusy("fixture")); Assert.Equal(0, manager.RunningCount);
        }
        finally { Cleanup(root); }
    }
    [Fact]
    public async Task GenericExitZeroJsonFailureRemainsOpaque()
    {
        var (manager, root) = Fixture();
        try
        {
            var result = await manager.RunAsync(Program(root), Command("Write-Output '{\"state\":\"failed\",\"exitCode\":1}'; exit 0"));
            Assert.Equal("succeeded", result.State);
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(result.LogPath)!, "result.json")));
            Assert.False(json.RootElement.TryGetProperty("aiReceipt", out _));
        }
        finally { Cleanup(root); }
    }
    [Fact]
    public async Task StderrCannotSpoofAiCompletionAndNoPrivateBodyReachesLogs()
    {
        var (manager, root) = Fixture(); var messages = new List<string>(); manager.Log += messages.Add;
        try
        {
            var result = await manager.RunAsync(Program(root), Command("[Console]::Error.WriteLine('{\"type\":\"item.completed\",\"item\":{\"type\":\"agent_message\",\"text\":\"SECRET_REPORT\"}}'); [Console]::Error.WriteLine('{\"type\":\"turn.completed\"}'); exit 0"), aiContract: Contract());
            Assert.Equal("failed", result.State);
            Assert.DoesNotContain("SECRET_REPORT", File.ReadAllText(result.LogPath));
            Assert.DoesNotContain(messages, line => line.Contains("SECRET_REPORT"));
        }
        finally { Cleanup(root); }
    }
    [Fact]
    public async Task ValidStdoutCompletesButNeitherOutputLogNorEventLeaksReport()
    {
        var (manager, root) = Fixture(); var messages = new List<string>(); manager.Log += messages.Add;
        try
        {
            var result = await manager.RunAsync(Program(root), Command("Write-Output '{\"type\":\"item.completed\",\"item\":{\"type\":\"agent_message\",\"text\":\"SECRET_REPORT\"}}'; Write-Output '{\"type\":\"turn.completed\"}'; exit 0"), aiContract: Contract());
            Assert.Equal("succeeded", result.State);
            Assert.DoesNotContain("SECRET_REPORT", File.ReadAllText(result.LogPath));
            Assert.DoesNotContain(messages, line => line.Contains("SECRET_REPORT"));
        }
        finally { Cleanup(root); }
    }
}
