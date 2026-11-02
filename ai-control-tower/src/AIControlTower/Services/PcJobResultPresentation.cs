namespace AIControlTower.Services;

/// <summary>Selection-scoped read presentation. It never submits, retries or stops a PC job.</summary>
public sealed class PcJobResultPresentation
{
    private readonly Func<string,CancellationToken,Task<string>> _read;
    private readonly TimeSpan _budget;
    private Task<string>? _inFlight;
    private CancellationTokenSource? _selection;
    private long _generation;
    public string SelectedId {get;private set;}="";
    public string State {get;private set;}="idle";
    public string Text {get;private set;}="작업 선택";
    public event Action? Changed;
    public PcJobResultPresentation(Func<string,Task<string>> read,TimeSpan budget):this((id,_)=>read(id),budget) { }
    public PcJobResultPresentation(Func<string,CancellationToken,Task<string>> read,TimeSpan budget)
    {
        if(budget<=TimeSpan.Zero||budget>TimeSpan.FromSeconds(3))throw new ArgumentOutOfRangeException(nameof(budget));
        _read=read;_budget=budget;
    }
    public void Cancel()
    {
        _generation++;_selection?.Cancel();
        Set("unavailable",SelectedId.Length==0?"작업 선택":"결과 조회 대기");
    }
    public async Task RefreshAsync(string? jobId,bool allowRead,bool active,CancellationToken ct=default)
    {
        var id=jobId??"";var changed=SelectedId!=id;
        if(changed){_generation++;_selection?.Cancel();SelectedId=id;}
        if(id.Length==0){Set("idle","작업 선택");return;}
        if(!allowRead||!active){_generation++;_selection?.Cancel();Set("unavailable",allowRead?"결과 조회 대기":"조회 전용 · 결과 미조회");return;}
        if(_inFlight is {IsCompleted:false})
        {
            if(changed||State is not("timeout" or "loading"))Set("waiting","이전 조회 대기");
            return;
        }
        _selection?.Dispose();_selection=CancellationTokenSource.CreateLinkedTokenSource(ct);var token=_selection.Token;
        var generation=_generation;Set("loading","결과 확인 중…");
        Task<string>? operation=null;
        try
        {
            operation=_read(id,token);_inFlight=operation;
            // Also guard readers which ignore cancellation. Retain the gate until their actual end.
            var value=await operation.WaitAsync(_budget,token);
            if(Current(generation,id))Set("ready",value);
        }
        catch(TimeoutException)
        {if(Current(generation,id)){_selection?.Cancel();Set("timeout","결과 조회 시간 초과");}}
        catch(OperationCanceledException)
        {if(Current(generation,id))Set("unavailable","결과 조회 취소됨");}
        catch(Exception e)
        {if(Current(generation,id))Set("error",ProcessRunner.Sanitize(e.Message));}
        finally
        {
            // Observe any late fault without letting it update another selection or start another request.
            if(operation is not null)
                _=operation.ContinueWith(t=>{if(t.IsFaulted)_=t.Exception;},CancellationToken.None,TaskContinuationOptions.ExecuteSynchronously,TaskScheduler.Default);
        }
    }
    private bool Current(long generation,string id)=>generation==_generation&&id==SelectedId;
    private void Set(string state,string text){State=state;Text=text;Changed?.Invoke();}
}
