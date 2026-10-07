using AIControlTower.Models;
namespace AIControlTower.Services;
public sealed class ProjectBridgeStatusProvider : IStatusProvider
{
    public string Id => "project-bridge";
    public async Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        using var service = new ProjectBridgeService(); var s = await service.CheckAsync(cancellationToken);
        var kind = !s.Installed ? StatusKind.NotInstalled : s.Connected ? (s.ActiveJobs > 0 ? StatusKind.Running : StatusKind.Ready) : s.Stage is "error" or "offline" or "stale" ? StatusKind.Error : StatusKind.NotConfigured;
        return new(Id,"ProjectBridge",kind,s.Detail,DateTimeOffset.Now);
    }
}
