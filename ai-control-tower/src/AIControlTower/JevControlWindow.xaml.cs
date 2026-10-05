using System.Windows;
using AIControlTower.ViewModels;

namespace AIControlTower;

public partial class JevControlWindow : Window
{
    private readonly MainViewModel _viewModel;

    public JevControlWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
    }

    private void RunJev_Click(object sender, RoutedEventArgs e) => _viewModel.RunJevTask();
    private async void CancelJev_Click(object sender, RoutedEventArgs e) => await _viewModel.CancelJevTaskAsync();
}
