using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using AIControlTower.Services;

namespace AIControlTower.Tests;

/// <summary>Production XAML fragments on detached STA controls. No App/MainWindow/VM construction or visible windows.</summary>
public sealed class UiLayoutRefinementTests
{
    private static readonly XNamespace X="http://schemas.microsoft.com/winfx/2006/xaml";
    [Fact]
    public void ProjectRailShowsTitlesAndOneNamedProjectSearch() => Sta(()=>
    {
        var rail=Fragment("ProjectRail");
        rail.DataContext=new { ProjectsCount=1,FilteredProjects=new[]{new { DisplayName="선택 프로젝트",Description="숨겨야 할 상세 설명",RoleLabel="숨겨야 할 내부 역할" } } };
        Layout(rail,238,600);
        var texts=Children(rail).OfType<TextBlock>().Select(t=>t.Text).ToArray();
        Assert.Contains("선택 프로젝트",texts);Assert.DoesNotContain("숨겨야 할 상세 설명",texts);Assert.DoesNotContain("숨겨야 할 내부 역할",texts);
        var search=Assert.Single(Children(rail).OfType<TextBox>());Assert.Equal("ProjectSearchBox",search.Name);Assert.True(search.ActualHeight>=40);
    });

    [Theory][InlineData(540)][InlineData(730)]
    public void HeroContentStaysInsetInsideActualFrontCardAtNarrowWidths(double width) => Sta(()=>
    {
        var hero=Fragment("ProjectHeader");hero.DataContext=new { SelectedProject=new { DisplayName="아주 긴 프로젝트 이름으로 줄바꿈과 테두리 내부 여백을 검증합니다",CategoryLabel="프로젝트",Description="긴 설명이 카드의 둥근 테두리를 넘지 않아야 합니다",Path=@"D:\A_KJ\AI\프로젝트" } };
        Layout(hero,width,400);
        var face=Children(hero).OfType<Border>().SingleOrDefault(b=>b.Name=="ProjectHeaderFace");Assert.NotNull(face);
        var body=Children(hero).OfType<FrameworkElement>().Single(g=>g.Name=="ProjectHeaderBody");
        var bounds=body.TransformToAncestor(face).TransformBounds(new Rect(body.RenderSize));
        Assert.True(bounds.Left>=8);Assert.True(bounds.Top>=8);
        Assert.True(bounds.Right<=face.ActualWidth-8);Assert.True(bounds.Bottom<=face.ActualHeight-8);
        foreach(var text in Children(body).OfType<TextBlock>().Where(t=>t.ActualHeight>0))
        {var box=text.TransformToAncestor(face).TransformBounds(new Rect(text.RenderSize));Assert.True(box.Left>=8);Assert.True(box.Right<=face.ActualWidth-8);}
    });

    [Theory][InlineData(430)][InlineData(1030)]
    public void ActualWorkspaceTemplateKeepsSelectedTabBorderInsideHorizontalViewport(double width) => Sta(()=>
    {
        var resources=Resources();var main=MainSource();
        var template=main.Descendants().Single(e=>Name(e)=="WorkspaceTabs").Elements().Single(e=>e.Name.LocalName=="TabControl.Template").Elements().Single();
        var tabs=new TabControl {Resources=resources,Template=Parse<ControlTemplate>(template),ItemContainerStyle=(Style)resources["WorkspaceTabItem"]};
        for(var i=0;i<9;i++)tabs.Items.Add(new TabItem{Header="프로젝트 작업 "+i});
        Layout(tabs,width,220);tabs.SelectedIndex=8;Layout(tabs,width,220);MainWindow.EnsureSelectedTabVisible(tabs);Layout(tabs,width,220);
        var scroll=Children(tabs).OfType<ScrollViewer>().Single(s=>s.Name=="TabHeaderScroll");
        var viewport=Children(scroll).OfType<ScrollContentPresenter>().First();
        var selected=(TabItem)tabs.Items[8];var border=Children(selected).OfType<Border>().Single(b=>b.Name=="TabTile");
        var bounds=border.TransformToAncestor(viewport).TransformBounds(new Rect(border.RenderSize));
        Assert.InRange(bounds.Left,0,viewport.ActualWidth);Assert.InRange(bounds.Right,0,viewport.ActualWidth+.5);
        Assert.Equal(new Thickness(1),border.BorderThickness);Assert.Equal(0d,selected.Margin.Left);Assert.Equal(0d,selected.Margin.Right);
    });

