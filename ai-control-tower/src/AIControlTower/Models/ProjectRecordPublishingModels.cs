using System.Text.Json.Serialization;

namespace AIControlTower.Models;

// Explicit project/repository registration authorizes metadata only, never private record text.
public sealed record ProjectRecordPublishingRegistration
{
    public string ProjectId { get; init; } = "";
    public string RootPath { get; init; } = "";
    public string OriginUrl { get; init; } = "";
    public long RepositoryId { get; init; }
    public string RepositoryFullName { get; init; } = "";
    public string BaseBranch { get; init; } = "";
    public string SharedPrefix { get; init; } = "";
    public bool Enabled { get; init; }
}
public sealed record ProjectRecordPublishingResult(string Status, string ProjectId, string BatchHash,
    bool Uploaded, string? PullRequestUrl, string? FailureStage)
{
    public bool CentralShared => false;
}
public sealed record ProjectRecordExport(string Path, string Content)
{
    [JsonIgnore] public string SourceHash { get; init; } = "";
}
