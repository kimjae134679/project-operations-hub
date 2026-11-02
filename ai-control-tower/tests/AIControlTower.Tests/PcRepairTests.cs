using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Diagnostics;
using System.Text.Json;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class PcRepairTests : IDisposable
{
    private readonly string _root=Path.Combine(Path.GetTempPath(),"PcRepairTests-"+Guid.NewGuid().ToString("N"));
    private sealed class Handler : HttpMessageHandler
    {
        public int Active;public bool Ready=true;public HttpStatusCode Code=HttpStatusCode.OK;
        public Exception? Failure;
        public readonly List<string> Requests=[];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {Requests.Add(request.Method+" "+request.RequestUri!.AbsolutePath);if(Failure is not null)throw Failure;return Task.FromResult(new HttpResponseMessage(Code){Content=new StringContent(JsonSerializer.Serialize(new{stage="connected",deviceId="fixture",localReady=Ready,relayConnected=false,updatedAt=DateTimeOffset.UtcNow.ToString("O"),activeJobCount=Active,jobs=Array.Empty<object>(),accepted=true}))});}
    }
    public PcRepairTests(){Directory.CreateDirectory(Path.Combine(_root,"state"));File.WriteAllText(Path.Combine(_root,"state","local_endpoint.json"),JsonSerializer.Serialize(new{baseUrl="http://127.0.0.1:1/",token=new string('x',32)}));}
    private PcConnectionViewModel Create(Handler handler,Func<Task<bool>> confirm,Func<CancellationToken,Task> install,bool readOnly=true,bool manual=true)
    {
        var ctor=typeof(PcConnectionViewModel).GetConstructor([typeof(ProjectBridgeService),typeof(Func<string,string,string,Task>),typeof(bool),typeof(bool),typeof(Func<Task<bool>>),typeof(Func<CancellationToken,Task>)]);
        Assert.NotNull(ctor);
        return (PcConnectionViewModel)ctor!.Invoke([new ProjectBridgeService(_root,new HttpClient(handler)),null,readOnly,manual,confirm,install]);
    }
    private static Task Install(PcConnectionViewModel vm){var method=typeof(PcConnectionViewModel).GetMethod("InstallRepairAsync");Assert.NotNull(method);return (Task)method!.Invoke(vm,[])!;}
    [Fact] public void ManualInstallCommandIsEnabledWithoutBroadMutations()
    {using var vm=new PcConnectionViewModel(new ProjectBridgeService(_root,new HttpClient(new Handler())),null,true,true);Assert.False(vm.IsReadOnly);Assert.True(vm.InstallCommand.CanExecute(null));Assert.False(new StartupPolicy(false){IsManualControl=true}.AllowMutations);}
    [Fact] public async Task ConfirmationCancellationNeverInvokesInstaller()
    {var calls=0;using var vm=Create(new(),()=>Task.FromResult(false),_=>{calls++;return Task.CompletedTask;});await Install(vm);Assert.Equal(0,calls);Assert.Contains("취소",vm.DetailText);}
    [Fact] public async Task ConfirmedInstallRunsExactlyOnceAndReportsConnectedResult()
    {var calls=0;using var vm=Create(new(),()=>Task.FromResult(true),_=>{calls++;return Task.CompletedTask;});await Install(vm);Assert.Equal(1,calls);Assert.True(vm.IsConnected);Assert.Contains("완료",vm.DetailText);}
    [Fact] public async Task ActiveRemoteWorkBlocksInstallationBeforeConfirmation()
    {var confirms=0;var installs=0;using var vm=Create(new(){Active=1},()=>{confirms++;return Task.FromResult(true);},_=>{installs++;return Task.CompletedTask;});await Install(vm);Assert.Equal(0,confirms);Assert.Equal(0,installs);Assert.Contains("진행 중",vm.DetailText);}
    [Fact] public async Task InstallationFailureIsVisibleAndDoesNotClaimSuccess()
    {using var vm=Create(new(),()=>Task.FromResult(true),_=>Task.FromException(new IOException("config_not_writable_before_install")));await Install(vm);Assert.True(vm.HasError);Assert.Contains("config_not_writable",vm.DetailText);Assert.DoesNotContain("설치·복구 완료",vm.DetailText);}
    [Fact] public async Task ReadOnlyRejectsDirectInstallationAsWellAsButton()
    {var calls=0;using var vm=Create(new(),()=>Task.FromResult(true),_=>{calls++;return Task.CompletedTask;},true,false);Assert.False(vm.InstallCommand.CanExecute(null));await Install(vm);Assert.Equal(0,calls);}
    [Fact] public async Task AutomaticMaintenanceDoesNotInstall()
    {var calls=0;using var vm=Create(new(),()=>Task.FromResult(true),_=>{calls++;return Task.CompletedTask;});await vm.MaintainConnectionAsync(DateTimeOffset.UtcNow);Assert.Equal(0,calls);}
    [Fact] public async Task InstallationCountsAsOwnedMutationUntilInstallerCompletes()
    {var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);using var vm=Create(new(),()=>Task.FromResult(true),async _=>{entered.SetResult();await release.Task;});var task=Install(vm);await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));Assert.True(vm.OwnedMutationInFlight);Assert.False(vm.OwnedActivitiesIdle);release.SetResult();await task;Assert.False(vm.OwnedMutationInFlight);}
    private ProjectBridgeService RecoveryService(Handler handler,Action<ProcessStartInfo> start)
    {
        var ctor=typeof(ProjectBridgeService).GetConstructor([typeof(string),typeof(HttpClient),typeof(Action<ProcessStartInfo>)]);Assert.NotNull(ctor);
        return (ProjectBridgeService)ctor!.Invoke([_root,new HttpClient(handler),start]);
    }
    private static Task Resume(ProjectBridgeService service){var method=typeof(ProjectBridgeService).GetMethod("ResumeExistingAsync");Assert.NotNull(method);return (Task)method!.Invoke(service,[CancellationToken.None])!;}
    [Fact] public async Task ExplicitMissingEndpointResumeUsesOwnedHiddenLauncher()
    {
        File.Delete(Path.Combine(_root,"state","local_endpoint.json"));File.WriteAllText(Path.Combine(_root,"프로젝트연결.exe"),"fixture");ProcessStartInfo? launched=null;
        using var service=RecoveryService(new(),info=>launched=info);await Resume(service);
        Assert.NotNull(launched);Assert.Equal(Path.Combine(_root,"프로젝트연결.exe"),launched!.FileName);Assert.Equal(new[]{"--resume","--background"},launched.ArgumentList);Assert.False(launched.UseShellExecute);Assert.True(launched.CreateNoWindow);Assert.Equal(ProcessWindowStyle.Hidden,launched.WindowStyle);
    }
    [Theory][InlineData(HttpStatusCode.Forbidden)][InlineData(HttpStatusCode.Unauthorized)]
    public async Task PermissionDeniedNeverFallsBackToNativeResume(HttpStatusCode code)
    {var calls=0;using var service=RecoveryService(new(){Code=code},_=>calls++);await Assert.ThrowsAsync<HttpRequestException>(()=>Resume(service));Assert.Equal(0,calls);}
    [Fact] public async Task MalformedEndpointCannotTriggerNativeResume()
    {File.WriteAllText(Path.Combine(_root,"state","local_endpoint.json"),"{\"baseUrl\":\"http://example.com\",\"token\":\"invalid\"}");var calls=0;using var service=RecoveryService(new(),_=>calls++);await Assert.ThrowsAsync<InvalidDataException>(()=>Resume(service));Assert.Equal(0,calls);}
    [Fact] public async Task MissingCredentialInPresentEndpointNeverTriggersNativeResume()
    {File.WriteAllText(Path.Combine(_root,"state","local_endpoint.json"),"{\"baseUrl\":\"http://127.0.0.1:1/\"}");var calls=0;using var service=RecoveryService(new(),_=>calls++);await Assert.ThrowsAsync<InvalidDataException>(()=>Resume(service));Assert.Equal(0,calls);}
    [Fact] public async Task UnrelatedMissingFileWithPresentEndpointNeverTriggersNativeResume()
    {File.WriteAllText(Path.Combine(_root,"프로젝트연결.exe"),"fixture");var calls=0;using var service=RecoveryService(new(){Failure=new FileNotFoundException("credential unavailable")},_=>calls++);await Assert.ThrowsAsync<FileNotFoundException>(()=>Resume(service));Assert.Equal(0,calls);}
    [Fact] public async Task AutomaticOfflineMaintenanceNeverStartsNativeLauncher()
    {File.Delete(Path.Combine(_root,"state","local_endpoint.json"));var calls=0;using var vm=new PcConnectionViewModel(RecoveryService(new(),_=>calls++),null,true,true);await vm.MaintainConnectionAsync(DateTimeOffset.UtcNow);Assert.Equal(0,calls);}
    [Fact] public async Task ManualConnectExplicitlyResumesMissingWorker()
    {
        File.Delete(Path.Combine(_root,"state","local_endpoint.json"));File.WriteAllText(Path.Combine(_root,"프로젝트연결.exe"),"fixture");var started=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var vm=new PcConnectionViewModel(RecoveryService(new(),_=>started.SetResult()),null,true,true);Assert.True(vm.ConnectCommand.CanExecute(null));vm.ConnectCommand.Execute(null);await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
    }
    [Fact] public async Task ExhaustedCommanderQuotaStillSubmitsActualProjectBridgeJob()
    {
        File.WriteAllText(Path.Combine(_root,"remote-commander-observation.json"),JsonSerializer.Serialize(new{schemaVersion=1,source="desktop_commander_connector",observedAtUtc=DateTimeOffset.UtcNow.ToString("O"),online=true,authenticated=true,remainingPercent=0}));
        var handler=new Handler();using var vm=new PcConnectionViewModel(new ProjectBridgeService(_root,new HttpClient(handler)),null,true,true,()=>Task.FromResult(false),_=>Task.CompletedTask,new CommanderStatusService(_root));
        await vm.PollAsync();Assert.Contains("0%",vm.CommanderStatusText);Assert.Contains("사용 가능",vm.BridgeStatusText);Assert.Contains("ProjectBridge",vm.PrimaryPathText);
        var id=await vm.SubmitAsync("Control-Tower","fixture","capabilities",new{});Assert.StartsWith("tower-",id);Assert.Contains("POST /v1/jobs",handler.Requests);
    }
    [Fact] public async Task MissingBridgeCannotShowWorkingPathEvenWithCommanderOnline()
    {
        File.WriteAllText(Path.Combine(_root,"remote-commander-observation.json"),JsonSerializer.Serialize(new{schemaVersion=1,source="desktop_commander_connector",observedAtUtc=DateTimeOffset.UtcNow.ToString("O"),online=true,authenticated=true,remainingPercent=0}));
        using var vm=new PcConnectionViewModel(new ProjectBridgeService(_root,new HttpClient(new Handler(){Ready=false})),null,true,true,()=>Task.FromResult(false),_=>Task.CompletedTask,new CommanderStatusService(_root));
        await vm.PollAsync();Assert.False(vm.IsConnected);Assert.Contains("미확인",vm.PrimaryPathText);await Assert.ThrowsAsync<InvalidOperationException>(()=>vm.SubmitAsync("Control-Tower","fixture","capabilities",new{}));
    }
    [Theory][InlineData("private-output config_not_writable_before_install","config_not_writable_before_install")][InlineData("private-output Release file hash mismatch","release_validation_failed")][InlineData("private-output config_changed_since_staging","config_changed")][InlineData("private-output unexpected secret token","installation_failed")]
    public void RealInstallerFailureUsesOnlyBoundedReasonNotRawOutput(string output,string code)
    {var method=typeof(ProjectBridgeService).GetMethod("InstallationFailureMessage");Assert.NotNull(method);var message=(string)method!.Invoke(null,[output])!;Assert.Contains(code,message);Assert.DoesNotContain("private-output",message);Assert.DoesNotContain("secret token",message);}
    public void Dispose(){Directory.Delete(_root,true);}
}
