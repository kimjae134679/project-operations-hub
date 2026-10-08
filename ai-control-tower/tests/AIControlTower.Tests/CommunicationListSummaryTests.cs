using System.Reflection;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class CommunicationListSummaryTests
{
    [Fact]
    public void ListShowsExplicitCommentsBeforeSelectionWithoutReadingThem()
    {
        using var vm = NewVm();
        Apply(vm, Snapshot("## 2026-10-08 | User | 요청 작업\n본문\n## 2026-10-08 | AI | 댓글 답변\n답변"));
        var row = Assert.Single(vm.InboxItems);
        Assert.Equal("글 1 · 댓글 1", Value<string>(row, "CountLabel"));
        Assert.Equal("↩ 답변 있는 글", Value<string>(row, "ReplyStateLabel"));
        Assert.Equal("새 댓글 1", Value<string>(row, "NewCommentLabel"));
        Assert.True(vm.Entries.All(e => e.IsUnread));
        Assert.Single(vm.CommunityComments);
    }

    [Fact]
    public void ZeroCommentsAndUnansweredRequestAreDifferentFromMissingData()
    {
        using var vm = NewVm();
        Apply(vm, Snapshot("## 2026-10-08 | User | 요청 작업\n본문"));
        Assert.Equal("글 1 · 댓글 0", Value<string>(vm.InboxItems[0], "CountLabel"));
        Assert.Equal("◷ 답변 없는 요청", Value<string>(vm.InboxItems[0], "ReplyStateLabel"));
        Apply(vm, Snapshot(null));
        Assert.Equal("글·댓글 수 미확인", Value<string>(vm.InboxItems[0], "CountLabel"));
        Assert.Equal("? 내용 미확인", Value<string>(vm.InboxItems[0], "ReplyStateLabel"));
        Assert.Empty(vm.CommunityComments);
        Assert.DoesNotContain("댓글 0", vm.CommunityCommentCountLabel);
    }

    [Fact]
    public void BodyAndKnownReadHistorySurviveNewCommentRefresh()
    {
        using var vm = NewVm();
        const string original = "## 2026-10-08 | User | 요청 작업\n본문\n## 2026-10-08 | AI | 댓글 첫 답변\n답변";
        Apply(vm, Snapshot(original));
        vm.MarkCommunicationViewed();
        var mark = typeof(MainViewModel).GetMethod("MarkCommunityCommentViewed");
        Assert.NotNull(mark);
        mark.Invoke(vm,[Assert.Single(vm.CommunityComments)]); // The real inline-comment action, isolated in memory.
        Assert.Equal("새 댓글 없음", Value<string>(vm.InboxItems[0], "NewCommentLabel"));
        var first = vm.Entries[0].Entry;
        Apply(vm, Snapshot(original + "\n## 2026-10-09 | AI | 댓글 새 답변\n추가 답변"));
        Assert.Equal(first, vm.Entries[0].Entry);
        Assert.False(vm.Entries[1].IsUnread);
        Assert.Equal("새 댓글 1", Value<string>(vm.InboxItems[0], "NewCommentLabel"));
        Assert.Equal(2, vm.CommunityComments.Count);
    }

    [Fact]
    public void UnlabelledLaterPostsAreNotInventedAsReplies()
    {
        using var vm = NewVm();
        Apply(vm, Snapshot("## 2026-10-08 | User | 첫 글\n본문\n## 2026-10-09 | AI | 별도 글\n본문"));
        Assert.Equal("글 2 · 댓글 0", Value<string>(vm.InboxItems[0], "CountLabel"));
        Assert.Equal("? 답변 관계 미확인", Value<string>(vm.InboxItems[0], "ReplyStateLabel"));
    }

    [Fact]
    public void SummaryMarksFailedReadsInsteadOfClaimingZeroOrEmptyRoom()
    {
        using var vm = NewVm();
        Apply(vm, new([], [], [], [], [new(null,"thread_retry","읽기 실패")], DateTimeOffset.UtcNow, []));
        Assert.Contains("읽기 실패", Value<string>(vm, "CommunityListSummary"));
        Assert.DoesNotContain("댓글 0", Value<string>(vm, "CommunityListSummary"));
        Assert.Contains("읽기 실패", vm.InboxEmptyLabel);
    }

    [Fact]
    public void CompletedTaskResponseIsNotManufacturedAsAComment()
    {
        var row = new InboxRow("P","작업","","body","path","time")
        {
            Exchange = new("r","P","AI",3,"작업","요청","실제 답변","completed",DateTimeOffset.UtcNow,"","") { ResponseSource="current_chat" },
            ThreadEntries = [new("post","작업","AI","time","본문","hash")], RevisionCount=3
        };
        Assert.Equal("글 1 · 댓글 0", Value<string>(row,"CountLabel"));
        Assert.Equal("✓ 완료 기록", Value<string>(row,"ReplyStateLabel"));
        Assert.Contains("수정 이력 3", row.ConversationSummary);
    }

    [Fact]
    public void UnknownRowsNeverProduceZeroTotalsAndReadFailureHasItsOwnLabel()
    {
        using var vm=NewVm();
        Apply(vm,Snapshot(null));
        Assert.Contains("글·댓글 수 미확인",Value<string>(vm,"CommunityListSummary"));
        Assert.DoesNotContain("댓글 0",Value<string>(vm,"CommunityListSummary"));
        Apply(vm,Snapshot(null) with {Errors=[new(null,"thread_retry","읽기 실패")]});
        Assert.Equal("글·댓글 수 읽기 실패",Value<string>(vm.InboxItems[0],"CountLabel"));
    }

    [Theory]
    [InlineData("work_checkpoint","최종 답변 대기","◷ 답변 없는 요청")]
    [InlineData("unknown","미확인","◷ 답변 없는 요청")]
    [InlineData("current_chat","실제로 전달한 진행 답변","↩ 답변 기록 있음")]
    public void ResponsePlaceholderDoesNotCountAsDeliveredReply(string source,string summary,string expected)
    {
        var row=new InboxRow("P","작업","","body","path","time") {Exchange=new("r","P","AI",1,"작업","요청",summary,"in_progress",DateTimeOffset.UtcNow,"","") {ResponseSource=source},ThreadEntries=[new("post","작업","AI","time","본문","hash")]};
        Assert.Equal(expected,Value<string>(row,"ReplyStateLabel"));
        Assert.Equal(0,row.CommentCount);
    }

    [Fact]
    public void CommentReadActionDoesNotReadOtherCommentsOrStaleContent()
    {
        using var vm=NewVm();
        Apply(vm,Snapshot("## 2026-10-08 | User | 요청 작업\n본문\n## 2026-10-08 | AI | 댓글 첫 답변\n답변\n## 2026-10-09 | AI | 댓글 둘째 답변\n추가"));
        var stale=vm.CommunityComments[0];
        var mark=typeof(MainViewModel).GetMethod("MarkCommunityCommentViewed");
        Assert.NotNull(mark);
        mark.Invoke(vm,[stale]);
        Assert.Equal("새 댓글 1",Value<string>(vm.InboxItems[0],"NewCommentLabel"));
        Assert.False(Value<bool>(vm.CommunityComments[0],"IsUnread"));
        Assert.True(Value<bool>(vm.CommunityComments[1],"IsUnread"));
        Apply(vm,Snapshot("## 2026-10-08 | User | 요청 작업\n본문\n## 2026-10-08 | AI | 댓글 첫 답변\n변경된 본문"));
        mark.Invoke(vm,[stale]);
        Assert.Equal("새 댓글 1",Value<string>(vm.InboxItems[0],"NewCommentLabel"));
    }

    [Fact]
    public void ReadStateUpdatesTheAccessibleListLabelEvenWithoutComments()
    {
        var row=new InboxRow("P","제목","","body","path","time") {IsUnread=true};
        var changes=new List<string?>();
        row.PropertyChanged+=(_,e)=>changes.Add(e.PropertyName);
        row.IsUnread=false;
        Assert.Contains("AutomationLabel",changes);
    }

    private static T Value<T>(object target, string name)
    {
        var property = target.GetType().GetProperty(name);
        Assert.NotNull(property);
        return (T)property.GetValue(target)!;
    }
    private static MainViewModel NewVm() => new(new ControlTowerSettings {IsTemporary=true,TransientReadOnly=true,CommunicationHubPath=Path.GetTempPath()},enablePolling:false);
    private static CommunicationSnapshot Snapshot(string? body) => new([],[],[],[new("P","hash","THREAD.md","thread",DateTimeOffset.UtcNow) {Title="주제",Body=body,IsThread=true}],[],DateTimeOffset.UtcNow,[]);
    private static void Apply(MainViewModel vm, CommunicationSnapshot snapshot) => typeof(MainViewModel).GetMethod("ApplyCommunicationSnapshot",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(vm,[snapshot]);
}
