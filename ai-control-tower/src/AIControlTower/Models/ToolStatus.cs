namespace AIControlTower.Models;

public sealed record ToolStatus(
    string Id,
    string DisplayName,
    StatusKind Kind,
    string Detail,
    DateTimeOffset CheckedAt);
