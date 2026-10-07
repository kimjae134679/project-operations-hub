using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class ControlThemeTests
{
    private const string LongLabel = "선택한 프로젝트의 작업 결과와 사용 안내를 함께 확인하기";

    [Fact]
    public void RealButtonAndToggleTemplatesWrapKoreanWithoutClippingOrReplacingCustomContent() => OnSta(() =>
    {
        var resources = LoadResources(false);
        foreach (var type in new[] { typeof(Button), typeof(ToggleButton) })
        {
            var control = (ContentControl)Activator.CreateInstance(type)!;
            Prepare(control, resources); control.Content = LongLabel; Layout(control, 150);
            var label = Descendants(control).OfType<AccessText>().Single();
            Assert.Equal(TextWrapping.Wrap, label.TextWrapping);
            Assert.True(control.DesiredSize.Height > 40, "Long Korean labels must grow rather than clip.");
            Assert.True(label.ActualWidth <= 126);
            Assert.Equal(40, control.MinHeight); Assert.Equal(new Thickness(12, 8, 12, 8), control.Padding);
            Assert.Equal(new CornerRadius(8), Part<Border>(control, "Chrome").CornerRadius);
            var custom = new TextBlock { Text = "기존 아이콘/콘텐츠", FontSize = 17 };
            control.Content = custom; Layout(control, 150);
            Assert.Contains(custom, Descendants(control)); Assert.Equal(17, custom.FontSize);
        }
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActualButtonTriggersPreservePrimaryInkAndDisabledOverridesEveryInteraction(bool dark) => OnSta(() =>
    {
        var resources = LoadResources(dark);
        foreach (var primary in new[] { false, true })
        {
            var button = new Button { Content = "작업 확인" }; Prepare(button, resources);
            if (primary) button.Style = (Style)resources["PrimaryButton"];
            Layout(button, 170); var chrome = Part<Border>(button, "Chrome");
            SetReadOnly(button, "IsMouseOverPropertyKey", true); Layout(button, 170);
            Assert.Same(resources[primary ? "PrimaryHoverBrush" : "ButtonHoverBrush"], chrome.Background);
            AssertReadable(button.Foreground, chrome.Background);
            SetReadOnly(button, "IsPressedPropertyKey", true); SetReadOnly(button, "IsKeyboardFocusedPropertyKey", true); Layout(button, 170);
            Assert.Same(resources[primary ? "PrimaryPressedBrush" : "ButtonPressedBrush"], chrome.Background);
            AssertReadable(button.Foreground, chrome.Background);
            var focus = Part<Border>(button, "FocusRing");
            Assert.Equal(Visibility.Visible, focus.Visibility); Assert.Equal(new Thickness(0), focus.Margin);
            AssertReadable(focus.BorderBrush, chrome.Background);
            button.IsEnabled = false; Layout(button, 170);
            Assert.Same(resources["DisabledBrush"], chrome.Background);
            Assert.Same(resources["DisabledInkBrush"], button.Foreground);
            Assert.Equal(Visibility.Collapsed, focus.Visibility);
            Assert.Equal(Visibility.Collapsed, Part<Border>(button, "PressedOutline").Visibility);
            AssertReadable(button.Foreground, chrome.Background);
        }
    });

    [Fact]
    public void SelectedTabAndToggleRemainDistinctOnHoverAndDisableCleanly() => OnSta(() =>
    {
        var resources = LoadResources(false);
        var tab = new TabItem { Header = "실행 상태", IsSelected = true }; Prepare(tab, resources); Layout(tab, 150);
        SetReadOnly(tab, "IsMouseOverPropertyKey", true); SetReadOnly(tab, "IsKeyboardFocusWithinPropertyKey", true); Layout(tab, 150);
        Assert.Same(resources["SelectionBrush"], Part<Border>(tab, "TabTile").Background);
        Assert.Equal(new Thickness(0), Part<Border>(tab, "TabFocus").Margin);
        Assert.Equal(Visibility.Visible, Part<Border>(tab, "TabFocus").Visibility);
        tab.IsEnabled = false; Layout(tab, 150);
        Assert.Same(resources["DisabledInkBrush"], tab.Foreground);
        Assert.Equal(Visibility.Collapsed, Part<Border>(tab, "TabFocus").Visibility);
        var toggle = new ToggleButton { Content = "자동 확인", IsChecked = true }; Prepare(toggle, resources); Layout(toggle, 150);
        SetReadOnly(toggle, "IsMouseOverPropertyKey", true); Layout(toggle, 150);
        Assert.Same(resources["SelectionBrush"], Part<Border>(toggle, "Chrome").Background);
        toggle.IsEnabled = false; Layout(toggle, 150);
        Assert.Same(resources["DisabledBrush"], Part<Border>(toggle, "Chrome").Background);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ComboHonorsThemePropertiesFocusDisabledAndConstrainedSelectedText(bool dark) => OnSta(() =>
    {
        var resources = LoadResources(dark);
        var combo = new ComboBox { ItemsSource = new[] { LongLabel }, SelectedIndex = 0 }; Prepare(combo, resources); Layout(combo, 150);
        Assert.Equal(40, combo.MinHeight); Assert.Equal(new Thickness(12, 8, 12, 8), combo.Padding);
        var chrome = Part<Border>(combo, "ComboChrome");
        var customBrush = Brushes.PapayaWhip; combo.Background = customBrush; Layout(combo, 150);
        Assert.Same(customBrush, chrome.Background);
        var label = Descendants(combo).OfType<TextBlock>().Single(t => t.Text == LongLabel);
        Assert.Equal(TextWrapping.Wrap, label.TextWrapping); Assert.True(combo.DesiredSize.Height > 40);
        SetReadOnly(combo, "IsKeyboardFocusWithinPropertyKey", true); Layout(combo, 150);
        Assert.Equal(Visibility.Visible, Part<Border>(combo, "ComboFocus").Visibility);
        Assert.Equal(new Thickness(0), Part<Border>(combo, "ComboFocus").Margin);
        combo.IsEnabled = false; Layout(combo, 150);
        Assert.Equal(Visibility.Collapsed, Part<Border>(combo, "ComboFocus").Visibility);
        Assert.Same(resources["DisabledBrush"], chrome.Background);
        AssertReadable(combo.Foreground, chrome.Background);
    });

    private static ResourceDictionary LoadResources(bool dark)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ai-control-tower", "src", "AIControlTower", "App.xaml"))) directory = directory.Parent;
        Assert.NotNull(directory);
        var document = XDocument.Load(Path.Combine(directory.FullName, "ai-control-tower", "src", "AIControlTower", "App.xaml"));
        var root = document.Root!;
        var dictionary = new XElement(root.Name.Namespace + "ResourceDictionary", root.Attributes().Where(a => a.IsNamespaceDeclaration), root.Elements().Single().Nodes());
        // Loose XAML needs the same local assembly context that the app's BAML compiler supplies.
        foreach (var declaration in dictionary.Attributes().Where(a => a.IsNamespaceDeclaration && a.Value.StartsWith("clr-namespace:AIControlTower") && !a.Value.Contains(";assembly=")))
        {
            var localNamespace = declaration.Value;
            declaration.Value += ";assembly=AIControlTower";
            foreach (var element in dictionary.Descendants().Where(e => e.Name.NamespaceName == localNamespace))
                element.Name = XName.Get(element.Name.LocalName, declaration.Value);
        }
        // Actual WPF resources/templates, without constructing App or running its remote/settings startup.
        var resources = (ResourceDictionary)XamlReader.Parse(dictionary.ToString(), new ParserContext
        { BaseUri = new Uri(Path.Combine(directory.FullName, "ai-control-tower", "src", "AIControlTower", "App.xaml")) });
        foreach (var item in ThemeService.Palette(dark)) resources[item.Key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.Value));
        foreach (var item in ThemeService.Gradients(dark))
        {
            var brush = new LinearGradientBrush();
            for (var i = 0; i < item.Value.Length; i++) brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(item.Value[i]), (double)i / (item.Value.Length - 1)));
            resources[item.Key] = brush;
        }
        return resources;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CheckboxUsesAFullHitTargetAndReadableDisabledCheckedState(bool dark) => OnSta(() =>
    {
        var resources = LoadResources(dark);
        var check = new CheckBox { Content = LongLabel, IsChecked = true }; Prepare(check, resources); Layout(check, 150);
        Assert.Equal(40, check.MinHeight);
        Assert.Equal(TextWrapping.Wrap, Descendants(check).OfType<AccessText>().Single().TextWrapping);
        Assert.True(check.DesiredSize.Height > 40);
        SetReadOnly(check, "IsKeyboardFocusedPropertyKey", true); Layout(check, 150);
        Assert.Equal(Visibility.Visible, Part<Border>(check, "CheckFocus").Visibility);
        check.IsEnabled = false; Layout(check, 150);
        Assert.Same(resources["DisabledInkBrush"], check.Foreground);
        Assert.Equal(Visibility.Collapsed, Part<Border>(check, "CheckFocus").Visibility);
        AssertReadable(Part<System.Windows.Shapes.Path>(check, "Tick").Stroke, Part<Border>(check, "CheckFace").Background);
    });

    [Fact]
    public void ComboWrappingPreservesCustomTemplatesDisplayMemberAndStringFormat() => OnSta(() =>
    {
        var resources = LoadResources(false);
        var template = (DataTemplate)XamlReader.Parse("<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><TextBlock Text='{Binding}' FontSize='19' TextWrapping='NoWrap'/></DataTemplate>");
        var custom = new ComboBox { ItemsSource = new[] { LongLabel }, SelectedIndex = 0, ItemTemplate = template };
        Prepare(custom, resources); Layout(custom, 150);
        var label = Descendants(custom).OfType<TextBlock>().Single(t => t.Text == LongLabel);
        Assert.Equal(19, label.FontSize); Assert.Equal(TextWrapping.NoWrap, label.TextWrapping);
        var display = new ComboBox { ItemsSource = new[] { new NamedChoice(LongLabel) }, SelectedIndex = 0, DisplayMemberPath = "Name" };
        Prepare(display, resources); Layout(display, 150);
        Assert.Contains(Descendants(display).OfType<TextBlock>(), t => t.Text == LongLabel);
        var formatted = new ComboBox { ItemsSource = new[] { "작업" }, SelectedIndex = 0, ItemStringFormat = "선택: {0}" };
        Prepare(formatted, resources); Layout(formatted, 150);
        Assert.Contains(Descendants(formatted).OfType<TextBlock>(), t => t.Text == "선택: 작업");
        var converter = (ComboStringTemplateConverter)resources["ComboSelectionTemplate"];
        var original = new DataTemplate(); var selector = new DataTemplateSelector();
        Assert.Same(original, converter.Convert([original, null!, selector, LongLabel, null!], typeof(DataTemplate), null!, System.Globalization.CultureInfo.InvariantCulture));
    });

    private sealed record NamedChoice(string Name);

    private static void Prepare(Control control, ResourceDictionary resources)
    {
        control.Resources = resources; control.Style = (Style)resources[control.GetType()];
        control.FontFamily = new FontFamily("Segoe UI, Malgun Gothic"); control.FontSize = 14;
    }
    private static void Layout(Control control, double width)
    {
        control.ApplyTemplate(); control.Measure(new Size(width, double.PositiveInfinity));
        control.Arrange(new Rect(0, 0, width, control.DesiredSize.Height)); control.UpdateLayout();
    }
    private static T Part<T>(Control control, string name) where T : FrameworkElement => Assert.IsType<T>(control.Template.FindName(name, control));
    private static void SetReadOnly(DependencyObject control, string keyName, object value)
    {
        FieldInfo? field = null;
        for (var type = control.GetType(); type is not null && field is null; type = type.BaseType)
            field = type.GetField(keyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(field);
        control.SetValue((DependencyPropertyKey)field.GetValue(null)!, value);
    }
    private static void AssertReadable(Brush foreground, Brush background) => Assert.True(ThemeVerification.Ratio(((SolidColorBrush)foreground).Color, ((SolidColorBrush)background).Color) >= 4.5);
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
