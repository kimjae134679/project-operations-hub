using System.Collections.ObjectModel;
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
    public WorkDashboardViewModel(Func<CancellationToken, Task<IReadOnlyList<WorkActivity>>> read,
        Func<WorkActivity, Task<string>>? details = null, Func<string, bool>? owns = null, Func<string, bool>? stop = null)
    { _read = read; _details = details ?? (r => Task.FromResult(r.Evidence)); _owns = owns ?? (_ => false); _stop = stop ?? (_ => false); }
    public ObservableCollection<WorkActivity> Activities { get; } = [];
    public IReadOnlyList<WorkActivity> FilteredActivities => Activities.Where(r => (!_managementOnly || r.IsManagementRecord)
        && (string.IsNullOrWhiteSpace(Search) || new[] { r.Project, r.Worker, r.Title, r.StatusLabel, r.Stage, r.Source }.Any(x => x.Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase)))).ToArray();
    public string Search { get => _search; set { if (SetProperty(ref _search, value)) OnPropertyChanged(nameof(FilteredActivities)); } }
    public bool ManagementOnly { get => _managementOnly; set { if (SetProperty(ref _managementOnly, value)) { OnPropertyChanged(nameof(FilteredActivities)); OnPropertyChanged(nameof(Heading)); OnPropertyChanged(nameof(ContextMessage)); } } }
    public bool IsFixture { get; init; }
    public string Heading => ManagementOnly ? "현재 관제 과정" : "전체 작업 현황";
    public string ContextMessage => ManagementOnly ? "명령·답변 기록의 최신 수정본에서 진행·검증·남은 일을 봅니다. 자동 채팅 감시가 아닙니다." : "관제탑·PC 연결·등록된 연속 실행기의 실제 기록을 한곳에서 봅니다. 외부 콘솔을 임의로 감시하지 않습니다.";
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
    public Action<string>? RegisterContinuousPath { get; set; }
    public string SourcePathInput { get; set; } = "";
    public event EventHandler? SnapshotApplying;
    public event EventHandler? SnapshotApplied;
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try { if (!await _gate.WaitAsync(0, ct)) return; }
        catch (OperationCanceledException) { return; }
        try { IsRefreshing = true; ApplySnapshot(await _read(ct)); Message = $"{(IsFixture ? "검증용 가짜 자료 · 실제 실행 아님" : "실제 기록 조회")} · {DateTime.Now:HH:mm:ss} · {Activities.Count}개"; }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Message = "조회 실패 · " + WorkDashboardService.SafeText(ex.Message); }
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
        OnPropertyChanged(nameof(FilteredActivities)); OnPropertyChanged(nameof(CanStopOwned));
        SnapshotApplied?.Invoke(this,EventArgs.Empty);
    }
    public async Task LoadSelectedDetailsAsync()
    {
        if (Selected is not { } selected) return; var version = _selectionVersion;
        try { var result = await _details(selected); if (version == _selectionVersion && Selected?.Id == selected.Id) SelectedDetails = WorkDashboardService.SafeText(result); }
        catch (Exception ex) { if (version == _selectionVersion) SelectedDetails = "상세 조회 실패 · " + WorkDashboardService.SafeText(ex.Message); }
    }
    public void StopSelectedOwned()
    {
        if (Selected?.OwnedProgramId is not { } id || !_owns(id)) { Message = "현재 관제탑이 소유한 실행 작업이 아닙니다. 외부 프로세스는 중지하지 않습니다."; return; }
        Message = _stop(id) ? "선택한 관제탑 소유 작업의 중단을 요청했습니다." : "이미 끝났거나 소유 실행을 찾지 못했습니다.";
        OnPropertyChanged(nameof(CanStopOwned));
    }
    public async Task RegisterSourceAsync()
    {
        try { RegisterContinuousPath?.Invoke(SourcePathInput); Message = "명시한 상태 파일을 연결했습니다."; await RefreshAsync(); }
        catch (Exception ex) { Message = "연결하지 못함 · " + WorkDashboardService.SafeText(ex.Message); }
    }
}
