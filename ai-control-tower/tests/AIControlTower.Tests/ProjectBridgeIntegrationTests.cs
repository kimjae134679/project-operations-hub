using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;
public sealed class ProjectBridgeIntegrationTests : IDisposable
{
    private readonly string _home=Path.Combine(Path.GetTempPath(),"TowerBridgeTests-"+Guid.NewGuid().ToString("N"));
    private sealed class Handler(Func<string> body) : HttpMessageHandler
    {
        public int Calls; public string? Header; public Uri? Uri;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        { Calls++;Header=request.Headers.GetValues("X-ProjectBridge-Token").Single();Uri=request.RequestUri;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(body(),Encoding.UTF8,"application/json")}); }
    }
    public ProjectBridgeIntegrationTests()
    {
        Directory.CreateDirectory(Path.Combine(_home,"state"));File.WriteAllText(Path.Combine(_home,"프로젝트연결.exe"),"");
        File.WriteAllText(Path.Combine(_home,"config.json"),"{\"deviceId\":\"test-device\"}");
        File.WriteAllText(Path.Combine(_home,"state","local_endpoint.json"),"{\"baseUrl\":\"http://127.0.0.1:32345\",\"token\":\"test-local-only-non-secret-12345\"}");
    }
    [Theory]
    [InlineData("https://127.0.0.1:32345")]
    [InlineData("http://example.com:32345")]
    [InlineData("http://127.0.0.1.evil.example:32345")]
    [InlineData("http://secret@127.0.0.1:32345")]
    [InlineData("http://127.0.0.1:32345/api")]
    [InlineData("http://127.0.0.1:32345/?token=x")]
    public void LocalCredentialNeverTargetsRemoteHost(string endpoint)=>Assert.Throws<InvalidDataException>(()=>ProjectBridgeService.ValidateEndpoint(endpoint));
    private static string Status(string stage="connected",bool ready=true,int secondsAgo=0,string state="running")=>JsonSerializer.Serialize(new{stage,localReady=ready,relayConnected=false,deviceId="test-device",updatedAt=DateTimeOffset.UtcNow.AddSeconds(-secondsAgo).ToString("o"),parallelLimit=4,activeJobCount=state=="running"?1:0,jobs=new[]{new{id="job-one",title="파일 확인",projectId="Control-Tower",toolId="codex",state}}});
    [Fact] public async Task FreshAuthenticatedHeartbeatIsConnectedButRelaySeparate()
    {
        var handler=new Handler(()=>Status());using var service=new ProjectBridgeService(_home,new HttpClient(handler));var result=await service.CheckAsync(default);
        Assert.True(result.Connected);Assert.False(result.RelayConnected);Assert.Equal("test-device",result.Device);Assert.Equal(4,result.ParallelLimit);Assert.Equal("test-local-only-non-secret-12345",handler.Header);Assert.Equal("127.0.0.1",handler.Uri!.Host);
    }
    [Fact] public async Task StaleHeartbeatCannotShowConnected()
    {using var service=new ProjectBridgeService(_home,new HttpClient(new Handler(()=>Status(secondsAgo:60))));var s=await service.CheckAsync(default);Assert.False(s.Connected);Assert.Equal("stale",s.Stage);}
    [Fact] public async Task PausedHeartbeatCannotShowConnected()
    {using var service=new ProjectBridgeService(_home,new HttpClient(new Handler(()=>Status("paused",false))));Assert.False((await service.CheckAsync(default)).Connected);}
    [Theory][InlineData("[]")][InlineData("{\"parallelLimit\":\"4\",\"jobs\":[]}")]
    public async Task IncorrectStatusShapeDoesNotCrashTimer(string json)
    {using var service=new ProjectBridgeService(_home,new HttpClient(new Handler(()=>json)));Assert.False((await service.CheckAsync(default)).Connected);}
    [Fact] public async Task LegacyHeartbeatDoesNotClaimNewLocalApi()
    {
        File.Delete(Path.Combine(_home,"state","local_endpoint.json"));File.WriteAllText(Path.Combine(_home,"state","universal_status.json"),JsonSerializer.Serialize(new{stage="connected",device="test-device",privateChannelAvailable=true,updatedAt=DateTimeOffset.UtcNow.ToString("o")}));
        using var service=new ProjectBridgeService(_home);var s=await service.CheckAsync(default);Assert.False(s.Connected);Assert.True(s.RelayConnected);Assert.Equal("upgrade_required",s.Stage);
    }
    [Fact] public async Task JobStateUpdateRetainsSelectedRowIdentity()
    {
        var state="running";using var vm=new PcConnectionViewModel(new ProjectBridgeService(_home,new HttpClient(new Handler(()=>Status(state:state)))));
        await vm.PollAsync();var row=Assert.Single(vm.Jobs);Assert.True(row.IsRunning);state="completed";await vm.PollAsync();Assert.Same(row,Assert.Single(vm.Jobs));Assert.Equal("완료",row.StatusText);Assert.False(row.IsRunning);
    }
    [Fact] public void FailureAndInterruptedHaveDistinctStatus()
    {Assert.Equal("오류",new PcJobViewModel(new("x","작업","P","T","failed")).StatusText);Assert.True(new PcJobViewModel(new("y","작업","P","T","interrupted")).HasError);}
    [Fact] public void BundledInstallerContainsEveryManifestFileWithExactBytes()
    {
        var assembly=typeof(ProjectBridgeService).Assembly;
        using var manifest=assembly.GetManifestResourceStream("ProjectBridge/release_manifest.json");
        Assert.NotNull(manifest);
        using var document=JsonDocument.Parse(manifest);
        foreach(var file in document.RootElement.GetProperty("files").EnumerateArray())
        {
            var path=file.GetProperty("path").GetString()!;
            using var resource=assembly.GetManifestResourceStream("ProjectBridge/"+path);
            Assert.NotNull(resource);
            var hash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(resource)).ToLowerInvariant();
            Assert.Equal(file.GetProperty("sha256").GetString(),hash);
        }
    }
    [Theory]
    [InlineData("project-operations-hub","Control-Tower")]
    [InlineData("audiobook","Mushoku-Audiobook")]
    [InlineData("phonelol-current","PhoneLOL")]
    [InlineData("stock-planned","Investment-Lab")]
    [InlineData("housing-planned","ChungYack")]
    [InlineData("custom-project","custom-project")]
    public void CatalogAliasSharesTheCanonicalProjectWorkspace(string id,string expected)
        => Assert.Equal(expected,ProjectBridgeService.CanonicalProjectId(id));
    public void Dispose(){if(Directory.Exists(_home))Directory.Delete(_home,true);}
}
