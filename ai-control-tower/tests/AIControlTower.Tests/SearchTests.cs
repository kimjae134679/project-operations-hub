using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class SearchTests
{
    [Fact]
    public void FilteringRetainsExecutableIdentityAndSupportsEmptyResults()
    {
        using var vm = new MainViewModel(new ControlTowerSettings(), enablePolling: false);
        var sdk = new ProgramItem { Id = "hub/sdk", Name = ".NET 검사기", Kind = "CLI", Commands = [new() { FileName = "dotnet", Arguments = ["--version"] }] };
        var docs = new ProgramItem { Id = "hub/docs", Name = "운영 문서" };
        var project = new ProjectItem { Id = "hub", Name = "통합소통방", Path = @"D:\AI\hub", Functions = [new() { Id = "validation", Name = "검사", Programs = [sdk, docs] }] };
        vm.Projects.Add(project); vm.SelectedProject = project;
        vm.ProjectSearch = " 통합 "; vm.ProgramSearch = ".net";
        Assert.Same(project, Assert.Single(vm.FilteredProjects));
        Assert.Same(sdk, Assert.Single(Assert.Single(vm.FilteredFunctions).Programs));
        vm.ProgramSearch = "없는 프로그램";
        Assert.True(vm.NoProgramSearchResults);
        vm.ProgramSearch = "검사";
        Assert.Equal(2, Assert.Single(vm.FilteredFunctions).Programs.Count);
        vm.ProjectSearch = "없는 프로젝트";
        Assert.True(vm.NoProjectSearchResults);
    }
}
