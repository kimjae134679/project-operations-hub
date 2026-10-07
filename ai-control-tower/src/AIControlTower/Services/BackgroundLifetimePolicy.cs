namespace AIControlTower.Services;

public enum WindowCloseAction { KeepVisible, HideToTray, Exit }

/// <summary>Pure decisions only. Hiding is not disposal, task cancellation or remote shutdown.</summary>
public static class BackgroundLifetimePolicy
{
    public static WindowCloseAction EvaluateClose(bool trayAvailable,bool exitRequested,bool ownedTasksBusy,bool sessionEnding)
    {
        if(sessionEnding)return WindowCloseAction.Exit;
        if(exitRequested)return ownedTasksBusy?WindowCloseAction.KeepVisible:WindowCloseAction.Exit;
        return trayAvailable?WindowCloseAction.HideToTray:WindowCloseAction.KeepVisible;
    }
}
