using System.Windows;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private readonly InstallationService _installationService = new();
    private JevControlWindow? _jevWindow;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.RefreshAsync();
        Closed += (_, _) => _viewModel.Dispose();
    }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await _viewModel.RefreshAsync();
    private void Home_Click(object sender, RoutedEventArgs e) => DashboardScroll.ScrollToTop();
    private void Status_Click(object sender, RoutedEventArgs e) => StatusSection.BringIntoView();
    private void Jev_Click(object sender, RoutedEventArgs e) => OpenJevWindow();
    private void InstallSection_Click(object sender, RoutedEventArgs e) => InstallSection.BringIntoView();
    private void OpenJevWindow()
    {
        if (_jevWindow is { IsLoaded: true })
        {
            _jevWindow.Activate();
            return;
        }

        _jevWindow = new JevControlWindow(_viewModel) { Owner = this };
        _jevWindow.Closed += (_, _) => _jevWindow = null;
        _jevWindow.Show();
    }
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
