using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using AIControlTower.Models;
using AIControlTower.Services;
namespace AIControlTower.ViewModels;

public sealed class WorkDashboardViewModel : ObservableObject
{
    private readonly Func<CancellationToken, Task<IReadOnlyList<WorkActivity>>> _read;
    private readonly Func<WorkActivity, Task<string>> _details;
    private readonly Func<string, bool> _owns;
    private readonly Func<string, bool> _stop;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string _search = "", _message = "연결된 실제 작업 기록만 표시합니다.", _detailsText = "작업을 선택하세요.";
    private WorkActivity? _selected;
    private bool _refreshing, _managementOnly;
    private int _selectionVersion;
    private int _detailRequestVersion;
    private string _projectFilterId = "", _workerFilterKey = "", _statusFilterKey = "";
    private string _currentProjectId="",_currentProjectName="";
    private WorkActivity? _selectedRecord,_selectedExecution;
    public string CurrentProjectName => _currentProjectName;
    // Explicit catalog identity only; current project does not depend on legacy UI filters or actor names.
    public IReadOnlyList<WorkActivity> ProjectRecords => Activities.Where(r=>_currentProjectId.Length>0 && r.ProjectId==_currentProjectId && r.IsConversation)
        .OrderByDescending(r=>r.UpdatedAt).ThenByDescending(r=>r.Revision).Take(200).ToArray();
    public IReadOnlyList<WorkActivity> ProjectExecutions => Activities.Where(r=>_currentProjectId.Length>0 && r.ProjectId==_currentProjectId && r.IsRegisteredExecution)
        .OrderByDescending(r=>r.UpdatedAt).Take(200).ToArray();
    public bool NoProjectRecords => ProjectRecords.Count==0;
    public bool NoProjectExecutions => ProjectExecutions.Count==0;
    public WorkActivity? SelectedRecord { get=>_selectedRecord;set=>SetProperty(ref _selectedRecord,value); }
    public WorkActivity? SelectedExecution { get=>_selectedExecution;set {if(SetProperty(ref _selectedExecution,value))OnPropertyChanged(nameof(CanStopExecution));} }
    public bool CanStopExecution => SelectedExecution?.OwnedProgramId is {} id && _owns(id);
    public void StopSelectedExecutionOwned()
    {
        if(!CanStopExecution)return;
        Selected=SelectedExecution;StopSelectedOwned();OnPropertyChanged(nameof(CanStopExecution));
    }
    public void SetProjectContext(string projectId,string name)
    {
        var changed=_currentProjectId!=projectId;
        _currentProjectId=projectId??"";_currentProjectName=WorkDashboardService.SafeText(name??"");
        if(changed) { _selectedRecord=null;_selectedExecution=null; }
        NotifyProjectContext();
    }
    private void NotifyProjectContext()
    {
        _selectedRecord=ProjectRecords.FirstOrDefault(r=>r.Id==_selectedRecord?.Id)??ProjectRecords.FirstOrDefault();
        _selectedExecution=ProjectExecutions.FirstOrDefault(r=>r.Id==_selectedExecution?.Id)??ProjectExecutions.FirstOrDefault();
        foreach(var name in new[]{nameof(CurrentProjectName),nameof(ProjectRecords),nameof(ProjectExecutions),nameof(NoProjectRecords),nameof(NoProjectExecutions),nameof(SelectedRecord),nameof(SelectedExecution),nameof(CanStopExecution)})OnPropertyChanged(name);
    }
    public WorkDashboardViewModel(Func<CancellationToken, Task<IReadOnlyList<WorkActivity>>> read,
        Func<WorkActivity, Task<string>>? details = null, Func<string, bool>? owns = null, Func<string, bool>? stop = null)
    { _read = read; _details = details ?? (r => Task.FromResult(r.Evidence)); _owns = owns ?? (_ => false); _stop = stop ?? (_ => false); }
    public ObservableCollection<WorkActivity> Activities { get; } = [];
    private IEnumerable<WorkActivity> ScopedActivities => Activities.Where(r => r.IsManagementRecord == _managementOnly);
    public IReadOnlyList<WorkActivity> FilteredActivities => ScopedActivities.Where(r => (ProjectFilterId.Length==0 || (r.ProjectGroupKey.Length==0?"__unassigned":r.ProjectGroupKey)==ProjectFilterId)
        && (WorkerFilterKey.Length==0 || r.WorkerKind==WorkerFilterKey)
        && (StatusFilterKey.Length==0 || StateGroup(r)==StatusFilterKey)
        && (string.IsNullOrWhiteSpace(Search) || new[] { r.ProjectLabel, r.ProjectId, r.Worker, r.WorkerKind, r.ProgramId, r.Title, r.StatusLabel, r.Stage, r.Source }.Any(x => x.Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase)))).ToArray();
    // Groups describe reported evidence only; filtering never submits, pauses or completes jobs.
    private static string StateGroup(WorkActivity row) => row.Status switch
    {
        "completed" or "succeeded" => "completed",
        "running" when row.LivenessKnown => "active",
        "in_progress" => "active",
        "queued" or "pending" or "accepted" => "waiting",
        _ => "attention"
    };
    public IReadOnlyList<WorkFilterOption> StatusFilters { get; } =
    [
        new("", "모든 상태"), new("active", "실행·진행 기록"),
        new("waiting", "대기·접수"), new("attention", "오류·중단·확인 필요"),
        new("completed", "완료 기록")
    ];
    public string StatusFilterKey { get => _statusFilterKey; set { if (SetProperty(ref _statusFilterKey, value ?? "")) NotifyFilters(); } }
    public bool NoMatchingActivities => FilteredActivities.Count == 0;
    public string ResultCountText => $"조건에 맞는 실제 기록 {FilteredActivities.Count}개";
    public IReadOnlyList<WorkFilterOption> ProjectFilters
    {
        get
        {
            var options = ScopedActivities.GroupBy(r=>r.ProjectGroupKey,StringComparer.Ordinal)
                .Select(g=>new WorkFilterOption(g.Key.Length==0?"__unassigned":g.Key,g.First().ProjectGroupLabel)).OrderBy(p=>p.Label).ToList();
            if(ProjectFilterId.Length>0 && !options.Any(p=>p.Id==ProjectFilterId))options.Add(new(ProjectFilterId,ProjectFilterId+" · 현재 기록 없음"));
            return new[] { new WorkFilterOption("","모든 프로젝트") }.Concat(options).ToArray();
        }
    }
    public IReadOnlyList<WorkFilterOption> WorkerFilters
    {
        get
        {
            var options=ScopedActivities.Where(r=>ProjectFilterId.Length==0 || (r.ProjectGroupKey.Length==0?"__unassigned":r.ProjectGroupKey)==ProjectFilterId)
                .Select(r=>r.WorkerKind).Distinct(StringComparer.Ordinal).OrderBy(s=>s).Select(s=>new WorkFilterOption(s,s)).ToList();
            if(WorkerFilterKey.Length>0 && !options.Any(p=>p.Id==WorkerFilterKey))options.Add(new(WorkerFilterKey,WorkerFilterKey+" · 현재 기록 없음"));
            return new[] { new WorkFilterOption("","모든 실행기") }.Concat(options).ToArray();
        }
    }
    public IReadOnlyList<WorkProjectGroup> ProjectGroups => FilteredActivities.GroupBy(r=>r.ProjectGroupKey,StringComparer.Ordinal)
        .Select(g=>new WorkProjectGroup(g.Key,g.First().ProjectLabel,g.GroupBy(r=>r.WorkerKind,StringComparer.Ordinal).Select(w=>new WorkWorkerGroup(w.Key,w.ToArray())).ToArray())).ToArray();
    public ICollectionView GroupedActivities
    {
        get
        {
            var view=new ListCollectionView(FilteredActivities.ToArray());
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(WorkActivity.ProjectGroupLabel)));
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(WorkActivity.WorkerKind)));
            return view;
        }
    }
    private void NotifyFilters()
    {
        OnPropertyChanged(nameof(FilteredActivities));OnPropertyChanged(nameof(GroupedActivities));OnPropertyChanged(nameof(ProjectGroups));
        OnPropertyChanged(nameof(ProjectFilters));OnPropertyChanged(nameof(WorkerFilters));
        OnPropertyChanged(nameof(ResultCountText));OnPropertyChanged(nameof(NoMatchingActivities));
    }
    public string ProjectFilterId { get=>_projectFilterId;set { if(SetProperty(ref _projectFilterId,value??""))NotifyFilters(); } }
    public string WorkerFilterKey { get=>_workerFilterKey;set { if(SetProperty(ref _workerFilterKey,value??""))NotifyFilters(); } }
    public string Search { get => _search; set { if (SetProperty(ref _search, value)) NotifyFilters(); } }
    public bool ManagementOnly { get => _managementOnly; set { if (SetProperty(ref _managementOnly, value)) { NotifyFilters(); OnPropertyChanged(nameof(Heading)); OnPropertyChanged(nameof(ContextMessage)); } } }
    public bool IsFixture { get; init; }
    public string Heading => ManagementOnly ? "통합관리 진행" : "프로젝트 작업";
    public string ContextMessage => ManagementOnly ? "명시된 관리자 프로젝트 기록의 최신 수정본에서 진행·검증·남은 일을 봅니다. 자동 채팅 감시가 아닙니다." : "프로젝트 → 등록 실행기별 명령·진행·결과를 봅니다. 미등록 외부 콘솔은 연결 미확인으로 남습니다.";
    public bool CanRegisterSource => RegisterContinuousPath is not null;
    public string Message { get => _message; private set => SetProperty(ref _message, value); }
    public string SelectedDetails { get => _detailsText; private set => SetProperty(ref _detailsText, value); }
    public bool IsRefreshing { get => _refreshing; private set => SetProperty(ref _refreshing, value); }
    public WorkActivity? Selected
    {
        get => _selected;
        set
        {
            var previousId=_selected?.Id;
            if (!SetProperty(ref _selected, value)) return;
            if(previousId!=value?.Id) { _selectionVersion++; SelectedDetails=value?.Evidence ?? "작업을 선택하세요."; }
            OnPropertyChanged(nameof(CanStopOwned));
        }
    }
    public bool CanStopOwned => Selected?.OwnedProgramId is { } id && _owns(id);
    public Func<string, bool>? RegisterContinuousPath { get; set; }
    public string SourcePathInput { get; set; } = "";
    public event EventHandler? SnapshotApplying;
    public event EventHandler? SnapshotApplied;
    public async Task RefreshAsync(CancellationToken ct = default)
        => _ = await RefreshCoreAsync(ct);
    private enum RefreshOutcome { Completed, Inflight, Cancelled, Failed }
    private async Task<RefreshOutcome> RefreshCoreAsync(CancellationToken ct)
    {
        try { if (!await _gate.WaitAsync(0, ct)) return RefreshOutcome.Inflight; }
        catch (OperationCanceledException) { return RefreshOutcome.Cancelled; }
        try { IsRefreshing = true; ApplySnapshot(await _read(ct)); Message = $"{(IsFixture ? "검증용 가짜 자료 · 실제 실행 아님" : "실제 기록 조회")} · {DateTime.Now:HH:mm:ss} · {Activities.Count}개"; return RefreshOutcome.Completed; }
        catch (OperationCanceledException) { return RefreshOutcome.Cancelled; }
        catch (Exception ex) { Message = "조회 실패 · " + WorkDashboardService.SafeText(ex.Message); return RefreshOutcome.Failed; }
        finally { IsRefreshing = false; _gate.Release(); }
    }
    public void ApplySnapshot(IReadOnlyList<WorkActivity> rows)
    {
        SnapshotApplying?.Invoke(this,EventArgs.Empty);
        var id = Selected?.Id;
        // Keep identities/positions stable so polling does not reset selection or scroll.
        var incoming = rows.ToDictionary(r => r.Id, StringComparer.Ordinal);
        for (var i = Activities.Count - 1; i >= 0; i--) if (!incoming.ContainsKey(Activities[i].Id)) Activities.RemoveAt(i);
        for (var i = 0; i < Activities.Count; i++) { Activities[i] = incoming[Activities[i].Id]; incoming.Remove(Activities[i].Id); }
        foreach (var row in incoming.Values) Activities.Add(row);
        Selected = id is null ? null : Activities.FirstOrDefault(r => r.Id == id);
        NotifyFilters(); OnPropertyChanged(nameof(CanStopOwned));
        NotifyProjectContext();
        SnapshotApplied?.Invoke(this,EventArgs.Empty);
    }
    public async Task LoadSelectedDetailsAsync()
    {
        if (Selected is not { } selected) return; var version = _selectionVersion; var requestVersion=++_detailRequestVersion;
        try { var result = await _details(selected); if (version == _selectionVersion && requestVersion==_detailRequestVersion && Selected?.Id == selected.Id) SelectedDetails = WorkDashboardService.SafeText(result); }
        catch (Exception ex) { if (version == _selectionVersion && requestVersion==_detailRequestVersion) SelectedDetails = "상세 조회 실패 · " + WorkDashboardService.SafeText(ex.Message); }
    }
    public void StopSelectedOwned()
    {
        if (Selected?.OwnedProgramId is not { } id || !_owns(id)) { Message = "현재 관제탑이 소유한 실행 작업이 아닙니다. 외부 프로세스는 중지하지 않습니다."; return; }
        Message = _stop(id) ? "선택한 관제탑 소유 작업의 중단을 요청했습니다." : "이미 끝났거나 소유 실행을 찾지 못했습니다.";
        OnPropertyChanged(nameof(CanStopOwned));
    }
    public async Task RegisterSourceAsync()
    {
        try
        {
            var register = RegisterContinuousPath;
            if (register is null) { Message = "연결하지 못함 · 상태 파일 연결 경로 없음 · 작업 실행 없음"; return; }
            if (!register(SourcePathInput)) { Message = "상태 파일 연결이 거절되었습니다 · 기존 연결 유지 · 작업 실행 없음"; return; }
            // Registration and reading are separate outcomes; a failed read must remain visible.
            switch (await RefreshCoreAsync(CancellationToken.None))
            {
                case RefreshOutcome.Completed: Message = "명시한 상태 파일을 연결했습니다 · 기록 조회 확인 · 작업 실행 없음"; break;
                case RefreshOutcome.Cancelled: Message = "상태 파일 연결 확인 · 조회 취소 · 작업 실행 없음"; break;
                case RefreshOutcome.Inflight: Message = "상태 파일 연결 확인 · 기존 조회 진행 중 · 작업 실행 없음"; break;
            }
        }
        catch (OperationCanceledException) { Message = "상태 파일 연결 취소 · 기존 연결 유지 · 작업 실행 없음"; }
        catch (Exception ex) { Message = "연결하지 못함 · " + WorkDashboardService.SafeText(ex.Message); }
    }
}
