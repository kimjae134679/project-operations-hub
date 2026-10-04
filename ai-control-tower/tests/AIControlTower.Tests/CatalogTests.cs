using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class CatalogTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tower-catalog-" + Guid.NewGuid().ToString("N"));
    public CatalogTests() => Directory.CreateDirectory(_root);
    [Fact]
    public void GroupsRelatedFoldersWithoutMovingFilesOrAddingExecutionCommands()
    {
        var main = Directory.CreateDirectory(Path.Combine(_root, "current")).FullName;
        var old = Directory.CreateDirectory(Path.Combine(_root, "old")).FullName;
        var evidence = Path.Combine(old, "keep.txt"); File.WriteAllText(evidence, "original");
        var raw = new[] { new ProjectItem { Id = "current", Name = "current", Path = main }, new ProjectItem { Id = "old", Name = "old", Path = old } };
        var catalog = new CatalogDefinition { AnchorRoot = _root, Projects = [new() { Id = "reviewed", Path = main, Name = "현재 작업", Description = "실제 작업", RelatedFolders = [new() { Id = "archive", Name = "이전 자료", Path = old, Description = "이전 사본" }] }] };
        var result = new ProjectCatalogService().Apply(raw, _root, catalog);
        var project = Assert.Single(result); Assert.Equal("현재 작업", project.DisplayName);
        var program = Assert.Single(Assert.Single(project.Functions).Programs);
        Assert.Equal(old, program.Path); Assert.False(program.CanLaunch); Assert.True(project.Functions[0].IsAdvanced);
        Assert.Equal("original", File.ReadAllText(evidence));
        Assert.Equal(2, raw.Length);
    }
    [Fact]
    public void LatestApkUsesVersionOrderAndDoesNotClaimAcceptanceOrExecuteIt()
    {
        Directory.CreateDirectory(Path.Combine(_root, "Builds"));
        foreach (var version in new[] { "0.9.0", "0.23.4", "0.23.5" }) File.WriteAllText(Path.Combine(_root, "Builds", "PhoneLOL-" + version + ".apk"), "artifact");
        var catalog = new CatalogDefinition
        {
            AnchorRoot = _root,
            Projects = [new()
            {
                Id = "game", Path = _root, Name = "게임",
                Functions = [new()
                {
                    Id = "apk", Name = "설치파일",
                    Programs = [new() { Id = "latest", Name = "최근 작업본", Path = "Builds/PhoneLOL-0.23.4.apk", LatestApkFolder = "Builds" }]
                }]
            }]
        };
        var program = new ProjectCatalogService().Apply([], _root, catalog).Single().Functions.Single().Programs.Single();
        Assert.EndsWith("PhoneLOL-0.23.5.apk", program.Path); Assert.False(program.CanLaunch);
        Assert.DoesNotContain("확인된", program.DisplayName);
    }
    [Fact]
    public void DifferentSelectedRootDoesNotImportUnrelatedCatalogProjects()
    {
        var unrelated = Directory.CreateDirectory(Path.Combine(_root, "selected")).FullName;
        var elsewhere = Directory.CreateDirectory(Path.Combine(_root, "other")).FullName;
        var catalog = new CatalogDefinition { AnchorRoot = elsewhere, Projects = [new() { Id = "other", Path = elsewhere, Name = "다른 프로젝트" }] };
        Assert.Empty(new ProjectCatalogService().Apply([], unrelated, catalog));
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EditorLinkRequiresInstalledEditorAndRetainsIndependentLifetime(bool installed)
    {
        var executable = Path.Combine(_root, "editor.exe");
        if (installed) File.WriteAllText(executable, "fixture; never executed");
        File.WriteAllText(Path.Combine(_root, "OpenEditor.ps1"), "fixture; never executed");
        var catalog = new CatalogDefinition { AnchorRoot = _root, Projects = [new()
        {
            Id = "game", Path = _root, Functions = [new() { Id = "edit", Programs = [new()
            {
                Id = "editor", EditorExecutable = executable, EditorOpenScript = "OpenEditor.ps1",
                ActionLabel = "편집기로 열기", OpenActionLabel = "폴더 보기"
            }] }]
        }] };
        var program = new ProjectCatalogService().Apply([], _root, catalog).Single().Functions.Single().Programs.Single();
        Assert.Equal(installed, program.HasEditorLauncher);
        Assert.False(program.CanLaunch); // Editor sessions are not owned jobs killed by app shutdown.
        Assert.Equal(installed ? "편집기로 열기" : "폴더 보기", program.ActionLabel);
    }
    public void Dispose() { try { Directory.Delete(_root, true); } catch (IOException) { } }
}
