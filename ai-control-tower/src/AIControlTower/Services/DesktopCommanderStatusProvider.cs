using AIControlTower.Models;
namespace AIControlTower.Services;
public sealed class DesktopCommanderStatusProvider : CommandStatusProvider
{
    public DesktopCommanderStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "desktop-commander";
    public override async Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        var state = await new RemoteBridgeService(Runner).CheckAsync(cancellationToken);
        if (!state.ProbeSucceeded) return Status(Id, "Remote Desktop Commander", StatusKind.Unknown, "프로세스 상태 조회 실패. 연결 성공으로 추정하지 않습니다.");
        var detail = $"연결 프로세스 트리 {state.RootCount}개 · 자식 포함 {state.ProcessCount}개 · 시작 등록 {state.StartupCount}개. 온라인/인증은 원격 서비스에서 별도 확인합니다.";
        return Status(Id, "Remote Desktop Commander", state.RootCount > 1 || state.StartupCount > 1 ? StatusKind.Error : state.RootCount == 1 ? StatusKind.Running : StatusKind.Unknown, detail);
    }
}
