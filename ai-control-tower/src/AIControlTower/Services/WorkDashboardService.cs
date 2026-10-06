using System.Text;
using System.Net.Http;
using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Bounded reads of explicitly linked sources only. No model, shell, sync or control calls.</summary>
public sealed class WorkDashboardService
{
    public const string FoundationState = @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\manager-foundation-verify\state.json";
    private readonly string _runs;
    private readonly Func<string, bool> _owns;
    private readonly Func<CancellationToken, Task<PcConnectionSnapshot>>? _bridge;
    private readonly Func<string, CancellationToken, Task<string>>? _bridgeDetail;
    public List<string> ContinuousPaths { get; } = [];
    public List<string> ExchangeRoots { get; } = [];
    public WorkDashboardService(string runs, Func<string, bool>? owns = null,
        Func<CancellationToken, Task<PcConnectionSnapshot>>? bridge = null,
        Func<string, CancellationToken, Task<string>>? bridgeDetail = null)
    { _runs = runs; _owns = owns ?? (_ => false); _bridge = bridge; _bridgeDetail = bridgeDetail; }

    /// <summary>Refresh only explicitly linked/catalogued inboxes; never discover drives or modify records.</summary>
    public void RefreshKnownExchangeRoots(string centralHub, IEnumerable<string> knownProjects, IEnumerable<string> linkedFolders)
    {
        var roots = new List<string>();
        void Add(string parent, params string[] segments)
        {
            if (roots.Count >= 40 || string.IsNullOrWhiteSpace(parent)) return;
            try
            {
                var root = SafePath(parent);
                var path = SafePath(Path.Combine(new[] { root }.Concat(segments).ToArray()), root);
                if (!roots.Contains(path, StringComparer.OrdinalIgnoreCase)) roots.Add(path);
            }
            catch (Exception ex) when (IsReadError(ex) || ex is ArgumentException or NotSupportedException) { }
        }
        Add(centralHub, "04_COMMUNICATION", "project-inbox");
        foreach (var path in knownProjects.Take(40)) Add(path, "_통합소통", "보낼자료");
        foreach (var path in linkedFolders.Take(40)) Add(path, "_통합소통", "보낼자료");
        ExchangeRoots.Clear(); ExchangeRoots.AddRange(roots);
    }

    public async Task<IReadOnlyList<WorkActivity>> ReadAsync(CancellationToken ct)
    {
        // Copy registration on the UI thread before asynchronous I/O.
        var paths = ContinuousPaths.Take(40).ToArray(); var exchanges = ExchangeRoots.Take(40).ToArray();
        var rows = await Task.Run(() =>
        {
            var result = new List<WorkActivity>();
            if (Directory.Exists(_runs))
                foreach (var dir in Directory.EnumerateDirectories(_runs).Take(200))
                {
                    ct.ThrowIfCancellationRequested();
                    var file = Path.Combine(dir, "result.json");
                    try { if (File.Exists(file)) result.Add(ReadLocalJob(file, _owns)); }
                    catch (Exception ex) when (IsReadError(ex)) { result.Add(ReadError(file, "관제탑 로컬 실행", ex)); }
                }
            foreach (var path in paths)
            {
                ct.ThrowIfCancellationRequested();
                try { result.Add(ReadContinuous(path)); }
                catch (Exception ex) when (IsReadError(ex)) { result.Add(ReadError(path, "연속 실행기", ex)); }
            }
            var records = new List<WorkActivity>();
            foreach (var root in exchanges)
            {
                if (!Directory.Exists(root)) continue;
                try
                {
                    SafePath(root);
                    var options = new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 4, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
                    foreach (var file in Directory.EnumerateFiles(root, "*.json", options).Take(500))
                    {
                        ct.ThrowIfCancellationRequested();
                        try { var row = ReadExchange(file); if (row is not null) records.Add(row); }
                        catch (Exception ex) when (IsReadError(ex)) { }
                    }
                }
                catch (Exception ex) when (IsReadError(ex)) { result.Add(ReadError(root, "관제 과정 기록", ex) with { IsManagementRecord = true }); }
            }
            result.AddRange(records.GroupBy(r => r.Id).Select(g => g.OrderByDescending(r => r.Revision).ThenByDescending(r => r.UpdatedAt).First()));
            return result;
        }, ct);
        if (_bridge is not null)
        {
            try
            {
                var snapshot = await _bridge(ct);
                rows.Add(new() { Id = "bridge:connection", Project = "공용 PC", Worker = "ProjectBridge", Source = "로컬 GET 상태",
                    Title = "PC 연결", Status = snapshot.Connected ? "ready" : snapshot.Stage == "stale" ? "stale" : "unknown",
                    Stage = snapshot.Stage, Evidence = SafeText(snapshot.Detail), UpdatedAt = DateTimeOffset.UtcNow, LivenessKnown = snapshot.Connected });
                rows.AddRange(snapshot.Jobs.Select(j => new WorkActivity { Id = "bridge:" + j.Id, BridgeJobId = j.Id,
                    Project = SafeText(j.Project), Worker = SafeText(j.Tool), Source = "ProjectBridge GET",
                    Title = SafeText(j.Title), Status = !snapshot.Connected ? "stale" : j.Action == "start_process" && j.State is "accepted" or "succeeded" or "completed" ? "accepted" : j.State,
                    Stage = SafeText(j.State), Evidence = !snapshot.Connected ? "오래된/미연결 응답 · 현재 실행 미확인" : j.Action=="start_process" ? "시작 접수와 종료 확인을 분리합니다. 요약만으로 자식 완료 미확인 · 상세 조회 필요" : "최근 로컬 응답의 보고 상태 · 실제 결과는 상세 조회",
                    UpdatedAt = DateTimeOffset.UtcNow, LivenessKnown = false }));
            }
            catch (Exception ex) when (IsReadError(ex) || ex is HttpRequestException or TaskCanceledException)
            { ct.ThrowIfCancellationRequested(); rows.Add(ReadError("bridge:connection", "ProjectBridge", ex)); }
        }
        foreach (var worker in new[] { "Jev", "외부 CMD / PowerShell / 콘솔" })
            rows.Add(new() { Id = "unknown:" + worker, Project = "외부 작업", Worker = worker, Source = "작업 연결 없음",
                Title = worker + " 외부 작업", Evidence = "설치·프로세스 존재는 작업 진행이 아닙니다. 등록된 실제 작업/API/소유 기록이 없으므로 미확인입니다." });
        return rows.DistinctBy(r=>r.Id).OrderByDescending(r => r.UpdatedAt).Take(600).ToArray();
    }

