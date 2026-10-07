using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class PcStatusDiagnosticsTests
{
    private const string PrivateSentinel="private-secret-token-D:\\private\\record";
    private sealed class Handler : HttpMessageHandler
    {
        public Func<CancellationToken,Task<HttpResponseMessage>> Reply = _=>Task.FromResult(Status());
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        { Assert.Equal(HttpMethod.Get,request.Method);Assert.Equal("/v1/status",request.RequestUri!.AbsolutePath);Calls++;return Reply(ct); }
    }
    private static HttpResponseMessage Status(string stage="connected",DateTimeOffset? updated=null,bool ready=true) =>
        new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new{stage,deviceId=PrivateSentinel,localReady=ready,relayConnected=true,updatedAt=(updated??DateTimeOffset.UtcNow).ToString("O"),jobs=Array.Empty<object>()}),Encoding.UTF8,"application/json")};
    private static (ProjectBridgeService Service,Handler Handler,string Root) ServiceFixture()
    {
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","pc-diagnostics-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root,"state"));
        File.WriteAllText(Path.Combine(root,"state","local_endpoint.json"),JsonSerializer.Serialize(new{baseUrl="http://127.0.0.1:1/",token=new string('x',32)}));
        var handler=new Handler();return(new ProjectBridgeService(root,new HttpClient(handler)),handler,root);
    }
    private static object Property(object value,string name)
    {var p=value.GetType().GetProperty(name);Assert.NotNull(p);return p!.GetValue(value)!;}
    private static object Diagnostics(PcConnectionViewModel vm)=>Property(vm,"Diagnostics");
    private static JsonElement Json(object value)=>JsonSerializer.SerializeToElement(value);
    private static string Reason(PcConnectionSnapshot snapshot)=>(string)Property(snapshot,"StatusReasonCode");
    private static PcConnectionViewModel View(ProjectBridgeService service)=>new(service,null,true,true);

    [Theory]
    [InlineData(403,"permission_denied")][InlineData(503,"http_error")]
    public async Task HttpFailuresGetAllowlistedReasonWithoutRawBody(int code,string expected)
    {
        var (service,handler,_)=ServiceFixture();using(service)
        {
            handler.Reply=_=>Task.FromResult(new HttpResponseMessage((HttpStatusCode)code){Content=new StringContent(PrivateSentinel)});
            var snapshot=await service.CheckApiOnlyAsync(default);Assert.False(snapshot.Connected);Assert.Equal(expected,Reason(snapshot));
        }
    }
    [Fact] public async Task TimeoutReasonIsSeparateFromTransportException()
    {
        var (service,handler,_)=ServiceFixture();using(service)
        {handler.Reply=_=>throw new TaskCanceledException(PrivateSentinel);var value=await service.CheckApiOnlyAsync(default);Assert.Equal("timeout",Reason(value));Assert.Equal("offline",value.Stage);}
    }
    [Fact] public async Task InvalidJsonReasonIsSeparateFromHttpFailure()
    {
        var (service,handler,_)=ServiceFixture();using(service)
        {handler.Reply=_=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(PrivateSentinel)});var value=await service.CheckApiOnlyAsync(default);Assert.Equal("invalid_json",Reason(value));}
    }
    [Fact] public async Task MissingOwnedEndpointIsMetadataFailureWithoutAnyHttpCall()
    {
        var (service,handler,root)=ServiceFixture();using(service)
        {File.Delete(Path.Combine(root,"state","local_endpoint.json"));var value=await service.CheckApiOnlyAsync(default);Assert.Equal("endpoint_unavailable",Reason(value));Assert.Equal(0,handler.Calls);}
    }
    [Fact] public async Task StaleAndInvalidStatusTimestampsAreDistinguishedWithoutThresholdChange()
    {
        var (service,handler,_)=ServiceFixture();using(service)
        {
            handler.Reply=_=>Task.FromResult(Status(updated:DateTimeOffset.UtcNow.AddMinutes(-2)));
            var stale=await service.CheckApiOnlyAsync(default);Assert.Equal("stale_status",Reason(stale));Assert.Equal("stale",stale.Stage);
            handler.Reply=_=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"updatedAt\":\"not-a-date\",\"localReady\":true}")});
            var invalid=await service.CheckApiOnlyAsync(default);Assert.Equal("invalid_status_timestamp",Reason(invalid));Assert.False(invalid.Connected);
        }
    }
    [Fact] public async Task SuccessPublishesTimingAndLastAuthenticatedSuccess()
    {
        var (service,handler,_)=ServiceFixture();using var vm=View(service);
        await vm.PollAsync();var d=Json(Diagnostics(vm));
        Assert.Equal("connected",d.GetProperty("Stage").GetString());Assert.Equal("status_confirmed",d.GetProperty("ReasonCode").GetString());
        Assert.False(d.GetProperty("InFlight").GetBoolean());Assert.True(d.GetProperty("ElapsedMilliseconds").GetInt64()>=0);
        var start=d.GetProperty("AttemptStartedAtUtc").GetDateTimeOffset();var finish=d.GetProperty("AttemptCompletedAtUtc").GetDateTimeOffset();
        Assert.True(finish>=start);Assert.Equal(finish,d.GetProperty("LastSuccessAtUtc").GetDateTimeOffset());Assert.Equal(1,handler.Calls);
    }
    [Fact] public async Task InFlightExistsEvenWhenNoUserOperationIsBusy()
    {
        var (service,handler,_)=ServiceFixture();using var vm=View(service);
        var completion=new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        handler.Reply=_=>completion.Task;var pending=vm.PollAsync();
        try
        {
            var during=Json(Diagnostics(vm));Assert.True(during.GetProperty("InFlight").GetBoolean());Assert.False(vm.IsBusy);
            Assert.Equal(JsonValueKind.Null,during.GetProperty("AttemptCompletedAtUtc").ValueKind);
            await vm.PollAsync();Assert.Equal(1,handler.Calls);
        }
        finally{completion.TrySetResult(Status());await pending;}
        Assert.False(Json(Diagnostics(vm)).GetProperty("InFlight").GetBoolean());
    }
    [Fact] public async Task FailurePreservesLastSuccessAndExportsSameBoundedRetryState()
    {
        var (service,handler,_)=ServiceFixture();using var vm=View(service);
        await vm.MaintainConnectionAsync(DateTimeOffset.UtcNow);var first=Json(Diagnostics(vm)).GetProperty("LastSuccessAtUtc").GetDateTimeOffset();
        handler.Reply=_=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        await vm.MaintainConnectionAsync(DateTimeOffset.UtcNow.AddMinutes(1));var d=Json(Diagnostics(vm));
        Assert.False(vm.IsConnected);Assert.Equal("http_error",d.GetProperty("ReasonCode").GetString());
        Assert.Equal(first,d.GetProperty("LastSuccessAtUtc").GetDateTimeOffset());Assert.Equal(vm.NextRetryAt,d.GetProperty("NextRetryAtUtc").GetDateTimeOffset());
    }
    [Fact] public async Task PermissionDenialKeepsRetrySuppressedWithoutExtraPolls()
    {
        var (service,handler,_)=ServiceFixture();using var vm=View(service);
        handler.Reply=_=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden));
        await vm.MaintainConnectionAsync(DateTimeOffset.UtcNow);var count=handler.Calls;
        await vm.MaintainConnectionAsync(DateTimeOffset.UtcNow.AddHours(1));
        Assert.True(vm.AutoReconnectSuppressed);Assert.Equal(count,handler.Calls);Assert.Equal("permission_denied",Json(Diagnostics(vm)).GetProperty("ReasonCode").GetString());
    }
    [Fact] public async Task RuntimeMetadataHasExactPrivateSafeFieldAllowlist()
    {
        var (service,handler,_)=ServiceFixture();using var vm=View(service);
        handler.Reply=_=>Task.FromResult(Status(stage:PrivateSentinel));await vm.PollAsync();var d=Json(Diagnostics(vm));
        Assert.Equal("unknown",d.GetProperty("Stage").GetString());
        Assert.Equal(new[]{"AttemptCompletedAtUtc","AttemptStartedAtUtc","ElapsedMilliseconds","InFlight","LastSuccessAtUtc","NextRetryAtUtc","ReasonCode","Stage"},d.EnumerateObject().Select(x=>x.Name).Order().ToArray());
        var serialized=d.GetRawText();Assert.DoesNotContain(PrivateSentinel,serialized);Assert.DoesNotContain("127.0.0.1",serialized);Assert.DoesNotContain(new string('x',32),serialized);
    }
    [Fact] public void ActualRuntimeWriterIncludesOnlySafeDiagnosticsObject()
    {
        var source=File.ReadAllText(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\source\ai-control-tower\src\AIControlTower\MainWindow.xaml.cs");
        var section=source[source.IndexOf("private void SaveManualRuntimeStatus(",StringComparison.Ordinal)..];section=section[..section.IndexOf("public void PrepareSessionEnding",StringComparison.Ordinal)];
        Assert.Contains("pcStatus=_viewModel.PcConnection.Diagnostics",section);
        Assert.DoesNotContain("DetailText",section);Assert.DoesNotContain("Snapshot.Detail",section);Assert.DoesNotContain("local_endpoint",section);
    }
}