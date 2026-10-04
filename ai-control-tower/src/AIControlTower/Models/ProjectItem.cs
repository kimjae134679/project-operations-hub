using AIControlTower.ViewModels;

namespace AIControlTower.Models;

public sealed class ProjectItem
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public string Summary { get; init; } = "";
    public string ManifestPath { get; init; } = "";
    public IReadOnlyList<FunctionItem> Functions { get; init; } = [];
}

public sealed class FunctionItem
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public IReadOnlyList<ProgramItem> Programs { get; init; } = [];
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
    public string Status { get => _status; set => SetProperty(ref _status, value); }
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
