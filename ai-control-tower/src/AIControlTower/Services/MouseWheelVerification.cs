using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AIControlTower.Models;
using AIControlTower.ViewModels;

namespace AIControlTower.Services;

/// <summary>Native routed-input regression checks, run only by the opt-in UI verification harness.</summary>
internal static class MouseWheelVerification
{
    internal sealed record Result(string Layout, string List, int Delta, double Before, double After,
        double Restored, bool ViewportConstrainedForTest, bool OriginalDeltaPreserved);

    internal static async Task<Result[]> VerifyListsAsync(MainWindow window)
    {
        var vm = (MainViewModel)window.DataContext;
        var oldProject = vm.SelectedProject; var oldProgram = vm.SelectedProgram;
        var oldWidth = window.Width; var oldHeight = window.Height;
        var projectSearch = vm.ProjectSearch; var programSearch = vm.ProgramSearch;
        var tabs = (TabControl)window.FindName("WorkspaceTabs"); var oldTab = tabs.SelectedIndex;
        var results = new List<Result>();
        var expanded = new List<(Expander Control, bool Previous)>();
        try
        {
            tabs.SelectedIndex = 0;
            vm.ProjectSearch = ""; vm.ProgramSearch = "";
            vm.SelectedProject = vm.Projects.OrderByDescending(p => p.Functions.Sum(f => f.Programs.Count)).FirstOrDefault();
            await LayoutAsync(window);
            foreach (var expander in Walk(window).OfType<Expander>().Where(e => Walk(e).OfType<ListBox>().Any(IsProgramList)))
            {
                expanded.Add((expander, expander.IsExpanded));
                expander.SetCurrentValue(Expander.IsExpandedProperty, true);
            }
            foreach (var layout in new[] { (Name: "wide", Width: 1460d, Height: 920d), (Name: "compact", Width: 1060d, Height: 720d) })
            {
                window.Width = layout.Width; window.Height = layout.Height;
                await LayoutAsync(window);
                results.Add(await VerifyListAsync(window, (ListBox)window.FindName("ProjectList"), layout.Name, "projects"));
                var programs = Walk(window).OfType<ListBox>().FirstOrDefault(l => l.IsVisible && IsProgramList(l));
                if (programs is null) throw new InvalidOperationException("Wheel verification requires an actual visible program list.");
                results.Add(await VerifyListAsync(window, programs, layout.Name, "programs"));
            }
        }
        finally
        {
            foreach (var item in expanded) item.Control.SetCurrentValue(Expander.IsExpandedProperty, item.Previous);
            vm.ProjectSearch = projectSearch; vm.ProgramSearch = programSearch;
            vm.SelectedProject = oldProject; vm.SelectedProgram = oldProgram;
            tabs.SelectedIndex = oldTab; window.Width = oldWidth; window.Height = oldHeight;
            await LayoutAsync(window);
        }
        return results.ToArray();
    }

    private static bool IsProgramList(ListBox list) => list.Items.OfType<ProgramItem>().Any();

    private static async Task<Result> VerifyListAsync(MainWindow window, ListBox list, string layout, string name)
    {
        if (list.Items.Count == 0) throw new InvalidOperationException("Wheel verification requires actual list items: " + name);
        list.ScrollIntoView(list.Items[0]);
        await LayoutAsync(window);
        var container = list.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem
            ?? throw new InvalidOperationException("Actual list item was not generated: " + name);
        var source = Walk(container).OfType<TextBlock>().FirstOrDefault() as UIElement ?? container;
        var viewers = MouseWheelRouting.Ancestors(source).OfType<ScrollViewer>().ToArray();
        var target = viewers.FirstOrDefault(v => v.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
            ?? throw new InvalidOperationException("List has no enabled scroll owner: " + name);
        var localMaxHeight = target.ReadLocalValue(FrameworkElement.MaxHeightProperty);
        var maxHeightBinding = BindingOperations.GetBindingBase(target, FrameworkElement.MaxHeightProperty);
        var oldMaxHeight = target.MaxHeight;
        var constrained = false;
        try
        {
            // A short real inventory can fit entirely. Constrain only its real viewport, never add mock rows.
            if (target.ScrollableHeight <= 0.000001)
            {
                constrained = true;
                target.SetCurrentValue(FrameworkElement.MaxHeightProperty, Math.Min(target.ActualHeight, 120));
                await LayoutAsync(window);
            }
            foreach (var viewer in viewers) viewer.ScrollToTop();
            await LayoutAsync(window);
            if (target.ScrollableHeight <= 0.000001) throw new InvalidOperationException("Actual list cannot exercise scrolling: " + name);
            var observed = new List<int>();
            MouseWheelEventHandler observer = (_, e) => observed.Add(e.Delta);
            target.AddHandler(Mouse.MouseWheelEvent, observer, true);
            try
            {
                var before = target.VerticalOffset;
                RaiseWheel(source, -240);
                await LayoutAsync(window);
                var after = target.VerticalOffset;
                if (after <= before) throw new InvalidOperationException($"Mouse wheel swallowed over {layout}/{name}: {before} -> {after}");
                RaiseWheel(source, 240);
                await LayoutAsync(window);
                var restored = target.VerticalOffset;
                if (restored >= after) throw new InvalidOperationException($"Reverse mouse wheel swallowed over {layout}/{name}.");
                var preserved = observed.SequenceEqual(new[] { -240, 240 });
                if (!preserved) throw new InvalidOperationException("Wheel delta changed or wheel handled more than once: " + name);
                return new(layout, name, -240, before, after, restored, constrained, preserved);
            }
            finally { target.RemoveHandler(Mouse.MouseWheelEvent, observer); }
        }
        finally
        {
            if (constrained)
            {
                if (maxHeightBinding is not null) BindingOperations.SetBinding(target, FrameworkElement.MaxHeightProperty, maxHeightBinding);
                else if (localMaxHeight == DependencyProperty.UnsetValue) target.ClearValue(FrameworkElement.MaxHeightProperty);
                else target.SetCurrentValue(FrameworkElement.MaxHeightProperty, oldMaxHeight);
            }
            foreach (var viewer in viewers) viewer.ScrollToTop();
            await LayoutAsync(window);
        }
    }

    internal static void RaiseWheel(UIElement source, int delta) => source.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
    { RoutedEvent = Mouse.PreviewMouseWheelEvent });

    private static async Task LayoutAsync(Window window)
    {
        window.UpdateLayout();
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.ContextIdle);
    }

    private static IEnumerable<DependencyObject> Walk(DependencyObject element)
    {
        yield return element;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            foreach (var child in Walk(VisualTreeHelper.GetChild(element, i))) yield return child;
    }
}
