using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    public PcConnectionViewModel PcConnection { get; }
    private readonly IReadOnlyList<IStatusProvider> _providers;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);
    private readonly ProjectDiscoveryService _discovery = new();
    private readonly ProjectCatalogService _catalogService = new();
    private readonly CatalogDefinition _catalog = ProjectCatalogService.Load();
    private readonly JobManager _jobs;
    private readonly WorkDashboardService _workDashboardService;
    // Explicit central registry IDs supplied by the user/project notice map, never guessed from names.
    private static readonly IReadOnlyDictionary<string,string> DashboardProjectAliases=new Dictionary<string,string>(StringComparer.Ordinal)
    {
        ["PhoneLOL"]="phonelol-current",["Mushoku-Audiobook"]="audiobook",["Video-Downloader"]="video-downloader",["Control-Tower"]="project-operations-hub"
    };
    public WorkDashboardViewModel WorkDashboard { get; }
    public WorkDashboardViewModel ManagementDashboard { get; }
    public DocumentReaderViewModel Documents { get; } = new();
    public string ApplicationVersionLabel => "실행 버전 " + (typeof(MainViewModel).Assembly.GetName().Version?.ToString() ?? "미확인");
    public string ApplicationSourceLabel => "실행 파일: " + (Environment.ProcessPath ?? "미확인");
    private readonly ControlTowerSettings _settings;
    private readonly StartupPolicy _startupPolicy;
    public StartupPolicy StartupMode => _startupPolicy;
    public bool IsManualControl => _startupPolicy.IsManualControl;
    public bool CanRunRegisteredTasks => _startupPolicy.AllowRegisteredTasks;
    public bool CanReadLiveStatus => _startupPolicy.AllowLiveQueries;
    public bool IsReadOnlyView => _settings.TransientReadOnly;
    public bool CanMutate => !IsReadOnlyView;
    public string ViewModeLabel => IsManualControl ? "연결 자동 유지 · 등록 작업 수동 실행 · 설정/Git/설치 자동 변경 없음" : IsReadOnlyView ? "로컬 조회 · 외부 연결/실행/공유는 중지 · 표시 시각 ≠ 생존" : "자동으로 최신 상태 유지";
    public bool BlockRegisteredOperation()
    { if(CanRunRegisteredTasks)return false;return BlockOperation(); }
    public bool BlockOperation()
    { if(!IsReadOnlyView)return false; Message="로컬 조회 · 외부 연결/실행/공유 보류 · 기존 파일과 작업은 변경하지 않습니다."; return true; }
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
    private DateTime _lastManualDashboardRefresh = DateTime.MinValue;
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

    public MainViewModel(ControlTowerSettings? settings = null, bool enablePolling = true) : this(settings,enablePolling,JobManager.RecoverInterruptedRuns) { }
    public MainViewModel(ControlTowerSettings? settings, bool enablePolling, Action recoverRuns)
        : this(settings,enablePolling,recoverRuns,null,null) { }
    public MainViewModel(ControlTowerSettings? settings, bool enablePolling, Action recoverRuns, StartupPolicy? startupPolicy, ProjectBridgeService? pcService)
    {
        _settings = settings ?? ControlTowerSettings.Load();
        _startupPolicy = startupPolicy ?? new(IsReadOnlyView);
        if(IsManualControl){_settings.TransientReadOnly=true;_settings.TransientDataDirectory??=@"D:\A_KJ\AI\ControlTowerData\manual-control";}
        _jobs=IsManualControl?new JobManager(_settings.ViewDataDirectory):new JobManager();
        _workDashboardService = IsReadOnlyView && !IsManualControl ? new WorkDashboardService(Path.Combine(_settings.ViewDataDirectory,"runs")) : new WorkDashboardService(Path.Combine(IsManualControl?_settings.ViewDataDirectory:ControlTowerSettings.DataDirectory,"runs"),_jobs.IsRunning,
            async ct => { if(IsManualControl)return PcConnection.Snapshot;if(pcService is not null)return await pcService.CheckAsync(ct);using var bridge=new ProjectBridgeService(); return await bridge.CheckAsync(ct); },
            async (id,ct) => { if(pcService is not null)return await pcService.ResultAsync(id,ct);using var bridge=new ProjectBridgeService(); return await bridge.ResultAsync(id,ct); });
        foreach(var path in (_settings.ContinuousStatePaths ?? []).Take(40)) _workDashboardService.ContinuousPaths.Add(path);
        if(!IsReadOnlyView && File.Exists(WorkDashboardService.FoundationState) && !_workDashboardService.ContinuousPaths.Contains(WorkDashboardService.FoundationState,StringComparer.OrdinalIgnoreCase))
            _workDashboardService.ContinuousPaths.Add(WorkDashboardService.FoundationState);
        PcConnection = new(pcService,(root,path,title) => Documents.OpenAsync(root,path,title),IsReadOnlyView,IsManualControl);
        _workDashboardService.RefreshKnownExchangeRoots(_settings.CommunicationHubPath, [], _settings.CommunicationFolders.Values);
        WorkDashboard = new(_workDashboardService.ReadAsync,_workDashboardService.DetailsAsync,_jobs.IsRunning,_jobs.Stop)
        {
            RegisterContinuousPath = path =>
            {
                if(IsManualControl)return _workDashboardService.RegisterContinuousSource(path,IsReadOnlyView,true);
                if(BlockOperation())return false;
                if(!_workDashboardService.RegisterContinuousSource(path,IsReadOnlyView,false))return false;
                _settings.ContinuousStatePaths=_workDashboardService.ContinuousPaths.ToList(); SaveSettings();
                return true;
            }
        };
        ManagementDashboard = new(_ => Task.FromResult<IReadOnlyList<WorkActivity>>(WorkDashboard.Activities.ToArray()),_workDashboardService.DetailsAsync) { ManagementOnly=true };
        if(!IsReadOnlyView)_settings.AutoCommunication = true;
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
            await RefreshWorkDashboardAsync();
            await PcConnection.PollAsync();
            await RefreshServerAsync();
            if (AutoCommunication) await SyncCommunicationAsync();
            if (_needsDiscovery || DateTime.UtcNow - _lastScan > TimeSpan.FromMinutes(1)) await DiscoverAsync();
            if (DateTime.UtcNow - _lastStatus > TimeSpan.FromSeconds(20)) await RefreshAsync();
        };
        _serverLiveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(RosterRefreshSeconds) };
        _serverLiveTimer.Tick += async (_, _) => {await RefreshRosterAsync();if(IsManualControl)await RefreshServerAsync();};
        _pcLiveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _pcLiveTimer.Tick += async (_, _) =>
        {
            if(IsManualControl)
            {
                await PcConnection.MaintainConnectionAsync(DateTimeOffset.UtcNow);
                _ = StartManualCollection();
                if(!_disposed&&DateTime.UtcNow-_lastManualDashboardRefresh>=TimeSpan.FromSeconds(5))
                {_lastManualDashboardRefresh=DateTime.UtcNow;await RefreshWorkDashboardAsync();}
            }
            else await PcConnection.PollAsync();
        };
        if (enablePolling && !IsReadOnlyView)
        {
            _startupPolicy.RecoverRuns(recoverRuns);
            if (!_settings.IsTemporary) _timer.Start();
            _serverLiveTimer.Start();
            _pcLiveTimer.Start();
        }
        if(enablePolling && IsManualControl){_pcLiveTimer.Start();_serverLiveTimer.Start();}
    }
    public async Task InitializeManualAsync()
    {
        if(!IsManualControl || _disposed)return;
        await DiscoverAsync();await PcConnection.MaintainConnectionAsync(DateTimeOffset.UtcNow);
        await RefreshAsync();await RefreshWorkDashboardAsync();await SyncCommunicationAsync(true);
        await RefreshRosterAsync(true);await RefreshServerAsync(true);
    }
    public ObservableCollection<ProjectItem> Projects { get; } = [];
    public ObservableCollection<ToolStatusViewModel> Statuses { get; } = [];
    private string _toolSearch = "";
    private bool _showOtherTools;
    public bool ShowOtherTools {get=>_showOtherTools;set {if(SetProperty(ref _showOtherTools,value))OnPropertyChanged(nameof(FilteredToolStatuses));}}
    public string ToolEvidenceSummary=>$"근거 연결 {Statuses.Count(s=>s.HasUsageEvidence)} / 등록 {Statuses.Count} · 설치 ≠ 실제 사용";
    public string ToolSearch { get => _toolSearch; set { if (SetProperty(ref _toolSearch,value)) OnPropertyChanged(nameof(FilteredToolStatuses)); } }
    public IReadOnlyList<ToolStatusViewModel> UserFacingStatuses => Statuses.Where(s => s.IsUserFacing).ToArray();
    public static IReadOnlyList<ToolStatusViewModel> SelectKnownTools(IEnumerable<ToolStatusViewModel> statuses,IReadOnlySet<string> knownInstallations,bool showOther)
        =>statuses.Where(s=>s.IsUserFacing && (showOther || s.HasUsageEvidence || knownInstallations.Contains(s.Id) || s.Kind is StatusKind.Ready or StatusKind.Running)).ToArray();
    private static IReadOnlySet<string> KnownToolInstallations()
    {
        // Existing allowlisted installation contracts only, never authentication or process probes.
        var ids=new HashSet<string>(StringComparer.Ordinal);
        if(File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"npm","node_modules","jev-router","package.json")))ids.Add("jev");
        if(EnvironmentProbe.FindCommand("codex") is not null)ids.Add("codex");
        if(EnvironmentProbe.FindCommand("gh") is not null)ids.Add("github-cli");
        return ids;
    }
    public IReadOnlyList<ToolStatusViewModel> FilteredToolStatuses => SelectKnownTools(UserFacingStatuses,KnownToolInstallations(),ShowOtherTools || !string.IsNullOrWhiteSpace(ToolSearch))
        .Where(s=>Matches(ToolSearch,s.RawName,s.DisplayName,s.Purpose,s.StateLabel,s.UsageEvidence)).ToArray();
    private void RefreshToolEvidence()
    {
        // Only explicit task/command identities, not installation probes or actor-name guesses.
        foreach(var tool in Statuses)
        {
            var workerNames=tool.Id switch
            {
                "jev"=>new[]{"Jev","Jev · 관제탑 등록 실행","Jev Router"},"codex"=>new[]{"Codex"},
                "project-bridge"=>new[]{"ProjectBridge"},"desktop-commander"=>new[]{"Remote Desktop Commander"},
                "n8n"=>new[]{"n8n","n8n local bridge"},"aider"=>new[]{"Aider"},"hyperframes"=>new[]{"HyperFrames"},
                "voicestudio"=>new[]{"VoiceStudio"},"zonos2"=>new[]{"Zonos2"},_=>Array.Empty<string>()
            };
            var record=WorkDashboard.Activities.FirstOrDefault(r=>!r.IsConversation && r.Source!="작업 연결 없음"
                && (workerNames.Contains(r.WorkerKind,StringComparer.Ordinal)||workerNames.Contains(r.Worker,StringComparer.Ordinal) && r.Source is "로컬 GET 상태" or "ProjectBridge GET"));
            string? evidence=record is null?null:$"등록 작업 기록: {record.Project} · {record.Title} · {record.UpdatedLabel} · 현재 생존/실사용 성공은 별도 확인";
            if(evidence is null)
            {
                var commands=tool.Id switch {"jev"=>new[]{"jev","jev.cmd","jev.exe"},"codex"=>new[]{"codex","codex.cmd","codex.exe"},"github-cli"=>new[]{"gh","gh.exe"},"aider"=>new[]{"aider","aider.exe"},_=>Array.Empty<string>()};
                var project=Projects.FirstOrDefault(p=>p.Functions.SelectMany(f=>f.Programs).SelectMany(p=>p.Commands).Any(c=>commands.Contains(Path.GetFileName(c.FileName),StringComparer.OrdinalIgnoreCase)));
                if(project is not null)evidence=$"프로젝트 실행도구 명시 등록: {project.DisplayName} · 실제 실행/성공 확인 아님";
            }
            tool.SetUsageEvidence(evidence is null?null:WorkDashboardService.SafeText(evidence));
        }
        OnPropertyChanged(nameof(ToolEvidenceSummary));OnPropertyChanged(nameof(FilteredToolStatuses));
    }
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
    public bool CanStopSelectedProgram => CanRunRegisteredTasks && SelectedProgram is not null && _jobs.IsRunning(SelectedProgram.Id);
    public bool CanLaunchSelectedProgram => CanRunRegisteredTasks && SelectedProgram is not null && CanOpenSelectedProgram && _jobs.CanRun(SelectedProgram);
    public bool CanOpenSelectedProgram => SelectedProgram is not null && (File.Exists(SelectedProgram.Path) || Directory.Exists(SelectedProgram.Path));
    public bool HasSelectedCommands => SelectedProgram?.CanLaunch == true;
    public bool ShowSeparateOpenButton => HasSelectedCommands || SelectedProgram?.HasEditorLauncher == true;
    public string RootPath { get => _settings.RootPath; set { if (_settings.RootPath == value) return; _settings.RootPath = value; _needsDiscovery = true; OnPropertyChanged(); } }
    private bool? _jevSessionEnabled;
    private bool _jevSessionVerified;
    private bool _independentJevWorkspace;
    public bool IndependentJevWorkspace
    {
        get => _independentJevWorkspace;
        set { if (SetProperty(ref _independentJevWorkspace, value)) OnPropertyChanged(nameof(JevWorkspacePath)); }
    }
    public string JevWorkspacePath => IndependentJevWorkspace ? SelectedProgram?.WorkingDirectory ?? "등록 작업 폴더 선택 필요" : ProjectPath;
    public bool EnableJev
    {
        get => JevExecutionPolicy.ResolveSessionEnable(_settings.EnableJev,IsManualControl,_jevSessionEnabled);
        set
        {
            if(IsManualControl)
            {
                if(value&&!_jevSessionVerified){Message="gpt-6.1-sol · 계정 검증 필요";return;}
                if(_jevSessionEnabled==value)return;_jevSessionEnabled=value;
            }
            else {if(_settings.EnableJev==value)return;_settings.EnableJev=value;SaveSettings();}
            OnPropertyChanged();OnPropertyChanged(nameof(IsLocalJevExecutionAllowed));OnPropertyChanged(nameof(LocalJevPolicyMessage));
        }
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
    public static IReadOnlyList<CommunicationProjectTarget> CurrentCommunicationTargets(IEnumerable<ProjectItem> projects,CatalogDefinition catalog)
    {
        static string Full(string path) {try{return Path.IsPathFullyQualified(path)?Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar):"";}catch(ArgumentException){return "";}}
        return projects.Select(p=>
        {
            var path=Full(p.Path);
            CatalogProject[] registered=path.Length==0?[]:catalog.Projects.Where(c=>Full(c.Path).Equals(path,StringComparison.OrdinalIgnoreCase)).Take(2).ToArray();
            var communication=registered.Length==1?CommunicationIdForCatalog(registered[0].Id):registered.Length>1?null:CommunicationIdForCatalog(p.Id);
            return new CommunicationProjectTarget(p.Id,p.DisplayName,communication);
        }).ToArray();
    }
    public ProjectItem? SelectedProject
    {
        get => _selectedProject;
        set
        {
            var sameScopeProject = value is not null && _selectedProject is not null
                && value.Id == _selectedProject.Id && SameJevWorkspacePath(value.Path, _selectedProject.Path);
            var previousProgramId = SelectedProgram?.Id;
            if (!SetProperty(ref _selectedProject, value)) return;
            if (!sameScopeProject) IndependentJevWorkspace = false;
            // Discovery replaces DTOs. Restore the semantic selection without temporarily choosing another scope.
            SelectedProgram = (sameScopeProject ? value?.Functions.SelectMany(f => f.Programs).FirstOrDefault(p => p.Id == previousProgramId) : null)
                ?? value?.Functions.Where(f => !f.IsAdvanced).SelectMany(f => f.Programs).FirstOrDefault();
            ProjectPath = value?.Path ?? "";
            WorkDashboard.SetProjectContext(value?.Id??"",value?.DisplayName??"");
            ProgramSearch = "";
            OnPropertyChanged(nameof(FilteredFunctions)); OnPropertyChanged(nameof(NoProgramSearchResults));
        }
    }
    public ProgramItem? SelectedProgram
    {
        get => _selectedProgram;
        set
        {
            var sameScope = value is not null && _selectedProgram is not null && value.Id == _selectedProgram.Id
                && value.ProjectId == _selectedProgram.ProjectId && value.CanLaunch && _selectedProgram.CanLaunch
                && SameJevWorkspacePath(value.WorkingDirectory, _selectedProgram.WorkingDirectory);
            if (!SetProperty(ref _selectedProgram, value)) return;
            if (!sameScope) IndependentJevWorkspace = false;
            SelectedCommand = value?.Commands.FirstOrDefault(); OnPropertyChanged(nameof(JevWorkspacePath)); OnPropertyChanged(nameof(CanStopSelectedProgram)); OnPropertyChanged(nameof(CanLaunchSelectedProgram)); OnPropertyChanged(nameof(CanOpenSelectedProgram)); OnPropertyChanged(nameof(HasSelectedCommands)); OnPropertyChanged(nameof(ShowSeparateOpenButton));
        }
    }
    private static bool SameJevWorkspacePath(string left, string right)
    {
        try { return JevExecutionWorkspace.Same(left, right); }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or InvalidOperationException) { return false; }
    }
    public ProgramCommand? SelectedCommand { get => _selectedCommand; set => SetProperty(ref _selectedCommand, value); }
    public string ProjectPath
    {
        get => _projectPath;
        set { var samePath = SameJevWorkspacePath(_projectPath, value); if (!SetProperty(ref _projectPath, value)) return; if (!samePath) IndependentJevWorkspace = false; OnPropertyChanged(nameof(JevWorkspacePath)); }
    }
    public string TaskInput { get => _taskInput; set => SetProperty(ref _taskInput, value); }
    public string Message { get => _message; private set => SetProperty(ref _message, value); }
    public bool IsRefreshing { get => _isRefreshing; private set => SetProperty(ref _isRefreshing, value); }
    public bool IsLocalJevExecutionAllowed => CanRunRegisteredTasks && EnableJev;
    public string LocalJevPolicyMessage => IsReadOnlyView && !IsManualControl ? "로컬 조회 · 실행 보류" : IsManualControl ? (_jevSessionVerified ? (EnableJev ? "gpt-6.1-sol · 계정 검증됨" : "Jev 사용 안 함") : "gpt-6.1-sol · 계정 검증 필요") : EnableJev ? "gpt-6.1-sol · 실행 전 검증" : LocalExecutionPolicy.LocalJevDisabledMessage;
    public string ActivitySummary => IsReadOnlyView && !IsManualControl ? "로컬 저장 기록 조회 · 실제 작업/프로세스 생존 미확인" : IsBusy ? $"관제탑 작업 {_jobs.BusyProjectCount}개 · 실행 프로세스 {RunningCount}개" : "실행 중인 관제탑 작업이 없습니다.";
    public async Task RefreshWorkDashboardAsync()
    {
        if(_disposed)return;
        _workDashboardService.RegisterCatalog(Projects,DashboardProjectAliases);
        _workDashboardService.RefreshKnownExchangeRoots(_settings.CommunicationHubPath,
            Projects.Select(project => project.Path), _settings.CommunicationFolders.Values);
        await WorkDashboard.RefreshAsync(_lifetime.Token);
        if(!_disposed) ManagementDashboard.ApplySnapshot(WorkDashboard.Activities.ToArray());
        RefreshToolEvidence();
    }

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
            _workDashboardService.RegisterCatalog(Projects,DashboardProjectAliases);
            WorkDashboard.SetProjectContext(SelectedProject?.Id??"",SelectedProject?.DisplayName??"");
            SetCurrentCommunicationProjects(CurrentCommunicationTargets(Projects,_catalog));
            RefreshToolEvidence();
            _workDashboardService.RefreshKnownExchangeRoots(_settings.CommunicationHubPath,
                Projects.Select(project => project.Path), _settings.CommunicationFolders.Values);
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
        if(IsManualControl)
        {
            await PcConnection.MaintainConnectionAsync(DateTimeOffset.UtcNow);
            Statuses.Clear();Statuses.Add(new(new("project-bridge","ProjectBridge",PcConnection.IsConnected?StatusKind.Ready:StatusKind.Unknown,PcConnection.DetailText,DateTimeOffset.UtcNow)));
            RefreshToolEvidence();Message=ViewModeLabel;OnPropertyChanged(nameof(UserFacingStatuses));OnPropertyChanged(nameof(FilteredToolStatuses));return;
        }
        if(IsReadOnlyView)
        {
            await PcConnection.PollAsync();
            if(Statuses.Count==0)foreach(var provider in _providers)
                Statuses.Add(new ToolStatusViewModel(new(provider.Id,provider.Id switch {"project-bridge"=>"ProjectBridge","desktop-commander"=>"Remote Desktop Commander","jev"=>"Jev Router","codex"=>"Codex","github-cli"=>"GitHub CLI",_=>provider.Id},StatusKind.Unknown,"로컬 조회 · 외부/API 상태 확인 없음 · 현재 생존 미확인",DateTimeOffset.UtcNow)));
            RefreshToolEvidence();Message=_settings.ReadOnlyLoadIssue.Length>0?_settings.ReadOnlyLoadIssue:ViewModeLabel;OnPropertyChanged(nameof(UserFacingStatuses));OnPropertyChanged(nameof(FilteredToolStatuses));return;
        }
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
            RefreshToolEvidence();
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
        if(IsReadOnlyView) { OpenExisting(SelectedProgram?.Path);return; }
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
        if(BlockRegisteredOperation())return;
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
    private async void OpenExisting(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || (!Directory.Exists(path) && !File.Exists(path))) { Message = "실제 경로를 찾지 못했습니다."; return; }
        try
        {
            if(File.Exists(path) && Path.GetExtension(path).ToLowerInvariant() is ".md" or ".txt" or ".json" or ".log")
            {
                var roots=new[] { SelectedProject?.Path, SelectedProgram?.WorkingDirectory, _settings.CommunicationHubPath, ControlTowerSettings.DataDirectory }
                    .Concat(_settings.CommunicationFolders.Values).Where(r=>!string.IsNullOrWhiteSpace(r));
                var full=Path.GetFullPath(path);
                var root=roots.FirstOrDefault(r=>full.StartsWith(Path.GetFullPath(r!).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase));
                if(root is null){Message="문서의 확인된 프로젝트/연결 루트가 없습니다. 외부 프로그램으로 자동 열지 않습니다.";return;}
                await Documents.OpenAsync(root,full,SelectedProgram?.DisplayName??Path.GetFileName(full));
                Message="통합 프로그램 내부 읽기로 안내·기록을 열었습니다.";
                return;
            }
            // Opening an artifact reveals it; it does not silently execute an installer/script.
            if(BlockOperation())return;
            var info = new ProcessStartInfo("explorer.exe") { UseShellExecute = false };
            if (File.Exists(path)) info.Arguments = "/select,\"" + path + "\""; else info.ArgumentList.Add(path);
            Process.Start(info)?.Dispose();
            Message = "파일·폴더 위치를 열었습니다.";
        }
        catch (Exception ex) { Message = "열기 실패 · " + ProcessRunner.Sanitize(ex.Message); }
    }
    public async Task LaunchProgramAsync()
    {
        if(BlockRegisteredOperation())return;
        var program = SelectedProgram;
        var command = SelectedCommand;
        if (program is null || command is null) { Message = "실행할 프로그램과 등록된 명령을 선택하세요."; return; }
        await RunProgramAsync(program, command);
    }
    private async Task RunProgramAsync(ProgramItem program, ProgramCommand command, string? input = null,AiCompletionContract? aiContract=null,string? independentWorkspaceRoot=null)
    {
        if(BlockRegisteredOperation())return;
        if (!_jobs.CanRun(program, independentWorkspaceRoot)) { Message = "같은 실행 ID·공유 프로젝트·겹치는 작업 폴더에 소유 작업이 있습니다."; return; }
        try
        {
            UpdateProgramState(program, "실행 중 · " + command.Name);
            var result = await _jobs.RunAsync(program, command, _lifetime.Token, input,aiContract:aiContract,independentWorkspaceRoot:independentWorkspaceRoot);
            UpdateProgramState(program, result.State + (result.ExitCode is null ? "" : " · exit " + result.ExitCode));
            Message = program.DisplayName + " · " + program.StatusLabel + " · 작업 기록에서 결과를 확인하세요.";
        }
        catch (Exception ex) { UpdateProgramState(program, "실행 실패"); Message = ProcessRunner.Sanitize(ex.Message); AddLog(Message); }
        finally { UpdateRunningProperties(); }
    }
    public void StopProgram()
    {
        if(BlockRegisteredOperation())return;
        Message = SelectedProgram is not null && _jobs.Stop(SelectedProgram.Id) ? "선택한 관제탑 작업의 취소를 요청했습니다." : "관제탑이 소유한 해당 실행 작업이 없습니다.";
    }
    public void PrepareJevSession()
    {
        if(BlockRegisteredOperation())return;
        var evidence=JevExecutionEvidenceReader.ReadDefault();
        _jevSessionVerified=evidence.IsVerified;
        if(IsManualControl)_jevSessionEnabled=evidence.IsVerified;
        OnPropertyChanged(nameof(EnableJev));OnPropertyChanged(nameof(IsLocalJevExecutionAllowed));OnPropertyChanged(nameof(LocalJevPolicyMessage));
        if(!evidence.IsVerified)Message="gpt-6.1-sol · 계정 검증 필요";
    }
    public async void RunJevTask()
    {
        if(BlockRegisteredOperation())return;
        if (!EnableJev) { Message = LocalJevPolicyMessage; return; }
        var evidence=JevExecutionEvidenceReader.ReadDefault();
        if(!JevExecutionPolicy.TryCreate(evidence.RequestedModel,evidence.IsVerified,out var contract,out var reason))
        { _jevSessionVerified=false;if(IsManualControl)_jevSessionEnabled=false;OnPropertyChanged(nameof(EnableJev));OnPropertyChanged(nameof(IsLocalJevExecutionAllowed));OnPropertyChanged(nameof(LocalJevPolicyMessage));Message="Jev 실행 보류 · " + (evidence.IsVerified?reason:evidence.Reason);return; }
        if (string.IsNullOrWhiteSpace(TaskInput)) { Message = "작업 내용을 입력하세요."; return; }
        if (!Directory.Exists(ProjectPath)) { Message = "존재하는 프로젝트 경로를 선택하세요. 다른 폴더로 대체 실행하지 않습니다."; return; }
        JevExecutionTarget target;
        try { target = JevExecutionWorkspace.Resolve(SelectedProject, SelectedProgram, ProjectPath, IndependentJevWorkspace); }
        catch (Exception ex) { Message = ProcessRunner.Sanitize(ex.Message); return; }
        var node = EnvironmentProbe.FindCommand("node.exe");
        if (node is null) { Message = "설치된 Node 런처를 찾지 못했습니다."; return; }
        string package;
        try { package=JevNativeLauncher.Prepare(); }
        catch { Message="Jev 런처 검증 실패 · 기존 파일은 덮어쓰지 않습니다.";return; }
        var command = new ProgramCommand { Name = "Jev 작업", FileName = node, Arguments = JevExecutionPolicy.BuildArguments(package,contract!.RequestedModel).ToArray(), TimeoutSeconds = 1800 };
        await RunProgramAsync(target.Program, command, TaskInput,contract,target.IndependentWorkspaceRoot);
    }
    public Task CancelJevTaskAsync()
    {
        if(BlockRegisteredOperation())return Task.CompletedTask;
        try
        {
            var target = JevExecutionWorkspace.Resolve(SelectedProject, SelectedProgram, ProjectPath, IndependentJevWorkspace);
            Message = _jobs.Stop(target.Program.Id) ? "선택한 Jev 소유 작업의 취소를 요청했습니다." : "선택한 범위에서 관제탑이 시작한 Jev 작업이 없습니다.";
        }
        catch (Exception ex) { Message = ProcessRunner.Sanitize(ex.Message); }
        return Task.CompletedTask;
    }
    public async Task ConsolidateRemoteStartupAsync()
    {
        if(BlockOperation())return;
        try { Message = _remote.ConsolidateStartup(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ControlTowerSettings.DataDirectory); Message += " " + await _remote.ConfigureNativeStartupAsync(Environment.ProcessPath!, _lifetime.Token); AddLog(Message); await RefreshAsync(); }
        catch (Exception ex) { Message = "시작 경로 통합 실패 · " + ProcessRunner.Sanitize(ex.Message); AddLog(Message); }
    }
    public async Task EnsureRemoteRunningAsync()
    {
        if(BlockOperation())return;
        try { Message = await _remote.EnsureRunningAsync(_lifetime.Token); AddLog(Message); }
        catch (Exception ex) { Message = "공유 연결 요청 실패 · " + ProcessRunner.Sanitize(ex.Message); }
    }
    public async Task StopRemoteRunningAsync()
    {
        if(BlockOperation())return;
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
    private void SaveSettings() { if(IsReadOnlyView){Message="로컬 조회 · 화면 선택은 임시이며 설정/읽기 위치는 저장하지 않습니다.";return;} try { _settings.Save(); } catch (Exception ex) { AddLog("설정 저장 실패 · " + ProcessRunner.Sanitize(ex.Message)); } }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true; _timer.Stop(); _serverLiveTimer.Stop(); _pcLiveTimer.Stop(); _watcher?.Dispose(); if(CanMutate)_jobs.StopAllOwned(); else if(CanRunRegisteredTasks)_jobs.StopAllOwned(); _lifetime.Cancel(); _rosterMonitor.Dispose(); _serverMonitor.Dispose(); _jobs.Log -= OnJobLog; PcConnection.Dispose();
        // Semaphores remain available for in-flight finally blocks during shutdown.
    }
}
