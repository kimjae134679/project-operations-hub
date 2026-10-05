using System.Net.Http;
using System.Text.Json;

namespace AIControlTower.Services;

public sealed record MultiplayerPlayer(string Nickname, string Guild, string Status, int? Elapsed)
{
    public string StatusLabel
    {
        get
        {
            if (Status == "main") return "메인 화면";
            var split = Status.LastIndexOf('_');
            var mode = split < 0 ? Status : Status[..split];
            var phase = split < 0 ? "" : Status[(split + 1)..];
            var name = mode switch { "ranked" => "랭크대전", "ranked1v1" => "1대1 대전", "modeBattle" => "모드대전", "mode" => "모드", "urf" => "URF", "urf5" => "URF 5인", _ => "게임" };
            return name + " · " + (phase switch { "waiting" => "대기 중", "playing" => "플레이 중", "spectating" => "관전 중", _ => "상태 확인 중" });
        }
    }
    public string ElapsedLabel => Elapsed is { } seconds ? TimeSpan.FromSeconds(seconds).ToString(seconds >= 3600 ? @"h\:mm\:ss" : @"m\:ss") : "";
    public string GuildLabel => string.IsNullOrEmpty(Guild) ? "" : "[" + Guild + "]";
}
public sealed record MultiplayerRosterSnapshot(bool IsHealthy, string Health, DateTimeOffset FetchedAt, IReadOnlyList<MultiplayerPlayer> Players);

public sealed class MultiplayerRosterService : IDisposable
{
    private readonly HttpClient _client;
    private readonly bool _ownsClient;
    public MultiplayerRosterService(HttpClient? client = null)
    {
        _ownsClient = client is null;
        _client = client ?? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false }) { Timeout = TimeSpan.FromSeconds(2) };
    }
    public async Task<MultiplayerRosterSnapshot> FetchAsync(CancellationToken ct = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));
            using var response = await _client.GetAsync("http://127.0.0.1:29001/roster", HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var input = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
            using var bytes = new MemoryStream();
            var buffer = new byte[4096];
            int read;
            while ((read = await input.ReadAsync(buffer, timeout.Token).ConfigureAwait(false)) > 0)
            {
                if (bytes.Length + read > 256 * 1024) throw new InvalidDataException("roster size");
                bytes.Write(buffer, 0, read);
            }
            using var json = JsonDocument.Parse(bytes.ToArray());
            return new(true, "접속자 목록 확인됨", DateTimeOffset.UtcNow, Parse(json.RootElement));
        }
        catch (Exception ex) when (!ct.IsCancellationRequested && ex is HttpRequestException or IOException or InvalidDataException or JsonException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException or OperationCanceledException)
        { return new(false, "접속자 목록 연결 대기 · 다시 확인 중", DateTimeOffset.UtcNow, []); }
    }
    internal static IReadOnlyList<MultiplayerPlayer> Parse(JsonElement json)
    {
        if (json.GetProperty("schemaVersion").GetInt32() != 1) throw new InvalidDataException("roster schema");
        var observed = DateTimeOffset.Parse(json.GetProperty("observedAt").GetString() ?? throw new InvalidDataException("timestamp"), System.Globalization.CultureInfo.InvariantCulture);
        if (DateTimeOffset.UtcNow - observed > TimeSpan.FromSeconds(10) || observed > DateTimeOffset.UtcNow.AddSeconds(5)) throw new InvalidDataException("stale roster");
        var players = new List<MultiplayerPlayer>();
        foreach (var row in json.GetProperty("rows").EnumerateArray())
        {
            var name = Text(row.GetProperty("nickname"), 64);
            if (name.Length == 0) throw new InvalidDataException("empty nickname");
            var guild = row.TryGetProperty("guild", out var g) ? Text(g, 64) : "";
            var status = Text(row.GetProperty("status"), 64);
            int? elapsed = row.TryGetProperty("elapsed", out var e) && e.ValueKind != JsonValueKind.Null ? e.GetInt32() : null;
            if (elapsed < 0) throw new InvalidDataException("elapsed");
            players.Add(new(name, guild, status, elapsed));
        }
        return players.OrderBy(p => p.Nickname, StringComparer.CurrentCulture).ToArray();
    }
    private static string Text(JsonElement field, int max)
    {
        var text = field.GetString() ?? throw new InvalidDataException("roster text");
        if (text.Length > max || text.Any(char.IsControl)) throw new InvalidDataException("roster text");
        return text;
    }
    public void Dispose() { if (_ownsClient) _client.Dispose(); }
}
