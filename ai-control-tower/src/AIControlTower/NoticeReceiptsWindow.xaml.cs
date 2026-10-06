using System.Windows;
namespace AIControlTower;
public partial class NoticeReceiptsWindow : Window
{
    public NoticeReceiptsWindow(object viewModel) { InitializeComponent(); DataContext = viewModel; PreviewMouseWheel += Services.MouseWheelRouting.HandlePreviewMouseWheel; }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
