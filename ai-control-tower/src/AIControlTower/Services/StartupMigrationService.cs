using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class StartupMigrationService
{
    public StartupMigrationResult BackupAndDisable(string sourcePath, string backupDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Startup file was not found.", sourcePath);
        Directory.CreateDirectory(backupDirectory);
        var backupPath = Path.Combine(backupDirectory, Path.GetFileName(sourcePath) + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".bak");
        File.Copy(sourcePath, backupPath, false);
        File.Delete(sourcePath);
        return new StartupMigrationResult(backupPath, sourcePath);
    }

    public void Restore(string backupPath, string targetPath)
    {
        if (!File.Exists(backupPath)) throw new FileNotFoundException("Backup file was not found.", backupPath);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(backupPath, targetPath, true);
    }
}
