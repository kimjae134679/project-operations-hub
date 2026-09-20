using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class JevStatusProvider : CommandStatusProvider
{
    public JevStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "jev";
    public override async Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        var launcher = EnvironmentProbe.FindCommand("jev-codex");
        if (launcher is null) return Status(Id, "Jev Router", StatusKind.NotInstalled, "jev-codex 실행 파일을 찾지 못했습니다.");
        if (!EnvironmentProbe.HasAnyEnvironmentVariable("JEV_API_KEY", "TYPESAFE_API_KEY")) return Status(Id, "Jev Router", StatusKind.NotConfigured, "API 키 설정 여부를 확인하지 못했습니다.");
        var result = await Runner.RunHiddenAsync("cmd.exe", "/d /c \"" + launcher + " --version\"", TimeSpan.FromSeconds(8), cancellationToken);
        return Status(Id, "Jev Router", ToStatusKind(result.ExitCode), result.ExitCode == 0 ? "설치와 런처 확인이 완료되었습니다." : "런처 확인에 실패했습니다. PowerShell 실행 정책 또는 Codex 연결을 확인하세요.");
    }
}
