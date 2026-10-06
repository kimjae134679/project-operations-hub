using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AIControlTower.Services;

/// <summary>Bypasses a nested list's non-scrolling viewer without replacing native wheel behavior.</summary>
public static class MouseWheelRouting
{
    public static void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || e.Delta == 0 || Keyboard.Modifiers != ModifierKeys.None
            || e.OriginalSource is not DependencyObject source) return;
        var ancestors = Ancestors(source).ToArray();
        // Editors and selectors own their wheel behavior, including popup/document descendants.
        if (ancestors.Any(IsNativeInteraction)) return;
        var target = ancestors.OfType<ScrollViewer>().FirstOrDefault(viewer => CanScroll(viewer, e.Delta));
        if (target is null) return;
        e.Handled = true;
        target.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = Mouse.MouseWheelEvent,
            Source = target
        });
    }

    private static bool IsNativeInteraction(DependencyObject value) =>
        value is not RichTextBox { IsReadOnly:true, VerticalScrollBarVisibility:ScrollBarVisibility.Disabled }
        && value is TextBoxBase or PasswordBox or ComboBox or ComboBoxItem or Popup or RangeBase or MenuBase;

    private static bool CanScroll(ScrollViewer viewer, int delta) =>
        viewer.IsVisible && viewer.IsEnabled && viewer.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled
        && viewer.ScrollableHeight > 0.000001
        && (delta < 0 ? viewer.VerticalOffset < viewer.ScrollableHeight - 0.000001 : viewer.VerticalOffset > 0.000001);

    internal static IEnumerable<DependencyObject> Ancestors(DependencyObject value)
    {
        for (DependencyObject? current = value; current is not null; current = Parent(current)) yield return current;
    }

    private static DependencyObject? Parent(DependencyObject value)
    {
        if (value is Visual or Visual3D)
        {
            var parent = VisualTreeHelper.GetParent(value);
            if (parent is not null) return parent;
        }
        if (value is ContentElement content)
            return ContentOperations.GetParent(content) ?? (content as FrameworkContentElement)?.Parent;
        return LogicalTreeHelper.GetParent(value);
    }
}
