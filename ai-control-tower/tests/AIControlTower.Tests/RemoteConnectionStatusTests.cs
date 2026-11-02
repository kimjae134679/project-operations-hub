using System.Reflection;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;
public sealed class RemoteConnectionStatusTests : IDisposable
{
    private readonly string _root=Path.Combine(Path.GetTempPath(),"RemoteStatusTests-"+Guid.NewGuid().ToString("N"));
    private readonly DateTimeOffset _now=DateTimeOffset.Parse("2026-10-08T00:00:00Z");
    public RemoteConnectionStatusTests()=>Directory.CreateDirectory(_root);
    private object Read()
    {
        var type=typeof(ProjectBridgeService).Assembly.GetType("AIControlTower.Services.CommanderStatusService");Assert.NotNull(type);
        var service=Activator.CreateInstance(type!,[_root]);Assert.NotNull(service);
        return type!.GetMethod("Read")!.Invoke(service,[_now])!;
    }
    private static T Value<T>(object row,string key)=>(T)row.GetType().GetProperty(key)!.GetValue(row)!;
    private void Observation(DateTimeOffset observed,double? remaining=0,bool online=true,bool authenticated=true,string source="desktop_commander_connector")
        =>File.WriteAllText(Path.Combine(_root,"remote-commander-observation.json"),JsonSerializer.Serialize(new{schemaVersion=1,observedAtUtc=observed.ToString("O"),source,online,authenticated,remainingPercent=remaining}));
    [Fact] public void OnlineAndAuthenticatedWithZeroQuotaIsNotCommandReady()
    {Observation(_now);var row=Read();Assert.True(Value<bool>(row,"ObservationFresh"));Assert.Equal(0,Value<double?>(row,"RemainingPercent"));Assert.False(Value<bool>(row,"RemoteCallsReady"));Assert.Contains("0%",Value<string>(row,"DisplayText"));}
    [Fact] public void PositiveQuotaRequiresBothOnlineAndAuthentication()
    {Observation(_now,25);Assert.True(Value<bool>(Read(),"RemoteCallsReady"));Observation(_now,25,false);Assert.False(Value<bool>(Read(),"RemoteCallsReady"));Observation(_now,25,true,false);Assert.False(Value<bool>(Read(),"RemoteCallsReady"));}
    [Fact] public void StaleQuotaRetainsLastObservationButIsNeverCurrentReadiness()
    {Observation(_now.AddMinutes(-6));var row=Read();Assert.False(Value<bool>(row,"ObservationFresh"));Assert.False(Value<bool>(row,"RemoteCallsReady"));Assert.Equal(0,Value<double?>(row,"RemainingPercent"));Assert.Contains("마지막 확인",Value<string>(row,"DisplayText"));Assert.Contains("재확인",Value<string>(row,"DisplayText"));}
    [Fact] public void MissingQuotaNeverBecomesZero()
    {Observation(_now,null);Assert.Null(Value<double?>(Read(),"RemainingPercent"));Assert.Contains("미확인",Value<string>(Read(),"DisplayText"));}
    [Fact] public void MissingObservationCannotBorrowSupervisorForCloudOrQuota()
    {File.WriteAllText(Path.Combine(_root,"remote-supervisor-status.json"),JsonSerializer.Serialize(new{Enabled=true,Running=true,RootCount=1,UpdatedUtc=_now.ToString("O")}));var row=Read();Assert.True(Value<bool>(row,"SupervisorFresh"));Assert.Null(Value<double?>(row,"RemainingPercent"));Assert.False(Value<bool>(row,"RemoteCallsReady"));Assert.Contains("미확인",Value<string>(row,"DisplayText"));}
    [Theory][InlineData(-1)][InlineData(101)]
    public void OutOfRangeQuotaIsUnknown(double remaining)
    {Observation(_now,remaining);Assert.Null(Value<double?>(Read(),"RemainingPercent"));Assert.False(Value<bool>(Read(),"RemoteCallsReady"));}
    [Fact] public void FutureOrWrongSourceObservationIsRejected()
    {Observation(_now.AddMinutes(1),20);Assert.False(Value<bool>(Read(),"ObservationFresh"));Observation(_now,20,source:"untrusted");Assert.Null(Value<double?>(Read(),"RemainingPercent"));}
    [Fact] public void MalformedAndOversizedObservationIsUnknownWithoutException()
    {File.WriteAllText(Path.Combine(_root,"remote-commander-observation.json"),"invalid private text");Assert.Null(Value<double?>(Read(),"RemainingPercent"));File.WriteAllText(Path.Combine(_root,"remote-commander-observation.json"),new string('x',70000));Assert.Null(Value<double?>(Read(),"RemainingPercent"));}
    [Fact] public void StaleSupervisorCannotClaimLocalRecoveryAlive()
    {File.WriteAllText(Path.Combine(_root,"remote-supervisor-status.json"),JsonSerializer.Serialize(new{Enabled=true,Running=true,RootCount=1,UpdatedUtc=_now.AddMinutes(-2).ToString("O")}));Assert.False(Value<bool>(Read(),"SupervisorFresh"));}
    [Theory][InlineData("{\"schemaVersion\":\"1\",\"source\":\"desktop_commander_connector\"}")][InlineData("[]")]
    public void WrongObservationFieldTypesAreUnknown(string json)
    {File.WriteAllText(Path.Combine(_root,"remote-commander-observation.json"),json);Assert.Null(Value<double?>(Read(),"RemainingPercent"));}
    [Fact] public void WrongSupervisorFieldTypesCannotCrashStatusPolling()
    {File.WriteAllText(Path.Combine(_root,"remote-supervisor-status.json"),JsonSerializer.Serialize(new{Enabled=true,Running=true,RootCount="1",UpdatedUtc=_now.ToString("O")}));Assert.False(Value<bool>(Read(),"SupervisorFresh"));}
    private sealed class BridgeHandler(string conflict="3",string pending="2",bool relay=true,bool counts=true,bool stale=false) : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;
            var body=JsonSerializer.Serialize(new{stage="connected",deviceId="fixture",localReady=true,relayConnected=relay,updatedAt=DateTimeOffset.UtcNow.AddMinutes(stale?-2:0).ToString("O"),jobs=Array.Empty<object>()});
            if(counts)body=body[..^1]+",\"publicationConflictCount\":"+conflict+",\"pendingPublicationCount\":"+pending+"}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(body)});
        }
    }
    private ProjectBridgeService Bridge(BridgeHandler handler)
    {
        Directory.CreateDirectory(Path.Combine(_root,"state"));
        File.WriteAllText(Path.Combine(_root,"state","local_endpoint.json"),JsonSerializer.Serialize(new{baseUrl="http://127.0.0.1:1/",token=new string('x',32)}));
        File.WriteAllText(Path.Combine(_root,"프로젝트연결.exe"),"fixture-not-executed");
        return new ProjectBridgeService(_root,new HttpClient(handler));
    }
    private static int? Count(PcConnectionSnapshot row,string property)
    {var info=typeof(PcConnectionSnapshot).GetProperty(property);Assert.NotNull(info);return (int?)info!.GetValue(row);}
    [Theory][InlineData(true)][InlineData(false)]
    public async Task BothStatusReadersRetainBoundedConflictAndPendingCounts(bool apiOnly)
    {
        using var service=Bridge(new());var row=apiOnly?await service.CheckApiOnlyAsync(default):await service.CheckAsync(default);
        Assert.True(row.Connected);Assert.True(row.RelayConnected);Assert.Equal(3,Count(row,"PublicationConflictCount"));Assert.Equal(2,Count(row,"PendingPublicationCount"));Assert.Empty(row.Jobs);
    }
    [Theory][InlineData("-1")][InlineData("\"3\"")][InlineData("null")][InlineData("1.5")][InlineData("2147483648")][InlineData("true")][InlineData("{}")]
    public async Task MalformedPublicationCountsAreUnknownWithoutBreakingEitherStatusReader(string count)
    {
        using var service=Bridge(new(count,count));
        foreach(var row in new[]{await service.CheckApiOnlyAsync(default),await service.CheckAsync(default)})
        {Assert.True(row.Connected);Assert.Null(Count(row,"PublicationConflictCount"));Assert.Null(Count(row,"PendingPublicationCount"));}
    }
    [Theory][InlineData(false,false)][InlineData(true,true)]
    public async Task MissingOrStalePublicationCountsRemainUnknownInBothReaders(bool counts,bool stale)
    {
        using var service=Bridge(new(counts:counts,stale:stale));
        foreach(var row in new[]{await service.CheckApiOnlyAsync(default),await service.CheckAsync(default)})
        {Assert.Null(Count(row,"PublicationConflictCount"));Assert.Null(Count(row,"PendingPublicationCount"));}
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ConflictNoticeDoesNotClaimOldResultsCompletedOrDelivered(bool relay)
    {
        using var vm=new PcConnectionViewModel(Bridge(new(relay:relay)),null,true,true);await vm.PollAsync();
        Assert.True(vm.IsConnected);Assert.Contains("과거 결과 3건 보존·분리",vm.BridgeStatusText);Assert.Contains("새 요청 처리와 별개",vm.BridgeStatusText);Assert.Contains("결과 게시 대기 2건",vm.BridgeStatusText);Assert.DoesNotContain("완료",vm.BridgeStatusText);Assert.DoesNotContain("전달됨",vm.BridgeStatusText);Assert.Empty(vm.Jobs);
        Assert.Contains(relay?"원격 GitHub 중계: 연결됨":"원격 GitHub 중계: 연결 확인 필요",vm.BridgeStatusText);
    }
    [Fact] public async Task ReadOnlyDoesNotQueryOrInferConflictCount()
    {
        var handler=new BridgeHandler();using var vm=new PcConnectionViewModel(Bridge(handler),null,true,false);await vm.PollAsync();
        Assert.Equal(0,handler.Calls);Assert.Contains("미확인",vm.BridgeStatusText);Assert.DoesNotContain("보존·분리",vm.BridgeStatusText);Assert.False(vm.InstallCommand.CanExecute(null));
    }
    public void Dispose()=>Directory.Delete(_root,true);
}
