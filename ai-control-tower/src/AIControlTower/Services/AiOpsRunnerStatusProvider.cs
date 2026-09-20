using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class AiOpsRunnerStatusProvider : CommandStatusProvider
{
    public AiOpsRunnerStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "ai-ops-runner";
    public override async Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        var result = await Runner.RunHiddenAsync("sc.exe", "query \"actions.runner.kimjae134679-PhoneLOL.MultiGod-PC\"", TimeSpan.FromSeconds(6), cancellationToken);
        if (result.TimedOut) return Status(Id, "AI Ops Runner", StatusKind.Unknown, "서비스 상태 확인 시간이 초과되었습니다.");
        if (result.ExitCode != 0) return Status(Id, "AI Ops Runner", StatusKind.Unknown, "예약 작업 또는 실행기 상태를 확인할 수 없습니다.");
        var running = result.StandardOutput.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);
        return Status(Id, "AI Ops Runner", running ? StatusKind.Running : StatusKind.Ready, running ? "Actions Runner 서비스가 실행 중입니다." : "Actions Runner 서비스가 중지되어 있습니다.");
    }
}
