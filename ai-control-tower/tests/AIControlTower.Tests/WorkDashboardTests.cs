using AIControlTower.Services;
using AIControlTower.Models;
using AIControlTower.ViewModels;
using System.Text.Json;
namespace AIControlTower.Tests;
public sealed class WorkDashboardTests
{
    [Fact]
    public void ReadOnlyDashboardAndIsolatedFixtureSeamsExist()
    {
        var assembly = typeof(JobManager).Assembly;
        Assert.NotNull(assembly.GetType("AIControlTower.Models.WorkActivity"));
        Assert.NotNull(assembly.GetType("AIControlTower.Services.WorkDashboardService"));
        Assert.NotNull(assembly.GetType("AIControlTower.ViewModels.WorkDashboardViewModel"));
        Assert.NotNull(assembly.GetType("AIControlTower.Services.WorkDashboardVerification"));
    }

    [Fact]
    public async Task DiscoveredLocalRecordsRefreshWithoutCentralSyncAndHighestRevisionWins()
    {
        using var temp = new TestDirectory();
        var project = Path.Combine(temp.Root, "known-project");
        var inbox = Path.Combine(project, "_통합소통", "보낼자료");
        Directory.CreateDirectory(inbox);
        object Record(int revision) => new { schemaVersion = 1, recordType = "task_exchange", recordId = "manager-test", revision, title = "bounded local fixture",
            projectId = "Control-Tower", actorId = "manager-test", sessionId = "fixture", receivedAt = "2026-10-07T01:00:00+09:00", updatedAt = "2026-10-07T02:00:00+09:00",
            request = new { summary = "private request", details = "PRIVATE_PROMPT", source = "fixture" }, response = new { summary = "private response", details = "PRIVATE_MODEL", source = "fixture" },
            status = "in_progress", workDone = new[] { "safe verified fixture" }, verification = new[] { new { name = "fixture", result = "pass", evidence = new[] { "test only" } } },
            nextActions = new[] { "remaining checkpoint" }, blockers = Array.Empty<string>(), supersedes = Array.Empty<string>() };
        File.WriteAllText(Path.Combine(inbox, "older.json"), JsonSerializer.Serialize(Record(2)));
        File.WriteAllText(Path.Combine(inbox, "newer.json"), JsonSerializer.Serialize(Record(10)));
        var service = new WorkDashboardService(Path.Combine(temp.Root, "runs"));
        var register = typeof(WorkDashboardService).GetMethod("RefreshKnownExchangeRoots");
        Assert.NotNull(register);
        register!.Invoke(service, [Path.Combine(temp.Root, "central-missing"), new[] { project }, Array.Empty<string>()]);
        Assert.Contains(inbox, service.ExchangeRoots);
        var records = (await service.ReadAsync(CancellationToken.None)).Where(r => r.IsManagementRecord).ToArray();
        Assert.Single(records); Assert.Equal(10, records[0].Revision);
        Assert.DoesNotContain("PRIVATE", records[0].RecentLog);
        Assert.Null(records[0].OwnedProgramId); Assert.False(records[0].LivenessKnown);
        register.Invoke(service, [Path.Combine(temp.Root, "central-missing"), Array.Empty<string>(), Array.Empty<string>()]);
        Assert.DoesNotContain(inbox, service.ExchangeRoots);
    }

    [Fact]
    public void CheckpointIsNotHeartbeatAndPendingStepsAreOnlyCandidates()
    {
        using var temp = new TestDirectory();
        var state = temp.Write("state.json", new { approved_root = temp.Root, plan_id = "two-step", project = "test", status = "running", updated_at = "2026-10-07T03:00:00+09:00",
            steps = new { first = new { status = "succeeded", attempts = 1, returncode = 0 }, second = new { status = "running", attempts = 1 } } });
        var row = WorkDashboardService.ReadContinuous(state);
        Assert.Equal("running", row.Status);
        Assert.False(row.LivenessKnown);
        Assert.Contains("미확인", row.Evidence);
        Assert.Contains("second", row.Stage);
        Assert.Null(row.OwnedProgramId);
    }

