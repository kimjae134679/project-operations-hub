using System.Reflection;
using System.Text.Json;
using System.Security.Cryptography;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class LocalViewStartupTests
{
    private static Type Policy => typeof(App).Assembly.GetType("AIControlTower.Services.StartupPolicy")!;
    private static string Fixture()
    {
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","local-view-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root); return root; // Retained, test-owned evidence; never clean operating paths.
    }
    private static void ReadOnly(ControlTowerSettings settings, string root)
    {
        var p=typeof(ControlTowerSettings).GetProperty("TransientReadOnly"); Assert.NotNull(p); p!.SetValue(settings,true);
        var data=typeof(ControlTowerSettings).GetProperty("TransientDataDirectory"); Assert.NotNull(data); data!.SetValue(settings,Path.Combine(root,"data"));
    }
    private static Dictionary<string,string> Snapshot(string root) => Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories)
        .ToDictionary(p=>Path.GetRelativePath(root,p),p=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)))+":"+(int)File.GetAttributes(p));
    private static void Same(Dictionary<string,string> before,string root)
    { var after=Snapshot(root); Assert.Equal(before.Count,after.Count); foreach(var p in before) Assert.Equal(p.Value,after[p.Key]); }
    private static void Sta(Action action)
    {
        Exception? failure=null; var thread=new Thread(()=>{try{action();}catch(Exception e){failure=e;}});
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); Assert.True(thread.Join(TimeSpan.FromSeconds(25)));
        if(failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
    private static string SourceRoot()
    {
        for(var d=new DirectoryInfo(AppContext.BaseDirectory);d is not null;d=d.Parent)
            if(File.Exists(Path.Combine(d.FullName,"ai-control-tower","src","AIControlTower","App.xaml.cs")))return d.FullName;
        throw new InvalidOperationException("Source root not found");
    }
    [Fact] public void ExplicitStartupPolicyExists() => Assert.NotNull(Policy);
    [Fact] public void LocalViewDuplicateNeverActivatesOperationalWindow()
    { Assert.False(App.ShouldActivateExistingInstance(["--local-view"])); Assert.True(App.ShouldActivateExistingInstance([])); }
    [Fact] public void ReadOnlySettingsPreservePathsAndNeverSave()
    {
        var root=Fixture();var path=Path.Combine(root,"settings.json");
        File.WriteAllText(path,JsonSerializer.Serialize(new{RootPath=root,CommunicationHubPath=root,EnableJev=true,DarkMode=true}));
        var before=Snapshot(root);var load=typeof(ControlTowerSettings).GetMethod("LoadReadOnly");Assert.NotNull(load);
        var settings=(ControlTowerSettings)load!.Invoke(null,[path])!; Assert.Equal(root,settings.RootPath);Assert.Equal(root,settings.CommunicationHubPath);
        Assert.True((bool)typeof(ControlTowerSettings).GetProperty("TransientReadOnly")!.GetValue(settings)!);
        settings.RootPath="changed in memory";settings.Save();Same(before,root);
    }
    [Fact] public void MissingOversizeOrInvalidSettingsDoNotCreateFallbackFiles()
    {
        var root=Fixture(); var load=typeof(ControlTowerSettings).GetMethod("LoadReadOnly"); Assert.NotNull(load);
        File.WriteAllText(Path.Combine(root,"invalid.json"),"{");File.WriteAllText(Path.Combine(root,"large.json"),new string('x',262145));var before=Snapshot(root);
        foreach(var name in new[]{"missing.json","invalid.json","large.json"})
        {var settings=(ControlTowerSettings)load!.Invoke(null,[Path.Combine(root,name)])!;Assert.Equal(@"D:\A_KJ\AI",settings.RootPath);settings.Save();}
        Same(before,root);
    }
    [Fact] public async Task LocalInitializationInvokesOnlyLocalReadsAndNoRecovery()
    {
        Assert.NotNull(Policy); var value=Activator.CreateInstance(Policy,[true])!; var counts=new int[4];
        var recovery=Policy.GetMethod("RecoverRuns"); Assert.NotNull(recovery); recovery!.Invoke(value,[new Action(()=>counts[3]++)]);
        var init=Policy.GetMethod("InitializeAsync"); Assert.NotNull(init);
        await (Task)init!.Invoke(value,[new Func<Task>(()=>{counts[0]++;return Task.CompletedTask;}),new Func<Task>(()=>{counts[1]++;return Task.CompletedTask;}),new Func<Task>(()=>{counts[2]++;return Task.CompletedTask;})])!;
        Assert.Equal(new[]{1,1,0,0},counts);
    }
    [Fact] public async Task NormalPolicyRetainsRecoveryAndOperationalInitialization()
    {
        Assert.NotNull(Policy); var value=Activator.CreateInstance(Policy,[false])!;var counts=new int[4];
        Policy.GetMethod("RecoverRuns")!.Invoke(value,[new Action(()=>counts[3]++)]);
        await (Task)Policy.GetMethod("InitializeAsync")!.Invoke(value,[new Func<Task>(()=>{counts[0]++;return Task.CompletedTask;}),new Func<Task>(()=>{counts[1]++;return Task.CompletedTask;}),new Func<Task>(()=>{counts[2]++;return Task.CompletedTask;})])!;
        Assert.Equal(new[]{0,0,1,1},counts);
    }
    [Fact] public void LocalViewGuardsRemoteProgramAndPcCommandsWithoutWindow()
    {
        var root=Fixture();var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root,EnableJev=true};ReadOnly(settings,root);
        var before=Snapshot(root);
        Sta(()=>
        {
            var recoveries=0;using var vm=new MainViewModel(settings,true,()=>recoveries++);Assert.Equal(0,recoveries);Assert.False(vm.IsLocalJevExecutionAllowed);
            Assert.False(vm.PcConnection.ConnectCommand.CanExecute(null));Assert.False(vm.PcConnection.PauseCommand.CanExecute(null));
            Assert.False(vm.PcConnection.StopCommand.CanExecute(null));Assert.False(vm.PcConnection.InstallCommand.CanExecute(null));
            vm.PcConnection.ConnectCommand.Execute(null);vm.PcConnection.StopCommand.Execute(null);
            vm.LaunchProgramAsync().GetAwaiter().GetResult();vm.StopProgram();vm.RunJevTask();vm.CancelJevTaskAsync().GetAwaiter().GetResult();
            vm.EnsureRemoteRunningAsync().GetAwaiter().GetResult();vm.StopRemoteRunningAsync().GetAwaiter().GetResult();vm.ConsolidateRemoteStartupAsync().GetAwaiter().GetResult();
            vm.RefreshAsync().GetAwaiter().GetResult();vm.RefreshServerAsync(true).GetAwaiter().GetResult();vm.RefreshRosterAsync(true).GetAwaiter().GetResult();
            Assert.Contains("로컬 조회",vm.Message);Assert.False(vm.PcConnection.IsConnected);
            Assert.Throws<InvalidOperationException>(()=>vm.PcConnection.SubmitAsync("fixture","test","run",new{}).GetAwaiter().GetResult());
            Assert.Throws<InvalidOperationException>(()=>vm.PcConnection.ResultAsync("fixture").GetAwaiter().GetResult());
        });
        Same(before,root);
    }
    private static void Board(string root)
    {
        var dir=Path.Combine(root,"04_COMMUNICATION","announcements");Directory.CreateDirectory(dir);
        const string body="# Fixture notice\nRead this locally.";File.WriteAllText(Path.Combine(dir,"notice.md"),body);
        var hash=Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        File.WriteAllText(Path.Combine(dir,"manifest.json"),JsonSerializer.Serialize(new{schemaVersion=1,projects=new[]{new{id="fixture",name="fixture",required=true}},participants=new[]{new{id="Sol",name="Sol"}},notices=new[]{new{id="N-0001",revision=1,title="fixture",path="notice.md",contentSha256=hash,targets=new[]{"*"},required=true,active=true}}}));
        var thread=Path.Combine(root,"04_COMMUNICATION","threads","fixture");Directory.CreateDirectory(thread);File.WriteAllText(Path.Combine(thread,"THREAD.md"),"# Fixture conversation\n\n## Reply\nlocal reply body");
    }
    [Fact] public void LocalCommunicationShowsExistingNoticesAndThreadsWithoutSyncWrites()
    {
        var root=Fixture();Board(root);var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root};ReadOnly(settings,root);var before=Snapshot(root);
        Sta(()=>
        {
            using var vm=new MainViewModel(settings); vm.SyncCommunicationAsync(true).GetAwaiter().GetResult();
            Assert.Single(vm.Notices);Assert.Contains(vm.InboxItems,x=>x.Body.Contains("local reply body"));
            vm.SelectedInbox=vm.InboxItems.First();vm.MarkCommunicationViewed();
            Assert.Contains("로컬 조회",vm.CentralSyncMessage);
        });Same(before,root);
    }
    [Fact] public void InvalidLocalBoardIsExplicitlyBlockedNotFakeConnected()
    {
        var root=Fixture();var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root};ReadOnly(settings,root);
        Sta(()=>{using var vm=new MainViewModel(settings);vm.SyncCommunicationAsync(true).GetAwaiter().GetResult();Assert.Empty(vm.Notices);Assert.NotEmpty(vm.CommunicationErrors);Assert.Contains("보류",vm.CentralSyncMessage);});
    }
    [Fact] public void SourceModeSelectionPrecedesRemoteAndUsesIndependentIdentityAndNoRepeatListener()
    {
        var source=File.ReadAllText(Path.Combine(SourceRoot(),"ai-control-tower","src","AIControlTower","App.xaml.cs"));
        var startup=source[source.IndexOf("protected override void OnStartup")..];
        Assert.True(startup.IndexOf("StartupPolicy.TryCreate")<startup.IndexOf("if (StartRemoteSupervisor(e))"));
        Assert.Contains("LocalView",source);Assert.Contains("LoadReadOnly",source);Assert.Contains("if (!policy.IsLocalView)",source);
    }
    [Theory]
    [InlineData("--remote-supervisor")][InlineData("--remote-control")][InlineData("--background")][InlineData("--verify-ui")][InlineData("--verify-work-dashboard")]
    public void MixedLocalViewModesAreRejectedBeforeOperationalStartup(string other)
    {Assert.False(StartupPolicy.TryCreate(["--local-view",other],out var policy));Assert.True(policy.IsLocalView);}
    [Fact] public void LocalIdentityAndActivationAreSeparateAndDefaultFlagsStayOperational()
    {
        Assert.True(StartupPolicy.TryCreate(["--local-view"],out var local));Assert.Equal("LocalView",local.InstanceIdentity);
        Assert.Equal(@"Local\AIControlTower.LocalView.Activate",local.ActivationEvent);
        Assert.True(StartupPolicy.TryCreate(["--background"],out var normal));Assert.False(normal.IsLocalView);Assert.True(normal.AllowMutations);
        Assert.Equal("Application",normal.InstanceIdentity);Assert.NotEqual(normal.ActivationEvent,local.ActivationEvent);
    }
    [Fact] public void RegisteredRunStaysRunningInFileButLivenessUnknownInLocalViewer()
    {
        var root=Fixture();var runs=Path.Combine(root,"data","runs","fixture");Directory.CreateDirectory(runs);
        File.WriteAllText(Path.Combine(runs,"result.json"),JsonSerializer.Serialize(new{Id="fixture",ProjectId="fixture",ProgramId="fixture/jev",Command="fixture",State="running",StartedAt="2026-10-07T00:00:00+09:00",LogPath=""}));
        var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root};ReadOnly(settings,root);var before=Snapshot(root);
        Sta(()=>{using var vm=new MainViewModel(settings);vm.RefreshWorkDashboardAsync().GetAwaiter().GetResult();var row=Assert.Single(vm.WorkDashboard.Activities,a=>a.Id=="local:fixture");Assert.Equal("running",row.Status);Assert.False(row.LivenessKnown);Assert.Null(row.OwnedProgramId);Assert.False(vm.WorkDashboard.CanStopOwned);});
        Same(before,root);
    }
    [Fact] public void RegisteredTaskExchangeLatestRevisionIsReadLocallyWithoutSharing()
    {
        var root=Fixture();var outbox=Path.Combine(root,"_통합소통","보낼자료");Directory.CreateDirectory(outbox);
        object Record(int revision)=>new{schemaVersion=1,recordType="task_exchange",recordId="fixture-request",revision,title="local fixture",projectId="fixture",actorId="fixture",sessionId="fixture",receivedAt="2026-10-07T00:00:00+09:00",updatedAt="2026-10-07T01:00:00+09:00",request=new{summary="fixture",details="fixture",source="current_chat"},response=new{summary="fixture",details="fixture",source="current_chat"},status="in_progress",workDone=Array.Empty<string>(),verification=Array.Empty<object>(),nextActions=new[]{"remaining fixture"},blockers=Array.Empty<string>(),supersedes=Array.Empty<string>()};
        File.WriteAllText(Path.Combine(outbox,"v1.json"),JsonSerializer.Serialize(Record(1)));File.WriteAllText(Path.Combine(outbox,"v3.json"),JsonSerializer.Serialize(Record(3)));
        var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root,CommunicationFolders=new(){{"fixture",root}}};ReadOnly(settings,root);var before=Snapshot(root);
        Sta(()=>{using var vm=new MainViewModel(settings,true,()=>throw new InvalidOperationException("Fixture forbids recovery"));vm.RefreshWorkDashboardAsync().GetAwaiter().GetResult();var row=Assert.Single(vm.WorkDashboard.Activities,a=>a.Id=="exchange:fixture:fixture:fixture-request");Assert.False(row.IsManagementRecord);Assert.Equal(3,row.Revision);Assert.False(row.LivenessKnown);Assert.Null(row.OwnedProgramId);});
        Same(before,root);
    }
    [Fact] public void MutatingUiHasBothDisabledControlsAndHandlerChecks()
    {
        var root=Path.Combine(SourceRoot(),"ai-control-tower","src","AIControlTower");var window=File.ReadAllText(Path.Combine(root,"MainWindow.xaml.cs"));
        foreach(var name in new[]{"Install_Click","Uninstall_Click","Restore_Click","StopRemote_Click","Jev_Click"})
        {var at=window.IndexOf("void "+name);Assert.True(at>0);Assert.Contains(name=="Jev_Click"?"BlockRegisteredOperation()":"BlockOperation()",window.Substring(at,Math.Min(200,window.Length-at)));}
        var panel=File.ReadAllText(Path.Combine(root,"PcJobsWindow.cs"));Assert.Contains("IsEnabled=!vm.IsReadOnly",panel);Assert.Contains("if(vm.IsReadOnly)",panel);
        var communication=File.ReadAllText(Path.Combine(root,"Views","CommunicationWorkspaceView.xaml"));Assert.DoesNotContain("Click=\"LinkCommunicationProject_Click\"",communication);
        var vm=File.ReadAllText(Path.Combine(root,"ViewModels","MainViewModel.cs"));Assert.Contains("if(CanMutate)_jobs.StopAllOwned()",vm);Assert.Contains("enablePolling && !IsReadOnlyView",vm);
    }
}
