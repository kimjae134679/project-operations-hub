using System.Reflection;
using System.Text;
using System.Text.Json;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class CommunicationViewRetentionTests : IDisposable
{
    private const string ProjectId = "Isolated-Test";
    private const string Body = "# 확인 안내\n본인이 읽은 내용과 적용 결과를 기록합니다.\n";
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tower-view-retention-" + Guid.NewGuid().ToString("N"));
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private string Board => Path.Combine(_root, "04_COMMUNICATION", "announcements");
    private string NoticePath => Path.Combine(Board, "notices", "N-0001.md");

    public CommunicationViewRetentionTests()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(NoticePath)!);
        WriteBoard();
        var receipt = new CommunicationReceipt
        {
            NoticeId = "N-0001", Revision = 1, ContentSha256 = CommunicationService.ContentHash(Body),
            ProjectId = ProjectId, ActorId = "TestReader", SessionId = "isolated-test",
            CheckedAt = "2026-10-05T08:00:00+00:00", ApplicationStatus = "pending",
            Note = "격리된 테스트 자료의 확인 기록", Evidence = []
        };
        var receiptDirectory = Path.Combine(Board, "receipts", "N-0001", "r1", ProjectId);
        Directory.CreateDirectory(receiptDirectory);
        File.WriteAllText(Path.Combine(receiptDirectory, CommunicationService.ReceiptFileName(receipt.ActorId, receipt.SessionId)), JsonSerializer.Serialize(receipt, _json));
        const string content = "# 전달 자료\n프로젝트의 확인 결과입니다.\n";
        var hash = CommunicationService.ContentHash(content);
        var prefix = $"04_COMMUNICATION/project-inbox/{ProjectId}/{hash}/";
        var directory = Path.Combine(_root, prefix.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "content.md"), content, new UTF8Encoding(false));
        var item = new CommunicationInboxItem(ProjectId, hash, "검토자료.md", prefix + "content.md", DateTimeOffset.Parse("2026-10-05T08:02:00+00:00"));
        File.WriteAllText(Path.Combine(directory, "item.json"), JsonSerializer.Serialize(item, _json));
    }

    [Fact]
    public async Task InvalidBoardKeepsLastGoodNoticeReceiptAndInboxThenRecovers()
    {
        using var vm = CreateViewModel();
        await vm.SyncCommunicationAsync(true);
        var notice = Assert.Single(vm.Notices);
        var receipt = Assert.Single(vm.SelectedNoticeReceipts);
        var inbox = Assert.Single(vm.InboxItems);
        var project = Assert.Single(vm.CommunicationProjects);
        var checkedAt = vm.CommunicationCheckedAt;

        // A partial checkout can expose a new body with the previous manifest's hash.
        File.WriteAllText(NoticePath, Body + "새 본문을 쓰는 중입니다.\n", new UTF8Encoding(false));
        await vm.SyncCommunicationAsync(true);
        Assert.Same(notice, Assert.Single(vm.Notices));
        Assert.Same(notice, vm.SelectedNotice);
        Assert.Same(receipt, Assert.Single(vm.SelectedNoticeReceipts));
        Assert.Same(inbox, Assert.Single(vm.InboxItems));
        Assert.Same(inbox, vm.SelectedInbox);
        Assert.Same(project, Assert.Single(vm.CommunicationProjects));
        Assert.Equal(checkedAt, vm.CommunicationCheckedAt);
        Assert.NotEmpty(vm.CommunicationErrors);
        Assert.False(vm.NoNotices);
        Assert.False(vm.IsCommunicating);

        WriteBoard();
        await vm.SyncCommunicationAsync(true);
        Assert.Equal(Body, Assert.Single(vm.Notices).Notice.Body);
        Assert.Single(vm.SelectedNoticeReceipts);
        Assert.Single(vm.InboxItems);
        Assert.DoesNotContain(vm.CommunicationErrors, e => e.Contains("공지 목록 또는 본문 검증 실패"));
    }

    [Fact]
    public async Task ForcedSyncWaitsForExistingSyncInsteadOfReturningBeforeNoticesLoad()
    {
        using var vm = CreateViewModel();
        var field = typeof(MainViewModel).GetField("_communicationLock", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var gate = Assert.IsType<SemaphoreSlim>(field.GetValue(vm));
        await gate.WaitAsync();
        Task sync;
        try
        {
            sync = vm.SyncCommunicationAsync(true);
            Assert.False(sync.IsCompleted);
            Assert.Empty(vm.Notices);
        }
        finally { gate.Release(); }
        await sync;
        Assert.Single(vm.Notices);
        Assert.False(vm.IsCommunicating);
    }

    [Fact]
    public async Task ValidBoardWithNoActiveNoticesClearsPreviouslyVisibleNotice()
    {
        using var vm = CreateViewModel();
        await vm.SyncCommunicationAsync(true);
        Assert.Single(vm.Notices);
        Assert.NotNull(vm.SelectedNotice);
        WriteBoard(active: false);
        await vm.SyncCommunicationAsync(true);
        Assert.Empty(vm.Notices);
        Assert.Null(vm.SelectedNotice);
        Assert.Empty(vm.SelectedNoticeReceipts);
        Assert.True(vm.NoNotices);
        Assert.DoesNotContain(vm.CommunicationErrors, e => e.Contains("공지 목록 또는 본문 검증 실패"));
    }

    private MainViewModel CreateViewModel()
    {
        var vm = new MainViewModel(new ControlTowerSettings
        {
            IsTemporary = true, AutoCommunication = false, AutoPublishCommunication = false,
            CommunicationHubPath = _root, RootPath = Path.Combine(_root, "absent-project-root"),
            MultiplayerServerRoot = Path.Combine(_root, "absent-server-root"), CommunicationFolders = []
        }, enablePolling: false);
        // The board intentionally registers none of the automatic PC catalog targets.
        // CommunicationService rejects those IDs before accessing/creating their mailboxes.
        Assert.DoesNotContain(vm.GetCommunicationTargets(), t => t.ProjectId == ProjectId);
        return vm;
    }

    private void WriteBoard(bool active = true)
    {
        File.WriteAllText(NoticePath, Body, new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(Board, "manifest.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            projects = new[] { new { id = ProjectId, name = "격리 프로젝트", required = true } },
            participants = new[] { new { id = "TestReader", name = "테스트 읽은 사람" } },
            notices = new[] { new { id = "N-0001", revision = 1, title = "확인 안내", path = "notices/N-0001.md", targets = new[] { "*" }, required = true, active, contentSha256 = CommunicationService.ContentHash(Body) } }
        }, _json), new UTF8Encoding(false));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