    [Fact]
    public void AcceptedProcessStartNeverMeansChildCompleted()
    {
        var raw = "{\"state\":\"succeeded\",\"action\":\"start_process\",\"result\":{\"status\":\"accepted\",\"processId\":\"p1\"}}";
        var summary = WorkDashboardService.BridgeDetail(raw);
        Assert.Contains("완료 미확인", summary);
        Assert.DoesNotContain("작업 완료", summary);
        Assert.DoesNotContain("secret", WorkDashboardService.BridgeDetail("{\"state\":\"failed\",\"error\":\"Authorization: Bearer secret\",\"args\":{\"prompt\":\"PRIVATE\"}}"));
        Assert.DoesNotContain("PRIVATE", WorkDashboardService.BridgeDetail("{\"state\":\"failed\",\"args\":{\"prompt\":\"PRIVATE\"}}"));
    }

    [Fact]
    public async Task LegacyStartProcessSummaryCannotClaimCompletionBeforeSelectedResult()
    {
        var bridge = new PcConnectionSnapshot(true,true,true,"fixture","connected","fresh","",0,4,
            [new("legacy","old start","test","ProjectBridge","succeeded","start_process"), new("normal","normal read","test","ProjectBridge","succeeded","read_file")]);
        var service=new WorkDashboardService(Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString()),bridge:_=>Task.FromResult(bridge));
        var rows=await service.ReadAsync(CancellationToken.None);
        Assert.Equal("accepted",rows.Single(r=>r.Id=="bridge:legacy").Status);
        Assert.DoesNotContain("완료 기록",rows.Single(r=>r.Id=="bridge:legacy").StatusLabel);
        Assert.Equal("succeeded",rows.Single(r=>r.Id=="bridge:normal").Status);
        Assert.Contains("완료 미확인",WorkDashboardService.BridgeDetail("{\"job\":{\"state\":\"succeeded\",\"action\":\"start_process\"},\"result\":{\"result\":{\"status\":\"accepted\"},\"returnCode\":0}}"));
    }

    [Fact]
    public void CanonicalTerminalProcessOverridesHistoricalAcceptedResult()
    {
        var raw="{\"id\":\"one\",\"state\":\"completed\",\"result\":{\"data\":{\"processId\":\"p\",\"stage\":\"completed\",\"returnCode\":0,\"running\":false,\"done\":true,\"succeeded\":true},\"outcome\":\"completed\"},\"process\":{\"processId\":\"p\",\"stage\":\"completed\",\"returnCode\":0,\"running\":false,\"done\":true,\"succeeded\":true},\"acceptedResult\":{\"data\":{\"stage\":\"starting\",\"running\":true,\"done\":false,\"succeeded\":false},\"outcome\":\"completed\"}}";
        var detail=WorkDashboardService.BridgeDetail(raw);
        Assert.Contains("프로세스 종료 확인",detail);Assert.Contains("종료 코드: 0",detail);
        Assert.DoesNotContain("starting",detail);Assert.Contains("AI 업무 완료 미확인",detail);
    }

    [Fact]
    public void RegisteredPathMustBeContainedAndExternalOwnedClaimDoesNotEnableStop()
    {
        using var temp = new TestDirectory();
        var external = temp.Write("state.json", new { approved_root = Path.Combine(temp.Root, "other"), plan_id = "x", project = "x", status = "running", steps = new { }, updated_at = DateTimeOffset.UtcNow });
        Assert.Throws<InvalidDataException>(() => WorkDashboardService.ReadContinuous(external));
        var job = temp.Write("result.json", new { Id = "one", ProjectId = "project", ProgramId = "unknown/program", Command = "PowerShell", State = "running", StartedAt = DateTimeOffset.UtcNow, LogPath = "" });
        var row = WorkDashboardService.ReadLocalJob(job, _ => false);
        Assert.Null(row.OwnedProgramId);
        Assert.False(row.LivenessKnown);
    }

    [Fact]
    public void DuplicateBackgroundStartupNeverSignalsExistingWindowButManualRepeatDoes()
    {
        var policy = typeof(AIControlTower.App).GetMethod("ShouldActivateExistingInstance");
        Assert.NotNull(policy);
        Assert.False((bool)policy!.Invoke(null, new object[] { new[] { "--background" } })!);
        Assert.False((bool)policy.Invoke(null, new object[] { new[] { "--other", "--background" } })!);
        Assert.True((bool)policy.Invoke(null, new object[] { Array.Empty<string>() })!);
    }

    [Fact]
    public async Task ShutdownCancellationDoesNotEscapePollingAndRevokedOwnershipCannotStop()
    {
        var owned = true; var stops = 0;
        var vm = new WorkDashboardViewModel(_ => Task.FromResult<IReadOnlyList<WorkActivity>>([]), owns: _ => owned, stop: _ => { stops++; return true; });
        vm.Selected = new() { Id = "owned", OwnedProgramId = "known" };
        Assert.True(vm.CanStopOwned); owned = false; vm.StopSelectedOwned(); Assert.Equal(0, stops);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await vm.RefreshAsync(cancellation.Token);
        Assert.False(vm.IsRefreshing);
    }

    [Fact]
    public async Task FilteringSelectionAndRefreshOverlapAreStable()
    {
        var calls = 0; var gate = new TaskCompletionSource<IReadOnlyList<WorkActivity>>();
        var vm = new WorkDashboardViewModel(_ => { calls++; return gate.Task; });
        var first = vm.RefreshAsync();
        await vm.RefreshAsync();
        gate.SetResult([new() { Id = "a", Project = "콘텐츠", Worker = "PowerShell", Title = "무해한 검증" }, new() { Id = "b", Project = "관제탑", Worker = "Jev", Title = "연결 미확인" }]);
        await first;
        Assert.Equal(1, calls);
        vm.Selected = vm.Activities.Single(x => x.Id == "b");
        vm.Search = "Jev";
        Assert.Single(vm.FilteredActivities);
        vm.ApplySnapshot([new() { Id = "b", Project = "관제탑", Worker = "Jev", Title = "동일 작업 최신 상태" }]);
        Assert.Equal("b", vm.Selected?.Id);
    }

    [Fact]
    public async Task LateOldDetailCannotReplaceNewSelectionAndOnlyCurrentOwnedJobStops()
    {
        var old = new TaskCompletionSource<string>();
        var stopped = new List<string>();
        var vm = new WorkDashboardViewModel(_ => Task.FromResult<IReadOnlyList<WorkActivity>>([]),
            row => row.Id == "old" ? old.Task : Task.FromResult("new selection"), id => id == "owned", id => { stopped.Add(id); return true; });
        vm.Selected = new() { Id = "old", OwnedProgramId = "external" };
        var loading = vm.LoadSelectedDetailsAsync();
        Assert.False(vm.CanStopOwned);
        vm.StopSelectedOwned();
        vm.Selected = new() { Id = "new", OwnedProgramId = "owned" };
        await vm.LoadSelectedDetailsAsync();
        old.SetResult("old unsafe late reply");
        await loading;
        Assert.Equal("new selection", vm.SelectedDetails);
        vm.StopSelectedOwned();
        Assert.Equal(new[] { "owned" }, stopped);
    }

    private sealed class TestDirectory : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "dashboard-" + Guid.NewGuid().ToString("N"));
        public TestDirectory() => Directory.CreateDirectory(Root);
        public string Write(string name, object value) { var p = Path.Combine(Root, name); File.WriteAllText(p, JsonSerializer.Serialize(value)); return p; }
        public void Dispose() => Directory.Delete(Root, true);
    }
}
