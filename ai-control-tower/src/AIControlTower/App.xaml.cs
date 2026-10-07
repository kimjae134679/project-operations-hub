using System.Windows;
namespace AIControlTower;
public partial class App : Application
{
    public static bool ShouldActivateExistingInstance(IEnumerable<string> arguments)
    {
        var args=arguments.ToArray();
        if(Services.StartupPolicy.TryCreate(args,out var policy) && policy.NoActivateExisting)return false;
        return !args.Contains("--background", StringComparer.Ordinal) && !Services.StartupPolicy.RequestsLocalView(args);
    }
    private Mutex? _instance;
    private EventWaitHandle? _activate;
    private RegisteredWaitHandle? _listener;
    private bool _owns;
    private CancellationTokenSource? _remoteLifetime;
    private CancellationTokenSource? _recordPublishLifetime;
    private Task? _recordPublishTask;
    private System.Windows.Interop.HwndSource? _shutdownWindow;
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        e.Cancel = false;
        if(MainWindow is MainWindow window)window.PrepareSessionEnding();
        _recordPublishLifetime?.Cancel();
        _remoteLifetime?.Cancel();
        base.OnSessionEnding(e);
    }
    private bool StartRemoteSupervisor(StartupEventArgs e)
    {
        var control = Array.IndexOf(e.Args, "--remote-control");
        if (control >= 0)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _ = RunRemoteControlAsync(control + 1 < e.Args.Length ? e.Args[control + 1] : "invalid");
            return true;
        }
        if (!e.Args.Contains("--remote-supervisor")) return false;
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _remoteLifetime = new CancellationTokenSource();
        // Invisible top-level HWND receives Windows logoff/shutdown broadcasts.
        _shutdownWindow = new System.Windows.Interop.HwndSource(
            new System.Windows.Interop.HwndSourceParameters("AIControlTower Remote Recovery")
            { Width = 0, Height = 0, WindowStyle = unchecked((int)0x80000000) });
        _shutdownWindow.AddHook((IntPtr hwnd, int message, IntPtr wparam, IntPtr lparam, ref bool handled) =>
        {
            if (message == 0x0011) { handled = true; return new IntPtr(1); }
            if (message == 0x0016 && wparam != IntPtr.Zero)
            { _remoteLifetime.Cancel(); Dispatcher.BeginInvoke(() => Shutdown()); handled = true; }
            return IntPtr.Zero;
        });
        _ = RunSupervisorAsync();
        return true;
    }
    private async Task RunRemoteControlAsync(string action)
    {
        var exitCode = 0;
        try
        {
            var bridge = new Services.RemoteBridgeService(new Services.ProcessRunner());
            var detail = action switch
            {
                "start" => await bridge.EnsureRunningAsync(CancellationToken.None),
                "stop" => await new Services.RemoteSupervisor(bridge).StopAsync(CancellationToken.None),
                "repair" => (await new Services.InstallationService().RestoreDesktopCommanderStartupAsync(
                    Path.GetDirectoryName(Environment.ProcessPath!)!, CancellationToken.None)).Detail,
                _ => throw new ArgumentException("알 수 없는 원격 제어 요청")
            };
            Directory.CreateDirectory(Services.ControlTowerSettings.DataDirectory);
            File.WriteAllText(Path.Combine(Services.ControlTowerSettings.DataDirectory, "remote-control-result.json"),
                System.Text.Json.JsonSerializer.Serialize(new { Action = action, Detail = detail, UpdatedUtc = DateTimeOffset.UtcNow, ExitCode = 0 }));
        }
        catch (Exception ex)
        {
            exitCode = 1;
            try
            {
                Directory.CreateDirectory(Services.ControlTowerSettings.DataDirectory);
                File.WriteAllText(Path.Combine(Services.ControlTowerSettings.DataDirectory, "remote-control-result.json"),
                    System.Text.Json.JsonSerializer.Serialize(new { Action = action, Detail = Services.ProcessRunner.Sanitize(ex.Message), UpdatedUtc = DateTimeOffset.UtcNow, ExitCode = 1 }));
            }
            catch (IOException) { }
        }
        finally { await Dispatcher.InvokeAsync(() => Shutdown(exitCode)); }
    }
    private async Task RunSupervisorAsync()
    {
        try { await new Services.RemoteSupervisor(new Services.RemoteBridgeService(new Services.ProcessRunner())).RunAsync(_remoteLifetime!.Token); }
        catch (OperationCanceledException) { }
        finally { await Dispatcher.InvokeAsync(() => Shutdown()); }
    }
    private bool TryStartRecordPublishing(StartupEventArgs e)
    {
        if(!Services.RegisteredRecordPublishing.RequestsOnce(e.Args))return false;
        // Fail-closed dedicated headless entry: never falls through settings/mutex/GUI/remote initialization.
        ShutdownMode=ShutdownMode.OnExplicitShutdown;
        if(e.Args.Length!=2||e.Args[0]!="--publish-records-once") {Services.RegisteredRecordPublishing.WriteConsoleOutcome(new{Status="invalid_request",ExitCode=2});Shutdown(2);return true;}
        _recordPublishLifetime=new CancellationTokenSource();
        _recordPublishTask=RunRecordPublishingAsync(e.Args,_recordPublishLifetime.Token);
        return true;
    }
    private async Task RunRecordPublishingAsync(string[] args,CancellationToken ct)
    {
        var code=1;
        try{code=await Task.Run(()=>new Services.RegisteredRecordPublishing().RunOnceAsync(args,ct),ct);}
        catch(OperationCanceledException){}
        catch(Exception){}
        finally{await Dispatcher.InvokeAsync(()=>Shutdown(code));}
    }
    protected override void OnStartup(StartupEventArgs e)
    {
        if(TryStartRecordPublishing(e))return;
        if(!Services.StartupPolicy.TryCreate(e.Args,out var policy)){Shutdown(2);return;}
        if(policy.IsManualControl)ShutdownMode=ShutdownMode.OnExplicitShutdown;
        var workFixture = Array.IndexOf(e.Args,"--verify-work-dashboard");
        if(workFixture>=0)
        {
            // Standalone in-memory control rendering: no settings, mutex activation,
            // MainWindow, process execution, communication or remote initialization.
            ShutdownMode=ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);
            _=Dispatcher.InvokeAsync(async () =>
            {
                var code=0;
                try
                {
                    if(workFixture+1>=e.Args.Length)throw new ArgumentException("검증 출력 경로가 필요합니다.");
                    await Services.WorkDashboardVerification.RunAsync(e.Args[workFixture+1]);
                }
                catch(Exception ex) { code=1; Console.Error.WriteLine(Services.ProcessRunner.Sanitize(ex.ToString())); }
                finally { Shutdown(code); }
            });
            return;
        }
        if (StartRemoteSupervisor(e)) return;
        var verification = e.Args.Contains("--verify-ui");
        var identity = verification ? "Verification." + Environment.ProcessId : policy.InstanceIdentity;
        // LocalView has a separate identity and never signals/listens to the operating GUI.
        _instance = new Mutex(false, @"Local\AIControlTower." + identity);
        _activate = new EventWaitHandle(false, EventResetMode.AutoReset, verification ? @"Local\AIControlTower.Verification.Activate." + Environment.ProcessId : policy.ActivationEvent);
        try { _owns = _instance.WaitOne(0); } catch (AbandonedMutexException) { _owns = true; }
        if (!_owns) { if (ShouldActivateExistingInstance(e.Args)) _activate.Set(); Shutdown(); return; }
        base.OnStartup(e);
        var settings = policy.IsLocalView || policy.IsManualControl ? Services.ControlTowerSettings.LoadReadOnly() : Services.ControlTowerSettings.Load();
        if(policy.IsManualControl)settings.TransientDataDirectory=@"D:\A_KJ\AI\ControlTowerData\manual-control";
        if (e.Args.Contains("--verify-ui")) settings.IsTemporary = true;
        var communicationRoot = Array.IndexOf(e.Args, "--communication-hub");
        if (communicationRoot >= 0 && communicationRoot + 1 < e.Args.Length)
        {
            settings.CommunicationHubPath = Path.GetFullPath(e.Args[communicationRoot + 1]);
            if (e.Args.Contains("--verify-ui")) settings.AutoPublishCommunication = false;
        }
        MainWindow = new MainWindow(settings,policy);
        if (!policy.IsLocalView)
        _listener = ThreadPool.RegisterWaitForSingleObject(_activate, (_, _) => Dispatcher.InvokeAsync(() =>
        {
            if (MainWindow.WindowState == WindowState.Minimized) MainWindow.WindowState = WindowState.Normal;
            MainWindow.Show(); MainWindow.Activate();
        }), null, Timeout.Infinite, false);
        Services.VerificationDisplay.Quiet = e.Args.Contains("--verify-ui-hidden");
        if (e.Args.Contains("--background") && !e.Args.Contains("--verify-ui"))
            _ = ((MainWindow)MainWindow).InitializeAsync();
        else
            Services.VerificationDisplay.Show(MainWindow);
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
        _recordPublishLifetime?.Cancel(); _recordPublishLifetime?.Dispose();
        _remoteLifetime?.Cancel(); _shutdownWindow?.Dispose(); _remoteLifetime?.Dispose();
        _listener?.Unregister(null); _activate?.Dispose();
        if (_owns) _instance?.ReleaseMutex();
        _instance?.Dispose(); base.OnExit(e);
    }
}
