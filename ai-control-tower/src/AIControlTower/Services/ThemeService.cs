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
        ["CanvasBrush"]="#181A1E", ["SurfaceBrush"]="#23262B", ["RaisedBrush"]="#2D3036",
        ["LineBrush"]="#50545D", ["TextBrush"]="#F5F6F8", ["MutedBrush"]="#C6CBD4",
        ["AccentBrush"]="#BDD1FF", ["AccentInkBrush"]="#FFFFFF", ["DangerBrush"]="#FFA7AD",
        ["RailBrush"]="#202329", ["RailTextBrush"]="#F5F6F8", ["RailMutedBrush"]="#C6CBD4",
        ["RailHoverBrush"]="#2D3036", ["RailSelectedBrush"]="#353942", ["RailEdgeBrush"]="#666C79",
        ["SelectionBrush"]="#353942", ["HoverBrush"]="#2D3036", ["FocusBrush"]="#BDD1FF",
        ["ShadowBrush"]="#0E1013", ["SheetBackBrush"]="#363A42", ["SheetMiddleBrush"]="#414650",
        ["HeroBrush"]="#2B2E34", ["ChipBrush"]="#353942", ["ChipInkBrush"]="#F5F6F8",
        ["BadgeBrush"]="#D6DCE8", ["BadgeInkBrush"]="#23262B", ["ScrollThumbBrush"]="#818895",
        ["DisabledBrush"]="#30343B", ["DisabledInkBrush"]="#BFC5D0", ["ButtonFootBrush"]="#17191D",
        ["HighlightBrush"]="#21FFFFFF"
    } : new()
    {
        ["CanvasBrush"]="#F0F1F4", ["SurfaceBrush"]="#FFFFFF", ["RaisedBrush"]="#E9ECF1",
        ["LineBrush"]="#CBD0D9", ["TextBrush"]="#23262B", ["MutedBrush"]="#505866",
        ["AccentBrush"]="#284E91", ["AccentInkBrush"]="#FFFFFF", ["DangerBrush"]="#A1273C",
        ["RailBrush"]="#FFFFFF", ["RailTextBrush"]="#23262B", ["RailMutedBrush"]="#505866",
        ["RailHoverBrush"]="#EEF0F4", ["RailSelectedBrush"]="#E3E8F1", ["RailEdgeBrush"]="#AEB9CD",
        ["SelectionBrush"]="#E3E8F1", ["HoverBrush"]="#EEF0F4", ["FocusBrush"]="#284E91",
        ["ShadowBrush"]="#D0D5DF", ["SheetBackBrush"]="#D2D7E0", ["SheetMiddleBrush"]="#E1E5ED",
        ["HeroBrush"]="#F7F8FA", ["ChipBrush"]="#E3E8F1", ["ChipInkBrush"]="#343E50",
        ["BadgeBrush"]="#E3E8F1", ["BadgeInkBrush"]="#23262B", ["ScrollThumbBrush"]="#919AA9",
        ["DisabledBrush"]="#E2E5EB", ["DisabledInkBrush"]="#505866", ["ButtonFootBrush"]="#C9CED8",
        ["HighlightBrush"]="#72FFFFFF"
    };

    internal static Dictionary<string, string[]> Gradients(bool dark) => dark ? new()
    {
        ["ButtonFace"]=["#33373F","#2D3036"], ["ActionFace"]=["#345EA1","#284E91"],
        ["CardFace"]=["#2B2E34","#23262B"], ["SelectionFace"]=["#353942","#30343B"],
        ["RimBrush"]=["#686F7C","#454B56"], ["RailMaterial"]=["#25282E","#202329"],
        ["HeroFace"]=["#2D3036","#25282E"]
    } : new()
    {
        ["ButtonFace"]=["#FFFFFF","#F3F4F7"], ["ActionFace"]=["#345EA1","#284E91"],
        ["CardFace"]=["#FFFFFF","#F5F6F9"], ["SelectionFace"]=["#E9EDF5","#E3E8F1"],
        ["RimBrush"]=["#E9ECF1","#BEC6D3"], ["RailMaterial"]=["#FFFFFF","#F5F6F9"],
        ["HeroFace"]=["#FFFFFF","#F1F3F7"]
    };
}
