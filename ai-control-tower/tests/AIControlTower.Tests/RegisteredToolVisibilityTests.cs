using AIControlTower.Models;
using AIControlTower.ViewModels;
namespace AIControlTower.Tests;

public sealed class RegisteredToolVisibilityTests
{
    [Fact] public void KnownInstalledAndExplicitlyRegisteredToolsStayVisibleWithoutUnconfirmedCandidates()
    {
        var jev=new ToolStatusViewModel(new("jev","Jev",StatusKind.NotConfigured,"installed",DateTimeOffset.UtcNow));
        var codex=new ToolStatusViewModel(new("codex","Codex",StatusKind.Unknown,"no live probe",DateTimeOffset.UtcNow));
        var candidate=new ToolStatusViewModel(new("aider","Aider",StatusKind.NotConfigured,"unconfirmed installation",DateTimeOffset.UtcNow));
        var registered=new ToolStatusViewModel(new("project-bridge","ProjectBridge",StatusKind.Unknown,"no probe",DateTimeOffset.UtcNow));registered.SetUsageEvidence("explicit registered task");
        var method=typeof(MainViewModel).GetMethod("SelectKnownTools");Assert.NotNull(method);
        var visible=(IReadOnlyList<ToolStatusViewModel>)method!.Invoke(null,[new[]{jev,codex,candidate,registered},new HashSet<string>{"jev","codex"},false])!;
        Assert.Equal(new[]{"jev","codex","project-bridge"},visible.Select(t=>t.Id));
        Assert.False(codex.HasUsageEvidence);Assert.Equal("미확인",codex.StateLabel);
        var all=(IReadOnlyList<ToolStatusViewModel>)method.Invoke(null,[new[]{jev,codex,candidate,registered},new HashSet<string>{"jev","codex"},true])!;
        Assert.Equal(4,all.Count);
    }
}
