using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class LocalExecutionPolicyTests
{
    [Fact]
    public void LocalJevExecutionIsDisabledByDefault()
    {
        Assert.False(LocalExecutionPolicy.AllowLocalJevExecution);
    }

    [Fact]
    public void RunJevTask_WhenLocalExecutionIsDisabled_ShowsPolicyMessage()
    {
        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", string.Empty);
            using var viewModel = new MainViewModel { TaskInput = "Do not launch Jev during this policy test." };

            viewModel.RunJevTask();

            Assert.Equal(LocalExecutionPolicy.LocalJevDisabledMessage, viewModel.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }
}
