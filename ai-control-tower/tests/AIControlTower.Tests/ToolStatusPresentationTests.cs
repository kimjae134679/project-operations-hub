using AIControlTower.Models;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class ToolStatusPresentationTests
{
    [Fact]
    public void EveryRegisteredToolRemainsDiscoverableByItsProductName()
    {
        var providers = new[]
        {
            ("desktop-commander", "Remote Desktop Commander", "Remote Desktop Commander"),
            ("jev", "Jev Router", "Jev Router"),
            ("codex", "Codex", "Codex"),
            ("github-cli", "GitHub CLI", "GitHub CLI"),
            ("n8n", "n8n local bridge", "n8n"),
            ("ai-ops-runner", "GitHub Actions · MultiGod-PC", "GitHub Actions"),
            ("delivery-chain", "GPT → Jev 전달", "Jev"),
            ("aider", "Aider", "Aider"),
            ("hyperframes", "HyperFrames", "HyperFrames")
        };
        foreach (var (id, name, product) in providers)
        {
            var row = Row(id, name, StatusKind.Unknown);
            Assert.True(row.IsUserFacing);
            Assert.Contains(product, row.DisplayName);
            Assert.False(string.IsNullOrWhiteSpace(row.Purpose));
        }
    }

    [Fact]
    public void MissingInstallationIsDistinctFromAnExecutionError()
    {
        var row = Row("jev", "Jev Router", StatusKind.NotInstalled);
        Assert.Equal("미설치", row.StateLabel);
        row.Update(new("jev", "Jev Router", StatusKind.Error, "실행 오류", DateTimeOffset.Now));
        Assert.Equal("오류", row.StateLabel);
    }

    [Fact]
    public void InstallationOnlyDoesNotClaimAiderOrHyperFramesAreRunning()
    {
        foreach (var product in new[] { "Aider", "HyperFrames" })
        {
            var row = Row(product.ToLowerInvariant(), product, StatusKind.Ready);
            Assert.Equal("설치 확인", row.StateLabel);
            Assert.Contains("설치 확인과 실제", row.Purpose);
            Assert.NotEqual("실행 중", row.StateLabel);
        }
    }

    private static ToolStatusViewModel Row(string id, string name, StatusKind kind) =>
        new(new(id, name, kind, "확인 범위는 상세에서 구분합니다.", DateTimeOffset.Now));
}
