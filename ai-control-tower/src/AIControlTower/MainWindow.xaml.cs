using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
using Microsoft.Win32;

namespace AIControlTower;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private readonly InstallationService _installationService = new();
    private JevControlWindow? _jevWindow;
    private ListBox? _activeProgramList;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += async (_, _) =>
        {
            _viewModel.ReduceMotion = !SystemParameters.ClientAreaAnimation;
            FadeIn(WorkspaceContent);
            await _viewModel.DiscoverAsync();
            await _viewModel.RefreshAsync();
        };
        Closed += (_, _) => _viewModel.Dispose();
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
        if (_activeProgramList is not null) _activeProgramList.SelectedItem = null;
        _activeProgramList = null;
        FadeIn(WorkspaceContent);
    }

    private void Programs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox list || e.AddedItems.Count == 0 || e.AddedItems[0] is not ProgramItem program) return;
        if (_activeProgramList is not null && _activeProgramList != list) _activeProgramList.SelectedItem = null;
        _activeProgramList = list;
        _viewModel.SelectedProgram = program;
        FadeIn(ProgramInspector);
        e.Handled = true;
    }

    private void WorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != sender || WorkspaceContent is null) return;
        FadeIn(WorkspaceContent);
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await _viewModel.RefreshAsync();
    private async void Discover_Click(object sender, RoutedEventArgs e) => await _viewModel.DiscoverAsync();
    private async void ConsolidateRemote_Click(object sender, RoutedEventArgs e) => await _viewModel.ConsolidateRemoteStartupAsync();
    private async void EnsureRemote_Click(object sender, RoutedEventArgs e) => await _viewModel.EnsureRemoteRunningAsync();
    private void OpenProject_Click(object sender, RoutedEventArgs e) => _viewModel.OpenProject();
    private void OpenProgram_Click(object sender, RoutedEventArgs e) => _viewModel.OpenProgram();
    private async void LaunchProgram_Click(object sender, RoutedEventArgs e) => await _viewModel.LaunchProgramAsync();
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
        MessageBox.Show(this, result.Detail, "Desktop Commander 복구", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
}
