using Microsoft.Win32;

namespace AIControlTower.Services;

public sealed record InstallationResult(bool Success, string InstallPath, string Detail);

public sealed class InstallationService
{
    private const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string RunValueName = "AIControlTower";
    private const string SilentLauncherName = "AIControlTower-DesktopCommanderSilent.vbs";

    public async Task<InstallationResult> InstallAsync(string sourceExecutable, CancellationToken cancellationToken)
    {
        if (!File.Exists(sourceExecutable)) return new(false, string.Empty, "설치할 실행 파일을 찾지 못했습니다.");
        var preferred = Path.Combine(@"D:\A_KJ\AI", "Applications", "AIControlTower");
        var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
        InstallationResult result;
        try { result = InstallTo(sourceExecutable, preferred); }
        catch (UnauthorizedAccessException) { result = InstallTo(sourceExecutable, fallback); }
        catch (System.Security.SecurityException) { result = InstallTo(sourceExecutable, fallback); }
        var startup = await new RemoteBridgeService(new ProcessRunner()).ConfigureNativeStartupAsync(Path.Combine(result.InstallPath,"AIControlTower.exe"), cancellationToken);
        return result with { Detail = result.Detail + " " + startup };
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
            if (!Path.GetFullPath(Environment.ProcessPath ?? string.Empty).Equals(Path.GetFullPath(targetExecutable), StringComparison.OrdinalIgnoreCase) && File.Exists(targetExecutable) && !File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower", "remote-supervisor-status.json")) && !File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), RemoteBridgeService.LauncherName))) File.Delete(targetExecutable);
            // Keep settings, queue, job history, backups and shared remote connection registration.

            return Task.FromResult(new InstallationResult(true, installPath, "관리 화면의 자동 시작을 제거했습니다. 원격 연결·자동 복구와 자료를 위해 실행 파일은 유지합니다."));
        }
        catch (Exception ex) { return Task.FromResult(new InstallationResult(false, installPath, ProcessRunner.Sanitize(ex.Message))); }
    }

    public async Task<InstallationResult> RestoreDesktopCommanderStartupAsync(string installPath, CancellationToken cancellationToken)
    {
        try
        {
            var exe = Path.Combine(installPath, "AIControlTower.exe");
            var bridge = new RemoteBridgeService(new ProcessRunner());
            bridge.ConsolidateStartup(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ControlTowerSettings.DataDirectory, exe);
            var detail = await bridge.ConfigureNativeStartupAsync(exe,cancellationToken);
            RemoteSupervisor.SetEnabled(true); RemoteSupervisor.EnsureSupervisorStarted(exe);
            return new(true, installPath, detail);
        }
        catch (Exception ex) { return new(false, installPath, ProcessRunner.Sanitize(ex.Message)); }
    }

    private static InstallationResult InstallTo(string sourceExecutable, string destination)
    {
        Directory.CreateDirectory(destination);
        var target = Path.Combine(destination, "AIControlTower.exe");
        if (!Path.GetFullPath(sourceExecutable).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase)) File.Copy(sourceExecutable, target, true);
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, true);
        key.SetValue(RunValueName, "\"" + target + "\" --background");
        var migrationDetail = new RemoteBridgeService(new ProcessRunner()).ConsolidateStartup(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ControlTowerSettings.DataDirectory, target);
        RemoteSupervisor.SetEnabled(true);
        return new InstallationResult(true, destination, "설치 및 현재 사용자 자동 시작 등록을 완료했습니다. " + migrationDetail);
    }

}
