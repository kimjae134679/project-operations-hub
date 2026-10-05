using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using AIControlTower.ViewModels;

namespace AIControlTower.Services;

/// <summary>Opt-in tests of the rendered native controls, not a second preview implementation.</summary>
internal static class ThemeVerification
{
    internal static async Task<object[]> RunAsync(MainWindow window, string output)
    {
        var vm = (MainViewModel)window.DataContext;
        var tabs = (TabControl)window.FindName("WorkspaceTabs");
        var oldDark = vm.DarkMode; var oldTab = tabs.SelectedIndex;
        var width = window.Width; var height = window.Height;
        var project = vm.SelectedProject; var program = vm.SelectedProgram;
        var projectSearch = vm.ProjectSearch; var programSearch = vm.ProgramSearch;
        var results = new List<object>();
        try
        {
            foreach (var dark in new[] { false, true, false })
            {
                vm.DarkMode = dark;
                ThemeService.Apply(dark);
                var mode = dark ? "dark" : "light";
                foreach (var layout in new[] { (Name:"wide",Width:1460d,Height:920d),(Name:"compact",Width:1060d,Height:720d) })
                {
                    window.Width = layout.Width; window.Height = layout.Height;
                    for (var tab = 0; tab < tabs.Items.Count; tab++)
                    {
                        tabs.SelectedIndex = tab;
                        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
                        var colors = CheckRenderedText(window);
                        results.Add(new { Theme = mode, Layout = layout.Name, Tab = tab, TextCount = colors.Count, MinimumContrast = colors.DefaultIfEmpty(21).Min() });
                        UiVerification.Capture(window, Path.Combine(output,$"{mode}-{layout.Name}-{tab}.png"));
                        if (tab == 2)
                        {
                            var reader=(RichTextBox)window.FindName("NoticeBody");
                            var expected=(SolidColorBrush)window.FindResource("TextBrush");
                            if (reader.Document.Foreground is not SolidColorBrush actual || actual.Color != expected.Color)
                                throw new InvalidOperationException("Open announcement kept the previous theme.");
                            foreach(var cell in reader.Document.Blocks.OfType<Table>().SelectMany(t=>t.RowGroups).SelectMany(g=>g.Rows).SelectMany(r=>r.Cells))
                                if (cell.Background is SolidColorBrush background && Ratio(expected.Color,background.Color)<4.5)
                                    throw new InvalidOperationException("Announcement table contrast failed.");
                        }
                    }
                }
                var jev = new JevControlWindow(vm) { Owner = window };
                try
                {
                    jev.Show();
                    await jev.Dispatcher.InvokeAsync(jev.UpdateLayout,DispatcherPriority.Render);
                    var colors=CheckRenderedText(jev);
                    results.Add(new { Theme=mode, Window="AI 작업 전달", TextCount=colors.Count, MinimumContrast=colors.DefaultIfEmpty(21).Min() });
                    UiVerification.Capture(jev,Path.Combine(output,mode+"-jev.png"));
                }
                finally { jev.Close(); }
                if(vm.SelectedProject!=project || vm.SelectedProgram!=program || vm.ProjectSearch!=projectSearch || vm.ProgramSearch!=programSearch)
                    throw new InvalidOperationException("Theme switching changed the selected task or search.");
            }
        }
        finally
        {
            vm.DarkMode=oldDark; ThemeService.Apply(oldDark);
            tabs.SelectedIndex=oldTab; window.Width=width; window.Height=height;
            await window.Dispatcher.InvokeAsync(window.UpdateLayout,DispatcherPriority.Render);
        }
        return results.ToArray();
    }

    private static List<double> CheckRenderedText(Window window)
    {
        var results=new List<double>();
        var content=(FrameworkElement)window.Content;
        foreach(var text in Walk(content).OfType<TextBlock>().Where(t=>t.IsVisible && !string.IsNullOrWhiteSpace(t.Text) && t.ActualWidth>0 && t.ActualHeight>0))
        {
            if(text.FontFamily.Source.Contains("Icons") || text.FontFamily.Source.Contains("MDL2")) continue;
            var bounds=text.TransformToAncestor(content).TransformBounds(new Rect(0,0,text.ActualWidth,text.ActualHeight));
            if(bounds.Right<0 || bounds.Bottom<0 || bounds.Left>content.ActualWidth || bounds.Top>content.ActualHeight) continue;
            if(text.Foreground is not SolidColorBrush foreground) continue;
            var backgrounds=Backgrounds(text,window.Background);
            var ratio=backgrounds.Min(background=>Ratio(foreground.Color,background));
            results.Add(ratio);
            if(ratio<4.5)
                throw new InvalidOperationException($"Rendered text contrast {ratio:F2}: '{text.Text}' foreground={foreground.Color}; backgrounds={string.Join(',',backgrounds)}; dark={ThemeService.IsDark}");
        }
        return results;
    }

    private static Color[] Backgrounds(DependencyObject element, Brush windowBackground)
    {
        if (element is TextBlock { Background: SolidColorBrush own } && own.Color.A==255) return [own.Color];
        for(var parent=VisualTreeHelper.GetParent(element);parent is not null;parent=VisualTreeHelper.GetParent(parent))
        {
            // A templated Control.Background may be an unused OS theme default.
            // Inspect the Border/Panel which actually paints the visible template.
            var brush=parent switch { Border border=>border.Background, Panel panel=>panel.Background, _=>null };
            if(brush is SolidColorBrush solid && solid.Color.A==255) return [solid.Color];
            if(brush is GradientBrush gradient && gradient.GradientStops.All(stop=>stop.Color.A==255))
                return gradient.GradientStops.Select(stop=>stop.Color).ToArray();
        }
        return windowBackground is SolidColorBrush background ? [background.Color] : [Colors.White];
    }
    internal static double Ratio(Color foreground,Color background)
    {
        static double L(Color color)
        {
            static double C(byte value) { var x=value/255d; return x<=0.04045?x/12.92:Math.Pow((x+0.055)/1.055,2.4); }
            return 0.2126*C(color.R)+0.7152*C(color.G)+0.0722*C(color.B);
        }
        var a=L(foreground); var b=L(background);
        return (Math.Max(a,b)+0.05)/(Math.Min(a,b)+0.05);
    }
    private static IEnumerable<DependencyObject> Walk(DependencyObject element)
    {
        yield return element;
        for(var i=0;i<VisualTreeHelper.GetChildrenCount(element);i++)
            foreach(var child in Walk(VisualTreeHelper.GetChild(element,i))) yield return child;
    }
}
