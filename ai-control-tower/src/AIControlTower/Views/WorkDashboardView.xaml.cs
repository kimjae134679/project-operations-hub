using System.Windows;
using System.Windows.Controls;
using AIControlTower.ViewModels;
namespace AIControlTower.Views;
public partial class WorkDashboardView : UserControl
{
    private WorkDashboardViewModel? _bound;
    private double _offset;
    public WorkDashboardView()
    {
        InitializeComponent(); PreviewMouseWheel += Services.MouseWheelRouting.HandlePreviewMouseWheel;
        DataContextChanged+=(_,_)=>BindSnapshot();
        Unloaded+=(_,_)=>UnbindSnapshot(); Loaded+=(_,_)=>BindSnapshot();
    }
    private void UnbindSnapshot() { if(_bound is null)return;_bound.SnapshotApplying-=SaveScroll;_bound.SnapshotApplied-=RestoreScroll;_bound=null; }
    private void BindSnapshot()
    {
        UnbindSnapshot();_bound=DataContext as WorkDashboardViewModel;
        if(_bound is not null){_bound.SnapshotApplying+=SaveScroll;_bound.SnapshotApplied+=RestoreScroll;}
    }
    private static ScrollViewer? FindScroll(DependencyObject root)
    {
        if(root is ScrollViewer scroll)return scroll;
        for(var i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);i++)
        { var found=FindScroll(System.Windows.Media.VisualTreeHelper.GetChild(root,i));if(found is not null)return found; }
        return null;
    }
    private void SaveScroll(object? sender,EventArgs e)=>_offset=FindScroll(WorkList)?.VerticalOffset??0;
    private void RestoreScroll(object? sender,EventArgs e)=>Dispatcher.BeginInvoke(()=>FindScroll(WorkList)?.ScrollToVerticalOffset(_offset),System.Windows.Threading.DispatcherPriority.Loaded);
    private async void Refresh_Click(object sender, RoutedEventArgs e) { if (DataContext is WorkDashboardViewModel vm) await vm.RefreshAsync(); }
    private async void Register_Click(object sender, RoutedEventArgs e) { if (DataContext is WorkDashboardViewModel vm) await vm.RegisterSourceAsync(); }
    private async void Details_Click(object sender, RoutedEventArgs e) { if (DataContext is WorkDashboardViewModel vm) await vm.LoadSelectedDetailsAsync(); }
    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkDashboardViewModel { CanStopOwned: true } vm) return;
        if (MessageBox.Show(Window.GetWindow(this), "현재 관제탑이 소유한 이 작업만 중지할까요? 다른 프로젝트·외부 콘솔·원격 연결은 중지하지 않습니다.", "선택 작업 중지", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK) vm.StopSelectedOwned();
    }
}
