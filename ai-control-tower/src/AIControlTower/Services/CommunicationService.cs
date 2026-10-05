using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Local file transport only. Delivery never generates a read/application receipt.</summary>
public sealed class CommunicationService
{
    public const int MaximumFileBytes = 1024 * 1024;
    public const int MaximumFilesPerFolder = 300;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly TimeSpan _settleTime;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };
    private static readonly Regex SafeId = new("^[A-Za-z0-9][A-Za-z0-9_.-]{0,127}$", RegexOptions.CultureInvariant);
    private static readonly Regex Digest = new("^[0-9a-f]{64}$", RegexOptions.CultureInvariant);
    private static readonly Regex SecretPattern = new(
        "-----BEGIN (?:[A-Z ]+ )?PRIVATE KEY-----|\\b(?:sk-[A-Za-z0-9_-]{16,}|gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|AKIA[A-Z0-9]{16})\\b|(?:api[_-]?key|access[_-]?token|refresh[_-]?token|password|passwd|token|secret|client[_-]?secret|private[_-]?key|authorization)\\s*[\\\"']?\\s*[:=]\\s*[\\\"']?[^\\s\\\"',;}{]{1,}|Bearer\\s+[A-Za-z0-9._~+/-]{12,}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(200));

    public CommunicationService(TimeSpan? settleTime = null)
    {
        _settleTime = settleTime ?? TimeSpan.FromMilliseconds(300);
        if (_settleTime < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(settleTime));
    }
    public Task<CommunicationSnapshot> SyncAsync(string hubRoot, IEnumerable<CommunicationTarget> targets,
        CancellationToken ct = default) => Sync(hubRoot, targets, ct);

    public async Task<CommunicationSnapshot> Sync(string hubRoot, IEnumerable<CommunicationTarget> targets,
        CancellationToken ct = default)
    {
        await _syncLock.WaitAsync(ct).ConfigureAwait(false);
        try { return await SyncCore(hubRoot, targets.ToArray(), ct).ConfigureAwait(false); }
        finally { _syncLock.Release(); }
    }

    public static string NormalizeText(byte[] data)
    {
        var text = StrictUtf8.GetString(data);
        if (text.StartsWith('\uFEFF')) text = text[1..];
        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }
    public static string ContentHash(string text) => Convert.ToHexStringLower(SHA256.HashData(StrictUtf8.GetBytes(text)));
    // Python json.dumps([actor, session], ensure_ascii=False), including its comma-space separator.
    public static string ReceiptFileName(string actor, string session)
    {
        ValidateIdentity(actor); ValidateIdentity(session);
        static string Quote(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        return ContentHash("[" + Quote(actor) + ", " + Quote(session) + "]") + ".json";
    }

    private async Task<CommunicationSnapshot> SyncCore(string hubRoot, CommunicationTarget[] targets, CancellationToken ct)
    {
        var errors = new List<CommunicationIssue>();
        var notices = new List<CommunicationNotice>();
        var receipts = new List<CommunicationReceiptItem>();
        var states = new List<CommunicationProjectState>();
        var items = new List<CommunicationInboxItem>();
        var publish = new HashSet<string>(StringComparer.Ordinal);
        string root;
        Board board;
        try
        {
            root = ExistingRoot(hubRoot);
            board = await LoadBoard(root, ct).ConfigureAwait(false);
            notices.AddRange(board.Notices.Values.Where(n => n.Active).Select(n => n.Notice));
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            errors.Add(new(null, "manifest_invalid", "공지 목록 또는 본문 검증 실패. 배포·수집하지 않았습니다."));
            return new(notices, receipts, states, items, errors, DateTimeOffset.UtcNow, []);
        }
        // A duplicate ID/root is ambiguous: reject every duplicate instead of selecting a guessed owner.
        var duplicateIds = targets.GroupBy(t => t.ProjectId, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        static string CanonicalRoot(string value)
        {
            try { return Path.GetFullPath(value).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar); }
            catch (Exception ex) when (IsFileError(ex)) { return value; }
        }
        var duplicateRoots = targets.Where(t => !string.IsNullOrWhiteSpace(t.RootPath)).GroupBy(t => CanonicalRoot(t.RootPath), PathComparer).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(PathComparer);
        foreach (var target in targets)
        {
            ct.ThrowIfCancellationRequested();
            var count = 0; var delivered = 0; var mailbox = "";
            var readFiles = 0;
            var before = errors.Count;
            try
            {
                ValidateId(target.ProjectId);
                if (!board.Projects.Contains(target.ProjectId) || duplicateIds.Contains(target.ProjectId) || duplicateRoots.Contains(CanonicalRoot(target.RootPath)))
                    throw new InvalidDataException("Unregistered or duplicate project.");
                var projectRoot = ExistingRoot(target.RootPath);
                mailbox = Inside(projectRoot, "_통합소통");
                var received = EnsureDirectory(projectRoot, "_통합소통/받은공지");
                var outbox = EnsureDirectory(projectRoot, "_통합소통/보낼자료");
                var localReceipts = EnsureDirectory(projectRoot, "_통합소통/확인기록");
                var importLedgerPath = Inside(mailbox, ".communication-imports.json");
                var imports = new Dictionary<string, string>(StringComparer.Ordinal);
                if (File.Exists(importLedgerPath))
                {
                    try { imports = JsonSerializer.Deserialize<Dictionary<string, string>>(await ReadText(importLedgerPath, ct).ConfigureAwait(false), JsonOptions) ?? imports; }
                    catch (Exception ex) when (IsFileError(ex)) { Hold(errors, target.ProjectId, "receipt_retry"); }
                }
                var current = board.Notices.Values.Where(n => Applicable(n, target.ProjectId)).ToArray();
                // Board snapshot must still be coherent immediately before replacing the local delivery marker.
                foreach (var n in current)
                {
                    var fresh = await ReadText(Inside(root, "04_COMMUNICATION/announcements/" + n.SourcePath), ct).ConfigureAwait(false);
                    if (ContentHash(fresh) != n.Notice.ContentSha256) throw new InvalidDataException("Notice changed during sync.");
                    await AtomicWrite(received, n.Notice.Id + ".md", Encoding.UTF8.GetBytes(n.Notice.Body), true, ct).ConfigureAwait(false);
                    delivered++;
                }
                var localManifest = new
                {
                    schemaVersion = 1,
                    deliveredProjectId = target.ProjectId,
                    projects = board.Manifest.GetProperty("projects"),
                    participants = board.Manifest.GetProperty("participants"),
                    notices = current.Select(n => new { id = n.Notice.Id, revision = n.Notice.Revision, title = n.Notice.Title,
                        path = n.Notice.Id + ".md", targets = n.Notice.Targets, required = n.Notice.Required, active = true, contentSha256 = n.Notice.ContentSha256 })
                };
                // No local read/receipt files are made by delivery. Inactive old bodies remain historical; manifest is authoritative.
                await AtomicWrite(received, "manifest.json", JsonSerializer.SerializeToUtf8Bytes(localManifest, JsonOptions), true, ct).ConfigureAwait(false);
                var helperSource = Inside(root, "scripts/project_notice.py");
                if (File.Exists(helperSource))
                {
                    var helper = await ReadText(helperSource, ct).ConfigureAwait(false);
                    await AtomicWrite(mailbox, "기록도우미.py", Encoding.UTF8.GetBytes(helper), true, ct).ConfigureAwait(false);
                }
                var instructions = "# 프로젝트 소통 폴더\n\n이 소통함의 프로젝트 ID: " + target.ProjectId + "\n\n" +
                    "- 받은공지: 자동 전달된 현재 공지와 목록입니다. 전달은 읽음 확인이 아닙니다. 목록에서 제외된 이전 본문은 이력입니다.\n" +
                    "- 보낼자료: UTF-8 .md/.txt/.json을 넣으면 중앙에서 수집합니다. 원본은 삭제하지 않습니다. 파일당 1MiB 이하, 변경 중·비밀키 의심 파일은 보류합니다.\n" +
                    "- 확인기록: 실제로 읽은 AI가 자기 프로젝트·작성자·세션 기록만 작성합니다. 읽음 대기와 적용 완료를 구분합니다.\n\n" +
                    "기록도우미.py가 배포되면 이 폴더에서 다음 명령으로 확인합니다. 프로그램은 도우미를 자동 실행하거나 확인 기록을 대신 작성하지 않습니다.\n\n" +
                    "```text\npython 기록도우미.py check --actor Sol --read\npython 기록도우미.py ack --actor Sol --session 본인작업세션 --notice N-0001 --revision 현재버전 --sha256 현재해시 --status pending --note \"본인이 실제 읽었고 적용 대기 이유\"\n```\n\n" +
                    "본문을 실제 읽은 뒤 현재 revision·hash를 사용하세요. 적용 완료에는 구체적인 설명·근거를 남깁니다. 받은공지의 manifest.json이 현재 전달 목록의 원본입니다. 로컬 배포·수집과 GitHub 공유 완료는 별개입니다. 다른 독립 채팅을 자동으로 깨우지 않습니다.\n";
                await AtomicWrite(mailbox, "README.md", Encoding.UTF8.GetBytes(instructions), true, ct).ConfigureAwait(false);
                foreach (var path in EnumerateSafe(localReceipts, errors, target.ProjectId))
                {
                    if (!Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)) continue;
                    if (++readFiles > MaximumFilesPerFolder) { Hold(errors, target.ProjectId, "file_limit"); break; }
                    try
                    {
                        var text = await ReadSettled(path, ct).ConfigureAwait(false);
                        if (HasSecret(text)) { Hold(errors, target.ProjectId, "secret_held"); continue; }
                        var receipt = ParseReceipt(text, board);
                        if (receipt.ProjectId != target.ProjectId || Path.GetFileName(path) != ReceiptFileName(receipt.ActorId, receipt.SessionId))
                            throw new InvalidDataException("Receipt owner mismatch.");
                        // History is retained centrally but never counted as current acknowledgement.
                        var relative = ReceiptRelative(receipt);
                        var destination = Inside(root, relative);
                        var serialized = JsonSerializer.Serialize(receipt, JsonOptions);
                        var serializedHash = ContentHash(serialized);
                        if (File.Exists(destination))
                        {
                            var existing = ParseReceipt(await ReadText(destination, ct).ConfigureAwait(false), board);
                            var existingHash = ContentHash(JsonSerializer.Serialize(existing, JsonOptions));
                            if (existingHash == serializedHash)
                            { imports[relative] = serializedHash; publish.Add(relative); continue; }
                            // Only update a central receipt which still matches what this mailbox last imported.
                            // An independently edited central record needs an explicit owner merge, not last-writer-wins.
                            if (imports.TryGetValue(relative, out var importedHash) && importedHash == existingHash &&
                                receipt.CheckedAt == existing.CheckedAt && receipt.ActorId == existing.ActorId && receipt.SessionId == existing.SessionId)
                            {
                                await AtomicWrite(root, relative, Encoding.UTF8.GetBytes(serialized), true, ct).ConfigureAwait(false);
                                imports[relative] = serializedHash; publish.Add(relative); count++; continue;
                            }
                            // No transport task silently resolves conflicting versions of an actor's receipt.
                            Hold(errors, target.ProjectId, "receipt_conflict"); continue;
                        }
                        await AtomicWrite(root, relative, JsonSerializer.SerializeToUtf8Bytes(receipt, JsonOptions), false, ct).ConfigureAwait(false);
                        imports[relative] = serializedHash;
                        publish.Add(relative); count++;
                    }
                    catch (Exception ex) when (IsFileError(ex)) { Hold(errors, target.ProjectId, "receipt_retry"); }
                }
                await AtomicWrite(mailbox, ".communication-imports.json", JsonSerializer.SerializeToUtf8Bytes(imports, JsonOptions), true, ct).ConfigureAwait(false);
                foreach (var path in EnumerateSafe(outbox, errors, target.ProjectId))
                {
                    var extension = Path.GetExtension(path).ToLowerInvariant();
                    if (extension is not (".md" or ".txt" or ".json")) continue;
                    if (++readFiles > MaximumFilesPerFolder) { Hold(errors, target.ProjectId, "file_limit"); break; }
                    try
                    {
                        var text = await ReadSettled(path, ct).ConfigureAwait(false);
                        if (extension == ".json") { using var json = JsonDocument.Parse(text); }
                        if (HasSecret(text)) { Hold(errors, target.ProjectId, "secret_held"); continue; }
                        var hash = ContentHash(text);
                        var relativeDirectory = "04_COMMUNICATION/project-inbox/" + target.ProjectId + "/" + hash;
                        var metadataPath = Inside(root, relativeDirectory + "/item.json");
                        if (File.Exists(metadataPath)) continue; // Committed content hash, irrespective of the original file name.
                        var relativeContent = relativeDirectory + "/content" + extension;
                        var contentPath = Inside(root, relativeContent);
                        if (File.Exists(contentPath))
                        {
                            if (ContentHash(await ReadText(contentPath, ct).ConfigureAwait(false)) != hash) throw new InvalidDataException("Existing inbox content differs.");
                        }
                        else await AtomicWrite(root, relativeContent, Encoding.UTF8.GetBytes(text), false, ct).ConfigureAwait(false);
                        var sourceName = Path.GetRelativePath(outbox, path).Replace('\\', '/');
                        if (HasSecret(sourceName)) { Hold(errors, target.ProjectId, "secret_held"); continue; }
                        var item = new CommunicationInboxItem(target.ProjectId, hash, sourceName, relativeContent, DateTimeOffset.UtcNow);
                        await AtomicWrite(root, relativeDirectory + "/item.json", JsonSerializer.SerializeToUtf8Bytes(item, JsonOptions), false, ct).ConfigureAwait(false);
                        publish.Add(relativeContent); publish.Add(relativeDirectory + "/item.json"); count++;
                    }
                    catch (Exception ex) when (IsFileError(ex)) { Hold(errors, target.ProjectId, "outbox_retry"); }
                }
            }
            catch (Exception ex) when (IsFileError(ex)) { Hold(errors, target.ProjectId, "project_unavailable"); }
            states.Add(new(target.ProjectId, target.RootPath, mailbox, delivered, count, errors.Count == before ? "연결됨" : "일부 보류"));
        }
        try { await ReadCentral(root, board, receipts, items, publish, errors, ct).ConfigureAwait(false); }
        catch (Exception ex) when (IsFileError(ex)) { Hold(errors, null, "path_blocked"); }
        try { await WriteStatus(root, board, receipts, publish, ct).ConfigureAwait(false); }
        catch (Exception ex) when (IsFileError(ex)) { Hold(errors, null, "status_retry"); }
        return new(notices, receipts, states, items, errors, DateTimeOffset.UtcNow, publish.Order(StringComparer.Ordinal).ToArray());
    }

    private async Task ReadCentral(string root, Board board, List<CommunicationReceiptItem> receipts,
        List<CommunicationInboxItem> items, HashSet<string> publish, List<CommunicationIssue> errors, CancellationToken ct)
    {
        var receiptRoot = Inside(root, "04_COMMUNICATION/announcements/receipts");
        foreach (var path in EnumerateSafe(receiptRoot, errors, null, 10000))
        {
            if (Path.GetExtension(path) != ".json") continue;
            try
            {
                var text = await ReadText(path, ct).ConfigureAwait(false);
                if (HasSecret(text)) { Hold(errors, null, "secret_held"); continue; }
                var receipt = ParseReceipt(text, board);
                var relative = ReceiptRelative(receipt);
                if (!PathComparer.Equals(path, Inside(root, relative))) throw new InvalidDataException("Receipt path mismatch.");
                var n = board.Notices[receipt.NoticeId];
                receipts.Add(new(receipt, Applicable(n, receipt.ProjectId) && receipt.Revision == n.Notice.Revision && receipt.ContentSha256 == n.Notice.ContentSha256, relative));
                publish.Add(relative);
            }
            catch (Exception ex) when (IsFileError(ex)) { Hold(errors, null, "central_receipt_invalid"); }
        }
        foreach (var path in EnumerateSafe(Inside(root, "04_COMMUNICATION/project-inbox"), errors, null, 10000))
        {
            if (Path.GetFileName(path) != "item.json") continue;
            try
            {
                var text = await ReadText(path, ct).ConfigureAwait(false);
                if (HasSecret(text)) throw new InvalidDataException("Metadata held.");
                var item = JsonSerializer.Deserialize<CommunicationInboxItem>(text, JsonOptions) ?? throw new InvalidDataException("Invalid item.");
                ValidateId(item.ProjectId); ValidateDigest(item.ContentSha256);
                if (!board.Projects.Contains(item.ProjectId)) throw new InvalidDataException("Unknown project.");
                var prefix = "04_COMMUNICATION/project-inbox/" + item.ProjectId + "/" + item.ContentSha256 + "/";
                if (Path.GetRelativePath(root, path).Replace('\\', '/') != prefix + "item.json" ||
                    item.CentralPath is null || !new[] { "content.txt", "content.md", "content.json" }.Any(name => item.CentralPath == prefix + name))
                    throw new InvalidDataException("Invalid item path.");
                var body = await ReadText(Inside(root, item.CentralPath), ct).ConfigureAwait(false);
                if (ContentHash(body) != item.ContentSha256 || HasSecret(body)) throw new InvalidDataException("Content held.");
                if (item.CentralPath.EndsWith(".json", StringComparison.Ordinal)) { using var json = JsonDocument.Parse(body); }
                var firstLine = body.Split('\n').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line))?.Trim() ?? "내용 없음";
                var preview = firstLine.Length > 160 ? firstLine[..160] : firstLine;
                items.Add(item with { Title = preview.TrimStart('#', ' '), Preview = preview, Body = body }); publish.Add(item.CentralPath); publish.Add(prefix + "item.json");
            }
            catch (Exception ex) when (IsFileError(ex)) { Hold(errors, null, "central_inbox_invalid"); }
        }
    }

    private static async Task WriteStatus(string root, Board board, List<CommunicationReceiptItem> receipts,
        HashSet<string> publish, CancellationToken ct)
    {
        const string relative = "04_COMMUNICATION/announcements/STATUS.md";
        var fingerprint = ContentHash(JsonSerializer.Serialize(board.Manifest, JsonOptions) + "\n" + string.Join("\n", receipts.OrderBy(r => r.Path, StringComparer.Ordinal).Select(r => JsonSerializer.Serialize(r.Receipt, JsonOptions))));
        var marker = "<!-- communication-state-sha256: " + fingerprint + " -->";
        var path = Inside(root, relative);
        if (File.Exists(path))
        {
            var existing = await ReadText(path, ct).ConfigureAwait(false);
            if (!HasSecret(existing) && existing.Contains(marker, StringComparison.Ordinal))
            { publish.Add(relative); return; }
        }
        static string Escape(string text) => text.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        var instant = DateTimeOffset.UtcNow;
        var lines = new List<string> { "# 공지 확인 현황", "", marker, "", "생성 시각: " + instant.ToOffset(TimeSpan.FromHours(9)).ToString("yyyy-MM-dd HH:mm:ss 'KST'", CultureInfo.InvariantCulture) + " / " + instant.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture), "",
            "프로젝트 기록은 누군가 해당 범위에서 확인했다는 뜻입니다. 전체 AI의 확인·적용 완료가 아닙니다. 자동 배포는 읽음 확인이 아닙니다.", "", "| 공지 | 확인 프로젝트 / 필수 대상 | 적용 완료 프로젝트 |", "|---|---:|---:|" };
        var required = board.Manifest.GetProperty("projects").EnumerateArray().Where(p => p.GetProperty("required").GetBoolean()).Select(p => p.GetProperty("id").GetString()!).ToHashSet(StringComparer.Ordinal);
        var current = receipts.Where(r => r.IsCurrent).Select(r => r.Receipt).ToArray();
        foreach (var n in board.Notices.Values.Where(n => n.Active))
        {
            var targets = required.Where(p => Applicable(n, p)).ToHashSet(StringComparer.Ordinal);
            var matching = current.Where(r => r.NoticeId == n.Notice.Id && targets.Contains(r.ProjectId)).ToArray();
            lines.Add($"| {Escape(n.Notice.Id + " · " + n.Notice.Title)} | {matching.Select(r => r.ProjectId).Distinct().Count()} / {targets.Count} | {matching.Where(r => r.ApplicationStatus == "applied").Select(r => r.ProjectId).Distinct().Count()} |");
        }
        lines.AddRange(["", "| 프로젝트 | 구분 | 현재 공지 확인 / 대상 | 작성자 |", "|---|---|---:|---|"]);
        var actorNames = board.Manifest.GetProperty("participants").EnumerateArray().ToDictionary(p => p.GetProperty("id").GetString()!, p => p.GetProperty("name").GetString() ?? p.GetProperty("id").GetString()!, StringComparer.Ordinal);
        foreach (var p in board.Manifest.GetProperty("projects").EnumerateArray())
        {
            var id = p.GetProperty("id").GetString()!;
            var records = current.Where(r => r.ProjectId == id).ToArray();
            var names = string.Join(", ", records.Select(r => actorNames.GetValueOrDefault(r.ActorId, r.ActorId)).Distinct());
            var name = p.GetProperty("name").GetString() ?? id;
            var category = p.GetProperty("category").GetString() ?? "";
            lines.Add($"| {Escape(name)} | {Escape(category)} · {(required.Contains(id) ? "필수" : "선택")} | {records.Select(r => r.NoticeId).Distinct().Count()} / {board.Notices.Values.Count(n => Applicable(n, id))} | {Escape(names.Length == 0 ? "미확인" : names)} |");
        }
        lines.AddRange(["", "| AI | 현재 확인 기록 |", "|---|---:|"]);
        foreach (var actor in actorNames.Keys)
            lines.Add($"| {Escape(actorNames[actor])} | {(current.Count(r => r.ActorId == actor) is var c && c > 0 ? c.ToString(CultureInfo.InvariantCulture) : "미확인")} |");
        await AtomicWrite(root, relative, Encoding.UTF8.GetBytes(string.Join("\n", lines) + "\n"), true, ct).ConfigureAwait(false);
        publish.Add(relative);
    }

    private sealed record Board(HashSet<string> Projects, Dictionary<string, BoardNotice> Notices, JsonElement Manifest);
    private sealed record BoardNotice(CommunicationNotice Notice, bool Active, string SourcePath);
    private async Task<Board> LoadBoard(string root, CancellationToken ct)
    {
        var manifestText = await ReadText(Inside(root, "04_COMMUNICATION/announcements/manifest.json"), ct).ConfigureAwait(false);
        if (HasSecret(manifestText)) throw new InvalidDataException("Manifest has sensitive data.");
        using var document = JsonDocument.Parse(manifestText);
        var manifest = document.RootElement;
        if (manifest.GetProperty("schemaVersion").GetInt32() != 1) throw new InvalidDataException("Schema version.");
        var projects = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in manifest.GetProperty("projects").EnumerateArray())
        {
            var id = p.GetProperty("id").GetString()!; ValidateId(id);
            _ = p.GetProperty("required").GetBoolean();
            if (!projects.Add(id)) throw new InvalidDataException("Duplicate project.");
        }
        var participants = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in manifest.GetProperty("participants").EnumerateArray())
        {
            var id = p.GetProperty("id").GetString()!; ValidateIdentity(id);
            if (!participants.Add(id)) throw new InvalidDataException("Duplicate participant.");
        }
        var notices = new Dictionary<string, BoardNotice>(StringComparer.Ordinal);
        foreach (var n in manifest.GetProperty("notices").EnumerateArray())
        {
            var id = n.GetProperty("id").GetString()!; ValidateId(id);
            var revision = n.GetProperty("revision").GetInt32();
            var title = n.GetProperty("title").GetString() ?? throw new InvalidDataException("Missing title.");
            var required = n.GetProperty("required").GetBoolean(); var active = n.GetProperty("active").GetBoolean();
            var hash = n.GetProperty("contentSha256").GetString()!; ValidateDigest(hash);
            var targets = n.GetProperty("targets").EnumerateArray().Select(x => x.GetString()!).ToArray();
            if (revision < 1 || targets.Length == 0 || targets.Any(t => t != "*" && !projects.Contains(t))) throw new InvalidDataException("Invalid targets.");
            var source = n.GetProperty("path").GetString()!;
            // This is a path inside announcements, not an arbitrary hub-relative path.
            var announcements = Inside(root, "04_COMMUNICATION/announcements");
            var body = await ReadText(Inside(announcements, source), ct).ConfigureAwait(false);
            if (ContentHash(body) != hash || HasSecret(body)) throw new InvalidDataException("Hash mismatch or sensitive notice.");
            if (!notices.TryAdd(id, new(new(id, revision, title, body, hash, targets, required), active, source))) throw new InvalidDataException("Duplicate notice.");
        }
        return new(projects, notices, manifest.Clone());
    }

    private static bool Applicable(BoardNotice notice, string project) => notice.Active && (notice.Notice.Targets.Contains("*") || notice.Notice.Targets.Contains(project));
    private static CommunicationReceipt ParseReceipt(string text, Board board)
    {
        using var raw = JsonDocument.Parse(text);
        foreach (var key in new[] { "schemaVersion", "noticeId", "revision", "contentSha256", "projectId", "actorId", "sessionId", "checkedAt", "applicationStatus", "note", "evidence" })
            if (!raw.RootElement.TryGetProperty(key, out _)) throw new InvalidDataException("Missing receipt field.");
        var r = JsonSerializer.Deserialize<CommunicationReceipt>(text, JsonOptions) ?? throw new InvalidDataException("Invalid receipt.");
        ValidateId(r.NoticeId); ValidateId(r.ProjectId); ValidateDigest(r.ContentSha256); ValidateIdentity(r.ActorId); ValidateIdentity(r.SessionId);
        if (r.SchemaVersion != 1 || r.Revision < 1 || !board.Projects.Contains(r.ProjectId) || !board.Notices.ContainsKey(r.NoticeId) ||
            string.IsNullOrWhiteSpace(r.Note) || r.Evidence is null || r.Evidence.Any(e => e is null) ||
            r.ApplicationStatus is not ("pending" or "applied" or "not_applicable" or "blocked")) throw new InvalidDataException("Invalid receipt fields.");
        var checkedAt = ParseTime(r.CheckedAt);
        if (r.ApplicationStatus == "applied")
        { if (ParseTime(r.AppliedAt) < checkedAt) throw new InvalidDataException("Invalid applied time."); }
        else if (r.AppliedAt is not null) throw new InvalidDataException("Unexpected applied time.");
        return r;
    }
    private static DateTimeOffset ParseTime(string? value)
    {
        if (value is null || !Regex.IsMatch(value, "(?:[zZ]|[+-][0-9]{2}:[0-9]{2})$") ||
            !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) throw new InvalidDataException("Timestamp timezone required.");
        return parsed;
    }
    private static string ReceiptRelative(CommunicationReceipt r) => "04_COMMUNICATION/announcements/receipts/" + r.NoticeId + "/r" + r.Revision + "/" + r.ProjectId + "/" + ReceiptFileName(r.ActorId, r.SessionId);
    private static void ValidateId(string? value) { if (value is null || !SafeId.IsMatch(value)) throw new InvalidDataException("Invalid ID."); }
    private static void ValidateDigest(string? value) { if (value is null || !Digest.IsMatch(value)) throw new InvalidDataException("Invalid digest."); }
    private static void ValidateIdentity(string? value)
    { if (string.IsNullOrWhiteSpace(value) || value.Length > 256 || value.Any(c => c < 32)) throw new InvalidDataException("Invalid actor/session."); }
    private static bool HasSecret(string text) => SecretPattern.IsMatch(text);
    private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    private static bool IsFileError(Exception ex) => ex is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException or KeyNotFoundException or RegexMatchTimeoutException;
    private static void Hold(List<CommunicationIssue> errors, string? id, string code)
    {
        var message = code switch
        {
            "secret_held" => "비밀키·인증정보 의심 패턴 때문에 보류했습니다. 본문을 표시하거나 중앙에 복사하지 않았습니다.",
            "receipt_conflict" => "같은 작성자·세션의 중앙 확인 기록과 달라 보류했습니다. 본인이 기록을 비교해 병합해야 합니다.",
            "project_unavailable" => "프로젝트 등록 ID·실제 폴더·안전한 경로를 확인해야 합니다.",
            "path_blocked" => "소통 폴더의 링크 또는 범위를 벗어난 경로를 건너뛰었습니다.",
            "file_limit" => "파일 수 제한에 도달했습니다. 나머지는 정리 후 다시 수집합니다.",
            _ => "파일이 변경 중이거나 형식·크기·경로 검증에 실패했습니다. 원본을 유지하고 다음 갱신에서 재시도합니다."
        };
        if (!errors.Any(e => e.ProjectId == id && e.Code == code)) errors.Add(new(id, code, message));
    }

    private static string ExistingRoot(string value)
    {
        if (!Path.IsPathFullyQualified(value)) throw new InvalidDataException("Explicit absolute root required.");
        var full = Path.GetFullPath(value);
        if (!Directory.Exists(full)) throw new DirectoryNotFoundException();
        RejectLinks(full);
        return full;
    }
    private static string Inside(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || relative.Contains('\\') || Path.IsPathRooted(relative) ||
            relative.Split('/').Any(p => p is "" or "." or ".." || p.Contains(':'))) throw new InvalidDataException("Unsafe relative path.");
        var fullRoot = Path.GetFullPath(root);
        var full = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var relativeAgain = Path.GetRelativePath(fullRoot, full);
        if (Path.IsPathRooted(relativeAgain) || relativeAgain == ".." || relativeAgain.StartsWith(".." + Path.DirectorySeparatorChar)) throw new InvalidDataException("Path escaped root.");
        RejectLinks(full);
        return full;
    }
    private static void RejectLinks(string path)
    {
        for (var current = Path.GetFullPath(path); !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
        {
            // File.GetAttributes detects a dangling link, for which File.Exists is false.
            try { if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Links are not communication roots."); }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
        }
    }
    private static string EnsureDirectory(string root, string relative)
    {
        var path = Inside(root, relative); Directory.CreateDirectory(path); RejectLinks(path); return path;
    }
    private static IEnumerable<string> EnumerateSafe(string root, List<CommunicationIssue> errors, string? project, int limit = MaximumFilesPerFolder)
    {
        if (!Directory.Exists(root)) yield break;
        var pending = new Stack<(string Path, int Depth)>(); pending.Push((root, 0)); var seen = 0;
        while (pending.Count > 0)
        {
            var (folder, depth) = pending.Pop();
            string[] entries;
            try { RejectLinks(folder); entries = Directory.EnumerateFileSystemEntries(folder).Take(limit + 1).Order(StringComparer.Ordinal).ToArray(); }
            catch (Exception ex) when (IsFileError(ex)) { Hold(errors, project, "path_blocked"); continue; }
            foreach (var entry in entries)
            {
                if (++seen > limit) { Hold(errors, project, "file_limit"); yield break; }
                FileAttributes attributes;
                try { RejectLinks(entry); attributes = File.GetAttributes(entry); }
                catch (Exception ex) when (IsFileError(ex)) { Hold(errors, project, "path_blocked"); continue; }
                if ((attributes & FileAttributes.Directory) != 0)
                { if (depth < 12) pending.Push((entry, depth + 1)); else Hold(errors, project, "path_blocked"); }
                else yield return entry;
            }
        }
    }
    private async Task<string> ReadSettled(string path, CancellationToken ct)
    {
        RejectLinks(path);
        var before = new FileInfo(path); var length = before.Length; var written = before.LastWriteTimeUtc;
        if (DateTime.UtcNow - written < _settleTime) throw new IOException("File is still settling; retry next sync.");
        var text = await ReadText(path, ct).ConfigureAwait(false);
        var after = new FileInfo(path);
        if (length != after.Length || written != after.LastWriteTimeUtc) throw new IOException("File changed; retry next sync.");
        return text;
    }
    private static async Task<string> ReadText(string path, CancellationToken ct)
    {
        RejectLinks(path);
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous);
        if (stream.Length > MaximumFileBytes) throw new InvalidDataException("File too large.");
        using var buffer = new MemoryStream();
        var chunk = new byte[8192]; int read;
        while ((read = await stream.ReadAsync(chunk, ct).ConfigureAwait(false)) != 0)
        {
            if (buffer.Length + read > MaximumFileBytes) throw new InvalidDataException("File too large.");
            buffer.Write(chunk, 0, read);
        }
        return NormalizeText(buffer.ToArray());
    }
    private static async Task AtomicWrite(string root, string relative, byte[] bytes, bool replace, CancellationToken ct)
    {
        var destination = Inside(root, relative); var directory = Path.GetDirectoryName(destination)!;
        if (replace && File.Exists(destination) && new FileInfo(destination).Length <= MaximumFileBytes)
        {
            var existing = await File.ReadAllBytesAsync(destination, ct).ConfigureAwait(false);
            if (existing.AsSpan().SequenceEqual(bytes)) return;
        }
        Directory.CreateDirectory(directory); RejectLinks(directory);
        var temp = Inside(directory, ".communication-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            await using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous))
            { await file.WriteAsync(bytes, ct).ConfigureAwait(false); await file.FlushAsync(ct).ConfigureAwait(false); file.Flush(true); }
            ct.ThrowIfCancellationRequested(); RejectLinks(destination); RejectLinks(directory);
            File.Move(temp, destination, replace);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
