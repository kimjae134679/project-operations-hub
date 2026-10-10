using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIControlTower.Services;

public sealed record TaskExchangeView(string RecordId, string ProjectId, string ActorId, int Revision,
    string Title, string RequestSummary, string ResponseSummary, string Status, DateTimeOffset UpdatedAt,
    string Markdown, string SearchText)
{
    public string SessionId { get; init; } = "";
    public string RequestSource { get; init; } = "";
    public string ResponseSource { get; init; } = "";
    public string StateLabel => TaskExchangeDocument.StateLabel(Status);
}

/// <summary>A read-only projection; the collected JSON remains the authoritative source.</summary>
public static class TaskExchangeDocument
{
    public static string StateLabel(string status) => status switch
    {
        "pending" => "대기", "in_progress" => "진행 중", "completed" => "완료",
        "blocked" => "막힘", "superseded" => "변경된 요청", _ => "전달 자료"
    };
    public static TaskExchangeView? Parse(string body, string expectedProjectId, out string? warning)
    {
        warning = null;
        if (!body.TrimStart().StartsWith('{')) return null;
        try
        {
            using var json = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = 32 });
            var root = json.RootElement;
            if (!root.TryGetProperty("recordType", out var type) || type.GetString() != "task_exchange") return null;
            if (root.GetProperty("schemaVersion").GetInt32() != 1) throw new FormatException("지원하지 않는 기록 버전");
            ExactKeys(root, "schemaVersion", "recordType", "recordId", "revision", "title", "projectId", "actorId", "sessionId", "receivedAt", "updatedAt", "request", "response", "status", "workDone", "verification", "nextActions", "blockers", "supersedes");
            var recordId = Text(root, "recordId");
            var projectId = Text(root, "projectId");
            var actor = Text(root, "actorId");
            var session = Text(root, "sessionId");
            if (projectId != expectedProjectId) throw new FormatException("수집 프로젝트와 기록 대상이 다름");
            if (!Regex.IsMatch(recordId, @"^[A-Za-z0-9][A-Za-z0-9_-]{0,119}$")) throw new FormatException("기록 식별자 확인 필요");
            var revision = root.GetProperty("revision").GetInt32();
            if (revision < 1) throw new FormatException("기록 버전 확인 필요");
            var title = Text(root, "title");
            var state = Text(root, "status");
            if (!new[] { "pending", "in_progress", "completed", "blocked", "superseded" }.Contains(state))
                throw new FormatException("작업 상태 확인 필요");
            var received = Time(root, "receivedAt");
            var updated = Time(root, "updatedAt");
            if (updated < received) throw new FormatException("기록 시간이 수신 시간보다 빠름");
            var request = root.GetProperty("request");
            var response = root.GetProperty("response");
            ExactKeys(request, "summary", "details", "source");
            ExactKeys(response, "summary", "details", "source");
            var requestSummary = Text(request, "summary");
            var responseSummary = Text(response, "summary");
            var requestDetails = Text(request, "details", true);
            var responseDetails = Text(response, "details", true);
            var requestSource = Text(request, "source");
            var responseSource = Text(response, "source");
            var done = Strings(root, "workDone");
            var next = Strings(root, "nextActions");
            var blockers = Strings(root, "blockers");
            var supersedes = Strings(root, "supersedes");
            if (title.Length > 200 || actor.Length > 200 || session.Length > 200 || !Regex.IsMatch(projectId, @"^[A-Za-z0-9][A-Za-z0-9_-]{0,119}$"))
                throw new FormatException("기록 범위 확인 필요");
            if (state == "completed" && (next.Length > 0 || blockers.Length > 0)) throw new FormatException("남은 작업이 있는 완료 기록");
            if (state is "pending" or "in_progress" or "blocked" && next.Length == 0) throw new FormatException("다음 행동이 없는 진행 기록");
            if (state == "blocked" && blockers.Length == 0) throw new FormatException("막힌 이유가 없는 기록");
            if (supersedes.Any(id => !Regex.IsMatch(id, @"^[A-Za-z0-9][A-Za-z0-9_-]{0,119}$"))) throw new FormatException("대체 기록 확인 필요");
            var builder = new StringBuilder();
            builder.AppendLine("## 받은 요청").AppendLine(requestSummary);
            if (requestDetails.Length > 0) builder.AppendLine().AppendLine(requestDetails);
            builder.AppendLine().AppendLine("## AI가 답한 내용").AppendLine(responseSummary);
            if (responseDetails.Length > 0) builder.AppendLine().AppendLine(responseDetails);
            Section(builder, "실제로 한 일", done, "아직 기록된 작업 없음");
            builder.AppendLine().AppendLine("## 확인 결과");
            var checks = root.GetProperty("verification");
            if (checks.ValueKind != JsonValueKind.Array) throw new FormatException("검증 목록 확인 필요");
            if (checks.GetArrayLength() == 0) builder.AppendLine("검증 기록 없음");
            foreach (var check in checks.EnumerateArray())
            {
                ExactKeys(check, "name", "result", "evidence");
                var name = Text(check, "name");
                var result = Text(check, "result");
                var label = result switch { "pass" => "통과", "fail" => "실패", "not_run" => "미실행", "partial" => "일부 확인", _ => throw new FormatException("검증 상태 확인 필요") };
                var evidence = Strings(check, "evidence");
                if (result is "pass" or "partial" or "fail" && evidence.Length == 0) throw new FormatException("검증 근거 없음");
                builder.AppendLine("- " + name + ": " + label);
                foreach (var path in evidence) builder.AppendLine("  근거: " + path);
            }
            Section(builder, "남은 일", next, state == "completed" ? "기록된 추가 작업 없음" : "다음 행동 기록 없음");
            if (blockers.Length > 0) Section(builder, "막힌 이유", blockers, "");
            builder.AppendLine().AppendLine("## 기록 출처");
            builder.AppendLine("- 작성자: " + actor);
            builder.AppendLine("- 요청 출처: " + requestSource);
            builder.AppendLine("- 답변 출처: " + responseSource);
            builder.AppendLine("- 요청 받은 시각: " + received.ToOffset(TimeSpan.FromHours(9)).ToString("yyyy-MM-dd HH:mm:ss") + " KST");
            builder.AppendLine("- 기록 갱신 시각: " + updated.ToOffset(TimeSpan.FromHours(9)).ToString("yyyy-MM-dd HH:mm:ss") + " KST");
            if (supersedes.Length > 0) builder.AppendLine("- 대체한 기록: " + string.Join(", ", supersedes));
            return new(recordId, projectId, actor, revision, title, requestSummary, responseSummary, state, updated, builder.ToString(), body)
                { SessionId=session, RequestSource=requestSource, ResponseSource=responseSource };
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException or KeyNotFoundException or OverflowException)
        {
            // Unknown JSON stays readable as its original; malformed task records are never labelled complete.
            warning = "기록 형식 확인 필요";
            return null;
        }
    }
    private static void ExactKeys(JsonElement value, params string[] expected)
    {
        if (value.ValueKind != JsonValueKind.Object) throw new FormatException("객체 형식 확인 필요");
        var keys = value.EnumerateObject().Select(p => p.Name).ToArray();
        if (keys.Length != expected.Length || keys.Distinct().Count() != keys.Length || keys.Except(expected).Any())
            throw new FormatException("기록 필드 확인 필요");
    }
    private static string Text(JsonElement value, string name, bool allowEmpty = false)
    {
        var field = value.GetProperty(name);
        if (field.ValueKind != JsonValueKind.String) throw new FormatException(name);
        var text = field.GetString()!;
        if (!allowEmpty && string.IsNullOrWhiteSpace(text)) throw new FormatException(name);
        return text;
    }
    private static DateTimeOffset Time(JsonElement root, string name)
    {
        var text = Text(root, name);
        if (!Regex.IsMatch(text, @"(Z|[+-]\d{2}:\d{2})$") || !DateTimeOffset.TryParse(text, out var time)) throw new FormatException(name);
        return time;
    }
    private static string[] Strings(JsonElement root, string name)
    {
        var items = root.GetProperty(name);
        if (items.ValueKind != JsonValueKind.Array || items.EnumerateArray().Any(v => v.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(v.GetString()))) throw new FormatException(name);
        return items.EnumerateArray().Select(v => v.GetString()!).ToArray();
    }
    private static void Section(StringBuilder builder, string title, string[] lines, string empty)
    {
        builder.AppendLine().AppendLine("## " + title);
        if (lines.Length == 0) builder.AppendLine(empty);
        foreach (var line in lines) builder.AppendLine("- " + line);
    }
}
