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
        var settings = Services.ControlTowerSettings.Load();
        var communicationRoot = Array.IndexOf(e.Args, "--communication-hub");
        if (communicationRoot >= 0 && communicationRoot + 1 < e.Args.Length)
        {
            settings.CommunicationHubPath = Path.GetFullPath(e.Args[communicationRoot + 1]);
            if (e.Args.Contains("--verify-ui")) settings.AutoPublishCommunication = false;
        }
        MainWindow = new MainWindow(settings);
        _listener = ThreadPool.RegisterWaitForSingleObject(_activate, (_, _) => Dispatcher.InvokeAsync(() =>
        {
            if (MainWindow.WindowState == WindowState.Minimized) MainWindow.WindowState = WindowState.Normal;
            MainWindow.Show(); MainWindow.Activate();
        }), null, Timeout.Infinite, false);
        MainWindow.Show();
        var capture = Array.IndexOf(e.Args, "--verify-ui");
        if (capture >= 0 && capture + 1 < e.Args.Length)
        {
            var window = (MainWindow)MainWindow;
            _ = Dispatcher.InvokeAsync(async () =>
            {
                try { await Services.UiVerification.RunAsync(window, Path.GetFullPath(e.Args[capture + 1]), e.Args.Contains("--consolidate-remote")); }
                catch (Exception ex)
                {
                    var output = Path.GetFullPath(e.Args[capture + 1]); Directory.CreateDirectory(output);
                    File.WriteAllText(Path.Combine(output, "ui-verification-error.txt"), Services.ProcessRunner.Sanitize(ex.ToString()));
                }
            });
        }
    }
    protected override void OnExit(ExitEventArgs e)
    {
        _listener?.Unregister(null); _activate?.Dispose();
        if (_owns) _instance?.ReleaseMutex();
        _instance?.Dispose(); base.OnExit(e);
    }
}
