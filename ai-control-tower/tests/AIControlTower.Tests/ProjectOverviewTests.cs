using AIControlTower.Models;
using AIControlTower.ViewModels;
namespace AIControlTower.Tests;
public sealed class ProjectOverviewTests
{
    private static string Text(WorkDashboardViewModel vm,string name)
    {
        var property=typeof(WorkDashboardViewModel).GetProperty(name);
        Assert.NotNull(property);
        return Assert.IsType<string>(property!.GetValue(vm));
    }
    [Fact]
    public void OverviewCountsOnlySelectedProjectAndDoesNotTurnRecordCountsIntoProductionPercent()
    {
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.SetProjectContext("Threads","콘텐츠 제작");
        vm.ApplySnapshot([new(){Id="one",ProjectId="Threads",Status="completed"},new(){Id="two",ProjectId="Threads",Status="queued"},new(){Id="three",ProjectId="Threads",Status="running",LivenessKnown=false},new(){Id="other",ProjectId="audiobook",Status="failed"}]);
        Assert.Equal("기록 3 · 완료 기록 1 · 대기 1 · 확인 필요 1",Text(vm,"ProjectOverviewCounts"));
        Assert.DoesNotContain("%",Text(vm,"ProjectOverviewCounts"));
        vm.Search="없는 항목";vm.StatusFilterKey="completed";
        Assert.Equal("기록 3 · 완료 기록 1 · 대기 1 · 확인 필요 1",Text(vm,"ProjectOverviewCounts"));
    }
    [Fact]
    public void MostRecentExplicitResultAndChangeAreKeptApartFromUnverifiedMetadata()
    {
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.SetProjectContext("Threads","콘텐츠 제작");
        var at=DateTimeOffset.UtcNow;
        vm.ApplySnapshot([new(){Id="explicit",ProjectId="Threads",Title="표지 확인",ResultSummary="표지 검수 기록",UpdatedAt=at.AddMinutes(-1)},new(){Id="newer",ProjectId="Threads",Title="새 원문 대기",UpdatedAt=at,Status="queued"}]);
        Assert.Equal("표지 검수 기록",Text(vm,"LatestProjectResult"));
        Assert.Contains("새 원문 대기",Text(vm,"ProjectLatestChange"));
        Assert.DoesNotContain("완료",Text(vm,"ProjectLatestChange"));
    }
    [Fact]
    public async Task FailedReadCannotBeReportedAsNoResultsAndRetainsEarlierEvidence()
    {
        var vm=new WorkDashboardViewModel(_=>throw new IOException("원본 읽기 거절"));
        vm.SetProjectContext("Threads","콘텐츠 제작");
        vm.ApplySnapshot([new(){Id="old",ProjectId="Threads",ResultSummary="이전 결과 기록"}]);
        await vm.RefreshAsync();
        Assert.Contains("읽기 실패",Text(vm,"ProjectReadState"));
        Assert.Contains("이전 조회",Text(vm,"ProjectOverviewCounts"));
        Assert.Equal("이전 결과 기록",Text(vm,"LatestProjectResult"));
        Assert.Single(vm.Activities);
    }
    [Fact]
    public async Task UnreadEmptyAndFailedSourcesRemainDistinct()
    {
        var fails=false;
        var vm=new WorkDashboardViewModel(_=>fails?throw new IOException("읽기 오류"):Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.SetProjectContext("Threads","콘텐츠 제작");
        Assert.Contains("조회 전",Text(vm,"ProjectReadState"));
        await vm.RefreshAsync();
        Assert.Equal("결과 기록 없음",Text(vm,"LatestProjectResult"));
        fails=true;await vm.RefreshAsync();
        Assert.Equal("읽기 실패 · 결과 유무 미확인",Text(vm,"LatestProjectResult"));
        Assert.Contains("읽기 실패",Text(vm,"EmptyListMessage"));
    }
    [Fact]
    public void PartialSourceErrorsRemainVisibleWithoutMixingOtherProjectsIntoCounts()
    {
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.SetProjectContext("Threads","콘텐츠 제작");
        vm.ApplySnapshot([new(){Id="error:file",Source="조회 오류",Error="접근 오류"},new(){Id="other",ProjectId="audiobook",Status="completed"}]);
        Assert.Contains("일부 자료 읽기 실패",Text(vm,"ProjectReadState"));
        Assert.Equal("읽기 실패 · 결과 유무 미확인",Text(vm,"LatestProjectResult"));
        Assert.Contains("기록 0",Text(vm,"ProjectOverviewCounts"));
    }
    [Fact]
    public void LargeListSearchIncludesActualResultAndNextStepAndKeepsSelection()
    {
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        var rows=Enumerable.Range(0,600).Select(i=>new WorkActivity{Id="row"+i,ProjectId="Threads",Title="실제 테스트 기록 "+i,ResultSummary=i==512?"회사청취 출력":"결과 메타데이터 미확인",NextCheckpoint=i==513?"표지 보완":"다음 단계 미확인"}).ToArray();
        vm.ApplySnapshot(rows);vm.Selected=rows[20];
        vm.Search="회사청취";Assert.Equal("row512",Assert.Single(vm.FilteredActivities).Id);
        vm.Search="표지 보완";Assert.Equal("row513",Assert.Single(vm.FilteredActivities).Id);
        Assert.Equal("row20",vm.Selected?.Id);
    }
    [Fact]
    public void LatestChangeCannotClaimAbsenceBeforeFirstRead()
    {
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.SetProjectContext("Threads","콘텐츠 제작");
        Assert.Contains("조회 전",Text(vm,"ProjectLatestChange"));
    }
    [Fact]
    public async Task ManagementCopyKeepsSourceReadFailureAndEarlierEvidence()
    {
        var source=new WorkDashboardViewModel(_=>throw new IOException("원본 읽기 거절"));
        source.ApplySnapshot([new(){Id="old",ProjectId="Control-Tower",IsManagementRecord=true,ResultSummary="이전 근거"}]);
        await source.RefreshAsync();
        var target=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([])){ManagementOnly=true};
        target.SetProjectContext("Control-Tower","통합 관리");
        var method=typeof(WorkDashboardViewModel).GetMethod("ApplySnapshotFrom");Assert.NotNull(method);
        method!.Invoke(target,[source]);
        Assert.Contains("읽기 실패",Text(target,"ProjectReadState"));
        Assert.Contains("이전 조회",Text(target,"ProjectOverviewCounts"));
        Assert.Equal("이전 근거",Text(target,"LatestProjectResult"));
    }
    [Fact]
    public void NeedsActionFilterIncludesWaitingAndProblemsButNotCompletedOrConfirmedRunning()
    {
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
        vm.ApplySnapshot([new(){Id="waiting",Status="queued"},new(){Id="failed",Status="failed"},new(){Id="unknown",Status="unknown"},new(){Id="complete",Status="completed"},new(){Id="running",Status="running",LivenessKnown=true}]);
        vm.StatusFilterKey="needs-action";
        Assert.Equal(new[]{"waiting","failed","unknown"},vm.FilteredActivities.Select(r=>r.Id).ToArray());
    }
}
