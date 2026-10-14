using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed record RecordPublishingRequest(bool Requested,bool Valid,string? ProjectId);

/// <summary>Explicit D-only registrations; local collection and remote metadata publication remain separate.</summary>
public sealed class RegisteredRecordPublishing
{
    public const string RegistryPath=@"D:\A_KJ\AI\ControlTowerData\record-publishing\registrations.json";
    private static readonly Dictionary<string,(long Id,string Name)> Repositories=new(StringComparer.Ordinal)
    {
        ["Threads"]=(1368308612,"kimjae134679/Threads"),
        ["Mushoku-Audiobook"]=(1404685845,"kimjae134679/Mushoku-Tensei-AI-Audiobook"),
        ["Control-Tower"]=(1362083625,"kimjae134679/project-operations-hub"),
        ["PhoneLOL"]=(1378550871,"kimjae134679/PhoneLoL_02-Source")
    };
    private sealed record Registry
    {
        public int SchemaVersion{get;init;}
        public string BoardRoot{get;init;}="";
        public ProjectRecordPublishingRegistration[] Registrations{get;init;}=[];
    }
    private readonly object _gate=new();
    private readonly string _registryPath;
    private readonly TimeSpan _timeout;
    private Registry? _registry;
    private string? _fingerprint;
    private bool _tampered,_running;
    private readonly Dictionary<string,string> _attempts=new(StringComparer.Ordinal);
    private readonly List<ProjectRecordPublishingResult> _latestResults=[];
    public bool IsRunning{get{lock(_gate)return _running;}}
    public IReadOnlyList<ProjectRecordPublishingResult> LatestResults{get{lock(_gate)return _latestResults.ToArray();}}
    public RegisteredRecordPublishing():this(RegistryPath,TimeSpan.FromMinutes(3)){}
    public RegisteredRecordPublishing(string registryPath,TimeSpan timeout)
    {
        _registryPath=registryPath;
        if(timeout<=TimeSpan.Zero||timeout>TimeSpan.FromMinutes(3))throw new ArgumentOutOfRangeException(nameof(timeout));
        _timeout=timeout;
    }
    public static bool RequestsOnce(IEnumerable<string> args)=>args.Any(a=>a.StartsWith("--publish-records",StringComparison.OrdinalIgnoreCase));
    public static RecordPublishingRequest ParseOnce(string[] args,IEnumerable<string> registeredIds)
    {
        var requested=RequestsOnce(args);
        var valid=requested&&args.Length==2&&args[0]=="--publish-records-once"&&Repositories.ContainsKey(args[1])&&registeredIds.Contains(args[1],StringComparer.Ordinal);
        return new(requested,valid,valid?args[1]:null);
    }
    public IReadOnlyList<ProjectRecordPublishingRegistration> ReadRegistrations()
    {
        lock(_gate)
        {
            if(_tampered)throw new InvalidDataException("registry_changed");
            CheckPath(_registryPath,false);
            var info=new FileInfo(_registryPath);if(!info.Exists||info.Length>65536)throw new InvalidDataException("registry_invalid");
            var bytes=File.ReadAllBytes(_registryPath);if(bytes.Length>65536)throw new InvalidDataException("registry_invalid");
            var hash=Convert.ToHexString(SHA256.HashData(bytes));
            if(_fingerprint is not null&&hash!=_fingerprint){_tampered=true;throw new InvalidDataException("registry_changed");}
            if(_registry is not null)return _registry.Registrations.ToArray();
            using var document=JsonDocument.Parse(bytes);RejectDuplicateKeys(document.RootElement);
            var json=new UTF8Encoding(false,true).GetString(bytes);
            var data=JsonSerializer.Deserialize<Registry>(json,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow})??throw new InvalidDataException("registry_invalid");
            if(data.SchemaVersion!=1||data.Registrations.Length>4)throw new InvalidDataException("registry_invalid");
            CheckPath(data.BoardRoot,true);
            var identities=new HashSet<string>(StringComparer.Ordinal);
            foreach(var r in data.Registrations)
            {
                if(!Repositories.TryGetValue(r.ProjectId,out var expected)||!identities.Add(r.ProjectId)||r.RepositoryId!=expected.Id||r.RepositoryFullName!=expected.Name||r.BaseBranch!="main"||r.SharedPrefix!="04_COMMUNICATION/shared-records/"+r.ProjectId||
                   (r.OriginUrl!="https://github.com/"+expected.Name&&r.OriginUrl!="https://github.com/"+expected.Name+".git"&&r.OriginUrl!="git@github.com:"+expected.Name+".git"))throw new InvalidDataException("registration_invalid");
                if(!ValidateProjectSource(r.ProjectId,r.RootPath))throw new InvalidDataException("project_source_invalid");
                CheckPath(r.RootPath,true,allowExistingThreads:r.ProjectId=="Threads");
            }
            _fingerprint=hash;_registry=data;return data.Registrations.ToArray();
        }
    }
    private static void RejectDuplicateKeys(JsonElement element)
    {
        if(element.ValueKind==JsonValueKind.Object)
        {
            var keys=new HashSet<string>(StringComparer.Ordinal);
            foreach(var p in element.EnumerateObject()){if(!keys.Add(p.Name))throw new InvalidDataException("registry_invalid");RejectDuplicateKeys(p.Value);}
        }
        else if(element.ValueKind==JsonValueKind.Array)foreach(var item in element.EnumerateArray())RejectDuplicateKeys(item);
    }
    public static bool ValidateProjectSource(string projectId,string path)
    {
        if(!Repositories.ContainsKey(projectId)||!Path.IsPathFullyQualified(path))return false;
        try
        {
            var full=Path.GetFullPath(path);
            if(!string.Equals(full.TrimEnd('\\'),path.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase)||full.Split(new[]{Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar}).Any(p=>p.Equals("Desktop",StringComparison.OrdinalIgnoreCase)||p.Equals("바탕 화면",StringComparison.OrdinalIgnoreCase)))return false;
            return full.StartsWith(@"D:\",StringComparison.OrdinalIgnoreCase)||(projectId=="Threads"&&full.TrimEnd('\\').Equals(@"C:\KJ\Github\Threads",StringComparison.OrdinalIgnoreCase));
        }
        catch(ArgumentException){return false;}
    }
    private static void CheckPath(string path,bool directory,bool allowExistingThreads=false)
    {
        if(!Path.IsPathFullyQualified(path)||(!path.StartsWith(@"D:\",StringComparison.OrdinalIgnoreCase)&&!(allowExistingThreads&&ValidateProjectSource("Threads",path))))throw new InvalidDataException("path_invalid");
        var full=Path.GetFullPath(path);
        if(!string.Equals(full.TrimEnd('\\'),path.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase)||full.Split(['\\','/']).Any(p=>p.Equals("Desktop",StringComparison.OrdinalIgnoreCase)||p.Equals("바탕 화면",StringComparison.OrdinalIgnoreCase)))throw new InvalidDataException("path_invalid");
        if(directory&&!Directory.Exists(full))throw new InvalidDataException("path_missing");
        for(var current=full;!string.IsNullOrEmpty(current);current=Path.GetDirectoryName(current))
            if((File.Exists(current)||Directory.Exists(current))&&(File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("path_linked");
    }
    private static string BatchKey(string project,CommunicationCollectionResult collection)
    {
        var values=collection.Records.Where(r=>r.ProjectId==project).Select(r=>r.Kind+"\n"+r.SourcePath+"\n"+r.ContentSha256).Order(StringComparer.Ordinal);
        return project+":"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n",values))));
    }
    public Task<string> PublishAsync(bool manual,CommunicationCollectionResult collection,
        Func<string,CommunicationCollectionResult,CancellationToken,Task<string>> publish,CancellationToken ct=default)
        =>PublishCoreAsync(manual,collection,publish,ct,null);
    private async Task<string> PublishCoreAsync(bool manual,CommunicationCollectionResult collection,
        Func<string,CommunicationCollectionResult,CancellationToken,Task<string>> publish,CancellationToken ct,string? onlyProject)
    {
        if(!manual)return "disabled";
        if(ct.IsCancellationRequested)return "held";
        ProjectRecordPublishingRegistration[] registrations;
        try{registrations=ReadRegistrations().Where(r=>r.Enabled&&(onlyProject is null||r.ProjectId==onlyProject)).ToArray();}catch(Exception){return "held";}
        if(registrations.Length==0)return "disabled";
        if(collection.Errors.Any(e=>e.Code is "manifest_invalid" or "collection_blocked"))return "held";
        var outcomes=new List<string>();
        lock(_gate)
        {
            if(_running)return "inflight";
            var statuses=registrations.Select(r=>_attempts.GetValueOrDefault(BatchKey(r.ProjectId,collection))).ToArray();
            if(statuses.All(s=>s is not null))return statuses.All(IsSuccessful)?"cached":"held";
            outcomes.AddRange(statuses.Where(s=>s is not null).Select(s=>s!));
            // A failure/uncertain transaction holds that project, even if another new batch arrives.
            registrations=registrations.Where(r=>!_attempts.Any(a=>a.Key.StartsWith(r.ProjectId+":",StringComparison.Ordinal)&&!IsSuccessful(a.Value))).Where(r=>!_attempts.ContainsKey(BatchKey(r.ProjectId,collection))).ToArray();
            if(registrations.Length==0)return "held";
            _running=true;
        }
        var linked=CancellationTokenSource.CreateLinkedTokenSource(ct);linked.CancelAfter(_timeout);
        var work=Task.Run(async()=>
        {
            var status="empty";
            foreach(var registration in registrations)
            {
                linked.Token.ThrowIfCancellationRequested();
                // Revalidate immutable registration bytes immediately before every attempt.
                ReadRegistrations();
                var key=BatchKey(registration.ProjectId,collection);
                lock(_gate)_attempts[key]="upload_uncertain";
                var selected=new CommunicationCollectionResult(collection.Records.Where(r=>r.ProjectId==registration.ProjectId).ToArray(),
                    collection.Errors.Where(e=>e.ProjectId is null||string.Equals(e.ProjectId,registration.ProjectId,StringComparison.Ordinal)).ToArray(),collection.CollectedAt);
                status=await publish(registration.ProjectId,selected,linked.Token).ConfigureAwait(false);
                linked.Token.ThrowIfCancellationRequested();
                if(status is not("held" or "empty" or "branch_uploaded" or "upload_uncertain" or "pr_uncertain" or "pr_open" or "pr_closed" or "pr_merged" or "already_present"))status="upload_uncertain";
                lock(_gate)_attempts[key]=status;
                outcomes.Add(status);
            }
            return new[]{"upload_uncertain","pr_uncertain","held","branch_uploaded","pr_open","pr_merged","pr_closed","already_present","empty"}.First(s=>outcomes.Contains(s,StringComparer.Ordinal));
        },CancellationToken.None);
        try{return await work.WaitAsync(linked.Token).ConfigureAwait(false);}
        catch(Exception){return "upload_uncertain";}
        finally
        {
            if(work.IsCompleted){_ = work.Exception;linked.Dispose();lock(_gate)_running=false;}
            else _=work.ContinueWith(t=>{_ = t.Exception;linked.Dispose();lock(_gate)_running=false;},CancellationToken.None,TaskContinuationOptions.ExecuteSynchronously,TaskScheduler.Default);
        }
    }
    private static bool IsSuccessful(string? status)=>status is "pr_open" or "pr_closed" or "pr_merged" or "branch_uploaded" or "empty" or "already_present";
    public Task<string> PublishRegisteredAsync(bool manual,CommunicationCollectionResult collection,CancellationToken ct=default)
        =>PublishAsync(manual,collection,async(project,selected,token)=>
        {
            var registration=ReadRegistrations().Single(r=>r.ProjectId==project&&r.Enabled);
            var result=await new ProjectRecordPublisher().PublishAsync(registration,selected,token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            lock(_gate){_latestResults.RemoveAll(r=>r.ProjectId==project);_latestResults.Add(result);}
            return result.Status;
        },ct);
    public async Task<int> RunOnceAsync(string[] args,CancellationToken ct=default)
    {
        ProjectRecordPublishingRegistration[] registrations;
        try{registrations=ReadRegistrations().Where(r=>r.Enabled).ToArray();}catch(Exception){WriteConsoleOutcome(new{Status="invalid_registration",ExitCode=2});return 2;}
        var request=ParseOnce(args,registrations.Select(r=>r.ProjectId));if(!request.Valid){WriteConsoleOutcome(new{Status="invalid_request",ExitCode=2});return 2;}
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(ct);deadline.CancelAfter(_timeout);
        string status;
        try
        {
            var registration=registrations.Single(r=>r.ProjectId==request.ProjectId);
            var boardRoot=_registry!.BoardRoot;
            var collection=await new CommunicationService().CollectOnlyAsync(boardRoot,[new(registration.ProjectId,registration.RootPath)],ManualCommunicationCollection.SinkRoot,deadline.Token).ConfigureAwait(false);
            // One-shot must not publish the other enabled registrations.
            status=await PublishCoreAsync(true,collection,async(project,selected,token)=>
            {
                if(project!=request.ProjectId)return "held";
                var current=ReadRegistrations().Single(r=>r.ProjectId==project&&r.Enabled);
                var result=await new ProjectRecordPublisher().PublishAsync(current,selected,token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            lock(_gate){_latestResults.RemoveAll(r=>r.ProjectId==project);_latestResults.Add(result);}
                return result.Status;
            },deadline.Token,request.ProjectId).ConfigureAwait(false);
        }
        catch(Exception){status="held";}
        var receipt=new{ProjectId=request.ProjectId,Status=status,CentralShared=false,CreatesAcknowledgement=false,UpdatedUtc=DateTimeOffset.UtcNow,Results=LatestResults};
        WriteConsoleOutcome(receipt);
        try
        {
            var root=Path.GetDirectoryName(RegistryPath)!;CheckPath(root,true);
            var file=Path.Combine(root,"once-"+request.ProjectId+".json");
            CheckPath(file,false);
            var temp=file+"."+Guid.NewGuid().ToString("N")+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(receipt),new UTF8Encoding(false));File.Move(temp,file,true);
        }
        catch(Exception){return 1;}
        return IsSuccessful(status)||status=="cached"?0:1;
    }    public static void WriteConsoleOutcome(object receipt)
    {
        try
        {
            // WinExe: honor redirected native stdout; no console allocation or window.
            using var writer=new StreamWriter(Console.OpenStandardOutput(),new UTF8Encoding(false),1024,leaveOpen:true){AutoFlush=true};
            writer.WriteLine(JsonSerializer.Serialize(receipt));
        }
        catch(IOException){}
    }
}
