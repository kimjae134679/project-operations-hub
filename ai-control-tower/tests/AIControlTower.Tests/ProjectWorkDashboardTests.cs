using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

/// <summary>Pure records/reflection/source fixtures; no Window.Show, process, model or global settings.</summary>
public sealed class ProjectWorkDashboardTests
{
    [Fact]
    public async Task CatalogUsesExactProjectIdentityAndPreservesProgramWithoutStopAuthority()
    {
        using var fixture = new DFixture();
        fixture.Job("first", "p-one", "p-one/powershell", "검증");
        fixture.Job("second", "p-two", "p-two/codex", "검증");
        var service = new WorkDashboardService(fixture.Runs);
        RegisterCatalog(service, [Project("p-one", "같은 이름", "p-one/powershell", "program", "pwsh.exe"), Project("p-two", "같은 이름", "p-two/codex", "Codex", "codex.exe")]);
        var jobs = (await service.ReadAsync(default)).Where(x => x.Id.StartsWith("local:")).ToArray();
        Assert.Equal(2, jobs.Length);
        Assert.Equal(new[] { "p-one", "p-two" }, jobs.Select(x => Value(x, "ProjectId")).OrderBy(x => x).ToArray());
        Assert.All(jobs, x => Assert.Equal("같은 이름", Value(x, "ProjectDisplayName")));
        Assert.Equal("PowerShell", Value(jobs.Single(x => x.Id == "local:first"), "WorkerKind"));
        Assert.Equal("Codex", Value(jobs.Single(x => x.Id == "local:second"), "WorkerKind"));
        Assert.Equal("p-one/powershell", Value(jobs.Single(x => x.Id == "local:first"), "ProgramId"));
        Assert.All(jobs, x => { Assert.Null(x.OwnedProgramId); Assert.False(x.LivenessKnown); });
    }

    [Fact]
    public async Task UnregisteredExternalRowsStayUnassignedAndNeverGainStopAuthority()
    {
        using var fixture = new DFixture(); var service = new WorkDashboardService(fixture.Runs);
        RegisterCatalog(service, [Project("audiobook", "무직전생오디오북", "audiobook/p", "program", "cmd.exe")]);
        var rows = (await service.ReadAsync(default)).Where(x => x.Id.StartsWith("unknown:")).ToArray();
        Assert.NotEmpty(rows);
        Assert.All(rows, row => { Assert.Equal("", Value(row, "ProjectId")); Assert.Equal("미확인", Value(row, "WorkerKind")); Assert.Equal("unknown", row.Status); Assert.Null(row.OwnedProgramId); });
    }

    [Theory]
    [InlineData("program", "cmd.exe", "CMD")]
    [InlineData("program", "powershell.exe", "PowerShell")]
    [InlineData("program", "pwsh.exe", "PowerShell")]
    [InlineData("Codex", "cmd.exe", "Codex")]
    [InlineData("Jev", "node.exe", "Jev")]
    [InlineData("program", "custom.exe", "program")]
    public async Task WorkerClassificationRequiresRegisteredProgramAndMatchingCommand(string kind, string executable, string expected)
    {
        using var fixture = new DFixture(); fixture.Job("known", "p", "p/run", "검증"); fixture.Job("mismatch", "p", "p/run", "other command");
        var service = new WorkDashboardService(fixture.Runs); RegisterCatalog(service, [Project("p", "p", "p/run", kind, executable)]);
        var rows = await service.ReadAsync(default);
        Assert.Equal(expected, Value(rows.Single(x => x.Id == "local:known"), "WorkerKind"));
        Assert.Equal("미확인", Value(rows.Single(x => x.Id == "local:mismatch"), "WorkerKind"));
    }

    [Fact]
    public async Task UnregisteredToolLookingTitlesDoNotInventWorkerOrKnownProject()
    {
        using var fixture = new DFixture();
        fixture.Job("unknown", "VoiceAudiobook", "unknown/codex", "PowerShell Jev Codex CMD");
        var service = new WorkDashboardService(fixture.Runs);
        RegisterCatalog(service, [Project("audiobook", "무직전생오디오북", "audiobook/program", "program", "cmd.exe")]);
        var row = (await service.ReadAsync(default)).Single(x => x.Id == "local:unknown");
        Assert.Equal("VoiceAudiobook", Value(row, "ProjectId"));
        Assert.NotEqual("무직전생오디오북", Value(row, "ProjectDisplayName"));
        Assert.Equal("미확인", Value(row, "WorkerKind"));
        Assert.Null(row.OwnedProgramId);
    }

