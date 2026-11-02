using System.Collections;
using System.Reflection;
using System.Xml.Linq;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
using Xunit;

namespace AIControlTower.Tests;

public sealed class CommunityNoticeRefinementTests
{
    [Fact]
    public void CurrentProjectsReplaceManifestPopulationAndSessionsCountOnce()
    {
        using var vm = NewVm();
        SetProjects(vm, ("catalog-a", "프로젝트 A", "A"), ("catalog-unmapped", "프로젝트 B", null), ("catalog-other", "프로젝트 C", "C"));
        Apply(vm, Snapshot(Notice(2, "current"), Receipt("A", "one", 2, "current"), Receipt("A", "two", 2, "current", "blocked"), Receipt("Legacy", "old", 2, "current")));
        var rows = Rows(vm, "NoticeProjectRows");
        Assert.Equal(3, rows.Length);
        Assert.Single(rows, r => Get<bool>(r, "IsChecked"));
        Assert.False(Get<bool>(rows[1], "CanCheck"));
        Assert.False(Get<bool>(rows[2], "CanCheck"));
        Assert.DoesNotContain(rows, r => Get<string>(r, "ProjectId") == "Legacy");
    }

    [Theory]
    [InlineData(3, "current")]
    [InlineData(2, "changed")]
    public void NoticeChangeResetsUntilMatchingCurrentVersionReceipt(int revision, string hash)
    {
        using var vm = NewVm();
        SetProjects(vm, ("catalog-a", "프로젝트 A", "A"));
        var old = Receipt("A", "one", 2, "current");
        Apply(vm, Snapshot(Notice(2, "current"), old));
        Assert.True(Get<bool>(Assert.Single(Rows(vm, "NoticeProjectRows")), "IsChecked"));
        // Even a bad caller's stale IsCurrent flag must not transfer a check to changed content.
        Apply(vm, Snapshot(Notice(revision, hash), old));
        Assert.False(Get<bool>(Assert.Single(Rows(vm, "NoticeProjectRows")), "IsChecked"));
        Apply(vm, Snapshot(Notice(revision, hash), old, Receipt("A", "fresh", revision, hash)));
        Assert.True(Get<bool>(Assert.Single(Rows(vm, "NoticeProjectRows")), "IsChecked"));
    }

    [Fact]
    public void InvalidNonTargetAndOtherNoticeReceiptsNeverCheckProject()
    {
        using var vm = NewVm();
        SetProjects(vm, ("catalog-a", "프로젝트 A", "A"), ("catalog-c", "프로젝트 C", "C"));
        Apply(vm, Snapshot(Notice(2, "current"), Receipt("A", "invalid", 2, "current") with { IsCurrent = false },
            Receipt("C", "nontarget", 2, "current"), Receipt("A", "other", 2, "current") with { Receipt = new() { NoticeId = "OTHER", ProjectId = "A", Revision = 2, ContentSha256 = "current" } }));
        Assert.All(Rows(vm, "NoticeProjectRows"), r => Assert.False(Get<bool>(r, "IsChecked")));
    }

    [Fact]
    public void CommunityBodyIncludesSourcePostsAndAllActualCommentsWithoutRecordReclassification()
    {
        using var vm = NewVm();
        var item = new CommunicationInboxItem("A", "hash", "THREAD.md", "isolated/thread.md", DateTimeOffset.UtcNow)
        {
            Title = "글 제목",
            Body = "## 2026-10-07 | AI | 첫 글\n첫 본문\n## 2026-10-07 | AI | 댓글 첫 답\n댓글 하나\n## 2026-10-07 | AI | 둘째 글\n둘째 본문\n## 2026-10-07 | AI | 댓글 둘째 답\n댓글 둘\n",
        };
        Apply(vm, new([], [], [], [item], [], DateTimeOffset.UtcNow, []));
        Assert.Contains("첫 본문", Get<string>(vm, "SelectedCommunityBody"));
        Assert.Contains("둘째 본문", Get<string>(vm, "SelectedCommunityBody"));
        Assert.DoesNotContain("댓글 하나", Get<string>(vm, "SelectedCommunityBody"));
        var comments = Rows(vm, "CommunityComments");
        Assert.Equal(2, comments.Length);
        Assert.All(comments, row => Assert.True(Get<CommunicationEntry>(row, "Entry").IsComment));
        var before = vm.SelectedInbox;
        Apply(vm, new([], [], [], [item], [], DateTimeOffset.UtcNow.AddSeconds(1), []));
        Assert.Same(before, vm.SelectedInbox);
        Assert.Equal(2, Rows(vm, "CommunityComments").Length);
    }

