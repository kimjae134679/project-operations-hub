using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class ProjectRecordPublisherTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive=true };
    private const string Repo="kimjae134679/Threads", Prefix="04_COMMUNICATION/shared-records/Threads";
    private static Type Required(string name) => Assert.IsAssignableFrom<Type>(typeof(CommunicationService).Assembly.GetType(name));
    private static string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static JsonObject Snapshot(object value)=>JsonNode.Parse(JsonSerializer.Serialize(value,value.GetType()))!.AsObject();
    private static string Text(JsonObject value,string key)=>value[key]?.GetValue<string>()??"";

    private sealed class Fixture : IDisposable
    {
        public string Root {get;}=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\project-record-publishing-fixtures",Guid.NewGuid().ToString("N"));
        public string Journal=>Path.Combine(Root,"journal");
        public Fixture()
        {
            Directory.CreateDirectory(Path.Combine(Root,".git"));
            File.WriteAllText(Path.Combine(Root,".git","config"),"[remote \"origin\"]\n url = https://github.com/kimjae134679/Threads.git\n");
            File.WriteAllText(Path.Combine(Root,"dirty-user-code.txt"),"Uncommitted synthetic code must remain untouched");
        }
        public object Registration(bool enabled=true,long repoId=1368308612,string? prefix=null)
        {
            var type=Required("AIControlTower.Models.ProjectRecordPublishingRegistration");
            return JsonSerializer.Deserialize(JsonSerializer.Serialize(new {ProjectId="Threads",RootPath=Root,OriginUrl="https://github.com/kimjae134679/Threads.git",RepositoryId=repoId,
                RepositoryFullName=Repo,BaseBranch="main",SharedPrefix=prefix??Prefix,Enabled=enabled}),type,JsonOptions)!;
        }
        public void Dispose()
        {
            // Keep owned D evidence; never recursively clean originals or other actors' files.
        }
    }
    private static string Body(string title="작업 기록",string summary="검사 후 진행",string details="PRIVATE_DETAIL_SENTINEL",string actor="/root",int revision=1)
        =>JsonSerializer.Serialize(new {schemaVersion=1,recordType="task_exchange",recordId="registered-work",revision,title,projectId="Threads",actorId=actor,sessionId="PRIVATE_SESSION_SENTINEL",
            receivedAt="2026-10-07T10:00:00+09:00",updatedAt="2026-10-07T10:01:00+09:00",request=new {summary,details,source="current_chat"},
            response=new {summary="이미 전달한 진행 답변",details="PRIVATE_REPLY_SENTINEL",source="current_chat"},status="in_progress",workDone=new[]{"검사 결과 보존"},
            verification=new[]{new{name="코드 검사",result="not_run",evidence=Array.Empty<string>()}},nextActions=new[]{"다음 단계 확인"},blockers=Array.Empty<string>(),supersedes=Array.Empty<string>()});
    private static CommunicationCollectionResult Collection(Fixture f,params string[] bodies)=>new(bodies.Select((body,i)=>new CommunicationCollectedRecord("Threads","outbox",
        "_통합소통/보낼자료/task-"+i+".json",Path.Combine(f.Root,"_통합소통","보낼자료","task-"+i+".json"),Hash(body),Path.Combine(f.Root,"collected",i+".json"),body,DateTimeOffset.UtcNow)).ToArray(),[],DateTimeOffset.UtcNow);
    private static object Publisher(Fixture f,FakeApi api)
    {
        var type=Required("AIControlTower.Services.ProjectRecordPublisher");
        Func<string,string,string?,CancellationToken,Task<(int StatusCode,string Json)>> callback=api.Call;
        return Activator.CreateInstance(type,callback,f.Journal)!;
    }
    private static async Task<JsonObject> Publish(Fixture f,FakeApi api,object registration,CommunicationCollectionResult collection)
    {
        var publisher=Publisher(f,api);
        var method=publisher.GetType().GetMethod("PublishAsync")!;
        var task=Assert.IsAssignableFrom<Task>(method.Invoke(publisher,[registration,collection,CancellationToken.None]));
        await task;
        return Snapshot(task.GetType().GetProperty("Result")!.GetValue(task)!);
    }
    private static JsonArray Exports(object registration,CommunicationCollectionResult collection)
    {
        var method=Required("AIControlTower.Services.ProjectRecordPublisher").GetMethod("PrepareExports",BindingFlags.Public|BindingFlags.Static);
        Assert.NotNull(method);
        return JsonNode.Parse(JsonSerializer.Serialize(method.Invoke(null,[registration,collection])))!.AsArray();
    }

    private sealed class FakeApi
    {
        public int RepoId=1368308612;
        public bool Push=true,Archived=false,Private=false,Branch,Pr,ThrowAfterBranch,ThrowAfterPr,Auth404,Conflict;
        public readonly List<(string Method,string Endpoint,string? Body)> Calls=[];
        public readonly Dictionary<string,string> Files=new(StringComparer.Ordinal);
        public string BaseSha=new('a',40),BaseTree=new('b',40),NewTree=new('c',40),Commit=new('d',40),BranchName="";
        public Task<(int StatusCode,string Json)> Call(string method,string endpoint,string? body,CancellationToken ct)
        {
            Calls.Add((method,endpoint,body));
            object result;
            if(method=="GET"&&endpoint=="repos/"+Repo)
            {if(Auth404)return Task.FromResult((404,"{}"));result=new{id=RepoId,full_name=Repo,default_branch="main",archived=Archived,@private=Private,permissions=new{push=Push}};}
            else if(method=="GET"&&endpoint.EndsWith("git/ref/heads/main"))result=new{@ref="refs/heads/main",@object=new{sha=BaseSha}};
            else if(method=="GET"&&endpoint.Contains("git/ref/heads/"))
            {if(!Branch)return Task.FromResult((404,"{}"));result=new{@ref="refs/heads/"+BranchName,@object=new{sha=Commit}};}
            else if(method=="GET"&&endpoint.Contains("git/commits/"))
                result=new{sha=endpoint.EndsWith(BaseSha)?BaseSha:Commit,tree=new{sha=endpoint.EndsWith(BaseSha)?BaseTree:NewTree},parents=new[]{new{sha=BaseSha}}};
            else if(method=="GET"&&endpoint.Contains("git/trees/"))
                result=new{truncated=false,tree=Files.Select(p=>new{path=p.Key,type="blob",mode="100644",sha=Conflict?new string('e',40):Blob(p.Value)}).ToArray()};
            else if(method=="GET"&&endpoint.Contains("/contents/"))
            {var path=Uri.UnescapeDataString(endpoint.Split("/contents/")[1].Split('?')[0]);if(!Files.TryGetValue(path,out var value))return Task.FromResult((404,"{}"));result=new{type="file",encoding="base64",content=Convert.ToBase64String(Encoding.UTF8.GetBytes(value)),sha=Blob(value)};}
            else if(method=="GET"&&endpoint.Contains("/compare/"))result=new{files=Files.Keys.Select(p=>new{filename=p,status="added"}).ToArray(),total_commits=1,commits=new[]{new{sha=Commit}}};
            else if(method=="GET"&&endpoint.Contains("/pulls?"))result=Pr?new[]{Pull()}:Array.Empty<object>();
            else if(method=="POST"&&endpoint.EndsWith("/git/trees"))
            {var j=JsonNode.Parse(body!)!;Assert.Equal(BaseTree,j["base_tree"]!.GetValue<string>());foreach(var entry in j["tree"]!.AsArray()){var path=entry!["path"]!.GetValue<string>();Assert.StartsWith(Prefix+"/",path);Files[path]=entry["content"]!.GetValue<string>();}result=new{sha=NewTree};}
            else if(method=="POST"&&endpoint.EndsWith("/git/commits"))
            {var j=JsonNode.Parse(body!)!;Assert.Equal(BaseSha,j["parents"]![0]!.GetValue<string>());result=new{sha=Commit};}
            else if(method=="POST"&&endpoint.EndsWith("/git/refs"))
            {var j=JsonNode.Parse(body!)!;BranchName=j["ref"]!.GetValue<string>()["refs/heads/".Length..];Assert.StartsWith("ai-records/",BranchName);Branch=true;if(ThrowAfterBranch){ThrowAfterBranch=false;throw new IOException("Lost branch response");}result=new{@ref="refs/heads/"+BranchName,@object=new{sha=Commit}};}
            else if(method=="POST"&&endpoint.EndsWith("/pulls"))
            {var j=JsonNode.Parse(body!)!;Assert.True(j["draft"]!.GetValue<bool>());Assert.Equal("main",j["base"]!.GetValue<string>());Pr=true;if(ThrowAfterPr){ThrowAfterPr=false;throw new IOException("Lost PR response");}result=Pull();}
            else throw new InvalidOperationException("Unexpected API request: "+method+" "+endpoint);
            return Task.FromResult((method=="POST"?201:200,JsonSerializer.Serialize(result)));
        }
        private object Pull()=>new{number=99,html_url="https://github.com/"+Repo+"/pull/99",state="open",draft=true,merged_at=(string?)null,
            head=new{@ref=BranchName,sha=Commit,repo=new{id=RepoId,full_name=Repo}},@base=new{@ref="main",repo=new{id=RepoId,full_name=Repo}}};
        private static string Blob(string value){var bytes=Encoding.UTF8.GetBytes(value);return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes("blob "+bytes.Length+"\0").Concat(bytes).ToArray())).ToLowerInvariant();}
        public int Posts=>Calls.Count(c=>c.Method=="POST");
    }

    [Fact]
    public void ExportOmitsPrivateIdentityDetailsPathsReceiptsAndNonTaskFiles()
    {
        using var f=new Fixture();var reg=f.Registration();var collection=Collection(f,Body());
        var extra=collection.Records.Concat(new[]{collection.Records[0] with{Kind="notice-receipt",Body="PRIVATE_RECEIPT"},collection.Records[0] with{Body="# PRIVATE_GUIDE"}}).ToArray();
        var exports=Exports(reg,new(extra,[],DateTimeOffset.UtcNow));Assert.Single(exports);
        var text=exports.ToJsonString();foreach(var privateValue in new[]{"PRIVATE_",f.Root,"/root","sessionId","sourcePath","details","evidence"})Assert.DoesNotContain(privateValue,text);
        Assert.DoesNotContain("검사 후 진행",text);Assert.DoesNotContain("이미 전달한 진행 답변",text);
        foreach(var forbidden in new[]{"title","request","response","workDone","nextActions","blockers","actorId","recordId"})Assert.DoesNotContain("\""+forbidden+"\"",text);
        Assert.Contains("in_progress",text);Assert.Contains("revision",text);Assert.Contains("verificationCounts",text);
    }
    [Theory]
    [InlineData("password=not-for-public")]
    [InlineData(@"C:\Users\user\private-data")]
    [InlineData("person@example.com")]
    [InlineData("제목\u001b[31m")]
    public void ArbitraryPrivateFreeTextIsNeverExported(string title)
    {using var f=new Fixture();var exports=Exports(f.Registration(),Collection(f,Body(title:title,summary:title)));Assert.Single(exports);Assert.DoesNotContain(title,exports.ToJsonString());}
    [Fact]
    public async Task InvalidControlledMetadataIsHeldBeforeAnyMutation()
    {
        using var f=new Fixture();var invalidDate=JsonNode.Parse(Body())!.AsObject();invalidDate["updatedAt"]="private-time";
        Assert.NotEqual(Body(),invalidDate.ToJsonString());
        foreach(var body in new[]{Body().Replace("in_progress","private_custom_state"),invalidDate.ToJsonString()})
        {var api=new FakeApi();var result=await Publish(f,api,f.Registration(),Collection(f,body));Assert.Equal("held",Text(result,"Status"));Assert.Equal("record_validation",Text(result,"FailureStage"));Assert.Equal(0,api.Posts);}
    }
    [Fact]
    public async Task DisabledOrUnverifiedRepositoryRegistrationCannotPublish()
    {using var f=new Fixture();foreach(var reg in new[]{f.Registration(enabled:false),f.Registration(repoId:42),f.Registration(prefix:"../private")}){var api=new FakeApi();var r=await Publish(f,api,reg,Collection(f,Body()));Assert.Equal("held",Text(r,"Status"));Assert.Equal(0,api.Posts);}}
    [Fact]
    public async Task ServerRepoIdentityPermissionAnd404AreNotMissingBranchPermission()
    {using var f=new Fixture();foreach(var api in new[]{new FakeApi{RepoId=42},new FakeApi{Push=false},new FakeApi{Archived=true},new FakeApi{Auth404=true}}){var r=await Publish(f,api,f.Registration(),Collection(f,Body()));Assert.Equal("held",Text(r,"Status"));Assert.Equal(0,api.Posts);}}
    [Fact]
    public async Task SuccessPublishesOnlySharedPathsAndOneDraftPrWithoutDirtyWrites()
    {
        using var f=new Fixture();var api=new FakeApi();var reg=f.Registration();var collection=Collection(f,Body());var dirty=Path.Combine(f.Root,"dirty-user-code.txt");var before=File.ReadAllBytes(dirty);
        var result=await Publish(f,api,reg,collection);Assert.Equal("pr_open",Text(result,"Status"));Assert.True(result["Uploaded"]!.GetValue<bool>());Assert.False(result["CentralShared"]!.GetValue<bool>());
        Assert.Equal("https://github.com/"+Repo+"/pull/99",Text(result,"PullRequestUrl"));Assert.Equal(4,api.Posts);Assert.Equal(before,File.ReadAllBytes(dirty));
        Assert.DoesNotContain(api.Calls,c=>c.Method is "PATCH" or "DELETE"||c.Endpoint.Contains("/merge"));
        var repeated=await Publish(f,api,reg,collection);Assert.Equal("pr_open",Text(repeated,"Status"));Assert.Equal(4,api.Posts);
    }
    [Fact]
    public async Task LostBranchReplyRecoversObservedRefWithoutDuplicateWrites()
    {using var f=new Fixture();var api=new FakeApi{ThrowAfterBranch=true};var reg=f.Registration();var records=Collection(f,Body());var first=await Publish(f,api,reg,records);Assert.Equal("upload_uncertain",Text(first,"Status"));Assert.Equal(3,api.Posts);var second=await Publish(f,api,reg,records);Assert.Equal("pr_open",Text(second,"Status"));Assert.Equal(4,api.Posts);}
    [Fact]
    public async Task LostPrReplyRecoversExactExistingPrWithoutDuplicateCreation()
    {using var f=new Fixture();var api=new FakeApi{ThrowAfterPr=true};var reg=f.Registration();var records=Collection(f,Body());var first=await Publish(f,api,reg,records);Assert.Equal("pr_uncertain",Text(first,"Status"));Assert.Equal(4,api.Posts);var second=await Publish(f,api,reg,records);Assert.Equal("pr_open",Text(second,"Status"));Assert.Equal(4,api.Posts);}
    [Fact]
    public async Task AmbiguousPrWithoutObservedPrDoesNotBlindlyRepeatPost()
    {using var f=new Fixture();var api=new FakeApi{ThrowAfterPr=true};var reg=f.Registration();var records=Collection(f,Body());await Publish(f,api,reg,records);api.Pr=false;var result=await Publish(f,api,reg,records);Assert.Equal("pr_uncertain",Text(result,"Status"));Assert.Equal(4,api.Posts);}
    [Fact]
    public async Task ConflictingLocalRevisionAndSourceHashMismatchAreHeld()
    {using var f=new Fixture();var reg=f.Registration();foreach(var collection in new[]{Collection(f,Body(),Body(summary:"不同 답변")),new CommunicationCollectionResult([Collection(f,Body()).Records[0] with{ContentSha256=new string('0',64)}],[],DateTimeOffset.UtcNow)}){var api=new FakeApi();var result=await Publish(f,api,reg,collection);Assert.Equal("held",Text(result,"Status"));Assert.Equal(0,api.Posts);}}
    [Fact]
    public async Task ExistingRemoteSamePathDifferentHashIsNeverOverwritten()
    {using var f=new Fixture();var reg=f.Registration();var records=Collection(f,Body());var exports=Exports(reg,records);var api=new FakeApi{Conflict=true};api.Files[exports[0]!["Path"]!.GetValue<string>()]="Other owned remote record";var result=await Publish(f,api,reg,records);Assert.Equal("held",Text(result,"Status"));Assert.Equal(0,api.Posts);}
    [Fact]
    public async Task UnknownJournalStageCannotAuthorizeMoreRemoteWrites()
    {
        using var f=new Fixture();var api=new FakeApi();var reg=f.Registration();var records=Collection(f,Body());
        Assert.Equal("pr_open",Text(await Publish(f,api,reg,records),"Status"));
        var path=Assert.Single(Directory.GetFiles(f.Journal,"*.json"));var journal=JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        journal["Stage"]="unknown-not-authorized";File.WriteAllText(path,journal.ToJsonString());
        var before=api.Posts;var result=await Publish(f,api,reg,records);
        Assert.Equal("held",Text(result,"Status"));Assert.Equal(before,api.Posts);
        Assert.Equal("unknown-not-authorized",JsonNode.Parse(File.ReadAllText(path))!["Stage"]!.GetValue<string>());
    }
    [Fact]
    public async Task TwoPublisherInstancesCannotPostConcurrentlyForSameJournalBatch()
    {
        using var f=new Fixture();var firstApi=new FakeApi();var secondApi=new FakeApi();
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Func<string,string,string?,CancellationToken,Task<(int StatusCode,string Json)>> blocked=async (method,endpoint,body,ct)=>
        {
            if(method=="POST"){entered.TrySetResult();await release.Task.WaitAsync(ct);}
            return await firstApi.Call(method,endpoint,body,ct);
        };
        var engine=Activator.CreateInstance(Required("AIControlTower.Services.ProjectRecordPublisher"),blocked,f.Journal)!;
        var task=Assert.IsAssignableFrom<Task>(engine.GetType().GetMethod("PublishAsync")!.Invoke(engine,[f.Registration(),Collection(f,Body()),CancellationToken.None]));
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var second=await Publish(f,secondApi,f.Registration(),Collection(f,Body()));
            Assert.Equal("held",Text(second,"Status"));Assert.Equal(0,secondApi.Posts);
        }
        finally{release.TrySetResult();await task.WaitAsync(TimeSpan.FromSeconds(5));}
        var first=Snapshot(task.GetType().GetProperty("Result")!.GetValue(task)!);
        Assert.Equal("pr_open",Text(first,"Status"));Assert.Equal(4,firstApi.Posts);
    }
    [Fact]
    public void NativeTransportUsesFixedGhHiddenShellFalseAndStdinNotCommandLinePayload()
    {
        var type=Required("AIControlTower.Services.GitHubRecordTransport");var method=type.GetMethod("BuildStartInfo",BindingFlags.Public|BindingFlags.Static);Assert.NotNull(method);
        var info=Assert.IsType<System.Diagnostics.ProcessStartInfo>(method.Invoke(null,["POST","repos/"+Repo+"/git/trees",true]));
        Assert.Equal(@"C:\Program Files\GitHub CLI\gh.exe",info.FileName);Assert.False(info.UseShellExecute);Assert.True(info.CreateNoWindow);Assert.Equal(System.Diagnostics.ProcessWindowStyle.Hidden,info.WindowStyle);
        Assert.True(info.RedirectStandardInput);Assert.Contains("--input",info.ArgumentList);Assert.Contains("-",info.ArgumentList);Assert.DoesNotContain("auth",info.ArgumentList);Assert.DoesNotContain("--verbose",info.ArgumentList);
    }
}
