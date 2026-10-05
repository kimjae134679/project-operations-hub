using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class MultiplayerRosterServiceTests
{
    private sealed class Handler(string body, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal("http://127.0.0.1:29001/roster", request.RequestUri!.AbsoluteUri);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Empty(request.Headers);
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }
    private static string Body(object[] rows, DateTimeOffset? at = null) => JsonSerializer.Serialize(new { schemaVersion = 1, observedAt = at ?? DateTimeOffset.UtcNow, rows });
    private static async Task<MultiplayerRosterSnapshot> Fetch(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        using var client = new HttpClient(new Handler(body, status));
        using var service = new MultiplayerRosterService(client);
        return await service.FetchAsync();
    }
    [Fact]
    public async Task ReadsNicknamesAndActualPlayingOrSpectatorState()
    {
        var result = await Fetch(Body([
            new { nickname = "테스트가", guild = "길드", status = "ranked_spectating", elapsed = (int?)125 },
            new { nickname = "테스트나", guild = "", status = "main", elapsed = (int?)null },
            new { nickname = "테스트다", guild = "", status = "urf_playing", elapsed = (int?)3605 }
        ]));
        Assert.True(result.IsHealthy);
        Assert.Equal(3, result.Players.Count);
        Assert.Contains(result.Players, p => p.Nickname == "테스트가" && p.StatusLabel == "랭크대전 · 관전 중" && p.ElapsedLabel == "2:05");
        Assert.Contains(result.Players, p => p.StatusLabel == "메인 화면" && p.ElapsedLabel == "");
        Assert.Contains(result.Players, p => p.ElapsedLabel == "1:00:05");
    }
    [Fact]
    public async Task ValidEmptyRosterIsDifferentFromUnavailable()
    {
        var empty = await Fetch(Body([]));
        var failed = await Fetch("", HttpStatusCode.ServiceUnavailable);
        Assert.True(empty.IsHealthy); Assert.Empty(empty.Players);
        Assert.False(failed.IsHealthy); Assert.Empty(failed.Players);
    }
    [Fact]
    public async Task RejectsStaleRosterRatherThanShowingOldNamesAsLive()
    {
        var result = await Fetch(Body([new { nickname = "테스트", status = "main" }], DateTimeOffset.UtcNow.AddMinutes(-1)));
        Assert.False(result.IsHealthy);
    }
    [Theory]
    [InlineData("{not-json}")]
    [InlineData("{\"schemaVersion\":2}")]
    public async Task RejectsInvalidResponses(string body) => Assert.False((await Fetch(body)).IsHealthy);
    [Fact]
    public async Task RejectsControlCharactersInNickname()
    {
        var result = await Fetch(Body([new { nickname = "테스트\n닉", status = "main" }]));
        Assert.False(result.IsHealthy);
    }
}
