using System.Collections.ObjectModel;
using System.Diagnostics;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed class MainViewModel : ObservableObject, IDisposable
{
    private readonly IReadOnlyList<IStatusProvider> _providers;
    private readonly ActivityStore _activityStore = new();
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly CommandQueueService _queue = new();
    private readonly System.Windows.Threading.DispatcherTimer _timer;
    private string _projectPath = Environment.CurrentDirectory;
    private string _taskInput = string.Empty;
    private string _message = "상태를 확인하는 중입니다.";
    private bool _isRefreshing;

    public MainViewModel()
    {
        var runner = new ProcessRunner();
        _providers = new IStatusProvider[]
        {
            new DesktopCommanderStatusProvider(runner), new JevStatusProvider(runner), new CodexStatusProvider(runner),
            new GitHubCliStatusProvider(runner), new N8nStatusProvider(runner), new AiOpsRunnerStatusProvider(runner), new DeliveryChainProvider(runner)
        };
        _timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();
    }

    public ObservableCollection<ToolStatusViewModel> Statuses { get; } = [];
    public string ProjectPath { get => _projectPath; set => SetProperty(ref _projectPath, value); }
    public string TaskInput { get => _taskInput; set => SetProperty(ref _taskInput, value); }
    public string Message { get => _message; private set => SetProperty(ref _message, value); }
    public bool IsRefreshing { get => _isRefreshing; private set => SetProperty(ref _isRefreshing, value); }
    public string ActivitySummary
    {
        get
        {
            var item = _activityStore.Current;
            return item.IsRunning ? $"{item.Task} · {item.Elapsed:hh\\:mm\\:ss}" : "실행 중인 Jev 작업이 없습니다.";
        }
    }

    public async Task RefreshAsync()
    {
        if (!await _refreshLock.WaitAsync(0)) return;
        try
        {
            IsRefreshing = true;
            var results = await Task.WhenAll(_providers.Select(provider => provider.CheckAsync(CancellationToken.None)));
            foreach (var result in results)
            {
                var existing = Statuses.FirstOrDefault(status => status.DisplayName == result.DisplayName);
                if (existing is null) Statuses.Add(new ToolStatusViewModel(result)); else existing.Update(result);
            }
            var queued = _queue.TryClaimNext();
            if (queued is not null) { TaskInput = queued.Instruction; RunJevTask(); Message = "큐 작업을 수신해 Jev 실행을 요청했습니다: " + queued.Id; }
            else Message = "상태를 " + DateTime.Now.ToString("HH:mm:ss") + "에 갱신했습니다.";
            OnPropertyChanged(nameof(ActivitySummary));
        }
        catch (Exception ex) { Message = "상태 갱신 실패: " + ProcessRunner.Sanitize(ex.Message); }
        finally { IsRefreshing = false; _refreshLock.Release(); }
    }

    public void RunJevTask()
    {
        if (string.IsNullOrWhiteSpace(TaskInput)) { Message = "실행할 작업 내용을 입력하세요."; return; }
        var launcher = EnvironmentProbe.FindCommand("jev-codex");
        if (launcher is null) { Message = "Jev 실행 파일을 찾지 못했습니다."; return; }
        if (!EnvironmentProbe.HasAnyEnvironmentVariable("JEV_API_KEY", "TYPESAFE_API_KEY")) { Message = "Jev API 키가 설정되지 않아 작업을 시작하지 않았습니다."; return; }
        try
        {
            var escapedTask = TaskInput.Replace("\"", "\\\"");
            var process = Process.Start(new ProcessStartInfo("cmd.exe", "/k \"\"" + launcher + "\" \"" + escapedTask + "\"\"") { UseShellExecute = true, WorkingDirectory = Directory.Exists(ProjectPath) ? ProjectPath : Environment.CurrentDirectory });
            _activityStore.Start(ProjectPath, TaskInput, process);
            Message = "Jev 콘솔에서 작업을 시작했습니다.";
            OnPropertyChanged(nameof(ActivitySummary));
        }
        catch (Exception ex) { Message = "Jev 작업 시작 실패: " + ProcessRunner.Sanitize(ex.Message); }
    }

    public async Task CancelJevTaskAsync()
    {
        await _activityStore.CancelAsync();
        Message = "관제탑이 시작한 Jev 작업만 취소했습니다.";
        OnPropertyChanged(nameof(ActivitySummary));
    }

    public void Dispose() { _timer.Stop(); _refreshLock.Dispose(); }
}


