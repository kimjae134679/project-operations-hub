namespace AIControlTower.Services;

public interface IStatusProvider
{
    string Id { get; }
    Task<AIControlTower.Models.ToolStatus> CheckAsync(CancellationToken cancellationToken);
}
