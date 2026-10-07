using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed record JevExecutionTarget(ProgramItem Program, string? IndependentWorkspaceRoot);

/// <summary>Explicit declared workspace scope, not a filesystem sandbox or AI/session orchestration.</summary>
public static class JevExecutionWorkspace
{
    public static string Normalize(string path)
    {
        if (!Path.IsPathFullyQualified(path)) throw new InvalidOperationException("절대 작업 폴더를 확인하세요.");
        var full = Path.GetFullPath(path);
        var trimmed = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return trimmed.Length < Path.GetPathRoot(full)!.Length ? Path.GetPathRoot(full)! : trimmed;
    }
    public static bool Same(string left, string right) => Normalize(left).Equals(Normalize(right), StringComparison.OrdinalIgnoreCase);
    public static bool Overlaps(string left, string right)
    {
        left = Normalize(left); right = Normalize(right);
        static string Prefix(string path) => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
        return left.Equals(right, StringComparison.OrdinalIgnoreCase)
            || left.StartsWith(Prefix(right), StringComparison.OrdinalIgnoreCase)
            || right.StartsWith(Prefix(left), StringComparison.OrdinalIgnoreCase);
    }
    public static string Validate(string root, string workspace)
    {
        root = Normalize(root); workspace = Normalize(workspace);
        if (!Directory.Exists(root) || !Directory.Exists(workspace)
            || root.Equals(Normalize(Path.GetPathRoot(root)!), StringComparison.OrdinalIgnoreCase)
            || !workspace.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || !JevExecutionEvidenceReader.NoReparse(root) || !JevExecutionEvidenceReader.NoReparse(workspace))
            throw new InvalidOperationException("등록 프로젝트 안의 별도 기존 작업 폴더를 선택하세요. 루트·외부·링크 폴더는 독립 실행하지 않습니다.");
        return workspace;
    }
    public static string ScopedProgramId(ProjectItem project, ProgramItem program)
    {
        if (program.ProjectId != project.Id || !program.CanLaunch) throw new InvalidOperationException("명령이 등록된 프로젝트 작업을 선택하세요.");
        var workspace = Validate(project.Path, program.WorkingDirectory);
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] { project.Id, program.Id, workspace.ToLowerInvariant() }));
        return project.Id + "/jev/" + Convert.ToHexString(SHA256.HashData(bytes))[..24].ToLowerInvariant();
    }
    public static JevExecutionTarget Resolve(ProjectItem? project, ProgramItem? selected, string projectPath, bool independent)
    {
        if (independent)
        {
            if (project is null || selected is null || !Same(project.Path, projectPath)
                || !project.Functions.SelectMany(f => f.Programs).Any(p => ReferenceEquals(p, selected)))
                throw new InvalidOperationException("선택한 프로젝트의 등록 작업 폴더를 선택하세요.");
            var workspace = Validate(project.Path, selected.WorkingDirectory);
            return new(new ProgramItem { Id = ScopedProgramId(project, selected), ProjectId = project.Id,
                Name = selected.Name + " · Jev", Kind = "Jev", Path = workspace, WorkingDirectory = workspace }, Normalize(project.Path));
        }
        var path = Normalize(projectPath);
        var projectId = project is not null && Same(project.Path, path) ? project.Id : "folder:" + path.ToLowerInvariant();
        return new(new ProgramItem { Id = projectId + "/jev", ProjectId = projectId, Name = "Jev · Codex 작업",
            Kind = "Jev", Path = path, WorkingDirectory = path }, null);
    }
    public static bool MatchesScopedProgram(ProjectItem project, string id)
    {
        foreach (var program in project.Functions.SelectMany(f => f.Programs))
        {
            try { if (ScopedProgramId(project, program) == id) return true; }
            catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException) { }
        }
        return false;
    }
}