    [Fact]
    public void ReaderXamlHasInlineCommentsAndReadonlyProjectChecksWithoutAdminControls()
    {
        var views = Path.Combine(SourceRoot(), "ai-control-tower", "src", "AIControlTower", "Views");
        var community = XDocument.Load(Path.Combine(views, "CommunicationWorkspaceView.xaml"));
        var notice = XDocument.Load(Path.Combine(views, "NoticeOverviewView.xaml"));
        var forbiddenBindings = new[] { "CommunicationSearch", "InboxProjectFilterId", "CommunicationStateFilter", "OnlyUnread", "CentralSyncMessage", "CommunicationMessage", "ReceiptSearch", "ReceiptFilter", "UnreadLabel", "StateLabel", "PrevEntryCommand", "NextEntryCommand" };
        foreach (var document in new[] { community, notice })
            Assert.DoesNotContain(document.Descendants().Attributes(), a => forbiddenBindings.Any(binding => a.Value.Contains("{Binding " + binding)));
        Assert.DoesNotContain(community.Descendants(), e => (string?)e.Attribute("Name") == "EntryList" || (string?)e.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml")) == "EntryList");
        Assert.Contains(community.Descendants(), e => (string?)e.Attribute("ItemsSource") == "{Binding CommunityComments}");
        Assert.Contains(notice.Descendants(), e => (string?)e.Attribute("ItemsSource") == "{Binding NoticeProjectRows}");
        var check = Assert.Single(notice.Descendants(), e => e.Name.LocalName == "CheckBox");
        Assert.Equal("{Binding IsChecked, Mode=OneWay}", (string?)check.Attribute("IsChecked"));
        Assert.Equal("False", (string?)check.Attribute("IsHitTestVisible"));
        Assert.Equal("False", (string?)check.Attribute("Focusable"));
    }

    [Fact]
    public void ProjectChecksDeduplicateCurrentProjectsAndNeverMutateReceipts()
    {
        var receipts = new[] { Receipt("A", "one", 2, "current"), Receipt("A", "two", 2, "current", "blocked") };
        var before = System.Text.Json.JsonSerializer.Serialize(receipts);
        var projects = new[] { new CommunicationProjectTarget("catalog-a", "A", "A"), new CommunicationProjectTarget("catalog-a", "duplicate folder", "A") };
        var row = Assert.Single(NoticeProjectChecks.Project(Notice(2, "current"), projects, receipts));
        Assert.True(row.IsChecked);
        Assert.True(row.CanCheck);
        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(receipts));
        Assert.All(receipts, receipt => Assert.Null(receipt.Receipt.AppliedAt));
    }

    [Fact]
    public void ProjectPopulationChangeImmediatelyRefreshesChecksWithoutNoticeReload()
    {
        using var vm = NewVm();
        Apply(vm, Snapshot(Notice(2, "current"), Receipt("A", "one", 2, "current")));
        SetProjects(vm, ("catalog-a", "A", "A"));
        Assert.True(Get<bool>(Assert.Single(Rows(vm, "NoticeProjectRows")), "IsChecked"));
        SetProjects(vm, ("catalog-new", "새 프로젝트", null));
        var row = Assert.Single(Rows(vm, "NoticeProjectRows"));
        Assert.Equal("catalog-new", Get<string>(row, "ProjectId"));
        Assert.False(Get<bool>(row, "IsChecked"));
    }

    [Fact]
    public void SourceRecordsAndTaskHistoryNeverBecomeCommentsFromFilenameOrReplyText()
    {
        var entry = new CommunicationEntry("task/r2", "reply 수정 이력", "AI", "시각", "실제 수정 내용", "hash") { RecordKind = "record", SourceName = "reply-r2.json" };
        var article = CommunityPostProjection.Project([entry]);
        Assert.Contains("실제 수정 내용", article.Body);
        Assert.Empty(article.Comments);
        Assert.False(entry.IsComment);
        Assert.Equal("record", entry.RecordKind);
    }

