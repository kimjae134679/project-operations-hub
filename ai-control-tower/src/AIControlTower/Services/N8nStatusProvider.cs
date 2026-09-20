using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class N8nStatusProvider : CommandStatusProvider
{
    public N8nStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "n8n";
    public override Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        var installed = EnvironmentProbe.FindCommand("n8n") is not null;
        var running = Process.GetProcesses().Any(p => p.ProcessName.Equals("node", StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(Status(Id, "n8n", running ? StatusKind.Running : installed ? StatusKind.Ready : StatusKind.NotInstalled,
            running ? "Node 프로세스가 실행 중입니다. n8n 전용 여부는 확인할 수 없습니다." : installed ? "설치되어 있으나 실행 중인지 확인하지 못했습니다." : "n8n 실행 파일을 PATH에서 찾지 못했습니다."));
    }
}
