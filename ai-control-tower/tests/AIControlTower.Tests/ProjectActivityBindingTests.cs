using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using AIControlTower.Models;
using AIControlTower.ViewModels;
using AIControlTower.Views;
namespace AIControlTower.Tests;
public sealed class ProjectActivityBindingTests
{
    [Fact] public void DetachedProjectPanesKeepBothSelectedRecordIdentitiesDuringPolling()
    {
        Exception? failure=null;
        var thread=new Thread(()=>{try {
            var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));vm.SetProjectContext("p","프로젝트");
            var rows=new[]{new WorkActivity{Id="r-one",ProjectId="p",Source="명령·답변 task_exchange 기록"},new WorkActivity{Id="r-two",ProjectId="p",Source="명령·답변 task_exchange 기록"},new WorkActivity{Id="e-one",ProjectId="p",Source="관제탑 소유 실행 기록"},new WorkActivity{Id="e-two",ProjectId="p",Source="관제탑 소유 실행 기록"}};
            vm.ApplySnapshot(rows);var view=new ProjectActivityView{DataContext=vm};view.Measure(new Size(900,600));view.Arrange(new Rect(0,0,900,600));view.UpdateLayout();
            var records=(ListBox)view.FindName("ProjectRecordsList");var executions=(ListBox)view.FindName("ProjectExecutionsList");
            records.SelectedItem=rows[1];executions.SelectedItem=rows[3];
            vm.ApplySnapshot(rows.Select(r=>r with {Stage="다음 기록"}).ToArray());view.UpdateLayout();
            Assert.Equal("r-two",vm.SelectedRecord?.Id);Assert.Equal("e-two",vm.SelectedExecution?.Id);
            Assert.Equal("r-two",(records.SelectedItem as WorkActivity)?.Id);Assert.Equal("e-two",(executions.SelectedItem as WorkActivity)?.Id);
        }catch(Exception ex){failure=ex;}});
        thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();if(failure is not null)ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
