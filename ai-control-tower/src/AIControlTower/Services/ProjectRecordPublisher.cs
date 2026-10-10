using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Registered metadata-only Git Data publishing. Journals uncertain writes; never repeats them blindly.</summary>
public sealed class ProjectRecordPublisher
{
    public const string JournalRoot = @"D:\A_KJ\AI\ControlTowerData\record-publication";
    private const int MaximumNewPaths = 100;
    private const int MaximumExportBytes = 8 * 1024 * 1024;
    private const string FixtureRoot = @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\project-record-publishing-fixtures";
    private static readonly Dictionary<string,(long Id,string Name,bool Private)> Repositories = new(StringComparer.Ordinal)
    {
        ["Threads"]=(1368308612,"kimjae134679/Threads",false),
        ["Mushoku-Audiobook"]=(1404685845,"kimjae134679/Mushoku-Tensei-AI-Audiobook",true),
        ["Control-Tower"]=(1362083625,"kimjae134679/project-operations-hub",false),
        ["PhoneLOL"]=(1378550871,"kimjae134679/PhoneLoL_02-Source",true)
    };
    private readonly Func<string,string,string?,CancellationToken,Task<(int StatusCode,string Json)>> _api;
    private readonly string _journalRoot;
    public ProjectRecordPublisher() : this(new GitHubRecordTransport().SendAsync,JournalRoot) { }
    public ProjectRecordPublisher(Func<string,string,string?,CancellationToken,Task<(int StatusCode,string Json)>> api,string journalRoot)
    {
        _api=api; _journalRoot=Path.GetFullPath(journalRoot);
        if (!Inside(_journalRoot,JournalRoot) && !Inside(_journalRoot,FixtureRoot)) throw new ArgumentException("Unowned publication journal");
        SafePath(_journalRoot);
    }
    public static IReadOnlyList<ProjectRecordExport> PrepareExports(ProjectRecordPublishingRegistration r,CommunicationCollectionResult collection)
    {
        ValidateRegistration(r);
        if (collection.Errors.Count>0 || collection.Records.Count>3000) throw new InvalidDataException("Collection incomplete");
        var records=new Dictionary<string,ProjectRecordExport>(StringComparer.Ordinal);
        foreach (var source in collection.Records)
        {
            if (source.ProjectId!=r.ProjectId || source.Kind!="outbox" || !source.Body.TrimStart().StartsWith('{')) continue;
            if (Encoding.UTF8.GetByteCount(source.Body)>CommunicationService.MaximumFileBytes) throw new InvalidDataException("Record limit");
            using var document=JsonDocument.Parse(source.Body,new JsonDocumentOptions {MaxDepth=32}); var root=document.RootElement;
            if (!root.TryGetProperty("recordType",out var kind) || kind.GetString()!="task_exchange") continue;
            UniqueKeys(root);
            var normalized=source.Body.TrimStart('\uFEFF').Replace("\r\n","\n",StringComparison.Ordinal).Replace('\r','\n');
            if (Hash(normalized)!=source.ContentSha256 || !Inside(source.SourcePath,r.RootPath)) throw new InvalidDataException("Collected record provenance mismatch");
            SafePath(source.SourcePath);
            var task=TaskExchangeDocument.Parse(source.Body,r.ProjectId,out _);
            if (task is null) throw new InvalidDataException("Invalid task record");
            // Never emit title, free-text request/answer, work lists, evidence, source paths or private actor/session.
            var opaque=Hash(r.ProjectId+"\n"+task.ActorId+"\n"+task.RecordId);
            var checks=root.GetProperty("verification").EnumerateArray().Select(v=>v.GetProperty("result").GetString()).ToArray();
            var content=JsonSerializer.Serialize(new {schemaVersion=1,recordType="task_exchange_metadata",projectId=r.ProjectId,recordKey=opaque,
                revision=task.Revision,status=task.Status,
                receivedAt=DateTimeOffset.Parse(root.GetProperty("receivedAt").GetString()!,System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime().ToString("O"),
                updatedAt=task.UpdatedAt.ToUniversalTime().ToString("O"),
                verificationCounts=new {pass=checks.Count(v=>v=="pass"),fail=checks.Count(v=>v=="fail"),not_run=checks.Count(v=>v=="not_run"),partial=checks.Count(v=>v=="partial")}});
            var path=r.SharedPrefix+"/"+opaque+"/r"+task.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture)+".json";
            var export=new ProjectRecordExport(path,content) {SourceHash=source.ContentSha256};
            if (records.TryGetValue(path,out var prior) && (prior.SourceHash!=export.SourceHash || prior.Content!=export.Content)) throw new InvalidDataException("Conflicting local revision");
            records[path]=export;
        }
        // Retain and validate every revision; only missing remote paths count toward a write batch.
        if (records.Values.Sum(v=>(long)Encoding.UTF8.GetByteCount(v.Content))>MaximumExportBytes)
            throw new InvalidDataException("Publication export byte limit");
        return records.Values.OrderBy(v=>v.Path,StringComparer.Ordinal).ToArray();
    }
    public async Task<ProjectRecordPublishingResult> PublishAsync(ProjectRecordPublishingRegistration r,CommunicationCollectionResult collection,CancellationToken ct=default)
    {
        var batch=""; var stage="registration"; var uploaded=false; Journal? state=null; string? journalPath=null;
        ProjectRecordPublishingResult Result(string status,string? url=null)=>new(status,r.ProjectId,batch,uploaded,url,status is "held" or "upload_uncertain" or "pr_uncertain"?stage:null);
        try
        {
            ct.ThrowIfCancellationRequested();
            ValidateRegistration(r);
            // This lease spans record validation, remote reads/writes and the batch journal. A changed batch
            // must not race another publisher of this registered project, including another process.
            stage="project_lease"; SafePath(_journalRoot); Directory.CreateDirectory(_journalRoot); SafePath(_journalRoot);
            var projectLeasePath=Path.Combine(_journalRoot,r.RepositoryId.ToString(System.Globalization.CultureInfo.InvariantCulture)+"-publisher.lock");
            SafePath(projectLeasePath);
            using var projectLease=new FileStream(projectLeasePath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            stage="record_validation";
            var exports=PrepareExports(r,collection);
            if (exports.Count==0) return Result("empty");
            batch=Hash(r.RepositoryId+"\n"+r.BaseBranch+"\n"+string.Join("\n",exports.Select(e=>e.Path+"\n"+Hash(e.Content)+"\n"+e.SourceHash)));
            var repo="repos/"+r.RepositoryFullName;
            var branch="ai-records/"+r.ProjectId.ToLowerInvariant()+"-"+batch;
            stage="repository";
            var metadata=await Get(repo,ct).ConfigureAwait(false);
            var expected=Repositories[r.ProjectId];
            if (metadata.GetProperty("id").GetInt64()!=r.RepositoryId || Text(metadata,"full_name")!=r.RepositoryFullName || Text(metadata,"default_branch")!=r.BaseBranch
                || metadata.GetProperty("archived").GetBoolean() || metadata.GetProperty("private").GetBoolean()!=expected.Private || !metadata.GetProperty("permissions").GetProperty("push").GetBoolean())
                throw new InvalidDataException("Repository authorization mismatch");
            stage="journal"; SafePath(_journalRoot); Directory.CreateDirectory(_journalRoot); SafePath(_journalRoot);
            var prefix=Path.Combine(_journalRoot,r.RepositoryId.ToString(System.Globalization.CultureInfo.InvariantCulture)+"-"+batch);
            SafePath(prefix+".lock");
            using var lease=new FileStream(prefix+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            journalPath=prefix+".json"; SafePath(journalPath);
            if (File.Exists(journalPath))
            {
                if (new FileInfo(journalPath).Length>1024*1024) throw new InvalidDataException("Journal limit");
                var bytes=File.ReadAllBytes(journalPath); using var j=JsonDocument.Parse(bytes); UniqueKeys(j.RootElement);
                state=JsonSerializer.Deserialize<Journal>(bytes)??throw new InvalidDataException("Journal invalid");
                if (state.Stage is not ("prepared" or "tree_pending" or "tree_created" or "commit_pending" or "commit_created" or "branch_pending" or "branch_uploaded" or "pr_pending" or "pr_open"))
                    throw new InvalidDataException("Unknown journal stage");
                if (state.BatchHash!=batch || state.RepositoryId!=r.RepositoryId || state.Branch!=branch || state.BaseBranch!=r.BaseBranch || state.Paths.Except(exports.Select(e=>e.Path),StringComparer.Ordinal).Any()) throw new InvalidDataException("Journal binding mismatch");
                if (state.Paths.Length>MaximumNewPaths || state.Paths.Distinct(StringComparer.Ordinal).Count()!=state.Paths.Length)
                    throw new InvalidDataException("Publication journal path limit");
            }
            else
            {
                stage="base"; var basis=await Get(repo+"/git/ref/heads/"+r.BaseBranch,ct).ConfigureAwait(false);
                var baseSha=Sha(basis.GetProperty("object")); var commit=await Get(repo+"/git/commits/"+baseSha,ct).ConfigureAwait(false);
                var baseTree=Sha(commit.GetProperty("tree"));
                var tree=await ReadTree(repo,baseTree,ct).ConfigureAwait(false);
                var missing=new List<string>();
                foreach (var export in exports)
                {
                    if (tree.TryGetValue(export.Path,out var existing)) { if (existing!=Blob(export.Content)) throw new InvalidDataException("Remote revision conflict"); }
                    else missing.Add(export.Path);
                }
                if (missing.Count==0) {uploaded=true;return Result("already_present");}
                if (missing.Count>MaximumNewPaths) throw new InvalidDataException("Publication batch limit");
                state=new Journal {BatchHash=batch,RepositoryId=r.RepositoryId,Branch=branch,BaseBranch=r.BaseBranch,BaseSha=baseSha,BaseTree=baseTree,Paths=missing.ToArray(),Stage="prepared"};
                Save(journalPath,state);
            }
            stage="branch_lookup";
            var branchResponse=await _api("GET",repo+"/git/ref/heads/"+branch,null,ct).ConfigureAwait(false);
            if (branchResponse.StatusCode==200)
            {
                var observed=Parse(branchResponse.Json); var remoteSha=Sha(observed.GetProperty("object"));
                if (state.CommitSha.Length>0 && remoteSha!=state.CommitSha) throw new InvalidDataException("Branch conflict");
                state.CommitSha=remoteSha;
                await VerifyBranch(repo,state,exports,ct).ConfigureAwait(false); uploaded=true;
                if (state.Stage is not ("pr_pending" or "pr_open")) {state.Stage="branch_uploaded";Save(journalPath,state);}
            }
            else if (branchResponse.StatusCode==404)
            {
                if (state.Stage!="prepared") {stage=state.Stage;return Result("upload_uncertain");}
                stage="tree_pending";state.Stage=stage;Save(journalPath,state);
                state.NewTree=Sha(await Post(repo+"/git/trees",new {base_tree=state.BaseTree,tree=exports.Where(e=>state.Paths.Contains(e.Path,StringComparer.Ordinal)).Select(e=>new{path=e.Path,mode="100644",type="blob",content=e.Content}).ToArray()},ct).ConfigureAwait(false));
                state.Stage="tree_created";Save(journalPath,state);
                stage="commit_pending";state.Stage=stage;Save(journalPath,state);
                state.CommitSha=Sha(await Post(repo+"/git/commits",new {message="Registered project record metadata "+batch,tree=state.NewTree,parents=new[]{state.BaseSha}},ct).ConfigureAwait(false));
                state.Stage="commit_created";Save(journalPath,state);
                stage="branch_pending";state.Stage=stage;Save(journalPath,state);
                var created=await Post(repo+"/git/refs",new {@ref="refs/heads/"+branch,sha=state.CommitSha},ct).ConfigureAwait(false);
                if (Sha(created.GetProperty("object"))!=state.CommitSha || Text(created,"ref")!="refs/heads/"+branch) throw new InvalidDataException("Branch write not confirmed");
                await VerifyBranch(repo,state,exports,ct).ConfigureAwait(false);uploaded=true;state.Stage="branch_uploaded";Save(journalPath,state);
            }
            else throw new InvalidDataException("Branch lookup not confirmed");
            stage="pr_lookup";
            var pulls=await Get(repo+"/pulls?state=all&head="+Uri.EscapeDataString(r.RepositoryFullName.Split('/')[0]+":"+branch)+"&base="+r.BaseBranch+"&per_page=100",ct).ConfigureAwait(false);
            if (pulls.ValueKind!=JsonValueKind.Array || pulls.GetArrayLength()>1) throw new InvalidDataException("Ambiguous PR lookup");
            if (pulls.GetArrayLength()==1)
            {
                var pull=pulls[0]; var status=ValidatePull(pull,r,state);state.Stage="pr_open";state.PullRequestUrl=Text(pull,"html_url");Save(journalPath,state);
                return Result(status,state.PullRequestUrl);
            }
            if (state.Stage is "pr_pending" or "pr_open") {stage="pr_pending";return Result("pr_uncertain");}
            stage="pr_pending";state.Stage=stage;Save(journalPath,state);
            var pr=await Post(repo+"/pulls",new {title="Project record metadata "+r.ProjectId,head=branch,@base=r.BaseBranch,draft=true,body="Metadata-only registered records. Complete originals remain local; no acknowledgement or central sync is implied."},ct).ConfigureAwait(false);
            var prStatus=ValidatePull(pr,r,state);state.Stage="pr_open";state.PullRequestUrl=Text(pr,"html_url");Save(journalPath,state);
            return Result(prStatus,state.PullRequestUrl);
        }
        catch(Exception)
        {
            // A stage was persisted before every write. Unknown results cannot trigger blind mutation retries.
            var status=stage=="pr_pending"?"pr_uncertain":stage is "tree_pending" or "commit_pending" or "branch_pending"?"upload_uncertain":"held";
            return Result(status);
        }
    }
    private async Task VerifyBranch(string repo,Journal state,IReadOnlyList<ProjectRecordExport> exports,CancellationToken ct)
    {
        var commit=await Get(repo+"/git/commits/"+state.CommitSha,ct).ConfigureAwait(false);
        var parents=commit.GetProperty("parents");
        if (parents.GetArrayLength()!=1 || Sha(parents[0])!=state.BaseSha) throw new InvalidDataException("Unexpected commit ancestry");
        var tree=await ReadTree(repo,Sha(commit.GetProperty("tree")),ct).ConfigureAwait(false);
        foreach(var export in exports) if(!tree.TryGetValue(export.Path,out var hash)||hash!=Blob(export.Content)) throw new InvalidDataException("Remote metadata not confirmed");
        var compare=await Get(repo+"/compare/"+state.BaseSha+"..."+state.CommitSha,ct).ConfigureAwait(false);
        if (compare.GetProperty("total_commits").GetInt32()!=1 || compare.GetProperty("commits").GetArrayLength()!=1 || Sha(compare.GetProperty("commits")[0])!=state.CommitSha) throw new InvalidDataException("Unexpected branch commits");
        var changed=compare.GetProperty("files").EnumerateArray().ToArray();
        if(changed.Length!=state.Paths.Length || changed.Any(f=>Text(f,"status")!="added" || !state.Paths.Contains(Text(f,"filename"),StringComparer.Ordinal))
            || changed.Select(f=>Text(f,"filename")).Distinct(StringComparer.Ordinal).Count()!=state.Paths.Length) throw new InvalidDataException("Branch touches unapproved paths");
    }
    private async Task<Dictionary<string,string>> ReadTree(string repo,string sha,CancellationToken ct)
    {
        var tree=await Get(repo+"/git/trees/"+sha+"?recursive=1",ct).ConfigureAwait(false);
        if(tree.GetProperty("truncated").GetBoolean()) throw new InvalidDataException("Incomplete Git tree");
        var result=new Dictionary<string,string>(StringComparer.Ordinal);
        foreach(var item in tree.GetProperty("tree").EnumerateArray())
        {
            var path=Text(item,"path");
            if(!result.TryAdd(path,Text(item,"type")=="blob"&&Text(item,"mode")=="100644"?Sha(item):"not-regular-blob")) throw new InvalidDataException("Duplicate tree path");
        }
        return result;
    }
    private static string ValidatePull(JsonElement pull,ProjectRecordPublishingRegistration r,Journal state)
    {
        foreach(var section in new[]{"head","base"})
        {
            var repo=pull.GetProperty(section).GetProperty("repo");
            if(repo.GetProperty("id").GetInt64()!=r.RepositoryId||Text(repo,"full_name")!=r.RepositoryFullName) throw new InvalidDataException("PR repository mismatch");
        }
        if(Text(pull.GetProperty("head"),"ref")!=state.Branch || Sha(pull.GetProperty("head"))!=state.CommitSha || Text(pull.GetProperty("base"),"ref")!=r.BaseBranch) throw new InvalidDataException("PR branch mismatch");
        var number=pull.GetProperty("number").GetInt32();
        if(number<1||Text(pull,"html_url")!="https://github.com/"+r.RepositoryFullName+"/pull/"+number.ToString(System.Globalization.CultureInfo.InvariantCulture)) throw new InvalidDataException("PR URL mismatch");
        var status=Text(pull,"state");
        if(status=="open" && !pull.GetProperty("draft").GetBoolean()) throw new InvalidDataException("PR is not draft");
        return status=="open"?"pr_open":status=="closed"?(pull.GetProperty("merged_at").ValueKind==JsonValueKind.Null?"pr_closed":"pr_merged"):throw new InvalidDataException("PR state invalid");
    }
    private async Task<JsonElement> Get(string endpoint,CancellationToken ct)
    {var response=await _api("GET",endpoint,null,ct).ConfigureAwait(false);if(response.StatusCode!=200)throw new IOException("GitHub read not confirmed");return Parse(response.Json);}
    private async Task<JsonElement> Post(string endpoint,object body,CancellationToken ct)
    {var response=await _api("POST",endpoint,JsonSerializer.Serialize(body),ct).ConfigureAwait(false);if(response.StatusCode!=201)throw new IOException("GitHub write not confirmed");return Parse(response.Json);}
    private static JsonElement Parse(string json)
    {if(json.Length>4*1024*1024)throw new InvalidDataException("JSON response limit");using var doc=JsonDocument.Parse(json,new JsonDocumentOptions{MaxDepth=32});UniqueKeys(doc.RootElement);return doc.RootElement.Clone();}
    private static void UniqueKeys(JsonElement e)
    {
        if(e.ValueKind==JsonValueKind.Object)
        {var names=new HashSet<string>(StringComparer.Ordinal);foreach(var p in e.EnumerateObject()){if(!names.Add(p.Name))throw new InvalidDataException("Duplicate JSON field");UniqueKeys(p.Value);}}
        else if(e.ValueKind==JsonValueKind.Array)foreach(var child in e.EnumerateArray())UniqueKeys(child);
    }
    private static void ValidateRegistration(ProjectRecordPublishingRegistration r)
    {
        if(!r.Enabled||!Repositories.TryGetValue(r.ProjectId,out var expected)||r.RepositoryId!=expected.Id||r.RepositoryFullName!=expected.Name||r.BaseBranch!="main"
            ||r.SharedPrefix!="04_COMMUNICATION/shared-records/"+r.ProjectId||NormalizeOrigin(r.OriginUrl)!=expected.Name||!Path.IsPathFullyQualified(r.RootPath)) throw new InvalidDataException("Project registration required");
        SafePath(r.RootPath);var config=Path.Combine(r.RootPath,".git","config");SafePath(config);
        if(!File.Exists(config)||new FileInfo(config).Length>64*1024)throw new InvalidDataException("Local origin unavailable");
        string? section=null;var origins=new List<string>();
        foreach(var line in File.ReadLines(config))
        {
            var value=line.Trim();if(value.StartsWith('[')){section=value;continue;}
            if(section=="[remote \"origin\"]"&&Regex.IsMatch(value,@"^url\s*=",RegexOptions.CultureInvariant))origins.Add(value[(value.IndexOf('=')+1)..].Trim());
        }
        if(origins.Count!=1||NormalizeOrigin(origins[0])!=expected.Name)throw new InvalidDataException("Local origin mismatch");
    }
    private static string NormalizeOrigin(string origin)
    {
        var value=origin.StartsWith("https://github.com/",StringComparison.Ordinal)?origin[19..]:origin.StartsWith("git@github.com:",StringComparison.Ordinal)?origin[15..]:"";
        if(value.EndsWith(".git",StringComparison.Ordinal))value=value[..^4];
        return Regex.IsMatch(value,@"^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$",RegexOptions.CultureInvariant)?value:"";
    }
    private static bool Inside(string path,string root)
    {var p=Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);var r=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);return p.Equals(r,StringComparison.OrdinalIgnoreCase)||p.StartsWith(r+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);}
    private static void SafePath(string path)
    {
        var full=Path.GetFullPath(path);if(full.StartsWith(@"\\",StringComparison.Ordinal)||full.IndexOf(':',2)>=0)throw new InvalidDataException("Untrusted file path");
        for(string? current=full;current is not null;current=Path.GetDirectoryName(current))
            if((File.Exists(current)||Directory.Exists(current))&&(File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Reparse path blocked");
    }
    private static string Text(JsonElement e,string key)=>e.GetProperty(key).GetString()??throw new InvalidDataException("Missing text");
    private static string Sha(JsonElement e)
    {var value=Text(e,"sha");if(!Regex.IsMatch(value,@"^[0-9a-f]{40}$",RegexOptions.CultureInvariant))throw new InvalidDataException("Invalid Git object");return value;}
    private static string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static string Blob(string value)
    {var bytes=Encoding.UTF8.GetBytes(value);return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes("blob "+bytes.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)+"\0").Concat(bytes).ToArray())).ToLowerInvariant();}
    private static void Save(string path,Journal state)
    {
        SafePath(path);if(File.Exists(path)&&(File.GetAttributes(path)&FileAttributes.ReadOnly)!=0)throw new IOException("Protected journal");
        var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        using(var file=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){var bytes=JsonSerializer.SerializeToUtf8Bytes(state);file.Write(bytes);file.Flush(flushToDisk:true);}
        SafePath(path);File.Move(temp,path,overwrite:true); // One atomic attempt. Failure leaves evidence, no cleanup/retry/rollback.
    }
    private sealed class Journal
    {
        public Journal() { }
        public string BatchHash {get;set;}="";
        public long RepositoryId {get;set;}
        public string Branch {get;set;}="";
        public string BaseBranch {get;set;}="";
        public string BaseSha {get;set;}="";
        public string BaseTree {get;set;}="";
        public string NewTree {get;set;}="";
        public string CommitSha {get;set;}="";
        public string[] Paths {get;set;}=[];
        public string Stage {get;set;}="";
        public string? PullRequestUrl {get;set;}
    }
}

