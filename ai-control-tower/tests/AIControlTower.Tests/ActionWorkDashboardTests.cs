using System.Reflection;
using AIControlTower.Models;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class ActionWorkDashboardTests
{
    private static WorkDashboardViewModel Create() => new(_ => Task.FromResult<IReadOnlyList<WorkActivity>>([]));
    private static void Filter(WorkDashboardViewModel vm,string key)
    {
        var property=typeof(WorkDashboardViewModel).GetProperty("StatusFilterKey");
        Assert.NotNull(property);
        property!.SetValue(vm,key);
    }
    private static string Text(WorkDashboardViewModel vm,string property)
    {
        var found=typeof(WorkDashboardViewModel).GetProperty(property);
        Assert.NotNull(found);
        return Assert.IsType<string>(found!.GetValue(vm));
    }
    [Fact]
    public void StateFiltersSeparateConfirmedExecutionFromUncertainAndTerminalRecords()
    {
        var vm=Create();
        vm.ApplySnapshot([
            new() {Id="alive",Status="running",LivenessKnown=true},
            new() {Id="unconfirmed",Status="running",LivenessKnown=false},
            new() {Id="progress",Status="in_progress"},
            new() {Id="queued",Status="queued"},
            new() {Id="accepted",Status="accepted"},
            new() {Id="complete",Status="completed"},
            new() {Id="failed",Status="failed"},
            new() {Id="interrupted",Status="interrupted"},
            new() {Id="unknown",Status="unknown"}
        ]);
        Filter(vm,"active");Assert.Equal(new[]{"alive","progress"},vm.FilteredActivities.Select(x=>x.Id).ToArray());
        Filter(vm,"waiting");Assert.Equal(new[]{"queued","accepted"},vm.FilteredActivities.Select(x=>x.Id).ToArray());
        Filter(vm,"completed");Assert.Equal("complete",Assert.Single(vm.FilteredActivities).Id);
        Filter(vm,"attention");Assert.Equal(new[]{"unconfirmed","failed","interrupted","unknown"},vm.FilteredActivities.Select(x=>x.Id).ToArray());
        Filter(vm,"");Assert.Equal(9,vm.FilteredActivities.Count);
    }
    [Theory]
    [InlineData("error")][InlineData("blocked")][InlineData("stopped")][InlineData("cancelled")]
    [InlineData("timed_out")][InlineData("stale")][InlineData("output_limit")][InlineData("ready")][InlineData("superseded")]
    public void NonCompletedStatesRequireAttentionInsteadOfInventingSuccess(string status)
    {
        var vm=Create();vm.ApplySnapshot([new(){Id="row",Status=status}]);
        Filter(vm,"attention");Assert.Single(vm.FilteredActivities);
        Filter(vm,"completed");Assert.Empty(vm.FilteredActivities);
    }
    [Fact]
    public void StatusFilterIntersectsProjectWorkerAndSearchAndNeverCallsStop()
    {
        var stops=0;
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]),owns:_=>true,stop:_=>{stops++;return true;});
        var row=new WorkActivity{Id="wanted",ProjectId="Threads",Title="표지 검토",WorkerKind="Codex",Status="failed",OwnedProgramId="owned"};
        vm.ApplySnapshot([row,row with {Id="other-project",ProjectId="Other"},row with {Id="other-worker",WorkerKind="Jev"},row with {Id="complete",Status="completed"}]);
        vm.Selected=row;vm.ProjectFilterId="Threads";vm.WorkerFilterKey="Codex";vm.Search="표지";
        Filter(vm,"attention");
        Assert.Equal("wanted",Assert.Single(vm.FilteredActivities).Id);
        Assert.Equal("wanted",vm.Selected?.Id);Assert.Equal(0,stops);
        vm.ApplySnapshot([row with {Stage="원본 보완 필요"}]);
        Assert.Equal("wanted",vm.Selected?.Id);Assert.Equal("원본 보완 필요",vm.Selected?.Stage);
        Assert.Equal(0,stops);
    }
    [Fact]
    public void EmptySearchAndSnapshotCountsAreObservableAndNotPercentages()
    {
        var vm=Create();var changes=new List<string>();
        vm.PropertyChanged+=(_,e)=>changes.Add(e.PropertyName??"");
        vm.ApplySnapshot([new(){Id="one",Title="실제 기록",Status="completed"}]);
        Assert.Contains("1",Text(vm,"ResultCountText"));
        vm.Search="없는 제목";
        Assert.Contains("0",Text(vm,"ResultCountText"));
        var property=typeof(WorkDashboardViewModel).GetProperty("NoMatchingActivities");Assert.NotNull(property);Assert.True((bool)property!.GetValue(vm)!);
        Assert.Contains("ResultCountText",changes);Assert.Contains("NoMatchingActivities",changes);
        Assert.DoesNotContain("%",Text(vm,"ResultCountText"));
    }
}
