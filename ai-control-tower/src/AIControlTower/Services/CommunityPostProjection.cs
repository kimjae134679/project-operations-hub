namespace AIControlTower.Services;

public sealed record CommunityCommentRow(CommunicationEntry Entry, string Body)
{
    public string Title => Entry.Title;
    public string Author => Entry.Author;
    public string TimeDisplay => Entry.TimeDisplay;
    public string AutomationLabel => $"댓글 · {Title} · {Author} · {TimeDisplay}";
}

public sealed record CommunityArticle(string Body, IReadOnlyList<CommunityCommentRow> Comments);

/// <summary>The source topic remains the article; only explicitly classified comments become comment cards.
/// No cross-source parent relationship or task-revision reply is inferred.</summary>
public static class CommunityPostProjection
{
    public static CommunityArticle Project(IReadOnlyList<CommunicationEntry> entries, bool collected = false)
    {
        string Display(string body) => collected ? ManualCommunicationCollection.SafeDisplayBody(body) : body;
        return new(Display(string.Join("\n\n", entries.Where(entry => !entry.IsComment).Select(entry => entry.Body))),
            entries.Where(entry => entry.IsComment).Select(entry => new CommunityCommentRow(entry, Display(entry.Body))).ToArray());
    }
}
