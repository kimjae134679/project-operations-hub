using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class CodexStatusProvider : CommandStatusProvider
{
    public CodexStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "codex";
    public override async Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        if (EnvironmentProbe.FindCommand("codex") is null) return Status(Id, "Codex", StatusKind.NotInstalled, "Codex CLI를 PATH에서 찾지 못했습니다.");
        var result = await Runner.RunHiddenAsync("cmd.exe", "/d /c codex login status", TimeSpan.FromSeconds(8), cancellationToken);
        var kind = result.ExitCode == 0 ? StatusKind.Ready : result.TimedOut ? StatusKind.Unknown : StatusKind.NotConfigured;
        return Status(Id, "Codex", kind, kind == StatusKind.Ready ? "로그인 상태를 확인했습니다." : "로그인 상태를 확인하지 못했습니다.");
    }
}
