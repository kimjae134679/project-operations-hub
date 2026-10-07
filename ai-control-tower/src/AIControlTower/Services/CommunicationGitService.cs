using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AIControlTower.Services;

public sealed record CommunicationGitResult(bool Success, bool Published, string Message)
{
    public string FailureStage { get; init; } = "";
    public int? ExitCode { get; init; }
    public string Diagnostic { get; init; } = "";
    public string StatusBrief => Message;
}

/// <summary>Owns a dedicated mirror. Only validated, generated communication paths are published.</summary>
public sealed class CommunicationGitService
{
    public const string RepositoryUrl = "https://github.com/kimjae134679/project-operations-hub.git";
    private readonly string _origin;
    private readonly Func<string?,CancellationToken,string[],Task<(int Code,string Output,string Error)>>? _runner;
    public CommunicationGitService(string origin = RepositoryUrl, Func<string?,CancellationToken,string[],Task<(int Code,string Output,string Error)>>? runner = null)
    { _origin = origin; _runner = runner; }
    public static bool IsPublishablePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\\') || value.Split('/').Any(p => p is ".." or "." or "")) return false;
        return value == "04_COMMUNICATION/announcements/STATUS.md"
            || value.StartsWith("04_COMMUNICATION/announcements/receipts/", StringComparison.Ordinal) && value.EndsWith(".json", StringComparison.Ordinal)
            || value.StartsWith("04_COMMUNICATION/project-inbox/", StringComparison.Ordinal) && Path.GetExtension(value) is ".md" or ".txt" or ".json";
    }
    private static string Marker(string path) => Path.Combine(path, ".git", "control-tower-communication-mirror");
    private bool Owned(string path) => File.Exists(Marker(path)) && File.ReadAllText(Marker(path)).Trim() == _origin;
    public async Task<CommunicationGitResult> PrepareAsync(string path, CancellationToken ct = default)
    {
        if (Directory.Exists(Path.Combine(path, ".git")))
        {
            if (!Owned(path)) return new(false, false, "기존 작업 폴더의 자료를 보존합니다. 자동 소통은 전용 폴더에서만 가능합니다.");
            var origin = await Git(path, ct, "remote", "get-url", "origin");
            if (origin.Code != 0) return Failure("remote",origin,"소통 저장소 연결 주소 확인 실패");
            if (origin.Output.Trim() != _origin) return new(false, false, "소통 저장소의 연결 주소가 변경되어 자동 업로드를 보류했습니다.");
            return new(true, false, "전용 소통 저장소 연결됨");
        }
        if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any()) return new(false, false, "선택한 소통 폴더에 기존 자료가 있어 초기화를 보류했습니다.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var staging = path + ".clone-" + Guid.NewGuid().ToString("N");
        try
        {
            var cloned = await Git(null, ct, "clone", "--depth", "1", "--single-branch", "--branch", "main", "--", _origin, staging);
            if (cloned.Code != 0) return Failure("clone",cloned,"GitHub 연결 대기 · 네트워크와 Git 로그인을 확인하세요.");
            File.WriteAllText(Marker(staging), _origin);
            var name = await Git(staging, ct, "config", "user.name", "Control Tower");
            if (name.Code != 0) return Failure("config",name,"소통 저장소 초기화 대기 · 다음 연결에서 다시 시도합니다.");
            var email = await Git(staging, ct, "config", "user.email", "control-tower@users.noreply.github.com");
            if (email.Code != 0) return Failure("config",email,"소통 저장소 초기화 대기 · 다음 연결에서 다시 시도합니다.");
            if (Directory.Exists(path)) Directory.Delete(path, false); // Only an empty destination may be removed.
            Directory.Move(staging, path);
            return new(true, false, "소통 저장소 연결됨");
        }
        finally
        {
            // This unique staging clone belongs to this attempt; never clean a user's destination.
            try
            {
                if (Directory.Exists(staging))
                {
                    foreach (var file in Directory.EnumerateFiles(staging, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal);
                    Directory.Delete(staging, true);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }
    public async Task<CommunicationGitResult> SynchronizeAsync(string path, IEnumerable<string> validatedPaths, bool publish, CancellationToken ct = default)
    {
        if (!Owned(path)) return new(false, false, "전용 소통 저장소가 아니므로 자동 업로드를 보류했습니다.");
        var originFailure = await ExpectedOriginFailure(path, ct);
        if (originFailure is not null) return originFailure;
        var paths = validatedPaths.Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
        if (paths.Any(p => !IsPublishablePath(p))) return new(false, false, "검증 대상 밖 경로가 있어 자동 업로드를 보류했습니다.");
        var status = await Git(path, ct, "status", "--porcelain=v1", "-z", "--untracked-files=all");
        if (status.Code != 0) return Failure("status",status,"소통 저장소 상태 확인 실패");
        var dirty = new List<string>();
        foreach (var row in status.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries))
        {
            if (row.Length < 4 || row[..2].Contains('R') || row[..2].Contains('C') || row[..2].Contains('D') || row[..2].Contains('U')) return new(false, false, "삭제·이동·충돌 기록이 있어 자동 동기화를 보류했습니다.");
            var relative = row[3..];
            if (!paths.Contains(relative) || !IsPublishablePath(relative)) return new(false, false, "자동 수집 범위 밖 수정이 있어 소통 저장소 동기화를 보류했습니다.");
            dirty.Add(relative);
        }
        if (dirty.Count > 0)
        {
            foreach (var relative in dirty)
            {
                var full = Path.GetFullPath(Path.Combine(path, relative));
                if (!full.StartsWith(Path.GetFullPath(path) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(full)) return new(false, false, "수집 파일 경로 확인 실패");
                for (var current = new FileInfo(full) as FileSystemInfo; current is not null; current = current is FileInfo f ? f.Directory : ((DirectoryInfo)current).Parent)
                {
                    if (current.Attributes.HasFlag(FileAttributes.ReparsePoint)) return new(false, false, "연결 파일은 자동 공개하지 않습니다.");
                    if (current.FullName.Equals(Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase)) break;
                }
                var add = await Git(path, ct, "add", "--", relative);
                if (add.Code != 0) return Failure("add",add,"수집 파일의 GitHub 반영 준비 실패");
            }
            var committed = await Git(path, ct, "commit", "-m", "Collect project communication records");
            if (committed.Code != 0) return Failure("commit",committed,"소통 기록 커밋 대기");
        }
        var fetched = await Git(path, ct, "fetch", "origin", "main");
        if (fetched.Code != 0) return Failure("fetch",fetched,"로컬 수집 완료 · GitHub 연결 대기");
        var rebased = await Git(path, ct, "rebase", "origin/main");
        if (rebased.Code != 0)
        {
            await Git(path, ct, "rebase", "--abort");
            return Failure("rebase",rebased,"다른 수정과 충돌하여 중앙 동기화를 보류했습니다. 수집 기록은 보존했습니다.");
        }
        var ahead = await Git(path, ct, "rev-list", "--count", "origin/main..HEAD");
        if (ahead.Code != 0) return Failure("rev-list",ahead,"중앙 업로드 대기 기록 확인 실패");
        var count = int.TryParse(ahead.Output.Trim(), out var n) ? n : 0;
        if (count == 0) return new(true, false, "최신 공지와 기록이 연결되어 있습니다.");
        if (!publish) return new(true, false, "공지 내려받음 · 업로드할 기록은 로컬 대기 중");
        // Every outgoing commit must touch only communication-generated paths, including retried commits.
        var outgoing = await Git(path, ct, "rev-list", "origin/main..HEAD");
        if (outgoing.Code != 0) return Failure("rev-list",outgoing,"중앙 업로드 커밋 확인 실패");
        foreach (var commit in outgoing.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var changes = await Git(path, ct, "diff-tree", "--root", "-m", "--no-commit-id", "--name-only", "-r", "-z", commit.Trim());
            if (changes.Code != 0) return Failure("diff-tree",changes,"중앙 업로드 커밋 경로 확인 실패");
            if (changes.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries).Any(p => !IsPublishablePath(p))) return new(false, false, "중앙 업로드 범위 밖 커밋을 발견해 보류했습니다.");
            foreach (var relative in changes.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries).Distinct())
            {
                var size = await Git(path, ct, "cat-file", "-s", commit.Trim() + ":" + relative);
                if (size.Code != 0) continue; // A deletion contains no new blob.
                if (!int.TryParse(size.Output.Trim(), out var bytes) || bytes > CommunicationService.MaximumFileBytes) return new(false, false, "크기 제한을 넘는 소통 기록을 보류했습니다.");
                var blob = await Git(path, ct, "show", commit.Trim() + ":" + relative);
                if (blob.Code != 0) return Failure("show",blob,"중앙 업로드 본문 확인 실패");
                if (CommunicationService.ContainsSensitiveText(blob.Output)) return new(false, false, "비밀값 의심 기록의 GitHub 업로드를 보류했습니다.");
            }
        }
        var pushed = await Git(path, ct, "push", _origin, "HEAD:main");
        if (pushed.Code != 0) return Failure("push",pushed,"수집 기록은 보존됨 · GitHub 업로드 재시도 대기");
        // Pushing to the verified URL does not advance the named remote's tracking ref.
        var confirmed = await Git(path, ct, "fetch", "origin", "main");
        return confirmed.Code == 0 ? new(true,true,"GitHub에 수집 기록을 반영했습니다.")
            : Failure("fetch-confirm",confirmed,"GitHub 반영 완료 · 최신 공지 재확인은 다음 연결에서 이어갑니다.",success:true,published:true);
    }
    private async Task<CommunicationGitResult?> ExpectedOriginFailure(string path, CancellationToken ct)
    {
        foreach(var push in new[]{false,true})
        {
            var arguments=push ? new[]{"remote","get-url","--push","--all","origin"} : new[]{"remote","get-url","--all","origin"};
            var result=await Git(path,ct,arguments);
            var urls=result.Output.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries);
            if(result.Code!=0) return Failure("remote",result,"소통 저장소 연결 주소 확인 실패");
            if(urls.Length!=1 || urls[0]!=_origin) return new(false,false,"소통 저장소의 연결 주소가 변경되어 자동 업로드를 보류했습니다.");
        }
        return null;
    }
    private Task<(int Code,string Output,string Error)> Git(string? root,CancellationToken ct,params string[] arguments) =>
        _runner is null ? ExecuteGit(root,ct,arguments) : _runner(root,ct,arguments);
    private static async Task<(int Code, string Output, string Error)> ExecuteGit(string? root, CancellationToken ct, string[] arguments)
    {
        using var process = new Process { StartInfo = new ProcessStartInfo("git") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8 } };
        process.StartInfo.ArgumentList.Add("-c"); process.StartInfo.ArgumentList.Add("core.hooksPath=" + Path.Combine(Path.GetTempPath(), "control-tower-no-hooks"));
        process.StartInfo.ArgumentList.Add("-c"); process.StartInfo.ArgumentList.Add("core.quotePath=false");
        if (root is not null) { process.StartInfo.ArgumentList.Add("-C"); process.StartInfo.ArgumentList.Add(root); }
        foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        process.StartInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
        process.StartInfo.Environment["GCM_INTERACTIVE"] = "Never";
        process.StartInfo.Environment["GIT_EDITOR"] = "true";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(arguments.FirstOrDefault() == "clone" ? 120 : 25));
        try
        {
            process.Start(); var output = process.StandardOutput.ReadToEndAsync(); var error = ReadBoundedError(process.StandardError);
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { if (!process.HasExited) process.Kill(true); await process.WaitForExitAsync(CancellationToken.None); await output; return (-1, "", (await error)+"\nGit command timed out or cancelled."); }
            return (process.ExitCode, await output, await error);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or OperationCanceledException) { return (-1, "", ex.Message); }
    }
    private static async Task<string> ReadBoundedError(TextReader reader)
    {
        const int limit=16*1024;var text=new StringBuilder();var buffer=new char[2048];var truncated=false;
        while(true){var read=await reader.ReadAsync(buffer);if(read==0)break;var keep=Math.Min(read,Math.Max(0,limit-text.Length));text.Append(buffer,0,keep);if(keep<read)truncated=true;}
        if(truncated)text.Append("\n[stderr length limited]");return text.ToString();
    }
    private static CommunicationGitResult Failure(string stage,(int Code,string Output,string Error) result,string message,bool success=false,bool published=false)
    {
        var diagnostic=SafeDiagnostic(result.Error);
        return new(success,published,message+" · Git "+stage+" · exit "+result.Code+" · "+diagnostic)
        {FailureStage=stage,ExitCode=result.Code,Diagnostic=diagnostic};
    }
    private static string SafeDiagnostic(string error)
    {
        try
        {
            var text=error??"";
            string Replace(string pattern,string replacement)=>Regex.Replace(text,pattern,replacement,RegexOptions.IgnoreCase|RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(100));
            text=Replace("\\x1b\\[[0-?]*[ -/]*[@-~]","");
            text=new string(text.Where(c=>!char.IsControl(c)||c is '\n' or '\r' or '\t').ToArray());
            var safeLines=new StringBuilder();var heldSection=false;
            foreach(var line in text.Replace("\r","").Split('\n'))
            {
                if(Regex.IsMatch(line,@"\b(?:prompt|command(?:\s+line)?|stdin|footer|payload|request|instructions?)[""']?\s*[:=]",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(100)))
                {heldSection=true;safeLines.AppendLine("[명령·입력 상세 생략]");continue;}
                if(heldSection && !Regex.IsMatch(line,@"^\s*(?:remote:\s*)?(?:fatal|error|warning):",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(100)))continue;
                heldSection=false;safeLines.AppendLine(line);
            }
            text=safeLines.ToString();
            text=Replace(@"(?:https?|ssh|git|file)://[^\s'""<>]+","[주소]");
            text=Replace(@"\b[^\s@]+@[^\s:]+:[^\s]+","[주소]");
            text=Replace(@"\b[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}\b","[이메일]");
            text=Regex.Replace(text,@"\b[A-Z]:[\\/][^\r\n'""<>|]+|\\\\[^\r\n'""<>|]+|/(?:home|Users|tmp|var/tmp)/[^\s'""<>]+",m=>
            {
                foreach(var known in new[]{"index.lock","packed-refs.lock","REBASE_HEAD.lock"})if(m.Value.EndsWith(known,StringComparison.OrdinalIgnoreCase))return "[경로]/"+known;
                return "[경로]";
            },RegexOptions.IgnoreCase|RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(100));
            text=Replace(@"\b(?:sk-[A-Z0-9_\-]+|gh[pousr]_[A-Z0-9_]+|github_pat_[A-Z0-9_]+)\b","[인증값]");
            text=Replace(@"\b[A-Z0-9_\-]{10,}\.[A-Z0-9_\-]{10,}\.[A-Z0-9_\-]{10,}\b","[인증값]");
            text=Replace(@"\bbearer\s+[^\s]+","Bearer [인증값]");
            text=Replace(@"(?m)\b(?:password|passwd|authorization|api[_\- ]?key|access[_\- ]?token|refresh[_\- ]?token|client[_\- ]?secret|secret|token)\b[^\r\n]*","[인증 상세 생략]");
            text=text.Trim();if(text.Length==0)return "stderr 없음 또는 표시 보류";
            return text.Length<=1024?text:text[..1023]+"…";
        }
        catch(RegexMatchTimeoutException){return "진단 정제 한도를 넘어 표시를 보류했습니다.";}
    }
}
