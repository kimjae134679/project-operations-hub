using System.Text.Json.Serialization;

namespace AIControlTower.Models;

// Roots and canonical announcement project IDs are supplied by the catalog/UI, never inferred.
public sealed record CommunicationTarget(string ProjectId, string RootPath, string? DisplayName = null);
public sealed record CommunicationNotice(string Id, int Revision, string Title, string Body,
    string ContentSha256, IReadOnlyList<string> Targets, bool Required);
public sealed record CommunicationReceipt
{
    public int SchemaVersion { get; init; } = 1;
    public string NoticeId { get; init; } = "";
    public int Revision { get; init; }
    public string ContentSha256 { get; init; } = "";
    public string ProjectId { get; init; } = "";
    public string ActorId { get; init; } = "";
    public string SessionId { get; init; } = "";
    public string CheckedAt { get; init; } = "";
    public string ApplicationStatus { get; init; } = "";
    public string Note { get; init; } = "";
    public string[] Evidence { get; init; } = [];
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AppliedAt { get; init; }
}
public sealed record CommunicationReceiptItem(CommunicationReceipt Receipt, bool IsCurrent, string Path);
public sealed record CommunicationProjectState(string ProjectId, string RootPath, string MailboxPath,
    int DeliveredCount, int CollectedCount, string State);
public sealed record CommunicationInboxItem(string ProjectId, string ContentSha256, string SourceName,
    string CentralPath, DateTimeOffset CollectedAt)
{
    [JsonIgnore] public string Title { get; init; } = "";
    [JsonIgnore] public string Preview { get; init; } = "";
    [JsonIgnore] public string? Body { get; init; }
    [JsonIgnore] public bool IsThread { get; init; }
    [JsonIgnore] public string ThreadGroup { get; init; } = "";
    [JsonIgnore] public bool IsStandaloneThreadRecord { get; init; }
}
public sealed record CommunicationIssue(string? ProjectId, string Code, string Message)
{
    // Read projection only: scope an unavailable member without displaying or copying its body.
    [JsonIgnore] public string SourceGroup { get; init; } = "";
}
public sealed record CommunicationSnapshot(IReadOnlyList<CommunicationNotice> Notices,
    IReadOnlyList<CommunicationReceiptItem> Receipts, IReadOnlyList<CommunicationProjectState> ProjectStates,
    IReadOnlyList<CommunicationInboxItem> InboxItems, IReadOnlyList<CommunicationIssue> Errors,
    DateTimeOffset LastSync, IReadOnlyList<string> PublishablePaths);
