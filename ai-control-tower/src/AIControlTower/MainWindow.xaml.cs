using System.Windows;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private readonly InstallationService _installationService = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.RefreshAsync();
        Closed += (_, _) => _viewModel.Dispose();
    }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await _viewModel.RefreshAsync();
    private void RunJev_Click(object sender, RoutedEventArgs e) => _viewModel.RunJevTask();
    private async void CancelJev_Click(object sender, RoutedEventArgs e) => await _viewModel.CancelJevTaskAsync();
    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        var result = await _installationService.InstallAsync(Environment.ProcessPath ?? string.Empty, CancellationToken.None);
        MessageBox.Show(result.Detail + Environment.NewLine + result.InstallPath, "AI Control Tower 설치", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
    private async void Uninstall_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
        var result = await _installationService.UninstallAsync(path, CancellationToken.None);
        MessageBox.Show(result.Detail, "AI Control Tower 제거", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        var preferred = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "_My", "AI", "Applications", "AIControlTower");
        var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
        var result = await _installationService.RestoreDesktopCommanderStartupAsync(Directory.Exists(preferred) ? preferred : fallback, CancellationToken.None);
        MessageBox.Show(result.Detail, "Desktop Commander 시작 구성 복구", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
}
