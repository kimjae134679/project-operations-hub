using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class BorderContainmentTests
{
    [Theory]
    [InlineData(false,430,0,13)] [InlineData(true,430,4,13)] [InlineData(false,430,8,13)]
    [InlineData(false,1028,0,13)] [InlineData(true,1028,4,19.5)] [InlineData(false,1028,8,26)]
    [InlineData(false,1428,0,13)] [InlineData(true,1428,4,19.5)] [InlineData(false,1428,8,26)]
    public void RealWorkspaceHeadersRemainWholeAtFirstMiddleLastSelectionAndTextSizes(bool dark,double width,int selected,double fontSize)=>Sta(()=>
    {
        var tabs=WorkspaceTabs(dark);
        foreach(TabItem tab in tabs.Items)tab.FontSize=fontSize;
        tabs.SelectedIndex=selected;Layout(tabs,width,600);
        var panel=Descendants(tabs).OfType<TabPanel>().SingleOrDefault();
        Assert.True(panel is not null,TreeDump(tabs));
        var viewport=(FrameworkElement?)Descendants(tabs).OfType<ScrollContentPresenter>().FirstOrDefault(p=>p.IsAncestorOf(panel!))??panel!;
        foreach(TabItem tab in tabs.Items)
        {
            var tile=Descendants(tab).OfType<Border>().Single(b=>b.Name=="TabTile");
            var bounds=tile.TransformToAncestor(viewport).TransformBounds(new Rect(tile.RenderSize));
            Assert.True(bounds.Left>=-0.01 && bounds.Right<=viewport.ActualWidth+0.01 && bounds.Top>=-0.01 && bounds.Bottom<=viewport.ActualHeight+0.01,
                $"Header '{tab.Header}' selected={tab.IsSelected} bounds={bounds} viewport={viewport.RenderSize} text={fontSize}");
            var own=tile.TransformToAncestor(tab).TransformBounds(new Rect(tile.RenderSize));
            Assert.InRange(own.Right,0,tab.ActualWidth+0.01);Assert.InRange(own.Bottom,0,tab.ActualHeight+0.01);
            Assert.Equal(new Thickness(1),tile.BorderThickness);
        }
    });

    [Theory]
    [InlineData(false,1.0,0)] [InlineData(true,1.25,4)] [InlineData(false,1.5,8)] [InlineData(true,2.0,0)]
    public void SelectedRealTabRendersAllFourBorderSegmentsAtFractionalDpi(bool dark,double scale,int selected)=>Sta(()=>
    {
        var tabs=WorkspaceTabs(dark);tabs.SelectedIndex=selected;Layout(tabs,1028,400);
        var tab=(TabItem)tabs.Items[selected];var tile=Descendants(tab).OfType<Border>().Single(b=>b.Name=="TabTile");
        AssertContentInside(tile);
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(tabs.ActualWidth*scale),(int)Math.Ceiling(tabs.ActualHeight*scale),96*scale,96*scale,PixelFormats.Pbgra32);
        bitmap.Render(tabs);
        var bounds=tile.TransformToAncestor(tabs).TransformBounds(new Rect(tile.RenderSize));
        var color=((SolidColorBrush)tile.BorderBrush).Color;
        var geometry=$" tile={bounds}; items="+string.Join(";",tabs.Items.Cast<TabItem>().Select(i=>$"{i.Header}:{i.TransformToAncestor(tabs).TransformBounds(new Rect(i.RenderSize))}:slot={LayoutInformation.GetLayoutSlot(i)}:margin={i.Margin}:clip={VisualTreeHelper.GetClip(i)?.Bounds}"));
        AssertBorderPixel(bitmap,bounds.Left+0.5,bounds.Top+bounds.Height/2,scale,color,"left"+geometry);
        AssertBorderPixel(bitmap,bounds.Right-0.5,bounds.Top+bounds.Height/2,scale,color,"right"+geometry);
        AssertBorderPixel(bitmap,bounds.Left+bounds.Width/2,bounds.Top+0.5,scale,color,"top");
        AssertBorderPixel(bitmap,bounds.Left+bounds.Width/2,bounds.Bottom-0.5,scale,color,"bottom");
    });

    [Theory] [InlineData(false,13)] [InlineData(true,26)]
    public void LongSelectedLabelAndRightThemeButtonHaveContainedChromeAndContent(bool dark,double fontSize)=>Sta(()=>
    {
        var tabs=WorkspaceTabs(dark);var item=(TabItem)tabs.Items[0];item.Header="프로젝트 작업 · 진행 중인 작업과 최근 실행 기록";item.FontSize=fontSize;tabs.SelectedIndex=0;
        Layout(tabs,1028,400);var tile=Descendants(item).OfType<Border>().Single(b=>b.Name=="TabTile");AssertContentInside(tile);
        var borderBounds=tile.TransformToAncestor(item).TransformBounds(new Rect(tile.RenderSize));Assert.InRange(borderBounds.Right,0,item.ActualWidth+0.01);
        var resources=Resources(dark);var button=new Button{Content="어둡게",Resources=resources,Style=(Style)resources["ThemeChoiceButton"],Tag="dark",FontSize=fontSize};
        Layout(button,170,70);var chrome=Descendants(button).OfType<Border>().Single(b=>b.Name=="Chrome");Assert.Equal(button.RenderSize,chrome.RenderSize);AssertContentInside(chrome);
    });

    [Theory] [InlineData(false,1.0)] [InlineData(true,2.0)]
    public void OriginalMarginClipStillFailsThePhysicalNeighbourProbe(bool dark,double scale)=>Sta(()=>
    {
        // Counterfactual regression: the +/-1px DPI sampling correction cannot hide the original
        // four-DIP clipping bug. Use the unchanged common template/style, not the new local fix.
        var resources=Resources(dark);var tabs=new TabControl{Resources=resources,Style=(Style)resources[typeof(TabControl)]};
        tabs.Items.Add(new TabItem{Header="내 프로젝트",Style=(Style)resources[typeof(TabItem)]});
        tabs.Items.Add(new TabItem{Header="프로젝트 작업",Style=(Style)resources[typeof(TabItem)]});tabs.SelectedIndex=0;
        Layout(tabs,1028,400);var item=(TabItem)tabs.Items[0];var clip=VisualTreeHelper.GetClip(item);Assert.NotNull(clip);
        Assert.True(clip!.Bounds.Width<item.ActualWidth);
        var tile=Descendants(item).OfType<Border>().Single(b=>b.Name=="TabTile");var bounds=tile.TransformToAncestor(tabs).TransformBounds(new Rect(tile.RenderSize));
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(tabs.ActualWidth*scale),(int)Math.Ceiling(tabs.ActualHeight*scale),96*scale,96*scale,PixelFormats.Pbgra32);bitmap.Render(tabs);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(()=>AssertBorderPixel(bitmap,bounds.Right-0.5,bounds.Top+bounds.Height/2,scale,((SolidColorBrush)tile.BorderBrush).Color,"right"));
    });

    private static void AssertContentInside(Border border)
    {
        var presenter=Descendants(border).OfType<ContentPresenter>().First();
        var bounds=presenter.TransformToAncestor(border).TransformBounds(new Rect(presenter.RenderSize));
        Assert.True(bounds.Left>=-0.01 && bounds.Right<=border.ActualWidth+0.01 && bounds.Top>=-0.01 && bounds.Bottom<=border.ActualHeight+0.01,
            $"Content bounds {bounds} exceed border {border.RenderSize}");
    }
    private static void AssertBorderPixel(BitmapSource bitmap,double x,double y,double scale,Color expected,string edge)
    {
        var px=(int)Math.Floor(x*scale);var py=(int)Math.Floor(y*scale);
        Assert.True(px>=0 && px<bitmap.PixelWidth && py>=0 && py<bitmap.PixelHeight,$"{edge} border outside bitmap at {px},{py}");
        var bytes=new byte[4];bitmap.CopyPixels(new Int32Rect(px,py,1,1),bytes,4,0);
        // At 150% a 1-DIP stroke crosses two physical pixels. The confirmed bottom edge has
        // partial coverage at py69 and its exact accent at py70; inspect only +/-1 physical pixel
        // along the stroke normal, not the background or a widened logical border region.
        // The pre-fix right clip removed four DIPs, so this still rejects that missing edge.
        bool IsAccent(byte[] sample)=>sample[3]>200 && Math.Abs(sample[2]-expected.R)<=35 && Math.Abs(sample[1]-expected.G)<=35 && Math.Abs(sample[0]-expected.B)<=35;
        var accentPresent=IsAccent(bytes);
        var horizontal=edge.StartsWith("left",StringComparison.Ordinal)||edge.StartsWith("right",StringComparison.Ordinal);
        foreach(var shift in new[]{-1,1})
        {
            var sx=px+(horizontal?shift:0);var sy=py+(horizontal?0:shift);
            if(sx<0||sy<0||sx>=bitmap.PixelWidth||sy>=bitmap.PixelHeight)continue;
            var adjacent=new byte[4];bitmap.CopyPixels(new Int32Rect(sx,sy,1,1),adjacent,4,0);accentPresent|=IsAccent(adjacent);
        }
        var neighbors=string.Join(";",Enumerable.Range(Math.Max(0,px-4),Math.Min(bitmap.PixelWidth,px+5)-Math.Max(0,px-4)).Select(n=>
        {var nearby=new byte[4];bitmap.CopyPixels(new Int32Rect(n,py,1,1),nearby,4,0);return $"{n}:{string.Join(',',nearby)}";}));
        var vertical=string.Join(";",Enumerable.Range(Math.Max(0,py-2),Math.Min(bitmap.PixelHeight,py+3)-Math.Max(0,py-2)).Select(n=>
        {var nearby=new byte[4];bitmap.CopyPixels(new Int32Rect(px,n,1,1),nearby,4,0);return $"{n}:{string.Join(',',nearby)}";}));
        Assert.True(accentPresent,
            $"{edge} expected {expected}, actual BGRA={string.Join(',',bytes)} at {px},{py}; nearby={neighbors}; vertical={vertical}");
    }
    private static TabControl WorkspaceTabs(bool dark)
    {
        var root=XDocument.Load(Source("MainWindow.xaml")).Root!;XNamespace x="http://schemas.microsoft.com/winfx/2006/xaml";
        var source=root.Descendants().Single(e=>(string?)e.Attribute(x+"Name")=="WorkspaceTabs");
        // Construct the isolated control exactly as the existing detached visual fixture does.
        // Extract only the real header names and any MainWindow-specific template, never parse/detach
        // the live-window shell (which leaves deferred WPF layout/implicit-style context behind).
        var resources=Resources(dark);var tabs=new TabControl{Resources=resources,Style=(Style)resources[typeof(TabControl)]};
        if((string?)source.Attribute("ItemContainerStyle")=="{StaticResource WorkspaceTabItem}")tabs.ItemContainerStyle=(Style)resources["WorkspaceTabItem"];
        var template=source.Elements().FirstOrDefault(e=>e.Name.LocalName=="TabControl.Template")?.Elements().Single();
        if(template is not null)
        {
            var copy=new XElement(template);
            foreach(var declaration in root.Attributes().Where(a=>a.IsNamespaceDeclaration))if(copy.Attribute(declaration.Name) is null)copy.Add(new XAttribute(declaration));
            tabs.Template=(ControlTemplate)XamlReader.Parse(copy.ToString(),new ParserContext{BaseUri=new Uri(Source("App.xaml"))});
        }
        foreach(var node in source.Elements().Where(e=>e.Name.LocalName=="TabItem"))
            tabs.Items.Add(new TabItem{Header=(string?)node.Attribute("Header"),Name=(string?)node.Attribute(x+"Name")??"",Style=tabs.ItemContainerStyle??(Style)resources[typeof(TabItem)]});
        return tabs;
    }
    private static XElement ResourceElement()
    {
        var root=XDocument.Load(Source("App.xaml")).Root!;
        var resources=new XElement(root.Name.Namespace+"ResourceDictionary",root.Attributes().Where(a=>a.IsNamespaceDeclaration),root.Elements().Single().Nodes());
        foreach(var declaration in resources.Attributes().Where(a=>a.IsNamespaceDeclaration&&a.Value.StartsWith("clr-namespace:AIControlTower")&&!a.Value.Contains(";assembly=")))
        {var ns=declaration.Value;declaration.Value+=";assembly=AIControlTower";foreach(var node in resources.Descendants().Where(n=>n.Name.NamespaceName==ns))node.Name=XName.Get(node.Name.LocalName,declaration.Value);}
        XNamespace x="http://schemas.microsoft.com/winfx/2006/xaml";
        var localStyle=XDocument.Load(Source("MainWindow.xaml")).Descendants().SingleOrDefault(e=>e.Name.LocalName=="Style"&&(string?)e.Attribute(x+"Key")=="WorkspaceTabItem");
        if(localStyle is not null)resources.Add(new XElement(localStyle));
        return resources;
    }
    private static ResourceDictionary Resources(bool dark)
    {
        var resources=(ResourceDictionary)XamlReader.Parse(ResourceElement().ToString(),new ParserContext{BaseUri=new Uri(Source("App.xaml"))});
        foreach(var entry in ThemeService.Palette(dark))resources[entry.Key]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(entry.Value));return resources;
    }
    private static string Source(string file)
    {var directory=new DirectoryInfo(AppContext.BaseDirectory);while(directory is not null&&!File.Exists(Path.Combine(directory.FullName,"ai-control-tower","src","AIControlTower","App.xaml")))directory=directory.Parent;Assert.NotNull(directory);return Path.Combine(directory!.FullName,"ai-control-tower","src","AIControlTower",file);}
    private static void Layout(FrameworkElement control,double width,double height)
    {
        control.ApplyTemplate();control.Measure(new Size(width,height));
        if(control is TabControl tabs)foreach(TabItem item in tabs.Items)item.ApplyTemplate();
        control.Measure(new Size(width,height));control.Arrange(new Rect(0,0,width,height));control.UpdateLayout();
    }
    private static string TreeDump(TabControl tabs)=>$"template={tabs.Template is not null}, style={tabs.Style is not null}, items={tabs.Items.Count}, size={tabs.RenderSize}; "+string.Join("; ",Descendants(tabs).OfType<FrameworkElement>().Select(e=>$"{e.GetType().Name}:{e.Name}:{e.RenderSize}"));
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {yield return root;for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)foreach(var child in Descendants(VisualTreeHelper.GetChild(root,i)))yield return child;}
    private static void Sta(Action action)
    {Exception? failure=null;var thread=new Thread(()=>{try{action();}catch(Exception ex){failure=ex;}});thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();if(failure is not null)ExceptionDispatchInfo.Capture(failure).Throw();}
}