    public async Task<string> DetailsAsync(WorkActivity row)
    {
        if (row.BridgeJobId is not null && _bridgeDetail is not null)
            return BridgeDetail(await _bridgeDetail(row.BridgeJobId, CancellationToken.None));
        if (row.DetailPath is null) return row.Evidence;
        return await Task.Run(() =>
        {
            if (row.Source == "연속 실행기 체크포인트")
                return EventTail(Path.Combine(Path.GetDirectoryName(SafePath(row.DetailPath))!, "events.private.jsonl"));
            if (row.Source == "관제탑 소유 실행 기록")
            {
                using var doc = ReadJson(row.DetailPath);
                var path = Text(doc.RootElement, "LogPath");
                if (string.IsNullOrWhiteSpace(path)) return "내부 로그 경로 없음";
                SafePath(path, _runs);
                return Tail(path, false);
            }
            return row.RecentLog + "\n\n검증/다음 단계\n" + row.NextCheckpoint + "\n" + row.Error;
        });
    }

    public static WorkActivity ReadContinuous(string path)
    {
        using var doc = ReadJson(path); var j = doc.RootElement;
        var root = Text(j, "approved_root");
        if (string.IsNullOrWhiteSpace(root)) throw new InvalidDataException("승인 루트 정보 없음");
        SafePath(path, root);
        if (!j.TryGetProperty("steps", out var steps) || steps.ValueKind != JsonValueKind.Object) throw new InvalidDataException("단계 체크포인트 없음");
        var list = steps.EnumerateObject().Take(1000).ToArray();
        var current = list.Where(s => Text(s.Value, "status") == "running").Select(s => s.Name).Take(5).ToArray();
        var pending = list.Where(s => Text(s.Value, "status") == "pending").Select(s => s.Name).Take(5).ToArray();
        var errors = list.Where(s => Text(s.Value, "status") is "failed" or "blocked" or "timed_out" or "output_limit")
            .Select(s => s.Name + ": " + Text(s.Value, "reason", Text(s.Value, "error", Text(s.Value, "status"))));
        return new() { Id = "manager:" + Path.GetFullPath(path), Project = SafeText(Text(j, "project", "미확인")), Worker = "연속 실행기",
            Source = "연속 실행기 체크포인트", Title = Path.GetFullPath(path).Equals(FoundationState, StringComparison.OrdinalIgnoreCase)
                ? "연속실행기 기반 검증(완료 기록)" : SafeText(Text(j, "plan_id", "등록 목록")),
            Status = Text(j, "status", "unknown"), Stage = $"{list.Count(s => Text(s.Value, "status") == "succeeded")}/{list.Length}단계 완료" + (current.Length > 0 ? " · 기록된 단계 " + string.Join(", ", current) : ""),
            Evidence = "마지막 체크포인트 기록 · 갱신 시각은 heartbeat가 아니며 현재 프로세스 생존은 미확인",
            UpdatedAt = Timestamp(j, "updated_at"), NextCheckpoint = pending.Length > 0 ? "대기 단계 후보: " + string.Join(", ", pending) + " · 의존성/실행 가능 여부 미확인" : "추가 대기 단계 기록 없음",
            Error = SafeText(string.Join("\n", errors)), DetailPath = path, LivenessKnown = false };
    }

