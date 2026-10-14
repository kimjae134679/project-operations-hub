using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Security.Cryptography;
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
    /// <summary>Registers a validated read source in memory only. Never saves settings or starts work.</summary>
    public bool RegisterContinuousSource(string path, bool localView, bool manualControl)
    {
        if (localView && !manualControl) return false;
        var full=SafePath(path); _=ReadContinuous(full);
        if (ContinuousPaths.Contains(full,StringComparer.OrdinalIgnoreCase)) return true;
        if (ContinuousPaths.Count>=40) throw new InvalidDataException("상태 파일 연결은 최대 40개입니다.");
        ContinuousPaths.Add(full); return true;
    }
    private Dictionary<string, ProjectItem> _catalog = new(StringComparer.Ordinal);
    private Dictionary<string, string> _aliases = new(StringComparer.Ordinal);
    private HashSet<string> _managementProjects = new(["Control-Tower"], StringComparer.Ordinal);

    public void RegisterCatalog(IEnumerable<ProjectItem> projects, IReadOnlyDictionary<string,string>? explicitProjectAliases = null)
    {
        // Explicit identity only: no project/name/path heuristics or discovery here.
        _catalog = projects.Take(200).Where(p => !string.IsNullOrWhiteSpace(p.Id))
            .GroupBy(p => p.Id, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        _aliases = (explicitProjectAliases ?? new Dictionary<string,string>()).Take(200)
            .Where(p => _catalog.ContainsKey(p.Value)).ToDictionary(p => p.Key,p => p.Value,StringComparer.Ordinal);
    }

    public void RegisterManagementProjects(IEnumerable<string> projectIds)
        => _managementProjects = projectIds.Take(40).Where(p => !string.IsNullOrWhiteSpace(p)).ToHashSet(StringComparer.Ordinal);
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
        var catalog = _catalog; var aliases = _aliases; var managementProjects = _managementProjects;
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
                try { result.Add(ReadContinuous(path)); result.AddRange(ReadContinuousSteps(path)); }
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
            result.AddRange(records.GroupBy(r=>r.Id).Select(g=>
            {
                var latest=g.OrderByDescending(r=>r.Revision).ThenByDescending(r=>r.UpdatedAt).First();
                var conflict=g.GroupBy(r=>r.Revision).Any(revisions=>revisions.Select(r=>r.RecordFingerprint).Distinct(StringComparer.Ordinal).Take(2).Count()>1);
                return conflict?latest with {Title="기록 충돌",Status="unknown",SessionId="",CommandSummary="",ResponseSummary="",RequestSource="",ResponseSource="",RecentLog="",NextCheckpoint="",ResultSummary="같은 기록 revision 내용 충돌",Error="같은 기록 revision 내용 충돌 · 원본 확인 필요",DetailPath=null}:latest;
            }));
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
                    Project = SafeText(j.Project), ProjectId = j.Project, Worker = SafeText(j.Tool),WorkerKind="PC 작업",ExecutionId=SafeText(j.Id), Source = "ProjectBridge GET",
                    CommandSummary = "등록된 PC 작업: " + SafeText(j.Action),
                    Title = SafeText(j.Title), Status = !snapshot.Connected ? "stale" : j.Action == "start_process" && j.State is "accepted" or "succeeded" or "completed" ? "accepted" : j.State,
                    Stage = SafeText(j.State), Evidence = !snapshot.Connected ? "오래된/미연결 응답 · 현재 실행 미확인" : j.Action=="start_process" ? "시작 접수와 종료 확인을 분리합니다. 요약만으로 자식 완료 미확인 · 상세 조회 필요" : "최근 로컬 응답의 보고 상태 · 실제 결과는 상세 조회",
                    UpdatedAt = DateTimeOffset.UtcNow, LivenessKnown = false }));
            }
            catch (Exception ex) when (IsReadError(ex) || ex is HttpRequestException or TaskCanceledException)
            { ct.ThrowIfCancellationRequested(); rows.Add(ReadError("bridge:connection", "ProjectBridge", ex)); }
        }
        foreach (var worker in new[] { "Jev", "Codex", "CMD", "PowerShell / 외부 콘솔" })
            rows.Add(new() { Id = "unknown:" + worker, Project = "외부 작업", Worker = worker, Source = "작업 연결 없음",
                Title = worker + " 외부 작업", Evidence = "설치·프로세스 존재는 작업 진행이 아닙니다. 등록된 실제 작업/API/소유 기록이 없으므로 미확인입니다." });
        return rows.DistinctBy(r=>r.Id).OrderByDescending(r => r.UpdatedAt).Take(600)
            .Select(row => ProjectMetadata(row,catalog,aliases,managementProjects)).ToArray();
    }

    private static WorkActivity ProjectMetadata(WorkActivity row, IReadOnlyDictionary<string,ProjectItem> catalog,
        IReadOnlyDictionary<string,string> aliases, IReadOnlySet<string> managementProjects)
    {
        var reported = row.ProjectId;
        var projectId = aliases.TryGetValue(reported,out var linked) ? linked : reported;
        var known = catalog.TryGetValue(projectId,out var project);
        var worker = row.WorkerKind; var command = row.CommandSummary;
        if(row.Source == "관제탑 소유 실행 기록")
        {
            var program = known ? project!.Functions.SelectMany(f=>f.Programs).FirstOrDefault(p=>p.Id==row.ProgramId && p.ProjectId==projectId) : null;
            var matched = program?.Commands.Where(c=>c.Name==row.Title).Take(2).ToArray();
            if(program is not null && matched?.Length==1)
            {
                worker = ClassifyWorker(program.Kind,matched[0].FileName);
                command = SafeText("등록 카탈로그 명령: " + matched[0].Name + " → " + matched[0].FileName + " · 인수/프롬프트 원문 비공개");
            }
            else if(known && row.Title=="Jev 작업" && (row.ProgramId==projectId+"/jev" || JevExecutionWorkspace.MatchesScopedProgram(project!,row.ProgramId)))
            {
                // Exact legacy or registered workspace identity created by RunJevTask; never infer from a model/process name.
                worker="Jev";command="Jev 작업";
            }
            else worker = "미확인";
        }
        var status=row.Status;
        if(row.Source=="관제탑 소유 실행 기록" && worker is "Codex" or "Jev" && status is "succeeded" or "completed")
            status=row.AICompletionState=="succeeded"?"succeeded":row.AICompletionState=="failed"?"failed":"unknown";
        return row with { ProjectId = projectId, ReportedProjectId = reported, ProjectDisplayName = known ? SafeText(project!.DisplayName) : projectId.Length>0 ? SafeText(projectId) : "프로젝트 연결 미확인",
            Status=status,
            WorkerKind = worker, CommandSummary = command,
            IsManagementRecord = row.Source == "명령·답변 task_exchange 기록" ? managementProjects.Contains(reported) : row.IsManagementRecord };
    }

    private static string ClassifyWorker(string kind, string executable)
    {
        if(kind == "Codex")return "Codex";
        if(kind == "Jev")return "Jev";
        return Path.GetFileName(executable).ToLowerInvariant() switch { "cmd.exe" or "cmd" => "CMD", "powershell.exe" or "powershell" or "pwsh.exe" or "pwsh" => "PowerShell", "codex.exe" or "codex" => "Codex", "jev.exe" or "jev" => "Jev", _ => "program" };
    }

    public async Task<string> DetailsAsync(WorkActivity row)
    {
        if(row.Source=="연속 실행기 단계 기록") return row.ResultSummary+"\n"+row.NextCheckpoint;
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
        return new() { Id = "manager:" + Path.GetFullPath(path), Project = SafeText(Text(j, "project", "미확인")), ProjectId = Text(j,"project"), Worker = "연속 실행기", WorkerKind = "연속 실행기",
            Source = "연속 실행기 체크포인트", Title = Path.GetFullPath(path).Equals(FoundationState, StringComparison.OrdinalIgnoreCase)
                ? "연속실행기 기반 검증(완료 기록)" : SafeText(Text(j, "plan_id", "등록 목록")),
            Status = Text(j, "status", "unknown"), Stage = $"{list.Count(s => Text(s.Value, "status") == "succeeded")}/{list.Length}단계 완료" + (current.Length > 0 ? " · 기록된 단계 " + string.Join(", ", current) : ""),
            Evidence = "마지막 체크포인트 기록 · 갱신 시각은 heartbeat가 아니며 현재 프로세스 생존은 미확인",
            UpdatedAt = Timestamp(j, "updated_at"), NextCheckpoint = pending.Length > 0 ? "대기 단계 후보: " + string.Join(", ", pending) + " · 의존성/실행 가능 여부 미확인" : "추가 대기 단계 기록 없음",
            Error = SafeText(string.Join("\n", errors)), ResultSummary = SafeText("체크포인트 결과 기록: " + Text(j,"status","unknown") + " · 실제 AI 업무 완료 별도 확인 필요"), DetailPath = path, LivenessKnown = false };
    }

    private static IReadOnlyList<WorkActivity> ReadContinuousSteps(string path)
    {
        using var doc=ReadJson(path);var root=doc.RootElement;
        SafePath(path,Text(root,"approved_root"));
        var plan=Text(root,"plan_id");var hash=Text(root,"plan_sha256");
        var rows=new List<WorkActivity>();
        foreach(var step in root.GetProperty("steps").EnumerateObject().Take(1000))
        {
            var value=step.Value;var state=Text(value,"status","unknown");var execution=plan+"/"+step.Name;
            var exit=value.TryGetProperty("returncode",out var code)&&code.ValueKind==JsonValueKind.Number&&code.TryGetInt32(out var number)?(int?)number:null;
            var hasReceipt=value.TryGetProperty("aiReceipt",out var receipt)&&receipt.ValueKind==JsonValueKind.Object;
            var identity=hasReceipt && plan.Length>0 && hash.Length==64 && hash.All(Uri.IsHexDigit)
                && Text(receipt,"planId")==plan && Text(receipt,"stepId")==step.Name && Text(receipt,"planSha256")==hash;
            var ai=identity ? ReadAIReceipt(value,execution,state,exit) : (State:"unknown",Summary:hasReceipt?"AI 업무 완료 미확인 · 체크포인트 실행 식별자 불일치":"AI 완료 영수증 없음");
            var adapter=identity ? Text(receipt,"adapter") : "";
            var worker=adapter switch {"codex-jsonl-v1"=>"Codex JSONL","wrapper-json-v1"=>"등록 AI wrapper",_=>"연속 실행기"};
            rows.Add(new WorkActivity {
                Id="manager-step:"+JsonSerializer.Serialize(new[]{Path.GetFullPath(path),plan,step.Name}),
                Project=SafeText(Text(root,"project","미확인")),ProjectId=Text(root,"project"),
                Source="연속 실행기 단계 기록",Title=SafeText(step.Name),Worker=worker,WorkerKind=worker,
                ExecutionId=SafeText(execution),SessionId=SafeText(plan),RequestedModel=identity?SafeText(Text(receipt,"requestedModel")):"",
                CommandSummary=SafeText(step.Name),ProcessStatus=state,AICompletionState=ai.State,
                Status=hasReceipt&&state is "succeeded" or "completed" ? ai.State : state,
                Stage=SafeText("시도 "+Text(value,"attempts","미확인")+" · "+state),
                ResultSummary=SafeText("프로세스: "+state+" · 종료 코드: "+Text(value,"returncode","미확인")+"\n"+ai.Summary),
                UpdatedAt=Timestamp(value,"finished_at")??Timestamp(value,"started_at")??Timestamp(root,"updated_at"),
                Evidence="저장된 단계 기록 · 현재 프로세스 생존 미확인",LivenessKnown=false,
                NextCheckpoint="", DetailPath=null
            });
        }
        return rows;
    }

    public static WorkActivity ReadLocalJob(string path, Func<string, bool> owns)
    {
        using var doc = ReadJson(path); var j = doc.RootElement; var program = Text(j, "ProgramId");
        var owned = owns(program); var state = Text(j, "State", "unknown");
        var exit=j.TryGetProperty("ExitCode",out var code) && code.ValueKind==JsonValueKind.Number && code.TryGetInt32(out var number)?(int?)number:null;
        var ai=ReadAIReceipt(j,Text(j,"Id"),state,exit);
        return new() { Id = "local:" + Text(j, "Id", path), Project = SafeText(Text(j, "ProjectId", "미확인")), ProjectId = Text(j,"ProjectId"), ProgramId = program,
            Worker = "관제탑 등록 프로그램", ExecutionId=Text(j,"Id"),SessionId=Text(j,"Id"),
            RequestedModel=j.TryGetProperty("aiReceipt",out var receipt)?SafeText(Text(receipt,"requestedModel")):"",
            Source = "관제탑 소유 실행 기록", Title = SafeText(Text(j, "Command", "등록 작업")), Status = ai.State=="failed"?"failed":state,
            ProcessStatus=state,AICompletionState=ai.State,
            Stage = Text(j, "ExitCode") is { Length: > 0 } recordedExit ? "exit " + recordedExit : state,
            UpdatedAt = Timestamp(j, "FinishedAt") ?? Timestamp(j, "StartedAt"),
            Evidence = owned ? "현재 관제탑 JobManager 소유 실행 확인" : "저장된 결과 기록 · 현재 프로세스 생존 미확인",
            ResultSummary = SafeText("프로세스 결과 기록: " + state + " · 종료 코드: " + Text(j,"ExitCode","미확인") + "\n" + ai.Summary),
            LivenessKnown = owned, OwnedProgramId = owned ? program : null, DetailPath = path };
    }

    private static (string State,string Summary) ReadAIReceipt(JsonElement job,string executionId,string processState,int? outerExit)
    {
        (string State,string Summary) Unknown(string reason)=> ("unknown","AI 업무 완료 미확인 · "+reason);
        (string State,string Summary) Failed(string reason)=> ("failed","AI 실패 증거 · "+reason);
        if(!job.TryGetProperty("aiReceipt",out var receipt) || receipt.ValueKind!=JsonValueKind.Object)return Unknown("선언된 AI 완료 영수증 없음");
        if(!receipt.TryGetProperty("schemaVersion",out var schema) || schema.ValueKind!=JsonValueKind.Number || !schema.TryGetInt32(out var version) || version!=1
            || Text(receipt,"adapter") is not ("codex-jsonl-v1" or "wrapper-json-v1"))return Unknown("영수증 형식/adapter 확인 필요");
        if(string.IsNullOrEmpty(executionId) || Text(receipt,"executionId")!=executionId || string.IsNullOrWhiteSpace(Text(receipt,"requestedModel")))
            return Unknown("실행 식별자/요청 모델 일치 확인 필요");
        if(processState is "failed" or "cancelled" or "stopped" or "timed_out" or "interrupted" or "output_limit" || outerExit is not null && outerExit!=0)
            return Failed("외부 프로세스 오류/중단이 성공 영수증보다 우선");
        var receiptExit=receipt.TryGetProperty("processExitCode",out var innerCode) && innerCode.ValueKind==JsonValueKind.Number && innerCode.TryGetInt32(out var innerNumber)?(int?)innerNumber:null;
        if(Text(receipt,"status")=="failed" || Text(receipt,"failureCode").Length>0 || receiptExit is not null && receiptExit!=0)
            return Failed(SafeText(Text(receipt,"failureCode","adapter 실패 보고")));
        if(processState is not ("succeeded" or "completed") || outerExit!=0 || Text(receipt,"status")!="succeeded" || receiptExit!=0
            || Bool(receipt,"terminalObserved")!=true || Bool(receipt,"reportObserved")!=true || Bool(receipt,"reportRequired") is null
            || !receipt.TryGetProperty("failureCode",out var failure) || failure.ValueKind!=JsonValueKind.Null)
            return Unknown("프로세스·terminal·report의 성공 증거 부족");
        var digest=Text(receipt,"artifactSha256");
        if(Bool(receipt,"reportRequired")==true && (Bool(receipt,"artifactVerified")!=true || digest.Length!=64 || !digest.All(Uri.IsHexDigit)))
            return Unknown("필수 결과 artifact 검증 증거 부족");
        return ("succeeded","AI 완료 증거 확인 · 선언된 실행·terminal·report 영수증 일치 · 계정 인증/결과 품질을 보증하지 않음");
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
        // Official helper identity is project + actor + record. Sessions may change during handover.
        return new() { Id = "exchange:"+parsed.ProjectId+":"+parsed.ActorId+":"+parsed.RecordId, Project = SafeText(parsed.ProjectId), ProjectId = parsed.ProjectId,
            RecordFingerprint=Fingerprint(j),
            ActorId=SafeText(parsed.ActorId), SessionId=SafeText(parsed.SessionId),WorkerKind="기록 담당자",
            RequestSource=SafeText(parsed.RequestSource),ResponseSource=SafeText(parsed.ResponseSource),
            CommandSummary=SafeText(parsed.RequestSummary),ResponseSummary=SafeText(parsed.ResponseSummary),
            Worker = SafeText(Text(j, "actorId", "기록 담당자")), Source = "명령·답변 task_exchange 기록", Title = SafeText(Text(j, "title")),
            Status = Text(j, "status", "unknown"), Stage = "revision " + revision.ToString("D8"), UpdatedAt = Timestamp(j, "updatedAt"),
            Evidence = "작성자가 남긴 최신 작업 기록 · 자동 채팅 감시/현재 실행 생존 확인이 아님",
            RecentLog = SafeText(work), NextCheckpoint = SafeText(next + "\n검증: " + checks), Error = SafeText(ArrayText(j, "blockers")),
            ResultSummary = SafeText("작성자 보고: " + parsed.StateLabel + "\n검증: " + checks),
            DetailPath = path, IsManagementRecord = parsed.ProjectId == "Control-Tower", Revision = parsed.Revision };
    }

    private static string Fingerprint(JsonElement root)
    {
        // Canonical metadata comparison only; no body/hash is written, logged or shown.
        using var bytes=new MemoryStream();
        using(var writer=new Utf8JsonWriter(bytes))
        {
            void Write(JsonElement item)
            {
                if(item.ValueKind==JsonValueKind.Object)
                {
                    writer.WriteStartObject();foreach(var field in item.EnumerateObject().OrderBy(p=>p.Name,StringComparer.Ordinal)) {writer.WritePropertyName(field.Name);Write(field.Value);}writer.WriteEndObject();
                }
                else if(item.ValueKind==JsonValueKind.Array) {writer.WriteStartArray();foreach(var value in item.EnumerateArray())Write(value);writer.WriteEndArray();}
                else item.WriteTo(writer);
            }
            Write(root);
        }
        return Convert.ToHexString(SHA256.HashData(bytes.ToArray()));
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
        var heading=accepted ? "접수/시작 기록 · 자식 종료/완료 미확인" : terminal ? "프로세스 종료 확인 · AI 업무 완료 미확인 · 프로세스 상태 " + child : "실제 응답 상태: " + state + (child.Length>0?" · 자식 " + child:"");
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
