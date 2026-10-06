using System.Diagnostics;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed partial class MainViewModel
{
    public IReadOnlyList<int> RosterRefreshOptions { get; } = [1, 3, 5, 10, 30];
    public IReadOnlyList<int> RefreshTurnOptions { get; } = [800, 1200, 1800, 2400];
    public int RosterRefreshSeconds
    {
        get => RosterRefreshOptions.Contains(_settings.RosterRefreshSeconds) ? _settings.RosterRefreshSeconds : 5;
        set
        {
            if (!RosterRefreshOptions.Contains(value) || _settings.RosterRefreshSeconds == value) return;
            _settings.RosterRefreshSeconds = value;
            _serverLiveTimer.Interval = TimeSpan.FromSeconds(value);
            SaveSettings(); OnPropertyChanged(); OnPropertyChanged(nameof(RosterStatus));
        }
    }
    public int RefreshTurnMilliseconds
    {
        get => RefreshTurnOptions.Contains(_settings.RefreshTurnMilliseconds) ? _settings.RefreshTurnMilliseconds : 1200;
        set { if (!RefreshTurnOptions.Contains(value) || _settings.RefreshTurnMilliseconds == value) return; _settings.RefreshTurnMilliseconds = value; SaveSettings(); OnPropertyChanged(); }
    }
    private bool _isManualRosterRefresh;
    public bool IsManualRosterRefresh
    {
        get => _isManualRosterRefresh;
        private set { if (SetProperty(ref _isManualRosterRefresh, value)) OnPropertyChanged(nameof(RosterStatus)); }
    }
    private ProjectGuide _projectGuide = new("", "프로젝트를 선택하세요.", "프로젝트 선택 전");
    public string ProjectGuideBody => _projectGuide.Body;
    public string ProjectGuidePath => _projectGuide.Path;
    public string ProjectGuideDirectory => Path.GetDirectoryName(_projectGuide.Path) ?? "";
    public string ProjectGuideStatus => _projectGuide.Status;
    public bool HasProjectGuide => File.Exists(_projectGuide.Path);
    public void LoadProjectGuide()
    {
        if (SelectedProject is null && _discoveryLock.CurrentCount == 0) return; // a catalog rebuild is not a new reading selection
        var next = SelectedProject is { } project ? ProjectGuideService.Read(project.Path) : new("", "프로젝트를 선택하세요.", "프로젝트 선택 전");
        if (next == _projectGuide) return; // preserve the open document and its scroll on unchanged refresh
        _projectGuide = next;
        foreach (var name in new[] { nameof(ProjectGuideBody), nameof(ProjectGuidePath), nameof(ProjectGuideDirectory), nameof(ProjectGuideStatus), nameof(HasProjectGuide) }) OnPropertyChanged(name);
    }
    public void OpenProjectGuide() { if (HasProjectGuide) OpenExisting(ProjectGuidePath); }
    public void OpenToolGuide()
    {
        var guide = Path.Combine(SelectedHubRoot(), "000_사용자용", "02_설치_도구_현황.md");
        if (File.Exists(guide)) OpenExisting(guide);
        else Message = "도구 안내 파일이 아직 연결되지 않았습니다. 전체 프로젝트 관리 폴더를 연결하세요.";
    }
    private string SelectedHubRoot() => Projects.FirstOrDefault(p => p.Functions.Any(f => f.Id == "guides") && File.Exists(Path.Combine(p.Path, "AGENTS.md")))?.Path
        ?? Projects.FirstOrDefault(p => File.Exists(Path.Combine(p.Path, "ai-control-tower", "AIControlTower.sln")))?.Path
        ?? @"D:\A_KJ\AI\Projects\project-operations-hub";
}
