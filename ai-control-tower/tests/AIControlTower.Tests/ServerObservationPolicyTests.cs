using System.Reflection;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

/// <summary>Pure access policies and source linkage, never constructs MainViewModel or sends HTTP.</summary>
public sealed class ServerObservationPolicyTests
{
    [Fact]
    public void LiveQueryPermissionAndDisposedBoundaryAreIndependentOfMutationPermission()
    {
        var method=Required("ShouldQueryServer");
        Assert.True((bool)method.Invoke(null,[true,false])!);
        Assert.False((bool)method.Invoke(null,[false,false])!);
        Assert.False((bool)method.Invoke(null,[true,true])!);
        Assert.False((bool)method.Invoke(null,[false,true])!);
    }

    [Fact]
    public void ManualReadOnlyObservationCannotWriteAutomaticServerSummary()
    {
        var method=Required("ShouldWriteServerSummary");
        Assert.True((bool)method.Invoke(null,[true,false,true,true])!);
        Assert.False((bool)method.Invoke(null,[false,false,true,true])!);
        Assert.False((bool)method.Invoke(null,[true,true,true,true])!);
        Assert.False((bool)method.Invoke(null,[true,false,false,true])!);
        Assert.False((bool)method.Invoke(null,[true,false,true,false])!);
    }

    [Fact]
    public void ExistingServerAndRosterFetchUseApprovedLiveGateAndSummaryUsesMutationGate()
    {
        var file=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../src/AIControlTower/ViewModels/MultiplayerServerViewModel.cs"));
        var code=File.ReadAllText(file);
        Assert.Equal(2,code.Split("ShouldQueryServer(StartupMode.AllowLiveQueries",StringSplitOptions.None).Length-1);
        Assert.DoesNotContain("if(IsReadOnlyView)",code);
        Assert.Contains("ShouldWriteServerSummary(CanMutate",code);
    }
    private static MethodInfo Required(string name)
    { var method=typeof(MainViewModel).GetMethod(name);Assert.NotNull(method);return method!; }
}
