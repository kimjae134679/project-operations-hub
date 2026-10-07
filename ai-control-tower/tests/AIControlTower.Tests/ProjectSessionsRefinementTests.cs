using System.Collections;
using System.Reflection;
using System.Text.Json;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

// D-drive owned metadata fixtures only; no live app, process, authentication or private AI bodies.
public sealed class ProjectSessionsRefinementTests
{
    private static PropertyInfo Property(Type type,string name) { var p=type.GetProperty(name);Assert.NotNull(p);return p!; }
    private static string Value(object value,string name)=>Property(value.GetType(),name).GetValue(value)?.ToString()??"";
    private static MethodInfo Method(Type type,string name) { var m=type.GetMethod(name);Assert.NotNull(m);return m!; }
    private static IReadOnlyList<WorkActivity> Rows(object vm,string name)=>((IEnumerable)Property(vm.GetType(),name).GetValue(vm)!).Cast<WorkActivity>().ToArray();
    private static readonly string CheckRoot=@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks";
    private static string Fixture()=>Directory.CreateDirectory(Path.Combine(CheckRoot,"project-sessions-fixture-"+Guid.NewGuid().ToString("N"))).FullName;
    private static string Write(string root,string name,object value) { var p=Path.Combine(root,name);File.WriteAllText(p,JsonSerializer.Serialize(value));return p; }
    private static object Exchange(string session,int revision,string request="받은 요청",string recordId="task",string response="실제 답변")=>new {schemaVersion=1,recordType="task_exchange",recordId,revision,title="진행",projectId="p",actorId="Codex",sessionId=session,receivedAt="2026-10-07T01:00:00+09:00",updatedAt="2026-10-07T02:00:00+09:00",request=new{summary=request,details="",source="사용자 대화"},response=new{summary=response,details="",source="Codex 대화"},status="in_progress",workDone=new[]{"확인"},verification=Array.Empty<object>(),nextActions=new[]{"다음"},blockers=Array.Empty<string>(),supersedes=Array.Empty<string>()};
    [Fact] public void ExchangePreservesValidatedSessionProvenanceAndSummaries()
    {
        var root=Fixture();var file=Write(root,"exchange.json",Exchange("session-one",2));
        var parsed=TaskExchangeDocument.Parse(File.ReadAllText(file),"p",out _)!;
        Assert.Equal("session-one",Value(parsed,"SessionId"));Assert.Equal("사용자 대화",Value(parsed,"RequestSource"));
        var row=WorkDashboardService.ReadExchange(file)!;
        Assert.Equal("session-one",Value(row,"SessionId"));Assert.Equal("Codex",Value(row,"ActorId"));
        Assert.Equal("받은 요청",row.CommandSummary);Assert.Equal("실제 답변",Value(row,"ResponseSummary"));
        Assert.Equal("Codex 대화",Value(row,"ResponseSource"));Assert.Equal("unknown",row.ProcessStatus);Assert.False(row.LivenessKnown);
    }
    [Fact] public async Task HandoverSessionRevisionUsesOfficialTaskIdentityAndDistinctRecordIdsStaySeparate()
    {
        var root=Fixture();Write(root,"old.json",Exchange("one",1,response:"이전"));Write(root,"new.json",Exchange("handover",2,response:"최신"));Write(root,"other.json",Exchange("two",1,recordId:"independent"));
        var service=new WorkDashboardService(Path.Combine(root,"runs"));service.ExchangeRoots.Add(root);
        var rows=(await service.ReadAsync(default)).Where(r=>r.Source=="명령·답변 task_exchange 기록").ToArray();
        Assert.Equal(2,rows.Length);Assert.Contains(rows,r=>Value(r,"ResponseSummary")=="최신");Assert.DoesNotContain(rows,r=>Value(r,"ResponseSummary")=="이전");
        Assert.Equal("handover",Value(rows.Single(r=>Value(r,"ResponseSummary")=="최신"),"SessionId"));
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>(rows));
        Method(vm.GetType(),"SetProjectContext").Invoke(vm,["p","프로젝트"]);vm.ApplySnapshot(rows);vm.Selected=rows[0];
        vm.ApplySnapshot(rows.Select(r=>r with {Stage="변경"}).ToArray());Assert.Equal(rows[0].Id,vm.Selected?.Id);
    }
    [Fact] public async Task ConflictingSessionContentAtSameOfficialIdentityRevisionIsNotTwoValidRecords()
    {
        var root=Fixture();Write(root,"one.json",Exchange("one",2));Write(root,"two.json",Exchange("two",2));
        var service=new WorkDashboardService(Path.Combine(root,"runs"));service.ExchangeRoots.Add(root);
        var row=Assert.Single(await service.ReadAsync(default),r=>r.IsConversation);
        Assert.Equal("unknown",row.Status);Assert.Contains("충돌",row.Error);Assert.False(row.LivenessKnown);
    }
    [Fact] public void CurrentProjectSeparatesConversationsAndExecutionsWithoutWorkerGuessing()
    {
        var root=Fixture();var record=WorkDashboardService.ReadExchange(Write(root,"record.json",Exchange("one",1)))!;
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.ApplySnapshot([record,new(){Id="local:a",ProjectId="p",Source="관제탑 소유 실행 기록",WorkerKind="Codex"},new(){Id="local:b",ProjectId="other",Source="관제탑 소유 실행 기록"}]);
        Method(vm.GetType(),"SetProjectContext").Invoke(vm,["p","프로젝트"]);
        Assert.Single(Rows(vm,"ProjectRecords"));Assert.Single(Rows(vm,"ProjectExecutions"));
        Method(vm.GetType(),"SetProjectContext").Invoke(vm,["","미선택"]);Assert.Empty(Rows(vm,"ProjectRecords"));Assert.Empty(Rows(vm,"ProjectExecutions"));
    }
    [Fact] public async Task RegisteredContinuousStepsStayDistinctWithoutReadingPrivateOutputOrClaimingLiveness()
    {
        var root=Fixture();var file=Write(root,"state.json",new{approved_root=root,plan_id="plan",plan_sha256=new string('a',64),project="p",status="running",updated_at="2026-10-07T02:00:00Z",steps=new Dictionary<string,object>{["first"]=new{status="succeeded",attempts=1,returncode=0,aiReceipt=new{schemaVersion=1,adapter="codex-jsonl-v1",executionId="plan/first",planId="plan",stepId="first",planSha256=new string('a',64),requestedModel="gpt-6.1-sol",status="succeeded",processExitCode=0,terminalObserved=true,reportObserved=true,reportRequired=false,artifactVerified=(bool?)null,artifactSha256=(string?)null,failureCode=(string?)null}},["second"]=new{status="running",attempts=1}}});
        File.WriteAllText(Path.Combine(root,"events.private.jsonl"),"SECRET PRIVATE BODY");
        var service=new WorkDashboardService(Path.Combine(root,"runs"));service.ContinuousPaths.Add(file);
        var rows=await service.ReadAsync(default);var steps=rows.Where(r=>r.Source=="연속 실행기 단계 기록").ToArray();
        Assert.Equal(2,steps.Length);Assert.Equal(2,steps.Select(r=>r.Id).Distinct().Count());Assert.All(steps,r=>Assert.False(r.LivenessKnown));
        var ai=Assert.Single(steps,r=>Value(r,"ExecutionId")=="plan/first");Assert.Equal("succeeded",ai.AICompletionState);Assert.Equal("gpt-6.1-sol",Value(ai,"RequestedModel"));
        Assert.DoesNotContain("SECRET",await service.DetailsAsync(ai));
        Assert.All(steps,r=>Assert.DoesNotContain("실행 중",r.StatusLabel));
    }
    [Fact] public async Task DynamicJevClassificationRequiresExactKnownProjectProgramIdentity()
    {
        var root=Fixture();var runs=Directory.CreateDirectory(Path.Combine(root,"runs")).FullName;
        foreach(var (id,program) in new[]{("exact","p/jev"),("similar","p/jev-other")}) {var dir=Directory.CreateDirectory(Path.Combine(runs,id)).FullName;Write(dir,"result.json",new{Id=id,ProjectId="p",ProgramId=program,Command="Jev 작업",State="running",StartedAt=DateTimeOffset.UtcNow,LogPath=""});}
        var service=new WorkDashboardService(runs);service.RegisterCatalog([new(){Id="p",Name="등록",DisplayName="등록"}]);
        var rows=await service.ReadAsync(default);Assert.Equal("Jev",rows.Single(r=>r.Id=="local:exact").WorkerKind);Assert.Equal("미확인",rows.Single(r=>r.Id=="local:similar").WorkerKind);
    }
    [Fact] public void ManualRegistrationReadsOnlyAndLocalViewRejects()
    {
        var root=Fixture();var file=Write(root,"state.json",new{approved_root=root,project="p",plan_id="plan",status="pending",steps=new{},updated_at="2026-10-07T02:00:00Z"});
        var bytes=File.ReadAllBytes(file);var stamp=File.GetLastWriteTimeUtc(file);var attrs=File.GetAttributes(file);var service=new WorkDashboardService(Path.Combine(root,"runs"));
        var method=Method(service.GetType(),"RegisterContinuousSource");
        Assert.False((bool)method.Invoke(service,[file,true,false])!);Assert.Empty(service.ContinuousPaths);
        Assert.True((bool)method.Invoke(service,[file,true,true])!);Assert.Single(service.ContinuousPaths);
        Assert.True((bool)method.Invoke(service,[file,true,true])!);Assert.Single(service.ContinuousPaths);
        Assert.Equal(bytes,File.ReadAllBytes(file));Assert.Equal(stamp,File.GetLastWriteTimeUtc(file));Assert.Equal(attrs,File.GetAttributes(file));
        Assert.Throws<TargetInvocationException>(()=>method.Invoke(service,["relative/state.json",true,true]));
        Assert.Throws<TargetInvocationException>(()=>method.Invoke(service,[Path.Combine(root,"Desktop","state.json"),true,true]));
        var outside=Write(root,"outside.json",new{approved_root=Path.Combine(root,"other"),steps=new{}});
        Assert.Throws<TargetInvocationException>(()=>method.Invoke(service,[outside,true,true]));
        var invalid=Write(root,"invalid.json",new{approved_root=root,steps="invalid"});Assert.Throws<TargetInvocationException>(()=>method.Invoke(service,[invalid,true,true]));
        for(var i=service.ContinuousPaths.Count;i<40;i++)service.ContinuousPaths.Add(Path.Combine(root,i+".json"));
        var extra=Write(root,"extra.json",new{approved_root=root,steps=new{}});Assert.Throws<TargetInvocationException>(()=>method.Invoke(service,[extra,true,true]));Assert.Equal(40,service.ContinuousPaths.Count);
    }
}
