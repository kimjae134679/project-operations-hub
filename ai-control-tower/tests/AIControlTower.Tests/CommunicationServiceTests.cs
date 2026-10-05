using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class CommunicationServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tower-communication-" + Guid.NewGuid().ToString("N"));
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private string Hub => Path.Combine(_root, "hub");
    private string Project => Path.Combine(_root, "project");
    private string Board => Path.Combine(Hub, "04_COMMUNICATION", "announcements");
    private string Local(string name) => Path.Combine(Project, "_통합소통", name);
    private CommunicationTarget Target => new("PhoneLOL", Project, "멀티의 신");
    private CommunicationService Service() => new(TimeSpan.Zero);

    public CommunicationServiceTests()
    {
        Directory.CreateDirectory(Board); Directory.CreateDirectory(Project);
        WriteManifest();
    }

    private void WriteManifest(string body = "# 공지\n실제로 읽고 본인 확인 기록만 작성합니다.\n", int revision = 1, string target = "*", bool active = true, string? hash = null)
    {
        Directory.CreateDirectory(Path.Combine(Board, "notices"));
        File.WriteAllText(Path.Combine(Board, "notices", "N-0001.md"), body, new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(Board, "manifest.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            projects = new[] { new { id = "PhoneLOL", name = "멀티의 신", category = "등록 프로젝트", required = true }, new { id = "Control-Tower", name = "관제탑", category = "관리", required = true }, new { id = "Optional", name = "참고", category = "경험", required = false } },
            participants = new[] { new { id = "Sol", name = "Sol", mailbox = "" }, new { id = "Astra", name = "Astra", mailbox = "" }, new { id = "account-b-nova", name = "B계정 Nova", mailbox = "" } },
            notices = new[] { new { id = "N-0001", revision, title = "실제 확인", path = "notices/N-0001.md", targets = new[] { target }, required = true, active, contentSha256 = hash ?? CommunicationService.ContentHash(CommunicationService.NormalizeText(Encoding.UTF8.GetBytes(body))) } }
        }, _json));
    }

    private CommunicationReceipt Receipt(string actor = "Sol", string session = "session", string status = "pending", int revision = 1, string? hash = null) => new()
    {
        NoticeId = "N-0001", Revision = revision, ContentSha256 = hash ?? CommunicationService.ContentHash("# 공지\n실제로 읽고 본인 확인 기록만 작성합니다.\n"),
        ProjectId = "PhoneLOL", ActorId = actor, SessionId = session, CheckedAt = "2026-10-05T08:00:00+00:00",
        ApplicationStatus = status, Note = "본인이 실제 읽고 기록함", Evidence = ["AGENTS.md"], AppliedAt = status == "applied" ? "2026-10-05T08:01:00+00:00" : null
    };

    private void SaveLocal(CommunicationReceipt receipt)
    {
        Directory.CreateDirectory(Local("확인기록"));
        File.WriteAllText(Path.Combine(Local("확인기록"), CommunicationService.ReceiptFileName(receipt.ActorId, receipt.SessionId)), JsonSerializer.Serialize(receipt, _json));
    }

    [Fact]
    public async Task DeliveryCreatesMailboxButNeverClaimsReadingAndStatusDoesNotChurn()
    {
        var service = Service();
        var first = await service.Sync(Hub, [Target]);
        Assert.Empty(first.Errors); Assert.Empty(first.Receipts);
        Assert.Single(first.Notices); Assert.Equal(1, Assert.Single(first.ProjectStates).DeliveredCount);
        Assert.True(File.Exists(Path.Combine(Local("받은공지"), "manifest.json")));
        Assert.True(File.Exists(Path.Combine(Local("받은공지"), "N-0001.md")));
        Assert.Empty(Directory.GetFiles(Local("확인기록")));
        var status = Path.Combine(Board, "STATUS.md"); var original = File.ReadAllText(status);
        File.SetLastWriteTimeUtc(status, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var second = await service.Sync(Hub, [Target]);
        Assert.Equal(original, File.ReadAllText(status));
        Assert.Equal(2020, File.GetLastWriteTimeUtc(status).Year);
        Assert.Contains("2", original); Assert.Contains("B계정 Nova", original); Assert.Contains("미확인", original);
        Assert.Contains("04_COMMUNICATION/announcements/STATUS.md", second.PublishablePaths);
    }

    [Fact]
    public async Task EntryHintPreservesExistingRulesAndDoesNotChurnOrAcknowledge()
    {
        var path=Path.Combine(Project,"AGENTS.md");var original="\uFEFF# 기존 지침\r\n이 지침은 그대로 보존합니다.\r\n";
        File.WriteAllText(path,original,new UTF8Encoding(false));
        var service=Service();await service.Sync(Hub,[Target]);
        var updated=File.ReadAllText(path,new UTF8Encoding(false));
        Assert.Contains("이 지침은 그대로 보존합니다.\r\n",updated);
        Assert.Contains("_통합소통/받은공지/manifest.json",updated);
        File.SetLastWriteTimeUtc(path,new DateTime(2020,1,1));
        var second=await service.Sync(Hub,[Target]);
        Assert.Equal(updated,File.ReadAllText(path,new UTF8Encoding(false)));Assert.Equal(2020,File.GetLastWriteTimeUtc(path).Year);Assert.Empty(second.Receipts);
    }
    [Fact]
    public async Task IncompleteEntryHintIsPreservedAndReportedWithoutBlockingNoticeDelivery()
    {
        var path=Path.Combine(Project,"AGENTS.md");const string original="# 작업규칙\n<!-- control-tower:communication-entry:begin -->\n진행 중";
        File.WriteAllText(path,original);
        var result=await Service().Sync(Hub,[Target]);
        Assert.Equal(original,File.ReadAllText(path));Assert.Contains(result.Errors,e=>e.Code=="entry_hint_held");
        Assert.True(File.Exists(Path.Combine(Local("받은공지"),"N-0001.md")));Assert.Empty(result.Receipts);
    }
    [Fact]
    public async Task HelperIsOnlyCopiedAndOnlyRewrittenWhenItChanges()
    {
        var scripts = Path.Combine(Hub, "scripts"); Directory.CreateDirectory(scripts);
        var source = Path.Combine(scripts, "project_notice.py"); File.WriteAllText(source, "raise RuntimeError('must never execute automatically')\n");
        var service = Service(); await service.Sync(Hub, [Target]);
        var helper = Local("기록도우미.py"); Assert.Equal(File.ReadAllText(source), File.ReadAllText(helper));
        File.SetLastWriteTimeUtc(helper, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await service.Sync(Hub, [Target]); Assert.Equal(2020, File.GetLastWriteTimeUtc(helper).Year);
        File.WriteAllText(source, "# version 2\n"); await service.Sync(Hub, [Target]); Assert.Equal("# version 2\n", File.ReadAllText(helper));
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Local("받은공지"), "manifest.json")));
        Assert.Equal("PhoneLOL", manifest.RootElement.GetProperty("deliveredProjectId").GetString());
    }

    [Fact]
    public async Task ConcurrentTicksSerializeAndDuplicateRootsAreRejected()
    {
        var service = Service();
        var results = await Task.WhenAll(service.Sync(Hub, [Target]), service.Sync(Hub, [Target]));
        Assert.All(results, result => Assert.Empty(result.Errors));
        var duplicate = await service.Sync(Hub, [Target, new CommunicationTarget("Control-Tower", Path.Combine(Project, "."))]);
        Assert.Equal(2, duplicate.ProjectStates.Count);
        Assert.All(duplicate.ProjectStates, p => Assert.Equal("일부 보류", p.State));
    }

    [Fact]
    public async Task InvalidBodyHashBlocksAllDeliveryAndCollection()
    {
        WriteManifest(hash: new string('0', 64));
        var result = await Service().Sync(Hub, [Target]);
        Assert.Contains(result.Errors, e => e.Code == "manifest_invalid");
        Assert.False(Directory.Exists(Local("받은공지"))); Assert.Empty(result.PublishablePaths);
    }

    [Fact]
    public async Task BomAndWindowsLineEndingsMatchPublishedContentHash()
    {
        const string normalized = "# 공지\n같은 내용\n";
        WriteManifest(body: "\uFEFF# 공지\r\n같은 내용\r", hash: CommunicationService.ContentHash(normalized));
        var result = await Service().Sync(Hub, [Target]);
        Assert.Empty(result.Errors); Assert.Equal(normalized, Assert.Single(result.Notices).Body);
    }

    [Theory]
    [InlineData("/root/efficiency_research", "20261005-notice-review", "3eef7e03795123e0629b427cd59c393ee8f9da76c19e98a637e10ca4bdb1c2cc.json")]
    [InlineData("한글/AI\u2028", "세션\\\"test", "8922a5fde0e0547082aea933499fb6e9cc26598c271c3efd23a84e3bc6e31ee2.json")]
    public void ReceiptFilenameMatchesPythonIncludingUnicodeAndEscapes(string actor, string session, string expected) => Assert.Equal(expected, CommunicationService.ReceiptFileName(actor, session));

    [Fact]
    public async Task ReceiptsAreSeparatedByActorAndSessionAndPendingIsNotApplied()
    {
        SaveLocal(Receipt()); SaveLocal(Receipt("Astra")); SaveLocal(Receipt(session: "other"));
        var result = await Service().Sync(Hub, [Target]);
        Assert.Empty(result.Errors); Assert.Equal(3, result.Receipts.Count);
        Assert.All(result.Receipts, r => { Assert.True(r.IsCurrent); Assert.Equal("pending", r.Receipt.ApplicationStatus); });
        Assert.Equal(3, Directory.GetFiles(Path.Combine(Board, "receipts"), "*.json", SearchOption.AllDirectories).Length);
    }

    [Fact]
    public async Task AppliedUpdateIsImportedOnlyIfCentralHasNotIndependentlyChanged()
    {
        var service = Service(); SaveLocal(Receipt());
        var first = await service.Sync(Hub, [Target]);
        SaveLocal(Receipt(status: "applied"));
        var second = await service.Sync(Hub, [Target]);
        Assert.Equal("applied", Assert.Single(second.Receipts).Receipt.ApplicationStatus);
        var central = Path.Combine(Hub, Assert.Single(second.Receipts).Path.Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllText(central, JsonSerializer.Serialize(Receipt(status: "blocked") with { Note = "중앙 본인 직접 변경" }, _json));
        SaveLocal(Receipt(status: "not_applicable"));
        var third = await service.Sync(Hub, [Target]);
        Assert.Contains(third.Errors, e => e.Code == "receipt_conflict");
        Assert.Equal("blocked", Assert.Single(third.Receipts).Receipt.ApplicationStatus);
    }

    [Fact]
    public async Task ChangedHashOrTargetDoesNotCountHistoricalReceiptAsCurrent()
    {
        SaveLocal(Receipt()); var service = Service(); await service.Sync(Hub, [Target]);
        WriteManifest(body: "변경된 공지\n");
        var changed = await service.Sync(Hub, [Target]); Assert.False(Assert.Single(changed.Receipts).IsCurrent);
        WriteManifest(target: "Control-Tower");
        var wrongTarget = await service.Sync(Hub, [Target]); Assert.False(Assert.Single(wrongTarget.Receipts).IsCurrent);
        Assert.Equal(0, Assert.Single(wrongTarget.ProjectStates).DeliveredCount);
        Assert.True(File.Exists(Path.Combine(Local("받은공지"), "N-0001.md"))); // Historical body is not deleted.
        using var local = JsonDocument.Parse(File.ReadAllText(Path.Combine(Local("받은공지"), "manifest.json")));
        Assert.Equal(0, local.RootElement.GetProperty("notices").GetArrayLength());
    }

    [Fact]
    public async Task OutboxDeduplicatesNormalizedContentAndPreservesSourcesAndBodyPreview()
    {
        Directory.CreateDirectory(Local("보낼자료"));
        File.WriteAllText(Path.Combine(Local("보낼자료"), "보고.md"), "# 완료\r\n검사 통과\r\n");
        File.WriteAllText(Path.Combine(Local("보낼자료"), "same.txt"), "# 완료\n검사 통과\n");
        var service = Service(); var first = await service.Sync(Hub, [Target]); var second = await service.Sync(Hub, [Target]);
        Assert.Single(first.InboxItems); Assert.Single(second.InboxItems); Assert.Equal("완료", first.InboxItems[0].Title);
        Assert.Equal("# 완료\n검사 통과\n", first.InboxItems[0].Body);
        Assert.True(File.Exists(Path.Combine(Local("보낼자료"), "보고.md")));
        Assert.Equal(0, Assert.Single(second.ProjectStates).CollectedCount);
        Assert.All(second.PublishablePaths, p => Assert.True(p.StartsWith("04_COMMUNICATION/project-inbox/PhoneLOL/") || p == "04_COMMUNICATION/announcements/STATUS.md"));
    }

    [Fact]
    public async Task SecretOversizedAndPartialJsonStayLocalThenValidJsonRetries()
    {
        Directory.CreateDirectory(Local("보낼자료"));
        var token = "sk-" + new string('x', 32);
        File.WriteAllText(Path.Combine(Local("보낼자료"), "secret.txt"), "OPENAI_API_KEY=" + token);
        File.WriteAllText(Path.Combine(Local("보낼자료"), "short-password.json"), "{\"password\": \"abc\"}");
        File.WriteAllText(Path.Combine(Local("보낼자료"), "large.txt"), new string('a', CommunicationService.MaximumFileBytes + 1));
        var partial = Path.Combine(Local("보낼자료"), "partial.json"); File.WriteAllText(partial, "{\"progress\":");
        var service = Service(); var first = await service.Sync(Hub, [Target]);
        Assert.Empty(first.InboxItems); Assert.Contains(first.Errors, e => e.Code == "secret_held");
        Assert.DoesNotContain(first.Errors, e => e.Message.Contains(token));
        File.WriteAllText(partial, "{\"progress\": 100}");
        var second = await service.Sync(Hub, [Target]); Assert.Single(second.InboxItems);
        Assert.True(File.Exists(partial));
    }

    [Fact]
    public async Task RecentlyWrittenTextWaitsUntilSettledInsteadOfSleepingPerFile()
    {
        Directory.CreateDirectory(Local("보낼자료")); var path = Path.Combine(Local("보낼자료"), "active.txt");
        File.WriteAllText(path, "작성중");
        var service = new CommunicationService(TimeSpan.FromMinutes(1));
        Assert.Empty((await service.Sync(Hub, [Target])).InboxItems);
        File.WriteAllText(path, "작성 완료"); File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(-2));
        Assert.Single((await service.Sync(Hub, [Target])).InboxItems);
    }

    [Fact]
    public async Task UnsafeNoticePathAndInvalidLocalReceiptDoNotEscapeOrSpoofOwnership()
    {
        SaveLocal(Receipt() with { ProjectId = "../escape" });
        var result = await Service().Sync(Hub, [Target]); Assert.Empty(result.Receipts); Assert.Contains(result.Errors, e => e.Code == "receipt_retry");
        var manifest = File.ReadAllText(Path.Combine(Board, "manifest.json")).Replace("notices/N-0001.md", "../outside.md");
        File.WriteAllText(Path.Combine(Board, "manifest.json"), manifest);
        Assert.Contains((await Service().Sync(Hub, [Target])).Errors, e => e.Code == "manifest_invalid");
    }

    [Fact]
    public async Task ReparseOutboxCannotCollectOutsideData()
    {
        Directory.CreateDirectory(Local("보낼자료")); var outside = Path.Combine(_root, "outside"); Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "outside.txt"), "범위 밖 원본");
        try { Directory.CreateSymbolicLink(Path.Combine(Local("보낼자료"), "link"), outside); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException) { return; } // Windows hosts may not grant symbolic-link privilege.
        var result = await Service().Sync(Hub, [Target]);
        Assert.Empty(result.InboxItems); Assert.Contains(result.Errors, e => e.Code == "path_blocked");
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
