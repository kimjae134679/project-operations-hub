using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed class PcCommand(Func<Task> action, Action<Exception>? failure = null) : ICommand
{
    private bool _running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !_running;
    public async void Execute(object? parameter)
    { if (_running) return; _running = true; CanExecuteChanged?.Invoke(this, EventArgs.Empty); try { await action(); } catch (Exception e) { failure?.Invoke(e); } finally { _running = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); } }
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
    private bool _working; private string _error = "";
    public PcConnectionViewModel(ProjectBridgeService? service = null)
    {
        _service = service ?? new(); ConnectCommand = new PcCommand(() => OperateAsync("resume")); PauseCommand = new PcCommand(() => OperateAsync("pause")); StopCommand = new PcCommand(StopAsync);
        InstallCommand = new PcCommand(InstallAsync); OpenJobsCommand = new PcCommand(() => { (Application.Current?.MainWindow as MainWindow)?.ShowPcJobs(); return Task.CompletedTask; }, ShowError);
        OpenGuideCommand = new PcCommand(() => { new ProjectGuideWindow("공용 PC 연결", _service.Home) { Owner = Application.Current?.MainWindow }.Show(); return Task.CompletedTask; }, ShowError);
    }
    public ObservableCollection<PcJobViewModel> Jobs { get; } = [];
    public ICommand ConnectCommand { get; } public ICommand PauseCommand { get; } public ICommand StopCommand { get; } public ICommand InstallCommand { get; } public ICommand OpenJobsCommand { get; } public ICommand OpenGuideCommand { get; }
    public string DeviceLabel => _snapshot.Device;
    public bool IsInstalled => _snapshot.Installed;
    public bool IsConnected => _snapshot.Connected;
    public bool HasError => _error.Length > 0 || _snapshot.Stage is "error" or "offline" or "stale";
    public bool IsBusy => _working || ActiveJobCount > 0;
    public int ActiveJobCount => _snapshot.ActiveJobs;
    public int ParallelLimit => _snapshot.ParallelLimit;
    public string StatusText => _working ? "처리 중" : _error.Length > 0 ? "확인 필요" : _snapshot.Connected ? "PC 연결됨" : _snapshot.Stage switch { "not_installed" => "미설치", "upgrade_required" => "통합 업데이트 필요", "paused" or "disconnected" => "일시정지", "stopped" => "연결 종료", "offline" or "stale" or "error" => "연결 확인 필요", _ => "연결 대기" };
    public string DetailText => _error.Length > 0 ? _error : _snapshot.Detail;
    public string RelayStatusText => _snapshot.RelayConnected ? "다른 GPT의 원격 요청 통로도 연결됨" : _snapshot.RelayCode switch { "github_login_required" => "원격 요청 통로: GitHub 로그인 필요", "github_http_404" => "원격 요청 통로: 저장소 접근 또는 주소 확인 필요", _ => "원격 요청 통로 대기 · PC 안의 도구는 로컬 연결로 작업합니다." };
    private void Notify() { foreach(var p in new[]{nameof(DeviceLabel),nameof(IsInstalled),nameof(IsConnected),nameof(HasError),nameof(IsBusy),nameof(ActiveJobCount),nameof(ParallelLimit),nameof(StatusText),nameof(DetailText),nameof(RelayStatusText)}) OnPropertyChanged(p); }
    private void ShowError(Exception e) { _error = ProcessRunner.Sanitize(e.Message); Notify(); }
    public async Task PollAsync()
    {
        if (!await _gate.WaitAsync(0)) return;
        try
        {
            _snapshot = await _service.CheckAsync(_lifetime.Token);
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
        catch(OperationCanceledException) { }
        finally { _gate.Release(); }
    }
    private async Task RunAsync(Func<Task> action)
    {
        if (!await _operations.WaitAsync(0)) return;
        _working = true; _error = ""; Notify();
        try { await action(); }
        catch(Exception e) { _error = ProcessRunner.Sanitize(e.Message); }
        finally { _working = false; _operations.Release(); await PollAsync(); Notify(); }
    }
    private Task OperateAsync(string action) => RunAsync(async()=> { await _service.ControlAsync(action,_lifetime.Token); await Task.Delay(500,_lifetime.Token); });
    private Task StopAsync()
    {
        if (MessageBox.Show("이번 로그인 동안 새 PC 요청을 받지 않습니다. 진행 중인 작업은 해당 작업 정책에 따라 마무리하며, 다음 로그인에는 다시 연결합니다.","PC 연결 끄기",MessageBoxButton.OKCancel,MessageBoxImage.Question)!=MessageBoxResult.OK) return Task.CompletedTask;
        return OperateAsync("stop");
    }
    private Task InstallAsync() => RunAsync(async()=>{await _service.InstallAsync(_lifetime.Token);await Task.Delay(1000,_lifetime.Token);});
    public Task<string> SubmitAsync(string project, string tool, string action, object args) => _service.SubmitAsync(project,tool,action,args,_lifetime.Token);
    public Task<string> ResultAsync(string id) => _service.ResultAsync(id,_lifetime.Token);
    public void Dispose() { _lifetime.Cancel(); _service.Dispose(); }
}
