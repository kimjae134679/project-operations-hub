namespace AIControlTower.Models;

/// <summary>One current project, with an explicitly resolved communication identity (or no mapping).</summary>
public sealed record CommunicationProjectTarget(string ProjectId, string Name, string? CommunicationId);

public sealed record NoticeProjectCheck(string ProjectId, string Name, string? CommunicationId, bool IsApplicable, bool IsChecked)
{
    public bool IsMapped => !string.IsNullOrWhiteSpace(CommunicationId);
    public bool CanCheck => IsMapped && IsApplicable;
    public string StatusLabel => !IsMapped ? "연결 미확인" : !IsApplicable ? "해당 없음" : "";
    public string AutomationLabel => $"{Name} · {(IsChecked ? "확인" : StatusLabel.Length > 0 ? StatusLabel : "미확인")}";
}
