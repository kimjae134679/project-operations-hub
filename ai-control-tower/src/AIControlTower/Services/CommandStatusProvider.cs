using AIControlTower.Models;

namespace AIControlTower.Services;

public abstract class CommandStatusProvider : IStatusProvider
{
    protected readonly ProcessRunner Runner;
    protected CommandStatusProvider(ProcessRunner runner) => Runner = runner;
    public abstract string Id { get; }
    public abstract Task<ToolStatus> CheckAsync(CancellationToken cancellationToken);
    public static StatusKind ToStatusKind(int exitCode) => exitCode switch { 0 => StatusKind.Ready, -1 => StatusKind.Unknown, _ => StatusKind.Error };
    protected static ToolStatus Status(string id, string displayName, StatusKind kind, string detail) => new(id, displayName, kind, detail, DateTimeOffset.Now);
}
