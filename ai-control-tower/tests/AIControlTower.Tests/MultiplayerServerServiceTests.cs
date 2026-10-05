using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class MultiplayerServerServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tower-server-tests-" + Guid.NewGuid().ToString("N"));
    private static readonly string[] ModeKeys = ["ranked", "ranked1v1", "modeBattle", "mode", "urf", "urf5"];
    private string SummaryPath => Path.Combine(_root, "_통합소통", "보낼자료", "컨트롤타워_서버상태.md");
    private static string Online(int online = 10, int preparing = 3, int playing = 4) => JsonSerializer.Serialize(new
    {
        schemaVersion = 1, observedAt = "2026-10-05T09:27:07Z", total = new { online, preparing, playing },
        modes = ModeKeys.ToDictionary(key => key, _ => new { online = 2, preparing = 1, playing = 1 }),
        players = new[] { new { name = "private-player-name", id = "private-player-id" } }
    });
    private const string Policy = """
        {"revision":9,"update":{"latestBuild":241,"latestVersion":"0.22.1"},"access":{"minimumBuild":0,"blockedBuilds":[]},"status":{"title":"private ignored title","body":"private ignored body"}}
        """;
    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => send(request, ct);
    }
    private static HttpResponseMessage Response(string content, HttpStatusCode code = HttpStatusCode.OK) => new(code)
    { Content = new StringContent(content, Encoding.UTF8, "application/json") };
    private static HttpClient Client(string? online = null, string? policy = null) => new(new Handler((request, _) =>
    {
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.StartsWith("http://127.0.0.1:29000/phonelol-", request.RequestUri!.AbsoluteUri);
        return Task.FromResult(Response(request.RequestUri.AbsolutePath.Contains("online") ? online ?? Online() : policy ?? Policy));
    }));

    public MultiplayerServerServiceTests() => Directory.CreateDirectory(_root);

    [Fact]
    public async Task FixedGetEndpointsParseActualObservationAndPolicyWithoutAddingSubsetsOrPlayers()
    {
        using var client = Client(); using var service = new MultiplayerServerService(client);
        var result = await service.FetchAsync();
        Assert.True(result.IsHealthy); Assert.Equal(10, result.Total!.Online); Assert.Equal(3, result.Total.Preparing); Assert.Equal(4, result.Total.Playing);
        Assert.Equal(DateTimeOffset.Parse("2026-10-05T09:27:07Z"), result.ObservedAt); Assert.NotEqual(result.ObservedAt, result.FetchedAt);
        Assert.Equal(ModeKeys, result.Modes.Select(m => m.Key)); Assert.Equal("랭크대전", result.Modes[0].Name); Assert.Equal("URF5", result.Modes[5].Name);
        Assert.Equal(241, result.Policy!.LatestBuild); Assert.Equal("0.22.1", result.Policy.LatestVersion); Assert.Equal(0, result.Policy.MinimumBuild); Assert.Equal(9, result.Policy.Revision);
        Assert.DoesNotContain("private-player", JsonSerializer.Serialize(result)); Assert.DoesNotContain("private ignored", JsonSerializer.Serialize(result));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 0, -1)]
    public async Task NegativeCountersAreUnknownInsteadOfConnectedZero(int online, int preparing, int playing)
    {
        using var client = Client(Online(online, preparing, playing)); using var service = new MultiplayerServerService(client);
        var result = await service.FetchAsync();
        Assert.False(result.IsHealthy); Assert.Null(result.Total); Assert.Null(result.ObservedAt); Assert.Empty(result.Modes); Assert.NotNull(result.Policy);
    }

    [Theory]
    [InlineData("{not-json")]
    [InlineData("{\"schemaVersion\":2}")]
    [InlineData("{\"schemaVersion\":1,\"observedAt\":\"2026-10-05T09:00:00\"}")]
    public async Task MalformedOrUnsupportedResponseFailsClearly(string online)
    {
        using var client = Client(online); using var service = new MultiplayerServerService(client);
        var result = await service.FetchAsync(); Assert.False(result.IsHealthy); Assert.Null(result.Total); Assert.Contains("미확인", result.Health);
    }

    [Fact]
    public async Task FractionalCountersAndMissingModeAreNotAccepted()
    {
        foreach (var online in new[] { Online().Replace("\"online\":10", "\"online\":1.5"), Online().Replace("\"urf5\":", "\"unknown\":") })
        {
            using var client = Client(online); using var service = new MultiplayerServerService(client);
            var result = await service.FetchAsync(); Assert.False(result.IsHealthy); Assert.Null(result.Total);
        }
    }

    [Fact]
    public async Task InvalidPolicyKeepsVerifiedOnlineCountsAndShowsPolicyUnknown()
    {
        using var client = Client(policy: Policy.Replace("\"latestBuild\":241", "\"latestBuild\":-1")); using var service = new MultiplayerServerService(client);
        var result = await service.FetchAsync(); Assert.True(result.IsHealthy); Assert.Equal(10, result.Total!.Online);
        Assert.Null(result.Policy); Assert.Contains("미확인", result.PolicyHealth);
    }

    [Fact]
    public async Task HttpFailureAndOversizedBodyDoNotBecomeSuccessfulZeroCounts()
    {
        foreach (var code in new[] { HttpStatusCode.ServiceUnavailable, HttpStatusCode.Found, HttpStatusCode.OK })
        {
            using var client = new HttpClient(new Handler((request, _) => Task.FromResult(
                request.RequestUri!.AbsolutePath.Contains("online") ? Response(new string('a', MultiplayerServerService.MaximumResponseBytes + 1), code) : Response(Policy))));
            using var service = new MultiplayerServerService(client);
            var result = await service.FetchAsync(); Assert.False(result.IsHealthy); Assert.Null(result.Total);
        }
    }

    [Fact]
    public async Task ResponseRedirectedAwayFromFixedEndpointIsRejected()
    {
        using var client = new HttpClient(new Handler((request, _) =>
        {
            var response = Response(request.RequestUri!.AbsolutePath.Contains("online") ? Online() : Policy);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/redirected");
            return Task.FromResult(response);
        }));
        using var service = new MultiplayerServerService(client);
        var result = await service.FetchAsync(); Assert.False(result.IsHealthy); Assert.Null(result.Policy);
    }

    [Fact]
    public async Task CallerCancellationPropagatesRatherThanWritingFailureSnapshot()
    {
        using var client = new HttpClient(new Handler(async (_, ct) => { await Task.Delay(Timeout.Infinite, ct); return Response("{}"); }));
        using var service = new MultiplayerServerService(client); using var cancel = new CancellationTokenSource(); cancel.CancelAfter(20);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.FetchAsync(cancel.Token));
    }

    [Fact]
    public async Task ThreeSecondTimeoutFailsAsUnknownWithoutUnboundedWaiting()
    {
        using var client = new HttpClient(new Handler(async (_, ct) => { await Task.Delay(Timeout.Infinite, ct); return Response("{}"); }));
        using var service = new MultiplayerServerService(client);
        var result = await service.FetchAsync(); Assert.False(result.IsHealthy); Assert.Null(result.Total); Assert.Contains("초과", result.Health);
    }

    [Fact]
    public async Task SummaryChangesOnlyForSemanticCountsPolicyOrHealthAndContainsNoNames()
    {
        using var client = Client(); using var service = new MultiplayerServerService(client);
        var snapshot = await service.FetchAsync();
        Assert.True((await service.WriteSummary(snapshot, _root)).Written);
        var initial = File.ReadAllText(SummaryPath); File.SetLastWriteTimeUtc(SummaryPath, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var timeOnly = snapshot with { ObservedAt = snapshot.ObservedAt!.Value.AddSeconds(5), FetchedAt = snapshot.FetchedAt.AddSeconds(5) };
        Assert.False((await service.WriteSummary(timeOnly, _root)).Written); Assert.Equal(2020, File.GetLastWriteTimeUtc(SummaryPath).Year); Assert.Equal(initial, File.ReadAllText(SummaryPath));
        Assert.True((await service.WriteSummary(timeOnly with { Total = new(11, 3, 4) }, _root)).Written);
        Assert.Contains("전체 접속 11명", File.ReadAllText(SummaryPath)); Assert.DoesNotContain("private-player", initial);
        Assert.Contains("서버 접속 정책의 최신 버전 0.22.1", initial);
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(SummaryPath)!, "*.tmp"));
    }

    [Fact]
    public async Task ForeignSummaryIsNeverOverwrittenAndFailureDoesNotPublishFakeZero()
    {
        using var client = Client(); using var service = new MultiplayerServerService(client); var snapshot = await service.FetchAsync();
        Directory.CreateDirectory(Path.GetDirectoryName(SummaryPath)!); File.WriteAllText(SummaryPath, "사용자 원본 자료\n");
        var result = await service.WriteSummary(snapshot, _root); Assert.False(result.Written); Assert.NotNull(result.Warning); Assert.Equal("사용자 원본 자료\n", File.ReadAllText(SummaryPath));
        File.Delete(SummaryPath);
        var unknown = snapshot with { IsHealthy = false, Health = "접속 현황 미확인", ObservedAt = null, Total = null, Modes = [] };
        Assert.True((await service.WriteSummary(unknown, _root)).Written);
        Assert.Contains("현재 접속 인원 미확인", File.ReadAllText(SummaryPath)); Assert.DoesNotContain("전체 접속 0명", File.ReadAllText(SummaryPath));
    }

    [Fact]
    public async Task ReparseAncestorRejectsSummaryWritingOutsideProject()
    {
        using var client = Client(); using var service = new MultiplayerServerService(client); var snapshot = await service.FetchAsync();
        var outside = Path.Combine(_root, "outside"); Directory.CreateDirectory(outside);
        try { Directory.CreateSymbolicLink(Path.Combine(_root, "_통합소통"), outside); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException) { return; }
        var result = await service.WriteSummary(snapshot, _root); Assert.False(result.Written); Assert.NotNull(result.Warning);
        Assert.False(File.Exists(Path.Combine(outside, "보낼자료", "컨트롤타워_서버상태.md")));
    }

    [Fact]
    public async Task DisposingServiceDoesNotDisposeInjectedClient()
    {
        using var client = Client(); var service = new MultiplayerServerService(client); service.Dispose();
        using var response = await client.GetAsync("http://127.0.0.1:29000/phonelol-online/v1"); Assert.True(response.IsSuccessStatusCode);
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
