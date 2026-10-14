using System.Windows.Controls;
using System.Windows;
using AIControlTower.ViewModels;
namespace AIControlTower.Views;
public partial class ProjectActivityView : UserControl
{
    private WorkDashboardViewModel? _bound;
    private string? _recordId,_executionId;
    private double _recordsOffset,_executionsOffset,_recordDetailOffset,_executionDetailOffset;
    public ProjectActivityView()
    {
        InitializeComponent();
        PreviewMouseWheel+=Services.MouseWheelRouting.HandlePreviewMouseWheel;
        DataContextChanged+=(_,_)=>BindSnapshot();Loaded+=(_,_)=>BindSnapshot();Unloaded+=(_,_)=>UnbindSnapshot();
    }
    private void UnbindSnapshot()
    {
        if(_bound is null)return;
        _bound.SnapshotApplying-=SaveSnapshot;_bound.SnapshotApplied-=RestoreSnapshot;_bound=null;
    }
    private void BindSnapshot()
    {
        var next=DataContext as WorkDashboardViewModel;if(ReferenceEquals(next,_bound))return;
        UnbindSnapshot();_bound=next;
        if(_bound is not null){_bound.SnapshotApplying+=SaveSnapshot;_bound.SnapshotApplied+=RestoreSnapshot;}
    }
    private static ScrollViewer? FindScroll(DependencyObject root)
    {
        if(root is ScrollViewer scroll)return scroll;
        for(var i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);i++)
        {var found=FindScroll(System.Windows.Media.VisualTreeHelper.GetChild(root,i));if(found is not null)return found;}
        return null;
    }
    private void SaveSnapshot(object? sender,EventArgs e)
    {
        _recordId=_bound?.SelectedRecord?.Id;_executionId=_bound?.SelectedExecution?.Id;
        _recordsOffset=FindScroll(ProjectRecordsList)?.VerticalOffset??0;_executionsOffset=FindScroll(ProjectExecutionsList)?.VerticalOffset??0;
        _recordDetailOffset=ProjectRecordDetail.VerticalOffset;_executionDetailOffset=ProjectExecutionDetail.VerticalOffset;
    }
    private void RestoreSnapshot(object? sender,EventArgs e)
    {
        if(_bound is null)return;
        // ItemsSource replacement can clear two-way SelectedItem before VM property notification.
        _bound.SelectedRecord=_bound.ProjectRecords.FirstOrDefault(r=>r.Id==_recordId)??_bound.ProjectRecords.FirstOrDefault();
        _bound.SelectedExecution=_bound.ProjectExecutions.FirstOrDefault(r=>r.Id==_executionId)??_bound.ProjectExecutions.FirstOrDefault();
        Dispatcher.BeginInvoke(()=>
        {
            FindScroll(ProjectRecordsList)?.ScrollToVerticalOffset(_recordsOffset);FindScroll(ProjectExecutionsList)?.ScrollToVerticalOffset(_executionsOffset);
            ProjectRecordDetail.ScrollToVerticalOffset(_recordDetailOffset);ProjectExecutionDetail.ScrollToVerticalOffset(_executionDetailOffset);
        },System.Windows.Threading.DispatcherPriority.Loaded);
    }
    private void StopExecution_Click(object sender,RoutedEventArgs e)
    {
        if(DataContext is not WorkDashboardViewModel {CanStopExecution:true} vm)return;
        if(MessageBox.Show(Window.GetWindow(this),"현재 관제탑이 소유한 선택 작업만 중지할까요?","선택 작업 중지",MessageBoxButton.OKCancel,MessageBoxImage.Warning)==MessageBoxResult.OK)vm.StopSelectedExecutionOwned();
    }
}
