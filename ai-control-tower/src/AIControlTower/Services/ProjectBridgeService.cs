using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AIControlTower.Services;

public sealed record PcJobSnapshot(string Id, string Title, string Project, string Tool, string State, string Action = "");
public sealed record PcConnectionSnapshot(bool Installed, bool Connected, bool RelayConnected, string Device,
    string Stage, string Detail, string RelayCode, int ActiveJobs, int ParallelLimit, IReadOnlyList<PcJobSnapshot> Jobs)
{
    public string StatusReasonCode { get; init; } = "unknown";
    public int? PublicationConflictCount { get; init; }
    public int? PendingPublicationCount { get; init; }
}

// Only allowlisted metadata may be persisted. Never add raw errors, body, endpoint or device data.
public sealed record PcConnectionDiagnostics(string Stage = "unknown", string ReasonCode = "unknown",
    DateTimeOffset? AttemptStartedAtUtc = null, DateTimeOffset? AttemptCompletedAtUtc = null,
    long ElapsedMilliseconds = 0, bool InFlight = false, DateTimeOffset? NextRetryAtUtc = null,
    DateTimeOffset? LastSuccessAtUtc = null)
{
    internal static string SafeStage(string stage) => stage switch
    {
        "connected" or "running" or "stopped" or "disconnected" or "paused" or "offline" or "stale" or
        "permission_denied" or "error" or "cancelled" or "not_installed" or "upgrade_required" => stage,
        _ => "unknown"
    };
    internal static string SafeReason(string reason) => reason switch
    {
        "status_confirmed" or "stale_status" or "invalid_status_timestamp" or "timeout" or "permission_denied" or
        "endpoint_unavailable" or "http_error" or "transport_error" or "invalid_json" or "invalid_status" or
        "invalid_metadata" or "io_error" or "cancelled" or "unexpected_error" => reason,
        _ => "unknown"
    };
}

