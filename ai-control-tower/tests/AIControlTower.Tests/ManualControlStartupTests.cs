using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class ManualControlStartupTests
{
    private static StartupPolicy Manual()
    {
        Assert.True(StartupPolicy.TryCreate(["--manual-control"], out var policy));
        var property = typeof(StartupPolicy).GetProperty("IsManualControl"); Assert.NotNull(property);
        Assert.True((bool)property!.GetValue(policy)!); return policy;
    }
    private static bool Flag(object value,string name)
    { var p=value.GetType().GetProperty(name);Assert.NotNull(p);return (bool)p!.GetValue(value)!; }
    [Fact] public void ManualModeHasSeparateIdentityAndNoBroadMutationOrRecovery()
    {
        var policy=Manual();Assert.Equal("ManualControl",policy.InstanceIdentity);
        Assert.False(policy.AllowMutations);Assert.True(Flag(policy,"AllowRegisteredTasks"));Assert.True(Flag(policy,"AllowLiveQueries"));
        var count=0;policy.RecoverRuns(()=>count++);Assert.Equal(0,count);
        Assert.NotEqual(new StartupPolicy(false).ActivationEvent,policy.ActivationEvent);
        Assert.NotEqual(new StartupPolicy(true).ActivationEvent,policy.ActivationEvent);
    }
    [Theory]
    [InlineData("--local-view")][InlineData("--background")][InlineData("--remote-supervisor")][InlineData("--remote-control")][InlineData("--verify-ui")]
    public void ManualMixedModesFailBeforeOperationalStartup(string other)
    {Assert.False(StartupPolicy.TryCreate(["--manual-control",other],out _));}
    [Fact] public async Task ManualInitializationNeverCallsOperationalCallback()
    {
        var policy=Manual();var calls=new int[3];
        await policy.InitializeAsync(()=>{calls[0]++;return Task.CompletedTask;},()=>{calls[1]++;return Task.CompletedTask;},()=>{calls[2]++;return Task.CompletedTask;});
        Assert.Equal(new[]{1,1,0},calls);
    }
    private sealed class Handler : HttpMessageHandler
    {
        public readonly List<string> Requests=[];public string Stage="connected";public HttpStatusCode Code=HttpStatusCode.OK;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Requests.Add(request.Method+" "+request.RequestUri!.AbsolutePath);
            if(request.Method==HttpMethod.Post && Code==HttpStatusCode.OK)
            {
                var text=request.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult();
                Stage=text.Contains("pause")?"paused":text.Contains("stop")?"stopped":"connected";
            }
            var body=request.Method==HttpMethod.Get?JsonSerializer.Serialize(new{stage=Stage,deviceId="fixture",localReady=true,relayConnected=true,updatedAt=DateTimeOffset.UtcNow.ToString("O"),activeJobCount=0,jobs=Array.Empty<object>()}):"{\"accepted\":true}";
            return Task.FromResult(new HttpResponseMessage(Code){Content=new StringContent(body,Encoding.UTF8,"application/json")});
        }
    }
    private static (PcConnectionViewModel Vm,Handler Handler,string Root) Fixture()
    {
        var ctor=typeof(PcConnectionViewModel).GetConstructor([typeof(ProjectBridgeService),typeof(Func<string,string,string,Task>),typeof(bool),typeof(bool)]);
        Assert.NotNull(ctor);
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","manual-control-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root,"state"));File.WriteAllText(Path.Combine(root,"프로젝트연결.exe"),"fixture-not-executable");
        File.WriteAllText(Path.Combine(root,"state","local_endpoint.json"),JsonSerializer.Serialize(new{baseUrl="http://127.0.0.1:1/",token=new string('x',32)}));
        // Config is not required for the API-only route; no real operating file is ever touched.
        var handler=new Handler();var service=new ProjectBridgeService(root,new HttpClient(handler));
        return ((PcConnectionViewModel)ctor!.Invoke([service,null,true,true]),handler,root);
    }
    private static Task Maintain(PcConnectionViewModel vm,DateTimeOffset now)
    {var method=typeof(PcConnectionViewModel).GetMethod("MaintainConnectionAsync");Assert.NotNull(method);return (Task)method!.Invoke(vm,[now])!;}
    [Fact] public async Task ManualAutoStartGetsStatusAndDoesNotResumeAlreadyConnectedWorker()
    {
        var (vm,handler,_)=Fixture();using(vm)
        {await Maintain(vm,DateTimeOffset.UtcNow);Assert.True(vm.IsConnected);Assert.False(vm.IsReadOnly);Assert.Equal(new[]{"GET /v1/status"},handler.Requests);Assert.False(vm.InstallCommand.CanExecute(null));Assert.True(vm.PauseCommand.CanExecute(null));}
    }
    [Fact] public async Task PausedFreshWorkerResumesThroughAuthenticatedApiOnly()
    {
        var (vm,handler,_)=Fixture();using(vm)
        {handler.Stage="paused";await Maintain(vm,DateTimeOffset.UtcNow);Assert.Contains("POST /v1/control",handler.Requests);Assert.True(vm.IsConnected);}
    }
    [Fact] public async Task ExplicitUserPauseSuppressesAutomaticResume()
    {
        var (vm,handler,_)=Fixture();using(vm)
        {
            await Maintain(vm,DateTimeOffset.UtcNow);vm.PauseCommand.Execute(null);
            var until=DateTime.UtcNow.AddSeconds(3);while(!Flag(vm,"AutoReconnectSuppressed")&&DateTime.UtcNow<until)await Task.Delay(10);
            Assert.True(Flag(vm,"AutoReconnectSuppressed"));await Maintain(vm,DateTimeOffset.UtcNow.AddMinutes(1));
            Assert.Equal(1,handler.Requests.Count(x=>x=="POST /v1/control"));
        }
    }
    [Fact] public async Task PermissionDeniedStopsRetriesAndOfflineUsesBoundedBackoff()
    {
        var (vm,handler,_)=Fixture();using(vm)
        {
            handler.Code=HttpStatusCode.Forbidden;var now=DateTimeOffset.UtcNow;await Maintain(vm,now);
            var count=handler.Requests.Count;await Maintain(vm,now.AddMinutes(2));Assert.Equal(count,handler.Requests.Count);
            Assert.True(vm.HasError);Assert.Contains("권한",vm.DetailText);
        }
        var (offline,down,_)=Fixture();using(offline)
        {
            down.Code=HttpStatusCode.ServiceUnavailable;var now=DateTimeOffset.UtcNow;await Maintain(offline,now);
            var count=down.Requests.Count;await Maintain(offline,now.AddMilliseconds(100));Assert.Equal(count,down.Requests.Count);
            await Maintain(offline,now.AddMinutes(1));Assert.True(down.Requests.Count>count);Assert.DoesNotContain(down.Requests,x=>x.StartsWith("POST"));
        }
    }
    [Fact] public void ManualMainVmKeepsReadOnlySettingsButSeparatelyAllowsRegisteredTasks()
    {
        var constructor=typeof(MainViewModel).GetConstructor([typeof(ControlTowerSettings),typeof(bool),typeof(Action),typeof(StartupPolicy),typeof(ProjectBridgeService)]);
        Assert.NotNull(constructor);var policy=Manual();Exception? failure=null;
        var thread=new Thread(()=>
        {
            try
            {
                var (pc,handler,root)=Fixture();pc.Dispose();var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root,IsTemporary=true,TransientReadOnly=true,TransientDataDirectory=Path.Combine(root,"data")};
                using var vm=(MainViewModel)constructor!.Invoke([settings,false,new Action(()=>throw new Exception("recovery forbidden")),policy,null]);
                Assert.False(vm.CanMutate);Assert.True(Flag(vm,"CanRunRegisteredTasks"));Assert.True(Flag(vm,"CanReadLiveStatus"));
                Assert.True(settings.TransientReadOnly);Assert.True(vm.BlockOperation());Assert.Equal(0,handler.Requests.Count);
            }
            catch(Exception e){failure=e;}
        });thread.SetApartmentState(ApartmentState.STA);thread.Start();Assert.True(thread.Join(TimeSpan.FromSeconds(15)));
        if(failure is not null)System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
    [Fact] public void ManualDashboardUsesCachedStatusAndCannotBypassPermissionRetryStop()
    {
        Exception? failure=null;var thread=new Thread(()=>
        {
            try
            {
                var (unused,handler,root)=Fixture();using(unused)
                {
                    handler.Code=HttpStatusCode.Forbidden;
                    var constructor=typeof(MainViewModel).GetConstructor([typeof(ControlTowerSettings),typeof(bool),typeof(Action),typeof(StartupPolicy),typeof(ProjectBridgeService)]);Assert.NotNull(constructor);
                    using var service=new ProjectBridgeService(root,new HttpClient(handler));
                    var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root,IsTemporary=true,TransientReadOnly=true,TransientDataDirectory=Path.Combine(root,"data")};
                    using var vm=(MainViewModel)constructor!.Invoke([settings,false,new Action(()=>throw new Exception("recovery forbidden")),Manual(),service]);
                    Maintain(vm.PcConnection,DateTimeOffset.UtcNow).GetAwaiter().GetResult();Assert.Single(handler.Requests);
                    vm.RefreshWorkDashboardAsync().GetAwaiter().GetResult();vm.RefreshWorkDashboardAsync().GetAwaiter().GetResult();
                    Assert.Single(handler.Requests);Assert.True(Flag(vm.PcConnection,"AutoReconnectSuppressed"));
                }
            }
            catch(Exception e){failure=e;}
        });thread.SetApartmentState(ApartmentState.STA);thread.Start();Assert.True(thread.Join(TimeSpan.FromSeconds(20)));
        if(failure is not null)System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
    [Fact] public void ManualDashboardTickIsReadOnlyFiveSecondThrottledAfterMaintenance()
    {
        DirectoryInfo? source=new(AppContext.BaseDirectory);
        while(source is not null&&!File.Exists(Path.Combine(source.FullName,"ai-control-tower","src","AIControlTower","ViewModels","MainViewModel.cs")))source=source.Parent;
        Assert.NotNull(source);
        var text=File.ReadAllText(Path.Combine(source!.FullName,"ai-control-tower","src","AIControlTower","ViewModels","MainViewModel.cs"));
        var start=text.IndexOf("_pcLiveTimer.Tick +=",StringComparison.Ordinal);
        var timer=text[start..text.IndexOf("if (enablePolling",start,StringComparison.Ordinal)];
        Assert.Contains("_lastManualDashboardRefresh",timer);Assert.Contains("TimeSpan.FromSeconds(5)",timer);
        Assert.True(timer.IndexOf("MaintainConnectionAsync",StringComparison.Ordinal)<timer.IndexOf("RefreshWorkDashboardAsync",StringComparison.Ordinal));
        Assert.DoesNotContain("SyncCommunication",timer);Assert.DoesNotContain("RecoverRuns",timer);Assert.DoesNotContain("_providers",timer);
    }
    [Fact] public void ManualInitialAndPeriodicServerQueriesAreWiredWithoutOperationalInitialization()
    {
        DirectoryInfo? source=new(AppContext.BaseDirectory);
        while(source is not null&&!File.Exists(Path.Combine(source.FullName,"ai-control-tower","src","AIControlTower","ViewModels","MainViewModel.cs")))source=source.Parent;
        Assert.NotNull(source);
        var text=File.ReadAllText(Path.Combine(source!.FullName,"ai-control-tower","src","AIControlTower","ViewModels","MainViewModel.cs"));
        var start=text.IndexOf("public async Task InitializeManualAsync()",StringComparison.Ordinal);
        var end=text.IndexOf("public ObservableCollection<ProjectItem>",start,StringComparison.Ordinal);
        var init=text[start..end];
        Assert.Contains("RefreshRosterAsync(true)",init);Assert.Contains("RefreshServerAsync(true)",init);
        Assert.DoesNotContain("RecoverRuns",init);Assert.DoesNotContain("InitializeOperational",init);
        var timer=text[text.IndexOf("_serverLiveTimer.Tick +=",StringComparison.Ordinal)..text.IndexOf("_pcLiveTimer =",StringComparison.Ordinal)];
        Assert.Contains("IsManualControl",timer);Assert.Contains("RefreshServerAsync()",timer);
        var refreshAt=text.IndexOf("public async Task RefreshAsync()",StringComparison.Ordinal);
        var manualRefresh=text[text.IndexOf("if(IsManualControl)",refreshAt,StringComparison.Ordinal)..text.IndexOf("if(IsReadOnlyView)",refreshAt,StringComparison.Ordinal)];
        Assert.DoesNotContain("_providers",manualRefresh);
    }
}
