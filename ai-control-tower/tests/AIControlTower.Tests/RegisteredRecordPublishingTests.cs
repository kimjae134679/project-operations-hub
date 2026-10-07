using System.Reflection;
using System.Text.Json;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class RegisteredRecordPublishingTests
{
    private static Type Helper()
    {
        var type=typeof(CommunicationService).Assembly.GetType("AIControlTower.Services.RegisteredRecordPublishing");
        Assert.NotNull(type);return type!;
    }
    private static object Parse(string[] args)
    {
        var method=Helper().GetMethod("ParseOnce",BindingFlags.Public|BindingFlags.Static);Assert.NotNull(method);
        return method!.Invoke(null,[args,new[]{"Threads","Control-Tower"}])!;
    }
    private static object? Property(object value,string name)=>value.GetType().GetProperty(name)!.GetValue(value);
    [Fact] public void ExactHeadlessRequestRequiresRegisteredIdentity()
    {
        var value=Parse(["--publish-records-once","Threads"]);
        Assert.Equal(true,Property(value,"Requested"));Assert.Equal(true,Property(value,"Valid"));Assert.Equal("Threads",Property(value,"ProjectId"));
    }
    [Theory]
    [InlineData("--publish-records-once")]
    [InlineData("--publish-records-once","threads")]
    [InlineData("--publish-records-once","D:\\private")]
    [InlineData("--publish-records-once","Threads","--manual-control")]
    [InlineData("--background","--publish-records-once","Threads")]
    [InlineData("--Publish-Records-Once","Threads")]
    public void MalformedPublicationRequestNeverFallsThroughGui(params string[] args)
    {
        var value=Parse(args);Assert.Equal(true,Property(value,"Requested"));Assert.Equal(false,Property(value,"Valid"));
    }
    [Fact] public void NormalManualStartupIsNotHeadlessPublication()
    {var value=Parse(["--manual-control"]);Assert.Equal(false,Property(value,"Requested"));}
    private sealed class Fixture:IDisposable
    {
        public string Root{get;}=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks", "publication-tests-"+Guid.NewGuid().ToString("N"));
        public string Registry=>Path.Combine(Root,"registrations.json");
        public Fixture(){Directory.CreateDirectory(Root);Write();}
        public void Write(bool enabled=true,string project="Threads",long repository=1368308612)
        {
            File.WriteAllText(Registry,JsonSerializer.Serialize(new {SchemaVersion=1,BoardRoot=Root,Registrations=new[]{new {ProjectId=project,RootPath=Root,OriginUrl="https://github.com/kimjae134679/Threads.git",RepositoryId=repository,RepositoryFullName="kimjae134679/Threads",BaseBranch="main",SharedPrefix="04_COMMUNICATION/shared-records/"+project,Enabled=enabled}}}));
        }
        public object New(TimeSpan? timeout=null)
        {
            var ctor=Helper().GetConstructor([typeof(string),typeof(TimeSpan)]);Assert.NotNull(ctor);
            return ctor!.Invoke([Registry,timeout??TimeSpan.FromMinutes(3)]);
        }
        public void Dispose(){if(Directory.Exists(Root))Directory.Delete(Root,true);}
    }
    private static CommunicationCollectionResult Collection()=>new([new("Threads","outbox","safe.md",@"D:\owned\Threads\_통합소통\보낼자료\safe.md",new string('a',64),@"D:\owned\sink\safe.md","# task_exchange\nbenign evidence",DateTimeOffset.UtcNow)],[],DateTimeOffset.UtcNow);
    private static async Task<string> Publish(object helper,bool manual,Func<string,CommunicationCollectionResult,CancellationToken,Task<string>> publish,CancellationToken ct=default)
    {
        var method=Helper().GetMethod("PublishAsync");Assert.NotNull(method);
        var task=(Task)method!.Invoke(helper,[manual,Collection(),publish,ct])!;await task;
        return (string)task.GetType().GetProperty("Result")!.GetValue(task)!;
    }
    private static bool Running(object helper)=>(bool)Helper().GetProperty("IsRunning")!.GetValue(helper)!;
    [Fact] public async Task LocalViewAndDisabledRegistrationNeverInvokePublisher()
    {
        using var fixture=new Fixture();var calls=0;
        Task<string> Callback(string _,CommunicationCollectionResult __,CancellationToken ___){calls++;return Task.FromResult("pr_open");}
        Assert.Equal("disabled",await Publish(fixture.New(),false,Callback));fixture.Write(false);
        Assert.Equal("disabled",await Publish(fixture.New(),true,Callback));Assert.Equal(0,calls);
    }
    [Fact] public async Task RegistryRepositoryMismatchFailsClosedBeforeTransport()
    {
        using var fixture=new Fixture();fixture.Write(repository:1);var calls=0;
        var value=await Publish(fixture.New(),true,(_,__,___)=>{calls++;return Task.FromResult("pr_open");});
        Assert.Equal("held",value);Assert.Equal(0,calls);
    }
    [Fact] public async Task RegistryFingerprintChangeBlocksCachedExecution()
    {
        using var fixture=new Fixture();var helper=fixture.New();var calls=0;
        Task<string> Callback(string _,CommunicationCollectionResult __,CancellationToken ___){calls++;return Task.FromResult("pr_open");}
        Assert.Equal("pr_open",await Publish(helper,true,Callback));File.AppendAllText(fixture.Registry," ");
        Assert.Equal("held",await Publish(helper,true,Callback));Assert.Equal(1,calls);
    }
    [Fact] public async Task SuccessReplayAndUncertainFailureAreNeverAutomaticallyRetried()
    {
        using var fixture=new Fixture();var helper=fixture.New();var calls=0;
        Task<string> Callback(string _,CommunicationCollectionResult __,CancellationToken ___){calls++;return Task.FromResult("upload_uncertain");}
        Assert.Equal("upload_uncertain",await Publish(helper,true,Callback));Assert.Equal("held",await Publish(helper,true,Callback));Assert.Equal(1,calls);
        var success=fixture.New();calls=0;
        Task<string> Succeeded(string _,CommunicationCollectionResult __,CancellationToken ___){calls++;return Task.FromResult("pr_open");}
        Assert.Equal("pr_open",await Publish(success,true,Succeeded));Assert.Equal("cached",await Publish(success,true,Succeeded));Assert.Equal(1,calls);
    }
    [Fact] public async Task SingleFlightTimeoutKeepsGateUntilIgnoringTransportSettles()
    {
        using var fixture=new Fixture();var helper=fixture.New(TimeSpan.FromMilliseconds(40));var calls=0;
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<string> Callback(string id,CommunicationCollectionResult result,CancellationToken ct){Assert.Equal("Threads",id);Assert.All(result.Records,r=>Assert.Equal(id,r.ProjectId));Interlocked.Increment(ref calls);entered.SetResult();return release.Task;}
        var first=Publish(helper,true,Callback);await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal("inflight",await Publish(helper,true,Callback));Assert.Equal("upload_uncertain",await first);Assert.True(Running(helper));
        release.SetResult("pr_open");for(var i=0;i<100&&Running(helper);i++)await Task.Delay(5);
        Assert.False(Running(helper));Assert.Equal("held",await Publish(helper,true,Callback));Assert.Equal(1,calls);
    }
    [Fact] public async Task CancelledLifetimeCannotStartAnyPublication()
    {
        using var fixture=new Fixture();using var cancellation=new CancellationTokenSource();cancellation.Cancel();var calls=0;
        Assert.Equal("held",await Publish(fixture.New(),true,(_,__,___)=>{calls++;return Task.FromResult("pr_open");},cancellation.Token));Assert.Equal(0,calls);
    }
    [Fact] public async Task RunOnceRejectsMixedAndUnregisteredArgsWithoutCollectorOrTransport()
    {
        using var fixture=new Fixture();var helper=fixture.New();var method=Helper().GetMethod("RunOnceAsync");Assert.NotNull(method);
        foreach(var args in new[]{new[]{"--publish-records-once","Threads","--manual-control"},new[]{"--publish-records-once","unknown"}})
        {
            var task=(Task)method!.Invoke(helper,[args,CancellationToken.None])!;await task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.Equal(2,task.GetType().GetProperty("Result")!.GetValue(task));Assert.False(Running(helper));
        }
    }
    [Fact] public async Task LaterProjectSuccessCannotHideEarlierUncertainPublication()
    {
        using var fixture=new Fixture();
        using(var document=JsonDocument.Parse(File.ReadAllText(fixture.Registry)))
        {
            var first=document.RootElement.GetProperty("Registrations")[0].Clone();
            File.WriteAllText(fixture.Registry,JsonSerializer.Serialize(new{SchemaVersion=1,BoardRoot=fixture.Root,Registrations=new object[]{first,new{ProjectId="Control-Tower",RootPath=fixture.Root,OriginUrl="https://github.com/kimjae134679/project-operations-hub.git",RepositoryId=1362083625L,RepositoryFullName="kimjae134679/project-operations-hub",BaseBranch="main",SharedPrefix="04_COMMUNICATION/shared-records/Control-Tower",Enabled=true}}}));
        }
        var result=await Publish(fixture.New(),true,(project,_,__)=>Task.FromResult(project=="Threads"?"upload_uncertain":"pr_open"));
        Assert.Equal("upload_uncertain",result);
    }
    [Fact] public void ExplicitExistingThreadsReadOnlySourceMayBeOnCButOtherCRootsAreRejected()
    {
        var method=Helper().GetMethod("ValidateProjectSource",BindingFlags.Public|BindingFlags.Static);Assert.NotNull(method);
        Assert.Equal(true,method!.Invoke(null,["Threads",@"C:\KJ\Github\Threads"]));
        Assert.Equal(false,method.Invoke(null,["Threads",@"C:\other\Threads"]));
        Assert.Equal(false,method.Invoke(null,["PhoneLOL",@"C:\KJ\Github\Threads"]));
        Assert.Equal(false,method.Invoke(null,["Threads",@"C:\KJ\Github\Threads\..\other"]));
        Assert.Equal(false,method.Invoke(null,["Threads",@"D:\Desktop\Threads"]));
        Assert.Equal(true,method.Invoke(null,["Control-Tower",@"D:\owned\hub"]));
    }
    [Fact] public void OneShotReceiptRejectsLinkedExistingFileBeforeAtomicOverwrite()
    {
        var source=Source(Path.Combine("Services","RegisteredRecordPublishing.cs"));
        var guard=source.IndexOf("CheckPath(file,false)",StringComparison.Ordinal);
        var write=source.IndexOf("File.WriteAllText(temp",StringComparison.Ordinal);
        Assert.True(guard>=0&&guard<write);
    }
    private static string Source(string file)
    {
        var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir is not null&&!File.Exists(Path.Combine(dir.FullName,"ai-control-tower","AIControlTower.sln")))dir=dir.Parent;Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(dir!.FullName,"ai-control-tower","src","AIControlTower",file));
    }
    [Fact] public void HeadlessRoutePrecedesSettingsMutexAndEveryWindowPath()
    {
        var app=Source("App.xaml.cs");var begin=app.IndexOf("protected override void OnStartup",StringComparison.Ordinal);
        var route=app.IndexOf("TryStartRecordPublishing",begin,StringComparison.Ordinal);Assert.True(route>begin);
        foreach(var next in new[]{"StartupPolicy.TryCreate","StartRemoteSupervisor(e)","new Mutex","ControlTowerSettings.LoadReadOnly()","new MainWindow"})Assert.True(route<app.IndexOf(next,begin,StringComparison.Ordinal));
        Assert.Contains("_recordPublishLifetime?.Cancel()",app);
    }
    [Fact] public void CompletedManualCollectionSchedulesSeparateTrackedPublicationWithoutWaiting()
    {
        var vm=Source(Path.Combine("ViewModels","CommunicationViewModel.cs"));Assert.Contains("_recordPublicationTask",vm);Assert.Contains("StartRegisteredRecordPublishing(result)",vm);
        Assert.DoesNotContain("await StartRegisteredRecordPublishing",vm);Assert.Contains("_lifetime.Token",vm);Assert.Contains("RecordPublicationStatus",vm);
    }
    [Fact] public void HeadlessCollectorHasNoSettingsCentralGitOrGuiFallback()
    {
        var path=Path.Combine("Services","RegisteredRecordPublishing.cs");var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir is not null&&!File.Exists(Path.Combine(dir.FullName,"ai-control-tower","AIControlTower.sln")))dir=dir.Parent;
        Assert.NotNull(dir);Assert.True(File.Exists(Path.Combine(dir!.FullName,"ai-control-tower","src","AIControlTower",path)));
        var source=Source(path);Assert.Contains("CollectOnlyAsync",source);Assert.Contains("RunOnceAsync",source);
        foreach(var banned in new[]{"ControlTowerSettings.Load","SyncAsync(","new MainWindow","Process.Start","git push","AutoPublishCommunication"})Assert.DoesNotContain(banned,source);
    }
}
