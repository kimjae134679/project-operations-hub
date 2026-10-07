namespace AIControlTower.Services;

/// <summary>Explicit operational versus local viewer lifecycle. No process or storage operations.</summary>
public sealed record StartupPolicy(bool IsLocalView)
{
    public bool IsManualControl { get; init; }
    public bool NoActivateExisting { get; init; }
    public bool AllowMutations => !IsLocalView && !IsManualControl;
    public bool AllowRegisteredTasks => !IsLocalView;
    public bool AllowLiveQueries => !IsLocalView;
    public bool AllowAutomaticMaintenance => IsManualControl;
    public string InstanceIdentity => IsLocalView ? "LocalView" : IsManualControl ? "ManualControl" : "Application";
    public string ActivationEvent => IsLocalView ? @"Local\AIControlTower.LocalView.Activate" : IsManualControl ? @"Local\AIControlTower.ManualControl.Activate" : @"Local\AIControlTower.Activate";
    public static bool RequestsLocalView(IEnumerable<string> args) => args.Any(a=>a.Equals("--local-view",StringComparison.OrdinalIgnoreCase));
    public static bool TryCreate(string[] args,out StartupPolicy policy)
    {
        var manual=args.Any(a=>a.Equals("--manual-control",StringComparison.OrdinalIgnoreCase));
        var noActivate=args.Any(a=>a.Equals("--no-activate-existing",StringComparison.OrdinalIgnoreCase));
        policy=new(RequestsLocalView(args)){IsManualControl=manual,NoActivateExisting=noActivate};
        if(manual)return !policy.IsLocalView && args[0]=="--manual-control" &&
            (args.Length==1 || args.Length==2 && args[1]=="--no-activate-existing");
        if(noActivate)return false;
        // A viewer request never falls through into remote/control/verification modes.
        return !policy.IsLocalView || args.Length==1 && args[0]=="--local-view";
    }
    public void RecoverRuns(Action recover) { if(AllowMutations)recover(); }
    public async Task InitializeAsync(Func<Task> discoverLocal,Func<Task> readLocal,Func<Task> operational)
    {
        if(IsLocalView || IsManualControl) { await discoverLocal(); await readLocal(); }
        else await operational();
    }
}
