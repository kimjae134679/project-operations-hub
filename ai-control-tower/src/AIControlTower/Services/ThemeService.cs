using System.Windows;
using System.Windows.Media;

namespace AIControlTower.Services;

/// <summary>One palette for backgrounds and all content states, including open documents/popups.</summary>
public static class ThemeService
{
    public static bool IsDark { get; private set; }
    public static void Apply(bool dark)
    {
        IsDark = dark;
        var resources = Application.Current.Resources;
        foreach (var item in Palette(dark))
            resources[item.Key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.Value));
        foreach (var item in Gradients(dark))
        {
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0.8, 1) };
            for (var i = 0; i < item.Value.Length; i++)
                brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(item.Value[i]), (double)i / (item.Value.Length - 1)));
            resources[item.Key] = brush;
        }
    }

    internal static Dictionary<string, string> Palette(bool dark) => dark ? new()
    {
        ["CanvasBrush"]="#171C1B", ["SurfaceBrush"]="#252D2A", ["RaisedBrush"]="#303B36",
        ["LineBrush"]="#48534D", ["TextBrush"]="#F5F4EC", ["MutedBrush"]="#C0C9C0",
        ["AccentBrush"]="#C7E59A", ["AccentInkBrush"]="#FFFFFF", ["DangerBrush"]="#FFA7AD",
        ["RailBrush"]="#202923", ["RailTextBrush"]="#F1F3E9", ["RailMutedBrush"]="#B8C8B3",
        ["RailHoverBrush"]="#354137", ["RailSelectedBrush"]="#43513D", ["RailEdgeBrush"]="#526246",
        ["SelectionBrush"]="#384335", ["HoverBrush"]="#303A33", ["FocusBrush"]="#D7EEAD",
        ["ShadowBrush"]="#0C110E", ["SheetBackBrush"]="#39483A", ["SheetMiddleBrush"]="#45563D",
        ["HeroBrush"]="#303B31", ["ChipBrush"]="#394433", ["ChipInkBrush"]="#E4EDD7",
        ["BadgeBrush"]="#DDE9BE", ["BadgeInkBrush"]="#25301D", ["ScrollThumbBrush"]="#7F9185",
        ["DisabledBrush"]="#323B36", ["DisabledInkBrush"]="#AFB8AD", ["ButtonFootBrush"]="#111810",
        ["HighlightBrush"]="#21FFFFFF"
    } : new()
    {
        ["CanvasBrush"]="#EDEDE6", ["SurfaceBrush"]="#FFFDF7", ["RaisedBrush"]="#E8EADD",
        ["LineBrush"]="#CDD1C2", ["TextBrush"]="#222B24", ["MutedBrush"]="#545F51",
        ["AccentBrush"]="#3E5726", ["AccentInkBrush"]="#FFFFFF", ["DangerBrush"]="#A1273C",
        ["RailBrush"]="#263126", ["RailTextBrush"]="#FCFCF1", ["RailMutedBrush"]="#C4D0BC",
        ["RailHoverBrush"]="#354535", ["RailSelectedBrush"]="#46583C", ["RailEdgeBrush"]="#769163",
        ["SelectionBrush"]="#E1E8D2", ["HoverBrush"]="#EFF0E5", ["FocusBrush"]="#425F25",
        ["ShadowBrush"]="#C2C8B8", ["SheetBackBrush"]="#BBC6A6", ["SheetMiddleBrush"]="#CCD5B9",
        ["HeroBrush"]="#F6F5E9", ["ChipBrush"]="#E2E8D3", ["ChipInkBrush"]="#3D4D30",
        ["BadgeBrush"]="#DDE9BE", ["BadgeInkBrush"]="#25301D", ["ScrollThumbBrush"]="#8A9680",
        ["DisabledBrush"]="#E1E4D9", ["DisabledInkBrush"]="#55614F", ["ButtonFootBrush"]="#AFB7A1",
        ["HighlightBrush"]="#72FFFFFF"
    };

    internal static Dictionary<string, string[]> Gradients(bool dark) => dark ? new()
    {
        ["ButtonFace"]=["#3B463D","#303B33"], ["ActionFace"]=["#53733B","#3B5529"],
        ["CardFace"]=["#2D362F","#252D28"], ["SelectionFace"]=["#3D4937","#333F31"],
        ["RimBrush"]=["#61715B","#43503E"], ["RailMaterial"]=["#2D382C","#202920"],
        ["HeroFace"]=["#35422F","#2B352D"]
    } : new()
    {
        ["ButtonFace"]=["#FFFDF7","#E4E7DA"], ["ActionFace"]=["#48662F","#304921"],
        ["CardFace"]=["#FFFDF7","#F0F0E5"], ["SelectionFace"]=["#EAF0DC","#DDE5CD"],
        ["RimBrush"]=["#E9ECDF","#BBC5AF"], ["RailMaterial"]=["#344331","#253025"],
        ["HeroFace"]=["#FCFBEF","#EAEDD9"]
    };
}
