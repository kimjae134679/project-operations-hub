namespace AIControlTower.Models;

/// <summary>A reported observation, not authority to execute or terminate an external process.</summary>
public sealed record WorkActivity
{
    public string Id { get; init; } = "";
    public string Project { get; init; } = "미확인";
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
    public string UpdatedLabel => UpdatedAt?.ToLocalTime().ToString("MM-dd HH:mm:ss") ?? "정보 없음";
    public string StatusLabel => Status switch
    {
        "succeeded" or "completed" => "완료 기록", "running" => LivenessKnown ? "실행 중" : "실행 기록 · 생존 미확인",
        "queued" or "pending" => "대기", "accepted" => "접수 · 완료 미확인", "failed" or "error" => "오류",
        "blocked" => "막힘", "stopped" or "cancelled" => "중단", "timed_out" => "시간 초과",
        "interrupted" => "결과 불명", "in_progress" => "진행 기록", "ready" => "연결 준비", "stale" => "오래된 기록", _ => "정보 없음"
    };
}
