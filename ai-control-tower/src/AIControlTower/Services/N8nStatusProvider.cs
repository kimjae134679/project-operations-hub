using AIControlTower.Models;
using System.Net.Http;
namespace AIControlTower.Services;
public sealed class N8nStatusProvider : CommandStatusProvider
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(3) };
    public N8nStatusProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "n8n";
    public override async Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Client.GetAsync("http://127.0.0.1:5678/healthz", cancellationToken);
            return Status(Id, "n8n local bridge", response.IsSuccessStatusCode ? StatusKind.Running : StatusKind.Error,
                response.IsSuccessStatusCode ? "localhost:5678/healthz 응답 확인. 개별 workflow 완료 여부는 별도 검증합니다." : "healthz가 정상 응답하지 않았습니다.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return Status(Id, "n8n local bridge", StatusKind.Unknown, "healthz 연결 불가. Node 프로세스만으로 Running을 표시하지 않습니다."); }
    }
}
