using System.Reflection;
using System.Text.Json;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

// Owned JSON fixtures only. No producer, model, audio, Window or global settings.
public sealed class ProjectProgressTests : IDisposable
{
    private readonly string _root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","project-progress-"+Guid.NewGuid().ToString("N"));
    private readonly DateTimeOffset _now=DateTimeOffset.Parse("2026-10-08T02:00:00Z");
    private const string Edition="production_fixture";
    private string Run=>Path.Combine(_root,"output",Edition);
    public ProjectProgressTests(){Directory.CreateDirectory(Run);Seed();}
    private void Seed(int completed=2,int total=4,double? updated=null,string stage="generating")
    {
        File.WriteAllText(Path.Combine(_root,"output","production_active.json"),JsonSerializer.Serialize(new{edition=Edition,run_relative="output/"+Edition}));
        File.WriteAllText(Path.Combine(Run,"status.json"),JsonSerializer.Serialize(new{edition=Edition,total,completed,current=147,stage,updated_at=updated??(_now.AddSeconds(-10).ToUnixTimeMilliseconds()/1000.0),chapters=new Dictionary<string,object>{{"144",new{status="ready"}},{"145",new{status="ready"}},{"146",new{status="needs_review"}},{"147",new{status="running"}}}}));
        File.WriteAllText(Path.Combine(Run,"worker_progress.json"),JsonSerializer.Serialize(new{chapter=147,id="147-0059",updated=_now.AddSeconds(-3).ToUnixTimeMilliseconds()/1000.0}));
    }
    private object Read(string id="audiobook",string? path=null,bool registered=true,DateTimeOffset? now=null)
    {
        var type=typeof(ProjectBridgeService).Assembly.GetType("AIControlTower.Services.ProjectProgressService");Assert.NotNull(type);
        var catalog=new CatalogDefinition{Projects=registered?[new CatalogProject{Id="audiobook",Path=_root}]:[]};
        var service=Activator.CreateInstance(type!);Assert.NotNull(service);
        return type!.GetMethod("Read")!.Invoke(service,[new ProjectItem{Id=id,Path=path??_root,Name="オーディオ",DisplayName="오디오북"},catalog,now??_now])!;
    }
    private static T Value<T>(object row,string property){var p=row.GetType().GetProperty(property);Assert.NotNull(p);return (T)p!.GetValue(row)!;}
    private void Unknown()=>Assert.False(Value<bool>(Read(),"IsKnown"));
    [Fact] public void RegisteredBatchUsesReadyCountAndOriginalUtcTimesNotWholeProjectCompletion()
    {
        var row=Read();Assert.True(Value<bool>(row,"IsKnown"));Assert.True(Value<bool>(row,"IsFresh"));Assert.Equal(50,Value<double?>(row,"Percent"));Assert.Equal(2,Value<int?>(row,"Completed"));Assert.Equal(4,Value<int?>(row,"Total"));Assert.Equal(1,Value<int?>(row,"Held"));
        Assert.Equal(_now.AddSeconds(-10),Value<DateTimeOffset?>(row,"SourceUpdatedAtUtc"));Assert.Equal(_now,Value<DateTimeOffset>(row,"ObservedAtUtc"));Assert.Contains("이번 제작 범위",Value<string>(row,"ScopeText"));Assert.Contains("미확인",Value<string>(row,"OverallText"));
    }
    [Fact] public void StaleSourceRetainsLastRatioButFreshWorkerCannotRebrandItCurrent()
    {
        Seed(updated:_now.AddHours(-3).ToUnixTimeSeconds());var row=Read();Assert.False(Value<bool>(row,"IsFresh"));Assert.Equal(50,Value<double?>(row,"Percent"));Assert.Contains("마지막 기록",Value<string>(row,"Headline"));Assert.Contains("현재 실행 확인 필요",Value<string>(row,"FreshnessText"));Assert.Equal("147-0059",Value<string>(row,"WorkerId"));
    }
    [Theory][InlineData("unknown")][InlineData("Audiobook")]
    public void NameOrAliasDoesNotRegisterAProgressSource(string id)=>Assert.False(Value<bool>(Read(id),"IsKnown"));
    [Fact] public void UnregisteredOrMismatchedRootDoesNotReadTheAudiobookSource()
    {Assert.False(Value<bool>(Read(registered:false),"IsKnown"));Assert.False(Value<bool>(Read(path:Path.GetDirectoryName(_root)),"IsKnown"));}
    [Theory][InlineData(3,4)][InlineData(2,0)][InlineData(5,4)][InlineData(-1,4)]
    public void InvalidOrInconsistentCountsDoNotProducePercentage(int completed,int total){Seed(completed,total);Unknown();}
    [Theory][InlineData(-1)][InlineData(999999999999)]
    public void InvalidEpochIsNotConvertedToCurrentStatus(double timestamp){Seed(updated:timestamp);Unknown();}
    [Fact] public void FutureTimestampIsRejectedAndObservationNeverReplacesSourceTime()
    {Seed(updated:_now.AddHours(1).ToUnixTimeSeconds());Unknown();}
    [Theory][InlineData("../../private")][InlineData("output/production_fixture/../private")][InlineData("C:/private")]
    public void PointerTraversalOrArbitraryPathIsHeld(string path)
    {File.WriteAllText(Path.Combine(_root,"output","production_active.json"),JsonSerializer.Serialize(new{edition=Edition,run_relative=path}));Unknown();}
    [Theory][InlineData("[]")][InlineData("{\"completed\":\"2\"}")][InlineData("private malformed text")]
    public void WrongTypeOrCorruptSourceIsUnknownWithoutRawError(string json)
    {File.WriteAllText(Path.Combine(Run,"status.json"),json);var row=Read();Assert.False(Value<bool>(row,"IsKnown"));Assert.DoesNotContain("private",Value<string>(row,"Headline"));}
    [Fact] public void MissingAndOversizedSourcesAreUnknownWithoutMutation()
    {File.Delete(Path.Combine(Run,"status.json"));Unknown();File.WriteAllText(Path.Combine(Run,"status.json"),new string('x',1048577));Unknown();}
    [Fact] public void EditionMismatchCannotMixOldAndNewCampaigns()
    {var path=Path.Combine(Run,"status.json");File.WriteAllText(path,File.ReadAllText(path).Replace(Edition,"production_old",StringComparison.Ordinal));Unknown();}
    [Fact] public void WrongWorkerChapterIsNotAttachedToTheSelectedChapter()
    {File.WriteAllText(Path.Combine(Run,"worker_progress.json"),JsonSerializer.Serialize(new{chapter=999,id="999-0001",updated=_now.ToUnixTimeSeconds()}));Assert.Equal("",Value<string>(Read(),"WorkerId"));}
    [Fact] public void ReadDoesNotCreateOrModifyAnyFixtureFile()
    {
        var before=Directory.GetFiles(_root,"*",SearchOption.AllDirectories).ToDictionary(p=>p,p=>File.ReadAllBytes(p));_ =Read();
        Assert.Equal(before.Keys.Order(),Directory.GetFiles(_root,"*",SearchOption.AllDirectories).Order());foreach(var p in before.Keys)Assert.Equal(before[p],File.ReadAllBytes(p));
    }
    [Fact] public void OffsetReaderClockProducesSameUtcContract()
    {var row=Read(now:_now.ToOffset(TimeSpan.FromHours(9)));Assert.Equal(TimeSpan.Zero,Value<DateTimeOffset>(row,"ObservedAtUtc").Offset);Assert.Equal(_now.AddSeconds(-10),Value<DateTimeOffset?>(row,"SourceUpdatedAtUtc"));}
    [Fact] public void DuplicateJsonFieldsCannotOverrideCountsOrEdition()
    {
        var path=Path.Combine(Run,"status.json");File.WriteAllText(path,File.ReadAllText(path).Replace("\"total\":4","\"total\":4,\"total\":4",StringComparison.Ordinal));Unknown();
        Seed();path=Path.Combine(_root,"output","production_active.json");File.WriteAllText(path,File.ReadAllText(path).Replace("\"edition\":","\"edition\":\"production_fixture\",\"edition\":",StringComparison.Ordinal));Unknown();
    }
    [Fact] public void ReparseStatusCannotReadAFileOutsideTheRegisteredRoot()
    {
        var status=Path.Combine(Run,"status.json");var target=Path.Combine(_root,"original-fixture.json");File.Move(status,target);File.CreateSymbolicLink(status,target);
        try{Unknown();Assert.True(File.Exists(target));}finally{File.Delete(status);}
    }
    [Fact] public void ReparseDirectoryInTheCanonicalPointerPathIsHeld()
    {
        var output=Path.Combine(_root,"output");var target=Path.Combine(_root,"original-fixture");Directory.Move(output,target);Directory.CreateSymbolicLink(output,target);
        try{Unknown();Assert.True(File.Exists(Path.Combine(target,Edition,"status.json")));}finally{Directory.Delete(output);}
    }
    [Fact] public void ClockSkewAndMalformedWorkerDoNotInventFreshBatchOrInternalEta()
    {
        Seed(updated:_now.AddSeconds(30).ToUnixTimeSeconds());File.WriteAllText(Path.Combine(Run,"worker_progress.json"),"{\"updated\":\"now\"}");
        var row=Read();Assert.True(Value<bool>(row,"IsKnown"));Assert.False(Value<bool>(row,"IsFresh"));Assert.Equal("",Value<string>(row,"WorkerId"));Assert.Contains("미확인",Value<string>(row,"WorkerText"));
    }
    public void Dispose()=>Directory.Delete(_root,true);
}
