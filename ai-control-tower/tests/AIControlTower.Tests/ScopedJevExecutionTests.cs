using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class ScopedJevExecutionTests
{
    [Theory]
    [InlineData(@"D:\", @"D:\child", true)]
    [InlineData(@"D:\child", @"D:\", true)]
    [InlineData(@"\\fixture-server\fixture-share\", @"\\fixture-server\fixture-share\child", true)]
    [InlineData(@"\\fixture-server\fixture-share\child", @"\\fixture-server\fixture-share\", true)]
    [InlineData(@"D:\a", @"D:\ab", false)]
    public void PathOverlapIncludesDriveAndUncRootAncestorsWithoutPrefixCollisions(string left, string right, bool expected)
        => Assert.Equal(expected, JevExecutionWorkspace.Overlaps(left, right));
    private const string FixtureBase = @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\scoped-jev";
    private static string Fixture()
    {
        var root = Path.Combine(FixtureBase, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "a")); Directory.CreateDirectory(Path.Combine(root, "b"));
        return root;
    }
    private static ProgramItem Program(string id, string work, string project = "fixture") => new()
    { Id = id, ProjectId = project, Name = id, Kind = "Jev", WorkingDirectory = work, Path = work, Commands = [Hold()] };
    private static ProgramCommand Hold() => new()
    {
        Name = "fixture", FileName = "powershell.exe", TimeoutSeconds = 20,
        Arguments = ["-NoProfile", "-Command", "[IO.File]::WriteAllText((Join-Path (Get-Location) 'ready.pid'),[string]$PID); while(!(Test-Path 'release')){Start-Sleep -Milliseconds 40}; exit 0"]
    };
    private static Task<JobResult> Scoped(JobManager manager, ProgramItem program, string root, CancellationToken ct = default)
    {
        var method = typeof(JobManager).GetMethods().Single(m => m.Name == "RunAsync");
        Assert.Contains(method.GetParameters(), p => p.Name == "independentWorkspaceRoot");
        return (Task<JobResult>)method.Invoke(manager, [program, Hold(), ct, null, null, root])!;
    }
    private static async Task<int> Ready(string work)
    {
        var until = DateTime.UtcNow.AddSeconds(8); var path = Path.Combine(work, "ready.pid");
        while (DateTime.UtcNow < until)
        {
            if (File.Exists(path) && int.TryParse(File.ReadAllText(path), out var pid)) return pid;
            await Task.Delay(30);
        }
        Assert.Fail("Owned fixture did not become ready."); return 0;
    }
    private static bool Alive(int pid)
    { try { using var p = System.Diagnostics.Process.GetProcessById(pid); return !p.HasExited; } catch (ArgumentException) { return false; } }
    private static void Cleanup(string root)
    {
        Assert.StartsWith(Path.GetFullPath(FixtureBase) + Path.DirectorySeparatorChar, Path.GetFullPath(root));
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)) File.Delete(path);
        foreach (var path in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).OrderByDescending(p => p.Length)) Directory.Delete(path);
        Directory.Delete(root);
    }
    [Fact]
    public async Task DeclaredSiblingJevWorkspacesRunTogetherAndStopIsScopeOwned()
    {
        var root = Fixture(); var manager = new JobManager(Path.Combine(root, "data"));
        Task<JobResult>? a = null, b = null;
        try
        {
            a = Scoped(manager, Program("fixture/jev/a", Path.Combine(root, "a")), root);
            var pidA = await Ready(Path.Combine(root, "a"));
            b = Scoped(manager, Program("fixture/jev/b", Path.Combine(root, "b")), root);
            var pidB = await Ready(Path.Combine(root, "b"));
            Assert.True(Alive(pidA)); Assert.True(Alive(pidB)); Assert.Equal(2, manager.RunningCount); Assert.Equal(1, manager.BusyProjectCount);
            Assert.True(manager.Stop("fixture/jev/a")); Assert.Equal("cancelled", (await a).State);
            Assert.False(Alive(pidA)); Assert.True(Alive(pidB)); Assert.True(manager.IsProjectBusy("fixture"));
            File.WriteAllText(Path.Combine(root, "b", "release"), "go"); Assert.Equal("succeeded", (await b).State);
            Assert.False(manager.IsProjectBusy("fixture"));
        }
        finally { manager.StopAllOwned(); if (a is not null) await a; if (b is not null) await b; Cleanup(root); }
    }
    [Fact]
    public async Task DefaultSharedProjectExecutionStillRejectsSecondProgram()
    {
        var root = Fixture(); var manager = new JobManager(Path.Combine(root, "data"));
        var a = manager.RunAsync(Program("fixture/shared/a", Path.Combine(root, "a")), Hold());
        try
        {
            await Ready(Path.Combine(root, "a"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.RunAsync(Program("fixture/shared/b", Path.Combine(root, "b")), Hold()));
            await Assert.ThrowsAsync<InvalidOperationException>(() => Scoped(manager, Program("fixture/jev/b", Path.Combine(root, "b")), root));
        }
        finally { manager.StopAllOwned(); await a; Cleanup(root); }
    }
    [Fact]
    public async Task DuplicateProgramAndOverlappingPathsRemainRejectedIncludingOtherProject()
    {
        var root = Fixture(); var manager = new JobManager(Path.Combine(root, "data")); Task<JobResult>? a = null;
        try
        {
            a = Scoped(manager, Program("fixture/jev/a", Path.Combine(root, "a")), root); await Ready(Path.Combine(root, "a"));
            Directory.CreateDirectory(Path.Combine(root, "a", "nested"));
            foreach (var path in new[] { Path.Combine(root, "a"), Path.Combine(root, "a").ToUpperInvariant() + "\\", Path.Combine(root, "a", "nested") })
                await Assert.ThrowsAsync<InvalidOperationException>(() => Scoped(manager, Program("other/jev", path, "other"), root));
            await Assert.ThrowsAsync<InvalidOperationException>(() => Scoped(manager, Program("fixture/jev/a", Path.Combine(root, "b"), "other"), root));
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.RunAsync(Program("fixture/shared", Path.Combine(root, "b")), Hold()));
        }
        finally { manager.StopAllOwned(); if (a is not null) await a; Cleanup(root); }
    }
    [Fact]
    public void ExplicitJevResolverBindsRegisteredWorkspaceAndLegacyDefaultStaysExclusive()
    {
        var root = Fixture();
        try
        {
            var type = typeof(JobManager).Assembly.GetType("AIControlTower.Services.JevExecutionWorkspace"); Assert.NotNull(type);
            var resolve = type!.GetMethod("Resolve"); Assert.NotNull(resolve);
            var a = Program("fixture/a", Path.Combine(root, "a")); var b = Program("fixture/b", Path.Combine(root, "b"));
            var project = new ProjectItem { Id = "fixture", Path = root, Functions = [new FunctionItem { Programs = [a, b] }] };
            object Target(ProgramItem selected, bool optIn) => resolve!.Invoke(null, [project, selected, root, optIn])!;
            string Id(object target) => ((ProgramItem)target.GetType().GetProperty("Program")!.GetValue(target)!).Id;
            var first = Target(a, true); Assert.NotEqual(Id(first), Id(Target(b, true))); Assert.Equal(Id(first), Id(Target(a, true)));
            Assert.Equal("fixture/jev", Id(Target(a, false)));
            Assert.Null(Target(a, false).GetType().GetProperty("IndependentWorkspaceRoot")!.GetValue(Target(a, false)));
            Assert.Throws<TargetInvocationException>(() => Target(Program("fixture/spoof", Path.Combine(root, "a")), true));
            var rootProgram = Program("fixture/root", root);
            project.Functions = [new FunctionItem { Programs = [rootProgram] }];
            Assert.Throws<TargetInvocationException>(() => Target(rootProgram, true));
        }
        finally { Cleanup(root); }
    }
    [Fact]
    public async Task GlobalThreeSlotsRemainBoundedAndQueuedScopeRetainsOwnership()
    {
        var root = Fixture(); var manager = new JobManager(Path.Combine(root, "data")); var tasks = new List<Task<JobResult>>();
        using var queuedCancellation = new CancellationTokenSource();
        try
        {
            foreach (var name in new[] { "a", "b", "c", "d" }) Directory.CreateDirectory(Path.Combine(root, name));
            foreach (var name in new[] { "a", "b", "c" })
            {
                tasks.Add(Scoped(manager, Program("fixture/jev/" + name, Path.Combine(root, name)), root)); await Ready(Path.Combine(root, name));
            }
            tasks.Add(Scoped(manager, Program("fixture/jev/d", Path.Combine(root, "d")), root, queuedCancellation.Token));
            Assert.Equal(3, manager.RunningCount); Assert.False(File.Exists(Path.Combine(root, "d", "ready.pid")));
            await Assert.ThrowsAsync<InvalidOperationException>(() => Scoped(manager, Program("other/queued", Path.Combine(root, "d"), "other"), root));
            queuedCancellation.Cancel(); Assert.Equal("cancelled", (await tasks[^1]).State);
            Assert.True(manager.IsProjectBusy("fixture")); Assert.Equal(1, manager.BusyProjectCount);
            var next = Scoped(manager, Program("fixture/jev/new", Path.Combine(root, "d")), root, queuedCancellation.Token);
            Assert.Equal("cancelled", (await next).State); // Queue cancellation releases only its scope.
        }
        finally { queuedCancellation.Cancel(); manager.StopAllOwned(); foreach (var task in tasks) await task; Cleanup(root); }
    }
    [Fact]
    public async Task SemanticAiFailureIsDurableBeforeScopeIsReleased()
    {
        var root = Fixture(); var manager = new JobManager(Path.Combine(root, "data"));
        try
        {
            var program = Program("fixture/jev/a", Path.Combine(root, "a"));
            var command = new ProgramCommand { Name = "Jev 작업", FileName = "powershell.exe", TimeoutSeconds = 10,
                Arguments = ["-NoProfile", "-Command", "Write-Output '{\"type\":\"turn.failed\"}'; exit 0"] };
            var result = await manager.RunAsync(program, command, aiContract: new("codex-jsonl-v1", "gpt-6.1-sol"), independentWorkspaceRoot: root);
            Assert.Equal(0, result.ExitCode); Assert.Equal("failed", result.State);
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(result.LogPath)!, "result.json")));
            Assert.Equal("failed", document.RootElement.GetProperty("aiReceipt").GetProperty("status").GetString());
            Assert.True(manager.CanRun(program, root)); Assert.False(manager.IsProjectBusy("fixture"));
        }
        finally { manager.StopAllOwned(); Cleanup(root); }
    }
    [Fact]
    public void IndependentScopeRejectsRootMissingOutsideAndGenericDeclarations()
    {
        var root = Fixture(); var manager = new JobManager(Path.Combine(root, "data"));
        try
        {
            Assert.False(manager.CanRun(Program("root", root), root));
            Assert.False(manager.CanRun(Program("missing", Path.Combine(root, "missing")), root));
            Assert.False(manager.CanRun(Program("outside", Path.GetDirectoryName(root)!), root));
            Assert.False(manager.CanRun(new ProgramItem { Id = "generic", ProjectId = "fixture", Kind = "program", WorkingDirectory = Path.Combine(root, "a") }, root));
        }
        finally { Cleanup(root); }
    }
    [Fact]
    public async Task DashboardRecognizesOnlyExactRegisteredScopedJevIdentity()
    {
        var root = Fixture();
        try
        {
            var a = Program("fixture/a", Path.Combine(root, "a"));
            var project = new ProjectItem { Id = "fixture", Path = root, Name = "fixture", Functions = [new FunctionItem { Programs = [a] }] };
            var target = JevExecutionWorkspace.Resolve(project, a, root, true);
            var runs = Path.Combine(root, "runs");
            foreach (var (id, program) in new[] { ("known", target.Program.Id), ("unknown", "fixture/jev/unknown") })
            {
                var folder = Path.Combine(runs, id); Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "result.json"), JsonSerializer.Serialize(new JobResult(id, "fixture", program, "Jev 작업", "failed", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 1, Path.Combine(folder, "output.log"))));
            }
            var service = new WorkDashboardService(runs); service.RegisterCatalog([project]);
            var rows = await service.ReadAsync(default);
            Assert.Equal("Jev", rows.Single(r => r.Id == "local:known").WorkerKind);
            Assert.Equal("미확인", rows.Single(r => r.Id == "local:unknown").WorkerKind);
        }
        finally { Cleanup(root); }
    }
    [Fact]
    public void IndependentOptInIsTransientDefaultFalseAndUiBindsSelectedRegisteredName()
    {
        var property = typeof(AIControlTower.ViewModels.MainViewModel).GetProperty("IndependentJevWorkspace"); Assert.NotNull(property);
        var settings = new ControlTowerSettings { TransientReadOnly = true, TransientDataDirectory = Path.Combine(FixtureBase, "view-only"), AutoCommunication = false };
        using var vm = new AIControlTower.ViewModels.MainViewModel(settings, false, () => { });
        Assert.False(vm.IndependentJevWorkspace);
        var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AIControlTower", "JevControlWindow.xaml"));
        var document = XDocument.Load(source);
        Assert.Contains(document.Descendants(), e => e.Name.LocalName == "CheckBox" && (string?)e.Attribute("Content") == "독립 작업 폴더" && ((string?)e.Attribute("IsChecked"))?.Contains("IndependentJevWorkspace") == true);
        Assert.Contains(document.Descendants(), e => e.Name.LocalName == "TextBlock" && ((string?)e.Attribute("Text"))?.Contains("SelectedProgram.Name") == true);
    }
    [Fact]
    public void RegisteredDtoRefreshPreservesExplicitScopeAndCancellationIdentity()
    {
        var root = Fixture();
        try
        {
            var settings = new ControlTowerSettings { TransientReadOnly = true, TransientDataDirectory = Path.Combine(root, "view"), AutoCommunication = false };
            using var vm = new AIControlTower.ViewModels.MainViewModel(settings, false, () => { });
            ProjectItem Project(string path, ProgramItem a, ProgramItem b) => new() { Id = "fixture", Path = path, Functions = [new FunctionItem { Programs = [a, b] }] };
            var a = Program("fixture/a", Path.Combine(root, "a")); var b = Program("fixture/b", Path.Combine(root, "b"));
            vm.SelectedProject = Project(root, a, b); vm.SelectedProgram = b; vm.IndependentJevWorkspace = true;
            var before = JevExecutionWorkspace.Resolve(vm.SelectedProject, vm.SelectedProgram, vm.ProjectPath, vm.IndependentJevWorkspace).Program.Id;
            var refreshedA = Program(a.Id, a.WorkingDirectory); var refreshedB = Program(b.Id, b.WorkingDirectory.ToUpperInvariant() + "\\");
            // Discovery rebuilds project/program DTOs, then restores the previous selected program ID.
            vm.SelectedProject = Project(root, refreshedA, refreshedB);
            vm.SelectedProgram = refreshedB;
            Assert.True(vm.IndependentJevWorkspace);
            Assert.Equal(before, JevExecutionWorkspace.Resolve(vm.SelectedProject, vm.SelectedProgram, vm.ProjectPath, vm.IndependentJevWorkspace).Program.Id);
            Assert.Equal(refreshedB.WorkingDirectory, vm.JevWorkspacePath);
        }
        finally { Cleanup(root); }
    }
    [Fact]
    public void ChangedRegisteredProgramProjectOrWorkspaceClearsExplicitScope()
    {
        var root = Fixture();
        try
        {
            var settings = new ControlTowerSettings { TransientReadOnly = true, TransientDataDirectory = Path.Combine(root, "view"), AutoCommunication = false };
            using var vm = new AIControlTower.ViewModels.MainViewModel(settings, false, () => { });
            var a = Program("fixture/a", Path.Combine(root, "a")); var b = Program("fixture/b", Path.Combine(root, "b"));
            ProjectItem Project(string id, string path, params ProgramItem[] programs) => new() { Id = id, Path = path, Functions = [new FunctionItem { Programs = programs }] };
            vm.SelectedProject = Project("fixture", root, a, b); vm.SelectedProgram = a; vm.IndependentJevWorkspace = true;
            vm.SelectedProgram = b; Assert.False(vm.IndependentJevWorkspace);
            vm.IndependentJevWorkspace = true; vm.SelectedProgram = Program(b.Id, a.WorkingDirectory); Assert.False(vm.IndependentJevWorkspace);
            vm.SelectedProgram = b; vm.IndependentJevWorkspace = true;
            vm.SelectedProject = Project("other", root, Program(b.Id, b.WorkingDirectory, "other")); Assert.False(vm.IndependentJevWorkspace);
            vm.SelectedProject = Project("fixture", root, a); vm.IndependentJevWorkspace = true;
            vm.SelectedProject = Project("fixture", Path.Combine(root, "a"), a); Assert.False(vm.IndependentJevWorkspace);
        }
        finally { Cleanup(root); }
    }
}
