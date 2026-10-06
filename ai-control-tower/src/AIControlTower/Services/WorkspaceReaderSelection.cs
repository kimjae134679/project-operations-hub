using AIControlTower.Models;
using AIControlTower.ViewModels;

namespace AIControlTower.Services;

/// <summary>Window-memory navigation only: no settings, files, receipts, APIs or execution.</summary>
public sealed record WorkspaceReaderSelection(string? ProjectId,string? ProgramId,ProgramCommand? Command,
    string ProjectSearch,string ProgramSearch,string ToolSearch,string WorkSearch,string ManagementSearch)
{
    public static WorkspaceReaderSelection Capture(MainViewModel vm)=>new(vm.SelectedProject?.Id,vm.SelectedProgram?.Id,
        vm.SelectedCommand is { } c?new(){Name=c.Name,FileName=c.FileName,Arguments=c.Arguments.ToArray(),TimeoutSeconds=c.TimeoutSeconds}:null,
        vm.ProjectSearch,vm.ProgramSearch,vm.ToolSearch,vm.WorkDashboard.Search,vm.ManagementDashboard.Search);
    public void Restore(MainViewModel vm)
    {
        var project=vm.Projects.FirstOrDefault(p=>p.Id==ProjectId);
        vm.SelectedProject=project;
        vm.SelectedProgram=project?.Functions.SelectMany(f=>f.Programs).FirstOrDefault(p=>p.Id==ProgramId);
        vm.SelectedCommand=Command is { } command?vm.SelectedProgram?.Commands.FirstOrDefault(c=>c.Name==command.Name && c.FileName==command.FileName && c.Arguments.SequenceEqual(command.Arguments)):null;
        // Restore search after setters: changing project/program resets dependent selection/query.
        vm.ProjectSearch=ProjectSearch;vm.ProgramSearch=ProgramSearch;vm.ToolSearch=ToolSearch;
        vm.WorkDashboard.Search=WorkSearch;vm.ManagementDashboard.Search=ManagementSearch;
    }
}
