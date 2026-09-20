using Microsoft.Win32;

namespace AIControlTower.Services;

public sealed record InstallationResult(bool Success, string InstallPath, string Detail);

public sealed class InstallationService
{
    private const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string RunValueName = "AIControlTower";
    private const string SilentLauncherName = "AIControlTower-DesktopCommanderSilent.vbs";

    public Task<InstallationResult> InstallAsync(string sourceExecutable, CancellationToken cancellationToken)
    {
        if (!File.Exists(sourceExecutable)) return Task.FromResult(new InstallationResult(false, string.Empty, "설치할 실행 파일을 찾지 못했습니다."));
        var preferred = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "_My", "AI", "Applications", "AIControlTower");
        var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
        try { return Task.FromResult(InstallTo(sourceExecutable, preferred)); }
        catch (UnauthorizedAccessException) { return Task.FromResult(InstallTo(sourceExecutable, fallback)); }
        catch (System.Security.SecurityException) { return Task.FromResult(InstallTo(sourceExecutable, fallback)); }
    }

    public Task<InstallationResult> UninstallAsync(string installPath, CancellationToken cancellationToken)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            key?.DeleteValue(RunValueName, false);
            var launcher = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), SilentLauncherName);
            if (File.Exists(launcher)) File.Delete(launcher);
            if (Directory.Exists(installPath) && !Path.GetFullPath(Environment.ProcessPath ?? string.Empty).StartsWith(Path.GetFullPath(installPath), StringComparison.OrdinalIgnoreCase)) Directory.Delete(installPath, true);
            return Task.FromResult(new InstallationResult(true, installPath, "자동 시작 등록을 제거했습니다. 설치된 EXE에서 실행 중이었다면 앱 종료 후 폴더를 제거하세요."));
        }
        catch (Exception ex) { return Task.FromResult(new InstallationResult(false, installPath, ProcessRunner.Sanitize(ex.Message))); }
    }

    public Task<InstallationResult> RestoreDesktopCommanderStartupAsync(string installPath, CancellationToken cancellationToken)
    {
        try
        {
            var target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "DesktopCommanderRemote.cmd");
            var backupDirectory = Path.Combine(installPath, "backups", "desktop-commander");
            var backup = Directory.Exists(backupDirectory) ? Directory.GetFiles(backupDirectory, "DesktopCommanderRemote.cmd.*.bak").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault() : null;
            if (backup is null) return Task.FromResult(new InstallationResult(false, installPath, "복구할 Desktop Commander 시작 파일 백업을 찾지 못했습니다."));
            new StartupMigrationService().Restore(backup, target);
            var launcher = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), SilentLauncherName);
            if (File.Exists(launcher)) File.Delete(launcher);
            return Task.FromResult(new InstallationResult(true, installPath, "기존 Desktop Commander 시작 파일을 복구했습니다."));
        }
        catch (Exception ex) { return Task.FromResult(new InstallationResult(false, installPath, ProcessRunner.Sanitize(ex.Message))); }
    }

    private static InstallationResult InstallTo(string sourceExecutable, string destination)
    {
        Directory.CreateDirectory(destination);
        var target = Path.Combine(destination, "AIControlTower.exe");
        File.Copy(sourceExecutable, target, true);
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, true);
        key.SetValue(RunValueName, "\"" + target + "\"");
        var migrationDetail = MigrateDesktopCommander(destination);
        return new InstallationResult(true, destination, "설치 및 현재 사용자 자동 시작 등록을 완료했습니다. " + migrationDetail);
    }

    private static string MigrateDesktopCommander(string installPath)
    {
        var startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var existingLauncher = Path.Combine(startup, "DesktopCommanderRemote.cmd");
        var autoBatch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopCommanderAutoConnect", "DesktopCommanderRemote_Auto.bat");
        if (!File.Exists(existingLauncher) || !File.Exists(autoBatch)) return "Desktop Commander 기존 시작 파일을 찾지 못해 변경하지 않았습니다.";
        var backupDirectory = Path.Combine(installPath, "backups", "desktop-commander");
        new StartupMigrationService().BackupAndDisable(existingLauncher, backupDirectory);
        var silentLauncher = Path.Combine(startup, SilentLauncherName);
        var command = "npx --yes @wonderwhy-er/desktop-commander@latest remote";
        var vbs = "Set shell = CreateObject(\"WScript.Shell\")" + Environment.NewLine + "shell.Run \"cmd.exe /d /c " + command + "\", 0, False";
        File.WriteAllText(silentLauncher, vbs);
        return "기존 표시형 Desktop Commander 시작 파일을 백업하고, 숨김 시작 런처로 교체했습니다.";
    }
}
