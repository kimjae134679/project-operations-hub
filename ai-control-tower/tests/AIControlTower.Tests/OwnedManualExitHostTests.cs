using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class OwnedManualExitHostTests
{
    [Fact]
    public void ProductionOwnerMustBeCanonicalDedicatedVersionExecutableNotSharedManagedHost()
    {
        var method = typeof(OwnedManualExitProtocol).GetMethod("IsProductionHost"); Assert.NotNull(method);
        const string root = @"D:\A_KJ\AI\Applications\AIControlTower\versions";
        var executable = root + @"\fixture-build\AIControlTower.exe";
        bool Valid(string path, string directory) => (bool)method!.Invoke(null, [path, directory])!;
        Assert.True(Valid(executable, root + @"\fixture-build\"));
        Assert.True(Valid(executable.ToUpperInvariant(), root + @"\fixture-build"));
        foreach (var invalid in new[] { @"C:\Program Files\dotnet\dotnet.exe", root + @"\fixture-build\testhost.exe", root + @"\AIControlTower.exe", root + @"\fixture-build\nested\AIControlTower.exe", root + @"\fixture-build\..\fixture-build\AIControlTower.exe", @"D:\source\bin\AIControlTower.exe", @"C:\fixture\AIControlTower.exe" })
            Assert.False(Valid(invalid, Path.GetDirectoryName(invalid)!));
        Assert.False(Valid(executable, root + @"\different-build"));
    }
    [Fact]
    public void ProductionAppChecksDedicatedHostBeforeClientOrServerButDoesNotOfferRootOverride()
    {
        var app = File.ReadAllText(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\source\ai-control-tower\src\AIControlTower\App.xaml.cs");
        var client = app[app.IndexOf("private async Task RunManualExitClientAsync", StringComparison.Ordinal)..app.IndexOf("private async Task StartOwnedManualExitEndpointAsync", StringComparison.Ordinal)];
        Assert.Contains("IsProductionHost", client);
        Assert.True(client.IndexOf("IsProductionHost", StringComparison.Ordinal) < client.IndexOf("OwnedManualExitEndpoint.RequestAsync", StringComparison.Ordinal));
        var server = app[app.IndexOf("private async Task StartOwnedManualExitEndpointAsync", StringComparison.Ordinal)..app.IndexOf("protected override void OnStartup", StringComparison.Ordinal)];
        Assert.Contains("IsProductionHost", server);
        Assert.True(server.IndexOf("IsProductionHost", StringComparison.Ordinal) < server.IndexOf("OwnedManualExitEndpoint.StartAsync", StringComparison.Ordinal));
        Assert.DoesNotContain("e.Args", client); Assert.DoesNotContain("e.Args", server);
    }
}
