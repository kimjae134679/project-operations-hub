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

    // Neutral surfaces; blue-violet is reserved for emphasis and focus.
    internal static Dictionary<string, string> Palette(bool dark) => dark ? new()
    {
        ["CanvasBrush"]="#15171D",
        ["SurfaceBrush"]="#1D2028",
        ["RaisedBrush"]="#262A35",
        ["LineBrush"]="#66728A",
        ["TextBrush"]="#F3F5FA",
        ["MutedBrush"]="#B8C0D0",
        ["AccentBrush"]="#A3AFFE",
        ["AccentInkBrush"]="#FFFFFF",
        ["DangerBrush"]="#FFB4BC",
        ["RailBrush"]="#191C24",
        ["RailTextBrush"]="#F3F5FA",
        ["RailMutedBrush"]="#B8C0D0",
        ["RailHoverBrush"]="#282E3C",
        ["RailSelectedBrush"]="#313954",
        ["RailEdgeBrush"]="#66728A",
        ["SelectionBrush"]="#313954",
        ["HoverBrush"]="#2B3040",
        ["FocusBrush"]="#B3BDFF",
        ["ShadowBrush"]="#101218",
        ["SheetBackBrush"]="#222631",
        ["SheetMiddleBrush"]="#2C3140",
        ["HeroBrush"]="#252A37",
        ["ChipBrush"]="#323A56",
        ["ChipInkBrush"]="#EDF0FF",
        ["BadgeBrush"]="#363D65",
        ["BadgeInkBrush"]="#EDF0FF",
        ["ScrollThumbBrush"]="#8E99AE",
        ["DisabledBrush"]="#252A35",
        ["DisabledInkBrush"]="#AAB4C8",
        ["ButtonFootBrush"]="#1D2028",
        ["HighlightBrush"]="#00FFFFFF"
    } : new()
    {
        ["CanvasBrush"]="#F2F4F8",
        ["SurfaceBrush"]="#FFFFFF",
        ["RaisedBrush"]="#EBEEF4",
        ["LineBrush"]="#7E8A9F",
        ["TextBrush"]="#1F2634",
        ["MutedBrush"]="#536077",
        ["AccentBrush"]="#4757BC",
        ["AccentInkBrush"]="#FFFFFF",
        ["DangerBrush"]="#A82B44",
        ["RailBrush"]="#F7F8FB",
        ["RailTextBrush"]="#1F2634",
        ["RailMutedBrush"]="#536077",
        ["RailHoverBrush"]="#E8ECF4",
        ["RailSelectedBrush"]="#E2E7FA",
        ["RailEdgeBrush"]="#7E8A9F",
        ["SelectionBrush"]="#E2E7FA",
        ["HoverBrush"]="#EEF1F8",
        ["FocusBrush"]="#4757BC",
        ["ShadowBrush"]="#DCE1EA",
        ["SheetBackBrush"]="#DCE1EB",
        ["SheetMiddleBrush"]="#EBEFF5",
        ["HeroBrush"]="#F4F5FB",
        ["ChipBrush"]="#E7EBF7",
        ["ChipInkBrush"]="#34417D",
        ["BadgeBrush"]="#E7EAFB",
        ["BadgeInkBrush"]="#34417D",
        ["ScrollThumbBrush"]="#79869C",
        ["DisabledBrush"]="#E6E9EF",
        ["DisabledInkBrush"]="#596378",
        ["ButtonFootBrush"]="#FFFFFF",
        ["HighlightBrush"]="#00FFFFFF"
    };

    // Keep the existing brush keys, with identical stops for flat surfaces.
    internal static Dictionary<string, string[]> Gradients(bool dark) => dark ? new()
    {
        ["ButtonFace"]=["#292E3C","#292E3C"],
        ["ActionFace"]=["#5361C4","#5361C4"],
        ["CardFace"]=["#1D2028","#1D2028"],
        ["SelectionFace"]=["#313954","#313954"],
        ["RimBrush"]=["#66728A","#66728A"],
        ["RailMaterial"]=["#191C24","#191C24"],
        ["HeroFace"]=["#252A37","#252A37"]
    } : new()
    {
        ["ButtonFace"]=["#FFFFFF","#FFFFFF"],
        ["ActionFace"]=["#4757BC","#4757BC"],
        ["CardFace"]=["#FFFFFF","#FFFFFF"],
        ["SelectionFace"]=["#E2E7FA","#E2E7FA"],
        ["RimBrush"]=["#7E8A9F","#7E8A9F"],
        ["RailMaterial"]=["#F7F8FB","#F7F8FB"],
        ["HeroFace"]=["#F4F5FB","#F4F5FB"]
    };
}
