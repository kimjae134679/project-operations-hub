using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class DesktopCommanderStatusProvider : CommandStatusProvider
{
    public DesktopCommanderStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "desktop-commander";
    public override Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        var autoRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopCommanderAutoConnect");
        var startup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "DesktopCommanderRemote.cmd");
        var configured = File.Exists(startup) || Directory.Exists(autoRoot);
        var running = Process.GetProcesses().Any(p => p.ProcessName.Contains("desktop", StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(Status(Id, "Desktop Commander", running ? StatusKind.Running : configured ? StatusKind.Ready : StatusKind.NotConfigured,
            running ? "관련 프로세스가 실행 중입니다." : configured ? "시작 구성이 있지만 실행 중인 전용 프로세스를 확인하지 못했습니다." : "자동 연결 구성을 찾지 못했습니다."));
    }
}
