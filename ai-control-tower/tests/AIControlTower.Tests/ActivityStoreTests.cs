using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class ActivityStoreTests
{
    [Fact]
    public void StartRecordsTaskAndElapsedTime()
    {
        var store = new ActivityStore();
        store.Start("C:\\repo", "Check status", null);

        Assert.Equal("Check status", store.Current.Task);
        Assert.True(store.Current.Elapsed >= TimeSpan.Zero);
    }
}
