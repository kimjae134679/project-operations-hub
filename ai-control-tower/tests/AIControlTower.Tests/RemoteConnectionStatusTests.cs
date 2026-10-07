using System.Reflection;
using System.Text.Json;
using AIControlTower.Services;

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
    public void Dispose()=>Directory.Delete(_root,true);
}
