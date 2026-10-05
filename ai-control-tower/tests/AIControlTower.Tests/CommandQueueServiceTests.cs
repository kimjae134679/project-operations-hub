using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class CommandQueueServiceTests
{
    [Fact]
    public void ClaimNextMovesTxtTaskIntoProcessing()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new CommandQueueService(root);
        service.EnsureDirectories();
        File.WriteAllText(Path.Combine(service.InboxPath, "task-1.txt"), "상태를 확인해줘");

        var task = service.TryClaimNext();

        Assert.NotNull(task);
        Assert.Equal("상태를 확인해줘", task!.Instruction);
        Assert.False(File.Exists(Path.Combine(service.InboxPath, "task-1.txt")));
        Assert.True(File.Exists(task.ProcessingPath));
        Directory.Delete(root, true);
    }

    [Fact]
    public void ClaimNextDoesNotMoveInboxTaskWhenExecutionIsDisabled()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new CommandQueueService(root);
        service.EnsureDirectories();
        var inboxFile = Path.Combine(service.InboxPath, "task-disabled.txt");
        File.WriteAllText(inboxFile, "정책상 실행하면 안 되는 작업");
        var policyAwareClaim = typeof(CommandQueueService).GetMethod(nameof(CommandQueueService.TryClaimNext), [typeof(bool)]);

        Assert.NotNull(policyAwareClaim);
        var task = (QueueTask?)policyAwareClaim!.Invoke(service, [false]);

        Assert.Null(task);
        Assert.True(File.Exists(inboxFile));
        Assert.False(File.Exists(Path.Combine(service.ProcessingPath, "task-disabled.txt")));
        Directory.Delete(root, true);
    }
}
