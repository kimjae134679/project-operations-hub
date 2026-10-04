using System.Windows;
namespace AIControlTower;
public partial class App : Application
{
    private Mutex? _instance;
    private EventWaitHandle? _activate;
    private RegisteredWaitHandle? _listener;
    private bool _owns;
    protected override void OnStartup(StartupEventArgs e)
    {
        _instance = new Mutex(false, @"Local\AIControlTower.Application");
        _activate = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\AIControlTower.Activate");
        try { _owns = _instance.WaitOne(0); } catch (AbandonedMutexException) { _owns = true; }
        if (!_owns) { _activate.Set(); Shutdown(); return; }
        base.OnStartup(e);
        MainWindow = new MainWindow();
        _listener = ThreadPool.RegisterWaitForSingleObject(_activate, (_, _) => Dispatcher.InvokeAsync(() =>
        {
            if (MainWindow.WindowState == WindowState.Minimized) MainWindow.WindowState = WindowState.Normal;
            MainWindow.Show(); MainWindow.Activate();
        }), null, Timeout.Infinite, false);
        MainWindow.Show();
    }
    protected override void OnExit(ExitEventArgs e)
    {
        _listener?.Unregister(null); _activate?.Dispose();
        if (_owns) _instance?.ReleaseMutex();
        _instance?.Dispose(); base.OnExit(e);
    }
}
