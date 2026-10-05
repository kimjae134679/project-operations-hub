using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class DeliveryChainProvider : CommandStatusProvider
{
    public DeliveryChainProvider(ProcessRunner runner) : base(runner) { }
    public override string Id => "delivery-chain";
    public override Task<ToolStatus> CheckAsync(CancellationToken cancellationToken) => Task.FromResult(Status(Id, "GPT → Jev 전달", StatusKind.Unknown, "외부 GPT 세션의 전달 여부는 로컬에서 확인할 수 없습니다."));
}
