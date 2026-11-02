using AIControlTower.Models;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class OwnedManualReaderDrainTests
{
    [Fact]
    public async Task ActualReaderLedgerRetainsOlderReadAfterLatestReadFinishes()
    {
        var older = new TaskCompletionSource<ReaderDocument>(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = new DocumentReaderViewModel((_, path, _) => path.EndsWith("older.md", StringComparison.Ordinal)
            ? older.Task : Task.FromResult(new ReaderDocument(path, "fixture", "new", "markdown", "")));
        var idle = typeof(DocumentReaderViewModel).GetProperty("OwnedReadsIdle"); Assert.NotNull(idle);
        var first = vm.OpenAsync(@"D:\fixture", @"D:\fixture\older.md");
        try
        {
            await vm.OpenAsync(@"D:\fixture", @"D:\fixture\newer.md");
            Assert.False(vm.IsLoading); Assert.False((bool)idle!.GetValue(vm)!);
        }
        finally { older.TrySetResult(new ReaderDocument(@"D:\fixture\older.md", "fixture", "old", "markdown", "")); await first; }
        Assert.True((bool)idle!.GetValue(vm)!); Assert.Equal("new", vm.RawText);
    }
    [Fact]
    public async Task FrozenReaderAdmissionDoesNotStartReadOrChangeHistory()
    {
        var called = 0;
        var vm = new DocumentReaderViewModel((_, path, _) => { called++; return Task.FromResult(new ReaderDocument(path, "fixture", "text", "markdown", "")); });
        var freeze = typeof(DocumentReaderViewModel).GetMethod("FreezeOwnedAdmission"); Assert.NotNull(freeze);
        freeze!.Invoke(vm, [true]); await vm.OpenAsync(@"D:\fixture", @"D:\fixture\note.md");
        Assert.Equal(0, called); Assert.Equal(0, vm.HistoryCount);
        freeze.Invoke(vm, [false]); await vm.OpenAsync(@"D:\fixture", @"D:\fixture\note.md");
        Assert.Equal(1, called); Assert.Equal(1, vm.HistoryCount);
    }
}
