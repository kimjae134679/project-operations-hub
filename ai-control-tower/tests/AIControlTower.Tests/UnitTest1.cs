using AIControlTower.Models;

namespace AIControlTower.Tests;

public sealed class ToolStatusTests
{
    [Fact]
    public void UnknownStatusIsNotASuccess()
    {
        var status = new ToolStatus("codex", "Codex", StatusKind.Unknown, "Not verified", DateTimeOffset.UtcNow);

        Assert.NotEqual(StatusKind.Ready, status.Kind);
        Assert.NotEqual(StatusKind.Running, status.Kind);
    }
}
