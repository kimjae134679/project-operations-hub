namespace AIControlTower.Models;

public sealed record ActivitySnapshot(string ProjectPath, string Task, string Branch, string Stage, DateTimeOffset? StartedAt, TimeSpan Elapsed, bool IsRunning)
{
    public static ActivitySnapshot Empty { get; } = new(string.Empty, string.Empty, string.Empty, "대기", null, TimeSpan.Zero, false);
}