    public static WorkActivity ReadLocalJob(string path, Func<string, bool> owns)
    {
        using var doc = ReadJson(path); var j = doc.RootElement; var program = Text(j, "ProgramId");
        var owned = owns(program); var state = Text(j, "State", "unknown");
        return new() { Id = "local:" + Text(j, "Id", path), Project = SafeText(Text(j, "ProjectId", "미확인")),
            Worker = program.EndsWith("/jev", StringComparison.OrdinalIgnoreCase) ? "Jev · 관제탑 등록 실행" : "로컬 등록 명령",
            Source = "관제탑 소유 실행 기록", Title = SafeText(Text(j, "Command", "등록 작업")), Status = state,
            Stage = Text(j, "ExitCode") is { Length: > 0 } exit ? "exit " + exit : state,
            UpdatedAt = Timestamp(j, "FinishedAt") ?? Timestamp(j, "StartedAt"),
            Evidence = owned ? "현재 관제탑 JobManager 소유 실행 확인" : "저장된 결과 기록 · 현재 프로세스 생존 미확인",
            LivenessKnown = owned, OwnedProgramId = owned ? program : null, DetailPath = path };
    }

    public static WorkActivity? ReadExchange(string path)
    {
        using var doc = ReadJson(path); var j = doc.RootElement;
        if (Text(j, "recordType") != "task_exchange") return null;
        var parsed = TaskExchangeDocument.Parse(j.GetRawText(), Text(j, "projectId"), out _);
        if (parsed is null) return null;
        var revision = j.TryGetProperty("revision", out var n) && n.TryGetInt32(out var r) ? r : 0;
        var work = ArrayText(j, "workDone"); var next = ArrayText(j, "nextActions");
        var checks = j.TryGetProperty("verification", out var v) && v.ValueKind == JsonValueKind.Array
            ? string.Join("\n", v.EnumerateArray().Take(12).Select(x => Text(x, "result") + " · " + Text(x, "name", Text(x, "summary")))) : "검증 기록 없음";
        return new() { Id = "exchange:" + parsed.ProjectId + ":" + parsed.ActorId + ":" + parsed.RecordId, Project = SafeText(parsed.ProjectId),
            Worker = SafeText(Text(j, "actorId", "기록 담당자")), Source = "명령·답변 task_exchange 기록", Title = SafeText(Text(j, "title")),
            Status = Text(j, "status", "unknown"), Stage = "revision " + revision.ToString("D8"), UpdatedAt = Timestamp(j, "updatedAt"),
            Evidence = "작성자가 남긴 최신 작업 기록 · 자동 채팅 감시/현재 실행 생존 확인이 아님",
            RecentLog = SafeText(work), NextCheckpoint = SafeText(next + "\n검증: " + checks), Error = SafeText(ArrayText(j, "blockers")),
            DetailPath = path, IsManagementRecord = true, Revision = parsed.Revision };
    }

    public static string BridgeDetail(string raw)
    {
        if (raw.Length > 512 * 1024) throw new InvalidDataException("상세 응답 크기 초과");
        using var doc = JsonDocument.Parse(raw); var j = doc.RootElement;
        var metadata=ObjectField(j,"job",j);
        var result=ObjectField(j,"result",j);
        var data=ObjectField(result,"data",ObjectField(result,"result",result));
        // Canonical current process always wins over historical acceptedResult.
        var process=ObjectField(j,"process",ObjectField(result,"process",data));
        var state=Text(metadata,"state",Text(metadata,"status",Text(result,"outcome","unknown")));
        var child=Text(process,"stage",Text(process,"status",Text(process,"state")));
        var start=Text(metadata,"action")=="start_process" || Text(process,"processId").Length>0 || j.TryGetProperty("acceptedResult",out _);
        var terminal=Bool(process,"done")==true && Bool(process,"running")==false && child is "completed" or "succeeded" or "failed" or "cancelled" or "timed_out" or "interrupted";
        var accepted=child=="accepted" || state=="accepted" || start && !terminal;
        var confirmedSuccess=terminal && Bool(process,"succeeded")==true && Text(process,"returnCode",Text(process,"exitCode"))=="0";
        var heading=accepted ? "접수/시작 기록 · 자식 종료/완료 미확인" : terminal ? "자식 종료 확인 · " + (confirmedSuccess ? "완료 기록" : "결과 " + child) : "실제 응답 상태: " + state + (child.Length>0?" · 자식 " + child:"");
        // Only allowlisted metadata. Never return raw args/prompt/output/body/payload.
        return SafeText(heading + "\n종료 코드: " + Text(process, "returnCode", Text(process, "exitCode", Text(j, "returnCode", "미확인")))
            + "\n최근 오류: " + Text(process,"error",Text(j,"error",Text(result,"error","없음/미확인"))));
    }

