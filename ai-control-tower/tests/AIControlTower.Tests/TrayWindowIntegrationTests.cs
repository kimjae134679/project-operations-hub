using AIControlTower.Services;

namespace AIControlTower.Tests;

/// <summary>Startup/close wiring only; never creates a Window, HWND or notification icon.</summary>
public sealed class TrayWindowIntegrationTests
{
    private static string Source(string name) => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../src/AIControlTower",name)));
    [Fact] public void JevEntryUsesRegisteredTaskPermissionNotBroadInstallationPermission()
    {
        var code=Source("MainWindow.xaml.cs");var start=code.IndexOf("private void Jev_Click",StringComparison.Ordinal);
        var end=code.IndexOf("private async void Install_Click",start,StringComparison.Ordinal);
        Assert.Contains("PrepareJevSession()",code[start..end]);Assert.Contains("BlockRegisteredOperation()",code[start..end]);Assert.DoesNotContain("BlockOperation()",code[start..end]);
    }
    [Fact] public void OnlyManualStartupUsesExplicitShutdownAndSessionEndingIsForwarded()
    {
        var app=Source("App.xaml.cs");
        Assert.Contains("if(policy.IsManualControl)ShutdownMode=ShutdownMode.OnExplicitShutdown;",app);
        Assert.Contains("PrepareSessionEnding()",app);
    }
    [Fact] public void ManualWindowCloseUsesTrayPolicyBeforeDisposalAndNoRemoteStop()
    {
        var code=Source("MainWindow.xaml.cs");
        Assert.Contains("BackgroundLifetimePolicy.EvaluateClose",code);
        Assert.Contains("_viewModel.IsBusy",code);
        Assert.Contains("WindowCloseAction.HideToTray",code);
        Assert.Contains("_tray.TryStart",code);
        Assert.Contains("manual-control-status.json",code);
        var start=code.IndexOf("private void HandleWindowClosing",StringComparison.Ordinal);
        var traySignature=System.Text.RegularExpressions.Regex.Match(code[start..],@"private\s+(?:async\s+)?void\s+RequestTrayExit\s*\(");
        Assert.True(traySignature.Success);
        var end=start+traySignature.Index;
        Assert.True(start>=0 && end>start);
        var close=code[start..end];
        Assert.DoesNotContain("Dispose",close);Assert.DoesNotContain("StopProgram",close);
        Assert.DoesNotContain("StopAsync",close);Assert.Contains("Hide()",close);
        var tray=code[end..code.IndexOf("[DllImport",end,StringComparison.Ordinal)];
        Assert.Matches(@"if\s*\(\s*await\s+RequestOwnedManualExitAsync\(\)\s*==\s*""accepted""\s*\)\s*CompleteOwnedManualExit\(\);",tray);
        Assert.DoesNotContain("Shutdown",tray);Assert.DoesNotContain("Dispose",tray);Assert.DoesNotContain("StopAsync",tray);
        var complete=code[code.IndexOf("public void CompleteOwnedManualExit",StringComparison.Ordinal)..start];
        Assert.Contains("if(!_viewModel.IsManualControl || !_viewModel.ManualExitFrozen)return;",complete);
        Assert.Contains("Application.Current.Shutdown()",complete);
        Assert.True(complete.IndexOf("ManualExitFrozen",StringComparison.Ordinal)<complete.IndexOf("Shutdown()",StringComparison.Ordinal));
    }
}
