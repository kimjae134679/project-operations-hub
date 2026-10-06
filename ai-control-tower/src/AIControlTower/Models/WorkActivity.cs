namespace AIControlTower.Models;

/// <summary>A reported observation, not authority to execute or terminate an external process.</summary>
public sealed record WorkActivity
{
    public string Id { get; init; } = "";
    public string Project { get; init; } = "미확인";
    public string ProjectId { get; init; } = "";
    public string ReportedProjectId { get; init; } = "";
    public string ProjectDisplayName { get; init; } = "";
    public string ProgramId { get; init; } = "";
    public string WorkerKind { get; init; } = "미확인";
    public string CommandSummary { get; init; } = "명령 메타데이터 미확인";
    public string ResultSummary { get; init; } = "결과 메타데이터 미확인";
    public string ProcessStatus { get; init; } = "unknown";
    public string AICompletionState { get; init; } = "unknown";
    public string Worker { get; init; } = "미확인";
    public string Source { get; init; } = "";
    public string Title { get; init; } = "";
    public string Status { get; init; } = "unknown";
    public string Stage { get; init; } = "";
    public string Evidence { get; init; } = "연결된 작업 기록이 없습니다.";
    public string RecentLog { get; init; } = "";
    public string Error { get; init; } = "";
    public string NextCheckpoint { get; init; } = "다음 단계 미확인";
    public DateTimeOffset? UpdatedAt { get; init; }
    public bool LivenessKnown { get; init; }
    public string? OwnedProgramId { get; init; }
    public string? DetailPath { get; init; }
    public string? BridgeJobId { get; init; }
    public bool IsManagementRecord { get; init; }
    public int Revision { get; init; }
    public string ProjectLabel => ProjectDisplayName.Length > 0 ? ProjectDisplayName : ProjectId.Length > 0 ? ProjectId : Project;
    public string ProjectGroupKey => ProjectId.Length > 0 ? ProjectId : "";
    public string ProjectGroupLabel => ProjectId.Length > 0 ? ProjectLabel + " · [" + ProjectId + "]" : "프로젝트 연결 미확인";
    public string UpdatedLabel => UpdatedAt?.ToLocalTime().ToString("MM-dd HH:mm:ss") ?? "정보 없음";
    public string StatusLabel => Status switch
    {
        "succeeded" or "completed" => "완료 기록", "running" => LivenessKnown ? "실행 중" : "실행 기록 · 생존 미확인",
        "queued" or "pending" => "대기", "accepted" => "접수 · 완료 미확인", "failed" or "error" => "오류",
        "blocked" => "막힘", "stopped" or "cancelled" => "중단", "timed_out" => "시간 초과",
        "interrupted" => "결과 불명", "in_progress" => "진행 기록", "ready" => "연결 준비", "stale" => "오래된 기록", _ => "정보 없음"
    };
}

public sealed record WorkFilterOption(string Id, string Label);
public sealed record WorkWorkerGroup(string WorkerKey, IReadOnlyList<WorkActivity> Activities);
public sealed record WorkProjectGroup(string ProjectId, string DisplayName, IReadOnlyList<WorkWorkerGroup> Workers);

/// <summary>Bounded in-memory reading positions. No window or persistent setting side effects.</summary>
public sealed class WorkReadingPositionStore
{
    private readonly Dictionary<string,(double Stage,double Log)> _positions = new(StringComparer.Ordinal);
    public void Save(string id,double stage,double log)
    {
        if(string.IsNullOrEmpty(id))return;
        if(_positions.Count>=100 && !_positions.ContainsKey(id))_positions.Remove(_positions.Keys.First());
        _positions[id]=(double.IsFinite(stage)?Math.Max(0,stage):0,double.IsFinite(log)?Math.Max(0,log):0);
    }
    public (double Stage,double Log) Read(string id)=>_positions.TryGetValue(id,out var offsets)?offsets:(0,0);
}
