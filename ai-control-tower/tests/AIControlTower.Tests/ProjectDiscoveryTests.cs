using AIControlTower.Services;
namespace AIControlTower.Tests;
public sealed class ProjectDiscoveryTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "control-tower-tests-" + Guid.NewGuid().ToString("N"));
    public ProjectDiscoveryTests() => Directory.CreateDirectory(_root);
    [Fact]
    public void DiscoversGitAndEmptyFoldersWithoutAutoCommands()
    {
        Directory.CreateDirectory(Path.Combine(_root, "한글 프로젝트", ".git"));
        Directory.CreateDirectory(Path.Combine(_root, "새 작업"));
        var result = new ProjectDiscoveryService().Scan(_root);
        Assert.Equal(2, result.Projects.Count);
        Assert.All(result.Projects, p => Assert.All(p.Functions.SelectMany(f => f.Programs), x => Assert.False(x.CanLaunch)));
    }
    [Fact]
    public void ManifestPreservesFunctionProgramCommandHierarchyAndArguments()
    {
        var project = Path.Combine(_root, "project"); Directory.CreateDirectory(project);
        File.WriteAllText(Path.Combine(project, "project.control.json"), """
            {"schemaVersion":1,"id":"sample","name":"프로젝트","functions":[{"id":"export","name":"내보내기","programs":[{"id":"tool","name":"생성기","path":".","workingDirectory":".","commands":[{"name":"검사","fileName":"node.exe","arguments":["text ; & \"quotes\""]}]}]}]}
            """);
        var result = new ProjectDiscoveryService().Scan(_root);
        var program = Assert.Single(Assert.Single(Assert.Single(result.Projects).Functions).Programs);
        Assert.Equal("sample/tool", program.Id);
        Assert.Equal("text ; & \"quotes\"", Assert.Single(Assert.Single(program.Commands).Arguments));
    }
    [Fact]
    public void InvalidAndDuplicateManifestsAreReportedWithoutStoppingOtherProjects()
    {
        foreach (var name in new[] { "one", "two", "invalid" }) Directory.CreateDirectory(Path.Combine(_root, name));
        foreach (var name in new[] { "one", "two" }) File.WriteAllText(Path.Combine(_root, name, "project.control.json"), "{\"id\":\"same\",\"name\":\"A\"}");
        File.WriteAllText(Path.Combine(_root, "invalid", "project.control.json"), "broken");
        var result = new ProjectDiscoveryService().Scan(_root);
        Assert.Single(result.Projects); Assert.Equal(2, result.Warnings.Count);
    }
    [Theory]
    [InlineData("../outside")]
    [InlineData("../../outside")]
    public void RejectsPathEscape(string relative) => Assert.Throws<InvalidDataException>(() => ProjectDiscoveryService.ResolveInside(_root, relative));
    [Fact]
    public void MissingRootIsReportedNotInvented() => Assert.Single(new ProjectDiscoveryService().Scan(Path.Combine(_root, "missing")).Warnings);
    public void Dispose() => Directory.Delete(_root, true);
}
