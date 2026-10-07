using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

// This collection alone owns process-global environment mutation; other collections remain parallel.
[CollectionDefinition("ProcessEnvironment", DisableParallelization = true)]
public sealed class ProcessEnvironmentCollection { }

[Collection("ProcessEnvironment")]
public sealed class LocalExecutionPolicyTests
{
    [Fact]
    public void LocalJevExecutionIsDisabledByDefault()
    {
        Assert.False(LocalExecutionPolicy.AllowLocalJevExecution);
    }

    [Fact]
    public void MainViewModel_ExposesDisabledJevPolicy()
    {
        using var viewModel = new MainViewModel(new ControlTowerSettings(), enablePolling: false);
        Assert.False(viewModel.IsLocalJevExecutionAllowed);
        Assert.Equal(LocalExecutionPolicy.LocalJevDisabledMessage, viewModel.LocalJevPolicyMessage);
    }

    [Fact]
    public void RunJevTask_WhenLocalExecutionIsDisabled_ShowsPolicyMessage()
    {
        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", string.Empty);
            using var viewModel = new MainViewModel(new ControlTowerSettings(), enablePolling: false) { TaskInput = "Do not launch Jev during this policy test." };

            viewModel.RunJevTask();

            Assert.Equal(LocalExecutionPolicy.LocalJevDisabledMessage, viewModel.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }
}

