using System.Reflection;
using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Reviewed human labels and selected entry points; it never writes into project folders or starts them.</summary>
public sealed class ProjectCatalogService
{
    public static CatalogDefinition Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith("project.catalog.json"));
        using var stream = assembly.GetManifestResourceStream(name)!;
        return JsonSerializer.Deserialize<CatalogDefinition>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
    }

    public IReadOnlyList<ProjectItem> Apply(IReadOnlyList<ProjectItem> discovered, string root, CatalogDefinition catalog, IList<string>? warnings = null)
    {
        if (catalog.SchemaVersion != 1) throw new InvalidDataException("지원하지 않는 프로젝트 소개 형식입니다.");
        var anchor = SamePath(root, catalog.AnchorRoot);
        var result = discovered.ToList();
        foreach (var entry in catalog.Projects)
        {
            if (!anchor && !Inside(root, entry.Path)) continue;
            if (!Directory.Exists(entry.Path)) continue;
            var registered = result.FirstOrDefault(p => p.ManifestPath.Length > 0 && p.Id.Equals(entry.Id, StringComparison.OrdinalIgnoreCase));
            if (registered is not null && !SamePath(registered.Path, entry.Path))
            {
                warnings?.Add("등록 manifest의 실제 위치를 유지했습니다 · " + entry.Id);
                result.RemoveAll(p => SamePath(p.Path, entry.Path) && p.ManifestPath.Length == 0);
                continue;
            }
            var original = result.FirstOrDefault(p => SamePath(p.Path, entry.Path));
            var id = original?.Id ?? entry.Id;
            var keepManifest = original is not null && (original.ManifestPath.Length > 0 || entry.PreserveManifest);
            // Do not parse stale catalog paths/commands when the owner manifest is authoritative.
            var functions = keepManifest ? original!.Functions.ToList() : entry.Functions.Select(f => new FunctionItem
            {
                Id = f.Id, Name = f.Name, IsAdvanced = f.IsAdvanced,
                Programs = f.Programs.Select(p => MakeProgram(entry.Path, id, p)).Where(p => p is not null).Cast<ProgramItem>().ToArray()
            }).Where(f => f.Programs.Count > 0).ToList();
            if (keepManifest) warnings?.Add("프로젝트 담당 manifest의 기능을 유지했습니다 · " + original!.Id);
            var related = new List<ProgramItem>();
            foreach (var folder in entry.RelatedFolders.Where(f => Directory.Exists(f.Path)))
            {
                result.RemoveAll(p => SamePath(p.Path, folder.Path) && p.ManifestPath.Length == 0);
                related.Add(new() { Id = id + "/related/" + folder.Id, ProjectId = id, Name = folder.Name, DisplayName = folder.Name,
                    Description = folder.Description, Detail = folder.Description, KindLabel = folder.RoleLabel, Kind = "folder",
                    Path = folder.Path, WorkingDirectory = folder.Path, ActionLabel = "폴더 보기", IsAdvanced = true });
            }
            if (related.Count > 0) functions.Add(new() { Id = "related-folders", Name = "연결된 폴더·보관 자료", IsAdvanced = true, Programs = related });
            result.RemoveAll(p => SamePath(p.Path, entry.Path));
            if (entry.Role == "planned" && !keepManifest)
                foreach (var p in functions.SelectMany(f => f.Programs))
                    p.Description = Directory.EnumerateFileSystemEntries(entry.Path).Any()
                        ? "새 파일이 들어 있는 준비 폴더입니다. 실행 기능은 아직 연결되지 않았습니다."
                        : "현재 비어 있는 준비 폴더입니다. 실행할 프로그램은 없습니다.";
            result.Add(new() { Id = id, Name = original?.Name ?? entry.Name, DisplayName = entry.Name, Path = entry.Path,
                Summary = entry.Description, Description = entry.Description, FolderRole = entry.Role, RoleLabel = entry.RoleLabel,
                CategoryLabel = entry.Category, EvidencePath = entry.EvidencePath, EvidenceDate = entry.CheckedAt,
                ManifestPath = original?.ManifestPath ?? "", Functions = functions });
        }
        return result.OrderBy(p => p.FolderRole == "planned" ? 2 : p.CategoryLabel == "전체 관리" ? 1 : 0)
            .ThenBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    private static ProgramItem? MakeProgram(string root, string projectId, CatalogProgram spec)
    {
        var path = ProjectDiscoveryService.ResolveInside(root, spec.Path);
        if (!string.IsNullOrEmpty(spec.LatestApkFolder))
        {
            var folder = ProjectDiscoveryService.ResolveInside(root, spec.LatestApkFolder);
            if (!Directory.Exists(folder)) return null;
            path = Directory.EnumerateFiles(folder, "PhoneLOL-*.apk")
                .Select(p => new { Path = p, Version = Version.TryParse(System.IO.Path.GetFileNameWithoutExtension(p).Replace("PhoneLOL-", ""), out var v) ? v : new Version(0, 0) })
                .OrderByDescending(p => p.Version).Select(p => p.Path).FirstOrDefault() ?? path;
        }
        if (!File.Exists(path) && !Directory.Exists(path)) return null;
        var work = ProjectDiscoveryService.ResolveInside(root, spec.WorkingDirectory);
        var commands = spec.Commands.Where(c => c.Arguments is not null && c.TimeoutSeconds is >= 0 and <= 86400 && !string.IsNullOrWhiteSpace(c.FileName))
            .Where(c => File.Exists(c.FileName) || EnvironmentProbe.FindCommand(c.FileName) is not null).ToArray();
        var editorScript = string.IsNullOrEmpty(spec.EditorOpenScript) ? "" : ProjectDiscoveryService.ResolveInside(root, spec.EditorOpenScript);
        if (!File.Exists(editorScript) || !File.Exists(spec.EditorExecutable)) editorScript = "";
        return new() { Id = projectId + "/" + spec.Id, ProjectId = projectId, Name = spec.Name, DisplayName = spec.Name,
            Description = spec.Description, Detail = spec.Description, Kind = spec.KindLabel, KindLabel = spec.KindLabel,
            Path = path, WorkingDirectory = work, Commands = commands, ServiceUrl = spec.ServiceUrl, EditorOpenScript = editorScript,
            ActionLabel = commands.Length > 0 || editorScript.Length > 0 ? spec.ActionLabel : spec.OpenActionLabel, OpenActionLabel = spec.OpenActionLabel, IsAdvanced = spec.IsAdvanced };
    }
    private static bool SamePath(string left, string right) => !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right)
        && System.IO.Path.GetFullPath(left).TrimEnd('\\', '/').Equals(System.IO.Path.GetFullPath(right).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
    private static bool Inside(string root, string path) => SamePath(root, path) || System.IO.Path.GetFullPath(path).StartsWith(System.IO.Path.GetFullPath(root).TrimEnd('\\', '/') + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}

public sealed class CatalogDefinition
{
    public int SchemaVersion { get; set; } = 1;
    public string AnchorRoot { get; set; } = "";
    public List<CatalogProject> Projects { get; set; } = [];
}
public sealed class CatalogProject
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Description { get; set; } = "";
    public string Role { get; set; } = "active";
    public string RoleLabel { get; set; } = "주 작업 폴더";
    public string Category { get; set; } = "";
    public string EvidencePath { get; set; } = "";
    public string CheckedAt { get; set; } = "";
    public bool PreserveManifest { get; set; }
    public List<CatalogFunction> Functions { get; set; } = [];
    public List<CatalogRelatedFolder> RelatedFolders { get; set; } = [];
}
public sealed class CatalogFunction
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsAdvanced { get; set; }
    public List<CatalogProgram> Programs { get; set; } = [];
}
public sealed class CatalogProgram
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Path { get; set; } = ".";
    public string WorkingDirectory { get; set; } = ".";
    public string Description { get; set; } = "";
    public string KindLabel { get; set; } = "파일·폴더";
    public string ActionLabel { get; set; } = "실행";
    public string OpenActionLabel { get; set; } = "폴더 보기";
    public string ServiceUrl { get; set; } = "";
    public string LatestApkFolder { get; set; } = "";
    public string EditorOpenScript { get; set; } = "";
    public string EditorExecutable { get; set; } = "";
    public bool IsAdvanced { get; set; }
    public List<ProgramCommand> Commands { get; set; } = [];
}
public sealed class CatalogRelatedFolder
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Description { get; set; } = "";
    public string RoleLabel { get; set; } = "보관 자료";
}
