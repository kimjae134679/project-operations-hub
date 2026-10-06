using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
using System.Reflection;
using System.Text.Json;

namespace AIControlTower.Tests;

public sealed class CommunicationThreadTests
{
    private const string Thread="# Room\n\n## 2026-10-05 KST — First post\n작성자: Codex · 범위: 설명\n\n### Purpose\nFirst body\n\n## 2026-10-06 KST — Second post\n작성자: Sol\nSecond body\n";
    [Fact]
    public void DatedPostsSplitWithoutSplittingOrdinaryArticleSections()
    {
        var entries=CommunicationThreadParser.Parse(Thread,"room","Room");
        Assert.Equal(2,entries.Count);
        Assert.Equal("First post",entries[0].Title);
        Assert.Equal("Codex",entries[0].Author);
        Assert.Equal("Sol",entries[1].Author);
        Assert.Contains("### Purpose",entries[0].Body);
        Assert.All(entries,e=>Assert.False(e.IsComment));
    }
    [Fact]
    public void CommentsRequireAnExplicitCommentLabel()
    {
        var entries=CommunicationThreadParser.Parse("## 2026-10-05 | Codex | Main\nBody\n## 2026-10-06 | Sol | 댓글 Answer\nReply","room","Room");
        Assert.False(entries[0].IsComment);
        Assert.True(entries[1].IsComment);
    }
    [Fact]
    public void AppendingPostKeepsOriginalEntryIdentityAndHash()
    {
        var before=CommunicationThreadParser.Parse(Thread,"room","Room");
        var after=CommunicationThreadParser.Parse(Thread+"\n## 2026-10-07 | Astra | Third\nNew body","room","Room");
        Assert.Equal(before[0],after[0]);
        Assert.Equal(before[1],after[1]);
    }
    [Fact]
    public void DuplicateDatedTitlesHaveDistinctEntryIdentities()
    {
        var entries=CommunicationThreadParser.Parse("## 2026-10-06 KST — Same\nOne\n## 2026-10-06 KST — Same\nTwo","room","Room");
        Assert.NotEqual(entries[0].Identity,entries[1].Identity);
    }
    [Fact]
    public void NormalHeadingsProduceOnePost()
    {
        var entries=CommunicationThreadParser.Parse("# Article\n## Work\nDone\n## Results\nPass","file","Article");
        Assert.Single(entries);
    }
    [Fact]
    public void DatedCodeExamplesAndDatesInsideSectionTitlesAreNotPosts()
    {
        const string body="# Room\n## 2026-10-05 | Codex | Main\n```markdown\n## 2026-10-06 | Fake | Example\n```\n### Actual verification (2026-10-07)\nPass\n";
        var entry=Assert.Single(CommunicationThreadParser.Parse(body,"room","Room"));
        Assert.Equal("Codex",entry.Author);
        Assert.Contains("Fake",entry.Body);
    }
    [Fact]
    public void GenericJsonIsReadableAndKeepsAuthor()
    {
        const string raw="{\"author\":\"Codex\",\"summary\":\"Readable text\"}";
        var entry=Assert.Single(CommunicationThreadParser.Parse(raw,"file","Article"));
        Assert.Equal("Codex",entry.Author);
        Assert.Contains("Readable text",entry.Body);
        Assert.NotEqual(raw,entry.Body);
        Assert.Equal(CommunicationService.ContentHash(raw),entry.ContentHash);
    }
    [Fact]
    public void ReadStateIsContentSpecificAndHistoryDoesNotUndoCurrentRead()
    {
        var store=new CommunicationReadStore();
        Assert.True(store.IsUnread("post","old"));
        store.MarkRead("post","old");
        Assert.True(store.IsUnread("post","new"));
        store.MarkRead("post","new");
        store.MarkRead("post","old");
        Assert.False(store.IsUnread("post","old"));
        Assert.False(store.IsUnread("post","new"));
        Assert.True(store.IsUnread("other-post","new"));
    }
    [Fact]
    public void ReadStateSurvivesRestartWithoutWritingAnyReceipt()
    {
        var directory=Path.Combine(Path.GetTempPath(),"communication-read-"+Guid.NewGuid().ToString("N"));
        var path=Path.Combine(directory,"user-read.json");
        try
        {
            new CommunicationReadStore(path).MarkRead("post","hash");
            Assert.False(new CommunicationReadStore(path).IsUnread("post","hash"));
            Assert.Single(Directory.GetFiles(directory));
        }
        finally { if(Directory.Exists(directory)) Directory.Delete(directory,true); }
    }
    [Fact]
    public void AutomaticRefreshDoesNotReadPostsAndPreservesEntrySelection()
    {
        using var vm=NewVm();
        var snapshot=Snapshot(Thread);
        Apply(vm,snapshot);
        Assert.True(Assert.Single(vm.InboxItems).IsUnread);
        Assert.Equal(2,vm.Entries.Count);
        vm.NextEntryCommand.Execute(null);
        Assert.Equal("Second post",vm.SelectedEntry!.Title);
        Assert.False(vm.SelectedEntry.IsUnread);
        Assert.True(vm.SelectedInbox!.IsUnread); // First post has not been viewed.
        var selected=vm.SelectedEntry;
        Apply(vm,snapshot with { LastSync=DateTimeOffset.UtcNow });
        Assert.Same(selected,vm.SelectedEntry);
        vm.PrevEntryCommand.Execute(null);
        Assert.False(vm.SelectedInbox.IsUnread);
        vm.OnlyUnread=true;
        Assert.Empty(vm.InboxItems);
    }
    [Fact]
    public void NewThreadEntryLeavesViewedEntriesReadAndNewEntryUnread()
    {
        using var vm=NewVm();
        Apply(vm,Snapshot(Thread));
        vm.MarkCommunicationViewed();
        vm.NextEntryCommand.Execute(null);
        Apply(vm,Snapshot(Thread+"\n## 2026-10-07 | Astra | Third\nNew"));
        Assert.Equal("Second post",vm.SelectedEntry!.Title);
        Assert.False(vm.Entries[0].IsUnread);
        Assert.False(vm.Entries[1].IsUnread);
        Assert.True(vm.Entries[2].IsUnread);
        Assert.True(vm.SelectedInbox!.IsUnread);
    }
    [Fact]
    public void NoticeWaitingRowsAndSearchUseActualCurrentReceipts()
    {
        using var vm=NewVm();
        var n=new CommunicationNotice("N-1",1,"Notice","Body","hash",["A","B"],true);
        var r=new CommunicationReceipt { NoticeId="N-1",ProjectId="A",ActorId="Codex",ApplicationStatus="blocked",Note="Dependency",CheckedAt="2026-10-06T12:00:00+00:00" };
        Apply(vm,new([n],[new(r,true,"receipt")],[],[],[],DateTimeOffset.UtcNow,[]));
        Assert.Equal(1,vm.SelectedNotice!.CheckedCount);
        Assert.Equal(1,vm.SelectedNotice.WaitingCount);
        Assert.Equal(1,vm.SelectedNotice.BlockedCount);
        vm.NoticeReceiptFilter="대기";
        Assert.Equal("B",Assert.Single(vm.SelectedNoticeReceipts).Project);
        vm.NoticeReceiptFilter="막힘";
        Assert.Equal("Codex",Assert.Single(vm.SelectedNoticeReceipts).Actor);
        vm.NoticeReceiptSearch="missing";
        Assert.Empty(vm.SelectedNoticeReceipts);
    }
    [Fact]
    public void TaskRevisionsAreHistoryAndNeverComments()
    {
        using var vm=NewVm();
        var first=TaskItem("task-one",1);
        var second=TaskItem("task-one",2);
        Apply(vm,new([],[],[],[first,second],[],DateTimeOffset.UtcNow,[]));
        var row=Assert.Single(vm.InboxItems);
        Assert.Equal(2,row.Exchange!.Revision);
        Assert.Equal(2,row.RevisionCount);
        Assert.Equal(0,row.CommentCount);
        Assert.Equal(1,row.EntryCount);
        vm.MarkCommunicationViewed();
        vm.ShowCommunicationHistory=true;
        Assert.Equal(2,vm.InboxItems.Count);
        vm.SelectedInbox=vm.InboxItems.First(r=>r.Exchange!.Revision==1);
        vm.ShowCommunicationHistory=false;
        Assert.Equal(2,Assert.Single(vm.InboxItems).Exchange!.Revision);
        Assert.False(vm.SelectedInbox!.IsUnread);
    }
    [Fact]
    public void PostNavigationFollowsVisibleFilteredList()
    {
        using var vm=NewVm();
        Apply(vm,new([],[],[],[TaskItem("task-one",1),TaskItem("task-two",1)],[],DateTimeOffset.UtcNow,[]));
        var first=vm.SelectedInbox;
        Assert.False(vm.PreviousPostCommand.CanExecute(null));
        Assert.True(vm.NextPostCommand.CanExecute(null));
        vm.NextPostCommand.Execute(null);
        Assert.NotSame(first,vm.SelectedInbox);
        Assert.False(vm.NextPostCommand.CanExecute(null));
        vm.PreviousPostCommand.Execute(null);
        Assert.Same(first,vm.SelectedInbox);
    }
    private static CommunicationInboxItem TaskItem(string id,int revision)
    {
        var body=JsonSerializer.Serialize(new
        {
            schemaVersion=1,recordType="task_exchange",recordId=id,revision,title=id,projectId="Test",actorId="Codex",sessionId="session",
            receivedAt="2026-10-06T10:00:00+00:00",updatedAt=$"2026-10-06T1{revision}:00:00+00:00",
            request=new { summary="Request",details="Details",source="user" },response=new { summary="Reply",details="Reply details",source="assistant" },
            status="completed",workDone=new[]{"Done"},verification=Array.Empty<object>(),nextActions=Array.Empty<string>(),blockers=Array.Empty<string>(),supersedes=Array.Empty<string>()
        });
        return new("Test",CommunicationService.ContentHash(body),id+$"-r{revision}.json",id+$"-r{revision}.json",DateTimeOffset.Parse($"2026-10-06T1{revision}:00:00Z")) { Body=body,Title=id };
    }
    private static MainViewModel NewVm() => new(new ControlTowerSettings { IsTemporary=true,CommunicationHubPath=Path.GetTempPath() },enablePolling:false);
    private static CommunicationSnapshot Snapshot(string body)
    {
        var hash=CommunicationService.ContentHash(body);
        return new([],[],[],[new("Test",hash,"THREAD.md","thread.md",DateTimeOffset.Parse("2026-10-06T12:00:00Z")) { Body=body,Title="Room" }],[],DateTimeOffset.UtcNow,[]);
    }
    private static void Apply(MainViewModel vm,CommunicationSnapshot snapshot) => typeof(MainViewModel).GetMethod("ApplyCommunicationSnapshot",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(vm,[snapshot]);
}
