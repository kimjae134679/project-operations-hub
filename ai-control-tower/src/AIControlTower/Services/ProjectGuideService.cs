namespace AIControlTower.Services;

public sealed record ProjectGuide(string Path, string Body, string Status);

/// <summary>Reads the guide owned by the project. Does not generate or overwrite project facts.</summary>
public static class ProjectGuideService
{
    private static readonly string[] Candidates = ["00_사용안내.md", "00_사용안내.txt", "사용안내.md", "PROJECT_GUIDE.md", "00_START_HERE/사용안내.md", "00_START_HERE/USER_GUIDE.md"];
    internal static string? ResolveResource(string directory, string target, bool imageOnly = false)
    {
        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(target)) return null;
        try
        {
            if (Uri.TryCreate(target, UriKind.Absolute, out var uri))
            {
                if (!uri.IsFile) return null;
                target = uri.LocalPath;
            }
            var path = Path.GetFullPath(Path.IsPathRooted(target) ? target : Path.Combine(directory, Uri.UnescapeDataString(target)));
            if (!imageOnly && Directory.Exists(path)) return path;
            var extension = Path.GetExtension(path).ToLowerInvariant();
            var supported = imageOnly ? new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }
                : new[] { ".mp3", ".wav", ".ogg", ".flac", ".mp4", ".mkv", ".webm", ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".pdf", ".txt", ".md", ".json", ".html" };
            return supported.Contains(extension) && File.Exists(path) ? path : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) { return null; }
    }

    public static ProjectGuide Read(string root)
    {
        try
        {
            if (!Directory.Exists(root)) return new("", "프로젝트 폴더에 연결되지 않았습니다.", "폴더 미연결");
            foreach (var relative in Candidates)
            {
                var path = ProjectDiscoveryService.ResolveInside(root, relative);
                if (!File.Exists(path)) continue;
                if (new FileInfo(path).Length > 1024 * 1024) return new(path, "안내가 1MiB를 넘습니다. 원본 파일을 열어 확인하세요.", "원본 열기 필요");
                return new(path, File.ReadAllText(path), "프로젝트의 실제 안내 파일");
            }
            return new("", "# 사용 안내가 아직 없습니다\n\n이 프로젝트의 담당 AI가 실제 작업 폴더에 00_사용안내.md 또는 00_사용안내.txt를 작성해야 합니다.\n\n음원 듣는 곳, 비교·참고 자료, 입력·결과·보관 폴더와 실행 방법을 실제 위치를 확인해 설명합니다. 확인하지 못한 위치는 미확인으로 표시합니다.", "안내 미작성");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return new("", "안내 파일을 읽지 못했습니다.\n\n" + ProcessRunner.Sanitize(ex.Message), "읽기 실패");
        }
    }
}