    [Theory][InlineData(false,1036)][InlineData(true,540)]
    public void RealProjectSplitterColumnsHaveGuttersAndResizeBeforeDragCompletes(bool inspector,double width) => Sta(()=>
    {
        var resources=Resources();var main=MainSource();
        var source=inspector?main.Descendants().Single(e=>Name(e)=="InspectorColumn").Parent!.Parent!:main.Descendants().Single(e=>Name(e)=="WorkspaceContent");
        var columns=new XElement(source.Elements().Single(e=>e.Name.LocalName=="Grid.ColumnDefinitions"));
        var splitterNode=new XElement(source.Elements().Single(e=>e.Name.LocalName=="GridSplitter"));
        var grid=Parse<Grid>(new XElement(source.Name,columns,splitterNode));grid.Resources=resources;
        var splitter=grid.Children.OfType<GridSplitter>().Single();splitter.Resources=resources;splitter.Style=(Style)resources[typeof(GridSplitter)];Layout(grid,width,200);
        Assert.True((grid.ColumnDefinitions[1].ActualWidth-splitter.ActualWidth)/2>=8);
        Assert.True(grid.ColumnDefinitions.Sum(c=>c.ActualWidth)<=width+.5);
        Assert.False(splitter.ShowsPreview);var before=grid.ColumnDefinitions[0].ActualWidth;
        splitter.RaiseEvent(new DragStartedEventArgs(0,0){RoutedEvent=Thumb.DragStartedEvent});
        splitter.RaiseEvent(new DragDeltaEventArgs(24,0){RoutedEvent=Thumb.DragDeltaEvent});Layout(grid,width,200);
        Assert.True(grid.ColumnDefinitions[0].ActualWidth>before+20);
        splitter.RaiseEvent(new DragCompletedEventArgs(24,0,false){RoutedEvent=Thumb.DragCompletedEvent});
    });

    [Fact]
    public void ExpanderStringHeaderWrapsAndKeepsRightBorderInsideDetachedWidth() => Sta(()=>
    {
        var resources=Resources();var expander=new Expander{Resources=resources,Style=(Style)resources[typeof(Expander)],Header="아주 긴 프로젝트 폴더와 프로그램 자료의 내부 이름",IsExpanded=false};Layout(expander,170,150);
        var label=Children(expander).OfType<AccessText>().Single();Assert.Equal(TextWrapping.Wrap,label.TextWrapping);Assert.True(label.ActualHeight>25);
        var border=Children(expander).OfType<Border>().Single(b=>b.Name=="GroupHead");
        var bounds=border.TransformToAncestor(expander).TransformBounds(new Rect(border.RenderSize));Assert.InRange(bounds.Right,0,170);
    });

    [Fact]
    public void UserShellKeepsThemeAndActionsWithoutRepeatedAdministrativeBanners() => Sta(()=>
    {
        var theme=Fragment("ThemeControls");theme.DataContext=new {DarkMode=false,ViewModeLabel="重复管理状态"};Layout(theme,500,100);
        Assert.Equal(2,Children(theme).OfType<Button>().Count());Assert.DoesNotContain(Children(theme).OfType<TextBlock>(),t=>t.Text=="重复管理状态");
        var main=MainSource();Assert.DoesNotContain(main.Descendants(),e=>Name(e)=="ToolSearchBox");Assert.Contains(main.Descendants(),e=>Name(e)=="ProgramSearchBox");Assert.Contains(main.Descendants(),e=>Name(e)=="ProjectSearchBox");
        var projectTabs=main.Descendants().SingleOrDefault(e=>Name(e)=="ProjectContentTabs");Assert.NotNull(projectTabs);
        Assert.Contains(projectTabs.Descendants(),e=>e.Name.LocalName=="ProjectActivityView" && (string?)e.Attribute("DataContext")=="{Binding SelectedProjectDashboard}");
    });

