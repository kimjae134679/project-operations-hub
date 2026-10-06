using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AIControlTower.ViewModels;
using Microsoft.Win32;
namespace AIControlTower.Views;
public partial class CommunicationWorkspaceView : UserControl
{
    private MainViewModel? _vm;
    private readonly Dictionary<string,double> _readPositions = new(StringComparer.Ordinal);
    private string _readingKey = "";
    private int _restoreVersion;
    public CommunicationWorkspaceView()
    {
        InitializeComponent();
        Loaded += (_,_) => Attach();
        Unloaded += (_,_) => Detach();
        DataContextChanged += (_,_) => { if(IsLoaded) Attach(); };
        ArticleReader.AddHandler(ScrollViewer.ScrollChangedEvent,new ScrollChangedEventHandler(ReaderScrolled));
    }
    private void Attach()
    {
        Detach();
        _vm = DataContext as MainViewModel;
        if(_vm is null) return;
        _vm.PropertyChanged += SelectionChanged;
        RestorePosition();
    }
    private void Detach()
    {
        if(_vm is not null) _vm.PropertyChanged -= SelectionChanged;
        _vm = null;
    }
    private void SelectionChanged(object? sender,PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(MainViewModel.SelectedInbox) or nameof(MainViewModel.SelectedEntry))
            RestorePosition();
    }
    private void RestorePosition()
    {
        var next = _vm?.EntryReadPositionKey ?? "";
        if(next == _readingKey) return;
        if(_readingKey.Length > 0) _readPositions[_readingKey] = ArticleReader.VerticalOffset;
        _readingKey = next;
        var version = ++_restoreVersion;
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded,new Action(() =>
        {
            if(version == _restoreVersion) ArticleReader.ScrollToVerticalOffset(_readPositions.GetValueOrDefault(next,0));
        }));
    }
    private void ReaderScrolled(object sender,ScrollChangedEventArgs e)
    {
        if(_readingKey.Length > 0 && e.VerticalChange != 0 && e.ExtentHeightChange == 0)
            _readPositions[_readingKey] = ArticleReader.VerticalOffset;
    }
    private void OpenCollectedFile_Click(object sender,RoutedEventArgs e) => _vm?.OpenCollectedFile();
    private void OpenCommunicationFolder_Click(object sender,RoutedEventArgs e) => _vm?.OpenCommunicationFolder();
    private async void LinkCommunicationProject_Click(object sender,RoutedEventArgs e)
    {
        if(_vm?.SelectedCommunicationProject is not { } project) return;
        var dialog = new OpenFolderDialog { Title=project.Name+"의 주 작업 폴더 선택",Multiselect=false };
        if(Directory.Exists(project.Root)) dialog.InitialDirectory=project.Root;
        if(dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        _vm.LinkCommunicationProject(project.Id,dialog.FolderName);
        await _vm.SyncCommunicationAsync(true);
    }
}
