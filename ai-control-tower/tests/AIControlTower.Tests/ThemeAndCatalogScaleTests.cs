using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class ThemeAndCatalogScaleTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ContentAndSelectionTextRemainReadableInBothThemes(bool dark)
    {
        var palette = ThemeService.Palette(dark);
        var textKeys = new[] { "TextBrush", "MutedBrush", "AccentBrush" };
        var surfaceKeys = new[] { "CanvasBrush", "SurfaceBrush", "RaisedBrush", "HeroBrush", "SelectionBrush", "HoverBrush" };
        foreach (var textKey in textKeys)
        {
            foreach (var surfaceKey in surfaceKeys)
                AssertContrast(palette[textKey], palette[surfaceKey], $"{dark}/{textKey}/{surfaceKey}");
            foreach (var gradientKey in new[] { "CardFace", "ButtonFace", "SelectionFace", "HeroFace" })
                AssertGradientContrast(palette[textKey], ThemeService.Gradients(dark)[gradientKey], $"{dark}/{textKey}/{gradientKey}");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RailLabelsAndColoredChipsHaveTheirOwnReadableInk(bool dark)
    {
        var palette = ThemeService.Palette(dark);
        foreach (var textKey in new[] { "RailTextBrush", "RailMutedBrush" })
        {
            foreach (var surfaceKey in new[] { "RailBrush", "RailHoverBrush", "RailSelectedBrush" })
                AssertContrast(palette[textKey], palette[surfaceKey], $"{dark}/{textKey}/{surfaceKey}");
            AssertGradientContrast(palette[textKey], ThemeService.Gradients(dark)["RailMaterial"], $"{dark}/{textKey}/RailMaterial");
        }
        AssertContrast(palette["ChipInkBrush"], palette["ChipBrush"], $"{dark}/chip");
        AssertContrast(palette["BadgeInkBrush"], palette["BadgeBrush"], $"{dark}/badge");
        AssertGradientContrast(palette["AccentInkBrush"], ThemeService.Gradients(dark)["ActionFace"], $"{dark}/action");
    }

    [Fact]
    public void TwoHundredProjectsRemainSearchableWithoutLosingOriginalObjects()
    {
        using var vm = new MainViewModel(new ControlTowerSettings
        {
            IsTemporary = true,
            AutoCommunication = false,
            AutoPublishCommunication = false,
            RootPath = Path.Combine(Path.GetTempPath(), "tower-scale-test-" + Guid.NewGuid().ToString("N"))
        }, enablePolling: false);
        var originals = Enumerable.Range(1, 200).Select(CreateProject).ToArray();
        foreach (var project in originals) vm.Projects.Add(project);

        Assert.Equal(200, vm.FilteredProjects.Count);
        for (var i = 0; i < originals.Length; i++) Assert.Same(originals[i], vm.FilteredProjects[i]);
        var last = originals[^1];
        Assert.Same(last, vm.FilteredProjects[^1]);
        foreach (var query in new[] { last.DisplayName, last.Description, last.Path, last.Name })
        {
            vm.ProjectSearch = "  " + query + "  ";
            Assert.Same(last, Assert.Single(vm.FilteredProjects));
            Assert.False(vm.NoProjectSearchResults);
        }

        vm.SelectedProject = Assert.Single(vm.FilteredProjects);
        var program = Assert.Single(last.Functions).Programs.Single();
        Assert.Same(program, vm.SelectedProgram);
        foreach (var query in new[] { program.DisplayName, program.Description, "문서출력" })
        {
            vm.ProgramSearch = query;
            Assert.Same(program, Assert.Single(Assert.Single(vm.FilteredFunctions).Programs));
            Assert.False(vm.NoProgramSearchResults);
        }

        vm.ProjectSearch = "일치하지 않는 프로젝트 검색어";
        Assert.Empty(vm.FilteredProjects);
        Assert.True(vm.NoProjectSearchResults);
        vm.ProjectSearch = "";
        Assert.Equal(200, vm.FilteredProjects.Count);
        Assert.Same(last, vm.FilteredProjects[^1]);
        Assert.Same(last, vm.SelectedProject);
        Assert.Same(program, vm.SelectedProgram);
    }

    private static ProjectItem CreateProject(int number)
    {
        var id = $"isolated-{number:000}";
        var program = new ProgramItem
        {
            Id = id + "/export", ProjectId = id,
            Name = "export", DisplayName = $"결과 문서 내보내기 {number:000}",
            Description = $"검토가 끝난 결과와 첨부 자료를 함께 정리합니다 {number:000}",
            KindLabel = "문서출력"
        };
        return new ProjectItem
        {
            Id = id, Name = $"internal-project-{number:000}",
            DisplayName = $"프로젝트 {number:000} 여러 사람이 읽어도 무엇을 만드는지 바로 알 수 있는 긴 한글 이름",
            Description = $"자료와 결과를 구분해서 관리하는 제작 작업 {number:000}",
            Path = Path.Combine("isolated-projects", $"작업폴더-{number:000}"),
            Functions = [new FunctionItem { Id = "documents", Name = "문서 작업", Programs = [program] }]
        };
    }

    private static void AssertGradientContrast(string foreground, IReadOnlyList<string> stops, string state)
    {
        Assert.True(stops.Count >= 2, "A gradient must contain at least two color stops.");
        for (var segment = 0; segment < stops.Count - 1; segment++)
        {
            var start = Channels(stops[segment]);
            var end = Channels(stops[segment + 1]);
            // Include endpoints and the rendered sRGB interpolation, rather than just testing one stop.
            for (var step = 0; step <= 100; step++)
            {
                var fraction = step / 100d;
                var background = start.Zip(end, (a, b) => a + (b - a) * fraction).ToArray();
                AssertContrast(Channels(foreground), background, $"{state}/segment-{segment}/sample-{step}");
            }
        }
    }

    private static void AssertContrast(string foreground, string background, string state) =>
        AssertContrast(Channels(foreground), Channels(background), state);

    private static void AssertContrast(double[] foreground, double[] background, string state)
    {
        var first = Luminance(foreground);
        var second = Luminance(background);
        var ratio = (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
        Assert.True(ratio >= 4.5, $"Small text contrast fell below 4.5:1 in {state}: {ratio:F3}:1.");
    }

    private static double[] Channels(string hex)
    {
        Assert.Matches("^#[0-9A-Fa-f]{6}$", hex);
        return Enumerable.Range(0, 3).Select(i => Convert.ToInt32(hex.Substring(1 + i * 2, 2), 16) / 255d).ToArray();
    }

    private static double Luminance(double[] channels)
    {
        static double Linear(double value) => value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        return 0.2126 * Linear(channels[0]) + 0.7152 * Linear(channels[1]) + 0.0722 * Linear(channels[2]);
    }
}
