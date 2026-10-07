using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Display-only aggregation. No receipt is created, changed or acknowledged by this projection.</summary>
public static class NoticeProjectChecks
{
    public static IReadOnlyList<NoticeProjectCheck> Project(CommunicationNotice? notice,
        IReadOnlyList<CommunicationProjectTarget> projects, IReadOnlyList<CommunicationReceiptItem> receipts)
    {
        return projects.DistinctBy(p => p.ProjectId, StringComparer.Ordinal).Select(project =>
        {
            var mapped = !string.IsNullOrWhiteSpace(project.CommunicationId);
            var applicable = mapped && notice is not null && (notice.Targets.Contains("*") || notice.Targets.Contains(project.CommunicationId!));
            var read = applicable && receipts.Any(item => item.IsCurrent && item.Receipt.NoticeId == notice!.Id
                && item.Receipt.Revision == notice.Revision && item.Receipt.ContentSha256 == notice.ContentSha256
                && item.Receipt.ProjectId == project.CommunicationId);
            return new NoticeProjectCheck(project.ProjectId, project.Name, project.CommunicationId, applicable, read);
        }).ToArray();
    }
}
