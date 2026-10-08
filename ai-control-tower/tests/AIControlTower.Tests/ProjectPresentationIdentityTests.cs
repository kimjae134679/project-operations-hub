using System.Reflection;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
namespace AIControlTower.Tests;
public sealed class ProjectPresentationIdentityTests
{
    private static MainViewModel Create()=>new(new ControlTowerSettings{IsTemporary=true,TransientReadOnly=true,AutoCommunication=false,AutoPublishCommunication=false},false,()=>throw new InvalidOperationException("Recovery must not run in read-only verification"),new StartupPolicy(true),null);
    [Fact]
    public void CatalogPathConnectsRecordsWithoutChangingDiscoveredExecutionIdentity()
    {
        var entry=ProjectCatalogService.Load().Projects.Single(p=>p.Id=="Threads");
        using var vm=Create();var selected=new ProjectItem{Id="folder:discovered-fixture",Path=entry.Path,DisplayName="검증용 이름"};
        vm.SelectedProject=selected;
        vm.WorkDashboard.ApplySnapshot([new(){Id="record",ProjectId=entry.Id,Source="명령·답변 task_exchange 기록",ResultSummary="fixture result"}]);
        Assert.Equal("record",Assert.Single(vm.WorkDashboard.ProjectRecords).Id);
        Assert.Equal("folder:discovered-fixture",vm.SelectedProject.Id);
        Assert.False(vm.CanRunRegisteredTasks);Assert.False(vm.CanReadLiveStatus);
    }
    [Fact]
    public void CanonicalProgressKeepsExactRootBoundaryAndDoesNotUseDisplayName()
    {
        var entry=ProjectCatalogService.Load().Projects.Single(p=>p.Id=="audiobook");
        using var vm=Create();vm.SelectedProject=new(){Id="folder:discovered-fixture",Path=entry.Path,DisplayName="검증용 이름"};
        var at=DateTimeOffset.UtcNow;
        var field=typeof(MainViewModel).GetField("_projectProgressSummaries",BindingFlags.NonPublic|BindingFlags.Instance)!;
        field.SetValue(vm,new ProjectProgressSnapshot[]{new(entry.Id,entry.Name,at){ProjectRoot=entry.Path,Completed=2,Total=4}});
        Assert.True(vm.SelectedProjectProgress.IsKnown);
        vm.SelectedProject=new(){Id="folder:other",Path=Path.Combine(Path.GetTempPath(),"unregistered-fixture"),DisplayName=entry.Name};
        Assert.False(vm.SelectedProjectProgress.IsKnown);
        Assert.Empty(vm.WorkDashboard.ProjectRecords);
    }
    [Fact]
    public void ManagementProjectActionUsesManagementRowsInTheSelectedContext()
    {
        var entry=ProjectCatalogService.Load().Projects.Single(p=>p.Id=="project-operations-hub");
        using var vm=Create();vm.SelectedProject=new(){Id="folder:manager-fixture",Path=entry.Path,DisplayName="관리 프로젝트"};
        vm.WorkDashboard.ApplySnapshot([new(){Id="manager",ProjectId="Control-Tower",IsManagementRecord=true,Status="queued"},new(){Id="foreign",ProjectId="Threads",Status="failed"},new(){Id="execution",ProjectId="Control-Tower",IsManagementRecord=false,Status="failed"}]);
        vm.ManagementDashboard.ApplySnapshotFrom(vm.WorkDashboard);
        var property=typeof(MainViewModel).GetProperty("SelectedProjectDashboard");Assert.NotNull(property);
        var selected=Assert.IsType<WorkDashboardViewModel>(property!.GetValue(vm));
        Assert.Same(vm.ManagementDashboard,selected);
        Assert.Contains("기록 1",selected.ProjectOverviewCounts);Assert.Contains("확인 필요 0",selected.ProjectOverviewCounts);
        selected.ProjectFilterId="project-operations-hub";selected.StatusFilterKey="needs-action";
        Assert.Equal("manager",Assert.Single(selected.FilteredActivities).Id);
    }
}
