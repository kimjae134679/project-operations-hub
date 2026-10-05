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
    private ListBox? _activeProgramList;
    private Task? _initializeTask;

    public MainWindow(ControlTowerSettings? settings = null)
    {
        _viewModel = new(settings);
        InitializeComponent();
        DataContext = _viewModel;
        SourceInitialized += (_, _) => { var dark = 1; DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 20, ref dark, sizeof(int)); };
        Loaded += async (_, _) => await InitializeAsync();
        SizeChanged += (_, _) => ApplyResponsiveLayout();
        _viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(MainViewModel.SelectedProgram)) { ApplyResponsiveLayout(); Dispatcher.BeginInvoke(SynchronizeProgramSelections, System.Windows.Threading.DispatcherPriority.DataBind); } };
        Closed += (_, _) => _viewModel.Dispose();
    }
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr handle, int attribute, ref int value, int size);

    public Task InitializeAsync() => _initializeTask ??= InitializeCoreAsync();

    private async Task InitializeCoreAsync()
    {
        ApplyResponsiveLayout();
        if (!SystemParameters.ClientAreaAnimation) _viewModel.ReduceMotion = true;
        FadeIn(WorkspaceContent);
        await _viewModel.DiscoverAsync();
        await _viewModel.RefreshAsync();
        await _viewModel.RefreshServerAsync(true);
        if (_viewModel.AutoCommunication) await _viewModel.SyncCommunicationAsync(true);
    }

    private void ApplyResponsiveLayout()
    {
        if (ContentShell is null) return;
        var compact = ActualHeight < 820;
        var narrow = ActualWidth < 1220;
        ContentShell.Margin = compact ? new Thickness(20, 14, 20, 10) : new Thickness(24);
        PageHeader.Margin = new Thickness(0, 0, 0, compact ? 8 : 14);
        WorkspaceContent.Margin = new Thickness(0, compact ? 10 : 16, 0, 0);
        CatalogColumn.Width = new GridLength(narrow ? 286 : 328);
        InspectorColumn.Width = new GridLength(narrow ? 254 : 280);
        ProjectHeader.Padding = new Thickness(compact ? 16 : 20);
        ProjectHeader.Margin = new Thickness(0, 0, 0, compact ? 12 : 16);
        ProjectTitle.FontSize = narrow ? 23 : 26;
        ProjectDescription.MaxHeight = compact ? 22 : 44;
        ProjectPathLine.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        JobLogList.Height = compact ? 65 : 100;
        LogPanel.Margin = new Thickness(0, compact ? 8 : 14, 0, 0);
        var hideMetadata = compact && _viewModel.HasSelectedCommands;
        InspectorMetadata.Visibility = hideMetadata ? Visibility.Collapsed : Visibility.Visible;
        InspectorMetadataRow.Height = hideMetadata ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
    }

    private void FadeIn(UIElement element)
    {
        if (_viewModel.ReduceMotion || !SystemParameters.ClientAreaAnimation)
        {
            element.BeginAnimation(OpacityProperty, null);
            element.Opacity = 1;
            return;
        }
        element.BeginAnimation(OpacityProperty, new DoubleAnimation(0.84, 1, TimeSpan.FromMilliseconds(160))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.Stop
        });
    }

    private void Projects_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != sender || e.AddedItems.Count == 0 || WorkspaceContent is null) return;
        _activeProgramList = null;
        FadeIn(WorkspaceContent);
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
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await _viewModel.RefreshAsync();
    private async void CommunicationSync_Click(object sender, RoutedEventArgs e) => await _viewModel.SyncCommunicationAsync(true);
    private async void RefreshServer_Click(object sender, RoutedEventArgs e) => await _viewModel.RefreshServerAsync(true);
    private void OpenServerFolder_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerFolder();
    private void OpenServerMailbox_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerMailbox();
    private void OpenServerSource_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerRepository(false);
    private void OpenServerRelease_Click(object sender, RoutedEventArgs e) => _viewModel.OpenServerRepository(true);
    private async void LinkServerFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "실제로 실행하는 멀티의 신 서버 폴더 선택" };
        if (dialog.ShowDialog(this) != true) return;
        _viewModel.ServerRootPath = dialog.FolderName;
        await _viewModel.RefreshServerAsync(true);
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
        if (MessageBox.Show(this, "관제탑의 자동 시작 등록과 설치 폴더를 제거합니다.\n프로젝트 폴더는 그대로 유지됩니다.\n\n" + path, "관제탑 제거", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        var result = await _installationService.UninstallAsync(path, CancellationToken.None);
        MessageBox.Show(this, result.Detail, "관제탑 제거", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        var result = await _installationService.RestoreDesktopCommanderStartupAsync(ResolveInstallPath(), CancellationToken.None);
        MessageBox.Show(this, result.Detail, "공유 연결 복구", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
}