    [Fact]
    public async Task OnlyExplicitAliasConnectsNoticeProjectAndManagerScopeIsExplicit()
    {
        using var fixture = new DFixture();
        fixture.Exchange("manager", "Control-Tower"); fixture.Exchange("project", "VoiceAudiobook");
        fixture.Exchange("not-manager", "control-tower");
        var service = new WorkDashboardService(fixture.Runs); service.ExchangeRoots.Add(fixture.Inbox);
        RegisterCatalog(service, [Project("audiobook", "무직전생오디오북", "audiobook/program", "program", "cmd.exe")], new Dictionary<string, string> { ["VoiceAudiobook"] = "audiobook" });
        var register = RequiredMethod(typeof(WorkDashboardService), "RegisterManagementProjects");
        register.Invoke(service, [new[] { "Control-Tower" }]);
        var rows = await service.ReadAsync(default);
        var manager = rows.Where(x => x.IsManagementRecord).ToArray();
        Assert.Single(manager); Assert.Equal("Control-Tower", Value(manager[0], "ProjectId"));
        Assert.Equal("무직전생오디오북", Value(rows.Single(x => x.Id.EndsWith(":project")), "ProjectDisplayName"));
        Assert.False(rows.Single(x => x.Id.EndsWith(":not-manager")).IsManagementRecord);
    }

