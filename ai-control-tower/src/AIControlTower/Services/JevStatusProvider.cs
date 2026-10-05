using AIControlTower.Models;
namespace AIControlTower.Services;
public sealed class JevStatusProvider : CommandStatusProvider
{
    public JevStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "jev";
    public override Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        var package = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "node_modules", "jev-router", "package.json");
        if (!File.Exists(package)) return Task.FromResult(Status(Id, "Jev Router", StatusKind.NotInstalled, "설치된 jev-router package를 찾지 못했습니다."));
        if (!EnvironmentProbe.HasAnyEnvironmentVariable("JEV_API_KEY", "TYPESAFE_API_KEY")) return Task.FromResult(Status(Id, "Jev Router", StatusKind.NotConfigured, "설치는 확인했으나 키 설정 여부를 확인하지 못했습니다."));
        return Task.FromResult(Status(Id, "Jev Router", StatusKind.Ready, "설치 파일·키 존재 확인. 인증·라우팅 왕복·업무 성공은 미검증이며 필요할 때 선택 사용합니다."));
    }
}
