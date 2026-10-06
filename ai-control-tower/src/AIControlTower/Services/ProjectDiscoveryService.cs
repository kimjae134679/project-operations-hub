using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed record DiscoveryResult(IReadOnlyList<ProjectItem> Projects, IReadOnlyList<string> Warnings);

public sealed class ProjectDiscoveryService
{
    public const string ManifestName = "project.control.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip };
    private static readonly HashSet<string> Ignored = new(StringComparer.OrdinalIgnoreCase)
    { ".git", "node_modules", ".venv", "venv", "__pycache__", "bin", "obj", "backups", "cache", "Installers", "99_ARCHIVE", "Intermediate", "Saved", ".next", "ControlTowerData", "_통합소통", "Models", "Workspace", "Data", "Scripts", "Launchers", "Tools", "Applications" };

    public DiscoveryResult Scan(string root, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(root)) return new([], ["탐색 루트가 없습니다: " + root]);
        var projects = new List<ProjectItem>();
        var warnings = new List<string>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Visit(string directory, int depth)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0) return;
                var manifest = System.IO.Path.Combine(directory, ManifestName);
                var marker = DetectKind(directory);
                var container = new[] { "Applications", "Projects", "Workspace", "External", "Programs" }.Contains(System.IO.Path.GetFileName(directory), StringComparer.OrdinalIgnoreCase);
                if (File.Exists(manifest) || marker is not null || (depth == 1 && !container))
                {
                    var item = File.Exists(manifest) ? ReadManifest(directory, manifest) : DiscoverCandidate(directory, marker ?? "폴더 · 등록 대기");
                    if (ids.Add(item.Id)) projects.Add(item);
                    else warnings.Add("중복 프로젝트 ID를 등록하지 않았습니다: " + item.Id + " · " + directory);
                    return; // 프로젝트 안의 dependencies는 별도 프로젝트로 등록하지 않습니다.
                }
                if (depth >= 3) return;
                foreach (var child in Directory.EnumerateDirectories(directory).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    if (!Ignored.Contains(System.IO.Path.GetFileName(child))) Visit(child, depth + 1);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or ArgumentException)
            { warnings.Add(System.IO.Path.GetFileName(directory) + ": " + ProcessRunner.Sanitize(ex.Message)); }
        }
        Visit(System.IO.Path.GetFullPath(root), 0);
        return new(projects.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToArray(), warnings);
    }

    public ProjectItem ReadManifest(string directory, string manifest)
    {
        if (new FileInfo(manifest).Length > 256 * 1024) throw new InvalidDataException("manifest는 256KB 이하로 작성하세요.");
        var config = JsonSerializer.Deserialize<ProjectManifest>(File.ReadAllText(manifest), JsonOptions) ?? throw new InvalidDataException("빈 manifest");
        if (config.SchemaVersion != 1) throw new InvalidDataException("지원하지 않는 schemaVersion: " + config.SchemaVersion);
        if (string.IsNullOrWhiteSpace(config.Id) || string.IsNullOrWhiteSpace(config.Name)) throw new InvalidDataException("프로젝트 id/name이 필요합니다.");
        if (config.Functions is null || config.Functions.Any(f => f is null || f.Programs is null || f.Programs.Any(p => p is null || p.Commands is null || p.Commands.Any(c => c is null))))
            throw new InvalidDataException("functions/programs/commands 배열과 요소는 null일 수 없습니다.");
        var programIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var functionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var functions = config.Functions.Select(function =>
        {
            if (string.IsNullOrWhiteSpace(function.Id) || !functionIds.Add(function.Id)) throw new InvalidDataException("기능 id는 비어 있거나 중복될 수 없습니다.");
            return new FunctionItem
            {
                Id = function.Id, Name = string.IsNullOrWhiteSpace(function.Name) ? function.Id : function.Name,
                Programs = function.Programs.Select(program =>
                {
                    if (string.IsNullOrWhiteSpace(program.Id) || !programIds.Add(program.Id)) throw new InvalidDataException("프로그램 id는 프로젝트 안에서 유일해야 합니다.");
                    var work = ResolveInside(directory, program.WorkingDirectory);
                    var path = ResolveInside(directory, program.Path);
                    if (program.Commands.Any(c => string.IsNullOrWhiteSpace(c.FileName) || c.TimeoutSeconds is < 0 or > 86400 || c.Arguments is null || c.Arguments.Any(a => a is null)))
                        throw new InvalidDataException("명령 fileName/arguments/timeoutSeconds를 확인하세요.");
                    return new ProgramItem
                    {
                        Id = config.Id + "/" + program.Id, ProjectId = config.Id,
                        Name = string.IsNullOrWhiteSpace(program.Name) ? program.Id : program.Name,
                        Kind = program.Kind, Detail = program.Description, Path = path,
                        WorkingDirectory = work, Commands = program.Commands,
                        Status = "등록됨 · 실행 전"
                    };
                }).ToArray()
            };
        }).ToArray();
        return new() { Id = config.Id, Name = config.Name, Path = directory, Summary = config.Description, ManifestPath = manifest, Functions = functions };
    }

    public static string ResolveInside(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || System.IO.Path.IsPathRooted(relative)) throw new InvalidDataException("프로젝트 내부 상대 경로를 사용하세요.");
        var fullRoot = System.IO.Path.GetFullPath(root).TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
        var result = System.IO.Path.GetFullPath(System.IO.Path.Combine(fullRoot, relative));
        if (!result.Equals(fullRoot.TrimEnd(System.IO.Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)
            && !result.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("프로젝트 밖 경로는 등록할 수 없습니다.");
        return result;
    }

    private static string? DetectKind(string directory)
    {
        if (Directory.Exists(System.IO.Path.Combine(directory, ".git")) || File.Exists(System.IO.Path.Combine(directory, ".git"))) return "Git workspace";
        if (File.Exists(System.IO.Path.Combine(directory, "package.json"))) return "Node / Web";
        if (File.Exists(System.IO.Path.Combine(directory, "pyproject.toml")) || File.Exists(System.IO.Path.Combine(directory, "requirements.txt"))) return "Python";
        if (Directory.EnumerateFiles(directory, "*.csproj").Any() || Directory.EnumerateFiles(directory, "*.sln").Any()) return ".NET";
        if (Directory.EnumerateFiles(directory, "*.uproject").Any()) return "Unreal";
        if (Directory.EnumerateFiles(directory, "*.exe").Any()) return "Windows 프로그램";
        return null;
    }

    private static ProjectItem DiscoverCandidate(string directory, string kind)
    {
        var id = "folder:" + directory.ToLowerInvariant();
        var program = new ProgramItem
        {
            Id = id + "/source", ProjectId = id, Name = "프로젝트 폴더", Kind = kind, KindLabel = "작업 폴더", ActionLabel = "폴더 보기",
            Detail = "새로 발견한 폴더입니다. 작업 목적과 실행 방법을 확인해야 합니다.", Description = "새로 발견한 폴더입니다. 작업 목적과 실행 방법을 아직 확인하지 않았습니다.",
            Path = directory, WorkingDirectory = directory, Status = "자동 발견 · 명령 미등록"
        };
        var functions = new List<FunctionItem> { new() { Id = "workspace", Name = "작업 공간", Programs = [program] } };
        var artifacts = DiscoverArtifacts(directory, id);
        if (artifacts.Count > 0) functions.Add(new() { Id = "artifacts", Name = "프로그램 및 산출물", Programs = artifacts });
        return new() { Id = id, Name = System.IO.Path.GetFileName(directory), Path = directory, Summary = kind + " · 자동 발견",
            Functions = functions };
    }

    private static IReadOnlyList<ProgramItem> DiscoverArtifacts(string directory, string projectId)
    {
        var items = new List<ProgramItem>();
        var foldersVisited = 0;
        void Visit(string folder, int depth)
        {
            if (++foldersVisited > 300 || items.Count >= 40) return;
            try
            {
                if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != 0) return;
                foreach (var file in Directory.EnumerateFiles(folder).Where(f => System.IO.Path.GetExtension(f).ToLowerInvariant() is ".exe" or ".apk" or ".aab").Take(40 - items.Count))
                {
                    var executable = System.IO.Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase);
                    items.Add(new() { Id = projectId + "/" + System.IO.Path.GetRelativePath(directory, file), ProjectId = projectId,
                        Name = System.IO.Path.GetFileName(file), Kind = executable ? "Windows 실행파일 · 발견 후보" : "Android 설치 산출물", KindLabel = executable ? "용도 확인이 필요한 파일" : "휴대폰 설치파일", ActionLabel = "파일 위치 보기", Description = executable ? "실행파일이 발견됐지만 어떤 프로그램인지 아직 확인하지 않았습니다." : "휴대폰에 설치하는 파일입니다. PC에서 실행하는 프로그램이 아닙니다.",
                        Detail = "파일 존재를 확인했습니다. 정상 동작·설치·최신 버전 여부는 별도 검증이 필요합니다.",
                        Path = file, WorkingDirectory = System.IO.Path.GetDirectoryName(file)!, Status = "발견됨 · 검증 전",
                        Commands = [] });
                }
                if (depth >= 4) return;
                foreach (var child in Directory.EnumerateDirectories(folder).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    var name = System.IO.Path.GetFileName(child);
                    if (Ignored.Contains(name) && name is not "bin") continue;
                    if (new[] { "Content", "assets", "images", "reference", "vendor", "Engine", ".codex", "Source" }.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
                    Visit(child, depth + 1);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        Visit(directory, 0);
        return items;
    }
}