    [Fact]
    public void WildcardNoticeStillUsesOnlyExplicitCurrentProjectPopulation()
    {
        var notice = Notice(2, "current") with { Targets = ["*"] };
        var projects = new[] { new CommunicationProjectTarget("catalog-a", "A", "A"), new CommunicationProjectTarget("catalog-new", "새 프로젝트", null) };
        var rows = NoticeProjectChecks.Project(notice, projects, [Receipt("A", "one", 2, "current"), Receipt("Legacy", "two", 2, "current")]);
        Assert.Equal(2, rows.Count);
        Assert.True(rows[0].IsChecked);
        Assert.False(rows[1].IsChecked);
        Assert.False(rows[1].CanCheck);
    }
    [Fact]
    public void SessionHandoverRemainsTaskRevisionHistoryAndSameRevisionConflictIsNotSplit()
    {
        using var vm = NewVm();
        var first = TaskItem("session-one", 1);
        var second = TaskItem("session-two", 2);
        Apply(vm, new([], [], [], [first, second], [], DateTimeOffset.UtcNow, []));
        var row = Assert.Single(vm.InboxItems);
        Assert.Equal(2, row.RevisionCount);
        Assert.Equal(2, row.Exchange!.Revision);
        Assert.Equal("session-two", row.Exchange.SessionId);
        Assert.Empty(vm.CommunityComments);
        var conflict = TaskItem("session-three", 2);
        Apply(vm, new([], [], [], [first, second, conflict], [], DateTimeOffset.UtcNow.AddSeconds(1), []));
        Assert.Equal(2, vm.InboxItems.Count);
        Assert.All(vm.InboxItems, item => Assert.Equal("상충 기록", item.Warning));
        Assert.Empty(vm.CommunityComments);
    }

    private static CommunicationInboxItem TaskItem(string session, int revision)
    {
        var body = System.Text.Json.JsonSerializer.Serialize(new
        {
            schemaVersion = 1, recordType = "task_exchange", recordId = "same-command", revision,
            title = "같은 지시", projectId = "A", actorId = "AI", sessionId = session,
            receivedAt = "2026-10-07T10:00:00+09:00", updatedAt = "2026-10-07T11:00:00+09:00",
            request = new { summary = "실제 받은 지시", details = "", source = "current_chat" },
            response = new { summary = "진행 답변", details = "", source = "current_chat" },
            status = "in_progress", workDone = Array.Empty<string>(), verification = Array.Empty<object>(),
            nextActions = new[] { "계속 확인" }, blockers = Array.Empty<string>(), supersedes = Array.Empty<string>()
        });
        return new("A", CommunicationService.ContentHash(body), session + "-r" + revision + ".json", "isolated/" + session + "-r" + revision + ".json", DateTimeOffset.Parse("2026-10-07T11:00:00+09:00")) { Title = "같은 지시", Body = body };
    }
    private static CommunicationNotice Notice(int revision, string hash) => new("N", revision, "공지", "공지 본문", hash, ["A"], true);
    private static CommunicationReceiptItem Receipt(string project, string session, int revision, string hash, string status = "pending") => new(new() { NoticeId = "N", ProjectId = project, ActorId = "AI", SessionId = session, Revision = revision, ContentSha256 = hash, ApplicationStatus = status, CheckedAt = "2026-10-07T10:00:00+09:00", Note = "실제 읽음" }, true, session);
    private static CommunicationSnapshot Snapshot(CommunicationNotice notice, params CommunicationReceiptItem[] receipts) => new([notice], receipts, [], [], [], DateTimeOffset.UtcNow, []);
    private static void SetProjects(MainViewModel vm, params (string Id, string Name, string? CommunicationId)[] projects)
    {
        var type = typeof(MainViewModel).Assembly.GetType("AIControlTower.Models.CommunicationProjectTarget");
        Assert.NotNull(type);
        var list = Array.CreateInstance(type!, projects.Length);
        for (var i = 0; i < projects.Length; i++) list.SetValue(Activator.CreateInstance(type!, projects[i].Id, projects[i].Name, projects[i].CommunicationId), i);
        var method = typeof(MainViewModel).GetMethod("SetCurrentCommunicationProjects");
        Assert.NotNull(method);
        method!.Invoke(vm, [list]);
    }
    private static object[] Rows(object owner, string name) => ((IEnumerable)Get<object>(owner, name)).Cast<object>().ToArray();
    private static T Get<T>(object owner, string name)
    {
        var property = owner.GetType().GetProperty(name);
        Assert.NotNull(property);
        return (T)property!.GetValue(owner)!;
    }
    private static MainViewModel NewVm() => new(new ControlTowerSettings { IsTemporary = true, TransientReadOnly = true, AutoCommunication = false, AutoPublishCommunication = false, CommunicationHubPath = Path.Combine(SourceRoot(), "checks", "community-reader-absent"), TransientDataDirectory = Path.Combine(SourceRoot(), "checks", "community-reader-absent") }, false, () => { });
    private static void Apply(MainViewModel vm, CommunicationSnapshot snapshot) => typeof(MainViewModel).GetMethod("ApplyCommunicationSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, [snapshot]);
    private static string SourceRoot()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "ai-control-tower", "AIControlTower.sln"))) root = root.Parent;
        return root?.FullName ?? throw new InvalidOperationException("Source root missing");
    }
}
