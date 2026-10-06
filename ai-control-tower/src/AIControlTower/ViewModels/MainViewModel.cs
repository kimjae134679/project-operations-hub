using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    public PcConnectionViewModel PcConnection { get; } = new();
    private readonly IReadOnlyList<IStatusProvider> _providers;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);
    private readonly ProjectDiscoveryService _discovery = new();
    private readonly ProjectCatalogService _catalogService = new();
    private readonly CatalogDefinition _catalog = ProjectCatalogService.Load();
    private readonly JobManager _jobs = new();
    private readonly ControlTowerSettings _settings;
    private readonly RemoteBridgeService _remote = new(new ProcessRunner());
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _serverLiveTimer;
    private readonly DispatcherTimer _pcLiveTimer;
    private readonly Dispatcher _dispatcher;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Dictionary<string, string> _programStates = new(StringComparer.OrdinalIgnoreCase);
    private FileSystemWatcher? _watcher;
    private DateTime _lastScan = DateTime.MinValue;
    private DateTime _lastStatus = DateTime.MinValue;
    private volatile bool _needsDiscovery;
    private string _projectPath = "";
    private string _taskInput = "";
    private string _message = "프로젝트 루트를 선택하거나 폴더를 끌어 놓으세요.";
    private bool _isRefreshing;
    private bool _disposed;
    private ProjectItem? _selectedProject;
    private ProgramItem? _selectedProgram;
    private ProgramCommand? _selectedCommand;
    private string _projectSearch = "";
    private string _programSearch = "";

    public MainViewModel(ControlTowerSettings? settings = null, bool enablePolling = true)
    {
        _settings = settings ?? ControlTowerSettings.Load();
        _settings.AutoCommunication = true;
        InitializeCommunication();
        _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        var runner = new ProcessRunner();
        _providers = [new ProjectBridgeStatusProvider(), new DesktopCommanderStatusProvider(runner), new JevStatusProvider(runner), new CodexStatusProvider(runner),
            new GitHubCliStatusProvider(runner), new N8nStatusProvider(runner), new AiOpsRunnerStatusProvider(runner), new DeliveryChainProvider(runner), new InstalledToolStatusProvider("aider"), new InstalledToolStatusProvider("hyperframes"), new InstalledToolStatusProvider("voicestudio"), new InstalledToolStatusProvider("zonos2")];
        _jobs.Log += OnJobLog;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timer.Tick += async (_, _) =>
        {
            UpdateRunningProperties();
            await PcConnection.PollAsync();
            await RefreshServerAsync();
            if (AutoCommunication) await SyncCommunicationAsync();
            if (_needsDiscovery || DateTime.UtcNow - _lastScan > TimeSpan.FromMinutes(1)) await DiscoverAsync();
            if (DateTime.UtcNow - _lastStatus > TimeSpan.FromSeconds(20)) await RefreshAsync();
        };
        _serverLiveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(RosterRefreshSeconds) };
        _serverLiveTimer.Tick += async (_, _) => await RefreshRosterAsync();
        _pcLiveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _pcLiveTimer.Tick += async (_, _) => await PcConnection.PollAsync();
        if (enablePolling)
        {
            JobManager.RecoverInterruptedRuns();
            if (!_settings.IsTemporary) _timer.Start();
            _serverLiveTimer.Start();
            _pcLiveTimer.Start();
        }
    }
    public ObservableCollection<ProjectItem> Projects { get; } = [];
    public ObservableCollection<ToolStatusViewModel> Statuses { get; } = [];
    private string _toolSearch = "";
    public string ToolSearch { get => _toolSearch; set { if (SetProperty(ref _toolSearch,value)) OnPropertyChanged(nameof(FilteredToolStatuses)); } }
    public IReadOnlyList<ToolStatusViewModel> UserFacingStatuses => Statuses.Where(s => s.IsUserFacing).ToArray();
    public IReadOnlyList<ToolStatusViewModel> FilteredToolStatuses => UserFacingStatuses.Where(s => Matches(ToolSearch,s.RawName,s.DisplayName,s.Purpose,s.StateLabel)).ToArray();
    public ObservableCollection<string> JobLogs { get; } = [];
    public event EventHandler? CatalogRefreshing;
    public event EventHandler? CatalogRefreshed;
    public string ProjectSearch
    {
        get => _projectSearch;
        set { if (!SetProperty(ref _projectSearch, value)) return; OnPropertyChanged(nameof(FilteredProjects)); OnPropertyChanged(nameof(NoProjectSearchResults)); }
    }
    public string ProgramSearch
    {
        get => _programSearch;
        set { if (!SetProperty(ref _programSearch, value)) return; OnPropertyChanged(nameof(FilteredFunctions)); OnPropertyChanged(nameof(NoProgramSearchResults)); }
    }
    public IReadOnlyList<ProjectItem> FilteredProjects => Projects.Where(p => Matches(ProjectSearch, p.DisplayName, p.Description, p.Name, p.Path, p.RoleLabel)).ToArray();
    public IReadOnlyList<FunctionItem> FilteredFunctions => SelectedProject?.Functions
        .Select(f => new FunctionItem { Id = f.Id, Name = f.Name, IsAdvanced = f.IsAdvanced && string.IsNullOrWhiteSpace(ProgramSearch), Programs = f.Programs.Where(p => Matches(ProgramSearch, f.Name, p.DisplayName, p.Name, p.KindLabel, p.Description)).ToArray() })
        .Where(f => f.Programs.Count > 0).ToArray() ?? [];
    public bool NoProjectSearchResults => Projects.Count > 0 && FilteredProjects.Count == 0;
    public bool NoProgramSearchResults => SelectedProject is not null && FilteredFunctions.Count == 0;
    private static bool Matches(string query, params string[] values) => string.IsNullOrWhiteSpace(query) || values.Any(v => v.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase));
    public int ProjectsCount => Projects.Count;
    public int ProgramsCount => Projects.Sum(p => p.Functions.Sum(f => f.Programs.Count));
    public int RunningCount => _jobs.RunningCount;
    public bool IsBusy => _jobs.BusyProjectCount > 0;
    public bool CanStopSelectedProgram => SelectedProgram is not null && _jobs.IsRunning(SelectedProgram.Id);
    public bool CanLaunchSelectedProgram => SelectedProgram is not null && CanOpenSelectedProgram && !_jobs.IsProjectBusy(SelectedProgram.ProjectId);
    public bool CanOpenSelectedProgram => SelectedProgram is not null && (File.Exists(SelectedProgram.Path) || Directory.Exists(SelectedProgram.Path));
    public bool HasSelectedCommands => SelectedProgram?.CanLaunch == true;
    public bool ShowSeparateOpenButton => HasSelectedCommands || SelectedProgram?.HasEditorLauncher == true;
    public string RootPath { get => _settings.RootPath; set { if (_settings.RootPath == value) return; _settings.RootPath = value; _needsDiscovery = true; OnPropertyChanged(); } }
    public bool EnableJev
    {
        get => _settings.EnableJev;
        set { if (_settings.EnableJev == value) return; _settings.EnableJev = value; SaveSettings(); OnPropertyChanged(); OnPropertyChanged(nameof(IsLocalJevExecutionAllowed)); OnPropertyChanged(nameof(LocalJevPolicyMessage)); }
    }
    public bool ReduceMotion
    {
        get => _settings.ReduceMotion;
        set { if (_settings.ReduceMotion == value) return; _settings.ReduceMotion = value; SaveSettings(); OnPropertyChanged(); }
    }
    public bool DarkMode
    {
        get => _settings.DarkMode;
        set { if (_settings.DarkMode == value) return; _settings.DarkMode = value; ThemeService.Apply(value); SaveSettings(); OnPropertyChanged(); OnPropertyChanged(nameof(ThemeSwitchLabel)); }
    }
    public string ThemeSwitchLabel => DarkMode ? "밝은 화면" : "어두운 화면";
    public ProjectItem? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (!SetProperty(ref _selectedProject, value)) return;
            SelectedProgram = value?.Functions.Where(f => !f.IsAdvanced).SelectMany(f => f.Programs).FirstOrDefault();
            ProjectPath = value?.Path ?? "";
            ProgramSearch = "";
            OnPropertyChanged(nameof(FilteredFunctions)); OnPropertyChanged(nameof(NoProgramSearchResults));
        }
    }
    public ProgramItem? SelectedProgram
    {
        get => _selectedProgram;
        set { if (!SetProperty(ref _selectedProgram, value)) return; SelectedCommand = value?.Commands.FirstOrDefault(); OnPropertyChanged(nameof(CanStopSelectedProgram)); OnPropertyChanged(nameof(CanLaunchSelectedProgram)); OnPropertyChanged(nameof(CanOpenSelectedProgram)); OnPropertyChanged(nameof(HasSelectedCommands)); OnPropertyChanged(nameof(ShowSeparateOpenButton)); }
    }
    public ProgramCommand? SelectedCommand { get => _selectedCommand; set => SetProperty(ref _selectedCommand, value); }
    public string ProjectPath { get => _projectPath; set => SetProperty(ref _projectPath, value); }
    public string TaskInput { get => _taskInput; set => SetProperty(ref _taskInput, value); }
    public string Message { get => _message; private set => SetProperty(ref _message, value); }
    public bool IsRefreshing { get => _isRefreshing; private set => SetProperty(ref _isRefreshing, value); }
    public bool IsLocalJevExecutionAllowed => EnableJev;
    public string LocalJevPolicyMessage => EnableJev ? "Jev를 이 관제탑에서 선택해 사용할 수 있습니다. 설치·키 존재는 인증이나 실행 성공을 뜻하지 않습니다." : LocalExecutionPolicy.LocalJevDisabledMessage;
    public string ActivitySummary => IsBusy ? $"관제탑 작업 {_jobs.BusyProjectCount}개 · 실행 프로세스 {RunningCount}개" : "실행 중인 관제탑 작업이 없습니다.";

    public async Task DiscoverAsync()
    {
        if (_disposed || !await _discoveryLock.WaitAsync(0)) return;
        try
        {
            if (!Directory.Exists(RootPath)) { Message = "탐색 폴더가 없습니다. 실제 프로젝트 루트를 선택하세요."; return; }
            var root = Path.GetFullPath(RootPath);
            var selectedId = SelectedProject?.Id;
            var selectedProgramId = SelectedProgram?.Id;
            var programSearch = ProgramSearch;
            var selectedCommand = SelectedCommand;
            var results = await Task.Run(() =>
            {
                var scanned = _discovery.Scan(root, _lifetime.Token);
                return new DiscoveryResult(_catalogService.Apply(scanned.Projects, root, _catalog), scanned.Warnings);
            }, _lifetime.Token);
            if (!Path.GetFullPath(RootPath).Equals(root, StringComparison.OrdinalIgnoreCase)) { _needsDiscovery = true; Message = "탐색 루트가 변경되어 이전 결과를 폐기했습니다. 새 루트를 다시 탐색합니다."; return; }
            CatalogRefreshing?.Invoke(this, EventArgs.Empty);
            Projects.Clear();
            foreach (var project in results.Projects)
            {
                foreach (var program in project.Functions.SelectMany(f => f.Programs))
                    if (_programStates.TryGetValue(program.Id, out var state)) program.Status = state;
                Projects.Add(project);
            }
            SelectedProject = Projects.FirstOrDefault(p => p.Id == selectedId) ?? Projects.FirstOrDefault();
            SelectedProgram = SelectedProject?.Functions.SelectMany(f => f.Programs).FirstOrDefault(p => p.Id == selectedProgramId)
                ?? SelectedProject?.Functions.Where(f => !f.IsAdvanced).SelectMany(f => f.Programs).FirstOrDefault();
            if (SelectedProject?.Id == selectedId) ProgramSearch = programSearch;
            if (selectedCommand is not null && SelectedProgram is not null && SelectedProgram.Id == selectedProgramId)
                SelectedCommand = SelectedProgram.Commands.FirstOrDefault(c => c.Name == selectedCommand.Name
                    && c.FileName == selectedCommand.FileName && c.Arguments.SequenceEqual(selectedCommand.Arguments)) ?? SelectedCommand;
            _lastScan = DateTime.UtcNow;
            _needsDiscovery = false;
            if (_watcher?.Path != root)
            {
                _watcher?.Dispose();
                _watcher = new FileSystemWatcher(root) { IncludeSubdirectories = true, NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite, InternalBufferSize = 8192 };
                void Mark(object? _, FileSystemEventArgs args)
                {
                    if (args.FullPath.Contains(Path.DirectorySeparatorChar + ".git" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                        || args.FullPath.Contains(Path.DirectorySeparatorChar + "ControlTowerData" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                        || args.FullPath.Contains(Path.DirectorySeparatorChar + "_통합소통" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                        || args.FullPath.Contains(Path.DirectorySeparatorChar + "node_modules" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return;
                    if (args.Name?.EndsWith(ProjectDiscoveryService.ManifestName, StringComparison.OrdinalIgnoreCase) == true
                        || args.ChangeType is WatcherChangeTypes.Created or WatcherChangeTypes.Deleted or WatcherChangeTypes.Renamed) _needsDiscovery = true;
                }
                _watcher.Created += Mark; _watcher.Deleted += Mark; _watcher.Renamed += (_, e) => Mark(null, e); _watcher.Changed += Mark;
                _watcher.Error += (_, _) => _needsDiscovery = true;
                _watcher.EnableRaisingEvents = true;
            }
            SaveSettings();
            OnPropertyChanged(nameof(ProjectsCount)); OnPropertyChanged(nameof(ProgramsCount));
            OnPropertyChanged(nameof(FilteredProjects)); OnPropertyChanged(nameof(NoProjectSearchResults));
            CatalogRefreshed?.Invoke(this, EventArgs.Empty);
            Message = $"프로젝트 {ProjectsCount}개 · 프로그램/산출물 {ProgramsCount}개를 확인했습니다.";
            foreach (var warning in results.Warnings) AddLog("탐색 참고 · " + warning);
            if (results.Warnings.Count > 0) Message += $" 참고 {results.Warnings.Count}건은 로그에서 확인하세요.";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Message = "탐색 실패 · " + ProcessRunner.Sanitize(ex.Message); AddLog(Message); }
        finally { _discoveryLock.Release(); }
    }
    public async Task RefreshAsync()
    {
        if (_disposed || !await _refreshLock.WaitAsync(0)) return;
        try
        {
            IsRefreshing = true;
            await PcConnection.PollAsync();
            var results = await Task.WhenAll(_providers.Select(provider => provider.CheckAsync(_lifetime.Token)));
            if (_disposed) return;
            foreach (var result in results)
            {
                var existing = Statuses.FirstOrDefault(status => status.RawName == result.DisplayName);
                if (existing is null) Statuses.Add(new ToolStatusViewModel(result)); else existing.Update(result);
            }
            _lastStatus = DateTime.UtcNow;
            OnPropertyChanged(nameof(UserFacingStatuses)); OnPropertyChanged(nameof(FilteredToolStatuses));
            Message = "연결 상태 확인 · " + DateTime.Now.ToString("HH:mm:ss");
            UpdateRunningProperties();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Message = "상태 갱신 실패 · " + ProcessRunner.Sanitize(ex.Message); }
        finally { IsRefreshing = false; _refreshLock.Release(); }
    }
    public void SelectProjectByPath(string path) => SelectedProject = Projects.FirstOrDefault(p => Path.GetFullPath(p.Path).Equals(Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase)) ?? SelectedProject;
    public void OpenProject() => OpenExisting(SelectedProject?.Path);
    public void OpenProgram()
    {
        if (SelectedProgram is not null && Uri.TryCreate(SelectedProgram.ServiceUrl, UriKind.Absolute, out var url)
            && url.IsLoopback && url.Scheme is "http" or "https")
        {
            try { Process.Start(new ProcessStartInfo(url.AbsoluteUri) { UseShellExecute = true })?.Dispose(); Message = "제작 화면을 열었습니다."; }
            catch (Exception ex) { Message = "화면 열기 실패 · " + ProcessRunner.Sanitize(ex.Message); }
            return;
        }
        OpenExisting(SelectedProgram?.Path);
    }
    public async Task ActivateSelectedProgramAsync()
    {
        if (SelectedProgram is { HasEditorLauncher: true } editor)
        {
            if (!File.Exists(editor.EditorOpenScript)) { Message = "편집기 열기 파일을 찾지 못했습니다."; return; }
            try
            {
                // Editing sessions have their own lifetime; closing the tower must not kill unsaved work.
                var start = new ProcessStartInfo("powershell.exe") { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = editor.WorkingDirectory };
                foreach (var argument in new[] { "-NoProfile", "-File", editor.EditorOpenScript }) start.ArgumentList.Add(argument);
                Process.Start(start)?.Dispose();
                Message = "Unity 열기를 요청했습니다. 편집기에서 확인하세요.";
            }
            catch (Exception ex) { Message = "편집기 열기 실패 · " + ProcessRunner.Sanitize(ex.Message); }
            return;
        }
        if (SelectedProgram?.CanLaunch != true) { OpenProgram(); return; }
        if (Uri.TryCreate(SelectedProgram.ServiceUrl, UriKind.Absolute, out var url) && url.IsLoopback && url.Scheme is "http" or "https")
        {
            try
            {
                using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                using var response = await client.GetAsync(url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, _lifetime.Token);
                if (response.IsSuccessStatusCode) { OpenProgram(); Message = "이미 실행 중인 제작 화면을 사용합니다."; return; }
            }
            catch (System.Net.Http.HttpRequestException) { }
            catch (TaskCanceledException) { }
            if (_lifetime.IsCancellationRequested) return;
        }
        await LaunchProgramAsync();
    }
    private void OpenExisting(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || (!Directory.Exists(path) && !File.Exists(path))) { Message = "실제 경로를 찾지 못했습니다."; return; }
        try
        {
            // Opening an artifact reveals it; it does not silently execute an installer/script.
            var info = new ProcessStartInfo("explorer.exe") { UseShellExecute = false };
            if (File.Exists(path) && System.IO.Path.GetExtension(path).ToLowerInvariant() is ".md" or ".txt")
            {
                info = new ProcessStartInfo("notepad.exe") { UseShellExecute = false };
                info.ArgumentList.Add(path);
            }
            else if (File.Exists(path)) info.Arguments = "/select,\"" + path + "\""; else info.ArgumentList.Add(path);
            Process.Start(info)?.Dispose();
            Message = File.Exists(path) && System.IO.Path.GetExtension(path).ToLowerInvariant() is ".md" or ".txt" ? "안내 파일을 열었습니다." : "파일·폴더 위치를 열었습니다.";
        }
        catch (Exception ex) { Message = "열기 실패 · " + ProcessRunner.Sanitize(ex.Message); }
    }
    public async Task LaunchProgramAsync()
    {
        var program = SelectedProgram;
        var command = SelectedCommand;
        if (program is null || command is null) { Message = "실행할 프로그램과 등록된 명령을 선택하세요."; return; }
        await RunProgramAsync(program, command);
    }
    private async Task RunProgramAsync(ProgramItem program, ProgramCommand command, string? input = null)
    {
        if (_jobs.IsProjectBusy(program.ProjectId)) { Message = "이 프로젝트에는 실행 중인 작업이 있습니다."; return; }
        try
        {
            UpdateProgramState(program, "실행 중 · " + command.Name);
            var result = await _jobs.RunAsync(program, command, _lifetime.Token, input);
            UpdateProgramState(program, result.State + (result.ExitCode is null ? "" : " · exit " + result.ExitCode));
            Message = program.DisplayName + " · " + program.StatusLabel + " · 작업 기록에서 결과를 확인하세요.";
        }
        catch (Exception ex) { UpdateProgramState(program, "실행 실패"); Message = ProcessRunner.Sanitize(ex.Message); AddLog(Message); }
        finally { UpdateRunningProperties(); }
    }
    public void StopProgram()
    {
        Message = SelectedProgram is not null && _jobs.Stop(SelectedProgram.Id) ? "선택한 관제탑 작업의 취소를 요청했습니다." : "관제탑이 소유한 해당 실행 작업이 없습니다.";
    }
    public async void RunJevTask()
    {
        if (!EnableJev) { Message = LocalJevPolicyMessage; return; }
        if (string.IsNullOrWhiteSpace(TaskInput)) { Message = "작업 내용을 입력하세요."; return; }
        if (!Directory.Exists(ProjectPath)) { Message = "존재하는 프로젝트 경로를 선택하세요. 다른 폴더로 대체 실행하지 않습니다."; return; }
        var package = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "node_modules", "jev-router", "bin", "jev-codex.mjs");
        var node = EnvironmentProbe.FindCommand("node.exe");
        if (!File.Exists(package) || node is null) { Message = "설치된 Jev node 런처를 찾지 못했습니다."; return; }
        if (!EnvironmentProbe.HasAnyEnvironmentVariable("JEV_API_KEY", "TYPESAFE_API_KEY")) { Message = "Jev 키 설정을 확인하지 못했습니다. 비밀값은 화면에 표시하지 않습니다."; return; }
        var projectId = SelectedProject?.Path.Equals(ProjectPath, StringComparison.OrdinalIgnoreCase) == true ? SelectedProject.Id : "folder:" + Path.GetFullPath(ProjectPath).ToLowerInvariant();
        var program = new ProgramItem { Id = projectId + "/jev", ProjectId = projectId, Name = "Jev · Codex 작업", Kind = "Jev", Path = ProjectPath, WorkingDirectory = ProjectPath };
        var command = new ProgramCommand { Name = "Jev 작업", FileName = node, Arguments = [package, "exec", "--json", "--sandbox", "workspace-write", "-"], TimeoutSeconds = 1800 };
        await RunProgramAsync(program, command, TaskInput);
    }
    public Task CancelJevTaskAsync()
    {
        var projectId = SelectedProject?.Path.Equals(ProjectPath, StringComparison.OrdinalIgnoreCase) == true ? SelectedProject.Id : "folder:" + Path.GetFullPath(ProjectPath).ToLowerInvariant();
        Message = _jobs.Stop(projectId + "/jev") ? "이 프로젝트 Jev 작업의 취소를 요청했습니다." : "이 프로젝트에서 관제탑이 시작한 Jev 작업이 없습니다.";
        return Task.CompletedTask;
    }
    public async Task ConsolidateRemoteStartupAsync()
    {
        try { Message = _remote.ConsolidateStartup(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ControlTowerSettings.DataDirectory); Message += " " + await _remote.ConfigureNativeStartupAsync(Environment.ProcessPath!, _lifetime.Token); AddLog(Message); await RefreshAsync(); }
        catch (Exception ex) { Message = "시작 경로 통합 실패 · " + ProcessRunner.Sanitize(ex.Message); AddLog(Message); }
    }
    public async Task EnsureRemoteRunningAsync()
    {
        try { Message = await _remote.EnsureRunningAsync(_lifetime.Token); AddLog(Message); }
        catch (Exception ex) { Message = "공유 연결 요청 실패 · " + ProcessRunner.Sanitize(ex.Message); }
    }
    public async Task StopRemoteRunningAsync()
    {
        try { Message = await new RemoteSupervisor(_remote).StopAsync(_lifetime.Token); AddLog(Message); await RefreshAsync(); }
        catch (Exception ex) { Message = "원격 연결 중지 실패 · " + ProcessRunner.Sanitize(ex.Message); }
    }
    private void UpdateProgramState(ProgramItem program, string state)
    {
        _programStates[program.Id] = state; program.Status = state;
        foreach (var item in Projects.SelectMany(p => p.Functions).SelectMany(f => f.Programs).Where(p => p.Id == program.Id)) item.Status = state;
    }
    private void UpdateRunningProperties()
    { OnPropertyChanged(nameof(RunningCount)); OnPropertyChanged(nameof(IsBusy)); OnPropertyChanged(nameof(ActivitySummary)); OnPropertyChanged(nameof(CanStopSelectedProgram)); OnPropertyChanged(nameof(CanLaunchSelectedProgram)); }
    private void OnJobLog(string line)
    {
        if (!_disposed) _dispatcher.InvokeAsync(() => { if (_disposed) return; AddLog(line); UpdateRunningProperties(); });
    }
    private void AddLog(string line) { JobLogs.Add(line); while (JobLogs.Count > 500) JobLogs.RemoveAt(0); }
    private void SaveSettings() { try { _settings.Save(); } catch (Exception ex) { AddLog("설정 저장 실패 · " + ProcessRunner.Sanitize(ex.Message)); } }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true; _timer.Stop(); _serverLiveTimer.Stop(); _pcLiveTimer.Stop(); _watcher?.Dispose(); _jobs.StopAllOwned(); _lifetime.Cancel(); _rosterMonitor.Dispose(); _serverMonitor.Dispose(); _jobs.Log -= OnJobLog; PcConnection.Dispose();
        // Semaphores remain available for in-flight finally blocks during shutdown.
    }
}
