using System.Diagnostics;
using System.Text;

namespace AIControlTower.Services;

public sealed record CommunicationGitResult(bool Success, bool Published, string Message);

/// <summary>Owns a dedicated mirror. Only validated, generated communication paths are published.</summary>
public sealed class CommunicationGitService
{
    public const string RepositoryUrl = "https://github.com/kimjae134679/project-operations-hub.git";
    private readonly string _origin;
    public CommunicationGitService(string origin = RepositoryUrl) => _origin = origin;
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
            if (!Owned(path)) return new(false, false, "폴더에서 공지를 읽습니다. 자동 GitHub 동기화는 전용 소통 폴더에서만 가능합니다.");
            var origin = await Git(path, ct, "remote", "get-url", "origin");
                if (origin.Code != 0 || origin.Output.Trim() != _origin) return new(false, false, "소통 저장소의 연결 주소가 변경되어 자동 업로드를 보류했습니다.");
            return new(true, false, "전용 소통 저장소 연결됨");
        }
        if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any()) return new(false, false, "선택한 소통 폴더에 기존 자료가 있어 초기화를 보류했습니다.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var cloned = await Git(null, ct, "clone", "--single-branch", "--branch", "main", "--", _origin, path);
        if (cloned.Code != 0) return new(false, false, "GitHub 연결 대기 · 네트워크와 Git 로그인을 확인하세요.");
        File.WriteAllText(Marker(path), _origin);
        await Git(path, ct, "config", "user.name", "Control Tower");
        await Git(path, ct, "config", "user.email", "control-tower@users.noreply.github.com");
        return new(true, false, "소통 저장소 연결됨");
    }
    public async Task<CommunicationGitResult> SynchronizeAsync(string path, IEnumerable<string> validatedPaths, bool publish, CancellationToken ct = default)
    {
        if (!Owned(path)) return new(false, false, "전용 소통 저장소가 아니므로 자동 업로드를 보류했습니다.");
        if (!await HasExpectedOrigin(path, ct)) return new(false, false, "소통 저장소의 연결 주소가 변경되어 자동 업로드를 보류했습니다.");
        var paths = validatedPaths.Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
        if (paths.Any(p => !IsPublishablePath(p))) return new(false, false, "검증 대상 밖 경로가 있어 자동 업로드를 보류했습니다.");
        var status = await Git(path, ct, "status", "--porcelain=v1", "-z", "--untracked-files=all");
        if (status.Code != 0) return new(false, false, "소통 저장소 상태 확인 실패");
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
                if (add.Code != 0) return new(false, false, "수집 파일의 GitHub 반영 준비 실패");
            }
            var committed = await Git(path, ct, "commit", "-m", "Collect project communication records");
            if (committed.Code != 0) return new(false, false, "소통 기록 커밋 대기");
        }
        var fetched = await Git(path, ct, "fetch", "origin", "main");
        if (fetched.Code != 0) return new(false, false, "로컬 수집 완료 · GitHub 연결 대기");
        var rebased = await Git(path, ct, "rebase", "origin/main");
        if (rebased.Code != 0)
        {
            await Git(path, ct, "rebase", "--abort");
            return new(false, false, "다른 수정과 충돌하여 중앙 동기화를 보류했습니다. 수집 기록은 보존했습니다.");
        }
        var ahead = await Git(path, ct, "rev-list", "--count", "origin/main..HEAD");
        var count = int.TryParse(ahead.Output.Trim(), out var n) ? n : 0;
        if (count == 0) return new(true, false, "최신 공지와 기록이 연결되어 있습니다.");
        if (!publish) return new(true, false, "공지 내려받음 · 업로드할 기록은 로컬 대기 중");
        // Every outgoing commit must touch only communication-generated paths, including retried commits.
        var outgoing = await Git(path, ct, "rev-list", "origin/main..HEAD");
        if (outgoing.Code != 0) return new(false, false, "중앙 업로드 커밋 확인 실패");
        foreach (var commit in outgoing.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var changes = await Git(path, ct, "diff-tree", "--root", "-m", "--no-commit-id", "--name-only", "-r", "-z", commit.Trim());
            if (changes.Code != 0 || changes.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries).Any(p => !IsPublishablePath(p))) return new(false, false, "중앙 업로드 범위 밖 커밋을 발견해 보류했습니다.");
            foreach (var relative in changes.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries).Distinct())
            {
                var size = await Git(path, ct, "cat-file", "-s", commit.Trim() + ":" + relative);
                if (size.Code != 0) continue; // A deletion contains no new blob.
                if (!int.TryParse(size.Output.Trim(), out var bytes) || bytes > CommunicationService.MaximumFileBytes) return new(false, false, "크기 제한을 넘는 소통 기록을 보류했습니다.");
                var blob = await Git(path, ct, "show", commit.Trim() + ":" + relative);
                if (blob.Code != 0 || CommunicationService.ContainsSensitiveText(blob.Output)) return new(false, false, "비밀값 의심 기록의 GitHub 업로드를 보류했습니다.");
            }
        }
        var pushed = await Git(path, ct, "push", _origin, "HEAD:main");
        return pushed.Code == 0 ? new(true, true, "GitHub에 수집 기록을 반영했습니다.") : new(false, false, "수집 기록은 보존됨 · GitHub 업로드 재시도 대기");
    }
    private async Task<bool> HasExpectedOrigin(string path, CancellationToken ct)
    {
        foreach(var push in new[]{false,true})
        {
            var arguments=push ? new[]{"remote","get-url","--push","--all","origin"} : new[]{"remote","get-url","--all","origin"};
            var result=await Git(path,ct,arguments);
            var urls=result.Output.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries);
            if(result.Code!=0 || urls.Length!=1 || urls[0]!=_origin) return false;
        }
        return true;
    }
    private static async Task<(int Code, string Output)> Git(string? root, CancellationToken ct, params string[] arguments)
    {
        using var process = new Process { StartInfo = new ProcessStartInfo("git") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8 } };
        process.StartInfo.ArgumentList.Add("-c"); process.StartInfo.ArgumentList.Add("core.hooksPath=" + Path.Combine(Path.GetTempPath(), "control-tower-no-hooks"));
        process.StartInfo.ArgumentList.Add("-c"); process.StartInfo.ArgumentList.Add("core.quotePath=false");
        if (root is not null) { process.StartInfo.ArgumentList.Add("-C"); process.StartInfo.ArgumentList.Add(root); }
        foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        process.StartInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
        process.StartInfo.Environment["GCM_INTERACTIVE"] = "Never";
        process.StartInfo.Environment["GIT_EDITOR"] = "true";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(25));
        try
        {
            process.Start(); var output = process.StandardOutput.ReadToEndAsync(ct); var error = process.StandardError.ReadToEndAsync(ct);
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { if (!process.HasExited) process.Kill(true); await process.WaitForExitAsync(CancellationToken.None); return (-1, ""); }
            var text = await output; await error; return (process.ExitCode, text);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or OperationCanceledException) { return (-1, ""); }
    }
}
