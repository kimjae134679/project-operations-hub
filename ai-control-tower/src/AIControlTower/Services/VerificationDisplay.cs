using System.Windows;

namespace AIControlTower.Services;

public static class VerificationDisplay
{
    public static bool Quiet { get; set; }
    public static void Show(Window window)
    {
        if (Quiet)
        {
            window.ShowActivated = false;
            window.ShowInTaskbar = false;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = -20000;
            window.Top = -20000;
        }
        window.Show();
    }
}
