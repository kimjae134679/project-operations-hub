using AIControlTower.Services;
namespace AIControlTower.Tests;
public sealed class RemoteBridgeTests
{
    [Fact]
    public void ConsolidationBacksUpAndLeavesOneRegistrationWithoutStartingRemote()
    {
        var root = Path.Combine(Path.GetTempPath(), "remote-startup-test-" + Guid.NewGuid().ToString("N"));
        var startup = Path.Combine(root, "startup"); var data = Path.Combine(root, "data");
        Directory.CreateDirectory(startup);
        File.WriteAllText(Path.Combine(startup, "DesktopCommanderRemote.cmd"), "original");
        File.WriteAllText(Path.Combine(startup, "AIControlTower-DesktopCommanderSilent.vbs"), "old-hidden");
        try
        {
            var service = new RemoteBridgeService(new ProcessRunner()); service.ConsolidateStartup(startup, data);
            Assert.Equal(RemoteBridgeService.LauncherName, Path.GetFileName(Assert.Single(Directory.GetFiles(startup))));
            Assert.Equal("original", File.ReadAllText(Assert.Single(Directory.GetFiles(Path.Combine(data, "backups"), "DesktopCommanderRemote.cmd.bak", SearchOption.AllDirectories))));
            Assert.Contains("System.Threading.Mutex", File.ReadAllText(Path.Combine(data, "RemoteBridge.ps1")));
            Assert.Contains("--no-install", File.ReadAllText(Path.Combine(data, "RemoteBridge.ps1")));
            service.ConsolidateStartup(startup, data); Assert.Single(Directory.GetFiles(startup));
        }
        finally { Directory.Delete(root, true); }
    }    [Fact]
    public void UnicodePathsAndRestoreKeepOneStartupEntry()
    {
        var root = Path.Combine(Path.GetTempPath(), "중계 테스트 " + Guid.NewGuid().ToString("N"));
        var startup = Path.Combine(root, "startup"); var data = Path.Combine(root, "한글 data");
        Directory.CreateDirectory(startup); File.WriteAllText(Path.Combine(startup, "DesktopCommanderRemote.cmd"), "original");
        try
        {
            var service = new RemoteBridgeService(new ProcessRunner()); service.ConsolidateStartup(startup, data);
            Assert.Contains(data, File.ReadAllText(Path.Combine(startup, RemoteBridgeService.LauncherName)));
            service.RestoreStartup(startup, data);
            Assert.Equal("DesktopCommanderRemote.cmd", Path.GetFileName(Assert.Single(Directory.GetFiles(startup))));
            Assert.Equal("original", File.ReadAllText(Path.Combine(startup, "DesktopCommanderRemote.cmd")));
        }
        finally { Directory.Delete(root, true); }
    }

}