    public static string SafeText(string text)
    {
        var safe = string.Join("\n", text.Replace("\r", "").Split('\n').Take(30)
            .Select(l => JobManager.SanitizeOutput(l.Length > 1200 ? l[..1200] : l)));
        return safe[..Math.Min(4000, safe.Length)];
    }
    private static string Text(JsonElement j, string key, string fallback = "") => j.ValueKind == JsonValueKind.Object && j.TryGetProperty(key, out var v)
        ? v.ValueKind == JsonValueKind.String ? v.GetString() ?? fallback : v.ValueKind == JsonValueKind.Number ? v.GetRawText() : fallback : fallback;
    private static JsonElement ObjectField(JsonElement j,string key,JsonElement fallback)=>j.ValueKind==JsonValueKind.Object && j.TryGetProperty(key,out var v) && v.ValueKind==JsonValueKind.Object?v:fallback;
    private static bool? Bool(JsonElement j,string key)=>j.ValueKind==JsonValueKind.Object && j.TryGetProperty(key,out var v)?v.ValueKind==JsonValueKind.True?true:v.ValueKind==JsonValueKind.False?false:null:null;
    private static DateTimeOffset? Timestamp(JsonElement j, string key) => DateTimeOffset.TryParse(Text(j, key), out var at) ? at : null;
    private static string ArrayText(JsonElement j, string key) => j.TryGetProperty(key, out var values) && values.ValueKind == JsonValueKind.Array
        ? string.Join("\n", values.EnumerateArray().Take(20).Select(v => v.ValueKind == JsonValueKind.String ? v.GetString() : Text(v, "summary", Text(v, "description", Text(v, "action"))))) : "정보 없음";
    private static JsonDocument ReadJson(string path)
    {
        path = SafePath(path); using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        if (stream.Length > 512 * 1024) throw new InvalidDataException("등록 기록 크기 초과");
        return JsonDocument.Parse(stream);
    }
    public static string SafePath(string path, string? root = null)
    {
        if (!Path.IsPathFullyQualified(path)) throw new InvalidDataException("명시한 절대 경로만 연결할 수 있습니다.");
        var full = Path.GetFullPath(path);
        if (full.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(p => p.Equals("Desktop", StringComparison.OrdinalIgnoreCase) || p == "바탕화면")) throw new InvalidDataException("바탕화면 경로 연결 금지");
        for (var at = full; !string.IsNullOrEmpty(at); at = Path.GetDirectoryName(at) ?? "")
            if ((File.Exists(at) || Directory.Exists(at)) && File.GetAttributes(at).HasFlag(FileAttributes.ReparsePoint)) throw new InvalidDataException("reparse/symlink 경로 연결 금지");
        if (root is not null)
        {
            var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("등록 기록이 승인 루트 밖에 있습니다.");
        }
        return full;
    }
    private static string EventTail(string path) => File.Exists(path) ? Tail(path, true) : "이벤트 기록 없음";
    private static string Tail(string path, bool events)
    {
        SafePath(path); using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var truncated = stream.Length > 32768; if (truncated) stream.Seek(-32768, SeekOrigin.End);
        using var reader = new StreamReader(stream, Encoding.UTF8); var text = reader.ReadToEnd();
        var lines = text.Replace("\r", "").Split('\n').AsEnumerable(); if (truncated) lines = lines.Skip(1);
        return SafeText(string.Join("\n", lines.TakeLast(25).Select(line =>
        {
            if (!events) return line;
            try { using var doc = JsonDocument.Parse(line); var j = doc.RootElement; return Text(j, "time") + " · " + Text(j, "event") + " · " + Text(j, "step") + " · " + Text(j, "status"); }
            catch (JsonException) { return "[해석할 수 없는 이벤트 생략]"; }
        })));
    }
    private static bool IsReadError(Exception ex) => ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException;
    private static WorkActivity ReadError(string path, string worker, Exception ex) => new() { Id = "error:" + path, Project = "연결 확인 필요", Worker = worker,
        Source = "조회 오류", Title = "등록된 기록을 읽을 수 없음", Status = "unknown", Error = SafeText(ex.Message), Evidence = "파일·실행·성공 상태를 추정하지 않습니다." };
}
