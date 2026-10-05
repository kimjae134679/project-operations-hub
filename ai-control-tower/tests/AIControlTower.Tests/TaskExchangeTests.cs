using System.Reflection;
using System.Text.Json;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class TaskExchangeTests
{
    private static string Body(int revision = 1, string status = "in_progress", string title = "소통 개선", string project = "Control-Tower", string response = "실제 답변", string[]? next = null) =>
        JsonSerializer.Serialize(new
        {
            schemaVersion = 1, recordType = "task_exchange", recordId = "reader-work", revision, title,
            projectId = project, actorId = "/root", sessionId = "reader-test",
            receivedAt = "2026-10-06T05:37:58+09:00", updatedAt = "2026-10-06T06:00:00+09:00",
            request = new { summary = "새로고침을 부드럽게", details = "요청 전문 요약", source = "current_chat" },
            response = new { summary = response, details = "", source = "current_chat" },
            status, workDone = new[] { "원인을 확인함" },
            verification = new[] { new { name = "검사", result = "not_run", evidence = Array.Empty<string>() } },
            nextActions = next ?? (status == "completed" ? [] : new[] { "화면 검사" }),
            blockers = status == "blocked" ? new[] { "연결 대기" } : [],
            supersedes = Array.Empty<string>()
        });

    [Fact]
    public void ReaderShowsRequestActualReplyChecksAndRemainingWorkWithoutMutatingSource()
    {
        var body = Body();
        var record = TaskExchangeDocument.Parse(body, "Control-Tower", out var warning);
        Assert.Null(warning);
        Assert.NotNull(record);
        Assert.Equal("진행 중", record.StateLabel);
        foreach (var text in new[] { "받은 요청", "AI가 답한 내용", "실제로 한 일", "확인 결과", "남은 일", "기록 출처", "미실행", "화면 검사", "실제 답변" })
            Assert.Contains(text, record.Markdown);
        Assert.Equal(body, record.SearchText);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"recordType\":\"other\",\"value\":1}")]
    [InlineData("# 기존 전달 문서\n내용")]
    public void LegacyDocumentsRemainReadable(string body)
    {
        Assert.Null(TaskExchangeDocument.Parse(body, "Control-Tower", out var warning));
        Assert.Null(warning);
    }

    [Fact]
    public void WrongProjectAndMissingFieldsAreWarningsNeverCompletion()
    {
        Assert.Null(TaskExchangeDocument.Parse(Body(project: "Other"), "Control-Tower", out var warning));
        Assert.NotNull(warning);
        Assert.Null(TaskExchangeDocument.Parse("{\"recordType\":\"task_exchange\"}", "Control-Tower", out warning));
        Assert.NotNull(warning);
    }

    [Theory]
    [InlineData("completed")]
    [InlineData("pending")]
    [InlineData("in_progress")]
    public void ContradictoryCompletionAndUnexplainedPendingRecordsAreRejected(string state)
    {
        var next = state == "completed" ? new[] { "아직 남은 일" } : Array.Empty<string>();
        Assert.Null(TaskExchangeDocument.Parse(Body(status: state, next: next), "Control-Tower", out var warning));
        Assert.NotNull(warning);
    }

    [Fact]
    public void UnknownFieldsAndBlockedWithoutNextActionStayUnverified()
    {
        var json = System.Text.Json.Nodes.JsonNode.Parse(Body())!.AsObject();
        json["futureMeaning"] = "unknown";
        Assert.Null(TaskExchangeDocument.Parse(json.ToJsonString(), "Control-Tower", out var warning));
        Assert.NotNull(warning);
        Assert.Null(TaskExchangeDocument.Parse(Body(status: "blocked", next: []), "Control-Tower", out warning));
        Assert.NotNull(warning);
        json = System.Text.Json.Nodes.JsonNode.Parse(Body())!.AsObject();
        json["verification"]![0]!["result"] = "fail";
        Assert.Null(TaskExchangeDocument.Parse(json.ToJsonString(), "Control-Tower", out warning));
        Assert.NotNull(warning);
    }

    [Fact]
    public void ConflictClearsWhenOnlyOneValidRevisionRemains()
    {
        using var vm = ViewModel();
        var original = Item(Body(), 1);
        var conflicting = Item(Body(response: "다른 답변"), 2);
        vm.ApplyCommunicationSnapshot(Snapshot(original, conflicting));
        Assert.All(vm.InboxItems, row => Assert.Equal("상충 기록", row.StateLabel));
        vm.ApplyCommunicationSnapshot(Snapshot(original));
        Assert.Equal("진행 중", Assert.Single(vm.InboxItems).StateLabel);
    }

    private static CommunicationInboxItem Item(string body, int number, string project = "Control-Tower", string? title = null) =>
        new(project, "hash" + number, "record.json", "04_COMMUNICATION/item" + number + ".json", DateTimeOffset.Parse("2026-10-06T06:00:00+09:00").AddMinutes(number))
            { Title = title ?? "원본 자료", Preview = title ?? "원본 미리보기", Body = body };

    private static CommunicationSnapshot Snapshot(params CommunicationInboxItem[] items) =>
        new([], [], [], items, [], DateTimeOffset.UtcNow, []);

    private static MainViewModel ViewModel()
    {
        var vm = new MainViewModel(new ControlTowerSettings { IsTemporary = true, AutoCommunication = false }, enablePolling: false);
        typeof(MainViewModel).GetField("_communicationNames", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(vm,
            new Dictionary<string, string> { ["Control-Tower"] = "통합소통", ["Threads"] = "콘텐츠" });
        return vm;
    }

    [Fact]
    public void LatestRevisionSearchStatusAndProjectFilterRestoreOriginalSource()
    {
        using var vm = ViewModel();
        var original = Body();
        var completed = Body(2, "completed", response: "회전 수정 완료");
        vm.ApplyCommunicationSnapshot(Snapshot(Item(original, 1), Item(completed, 2), Item("# 콘텐츠\n이미지 선택", 3, "Threads")));
        Assert.Equal(2, vm.InboxItems.Count);
        vm.ShowCommunicationHistory = true;
        Assert.Equal(3, vm.InboxItems.Count);
        vm.CommunicationStateFilter = "남은 일";
        Assert.Equal(original, Assert.Single(vm.InboxItems).Body);
        vm.ShowCommunicationHistory = false;
        Assert.Empty(vm.InboxItems); Assert.Null(vm.SelectedInbox); Assert.True(vm.NoInboxItems);
        vm.CommunicationStateFilter = "전체 상태";
        vm.CommunicationSearch = "회전 수정 완료";
        Assert.Equal(completed, Assert.Single(vm.InboxItems).Body);
        vm.CommunicationSearch = "";
        vm.SelectedCommunicationProject = vm.CommunicationProjects.Single(p => p.Id == "Threads");
        vm.SelectedProjectOnly = true;
        Assert.Equal("Threads", Assert.Single(vm.InboxItems).ProjectId);
        vm.SelectedProjectOnly = false;
        Assert.Equal(2, vm.InboxItems.Count);
    }

    [Fact]
    public void NewIncomingRecordKeepsSelectionAndRowIdentityEvenWithProjectFilter()
    {
        using var vm = ViewModel();
        var first = Item(Body(), 1);
        var second = Item("# 다른 안내\n내용", 2);
        vm.ApplyCommunicationSnapshot(Snapshot(first, second));
        vm.SelectedCommunicationProject = vm.CommunicationProjects.Single(p => p.Id == "Control-Tower");
        vm.SelectedProjectOnly = true;
        var selected = vm.InboxItems.Single(r => r.Exchange is not null);
        vm.SelectedInbox = selected;
        vm.ApplyCommunicationSnapshot(Snapshot(first, second, Item("# 새 자료\n내용", 3)));
        Assert.Same(selected, vm.SelectedInbox);
        Assert.Same(selected, vm.InboxItems.Single(r => r.Exchange is not null));
        vm.ApplyCommunicationSnapshot(Snapshot(first with { Title = "바뀐 제목" }, second));
        Assert.Same(selected, vm.SelectedInbox); // Structured title comes from the immutable source JSON.
    }

    [Fact]
    public void LegacyMetadataChangesAreReflectedWithoutLosingSelection()
    {
        using var vm = ViewModel();
        var item = Item("기존 문서", 1);
        vm.ApplyCommunicationSnapshot(Snapshot(item));
        var path = vm.SelectedInbox!.Path;
        vm.ApplyCommunicationSnapshot(Snapshot(item with { Title = "갱신된 제목", Preview = "새 미리보기" }));
        Assert.Equal("갱신된 제목", vm.SelectedInbox!.Title);
        Assert.Equal("새 미리보기", vm.SelectedInbox.Preview);
        Assert.Equal(path, vm.SelectedInbox.Path);
    }
}