    [Fact]
    public async Task CommandAndResultAreBoundedSanitizedMetadataNotPromptOrRawArguments()
    {
        using var fixture = new DFixture();
        fixture.Job("safe", "p", "p/program", "검증", 0);
        var project = Project("p", "프로젝트", "p/program", "program", "cmd.exe", ["/c", "Authorization: Bearer PRIVATE_TOKEN", "PRIVATE_PROMPT"]);
        var service = new WorkDashboardService(fixture.Runs); RegisterCatalog(service, [project]);
        var row = (await service.ReadAsync(default)).Single(x => x.Id == "local:safe");
        Assert.Contains("cmd.exe", Value(row, "CommandSummary"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0", Value(row, "ResultSummary"));
        Assert.DoesNotContain("PRIVATE", Value(row, "CommandSummary"));
        Assert.InRange(Value(row, "CommandSummary").Length, 1, 4000);
        Assert.InRange(Value(row, "ResultSummary").Length, 1, 4000);
    }

    [Fact]
    public async Task NewerDetailRequestForSameJobCannotBeOverwrittenByOlderReply()
    {
        var older=new TaskCompletionSource<string>();var newer=new TaskCompletionSource<string>();var calls=0;
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]),_=>++calls==1?older.Task:newer.Task);
        vm.Selected=new() { Id="same-job" };
        var first=vm.LoadSelectedDetailsAsync();var second=vm.LoadSelectedDetailsAsync();
        newer.SetResult("new terminal result");await second;
        older.SetResult("old acceptance result");await first;
        Assert.Equal("new terminal result",vm.SelectedDetails);
    }

    [Fact]
    public void PerJobStageAndLogReadingPositionsSurviveSameIdentityRefreshAndOtherSelection()
    {
        var type=typeof(WorkActivity).Assembly.GetType("AIControlTower.Models.WorkReadingPositionStore");Assert.NotNull(type);
        var store=Activator.CreateInstance(type!)!;var save=RequiredMethod(type!,"Save");var read=RequiredMethod(type!,"Read");
        save.Invoke(store,["one",30d,90d]);save.Invoke(store,["two",7d,12d]);
        var restored=read.Invoke(store,["one"])!;
        Assert.Equal(30d,(double)restored.GetType().GetField("Item1")!.GetValue(restored)!);
        Assert.Equal(90d,(double)restored.GetType().GetField("Item2")!.GetValue(restored)!);
        var unknown=read.Invoke(store,["never-selected"])!;
        Assert.Equal(0d,(double)unknown.GetType().GetField("Item1")!.GetValue(unknown)!);
    }

    [Fact]
    public async Task FailedAIReceiptOverridesSuccessfulWrapperProcess()
    {
        using var fixture = new DFixture(); fixture.ReceiptJob("run", 0, "run", "failed", 1, "agent_failed");
        var row = await ReceiptRow(fixture);
        Assert.Equal("succeeded", Value(row, "ProcessStatus")); Assert.Equal("failed", Value(row, "AICompletionState"));
        Assert.Equal("failed", row.Status); Assert.Contains("AI 실패 증거", Value(row, "ResultSummary"));
    }

    [Fact]
    public async Task MatchingTerminalReportArtifactReceiptConfirmsAIOnlyWhenOuterProcessSucceeded()
    {
        using var fixture = new DFixture(); fixture.ReceiptJob("run", 0, "run", "succeeded", 0, null);
        var row = await ReceiptRow(fixture);
        Assert.Equal("succeeded", Value(row, "AICompletionState")); Assert.Contains("AI 완료 증거 확인", Value(row, "ResultSummary"));
    }

    [Fact]
    public async Task ReceiptFromAnotherExecutionCannotConfirmThisAIWork()
    {
        using var fixture = new DFixture(); fixture.ReceiptJob("run", 0, "different-run", "succeeded", 0, null);
        var row = await ReceiptRow(fixture);
        Assert.Equal("unknown", Value(row, "AICompletionState")); Assert.Contains("AI 업무 완료 미확인", Value(row, "ResultSummary"));
        Assert.DoesNotContain("AI 완료 증거 확인", Value(row, "ResultSummary")); Assert.Equal("unknown", row.Status);
    }

    [Fact]
    public async Task NonzeroOuterExitWinsOverSuccessfulAIReceipt()
    {
        using var fixture = new DFixture(); fixture.ReceiptJob("run", 1, "run", "succeeded", 0, null);
        var row = await ReceiptRow(fixture);
        Assert.Equal("failed", row.Status); Assert.NotEqual("succeeded", Value(row, "AICompletionState"));
        Assert.DoesNotContain("AI 완료 증거 확인", Value(row, "ResultSummary"));
    }

    private static async Task<WorkActivity> ReceiptRow(DFixture fixture)
    {
        var service = new WorkDashboardService(fixture.Runs); RegisterCatalog(service, [Project("p", "프로젝트", "p/run", "Codex", "codex.exe")]);
        return (await service.ReadAsync(default)).Single(x => x.Id == "local:run");
    }

    [Fact]
    public void ProjectWorkerFiltersGroupsAndSelectionRemainStableAcrossPolling()
    {
        var vm = new WorkDashboardViewModel(_ => Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        var one = Row("one", "p-one", "같은 이름", "PowerShell");
        var two = Row("two", "p-two", "같은 이름", "Codex");
        vm.ApplySnapshot([one, two]);
        var groups = (IEnumerable)RequiredProperty(vm.GetType(), "ProjectGroups").GetValue(vm)!;
        Assert.Equal(2, groups.Cast<object>().Count());
        Set(vm, "ProjectFilterId", "p-two"); Set(vm, "WorkerFilterKey", "Codex"); vm.Selected = two;
        Assert.Single(vm.FilteredActivities); Assert.Equal("two", vm.FilteredActivities[0].Id);
        vm.ApplySnapshot([one with { Title = "new" }, two with { Stage = "next checkpoint" }]);
        Assert.Equal("p-two", Value(vm, "ProjectFilterId")); Assert.Equal("Codex", Value(vm, "WorkerFilterKey"));
        Assert.Equal("two", vm.Selected?.Id); Assert.Single(vm.FilteredActivities);
    }

    [Fact]
    public void ProjectWorkAndManagerProgressAreSeparatedRatherThanDuplicated()
    {
        var vm = new WorkDashboardViewModel(_ => Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.ApplySnapshot([new() { Id = "job", Project = "audiobook" }, new() { Id = "manager", Project = "Control-Tower", IsManagementRecord = true }]);
        Assert.Equal("프로젝트 작업", vm.Heading);
        Assert.Single(vm.FilteredActivities); Assert.Equal("job", vm.FilteredActivities[0].Id);
        vm.ManagementOnly = true;
        Assert.Equal("통합관리 진행", vm.Heading);
        Assert.Single(vm.FilteredActivities); Assert.Equal("manager", vm.FilteredActivities[0].Id);
    }

    [Fact]
    public void DashboardHasNamedBoundedColumnAndRowSplittersWithoutWindowFixture()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/AIControlTower/Views/WorkDashboardView.xaml"));
        var document = XDocument.Load(root); XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var column = Assert.Single(document.Descendants(), e => e.Name.LocalName == "GridSplitter" && (string?)e.Attribute(x + "Name") == "WorkColumnsSplitter");
        var row = Assert.Single(document.Descendants(), e => e.Name.LocalName == "GridSplitter" && (string?)e.Attribute(x + "Name") == "DetailRowsSplitter");
        Assert.Equal("Columns", (string?)column.Attribute("ResizeDirection"));
        Assert.Equal("Rows", (string?)row.Attribute("ResizeDirection"));
        Assert.Contains(document.Descendants(), e => e.Name.LocalName == "ColumnDefinition" && e.Attribute("MinWidth") is not null);
        Assert.Contains(document.Descendants(), e => e.Name.LocalName == "RowDefinition" && e.Attribute("MinHeight") is not null);
    }

    private static ProjectItem Project(string id, string name, string program, string kind, string executable, string[]? arguments = null) => new()
    {
        Id = id, Name = name, DisplayName = name, Path = @"D:\fixture-registered-project",
        Functions = [new() { Id = "function", Programs = [new() { Id = program, ProjectId = id, Kind = kind, Name = "registered program", Commands = [new() { Name = "검증", FileName = executable, Arguments = arguments ?? [] }] }] }]
    };
    private static void RegisterCatalog(WorkDashboardService service, IEnumerable<ProjectItem> projects, IReadOnlyDictionary<string, string>? aliases = null)
        => RequiredMethod(typeof(WorkDashboardService), "RegisterCatalog").Invoke(service, [projects, aliases]);
    private static MethodInfo RequiredMethod(Type type, string name) { var method = type.GetMethod(name); Assert.NotNull(method); return method!; }
    private static PropertyInfo RequiredProperty(Type type, string name) { var property = type.GetProperty(name); Assert.NotNull(property); return property!; }
    private static string Value(object target, string name) => RequiredProperty(target.GetType(), name).GetValue(target)?.ToString() ?? "";
    private static void Set(object target, string name, object value) => RequiredProperty(target.GetType(), name).SetValue(target, value);
    private static WorkActivity Row(string id, string projectId, string display, string worker)
    {
        var row = new WorkActivity { Id = id, Project = projectId, Worker = worker };
        Set(row, "ProjectId", projectId); Set(row, "ProjectDisplayName", display); Set(row, "WorkerKind", worker); return row;
    }
    private sealed class DFixture : IDisposable
    {
        public string Root { get; } = Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks", "dashboard-project-test-" + Guid.NewGuid().ToString("N"));
        public string Runs => Path.Combine(Root, "runs");
        public string Inbox => Path.Combine(Root, "inbox");
        public DFixture() { Directory.CreateDirectory(Runs); Directory.CreateDirectory(Inbox); }
        public void Job(string id, string project, string program, string command, int? exit = null)
        {
            var directory = Path.Combine(Runs, id); Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "result.json"), JsonSerializer.Serialize(new { Id = id, ProjectId = project, ProgramId = program, Command = command, State = exit == 0 ? "succeeded" : "running", StartedAt = DateTimeOffset.UtcNow, ExitCode = exit, LogPath = "" }));
        }
        public void ReceiptJob(string id,int outerExit,string execution,string status,int receiptExit,string? failure)
        {
            var directory=Path.Combine(Runs,id);Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,"result.json"),JsonSerializer.Serialize(new { Id=id,ProjectId="p",ProgramId="p/run",Command="검증",State=outerExit==0?"succeeded":"failed",StartedAt=DateTimeOffset.UtcNow,ExitCode=outerExit,LogPath="",
                aiReceipt=new { schemaVersion=1,adapter="codex-jsonl-v1",executionId=execution,requestedModel="fixture-model",status,processExitCode=receiptExit,terminalObserved=true,reportObserved=true,reportRequired=true,artifactVerified=true,artifactSha256=new string('a',64),failureCode=failure } }));
        }
        public void Exchange(string id, string project)
        {
            File.WriteAllText(Path.Combine(Inbox, id + ".json"), JsonSerializer.Serialize(new { schemaVersion = 1, recordType = "task_exchange", recordId = id, revision = 1, title = "fixture", projectId = project, actorId = "fixture", sessionId = "fixture", receivedAt = "2026-10-07T01:00:00+09:00", updatedAt = "2026-10-07T02:00:00+09:00", request = new { summary = "request", details = "", source = "fixture" }, response = new { summary = "response", details = "", source = "fixture" }, status = "in_progress", workDone = Array.Empty<string>(), verification = Array.Empty<object>(), nextActions = new[] { "next" }, blockers = Array.Empty<string>(), supersedes = Array.Empty<string>() }));
        }
        public void Dispose() => Directory.Delete(Root, true);
    }
}
