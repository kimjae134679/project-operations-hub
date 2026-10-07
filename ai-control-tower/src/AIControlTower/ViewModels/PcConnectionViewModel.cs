using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed class PcCommand(Func<Task> action, Action<Exception>? failure = null, Func<bool>? allowed = null) : ICommand
{
    private bool _running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !_running && (allowed?.Invoke()??true);
    public async void Execute(object? parameter)
    { if (!CanExecute(parameter)) return; _running = true; CanExecuteChanged?.Invoke(this, EventArgs.Empty); try { await action(); } catch (Exception e) { failure?.Invoke(e); } finally { _running = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); } }
}
public sealed class PcJobViewModel : ObservableObject
{
    private PcJobSnapshot _value;
    public PcJobViewModel(PcJobSnapshot value) => _value = value;
    public string Id => _value.Id;
    public string Title => _value.Title;
    public string Project => _value.Project;
    public string Tool => _value.Tool;
    public string State => _value.State;
    public void Update(PcJobSnapshot value)
    {
        if (_value == value) return; _value = value;
        foreach (var p in new[]{nameof(Title),nameof(Project),nameof(Tool),nameof(State),nameof(StatusText),nameof(HasError),nameof(IsRunning)}) OnPropertyChanged(p);
    }
    public bool HasError => State is "failed" or "interrupted" or "blocked" or "timed_out";
    public bool IsRunning => State is "starting" or "started" or "running";
    public string StatusText => State switch { "queued" or "pending" => "대기", "starting" or "started" or "running" => "실행 중", "finished" or "published" or "completed" => "완료", "failed" => "오류", "interrupted" => "중단 확인 필요", "timed_out" => "시간 초과", "stopped" => "중지", "blocked" => "선행 작업 대기", _ => "미확인" };
}
public sealed class PcConnectionViewModel : ObservableObject, IDisposable
{
    private readonly ProjectBridgeService _service; private readonly SemaphoreSlim _gate = new(1,1), _operations = new(1,1); private readonly CancellationTokenSource _lifetime = new();
    private PcConnectionSnapshot _snapshot = new(false,false,false,"기기 확인 대기","unknown","연결 상태를 확인하고 있습니다.","",0,4,[]);
    private bool _working; private string _error = "", _actionMessage=""; private readonly bool _readOnly, _manualControl;
    private readonly Func<Task<bool>> _confirmInstall;
    private readonly Func<CancellationToken,Task> _install;
    private readonly CommanderStatusService _commanderService;
    private CommanderStatusSnapshot _commander=new();
    private bool _desiredConnected=true, _permissionBlocked;private int _retryFailures;
    private readonly object _ownedAdmission=new();
    private bool _ownedFrozen;
    private int _ownedActivities,_ownedMutations;
    // Local API operations only. Remote Bridge job counts are not manager-owned lifetime.
    public bool OwnedActivitiesIdle {get {lock(_ownedAdmission)return _ownedActivities==0 && _gate.CurrentCount==1 && _operations.CurrentCount==1 && !_working;}}
    public bool OwnedMutationInFlight {get {lock(_ownedAdmission)return _ownedMutations>0;}}
    public void FreezeOwnedAdmission(bool frozen){lock(_ownedAdmission)_ownedFrozen=frozen;}
    private async Task<T> WithOwnedActivityAsync<T>(Func<Task<T>> action,bool mutation)
    {
        lock(_ownedAdmission)
        {
            if(_ownedFrozen)throw new InvalidOperationException("안전 종료 확인 중 · 새 PC 요청 보류");
            _ownedActivities++;if(mutation)_ownedMutations++;
        }
        try{return await action();}
        finally{lock(_ownedAdmission){_ownedActivities--;if(mutation)_ownedMutations--;}}
    }
    private async Task WithOwnedActivityAsync(Func<Task> action,bool mutation)
    {
        try{await WithOwnedActivityAsync(async()=>{await action();return true;},mutation);}
        catch(InvalidOperationException) when(_ownedFrozen){}
    }
    public bool AutoReconnectSuppressed => !_desiredConnected || _permissionBlocked;
    public DateTimeOffset NextRetryAt { get; private set; } = DateTimeOffset.MinValue;
    public PcConnectionDiagnostics Diagnostics { get; private set; } = new();
    public PcConnectionSnapshot Snapshot => _snapshot with { Jobs=Array.AsReadOnly(_snapshot.Jobs.ToArray()) };
    public PcConnectionViewModel(ProjectBridgeService? service = null, Func<string,string,string,Task>? openDocument = null) : this(service,openDocument,false) { }
    public PcConnectionViewModel(ProjectBridgeService? service, Func<string,string,string,Task>? openDocument, bool readOnly)
        : this(service,openDocument,readOnly,false) { }
    public PcConnectionViewModel(ProjectBridgeService? service,Func<string,string,string,Task>? openDocument,bool readOnly,bool manualControl)
        : this(service,openDocument,readOnly,manualControl,null,null) { }
    public PcConnectionViewModel(ProjectBridgeService? service,Func<string,string,string,Task>? openDocument,bool readOnly,bool manualControl,
        Func<Task<bool>>? confirmInstall,Func<CancellationToken,Task>? install)
        :this(service,openDocument,readOnly,manualControl,confirmInstall,install,null) { }
    public PcConnectionViewModel(ProjectBridgeService? service,Func<string,string,string,Task>? openDocument,bool readOnly,bool manualControl,
        Func<Task<bool>>? confirmInstall,Func<CancellationToken,Task>? install,CommanderStatusService? commanderService)
    {
        _service = service ?? new(); _readOnly=readOnly;_manualControl=manualControl;
        _commanderService=commanderService??new();
        _confirmInstall=confirmInstall??(()=>Task.FromResult(MessageBox.Show("PC 연결을 설치·복구합니다. 기존 기기 ID·인증·사용자 설정과 작업 기록을 보존하며 연결 프로그램만 잠시 다시 시작합니다. 보호된 설정의 접근 권한은 변경하지 않습니다. 진행 중인 PC 작업이 있으면 먼저 마무리하세요. 계속할까요?","PC 연결 설치·복구",MessageBoxButton.OKCancel,MessageBoxImage.Question)==MessageBoxResult.OK));
        _install=install??_service.InstallAsync;
        if(IsReadOnly)_snapshot=new(false,false,false,"로컬 조회","unknown","로컬 조회 · API 확인 없음 · 현재 PC/원격 생존 미확인","",0,4,[]);
        ConnectCommand = new PcCommand(() => OperateAsync("resume"),allowed:()=>!IsReadOnly); PauseCommand = new PcCommand(() => OperateAsync("pause"),allowed:()=>!IsReadOnly); StopCommand = new PcCommand(StopAsync,allowed:()=>!IsReadOnly);
        InstallCommand = new PcCommand(InstallRepairAsync,ShowError,allowed:()=>!IsReadOnly); OpenJobsCommand = new PcCommand(() => { (Application.Current?.MainWindow as MainWindow)?.ShowPcJobs(); return Task.CompletedTask; }, ShowError);
        OpenGuideCommand = new PcCommand(() => openDocument is not null
            ? openDocument(_service.Home,Path.Combine(_service.Home,"프로젝트_사용안내.md"),"공용 PC 연결")
            : Task.FromException(new InvalidOperationException("내부 문서 읽기 연결이 필요합니다. 파일을 외부 창에서 대신 열지 않습니다.")), ShowError);
    }
    public ObservableCollection<PcJobViewModel> Jobs { get; } = [];
    public ICommand ConnectCommand { get; } public ICommand PauseCommand { get; } public ICommand StopCommand { get; } public ICommand InstallCommand { get; } public ICommand OpenJobsCommand { get; } public ICommand OpenGuideCommand { get; }
    public bool IsReadOnly => _readOnly && !_manualControl;
    public string DeviceLabel => _snapshot.Device;
    public bool IsInstalled => _snapshot.Installed;
    public bool IsConnected => _snapshot.Connected;
    public bool HasError => _error.Length > 0 || _snapshot.Stage is "error" or "offline" or "stale" or "permission_denied";
    public bool IsBusy => _working || ActiveJobCount > 0;
    public int ActiveJobCount => _snapshot.ActiveJobs;
    public int ParallelLimit => _snapshot.ParallelLimit;
    public string StatusText => IsReadOnly ? "로컬 조회 · 현재 연결 미확인" : _working ? "처리 중" : _error.Length > 0 ? "확인 필요" : _snapshot.Connected ? "PC 연결됨" : _snapshot.Stage switch { "not_installed" => "미설치", "upgrade_required" => "통합 업데이트 필요", "paused" or "disconnected" => "일시정지", "stopped" => "연결 종료", "permission_denied"=>"인증 확인 필요 · 자동 재시도 중지", "offline" or "stale" or "error" => "연결 확인 필요", _ => "연결 대기" };
    public string DetailText => _error.Length > 0 ? _error : _actionMessage.Length>0?_actionMessage:_snapshot.Detail;
    public string RelayStatusText => IsReadOnly ? "로컬 조회 · 원격/API 상태 미확인 · 기존 연결은 변경하지 않습니다." : _snapshot.RelayConnected ? "다른 GPT의 원격 요청 통로도 연결됨" : _snapshot.RelayCode switch { "github_login_required" => "원격 요청 통로: GitHub 로그인 필요", "github_http_404" => "원격 요청 통로: 저장소 접근 또는 주소 확인 필요", _ => "원격 요청 통로 대기 · PC 안의 도구는 로컬 연결로 작업합니다." };
    public string CommanderStatusText=>IsReadOnly?"Desktop Commander · 로컬 조회 모드에서는 현재 상태 미확인":_commander.DisplayText;
    public string CommanderRecoveryText=>IsReadOnly?"자동 복구 감시 상태 미확인":_commander.RecoveryText;
    public string BridgeStatusText=>IsReadOnly?"ProjectBridge · 현재 연결 미확인":"ProjectBridge · PC 안의 명령 통로: "+(IsConnected?"사용 가능":"연결 확인 필요")+" · 원격 GitHub 중계: "+(_snapshot.RelayConnected?"연결됨":"연결 확인 필요");
    public string PrimaryPathText=>IsReadOnly?"작업 실행 없음 · 읽기 전용":IsConnected?"기본 작업 통로: ProjectBridge · Desktop Commander 잔여량이 0%여도 이 통로로 PC 작업을 실행합니다.":"ProjectBridge 작업 통로 미확인 · PC 연결 시작을 누르세요. 설치가 없거나 손상되었으면 설치·복구를 사용하세요. 다른 통로의 성공은 보장하지 않습니다.";
    private void Notify() { foreach(var p in new[]{nameof(DeviceLabel),nameof(IsInstalled),nameof(IsConnected),nameof(HasError),nameof(IsBusy),nameof(ActiveJobCount),nameof(ParallelLimit),nameof(StatusText),nameof(DetailText),nameof(RelayStatusText),nameof(AutoReconnectSuppressed),nameof(NextRetryAt),nameof(CommanderStatusText),nameof(CommanderRecoveryText),nameof(BridgeStatusText),nameof(PrimaryPathText)}) OnPropertyChanged(p); }
    private void ShowError(Exception e) { _error = ProcessRunner.Sanitize(e.Message); Notify(); }
    public Task PollAsync()=>WithOwnedActivityAsync(PollCoreAsync,false);
    private async Task PollCoreAsync()
    {
        if(IsReadOnly){Notify();return;}
        if (!await _gate.WaitAsync(0)) return;
        var started=DateTimeOffset.UtcNow;var elapsed=System.Diagnostics.Stopwatch.StartNew();var cancelled=false;var unexpected=false;
        Diagnostics=Diagnostics with {AttemptStartedAtUtc=started,AttemptCompletedAtUtc=null,ElapsedMilliseconds=0,InFlight=true};
        OnPropertyChanged(nameof(Diagnostics));
        try
        {
            _snapshot = _manualControl?await _service.CheckApiOnlyAsync(_lifetime.Token):await _service.CheckAsync(_lifetime.Token);
            _commander=_commanderService.Read(DateTimeOffset.UtcNow);
            if(_manualControl&&_snapshot.Stage=="permission_denied")_permissionBlocked=true;
            var ids = _snapshot.Jobs.Select(j=>j.Id).ToHashSet(StringComparer.Ordinal);
            for(var i=Jobs.Count-1;i>=0;i--) if(!ids.Contains(Jobs[i].Id)) Jobs.RemoveAt(i);
            for(var index=0;index<_snapshot.Jobs.Count;index++)
            {
                var value=_snapshot.Jobs[index];var row=Jobs.FirstOrDefault(j=>j.Id==value.Id);
                if(row is null){Jobs.Insert(index,new PcJobViewModel(value));continue;}
                row.Update(value);var oldIndex=Jobs.IndexOf(row);if(oldIndex!=index)Jobs.Move(oldIndex,index);
            }
            Notify();
        }
        catch(OperationCanceledException) { cancelled=true; }
        catch { unexpected=true;throw; }
        finally
        {
            var finished=DateTimeOffset.UtcNow;elapsed.Stop();
            var reason=cancelled?"cancelled":unexpected?"unexpected_error":PcConnectionDiagnostics.SafeReason(_snapshot.StatusReasonCode);
            Diagnostics=Diagnostics with {Stage=cancelled?"cancelled":unexpected?"error":PcConnectionDiagnostics.SafeStage(_snapshot.Stage),
                ReasonCode=reason,AttemptCompletedAtUtc=finished,ElapsedMilliseconds=Math.Clamp(elapsed.ElapsedMilliseconds,0,86_400_000),InFlight=false,
                NextRetryAtUtc=NextRetryAt==DateTimeOffset.MinValue?null:NextRetryAt,
                LastSuccessAtUtc=reason=="status_confirmed"?finished:Diagnostics.LastSuccessAtUtc};
            _gate.Release();OnPropertyChanged(nameof(Diagnostics));
        }
    }
    public Task MaintainConnectionAsync(DateTimeOffset now)=>WithOwnedActivityAsync(()=>MaintainConnectionCoreAsync(now),true);
    private async Task MaintainConnectionCoreAsync(DateTimeOffset now)
    {
        if(!_manualControl || _lifetime.IsCancellationRequested || _permissionBlocked || now<NextRetryAt)return;
        bool acquired;
        try{acquired=await _operations.WaitAsync(0,_lifetime.Token);}catch(OperationCanceledException){return;}
        if(!acquired)return;
        try
        {
            await PollAsync();
            if(_permissionBlocked)return;
            if(_snapshot.Stage=="stopped")_desiredConnected=false; // Never override an explicit stop observed from another tool.
            if(_desiredConnected&&_snapshot.Stage is ("paused" or "disconnected"))
            {
                await _service.ControlApiOnlyAsync("resume",_lifetime.Token);await PollAsync();
            }
            _retryFailures=_snapshot.Stage is "offline" or "stale" ?Math.Min(5,_retryFailures+1):0;
            NextRetryAt=now.AddSeconds(_retryFailures==0?2:Math.Min(30,Math.Pow(2,_retryFailures)));
        }
        catch(OperationCanceledException){}
        catch(Exception e)
        {
            if(e is UnauthorizedAccessException||e is System.Net.Http.HttpRequestException{StatusCode:System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized})
            {_permissionBlocked=true;_error="PC 로컬 API 권한 거부 · 자동 재시도/복구를 중지했습니다.";}
            else{_retryFailures=Math.Min(5,_retryFailures+1);NextRetryAt=now.AddSeconds(Math.Min(30,Math.Pow(2,_retryFailures)));_error="연결 유지 응답을 확인하지 못했습니다. 기존 실행기는 변경하지 않고 제한된 간격으로 다시 확인합니다.";}
        }
        finally{Diagnostics=Diagnostics with {NextRetryAtUtc=NextRetryAt==DateTimeOffset.MinValue?null:NextRetryAt};_operations.Release();OnPropertyChanged(nameof(Diagnostics));Notify();}
    }
    private Task RunAsync(Func<Task> action)=>WithOwnedActivityAsync(()=>RunCoreAsync(action),true);
    private async Task RunCoreAsync(Func<Task> action)
    {
        if(IsReadOnly){_error="로컬 조회 · PC 실행/설치/중지는 보류합니다.";Notify();return;}
        if(_manualControl)await _operations.WaitAsync(_lifetime.Token);else if (!await _operations.WaitAsync(0)) return;
        _working = true; _error = ""; _actionMessage=""; Notify();
        try { await action(); }
        catch(OperationCanceledException) { _actionMessage="요청 취소 · 설치 시작 이후의 결과는 연결 상태와 설치 기록에서 확인하세요."; }
        catch(Exception e) { _error = ProcessRunner.Sanitize(e.Message); }
        finally { _working = false; _operations.Release(); await PollAsync(); Notify(); }
    }
    private Task OperateAsync(string action)
    {
        lock(_ownedAdmission)if(_ownedFrozen)return Task.CompletedTask;
        if(_manualControl){_desiredConnected=action=="resume";if(action=="resume"){_permissionBlocked=false;NextRetryAt=DateTimeOffset.MinValue;}_error="";Notify();}
        return RunAsync(async()=> {if(_manualControl){if(action=="resume"){await _service.ResumeExistingAsync(_lifetime.Token);_actionMessage="PC 연결 시작 요청 · 실제 응답을 확인하고 있습니다.";}else await _service.ControlApiOnlyAsync(action,_lifetime.Token);}else {await _service.ControlAsync(action,_lifetime.Token);await Task.Delay(500,_lifetime.Token);}});
    }
    private Task StopAsync()
    {
        if(IsReadOnly)return Task.CompletedTask;
        if(_manualControl)return OperateAsync("stop");
        if (MessageBox.Show("이번 로그인 동안 새 PC 요청을 받지 않습니다. 진행 중인 작업은 해당 작업 정책에 따라 마무리하며, 다음 로그인에는 다시 연결합니다.","PC 연결 끄기",MessageBoxButton.OKCancel,MessageBoxImage.Question)!=MessageBoxResult.OK) return Task.CompletedTask;
        return OperateAsync("stop");
    }
    // Only this explicit command invokes installation. Startup/polling remain API-only.
    public Task InstallRepairAsync() => RunAsync(async()=>
    {
        await PollAsync();
        if(ActiveJobCount>0){_actionMessage="진행 중인 PC 작업이 있어 설치·복구를 보류했습니다. 작업이 끝난 뒤 다시 눌러 주세요.";return;}
        if(!await _confirmInstall()){_actionMessage="설치·복구 취소 · 기존 연결과 설정을 변경하지 않았습니다.";return;}
        _lifetime.Token.ThrowIfCancellationRequested();
        await PollAsync();
        if(ActiveJobCount>0){_actionMessage="진행 중인 PC 작업이 확인되어 설치·복구를 보류했습니다.";return;}
        await _install(_lifetime.Token);
        await PollAsync();
        _actionMessage=IsConnected?"설치·복구 완료 · ProjectBridge PC 연결 확인됨":"설치·복구 완료 · 현재 PC 연결은 확인 대기입니다. PC 연결 시작으로 복구하고 상태를 확인하세요.";
    });
    public Task<string> SubmitAsync(string project, string tool, string action, object args) => WithOwnedActivityAsync(()=>IsReadOnly ? Task.FromException<string>(new InvalidOperationException("로컬 조회 · PC 명령 제출 보류")) : _manualControl?_service.SubmitApiOnlyAsync(project,tool,action,args,_lifetime.Token):_service.SubmitAsync(project,tool,action,args,_lifetime.Token),true);
    public Task<string> ResultAsync(string id) => ResultAsync(id,CancellationToken.None);
    public Task<string> ResultAsync(string id,CancellationToken ct)=>WithOwnedActivityAsync(()=>ResultCoreAsync(id,ct),false);
    private async Task<string> ResultCoreAsync(string id,CancellationToken ct)
    {
        if(IsReadOnly)throw new InvalidOperationException("로컬 조회 · 외부 API 결과 확인 없음");
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token,ct);
        linked.Token.ThrowIfCancellationRequested();
        return await _service.ResultAsync(id,linked.Token).ConfigureAwait(false);
    }
    public void Dispose() { _lifetime.Cancel(); _service.Dispose(); }
}
