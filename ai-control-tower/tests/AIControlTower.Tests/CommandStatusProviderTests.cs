using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class CommandStatusProviderTests
{
    [Theory]
    [InlineData(0, StatusKind.Ready)]
    [InlineData(1, StatusKind.Error)]
    [InlineData(-1, StatusKind.Unknown)]
    public void ExitCodeMapsToSafeStatus(int exitCode, StatusKind expected)
    {
        Assert.Equal(expected, CommandStatusProvider.ToStatusKind(exitCode));
    }
}
