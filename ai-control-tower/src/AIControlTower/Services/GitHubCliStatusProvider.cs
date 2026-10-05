using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class GitHubCliStatusProvider : CommandStatusProvider
{
    public GitHubCliStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "github-cli";
    public override async Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        if (EnvironmentProbe.FindCommand("gh") is null) return Status(Id, "GitHub CLI", StatusKind.NotInstalled, "GitHub CLI를 PATH에서 찾지 못했습니다.");
        var result = await Runner.RunHiddenAsync("cmd.exe", "/d /c gh auth status", TimeSpan.FromSeconds(8), cancellationToken);
        var kind = result.ExitCode == 0 ? StatusKind.Ready : result.TimedOut ? StatusKind.Unknown : StatusKind.NotConfigured;
        return Status(Id, "GitHub CLI", kind, kind == StatusKind.Ready ? "로그인 상태를 확인했습니다." : "로그인 상태가 없거나 토큰이 유효하지 않습니다.");
    }
}
