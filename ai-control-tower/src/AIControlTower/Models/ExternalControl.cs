namespace AIControlTower.Models;

/// <summary>Finite JSON commands only. The external controller owns the service and its health semantics.</summary>
public sealed class ExternalControl
{
    public ExternalControlAssociation? Association { get; set; }
    public ProgramCommand? Status { get; set; }
    public ProgramCommand? Start { get; set; }
    public ProgramCommand? Stop { get; set; }
    public ProgramCommand? Settings { get; set; }
    public List<ExternalControlField> Fields { get; set; } = [];
    public List<ExternalControlOption> Options { get; set; } = [];
    public IReadOnlyList<ProgramCommand> Actions => new[] { Status, Start, Stop, Settings }.OfType<ProgramCommand>().ToArray();
}

public sealed class ExternalControlField
{
    public string Role { get; set; } = "";
    public bool IsAdvanced { get; set; }
    public string Label { get; set; } = "";
    public string Path { get; set; } = "";
    public string TrueLabel { get; set; } = "예";
    public string FalseLabel { get; set; } = "아니오";
    public string NullLabel { get; set; } = "미확인";
}

public sealed class ExternalControlAssociation
{
    public string Role { get; set; } = "";
    public string Root { get; set; } = "";
}

public sealed class ExternalControlOption
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Path { get; set; } = "";
    public ProgramCommand? Enable { get; set; }
    public ProgramCommand? Disable { get; set; }
}
