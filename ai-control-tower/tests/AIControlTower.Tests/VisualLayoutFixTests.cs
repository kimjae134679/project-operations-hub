using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class VisualLayoutFixTests
{
    [Fact] public void LatestRequestedMyProjectsIsFirstAndDefaultByNamedIdentity()
    {
        var xml=XDocument.Load(Source("MainWindow.xaml"));var tabs=xml.Descendants().Single(e=>e.Name.LocalName=="TabControl");
        Assert.Equal("ProjectsTab",(string?)tabs.Elements().First().Attribute(XName.Get("Name","http://schemas.microsoft.com/winfx/2006/xaml")));
        Assert.Equal("True",(string?)tabs.Elements().First().Attribute("IsSelected"));
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void SelectedRightmostRealTabHasCompletePixelAlignedRectangleInsideHeaderViewport(bool dark)=>Sta(()=>
    {
        var resources=Resources(dark);var tabs=new TabControl{Resources=resources,Style=(Style)resources[typeof(TabControl)]};
        for(var i=0;i<9;i++)tabs.Items.Add(new TabItem{Header="프로젝트 작업 "+i,Style=(Style)resources[typeof(TabItem)]});
        Layout(tabs,430,220);tabs.SelectedIndex=8;Layout(tabs,430,220);
        var ensure=typeof(MainWindow).GetMethod("EnsureSelectedTabVisible",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
        ensure?.Invoke(null,[tabs]);Layout(tabs,430,220);
        var scroll=Children(tabs).OfType<ScrollViewer>().FirstOrDefault(s=>s.Name=="TabHeaderScroll")??Children(tabs).OfType<ScrollViewer>().First();
        FrameworkElement viewport=Children(scroll).OfType<ScrollContentPresenter>().FirstOrDefault()??(FrameworkElement)scroll;var item=(TabItem)tabs.Items[8];
        var border=Children(item).OfType<Border>().Single(b=>b.Name=="TabTile");var bounds=border.TransformToAncestor(viewport).TransformBounds(new Rect(border.RenderSize));
        Assert.InRange(bounds.Left,0,viewport.ActualWidth);Assert.InRange(bounds.Right,0,viewport.ActualWidth+0.5);
        Assert.Equal(new CornerRadius(0),border.CornerRadius);Assert.True(item.SnapsToDevicePixels);Assert.True(item.UseLayoutRounding);
        Assert.Equal(new Thickness(1),border.BorderThickness);Assert.True(border.ActualHeight>=40);Assert.True(border.ActualWidth>=72);
        Assert.Same(resources["AccentBrush"],border.BorderBrush);
    });
    [Theory] [InlineData(false)] [InlineData(true)]
    public void DashboardUsesCompactSearchWatermarkAndHidesStateRegistrationUnderAdvanced(bool dark)=>Sta(()=>
    {
        var view=new AIControlTower.Views.WorkDashboardView{Resources=Resources(dark)};
        var vm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<AIControlTower.Models.WorkActivity>>([]));view.DataContext=vm;Layout(view,1060,660);
        var labels=Children(view).OfType<TextBlock>().Where(t=>t.Visibility==Visibility.Visible && t.ActualHeight>0).Select(t=>t.Text).ToArray();
        Assert.Contains("작업 검색",labels);
        var watermark=Children(view).OfType<TextBlock>().Single(t=>t.Name=="WorkSearchWatermark");
        var advanced=view.FindName("AdvancedSources") as Expander;Assert.NotNull(advanced);Assert.Equal("고급",advanced.Header);Assert.False(advanced.IsExpanded);
        Assert.Equal(Visibility.Visible,watermark.Visibility);Assert.False(watermark.IsHitTestVisible);
        vm.Search="Codex";Layout(view,1060,660);Assert.Equal(Visibility.Collapsed,watermark.Visibility);
        advanced.IsExpanded=true;Layout(view,1060,660);
        var stateHint=view.FindName("StatePathWatermark") as TextBlock;Assert.NotNull(stateHint);Assert.Equal(Visibility.Visible,stateHint.Visibility);
        var state=view.FindName("StatePathInput") as TextBox;Assert.NotNull(state);state.Text=@"D:\A_KJ\AI\explicit\state.json";Layout(view,1060,660);Assert.Equal(Visibility.Collapsed,stateHint.Visibility);
    });
    [Theory] [InlineData(false)] [InlineData(true)]
    public void ActualThemeChoiceButtonsShowCurrentSelectionAndKeepCompleteHitTargets(bool dark)=>Sta(()=>
    {
        var main=XDocument.Load(Source("MainWindow.xaml")).Root!;var name=XName.Get("Name","http://schemas.microsoft.com/winfx/2006/xaml");
        var panel=main.Descendants().FirstOrDefault(e=>(string?)e.Attribute(name)=="ThemeControls");Assert.NotNull(panel);
        var copy=new XElement(panel);foreach(var attribute in copy.DescendantsAndSelf().Attributes().Where(a=>a.Name.LocalName=="Click" || a.Name.LocalName=="Grid.Column").ToArray())attribute.Remove();
        foreach(var ns in main.Attributes().Where(a=>a.IsNamespaceDeclaration))if(copy.Attribute(ns.Name) is null)copy.Add(new XAttribute(ns));
        var host=new XElement(main.Name.Namespace+"Grid",main.Attributes().Where(a=>a.IsNamespaceDeclaration),new XElement(main.Name.Namespace+"Grid.Resources",ResourceElement()),copy);
        var controls=(FrameworkElement)XamlReader.Parse(host.ToString(),new ParserContext{BaseUri=new Uri(Source("App.xaml"))});controls.Resources=Resources(dark);var model=new ThemeFixture{DarkMode=dark};controls.DataContext=model;Layout(controls,600,100);
        var buttons=Children(controls).OfType<Button>().ToArray();Assert.Equal(2,buttons.Length);
        var light=buttons.Single(b=>b.Name=="LightThemeButton");var dim=buttons.Single(b=>b.Name=="DarkThemeButton");
        foreach(var button in buttons){Assert.True(button.ActualHeight>=40);Assert.True(button.ActualWidth>=72);Assert.True(button.SnapsToDevicePixels);var chrome=Children(button).OfType<Border>().Single(b=>b.Name=="Chrome");Assert.Equal(button.RenderSize,chrome.RenderSize);}
        Assert.Same(controls.Resources["SelectionBrush"],(dark?dim:light).Background);
        model.DarkMode=!dark;Layout(controls,600,100);Assert.Same(controls.Resources["SelectionBrush"],(dark?light:dim).Background);
    });
    private sealed class ThemeFixture:ObservableObject {private bool _dark;public bool DarkMode{get=>_dark;set=>SetProperty(ref _dark,value);}public string ViewModeLabel=>"로컬 조회 · 외부 실행 보류";}
    private static string Source(string file){var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir is not null&&!File.Exists(Path.Combine(dir.FullName,"ai-control-tower","src","AIControlTower","App.xaml")))dir=dir.Parent;Assert.NotNull(dir);return Path.Combine(dir.FullName,"ai-control-tower","src","AIControlTower",file);}
    private static XElement ResourceElement()
    {
        var root=XDocument.Load(Source("App.xaml")).Root!;var x=new XElement(root.Name.Namespace+"ResourceDictionary",root.Attributes().Where(a=>a.IsNamespaceDeclaration),root.Elements().Single().Nodes());
        foreach(var declaration in x.Attributes().Where(a=>a.IsNamespaceDeclaration&&a.Value.StartsWith("clr-namespace:AIControlTower")&&!a.Value.Contains(";assembly="))){var ns=declaration.Value;declaration.Value+=";assembly=AIControlTower";foreach(var e in x.Descendants().Where(e=>e.Name.NamespaceName==ns))e.Name=XName.Get(e.Name.LocalName,declaration.Value);}
        return x;
    }
    private static ResourceDictionary Resources(bool dark){var resources=(ResourceDictionary)XamlReader.Parse(ResourceElement().ToString(),new ParserContext{BaseUri=new Uri(Source("App.xaml"))});foreach(var entry in ThemeService.Palette(dark))resources[entry.Key]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(entry.Value));return resources;}
    private static void Layout(FrameworkElement element,double width,double height){element.Measure(new Size(width,height));element.Arrange(new Rect(0,0,width,height));element.UpdateLayout();}
    private static IEnumerable<DependencyObject> Children(DependencyObject root){yield return root;for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)foreach(var child in Children(VisualTreeHelper.GetChild(root,i)))yield return child;}
    private static void Sta(Action action){Exception? error=null;var t=new Thread(()=>{try{action();}catch(Exception ex){error=ex;}});t.SetApartmentState(ApartmentState.STA);t.Start();t.Join();if(error is not null)ExceptionDispatchInfo.Capture(error).Throw();}
}
