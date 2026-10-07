using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed record ManualCollectionRefresh(string Status,CommunicationCollectionResult? Collection);

/// <summary>Collect-only scheduling and evidence projection; never central transport or acknowledgement.</summary>
public sealed class ManualCommunicationCollection
{
    public const string SinkRoot=@"D:\A_KJ\AI\ControlTowerData\collected-communication\manager-20261007";
    public const string PathPrefix="__local_collection/";
    private readonly object _gate=new();
    private readonly TimeSpan _throttle,_timeout;
    private readonly Func<DateTimeOffset> _clock;
    private DateTimeOffset? _lastAttempt;
    private bool _running;
    private CommunicationCollectionResult? _latest;
    public ManualCommunicationCollection(TimeSpan throttle,TimeSpan timeout,Func<DateTimeOffset> clock)
    {
        if(throttle<TimeSpan.Zero||timeout<=TimeSpan.Zero||timeout>TimeSpan.FromMinutes(3))throw new ArgumentOutOfRangeException(nameof(timeout));
        _throttle=throttle;_timeout=timeout;_clock=clock;
    }
    public bool IsRunning {get {lock(_gate)return _running;}}
    public CommunicationCollectionResult? Latest {get {lock(_gate)return _latest;}}
    public async Task<ManualCollectionRefresh> RefreshAsync(bool manual,bool force,
        Func<CancellationToken,Task<CommunicationCollectionResult>> collect,CancellationToken ct=default)
    {
        if(!manual)return new("disabled",null);
        lock(_gate)
        {
            if(_running)return new("inflight",_latest);
            var now=_clock();
            if(!force&&_lastAttempt is {} last&&now-last<_throttle)return new("cached",_latest);
            if(ct.IsCancellationRequested)return new("blocked",_latest);
            _running=true;_lastAttempt=now;
        }
        var bounded=CancellationTokenSource.CreateLinkedTokenSource(ct);bounded.CancelAfter(_timeout);
        var work=Task.Run(()=>collect(bounded.Token),bounded.Token);
        try
        {
            var result=await work.WaitAsync(bounded.Token).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            if(result.Errors.Any(e=>e.Code is "collection_blocked" or "manifest_invalid"))return new("blocked",Latest);
            lock(_gate)_latest=result;
            return new("completed",result);
        }
        catch(Exception) {return new("blocked",Latest);}
        finally
        {
            if(work.IsCompleted){if(work.IsFaulted)_=work.Exception;bounded.Dispose();Release();}
            else
                // A cancellation-ignoring reader must retain the gate until its actual end.
                // Observe any late fault, but never publish a result after the deadline.
                _=work.ContinueWith(t=>{if(t.IsFaulted)_=t.Exception;bounded.Dispose();Release();},CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,TaskScheduler.Default);
        }
    }
    private void Release(){lock(_gate)_running=false;}
    public static bool IsCollectedPath(string? path)=>path is not null&&path.Replace('\\','/').Contains(PathPrefix,StringComparison.Ordinal);
    public static string SafeDisplayBody(string body)
    {
        var clean=new string(body.Take(CommunicationService.MaximumFileBytes).Where(c=>!char.IsControl(c)||c is '\n' or '\r' or '\t').ToArray());
        try{return Regex.Replace(clean,@"!?\[([^\]]*)\]\([^)]+\)","$1",RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(200));}
        catch(RegexMatchTimeoutException){return clean.Replace("](","] (",StringComparison.Ordinal);}
    }
    public static CommunicationSnapshot Merge(CommunicationSnapshot board,CommunicationCollectionResult collection)
    {
        var items=board.InboxItems.Where(i=>!IsCollectedPath(i.CentralPath)).ToList();
        var errors=board.Errors.Concat(collection.Errors).ToList();var seen=new HashSet<string>(StringComparer.Ordinal);var bodyBytes=0;
        foreach(var record in collection.Records.Take(3000))
        {
            var size=Encoding.UTF8.GetByteCount(record.Body);
            if(size>256*1024||bodyBytes+size>8*1024*1024)
            {errors.Add(new(record.ProjectId,"collection_display_limit","로컬 수집 본문 표시 한도 · 원본은 보존합니다."));continue;}
            bodyBytes+=size;
            // Content-addressed copies can share StoredPath; source/kind/hash remain distinct.
            var identity=record.ProjectId+"\n"+record.Kind+"\n"+record.SourcePath+"\n"+record.ContentSha256;
            var key=PathPrefix+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity))).ToLowerInvariant();
            if(!seen.Add(key))continue;
            var kind=record.Kind switch {"outbox"=>"보낼자료","notice-receipt"=>"기존 확인기록","received-notice"=>"받은공지",_=>"자료"};
            var name=$"로컬 수집 · {kind} · {record.SourceName} · 원본 {record.SourcePath}";
            var first=record.Body.Split('\n').FirstOrDefault(s=>!string.IsNullOrWhiteSpace(s))?.Trim().TrimStart('#').Trim()??record.SourceName;
            items.Add(new(record.ProjectId,record.ContentSha256,name,key,record.CollectedAt)
            {Body=record.Body,Title=ProcessRunner.Sanitize(first.Length>160?first[..160]:first),Preview="로컬 수집 · 중앙 공유 없음"});
        }
        if(collection.Records.Count>3000)errors.Add(new(null,"collection_display_limit","로컬 수집 목록 표시 한도 · 원본은 보존합니다."));
        // Actual collected receipt bodies are evidence rows, not synthesized current confirmations.
        return new(board.Notices,board.Receipts,board.ProjectStates,items,errors,collection.CollectedAt,[]);
    }
}