public sealed class ProjectBridgeService : IDisposable
{
    public const string DefaultHome = @"D:\A_KJ\AI\Applications\ProjectBridge";
    private readonly HttpClient _client;
    private readonly Action<ProcessStartInfo> _startBridge;
    public string Home { get; }
    public string Executable => Path.Combine(Home, "프로젝트연결.exe");
    public ProjectBridgeService(string? home = null, HttpClient? client = null)
        : this(home,client,null) { }
    public ProjectBridgeService(string? home,HttpClient? client,Action<ProcessStartInfo>? startBridge)
    { Home = home ?? DefaultHome; _client = client ?? new HttpClient(new HttpClientHandler{AllowAutoRedirect=false,UseProxy=false}) { Timeout = TimeSpan.FromSeconds(3) }; _startBridge=startBridge??(info=>Process.Start(info)?.Dispose()); }
    internal static string Text(JsonElement j, string key, string fallback = "") => j.ValueKind == JsonValueKind.Object && j.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? fallback : fallback;
    private static bool Flag(JsonElement j, string key) => j.ValueKind == JsonValueKind.Object && j.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.True;
    private static int Number(JsonElement j, string key, int fallback) => j.ValueKind == JsonValueKind.Object && j.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n) ? n : fallback;
    public static Uri ValidateEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != "http" || uri.Host != "127.0.0.1" ||
            uri.Port <= 0 || uri.UserInfo.Length > 0 || uri.AbsolutePath != "/" || uri.Query.Length > 0 || uri.Fragment.Length > 0)
            throw new InvalidDataException("로컬 연결 주소를 확인하세요.");
        return uri;
    }
    public static string CanonicalProjectId(string id) => id switch
    {
        "project-operations-hub" => "Control-Tower",
        "audiobook" => "Mushoku-Audiobook",
        "phonelol-current" => "PhoneLOL",
        "stock-planned" => "Investment-Lab",
        "housing-planned" => "ChungYack",
        _ => id
    };
    private (Uri Address, string Token) Endpoint()
    {
        var path=Path.Combine(Home,"state","local_endpoint.json");
        for(var item=path;!string.IsNullOrEmpty(item);item=Path.GetDirectoryName(item))
            if((File.Exists(item)||Directory.Exists(item))&&(File.GetAttributes(item)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("로컬 연결 메타데이터 경로를 확인하세요.");
        using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        if(stream.Length>64*1024)throw new InvalidDataException("로컬 연결 메타데이터가 너무 큽니다.");
        using var doc = JsonDocument.Parse(stream,new JsonDocumentOptions{MaxDepth=16});
        var row = doc.RootElement; var address = ValidateEndpoint(Text(row, "baseUrl")); var token = Text(row, "token");
        if (token.Length < 20 || token.Length > 200 || token.Any(char.IsControl)) throw new InvalidDataException("로컬 연결 인증을 확인하세요.");
        return (address, token);
    }
    private async Task<JsonDocument> RequestAsync(HttpMethod method, string path, object? value, CancellationToken ct)
    {
        var ep = Endpoint(); using var request = new HttpRequestMessage(method, new Uri(ep.Address, path));
        // ResponseHeadersRead ends HttpClient.Timeout at headers. Bound GET body reads too;
        // POST/control and their existing fallback/uncertain-result semantics are unchanged.
        using var deadline=method==HttpMethod.Get?CancellationTokenSource.CreateLinkedTokenSource(ct):null;
        if(deadline is not null && _client.Timeout!=Timeout.InfiniteTimeSpan)deadline.CancelAfter(_client.Timeout);
        var requestToken=deadline?.Token??ct;
        request.Headers.Add("X-ProjectBridge-Token", ep.Token);
        if (value is not null) request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, requestToken);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException(response.StatusCode is System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized?"PC 로컬 API 권한 거부":"PC 연결 요청 실패",null,response.StatusCode);
        using var input=await response.Content.ReadAsStreamAsync(requestToken);using var output=new MemoryStream();var buffer=new byte[8192];
        while(true){var count=await input.ReadAsync(buffer,requestToken);if(count==0)break;if(output.Length+count>900*1024)throw new InvalidDataException("결과가 너무 큽니다. 읽을 범위를 줄이세요.");output.Write(buffer,0,count);}
        return JsonDocument.Parse(output.ToArray(),new JsonDocumentOptions{MaxDepth=32});
    }
    /// <summary>Existing authenticated API only: no config read, installer, native fallback or process start.</summary>
    public async Task<PcConnectionSnapshot> CheckApiOnlyAsync(CancellationToken ct)
    {
        var installed=File.Exists(Executable);
        try
        {
            using var doc=await RequestAsync(HttpMethod.Get,"v1/status",null,ct);var j=doc.RootElement;
            var now=DateTimeOffset.UtcNow;
            var parsed=DateTimeOffset.TryParse(Text(j,"updatedAt"),out var at);
            var fresh=parsed&&now-at<TimeSpan.FromSeconds(30)&&at<now.AddMinutes(1);
            var stage=Text(j,"stage","unknown");var jobs=new List<PcJobSnapshot>();
            if(j.TryGetProperty("jobs",out var rows)&&rows.ValueKind==JsonValueKind.Array)
                foreach(var row in rows.EnumerateArray().Take(200))jobs.Add(new(Text(row,"id"),Text(row,"title",ActionName(Text(row,"action"))),Text(row,"projectId"),Text(row,"toolId","ProjectBridge"),Text(row,"state","unknown"),Text(row,"action")));
            return new(installed,fresh&&Flag(j,"localReady")&&stage is not("stopped" or "disconnected" or "paused"),fresh&&Flag(j,"relayConnected"),Text(j,"deviceId","기기 미확인"),fresh?stage:"stale",
                fresh?"기존 인증 API 응답 확인 · 연결 상태와 작업을 조회합니다. 설정/설치/원격 프로세스는 변경하지 않습니다.":"API 응답 시각이 오래되었습니다. 제한된 간격으로 상태만 다시 확인합니다.",Text(j,"relayCode"),Number(j,"activeJobCount",0),Math.Clamp(Number(j,"parallelLimit",4),1,8),jobs)
                {StatusReasonCode=fresh?"status_confirmed":parsed?"stale_status":"invalid_status_timestamp",
                    PublicationConflictCount=fresh?PublicationCount(j,"publicationConflictCount"):null,
                    PendingPublicationCount=fresh?PublicationCount(j,"pendingPublicationCount"):null};
        }
        catch(OperationCanceledException){ct.ThrowIfCancellationRequested();return new(installed,false,false,"기기 미확인","offline","PC 상태 조회 시간 초과 · 기존 연결 실행기에 복구를 맡기고 상태만 다시 확인합니다.","",0,4,[]) {StatusReasonCode="timeout"};}
        catch(Exception e) when(e is IOException or HttpRequestException or JsonException or UnauthorizedAccessException or InvalidOperationException)
        {
            var denied=e is UnauthorizedAccessException||e is HttpRequestException{StatusCode:System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized};
            return new(installed,false,false,"기기 미확인",denied?"permission_denied":"offline",denied?"PC 로컬 API 권한 거부 · 자동 재시도/복구 중지 · 인증은 별도로 확인해야 합니다.":"기존 PC API 응답 없음 · 설치/설정 변경이나 새 실행기 시작 없이 제한된 간격으로 조회합니다.","",0,4,[])
                {StatusReasonCode=denied?"permission_denied":e switch
                {
                    FileNotFoundException or DirectoryNotFoundException => "endpoint_unavailable",
                    HttpRequestException{StatusCode:not null} => "http_error",
                    HttpRequestException => "transport_error",
                    JsonException => "invalid_json",
                    InvalidDataException => "invalid_metadata",
                    IOException => "io_error",
                    _ => "invalid_status"
                }};
        }
    }
    public async Task ControlApiOnlyAsync(string action,CancellationToken ct)
    {
        if(action is not("resume" or "pause" or "stop"))throw new ArgumentException("알 수 없는 연결 동작");
        using var response=await RequestAsync(HttpMethod.Post,"v1/control",new{action},ct);
        if(!Flag(response.RootElement,"accepted"))throw new IOException("PC 연결 제어 승인 응답이 없습니다.");
    }
    /// <summary>Explicit user resume only; no installer, configuration, authentication or elevation changes.</summary>
    public async Task ResumeExistingAsync(CancellationToken ct)
    {
        try {await ControlApiOnlyAsync("resume",ct);return;}
        catch(Exception e) when((e is FileNotFoundException or DirectoryNotFoundException)&&!File.Exists(Path.Combine(Home,"state","local_endpoint.json")) || e is HttpRequestException{StatusCode:null} || e is TaskCanceledException && !ct.IsCancellationRequested) { }
        ct.ThrowIfCancellationRequested();
        for(var item=Executable;!string.IsNullOrEmpty(item);item=Path.GetDirectoryName(item))
            if((File.Exists(item)||Directory.Exists(item))&&(File.GetAttributes(item)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("PC 연결 실행 경로를 확인하세요.");
        if(!File.Exists(Executable))throw new FileNotFoundException("PC 연결 프로그램이 없습니다. 설치·복구를 눌러 주세요.");
        var info=new ProcessStartInfo(Executable){WorkingDirectory=Home,UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden};
        info.ArgumentList.Add("--resume");info.ArgumentList.Add("--background");_startBridge(info);
    }
    public async Task<PcConnectionSnapshot> CheckAsync(CancellationToken ct)
    {
        var installed = File.Exists(Executable); string device = "기기 확인 대기";
        try
        {
            using var config = JsonDocument.Parse(File.ReadAllText(Path.Combine(Home, "config.json")));
            device = Text(config.RootElement, "deviceId", device);
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException) { }
        if (!installed) return new(false, false, false, device, "not_installed", "공용 PC 연결을 설치하면 여러 도구가 함께 사용할 수 있습니다.", "", 0, 4, []);
        try
        {
            using var doc = await RequestAsync(HttpMethod.Get, "v1/status", null, ct); var j = doc.RootElement;
            var fresh = DateTimeOffset.TryParse(Text(j, "updatedAt"), out var at) && DateTimeOffset.UtcNow - at < TimeSpan.FromSeconds(30) && at < DateTimeOffset.UtcNow.AddMinutes(1);
            var stage = Text(j, "stage", "unknown");
            var connected = fresh && Flag(j, "localReady") && stage is not ("stopped" or "disconnected" or "paused");
            var jobs = new List<PcJobSnapshot>();
            if (j.ValueKind == JsonValueKind.Object && j.TryGetProperty("jobs", out var rows) && rows.ValueKind == JsonValueKind.Array)
                foreach (var r in rows.EnumerateArray().Take(200)) jobs.Add(new(Text(r, "id"), Text(r, "title", ActionName(Text(r, "action"))), Text(r, "projectId"), Text(r, "toolId", "ProjectBridge"), Text(r, "state", "unknown"), Text(r,"action")));
            return new(true, connected, fresh && Flag(j, "relayConnected"), Text(j, "deviceId", device), fresh ? stage : "stale",
                fresh ? "파일·명령은 병렬 처리하고 화면 조작과 같은 파일 수정은 순서대로 처리합니다." : "최근 응답이 없습니다. 연결을 다시 시작하세요.",
                Text(j, "relayCode"), Number(j, "activeJobCount", 0), Math.Clamp(Number(j, "parallelLimit", 4), 1, 8), jobs)
                {PublicationConflictCount=fresh?PublicationCount(j,"publicationConflictCount"):null,
                    PendingPublicationCount=fresh?PublicationCount(j,"pendingPublicationCount"):null};
        }
        catch (Exception e) when (e is IOException or HttpRequestException or JsonException or UnauthorizedAccessException or TaskCanceledException)
        {
            ct.ThrowIfCancellationRequested();
            // A legacy relay heartbeat proves relay connectivity, never v3 API availability.
            try
            {
                using var old = JsonDocument.Parse(File.ReadAllText(Path.Combine(Home, "state", "universal_status.json"))); var j = old.RootElement;
                var fresh = DateTimeOffset.TryParse(Text(j, "updatedAt"), out var at) && DateTimeOffset.UtcNow - at < TimeSpan.FromSeconds(30);
                var relay = fresh && Flag(j, "privateChannelAvailable") && Text(j, "stage") is "connected" or "running";
                return new(true, false, relay, Text(j, "device", device), "upgrade_required", "설치된 2.0 연결을 공용 병렬 작업 서비스로 업데이트하세요.", Text(j, "code"), 0, 1, []);
            }
            catch (Exception x) when (x is IOException or JsonException or UnauthorizedAccessException) { }
            return new(true, false, false, device, "offline", "PC 연결의 최근 응답을 확인할 수 없습니다. 연결 또는 설치·복구를 누르세요.", "", 0, 4, []);
        }
    }
    public async Task ControlAsync(string action, CancellationToken ct)
    {
        if (action is not ("resume" or "pause" or "stop")) throw new ArgumentException("알 수 없는 연결 동작");
        try { using var response = await RequestAsync(HttpMethod.Post, "v1/control", new { action }, ct); return; }
        catch (Exception e) when (e is IOException or HttpRequestException or JsonException or TaskCanceledException) { ct.ThrowIfCancellationRequested(); }
        if (!File.Exists(Executable)) throw new FileNotFoundException("PC 연결을 먼저 설치하세요.");
        var info = new ProcessStartInfo(Executable) { WorkingDirectory = Home, UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
        info.ArgumentList.Add("--" + action); info.ArgumentList.Add("--background"); Process.Start(info)?.Dispose();
    }
    public async Task<string> SubmitAsync(string project, string tool, string action, object args, CancellationToken ct)
        => await SubmitCoreAsync(project,tool,action,args,ct,false);
    public async Task<string> SubmitApiOnlyAsync(string project,string tool,string action,object args,CancellationToken ct)
        => await SubmitCoreAsync(project,tool,action,args,ct,true);
    private async Task<string> SubmitCoreAsync(string project,string tool,string action,object args,CancellationToken ct,bool apiOnly)
    {
        var state = apiOnly?await CheckApiOnlyAsync(ct):await CheckAsync(ct); if (!state.Connected) throw new InvalidOperationException("PC 연결이 준비되지 않았습니다.");
        var id = "tower-" + Guid.NewGuid().ToString("N"); var now = DateTimeOffset.UtcNow;
        using var doc = await RequestAsync(HttpMethod.Post, "v1/jobs", new { id, target = "PC", deviceId = state.Device, projectId = CanonicalProjectId(project),
            toolId = tool, action, createdAt = now.ToString("o"), expiresAt = now.AddDays(1).ToString("o"), args }, ct);
        return id;
    }
    public async Task<string> ResultAsync(string id, CancellationToken ct)
    {
        if (id.Length > 80 || id.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_')) throw new ArgumentException("작업 ID를 확인하세요.");
        using var doc = await RequestAsync(HttpMethod.Get, "v1/jobs/" + id, null, ct);
        return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }
    public async Task InstallAsync(CancellationToken ct)
    {
        var stage = Path.Combine(ControlTowerSettings.DataDirectory, "install-stage", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(stage);
        try
        {
            var assembly = typeof(ProjectBridgeService).Assembly;
            foreach (var name in assembly.GetManifestResourceNames().Where(n => n.StartsWith("ProjectBridge/", StringComparison.Ordinal)))
            {
                var file = name["ProjectBridge/".Length..].Replace('\\','/');
                var destination = Path.GetFullPath(Path.Combine(stage,file));
                if (!destination.StartsWith(Path.GetFullPath(stage) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("설치 자료 이름 오류");
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                using var source = assembly.GetManifestResourceStream(name)!; using var target = File.Create(destination); await source.CopyToAsync(target, ct);
            }
            var script = Path.Combine(stage, "install.ps1"); if (!File.Exists(script)) throw new FileNotFoundException("이 버전에 PC 연결 설치 자료가 없습니다.");
            var info = new ProcessStartInfo("powershell.exe") { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var a in new[] { "-NoProfile", "-NonInteractive", "-WindowStyle", "Hidden", "-ExecutionPolicy", "Bypass", "-File", script, "-SourceDirectory", stage, "-StartAtLogin" }) info.ArgumentList.Add(a);
            using var process = Process.Start(info) ?? throw new IOException("설치를 시작하지 못했습니다.");
            // Once installation starts, its staged files must stay available until it exits.
            var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync(); await process.WaitForExitAsync();
            var output = ProcessRunner.Sanitize((await stdout) + "\n" + (await stderr));
            Directory.CreateDirectory(ControlTowerSettings.DataDirectory); await File.WriteAllTextAsync(Path.Combine(ControlTowerSettings.DataDirectory, "projectbridge-install.log"), output);
            if (process.ExitCode != 0) throw new IOException(InstallationFailureMessage(output));
        }
        finally { if (Directory.Exists(stage)) Directory.Delete(stage, true); }
    }
    // Never surface installer stdout/stderr in the GUI; recognize only fixed reason codes.
    public static string InstallationFailureMessage(string output)
    {
        if(output.Contains("config_not_writable_before_install",StringComparison.Ordinal))return "설정 파일에 필요한 쓰기 권한이 없어 설치를 보류했습니다. 권한·인증·기존 연결은 강제로 변경하지 않았습니다. (config_not_writable_before_install)";
        if(output.Contains("config_changed_since_",StringComparison.Ordinal)||output.Contains("config_staged_content_changed",StringComparison.Ordinal))return "설정이 다른 작업에서 변경되어 설치를 보류했습니다. 기존 기록과 백업을 확인하세요. (config_changed)";
        if(output.Contains("hash mismatch",StringComparison.OrdinalIgnoreCase)||output.Contains("Unsupported release manifest",StringComparison.Ordinal))return "설치 자료의 무결성 검증이 실패하여 교체를 보류했습니다. (release_validation_failed)";
        if(output.Contains("Bridge release tests failed",StringComparison.Ordinal)||output.Contains("Native bridge compile failed",StringComparison.Ordinal))return "설치 자료의 빌드·검사가 실패하여 교체를 보류했습니다. (release_checks_failed)";
        return "PC 연결 설치가 완료되지 않았습니다. 설치 기록과 보존된 백업을 확인하세요. (installation_failed)";
    }
    public static string ActionName(string action) => action switch { "capabilities" => "PC 기능 확인", "list_dir" => "폴더 확인", "read_file" => "파일 읽기", "write_file" => "파일 수정", "run_command" => "명령 실행", "start_process" => "장기 작업", "process_status" => "진행 확인", "stop_process" => "작업 중지", "ui_control" => "화면 조작", _ => "PC 작업" };
    private static int? PublicationCount(JsonElement row,string name)
        =>row.ValueKind==JsonValueKind.Object&&row.TryGetProperty(name,out var value)&&value.ValueKind==JsonValueKind.Number&&value.TryGetInt32(out var count)&&count>=0?count:null;
    public void Dispose() => _client.Dispose();
}
