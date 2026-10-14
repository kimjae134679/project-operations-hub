using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
namespace AIControlTower.Tests;
public sealed class ProjectCommunicationTargetWiringTests
{
    [Fact] public void DiscoveredManifestIdentityUsesExplicitCatalogPathNotDisplayNameForCommunicationMapping()
    {
        var projects=new[]{new ProjectItem{Id="PhoneLOL",DisplayName="같은 이름",Path=@"D:\fixture\phone"},new ProjectItem{Id="unknown",DisplayName="같은 이름",Path=@"D:\fixture\unmapped"}};
        var catalog=new CatalogDefinition{Projects=[new(){Id="phonelol-current",Name="같은 이름",Path=@"D:\fixture\phone"}]};
        var method=typeof(MainViewModel).GetMethod("CurrentCommunicationTargets");Assert.NotNull(method);
        var targets=(IReadOnlyList<CommunicationProjectTarget>)method!.Invoke(null,[projects,catalog])!;
        Assert.Equal("PhoneLOL",targets[0].ProjectId);Assert.Equal("PhoneLOL",targets[0].CommunicationId);Assert.Null(targets[1].CommunicationId);
    }
}
