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
        var preferred = Path.Combine(@"D:\A_KJ\AI", "Applications", "AIControlTower");
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
            var targetExecutable = Path.Combine(installPath, "AIControlTower.exe");
            if (!Path.GetFullPath(Environment.ProcessPath ?? string.Empty).Equals(Path.GetFullPath(targetExecutable), StringComparison.OrdinalIgnoreCase) && File.Exists(targetExecutable)) File.Delete(targetExecutable);
            // Keep settings, queue, job history, backups and shared remote connection registration.

            return Task.FromResult(new InstallationResult(true, installPath, "자동 시작 등록을 제거했습니다. 설치된 EXE에서 실행 중이었다면 앱 종료 후 폴더를 제거하세요."));
        }
        catch (Exception ex) { return Task.FromResult(new InstallationResult(false, installPath, ProcessRunner.Sanitize(ex.Message))); }
    }

    public Task<InstallationResult> RestoreDesktopCommanderStartupAsync(string installPath, CancellationToken cancellationToken)
    {
        try
        {
            var detail = new RemoteBridgeService(new ProcessRunner()).RestoreStartup(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ControlTowerSettings.DataDirectory);
            return Task.FromResult(new InstallationResult(true, installPath, detail));
        }
        catch (Exception ex) { return Task.FromResult(new InstallationResult(false, installPath, ProcessRunner.Sanitize(ex.Message))); }
    }

    private static InstallationResult InstallTo(string sourceExecutable, string destination)
    {
        Directory.CreateDirectory(destination);
        var target = Path.Combine(destination, "AIControlTower.exe");
        if (!Path.GetFullPath(sourceExecutable).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase)) File.Copy(sourceExecutable, target, true);
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, true);
        key.SetValue(RunValueName, "\"" + target + "\"");
        var migrationDetail = new RemoteBridgeService(new ProcessRunner()).ConsolidateStartup(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ControlTowerSettings.DataDirectory);
        return new InstallationResult(true, destination, "설치 및 현재 사용자 자동 시작 등록을 완료했습니다. " + migrationDetail);
    }

}
