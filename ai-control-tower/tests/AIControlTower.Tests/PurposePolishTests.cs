using System.Xml.Linq;
using Xunit;

namespace AIControlTower.Tests;

public sealed class PurposePolishTests
{
    private const string ToolsIntroduction = "원격 연결, AI 작업, 자동화와 제작 도구를 한곳에서 찾습니다.";
    private const string ProjectsIntroduction = "이름으로 찾고 작업을 이어가세요";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void ToolCardPurposeIsAvailableAsTooltipNotAVisibleParagraph()
    {
        var cards = ReadMainXaml().Descendants().Single(e => (string?)e.Attribute(X + "Name") == "ToolCards");
        Assert.DoesNotContain(cards.Descendants(), e => (string?)e.Attribute("Text") == "{Binding Purpose}");
        Assert.Contains(cards.Descendants(), e => (string?)e.Attribute("ToolTip") == "{Binding Purpose}");
    }

    [Fact]
    public void RedundantWorkspaceIntroductionsAreTooltipsNotSubtitles()
    {
        var root = ReadMainXaml();
        var tools = root.Descendants().Single(e => (string?)e.Attribute(X + "Name") == "ToolsTab");
        var projects = root.Descendants().Single(e => (string?)e.Attribute(X + "Name") == "ProjectsTab");
        foreach (var (scope, text) in new[] { (tools, ToolsIntroduction), (projects, ProjectsIntroduction) })
        {
            Assert.DoesNotContain(scope.Descendants(), e => (string?)e.Attribute("Text") == text);
            Assert.Contains(scope.Descendants(), e => (string?)e.Attribute("ToolTip") == text);
        }
    }

    [Fact]
    public void ShortActionsAndHonestStatusEvidenceRemainVisibleAndWired()
    {
        var root = ReadMainXaml();
        var cards = root.Descendants().Single(e => (string?)e.Attribute(X + "Name") == "ToolCards");
        Assert.Equal("{Binding FilteredToolStatuses}", (string?)cards.Attribute("ItemsSource"));
        Assert.Contains(cards.Descendants(), e => e.Name.LocalName == "StatusBadge" && (string?)e.Attribute("Value") == "{Binding StateLabel}");
        Assert.Contains(cards.Descendants(), e => (string?)e.Attribute("Text") == "{Binding UsageEvidence}");
        Assert.Contains(cards.Descendants(), e => e.Name.LocalName == "Button" &&
            (string?)e.Attribute("Content") == "{Binding ActionLabel}" &&
            (string?)e.Attribute("Click") == "OpenTool_Click" &&
            (string?)e.Attribute("AutomationProperties.Name") == "{Binding DisplayName}");
        Assert.Contains(cards.Descendants(), e => e.Name.LocalName == "Expander" &&
            (string?)e.Attribute("IsExpanded") == "False" && e.Descendants().Any(child => (string?)child.Attribute("Text") == "{Binding Detail}"));
        Assert.Equal("True", (string?)root.Descendants().Single(e => (string?)e.Attribute(X + "Name") == "ProjectsTab").Attribute("IsSelected"));
    }

    [Fact]
    public void JevActionRemainsReachableOutsideEvidenceFilteredToolCards()
    {
        var tools = ReadMainXaml().Descendants().Single(e => (string?)e.Attribute(X + "Name") == "ToolsTab");
        var button = Assert.Single(tools.Descendants(), e => e.Name.LocalName == "Button" && (string?)e.Attribute("Content") == "Jev 작업");
        Assert.Equal("Jev_Click", (string?)button.Attribute("Click"));
        Assert.Equal("{Binding CanRunRegisteredTasks}", (string?)button.Attribute("IsEnabled"));
        Assert.DoesNotContain(button.Ancestors(), e => (string?)e.Attribute(X + "Name") == "ToolCards");
    }

    private static XElement ReadMainXaml()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "ai-control-tower", "AIControlTower.sln"))) root = root.Parent;
        Assert.NotNull(root);
        return XDocument.Load(Path.Combine(root!.FullName, "ai-control-tower", "src", "AIControlTower", "MainWindow.xaml")).Root!;
    }
}
