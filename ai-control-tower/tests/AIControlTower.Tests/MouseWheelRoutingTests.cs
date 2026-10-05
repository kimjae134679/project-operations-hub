using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class MouseWheelRoutingTests
{
    [Fact]
    public void NestedViewerScrollsLocallyThenHandsOriginalDeltaToParentAtBoundary() => OnSta(() =>
    {
        var row = new TextBlock { Text = "실제 휠 경로 검사", Height = 900 };
        var inner = new ScrollViewer { Content = row, Height = 100, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var content = new StackPanel(); content.Children.Add(inner); content.Children.Add(new Border { Height = 900 });
        var outer = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        WithWindow(outer, window =>
        {
            Assert.True(inner.ScrollableHeight > 0); Assert.True(outer.ScrollableHeight > 0);
            var observed = new List<int>();
            outer.AddHandler(Mouse.MouseWheelEvent, new MouseWheelEventHandler((_, e) => observed.Add(e.Delta)), true);
            RaiseWheel(row, -240); Layout(window);
            Assert.True(inner.VerticalOffset > 0); Assert.Equal(0, outer.VerticalOffset);
            inner.ScrollToBottom(); Layout(window); observed.Clear();
            var before = inner.VerticalOffset;
            RaiseWheel(row, -240); Layout(window);
            Assert.Equal(before, inner.VerticalOffset); Assert.True(outer.VerticalOffset > 0);
            Assert.Equal(new[] { -240 }, observed);
            var parentBefore = outer.VerticalOffset;
            RaiseWheel(row, 240); Layout(window);
            Assert.True(inner.VerticalOffset < before); Assert.Equal(parentBefore, outer.VerticalOffset);
        });
    });

    [Fact]
    public void DisabledNestedListViewerDoesNotSwallowParentScrolling() => OnSta(() =>
    {
        var list = new ListBox { ItemsSource = Enumerable.Range(0, 30).Select(i => "항목 " + i).ToArray() };
        ScrollViewer.SetVerticalScrollBarVisibility(list, ScrollBarVisibility.Disabled);
        var outer = new ScrollViewer { Content = list, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        WithWindow(outer, window =>
        {
            var item = Assert.IsType<ListBoxItem>(list.ItemContainerGenerator.ContainerFromIndex(0));
            var source = Descendants(item).OfType<TextBlock>().FirstOrDefault() as UIElement ?? item;
            Assert.True(outer.ScrollableHeight > 0);
            RaiseWheel(source, -120); Layout(window);
            Assert.True(outer.VerticalOffset > 0);
        });
    });

    [Fact]
    public void EditorAndComboBoxWheelRemainsWithNativeControl() => OnSta(() =>
    {
        var editor = new TextBox { AcceptsReturn = true, Height = 80, Text = "가\n나\n다\n라\n마", VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var document = new RichTextBox { Height = 80 };
        var combo = new ComboBox { ItemsSource = new[] { "첫 번째", "두 번째" }, SelectedIndex = 0 };
        var content = new StackPanel(); content.Children.Add(editor); content.Children.Add(document); content.Children.Add(combo); content.Children.Add(new Border { Height = 900 });
        var outer = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        WithWindow(outer, window =>
        {
            var rerouted = 0;
            outer.AddHandler(Mouse.MouseWheelEvent, new MouseWheelEventHandler((_, _) => rerouted++), true);
            foreach (var control in new UIElement[] { editor, document, combo })
            {
                var args = RaiseWheel(control, -120); Layout(window);
                Assert.False(args.Handled);
            }
            Assert.Equal(0, rerouted); Assert.Equal(0, outer.VerticalOffset); Assert.Equal(0, combo.SelectedIndex);
        });
    });

    private static MouseWheelEventArgs RaiseWheel(UIElement source, int delta)
    {
        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta) { RoutedEvent = Mouse.PreviewMouseWheelEvent };
        source.RaiseEvent(args); return args;
    }

    private static void WithWindow(UIElement content, Action<Window> verify)
    {
        var window = new Window { Content = content, Width = 360, Height = 240, ShowActivated = false, ShowInTaskbar = false, Opacity = 0 };
        window.PreviewMouseWheel += MouseWheelRouting.HandlePreviewMouseWheel;
        try { window.Show(); Layout(window); verify(window); }
        finally { window.Close(); }
    }

    private static void Layout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(window.UpdateLayout));
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject value)
    {
        yield return value;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(value); i++)
            foreach (var child in Descendants(VisualTreeHelper.GetChild(value, i))) yield return child;
    }

    private static void OnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { failure = ex; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