    [Fact]
    public void DocumentReaderHasHistoryAndRawCopyButNoSearchOrResultNavigation()
    {
        var reader=XDocument.Load(Source("Views/DocumentReaderView.xaml")).Root!;
        Assert.DoesNotContain(reader.Descendants(),e=>e.Name.LocalName=="TextBox" && (string?)e.Attribute("IsReadOnly")!="True");
        Assert.DoesNotContain(reader.Descendants(),e=>(string?)e.Attribute("Click") is "Previous_Click" or "Next_Click");
        Assert.DoesNotContain(reader.Descendants(),e=>(string?)e.Attribute("Text")=="{Binding SearchSummary}");
        foreach(var handler in new[]{"Back_Click","Forward_Click","Return_Click"})
            Assert.Contains(reader.Descendants(),e=>e.Name.LocalName=="Button" && (string?)e.Attribute("Click")==handler);
        var raw=reader.Descendants().Single(e=>Name(e)=="RawBody");Assert.Equal("True",(string?)raw.Attribute("IsReadOnly"));Assert.Equal("{Binding RawText,Mode=OneWay}",(string?)raw.Attribute("Text"));
        Assert.Contains(reader.Descendants(),e=>Name(e)=="FormattedBody" && (string?)e.Attribute("IsReadOnly")=="True");
    }

    private static string Name(XElement e)=>(string?)e.Attribute(X+"Name")??"";
    private static XElement MainSource()=>XDocument.Load(Source("MainWindow.xaml")).Root!;
    private static FrameworkElement Fragment(string name)
    {
        var element=new XElement(MainSource().Descendants().Single(e=>Name(e)==name));
        foreach(var attribute in element.DescendantsAndSelf().Attributes().Where(a=>a.Name.LocalName is "Click" or "SelectionChanged" or "Expanded" or "Collapsed").ToArray())attribute.Remove();
        var resources=Resources();
        var root=MainSource();var host=new XElement(root.Name.Namespace+"Grid",root.Attributes().Where(a=>a.IsNamespaceDeclaration),new XElement(root.Name.Namespace+"Grid.Resources",ResourceElement()),element);
        var control=Parse<FrameworkElement>(host);control.Resources=resources;return control;
    }
    private static T Parse<T>(XElement element)
    {
        var copy=new XElement(element);foreach(var ns in MainSource().Attributes().Where(a=>a.IsNamespaceDeclaration))if(copy.Attribute(ns.Name) is null)copy.Add(new XAttribute(ns));
        Qualify(copy);
        return (T)XamlReader.Parse(copy.ToString(),new ParserContext{BaseUri=new Uri(Source("App.xaml"))});
    }
    private static XElement ResourceElement()
    {
        var root=XDocument.Load(Source("App.xaml")).Root!;var dictionary=new XElement(root.Name.Namespace+"ResourceDictionary",root.Attributes().Where(a=>a.IsNamespaceDeclaration),root.Elements().Single().Nodes(),MainSource().Elements().Single(e=>e.Name.LocalName=="Window.Resources").Nodes());Qualify(dictionary);return dictionary;
    }
    private static void Qualify(XElement root)
    {
        foreach(var declaration in root.DescendantsAndSelf().Attributes().Where(a=>a.IsNamespaceDeclaration&&a.Value.StartsWith("clr-namespace:AIControlTower")&&!a.Value.Contains(";assembly=")).ToArray())
        {var old=declaration.Value;declaration.Value+=";assembly=AIControlTower";foreach(var e in root.DescendantsAndSelf().Where(e=>e.Name.NamespaceName==old))e.Name=XName.Get(e.Name.LocalName,declaration.Value);}
    }
    private static ResourceDictionary Resources()
    {
        var resources=Parse<ResourceDictionary>(ResourceElement());foreach(var item in ThemeService.Palette(false))resources[item.Key]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.Value));return resources;
    }
    private static string Source(string file)
    {var directory=new DirectoryInfo(AppContext.BaseDirectory);while(directory is not null&&!File.Exists(Path.Combine(directory.FullName,"ai-control-tower","src","AIControlTower",file)))directory=directory.Parent;Assert.NotNull(directory);return Path.Combine(directory.FullName,"ai-control-tower","src","AIControlTower",file);}
    private static void Layout(FrameworkElement control,double width,double height){control.Measure(new Size(width,height));control.Arrange(new Rect(0,0,width,height));control.UpdateLayout();}
    private static IEnumerable<DependencyObject> Children(DependencyObject value){yield return value;for(var i=0;i<VisualTreeHelper.GetChildrenCount(value);i++)foreach(var child in Children(VisualTreeHelper.GetChild(value,i)))yield return child;}
    private static void Sta(Action action){Exception? failure=null;var thread=new Thread(()=>{try{action();}catch(Exception error){failure=error;}});thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();if(failure is not null)ExceptionDispatchInfo.Capture(failure).Throw();}
}
