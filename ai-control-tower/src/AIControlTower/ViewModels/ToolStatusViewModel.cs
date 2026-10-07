using AIControlTower.Models;

namespace AIControlTower.ViewModels;

public sealed class ToolStatusViewModel : ObservableObject
{
    private ToolStatus _status;
    private string _usageEvidence="사용 근거 없음 · 설치/프로세스 존재만으로 사용·성공을 판단하지 않습니다.";
    private bool _hasUsageEvidence;
    public ToolStatusViewModel(ToolStatus status) => _status = status;
    public string Id => _status.Id;
    public string RawName => Id switch
    {
        "project-bridge"=>"ProjectBridge","desktop-commander"=>"Remote Desktop Commander","jev"=>"Jev Router",
        "codex"=>"Codex","github-cli"=>"GitHub CLI","n8n"=>"n8n local bridge","ai-ops-runner"=>"GitHub Actions · MultiGod-PC",
        "delivery-chain"=>"GPT → Jev 전달","aider"=>"Aider","hyperframes"=>"HyperFrames","voicestudio"=>"VoiceStudio","zonos2"=>"Zonos2",
        _=>_status.DisplayName
    };
    public bool HasUsageEvidence=>_hasUsageEvidence;
    public string UsageEvidence=>_usageEvidence;
    public void SetUsageEvidence(string? evidence)
    {
        _hasUsageEvidence=!string.IsNullOrWhiteSpace(evidence);
        _usageEvidence=_hasUsageEvidence?evidence!:"사용 근거 없음 · 설치/프로세스 존재만으로 사용·성공을 판단하지 않습니다.";
        OnPropertyChanged(nameof(HasUsageEvidence));OnPropertyChanged(nameof(UsageEvidence));
    }
    public string DisplayName => RawName switch
    {
        "ProjectBridge" => "ProjectBridge · 공용 PC 작업",
        "Remote Desktop Commander" => "Remote Desktop Commander · PC 원격 연결",
        "Jev Router" => "Jev Router · AI 작업 분배",
        "Codex" => "Codex · AI 코드 작업",
        "GitHub CLI" => "GitHub CLI · 저장소 연결",
        "GitHub Actions · MultiGod-PC" => "GitHub Actions · 자동 빌드 연결",
        "n8n local bridge" => "n8n · 작업 자동화",
        "GPT → Jev 전달" => "GPT → Jev · 작업 전달 상태",
        "Aider" => "Aider · AI 코드 수정",
        "HyperFrames" => "HyperFrames · 영상 제작",
        "VoiceStudio" => "VoiceStudio · 음성 제작",
        "Zonos2" => "Zonos2 · 음성 모델",
        _ => RawName
    };
    // Every registered provider stays discoverable, including missing or unconfigured tools.
    public bool IsUserFacing => true;
    public string ActionLabel => Id switch
    {
        "project-bridge" => "공용 PC 작업",
        "desktop-commander" => "원격 연결 화면",
        "jev" => "Jev 작업 전달",
        "n8n" => "자동화 화면",
        "aider" or "hyperframes" or "codex" or "github-cli" => "설치 위치 보기",
        _ => "사용 안내"
    };
    public string Category => Id switch
    {
        "project-bridge" => "PC 연결",
        "desktop-commander" => "PC 연결",
        "jev" or "codex" or "aider" => "AI 작업",
        "github-cli" => "저장소",
        "n8n" or "ai-ops-runner" or "delivery-chain" => "자동화",
        "hyperframes" => "영상 제작",
        "voicestudio" or "zonos2" => "음성 제작",
        _ => "기타 도구"
    };
    public string Purpose => RawName switch
    {
        "ProjectBridge" => "여러 도구와 프로젝트가 같은 PC 연결에서 파일·빌드·명령·화면 작업을 나눠 처리합니다.",
        "Remote Desktop Commander" => "AI가 이 PC에서 작업할 수 있게 연결합니다. 현재 연결을 공유해 사용합니다.",
        "Jev Router" => "작업에 맞는 AI 모델을 고르는 선택 도구입니다. 실제 작업 왕복은 아직 확인하지 않았습니다.",
        "Codex" => "프로젝트 코드를 읽고 고치는 AI 작업 도구입니다.",
        "GitHub CLI" => "프로젝트 코드와 작업 기록을 GitHub에 연결합니다.",
        "GitHub Actions · MultiGod-PC" => "등록된 자동 빌드 작업을 이 PC에서 처리하는 연결입니다.",
        "n8n local bridge" => "정해 둔 작업 흐름을 자동으로 실행하는 도구입니다. 서버 응답과 개별 작업 완료는 구분합니다.",
        "GPT → Jev 전달" => "외부 GPT 세션에서 Jev로 작업이 전달됐는지 확인하는 항목입니다. 전달 여부는 별도 확인이 필요합니다.",
        "Aider" => "프로젝트 코드를 AI와 함께 수정하는 도구입니다. 설치 확인과 실제 작업 성공은 구분합니다.",
        "HyperFrames" => "코드로 영상을 제작하는 도구입니다. 설치 확인과 실제 렌더링 성공은 구분합니다.",
        "VoiceStudio" => "음성·더빙 작업 도구입니다. 안내에서 실행기와 음원·배역·작업 폴더를 확인합니다.",
        "Zonos2" => "음성 모델 도구입니다. 안내에서 기존 모델 위치와 실행 조건을 확인합니다. 모델 연결은 미검증입니다.",
        _ => "등록된 도구의 확인 결과입니다."
    };
    public string State => _status.Kind.ToString();
    public string StateLabel => _status.Kind switch
    {
        StatusKind.Running => "실행 중",
        StatusKind.Ready when Id is "aider" or "hyperframes" or "voicestudio" or "zonos2" => "설치 확인",
        StatusKind.Ready => "확인됨",
        StatusKind.Unknown => "미확인",
        StatusKind.NotInstalled => "미설치",
        StatusKind.NotConfigured => "설정 필요",
        _ => "오류"
    };
    public string Detail => _status.Detail;
    public string CheckedAt => _status.CheckedAt.ToLocalTime().ToString("HH:mm:ss");
    public StatusKind Kind => _status.Kind;
    public void Update(ToolStatus status)
    {
        _status = status;
        foreach (var property in new[] { nameof(Id), nameof(RawName), nameof(DisplayName), nameof(IsUserFacing),
            nameof(Category), nameof(Purpose), nameof(ActionLabel), nameof(State), nameof(StateLabel), nameof(Detail), nameof(CheckedAt), nameof(Kind) })
            OnPropertyChanged(property);
    }
}
