using System.Reflection;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class PcJobsRefinementTests
{
    private static string Source()
    {
        var root=new DirectoryInfo(AppContext.BaseDirectory);
        while(root is not null && !File.Exists(Path.Combine(root.FullName,"project.catalog.json")))root=root.Parent;
        Assert.NotNull(root);
        return File.ReadAllText(Path.Combine(root.FullName,"ai-control-tower","src","AIControlTower","PcJobsWindow.cs"));
    }
    private static object Reader(Func<string,Task<string>> read,TimeSpan? budget=null)
    {
        var type=typeof(CommunicationService).Assembly.GetType("AIControlTower.Services.PcJobResultPresentation");
        Assert.NotNull(type);return Activator.CreateInstance(type,read,budget??TimeSpan.FromSeconds(3))!;
    }
    private static Task Refresh(object reader,string? id,bool allow=true,bool active=true)
        =>Assert.IsAssignableFrom<Task>(reader.GetType().GetMethod("RefreshAsync")!.Invoke(reader,[id,allow,active,CancellationToken.None]));
    private static string Text(object reader,string property)
        =>Assert.IsType<string>(reader.GetType().GetProperty(property)!.GetValue(reader));

    [Fact]
    public void ResultListHasNoProjectToolOrStateFilterControls()
    {var source=Source();foreach(var name in new[]{"_projectFilter","_toolFilter","_stateFilter","UpdateProjectFilters","FilterJob"})Assert.DoesNotContain(name,source);}
    [Fact]
    public void CardsKeepActualProjectAndRequestToolWithShortLabels()
    {var source=Source();Assert.Contains("\"Project\",\"Tool\"",source);Assert.Contains("프로젝트 · {0}",source);Assert.Contains("요청 도구 · {0}",source);Assert.Contains("_jobs.ItemsSource=vm.Jobs",source);}
    [Fact]
    public void ResultReaderUsesBoundedPresentationInsteadOfUnscopedLoadingFlag()
    {var source=Source();Assert.DoesNotContain("private bool _loading",source);Assert.Contains("PcJobResultPresentation",source);Assert.Contains("RefreshAsync",source);Assert.DoesNotContain("if(_active)_result.Text=",source);}
    [Fact]
    public void RepeatedInstructionsAreNotVisibleResultPaneContent()
    {var source=Source();Assert.DoesNotContain("form.Children.Add(workspaceHint)",source);Assert.DoesNotContain("작업을 선택하면 결과가 표시됩니다",source);Assert.DoesNotContain("요청했습니다. 작업 목록에서 진행 상황과 결과를 확인하세요.",source);}

    [Theory]
    [InlineData(false,true)]
    [InlineData(true,false)]
    public async Task ReadonlyOrInactiveNeverQueriesOrLeavesLoading(bool allow,bool active)
    {var count=0;var reader=Reader(_=>{count++;return Task.FromResult("not fetched");});await Refresh(reader,"job-a",allow,active);Assert.Equal(0,count);Assert.Equal("unavailable",Text(reader,"State"));Assert.DoesNotContain("확인 중",Text(reader,"Text"));}
    [Fact]
    public async Task LateErrorFromOldSelectionCannotReplaceNewSelection()
    {
        var old=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);var count=0;
        var reader=Reader(id=>{count++;return id=="job-old"?old.Task:Task.FromResult("new result");});
        var first=Refresh(reader,"job-old");await Refresh(reader,"job-new");Assert.Equal("waiting",Text(reader,"State"));Assert.Equal(1,count);
        old.SetException(new IOException("old error must not replace selection"));await first.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("job-new",Text(reader,"SelectedId"));Assert.DoesNotContain("old error",Text(reader,"Text"));
        await Refresh(reader,"job-new");Assert.Equal("ready",Text(reader,"State"));Assert.Equal("new result",Text(reader,"Text"));
    }
    [Fact]
    public async Task TimeoutEndsLoadingAndDoesNotSpawnRepeatedUnderlyingReads()
    {
        var pending=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);var count=0;
        var reader=Reader(_=>{count++;return pending.Task;},TimeSpan.FromMilliseconds(40));
        await Refresh(reader,"job-a").WaitAsync(TimeSpan.FromSeconds(2));Assert.Equal("timeout",Text(reader,"State"));
        await Refresh(reader,"job-a");Assert.Equal(1,count);Assert.Equal("timeout",Text(reader,"State"));
        await Refresh(reader,"job-b");Assert.Equal(1,count);Assert.Equal("waiting",Text(reader,"State"));
        pending.SetResult("late old body");await Task.Delay(30);Assert.Equal("job-b",Text(reader,"SelectedId"));Assert.DoesNotContain("late old body",Text(reader,"Text"));
    }
    [Fact]
    public async Task CurrentQueryErrorIsVisibleWithoutClaimingTaskCompleted()
    {var reader=Reader(_=>Task.FromException<string>(new IOException("actual result read failed")));await Refresh(reader,"job-a");Assert.Equal("error",Text(reader,"State"));Assert.Contains("actual result read failed",Text(reader,"Text"));Assert.DoesNotContain("완료",Text(reader,"Text"));}
}
