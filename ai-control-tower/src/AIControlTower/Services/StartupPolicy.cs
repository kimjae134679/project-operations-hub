namespace AIControlTower.Services;

/// <summary>Explicit operational versus local viewer lifecycle. No process or storage operations.</summary>
public sealed record StartupPolicy(bool IsLocalView)
{
    public bool AllowMutations => !IsLocalView;
    public string InstanceIdentity => IsLocalView ? "LocalView" : "Application";
    public string ActivationEvent => IsLocalView ? @"Local\AIControlTower.LocalView.Activate" : @"Local\AIControlTower.Activate";
    public static bool RequestsLocalView(IEnumerable<string> args) => args.Any(a=>a.Equals("--local-view",StringComparison.OrdinalIgnoreCase));
    public static bool TryCreate(string[] args,out StartupPolicy policy)
    {
        policy=new(RequestsLocalView(args));
        // A viewer request never falls through into remote/control/verification modes.
        return !policy.IsLocalView || args.Length==1 && args[0]=="--local-view";
    }
    public void RecoverRuns(Action recover) { if(AllowMutations)recover(); }
    public async Task InitializeAsync(Func<Task> discoverLocal,Func<Task> readLocal,Func<Task> operational)
    {
        if(IsLocalView) { await discoverLocal(); await readLocal(); }
        else await operational();
    }
}
