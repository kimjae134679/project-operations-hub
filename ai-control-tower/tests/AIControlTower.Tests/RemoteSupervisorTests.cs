using AIControlTower.Services;
using System.Diagnostics;

namespace AIControlTower.Tests;

public sealed class RemoteSupervisorTests
{
    private static readonly DateTimeOffset Boot = new(2026, 10, 6, 8, 0, 0, TimeSpan.Zero);
    private const string Entry = @"C:\Users\user\AppData\Local\npm-cache\_npx\cached\node_modules\@wonderwhy-er\desktop-commander\dist\index.js";

    [Fact]
    public void IntentionalStopAppliesOnlyToCurrentSignIn()
    {
        var request = new RemoteSupervisorRequest(false, 2, "ABC", Boot.AddMinutes(10));
        Assert.False(RemoteSupervisor.IsRequestEnabled(request, 2, "ABC", Boot));
        Assert.True(RemoteSupervisor.IsRequestEnabled(request, 3, "ABC", Boot));
        Assert.True(RemoteSupervisor.IsRequestEnabled(request, 2, "DEF", Boot));
    }

    [Fact]
    public void RestartingComputerClearsEarlierStopEvenIfIdsRepeat()
    {
        var request = new RemoteSupervisorRequest(false, 2, "ABC", Boot.AddMinutes(-10));
        Assert.True(RemoteSupervisor.IsRequestEnabled(request, 2, "ABC", Boot));
    }

    [Fact]
    public void BootTimeEstimateToleranceKeepsVeryEarlyIntentionalStop()
    {
        var request = new RemoteSupervisorRequest(false, 2, "ABC", Boot.AddSeconds(-2));
        Assert.False(RemoteSupervisor.IsRequestEnabled(request, 2, "ABC", Boot));
    }

    [Fact]
    public void ExplicitEnableAndFirstLoginAllowRecovery()
    {
        Assert.True(RemoteSupervisor.IsRequestEnabled(null, 2, "ABC", Boot));
        Assert.True(RemoteSupervisor.IsRequestEnabled(new(true, 2, "ABC", Boot.AddMinutes(1)), 2, "ABC", Boot));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(3, 30)]
    [InlineData(50, 30)]
    public void RepeatedLaunchFailuresBackOffAndStayCapped(int errors, int seconds) =>
        Assert.Equal(TimeSpan.FromSeconds(seconds), RemoteSupervisor.RetryDelay(errors));

    [Fact]
    public void CachedLaunchUsesNodeDirectlyWithoutShellOrVisibleWindow()
    {
        var info = RemoteSupervisor.RemoteStartInfo(new(@"C:\Program Files\Node\node.exe", Entry, "0.2.52"));
        Assert.Equal(@"C:\Program Files\Node\node.exe", info.FileName);
        Assert.False(info.UseShellExecute);
        Assert.True(info.CreateNoWindow);
        Assert.Equal(ProcessWindowStyle.Hidden, info.WindowStyle);
        Assert.Equal(new[] { Entry, "remote" }, info.ArgumentList);
        Assert.True(info.RedirectStandardOutput);
        Assert.True(info.RedirectStandardError);
    }

    [Fact]
    public void SupervisorUsesIndependentHiddenWinExeBranch()
    {
        var info = RemoteSupervisor.SupervisorStartInfo(@"D:\AI Tools\AIControlTower.exe");
        Assert.Equal(@"D:\AI Tools\AIControlTower.exe", info.FileName);
        Assert.Equal("--remote-supervisor", Assert.Single(info.ArgumentList));
        Assert.True(info.CreateNoWindow);
        Assert.False(info.UseShellExecute);
        Assert.Equal(ProcessWindowStyle.Hidden, info.WindowStyle);
    }

    [Fact]
    public void ExactInstalledEntryAndRemoteArgumentIdentifyOwnedNode()
    {
        Assert.True(RemoteSupervisor.IsManagedRemoteCommand("node.exe", $"\"C:\\Program Files\\Node\\node.exe\" \"{Entry}\" remote", new[] { Entry }));
        Assert.True(RemoteSupervisor.IsManagedRemoteCommand("NODE.EXE", $"node \"{Entry.ToUpperInvariant()}\" remote", new[] { Entry }));
    }

    [Theory]
    [InlineData("node.exe", "node index.js remote")]
    [InlineData("powershell.exe", "powershell desktop-commander remote")]
    [InlineData("node.exe", "node app.js --title desktop-commander remote")]
    [InlineData("node.exe", "node app.js remote")]
    public void UnrelatedProcessesAreNeverRemoteTargets(string name, string command) =>
        Assert.False(RemoteSupervisor.IsManagedRemoteCommand(name, command, new[] { Entry }));

    [Fact]
    public void StdioAndPrefixArgumentsAreNeverRemoteTargets()
    {
        Assert.False(RemoteSupervisor.IsManagedRemoteCommand("node.exe", $"node \"{Entry}\"", new[] { Entry }));
        Assert.False(RemoteSupervisor.IsManagedRemoteCommand("node.exe", $"node \"{Entry}\" remote-tool", new[] { Entry }));
        Assert.False(RemoteSupervisor.IsManagedRemoteCommand("node.exe", $"node \"{Entry}\" --remote", new[] { Entry }));
        Assert.False(RemoteSupervisor.IsManagedRemoteCommand("node.exe", null, new[] { Entry }));
        Assert.False(RemoteSupervisor.IsManagedRemoteCommand("node.exe", $"node \"{Entry}.bak\" remote", new[] { Entry }));
    }

    [Fact]
    public void StopRetainsValidatedPinnedEntryWhenNodeIsNoLongerOnPath()
    {
        var pinned = new RemoteSupervisorLaunch(@"D:\Node Moved\node.exe", Entry, "0.2.52");
        Assert.Equal(Entry, Assert.Single(RemoteSupervisor.BuildStopEntries(Array.Empty<RemoteSupervisorLaunch>(), pinned)));
        Assert.True(RemoteSupervisor.IsManagedRemoteCommand("node.exe", $"node \"{Entry}\" remote",
            RemoteSupervisor.BuildStopEntries(Array.Empty<RemoteSupervisorLaunch>(), pinned)));
    }

    [Fact]
    public void StopEntryAllowlistDeduplicatesAndRejectsUnvalidatedPin()
    {
        var cached = new[] { new RemoteSupervisorLaunch("node", Entry, "0.2.52"), new RemoteSupervisorLaunch("node", Entry.ToUpperInvariant(), "0.2.52") };
        Assert.Single(RemoteSupervisor.BuildStopEntries(cached, null));
        Assert.Empty(RemoteSupervisor.BuildStopEntries(Array.Empty<RemoteSupervisorLaunch>(), null));
    }
}
