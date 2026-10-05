using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIControlTower.Services;

public sealed record MultiplayerCounts(int Online, int Preparing, int Playing);
public sealed record MultiplayerModeCounts(string Key, string Name, int Online, int Preparing, int Playing);
public sealed record MultiplayerPolicy(int LatestBuild, string LatestVersion, int MinimumBuild,
    IReadOnlyList<int> BlockedBuilds, int? Revision);
public sealed record MultiplayerServerSnapshot(bool IsHealthy, string Health, DateTimeOffset? ObservedAt,
    DateTimeOffset FetchedAt, MultiplayerCounts? Total, IReadOnlyList<MultiplayerModeCounts> Modes,
    MultiplayerPolicy? Policy, string PolicyHealth);
public sealed record MultiplayerSummaryResult(bool Written, string? Path, string? Warning);

/// <summary>Reads only the two fixed local HTTP endpoints. Never runs server-management commands.</summary>
public sealed class MultiplayerServerService : IDisposable
{
    public const string RuntimeRoot = @"C:\Users\user\Documents\MultiGod\PhoneLOL_LocalRuntime";
    public const int MaximumResponseBytes = 64 * 1024;
    private const int MaximumSummaryBytes = 1024 * 1024;
    public const string SummaryHeader = "<!-- AIControlTower:PhoneLOL-server-summary:v1 -->";
    private const string SummaryName = "컨트롤타워_서버상태.md";
    private static readonly Uri OnlineEndpoint = new("http://127.0.0.1:29000/phonelol-online/v1");
    private static readonly Uri PolicyEndpoint = new("http://127.0.0.1:29000/phonelol-policy/v1");
    private static readonly (string Key, string Name)[] ModeNames =
    [
        ("ranked", "랭크대전"), ("ranked1v1", "1vs1대전"), ("modeBattle", "모드대전"),
        ("mode", "모드"), ("urf", "URF"), ("urf5", "URF5")
    ];
    private readonly HttpClient _client;
    private readonly bool _ownsClient;
    private readonly SemaphoreSlim _summaryLock = new(1, 1);
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public MultiplayerServerService(HttpClient? client = null)
    {
        _ownsClient = client is null;
        _client = client ?? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false })
        { Timeout = TimeSpan.FromSeconds(3) };
    }

    public async Task<MultiplayerServerSnapshot> FetchAsync(CancellationToken ct = default)
    {
        var onlineTask = FetchOnline(ct);
        var policyTask = FetchPolicy(ct);
        await Task.WhenAll(onlineTask, policyTask).ConfigureAwait(false);
        var online = await onlineTask.ConfigureAwait(false);
        var policy = await policyTask.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        // Online is an observed total. Preparing/playing are subsets, never added to it.
        return new(online.Total is not null, online.Health, online.ObservedAt, DateTimeOffset.UtcNow,
            online.Total, online.Modes, policy.Policy, policy.Health);
    }

    private async Task<(DateTimeOffset? ObservedAt, MultiplayerCounts? Total, IReadOnlyList<MultiplayerModeCounts> Modes, string Health)> FetchOnline(CancellationToken ct)
    {
        try
        {
            using var response = await ReadJson(OnlineEndpoint, ct).ConfigureAwait(false);
            var json = response.RootElement;
            if (json.GetProperty("schemaVersion").GetInt32() != 1) throw new InvalidDataException();
            var observed = Timestamp(json.GetProperty("observedAt").GetString());
            var total = Counts(json.GetProperty("total"));
            var modes = json.GetProperty("modes");
            var parsed = ModeNames.Select(mode =>
            {
                var counts = Counts(modes.GetProperty(mode.Key));
                return new MultiplayerModeCounts(mode.Key, mode.Name, counts.Online, counts.Preparing, counts.Playing);
            }).ToArray();
            return (observed, total, parsed, "서버 응답 확인됨");
        }
        catch (Exception ex) when (ExpectedFailure(ex, ct))
        { return (null, null, [], FailureMessage(ex, "접속 현황")); }
    }

    private async Task<(MultiplayerPolicy? Policy, string Health)> FetchPolicy(CancellationToken ct)
    {
        try
        {
            using var response = await ReadJson(PolicyEndpoint, ct).ConfigureAwait(false);
            var json = response.RootElement;
            // Some deployed policy versions predate the optional schemaVersion field.
            if (json.TryGetProperty("schemaVersion", out var schema) && schema.GetInt32() != 1) throw new InvalidDataException();
            var update = json.GetProperty("update"); var access = json.GetProperty("access");
            var latest = Nonnegative(update.GetProperty("latestBuild"));
            var version = update.GetProperty("latestVersion").GetString();
            if (version is null || version.Length > 64 || !Regex.IsMatch(version, "^[0-9]+(?:\\.[0-9]+){1,3}(?:[-+][0-9A-Za-z.-]+)?$")) throw new InvalidDataException();
            var minimum = Nonnegative(access.GetProperty("minimumBuild"));
            var blocked = access.GetProperty("blockedBuilds").EnumerateArray().Select(Nonnegative).Distinct().Order().ToArray();
            var revision = json.TryGetProperty("revision", out var r) ? Nonnegative(r) : (int?)null;
            return (new(latest, version, minimum, blocked, revision), "접속 정책 응답 확인됨");
        }
        catch (Exception ex) when (ExpectedFailure(ex, ct))
        { return (null, FailureMessage(ex, "접속 정책")); }
    }

    private async Task<JsonDocument> ReadJson(Uri endpoint, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
        // Even an injected client must not silently accept a response redirected away from the fixed endpoint.
        if (response.RequestMessage?.RequestUri is Uri actual && actual != endpoint) throw new InvalidDataException();
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > MaximumResponseBytes) throw new InvalidDataException();
        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
        using var buffer = new MemoryStream(); var chunk = new byte[4096]; int count;
        while ((count = await stream.ReadAsync(chunk, timeout.Token).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + count > MaximumResponseBytes) throw new InvalidDataException();
            buffer.Write(chunk, 0, count);
        }
        var text = StrictUtf8.GetString(buffer.ToArray());
        if (text.StartsWith('\uFEFF')) text = text[1..];
        return JsonDocument.Parse(text);
    }

    private static MultiplayerCounts Counts(JsonElement value) => new(Nonnegative(value.GetProperty("online")),
        Nonnegative(value.GetProperty("preparing")), Nonnegative(value.GetProperty("playing")));
    private static int Nonnegative(JsonElement value)
    {
        if (!value.TryGetInt32(out var number) || number < 0) throw new InvalidDataException();
        return number;
    }
    private static DateTimeOffset Timestamp(string? value)
    {
        if (value is null || !Regex.IsMatch(value, "^[0-9]{4}-[0-9]{2}-[0-9]{2}T.*(?:[zZ]|[+-][0-9]{2}:[0-9]{2})$") ||
            !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) throw new InvalidDataException();
        return parsed;
    }
    private static bool ExpectedFailure(Exception ex, CancellationToken ct) => !ct.IsCancellationRequested &&
        ex is HttpRequestException or IOException or InvalidDataException or JsonException or InvalidOperationException or
            KeyNotFoundException or FormatException or ArgumentException or OperationCanceledException;
    private static string FailureMessage(Exception ex, string name) => ex switch
    {
        OperationCanceledException => name + " 조회 시간이 초과되었습니다. 현재 값은 미확인입니다.",
        HttpRequestException => name + "에 연결하지 못했습니다. 현재 값은 미확인입니다.",
        _ => name + " 응답 형식·크기 검증에 실패했습니다. 현재 값은 미확인입니다."
    };

    public async Task<MultiplayerSummaryResult> WriteSummary(MultiplayerServerSnapshot snapshot, string projectRoot,
        CancellationToken ct = default)
    {
        await _summaryLock.WaitAsync(ct).ConfigureAwait(false);
        try { return await WriteSummaryCore(snapshot, projectRoot, ct).ConfigureAwait(false); }
        finally { _summaryLock.Release(); }
    }

    private static async Task<MultiplayerSummaryResult> WriteSummaryCore(MultiplayerServerSnapshot snapshot, string projectRoot, CancellationToken ct)
    {
        string? temporary = null;
        try
        {
            if (!Path.IsPathFullyQualified(projectRoot) || !Directory.Exists(projectRoot)) throw new InvalidDataException();
            var root = Path.GetFullPath(projectRoot); RejectLinks(root);
            var directory = Path.Combine(root, "_통합소통", "보낼자료"); RejectLinks(directory);
            Directory.CreateDirectory(directory); RejectLinks(directory);
            var destination = Path.Combine(directory, SummaryName); RejectLinks(destination);
            var semantic = JsonSerializer.Serialize(new
            {
                snapshot.IsHealthy, snapshot.Health, snapshot.Total,
                Modes = snapshot.Modes.OrderBy(mode => mode.Key, StringComparer.Ordinal),
                snapshot.Policy, snapshot.PolicyHealth
            });
            var hash = Convert.ToHexStringLower(SHA256.HashData(StrictUtf8.GetBytes(semantic)));
            var marker = "<!-- semantic-sha256: " + hash + " -->";
            var existed = File.Exists(destination); string? existingHash = null;
            if (existed)
            {
                var existing = await ReadExisting(destination, ct).ConfigureAwait(false);
                if (!existing.StartsWith(SummaryHeader + "\n", StringComparison.Ordinal))
                    return new(false, destination, "같은 이름의 기존 파일이 관리 앱 기록이 아니어서 덮어쓰지 않았습니다.");
                if (existing.Contains(marker, StringComparison.Ordinal)) return new(false, destination, null);
                existingHash = Convert.ToHexStringLower(SHA256.HashData(StrictUtf8.GetBytes(existing)));
            }
            var body = RenderSummary(snapshot, marker);
            var bytes = StrictUtf8.GetBytes(body);
            if (bytes.Length > MaximumSummaryBytes) throw new InvalidDataException();
            temporary = Path.Combine(directory, ".server-summary-" + Guid.NewGuid().ToString("N") + ".tmp"); RejectLinks(temporary);
            await using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous))
            {
                await file.WriteAsync(bytes, ct).ConfigureAwait(false);
                await file.FlushAsync(ct).ConfigureAwait(false); file.Flush(true);
            }
            ct.ThrowIfCancellationRequested(); RejectLinks(directory); RejectLinks(destination);
            if (existed)
            {
                var current = await ReadExisting(destination, ct).ConfigureAwait(false);
                if (!current.StartsWith(SummaryHeader + "\n", StringComparison.Ordinal) ||
                    Convert.ToHexStringLower(SHA256.HashData(StrictUtf8.GetBytes(current))) != existingHash)
                    return new(false, destination, "서버 요약 파일이 작성 중 변경되어 덮어쓰지 않았습니다. 다음 갱신에서 재확인합니다.");
            }
            File.Move(temporary, destination, existed); temporary = null;
            return new(true, destination, null);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested && ex is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException)
        { return new(false, null, "서버 요약 기록 경로·권한·크기 검증에 실패했습니다. 기존 파일은 유지했습니다."); }
        finally
        {
            if (temporary is not null)
            {
                try { RejectLinks(temporary); File.Delete(temporary); }
                catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException) { }
            }
        }
    }

    private static string RenderSummary(MultiplayerServerSnapshot snapshot, string marker)
    {
        static string Escape(string value) => value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        var lines = new List<string> { SummaryHeader, marker, "# 멀티의 신 서버 상태", "",
            "인원·정책·연결 상태가 바뀔 때만 요약을 갱신합니다. 아래 시각은 마지막 요약 갱신 때의 조회·관측 시각입니다.", "",
            "조회 시각: " + snapshot.FetchedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            "서버 관측 시각: " + (snapshot.ObservedAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? "미확인"), "",
            "접속 현황: " + Escape(snapshot.Health), "접속 정책: " + Escape(snapshot.PolicyHealth), "" };
        if (snapshot.IsHealthy && snapshot.Total is { } total)
        {
            lines.Add($"전체 접속 {total.Online}명 · 준비 중 {total.Preparing}명 · 플레이 중 {total.Playing}명");
            lines.Add("준비 중·플레이 중은 전체 접속자의 부분집합이며 전체 인원에 더하지 않습니다.");
            lines.AddRange(["", "| 모드 | 접속 | 준비 중 | 플레이 중 |", "|---|---:|---:|---:|"]);
            foreach (var mode in snapshot.Modes) lines.Add($"| {Escape(mode.Name)} | {mode.Online} | {mode.Preparing} | {mode.Playing} |");
        }
        else lines.Add("현재 접속 인원 미확인. 연결 실패를 0명 또는 정상 연결로 표시하지 않습니다.");
        if (snapshot.Policy is { } policy)
        {
            lines.Add(""); lines.Add($"서버 접속 정책의 최신 버전 {Escape(policy.LatestVersion)} · 정책 최신 빌드 {policy.LatestBuild} · 최소 접속 빌드 {policy.MinimumBuild}");
            lines.Add("차단 빌드: " + (policy.BlockedBuilds.Count == 0 ? "없음" : string.Join(", ", policy.BlockedBuilds)));
            lines.Add("정책 버전: " + (policy.Revision?.ToString(CultureInfo.InvariantCulture) ?? "미확인"));
        }
        lines.AddRange(["", "서버 응답의 합계만 읽어 기록했습니다. 플레이어 이름·계정 ID는 수집하지 않습니다. 관리 명령·실행·재시작·AI 확인 기록 작성은 수행하지 않았습니다.", ""]);
        return string.Join("\n", lines);
    }

    private static async Task<string> ReadExisting(string path, CancellationToken ct)
    {
        RejectLinks(path);
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        if (stream.Length > MaximumSummaryBytes) throw new InvalidDataException();
        using var buffer = new MemoryStream(); var chunk = new byte[4096]; int count;
        while ((count = await stream.ReadAsync(chunk, ct).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + count > MaximumSummaryBytes) throw new InvalidDataException();
            buffer.Write(chunk, 0, count);
        }
        var text = StrictUtf8.GetString(buffer.ToArray());
        if (text.StartsWith('\uFEFF')) text = text[1..];
        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private static void RejectLinks(string path)
    {
        for (var current = Path.GetFullPath(path); !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
        {
            try { if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException(); }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
        }
    }
    public void Dispose() { if (_ownsClient) _client.Dispose(); }
}
