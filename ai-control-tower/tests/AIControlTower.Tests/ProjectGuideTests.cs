using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class ProjectGuideTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tower-test-guide-" + Guid.NewGuid().ToString("N"));
    public ProjectGuideTests() => Directory.CreateDirectory(_root);
    public void Dispose() => Directory.Delete(_root, true);

    [Fact]
    public void MissingGuideDoesNotInventLocationsOrCreateProjectFiles()
    {
        var result = ProjectGuideService.Read(_root);
        Assert.Equal("안내 미작성", result.Status);
        Assert.Equal("", result.Path);
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }
    [Fact]
    public void ExistingKoreanGuideIsReadVerbatimAndPreferredOverFallback()
    {
        var path = Path.Combine(_root, "00_사용안내.md");
        const string body = "# 사용 안내\n\n[음원 듣기](results/음원.mp3)\n";
        File.WriteAllText(path, body);
        File.WriteAllText(Path.Combine(_root, "PROJECT_GUIDE.md"), "이전 안내");
        var result = ProjectGuideService.Read(_root);
        Assert.Equal(path, result.Path); Assert.Equal(body, result.Body);
        Assert.Equal(body, File.ReadAllText(path));
    }
    [Fact]
    public void ResourceLinksRequireExistingSupportedFilesAndNeverLaunchScripts()
    {
        var audio = Path.Combine(_root, "음원.mp3");
        File.WriteAllText(audio, "isolated fixture");
        File.WriteAllText(Path.Combine(_root, "run.cmd"), "isolated fixture");
        Assert.Equal(audio, ProjectGuideService.ResolveResource(_root, "음원.mp3"));
        Assert.Null(ProjectGuideService.ResolveResource(_root, "run.cmd"));
        Assert.Null(ProjectGuideService.ResolveResource(_root, "없는음원.mp3"));
        Assert.Null(ProjectGuideService.ResolveResource(_root, "https://example.com/image.png", true));
        Assert.Null(ProjectGuideService.ResolveResource("", audio)); // collected messages have no project resource context
    }
    [Fact]
    public void OversizedGuideKeepsItsPathAndRequestsTheOriginalInsteadOfTruncating()
    {
        var path = Path.Combine(_root, "00_사용안내.txt");
        File.WriteAllText(path, new string('x', 1024 * 1024 + 1));
        var result = ProjectGuideService.Read(_root);
        Assert.Equal(path, result.Path); Assert.Equal("원본 열기 필요", result.Status);
        Assert.Equal(1024 * 1024 + 1, new FileInfo(path).Length);
    }
    [Fact]
    public void InvalidSavedRefreshValuesUseDefaultsAndSupportedSettingsRemainIndependent()
    {
        using var vm = new MainViewModel(new ControlTowerSettings { IsTemporary = true, AutoCommunication = false,
            AutoPublishCommunication = false, RosterRefreshSeconds = 0, RefreshTurnMilliseconds = -1 }, enablePolling: false);
        Assert.Equal(5, vm.RosterRefreshSeconds); Assert.Equal(1200, vm.RefreshTurnMilliseconds);
        vm.RosterRefreshSeconds = 1; vm.RefreshTurnMilliseconds = 2400;
        Assert.Equal(1, vm.RosterRefreshSeconds); Assert.Equal(2400, vm.RefreshTurnMilliseconds);
        vm.RosterRefreshSeconds = 0; vm.RefreshTurnMilliseconds = int.MaxValue;
        Assert.Equal(1, vm.RosterRefreshSeconds); Assert.Equal(2400, vm.RefreshTurnMilliseconds);
    }
}
