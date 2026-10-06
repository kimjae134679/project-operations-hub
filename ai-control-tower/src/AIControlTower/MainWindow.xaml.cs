using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace AIControlTower;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly InstallationService _installationService = new();
    private JevControlWindow? _jevWindow;
    private NoticeReceiptsWindow? _receiptWindow;
    private readonly PcJobsPanel _pcJobsPanel;
    private ListBox? _activeProgramList;
    private Task? _initializeTask;
    private double _catalogOffset;
    private readonly Dictionary<string, bool> _expandedFunctions = new();

    public MainWindow(ControlTowerSettings? settings = null)
    {
        _viewModel = new(settings);
        ThemeService.Apply(_viewModel.DarkMode);
        InitializeComponent();
        DataContext = _viewModel;
        _pcJobsPanel = new PcJobsPanel(_viewModel.PcConnection,_viewModel);
        PcJobsHost.Content = _pcJobsPanel;
        PreviewMouseWheel += MouseWheelRouting.HandlePreviewMouseWheel;
        SourceInitialized += (_, _) => ApplyTitlebarTheme();
        Loaded += async (_, _) => await InitializeAsync();
        SizeChanged += (_, _) => ApplyResponsiveLayout();
        _viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(MainViewModel.SelectedProgram)) { ApplyResponsiveLayout(); Dispatcher.BeginInvoke(SynchronizeProgramSelections, System.Windows.Threading.DispatcherPriority.DataBind); } };
        _viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(MainViewModel.DarkMode)) ApplyTitlebarTheme(); };
        _viewModel.CatalogRefreshing += (_, _) => CaptureCatalogView();
        _viewModel.CatalogRefreshed += (_, _) => Dispatcher.BeginInvoke(RestoreCatalogView, System.Windows.Threading.DispatcherPriority.Loaded);
        Closed += (_, _) => _viewModel.Dispose();
    }
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr handle, int attribute, ref int value, int size);
    private void ApplyTitlebarTheme()
    {
        var dark = _viewModel.DarkMode ? 1 : 0;
        DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 20, ref dark, sizeof(int));
    }
    private void Theme_Click(object sender, RoutedEventArgs e) => _viewModel.DarkMode = !_viewModel.DarkMode;

    private static IEnumerable<DependencyObject> Descendants(DependencyObject element)
    {
        yield return element;
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(element); i++)
            foreach (var child in Descendants(System.Windows.Media.VisualTreeHelper.GetChild(element,i))) yield return child;
    }
    private void CaptureCatalogView()
    {
        _catalogOffset = Descendants(ProjectList).OfType<ScrollViewer>().FirstOrDefault()?.VerticalOffset ?? 0;
        _expandedFunctions.Clear();
        foreach (var group in Descendants(WorkspaceContent).OfType<Expander>().Where(e => e.DataContext is FunctionItem))
            _expandedFunctions[((FunctionItem)group.DataContext).Id] = group.IsExpanded;
    }
    private void RestoreCatalogView()
    {
        foreach (var group in Descendants(WorkspaceContent).OfType<Expander>().Where(e => e.DataContext is FunctionItem))
            if (_expandedFunctions.TryGetValue(((FunctionItem)group.DataContext).Id, out var open)) group.SetCurrentValue(Expander.IsExpandedProperty,open);
        Descendants(ProjectList).OfType<ScrollViewer>().FirstOrDefault()?.ScrollToVerticalOffset(_catalogOffset);
    }

    public Task InitializeAsync() => _initializeTask ??= InitializeCoreAsync();

    private async Task InitializeCoreAsync()
    {
        ApplyResponsiveLayout();
        if (!SystemParameters.ClientAreaAnimation) _viewModel.ReduceMotion = true;
        FadeIn(WorkspaceContent);
        await _viewModel.DiscoverAsync();
        await _viewModel.RefreshAsync();
        await _viewModel.RefreshRosterAsync(); await _viewModel.RefreshServerAsync(true);
        if (_viewModel.AutoCommunication) await _viewModel.SyncCommunicationAsync(true);
    }

    private void ApplyResponsiveLayout()
    {
        if (ContentShell is null) return;
        var compact = ActualHeight < 820;
        var narrow = ActualWidth < 1220;
        ContentShell.Margin = new Thickness(narrow ? 12 : 16);
        PageHeader.Margin = new Thickness(0, 0, 0, compact ? 8 : 14);
        WorkspaceContent.Margin = new Thickness(0, compact ? 16 : 20, 0, 0);
        CatalogColumn.Width = new GridLength(narrow ? 230 : 264);
        InspectorColumn.Width = new GridLength(narrow ? 246 : 284);
        ProjectHeader.Padding = compact ? new Thickness(18,10,18,6) : new Thickness(narrow ? 18 : 24);
        ProjectCategory.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        ProjectHeaderBody.Margin = new Thickness(0,0,7,compact ? 2 : 18);
        ProjectHeader.Margin = new Thickness(0, 0, 0, compact ? 12 : 16);
        ProjectTitle.FontSize = narrow ? 27 : 32;
        ProjectDescription.MaxHeight = compact ? 44 : 60;
        ProjectPathLine.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        JobLogList.Height = compact ? 65 : 100;

        LogPanel.Margin = new Thickness(0, compact ? 8 : 14, 0, 0);
        var hideMetadata = compact && _viewModel.HasSelectedCommands;
        ProgramInspector.Padding = new Thickness(compact ? 14 : 20);
        InspectorType.Visibility = hideMetadata ? Visibility.Collapsed : Visibility.Visible;
        InspectorStatus.Visibility = hideMetadata ? Visibility.Collapsed : Visibility.Visible;
        InspectorActionLabel.Visibility = hideMetadata ? Visibility.Collapsed : Visibility.Visible;
        InspectorActions.Margin = new Thickness(0,compact ? 6 : 16,0,0);
        InspectorMetadata.Visibility = hideMetadata ? Visibility.Collapsed : Visibility.Visible;
        InspectorMetadataRow.Height = hideMetadata ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
    }

    private void FadeIn(UIElement element)
    {
        if (_viewModel.ReduceMotion || !SystemParameters.ClientAreaAnimation)
        {
            element.BeginAnimation(OpacityProperty, null);
            element.Opacity = 1;
            element.RenderTransform = System.Windows.Media.Transform.Identity;
            return;
        }
        element.BeginAnimation(OpacityProperty, new DoubleAnimation(0.84, 1, TimeSpan.FromMilliseconds(160))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.Stop
        });
        var offset = new System.Windows.Media.TranslateTransform();
        element.RenderTransform = offset;
        offset.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, new DoubleAnimation(6, 0, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.Stop
        });
    }

    private void Projects_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != sender || e.AddedItems.Count == 0 || WorkspaceContent is null) return;
        _activeProgramList = null;
        FadeIn(ProjectHeader);
        FadeIn(ProgramInspector);
    }

    private void Programs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox list || e.AddedItems.Count == 0 || e.AddedItems[0] is not ProgramItem program) return;
        _activeProgramList = list;
        _viewModel.SelectedProgram = program;
        FadeIn(ProgramInspector);
        e.Handled = true;
    }
    private void SynchronizeProgramSelections()
    {
        void Visit(DependencyObject element)
        {
            if (element is ListBox list && list.Items.OfType<ProgramItem>().Any())
            {
                var expected = list.Items.Contains(_viewModel.SelectedProgram) ? _viewModel.SelectedProgram : null;
                if (list.SelectedItem != expected) list.SetCurrentValue(ListBox.SelectedItemProperty, expected);
            }
            for (var i=0; i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(element); i++) Visit(System.Windows.Media.VisualTreeHelper.GetChild(element,i));
        }
        Visit(this);
    }

    private void WorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != sender || WorkspaceContent is null) return;
        FadeIn(WorkspaceContent);
        if (CommunicationTab.IsSelected) _viewModel.MarkCommunicationViewed();
    }

    private bool _articleExpanded;
    public void ShowPcJobs()
    {
        WorkspaceTabs.SelectedItem = PcConnectionTab;
        if (!VerificationDisplay.Quiet) _pcJobsPanel.FocusJobs();
    }
    private void ToggleArticleWidth_Click(object sender, RoutedEventArgs e)
    {
        _articleExpanded = !_articleExpanded;
        ArticleListPanel.Visibility = _articleExpanded ? Visibility.Collapsed : Visibility.Visible;
        ArticleListColumn.Width = new GridLength(_articleExpanded ? 0 : 320);
        ArticleGapColumn.Width = new GridLength(_articleExpanded ? 0 : 18);
        ArticleExpandButton.Content = _articleExpanded ? "목록 함께 보기" : "본문 넓게";
    }
    private void OpenNoticeReceipts_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NoticeReceiptFilter = (sender as FrameworkElement)?.Tag as string ?? "전체";
        if (_receiptWindow is { IsLoaded: true }) { _receiptWindow.Activate(); return; }
        _receiptWindow = new NoticeReceiptsWindow(_viewModel) { Owner = this };
        _receiptWindow.Closed += (_,_) => _receiptWindow = null;
        _receiptWindow.Show();
    }
    private void OpenProjectGuide_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedProject is { } project)
            new ProjectGuideWindow(project.DisplayName, project.Path) { Owner = this }.Show();
    }
    private void OpenTool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: ToolStatusViewModel tool }) return;
        if (tool.RawName == "Jev Router") { Jev_Click(sender, e); return; }
        _viewModel.OpenTool(tool.Id);
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await _viewModel.RefreshAsync();
    private async void CommunicationSync_Click(object sender, RoutedEventArgs e) => await _viewModel.SyncCommunicationAsync(true);
    private async void RefreshServer_Click(object sender, RoutedEventArgs e) { await _viewModel.RefreshRosterAsync(true); await _viewModel.RefreshServerAsync(true); }
    private void OpenServerGuide_Click(object sender,RoutedEventArgs e) => new ProjectGuideWindow("멀티의 신 서버",_viewModel.ServerRootPath) { Owner=this }.Show();
    private void OpenServerFolder_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerFolder();
    private void OpenServerMailbox_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerMailbox();
    private void OpenServerSource_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerRepository(false);
    private void OpenServerRelease_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerRepository(true);
    private async void LinkServerFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "실제로 실행하는 멀티의 신 서버 폴더 선택" };
        if (dialog.ShowDialog(this) != true) return;
        _viewModel.ServerRootPath = dialog.FolderName;
        await _viewModel.RefreshRosterAsync(); await _viewModel.RefreshServerAsync(true);
        await _viewModel.SyncCommunicationAsync(true);
    }
    private void OpenCommunicationFolder_Click(object sender, RoutedEventArgs e) => _viewModel.OpenCommunicationFolder();
    private void OpenCollectedFile_Click(object sender, RoutedEventArgs e) => _viewModel.OpenCollectedFile();
    private async void LinkCommunicationProject_Click(object sender, RoutedEventArgs e)
    {
        var project = _viewModel.SelectedCommunicationProject;
        if (project is null) return;
        var dialog = new OpenFolderDialog { Title = project.Name + "의 주 작업 폴더 선택", Multiselect = false };
        if (Directory.Exists(project.Root)) dialog.InitialDirectory = project.Root;
        if (dialog.ShowDialog(this) != true) return;
        _viewModel.LinkCommunicationProject(project.Id, dialog.FolderName);
        await _viewModel.SyncCommunicationAsync(true);
    }
    private async void Discover_Click(object sender, RoutedEventArgs e) => await _viewModel.DiscoverAsync();
    private async void ConsolidateRemote_Click(object sender, RoutedEventArgs e) => await _viewModel.ConsolidateRemoteStartupAsync();
    private async void EnsureRemote_Click(object sender, RoutedEventArgs e) => await _viewModel.EnsureRemoteRunningAsync();
    private async void StopRemote_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, "원격 연결을 끄면 연결된 AI의 PC 작업도 끊깁니다. 이번 로그인 동안은 자동 복구를 멈추고, 다음 로그인에는 다시 켭니다.", "원격 연결 중지", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            await _viewModel.StopRemoteRunningAsync();
    }
    private void OpenProject_Click(object sender, RoutedEventArgs e) => _viewModel.OpenProject();
    private void OpenProgram_Click(object sender, RoutedEventArgs e) => _viewModel.OpenProgram();
    private async void LaunchProgram_Click(object sender, RoutedEventArgs e) => await _viewModel.ActivateSelectedProgramAsync();
    private void StopProgram_Click(object sender, RoutedEventArgs e) => _viewModel.StopProgram();

    private async void ChooseRoot_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "프로젝트를 모아둔 폴더를 선택하세요", Multiselect = false };
        if (Directory.Exists(_viewModel.RootPath)) dialog.InitialDirectory = _viewModel.RootPath;
        if (dialog.ShowDialog(this) != true) return;
        _viewModel.RootPath = dialog.FolderName;
        await _viewModel.DiscoverAsync();
    }

    private async void ProjectFolder_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;
        var folder = paths.FirstOrDefault(Directory.Exists);
        if (folder is null) return;
        _viewModel.RootPath = Directory.GetParent(folder)?.FullName ?? folder;
        await _viewModel.DiscoverAsync();
        _viewModel.SelectProjectByPath(folder);
        e.Handled = true;
    }

    private void Jev_Click(object sender, RoutedEventArgs e)
    {
        if (_jevWindow is { IsLoaded: true }) { _jevWindow.Activate(); return; }
        _jevWindow = new JevControlWindow(_viewModel) { Owner = this };
        _jevWindow.Closed += (_, _) => _jevWindow = null;
        _jevWindow.Show();
    }

    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        var result = await _installationService.InstallAsync(Environment.ProcessPath ?? string.Empty, CancellationToken.None);
        MessageBox.Show(this, result.Detail + Environment.NewLine + result.InstallPath, "관제탑 설치", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private static string ResolveInstallPath()
    {
        var registered = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "AIControlTower", null) as string;
        if (!string.IsNullOrWhiteSpace(registered))
        {
            var executable = registered.Trim().Trim('"');
            if (Path.GetFileName(executable).Equals("AIControlTower.exe", StringComparison.OrdinalIgnoreCase) && File.Exists(executable))
                return Path.GetDirectoryName(executable)!;
        }
        var preferred = @"D:\A_KJ\AI\Applications\AIControlTower";
        if (File.Exists(Path.Combine(preferred, "AIControlTower.exe"))) return preferred;
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
    }

    private async void Uninstall_Click(object sender, RoutedEventArgs e)
    {
        var path = ResolveInstallPath();
        if (MessageBox.Show(this, "관리 화면의 자동 시작을 제거합니다.\n원격 연결·자동 복구·프로젝트 자료와 실행 파일은 유지합니다.\n\n" + path, "관제탑 제거", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        var result = await _installationService.UninstallAsync(path, CancellationToken.None);
        MessageBox.Show(this, result.Detail, "관제탑 제거", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        var result = await _installationService.RestoreDesktopCommanderStartupAsync(ResolveInstallPath(), CancellationToken.None);
        MessageBox.Show(this, result.Detail, "공유 연결 복구", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
}
