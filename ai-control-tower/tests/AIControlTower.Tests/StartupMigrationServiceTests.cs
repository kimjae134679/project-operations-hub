using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class StartupMigrationServiceTests
{
    [Fact]
    public void BackupAndDisableCreatesBackupBeforeDisablingSource()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "DesktopCommanderRemote.cmd");
        var backup = Path.Combine(root, "backup");
        File.WriteAllText(source, "echo test");

        var result = new StartupMigrationService().BackupAndDisable(source, backup);

        Assert.True(File.Exists(result.BackupPath));
        Assert.False(File.Exists(source));
        Directory.Delete(root, true);
    }
}
