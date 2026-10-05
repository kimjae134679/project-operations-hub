using AIControlTower.ViewModels;

namespace AIControlTower.Models;

public sealed class ProjectItem
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public string Summary { get; init; } = "";
    public string ManifestPath { get; init; } = "";
    public IReadOnlyList<FunctionItem> Functions { get; set; } = [];
    private string _displayName = "";
    public string DisplayName { get => string.IsNullOrWhiteSpace(_displayName) ? Name : _displayName; set => _displayName = value; }
    public string Description { get; set; } = "용도가 아직 확인되지 않은 프로젝트 폴더입니다.";
    public string FolderRole { get; set; } = "unreviewed";
    public string RoleLabel { get; set; } = "새로 발견";
    public string CategoryLabel { get; set; } = "미분류";
    public string FolderName => System.IO.Path.GetFileName(Path.TrimEnd(System.IO.Path.DirectorySeparatorChar));
    public bool IsAuxiliary { get; set; }
    public string ActionLabel { get; set; } = "프로젝트 폴더 보기";
    public string EvidencePath { get; set; } = "";
    public string EvidenceDate { get; set; } = "";
}

public sealed class FunctionItem
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public IReadOnlyList<ProgramItem> Programs { get; init; } = [];
    public bool IsAdvanced { get; init; }
}

public sealed class ProgramItem : ObservableObject
{
    private string _status = "실행 전";
    public string Id { get; init; } = "";
    public string ProjectId { get; init; } = "";
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Detail { get; init; } = "";
    public string Path { get; init; } = "";
    public string WorkingDirectory { get; init; } = "";
    public bool CanLaunch => Commands.Count > 0;
    public IReadOnlyList<ProgramCommand> Commands { get; init; } = [];
    private string _displayName = "";
    public string DisplayName { get => string.IsNullOrWhiteSpace(_displayName) ? Name : _displayName; set => _displayName = value; }
    public string Description { get; set; } = "";
    public string KindLabel { get; set; } = "파일·폴더";
    public string ActionLabel { get; set; } = "폴더 보기";
    public string OpenActionLabel { get; set; } = "위치 보기";
    public string ServiceUrl { get; set; } = "";
    public string EditorOpenScript { get; init; } = "";
    public bool HasEditorLauncher => !string.IsNullOrEmpty(EditorOpenScript);
    public bool IsAdvanced { get; set; }
    public string Status { get => _status; set { if (SetProperty(ref _status, value)) OnPropertyChanged(nameof(StatusLabel)); } }
    public string StatusLabel => !CanLaunch ? "" : Status.Split(" · ")[0] switch
    {
        "succeeded" => "완료",
        "failed" => "실패",
        "cancelled" => "중지됨",
        "timed_out" => "시간 초과",
        "interrupted" => "중단됨",
        "queued" => "대기 중",
        "running" => "실행 중",
        "등록됨" => "실행 전",
        _ => Status
    };
}

public sealed class ProgramCommand
{
    public string Name { get; init; } = "실행";
    public string FileName { get; init; } = "";
    public string[] Arguments { get; init; } = [];
    public int TimeoutSeconds { get; init; } = 0;
}

public sealed class ProjectManifest
{
    public int SchemaVersion { get; set; } = 1;
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ManifestFunction> Functions { get; set; } = [];
}

public sealed class ManifestFunction
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ManifestProgram> Programs { get; set; } = [];
}

public sealed class ManifestProgram
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "program";
    public string Path { get; set; } = ".";
    public string WorkingDirectory { get; set; } = ".";
    public string Description { get; set; } = "";
    public List<ProgramCommand> Commands { get; set; } = [];
}
