using AIControlTower.Models;

namespace AIControlTower.ViewModels;

public sealed class ToolStatusViewModel : ObservableObject
{
    private ToolStatus _status;
    public ToolStatusViewModel(ToolStatus status) => _status = status;
    public string RawName => _status.DisplayName;
    public string DisplayName => RawName switch
    {
        "Remote Desktop Commander" => "PC 원격 연결",
        "Jev Router" => "AI 작업 분배",
        "Codex" => "AI 코드 작업",
        "GitHub CLI" => "GitHub 연결",
        "GitHub Actions · MultiGod-PC" => "자동 빌드 연결",
        _ => RawName
    };
    public bool IsUserFacing => RawName is "Remote Desktop Commander" or "Jev Router" or "Codex" or "GitHub CLI" or "GitHub Actions · MultiGod-PC";
    public string Purpose => RawName switch
    {
        "Remote Desktop Commander" => "AI가 이 PC에서 작업할 수 있게 연결합니다. 현재 연결을 공유해 사용합니다.",
        "Jev Router" => "작업에 맞는 AI 모델을 고르는 선택 도구입니다. 실제 작업 왕복은 아직 확인하지 않았습니다.",
        "Codex" => "프로젝트 코드를 읽고 고치는 AI 작업 도구입니다.",
        "GitHub CLI" => "프로젝트 코드와 작업 기록을 GitHub에 연결합니다.",
        "GitHub Actions · MultiGod-PC" => "등록된 자동 빌드 작업을 이 PC에서 처리하는 연결입니다.",
        _ => ""
    };
    public string State => _status.Kind.ToString();
    public string StateLabel => _status.Kind switch { StatusKind.Running => "실행 중", StatusKind.Ready => "확인됨", StatusKind.Unknown => "미확인", StatusKind.NotConfigured => "설정 필요", _ => "오류" };
    public string Detail => _status.Detail;
    public string CheckedAt => _status.CheckedAt.ToLocalTime().ToString("HH:mm:ss");
    public StatusKind Kind => _status.Kind;
    public void Update(ToolStatus status) { _status = status; OnPropertyChanged(nameof(State)); OnPropertyChanged(nameof(StateLabel)); OnPropertyChanged(nameof(Detail)); OnPropertyChanged(nameof(CheckedAt)); OnPropertyChanged(nameof(Kind)); }
}
