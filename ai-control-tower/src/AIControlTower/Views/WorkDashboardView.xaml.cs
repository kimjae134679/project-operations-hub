using System.Windows;
using System.Windows.Controls;
using AIControlTower.ViewModels;
namespace AIControlTower.Views;
public partial class WorkDashboardView : UserControl
{
    private WorkDashboardViewModel? _bound;
    private double _offset;
    private string? _detailId;
    private readonly Models.WorkReadingPositionStore _detailOffsets = new();
    public WorkDashboardView()
    {
        InitializeComponent(); PreviewMouseWheel += Services.MouseWheelRouting.HandlePreviewMouseWheel;
        DataContextChanged+=(_,_)=>BindSnapshot();
        Unloaded+=(_,_)=>UnbindSnapshot(); Loaded+=(_,_)=>BindSnapshot();
    }
    private void UnbindSnapshot() { if(_bound is null)return;_bound.SnapshotApplying-=SaveScroll;_bound.SnapshotApplied-=RestoreScroll;_bound.PropertyChanged-=SelectionChanged;_bound=null; }
    private void BindSnapshot()
    {
        UnbindSnapshot();_bound=DataContext as WorkDashboardViewModel;
        if(_bound is not null){_bound.SnapshotApplying+=SaveScroll;_bound.SnapshotApplied+=RestoreScroll;_bound.PropertyChanged+=SelectionChanged;_detailId=_bound.Selected?.Id;}
    }
    private static ScrollViewer? FindScroll(DependencyObject root)
    {
        if(root is ScrollViewer scroll)return scroll;
        for(var i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);i++)
        { var found=FindScroll(System.Windows.Media.VisualTreeHelper.GetChild(root,i));if(found is not null)return found; }
        return null;
    }
    private void SaveDetails()
    {
        if(_detailId is null)return;
        _detailOffsets.Save(_detailId,DetailStageScroll.VerticalOffset,DetailLogScroll.VerticalOffset);
    }
    private void RestoreDetails()
    {
        var id=_detailId; var offsets=id is not null?_detailOffsets.Read(id):(Stage:0d,Log:0d);
        Dispatcher.BeginInvoke(()=>{if(_detailId!=id)return;DetailStageScroll.ScrollToVerticalOffset(offsets.Stage);DetailLogScroll.ScrollToVerticalOffset(offsets.Log);},System.Windows.Threading.DispatcherPriority.Loaded);
    }
    private void SelectionChanged(object? sender,System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName!=nameof(WorkDashboardViewModel.Selected))return;
        SaveDetails();_detailId=_bound?.Selected?.Id;RestoreDetails();
    }
    private void SaveScroll(object? sender,EventArgs e){_offset=FindScroll(WorkList)?.VerticalOffset??0;SaveDetails();}
    private void RestoreScroll(object? sender,EventArgs e)
    {
        Dispatcher.BeginInvoke(()=>FindScroll(WorkList)?.ScrollToVerticalOffset(_offset),System.Windows.Threading.DispatcherPriority.Loaded);
        _detailId=_bound?.Selected?.Id;RestoreDetails();
    }
    private void Stop_Click(object sender,RoutedEventArgs e)
    {
        if(DataContext is not WorkDashboardViewModel {CanStopOwned:true} vm)return;
        if(MessageBox.Show(Window.GetWindow(this),"현재 관제탑이 소유한 이 작업만 중지할까요? 다른 프로젝트·외부 콘솔·원격 연결은 중지하지 않습니다.","선택 작업 중지",MessageBoxButton.OKCancel,MessageBoxImage.Warning)==MessageBoxResult.OK)vm.StopSelectedOwned();
    }
    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkDashboardViewModel model) return;
        model.Search = ""; model.ProjectFilterId = ""; model.StatusFilterKey = ""; model.WorkerFilterKey = "";
    }
    private void WorkSelection_Changed(object sender, SelectionChangedEventArgs e)
    {
        // Filtering can clear the list's selection; it must not discard the record being read.
        if (DataContext is WorkDashboardViewModel model && e.AddedItems.Count > 0 && e.AddedItems[0] is Models.WorkActivity selected) model.Selected = selected;
    }
}
