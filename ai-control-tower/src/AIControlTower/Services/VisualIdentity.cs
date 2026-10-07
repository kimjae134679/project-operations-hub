using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace AIControlTower.Services;

/// <summary>Role pictograms, with full product names beside them; no initials or guessed logos.</summary>
public sealed class IdentityIcon : Border
{
    public static readonly DependencyProperty IdentityProperty = DependencyProperty.Register(nameof(Identity), typeof(string), typeof(IdentityIcon), new PropertyMetadata("", Changed));
    public string Identity { get => (string)GetValue(IdentityProperty); set => SetValue(IdentityProperty, value); }
    private readonly Path _path = new() { Stretch = Stretch.Uniform, StrokeThickness = 1.7, StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
    public IdentityIcon()
    {
        Width = Height = 42; CornerRadius = new CornerRadius(11); Padding = new Thickness(10);
        SetResourceReference(BackgroundProperty, "ChipBrush"); _path.SetResourceReference(Shape.StrokeProperty, "ChipInkBrush"); Child = _path;
        Update();
    }
    private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((IdentityIcon)d).Update();
    private void Update()
    {
        var s = (Identity ?? "").ToLowerInvariant();
        var shape = s.Contains("voice") || s.Contains("오디오") || s.Contains("zonos") || s.Contains("무직") ? "M2,12 L2,7 C2,0 22,0 22,7 L22,12 M2,10 L6,10 L6,18 L2,18 Z M18,10 L22,10 L22,18 L18,18 Z M10,8 L10,16 M14,6 L14,18"
            : s.Contains("hyper") || s.Contains("영상") || s.Contains("downloader") ? "M2,2 L22,2 L22,22 L2,22 Z M7,2 L7,22 M17,2 L17,22 M2,7 L7,7 M2,17 L7,17 M17,7 L22,7 M17,17 L22,17 M10,8 L15,12 L10,16 Z"
            : s.Contains("github") ? "M6,3 A3,3 0 1 1 5.9,3 M18,4 A3,3 0 1 1 17.9,4 M6,18 A3,3 0 1 1 5.9,18 M6,6 L6,15 M6,12 C18,12 18,12 18,7"
            : s.Contains("jev") || s.Contains("n8n") || s.Contains("전달") || s.Contains("actions") ? "M1,9 L7,9 L7,15 L1,15 Z M17,1 L23,1 L23,7 L17,7 Z M17,17 L23,17 L23,23 L17,23 Z M7,12 L12,12 L12,4 L17,4 M12,12 L12,20 L17,20"
            : s.Contains("codex") || s.Contains("aider") || s.Contains("code") ? "M8,5 L1,12 L8,19 M16,5 L23,12 L16,19 M14,2 L10,22"
            : s.Contains("remote") || s.Contains("desktop") || s.Contains("bridge") || s.Contains("연결") ? "M1,2 L23,2 L23,17 L1,17 Z M8,22 L16,22 M12,17 L12,22 M7,9 L17,9 M14,6 L17,9 L14,12"
            : s.Contains("phone") || s.Contains("multi") || s.Contains("멀티") || s.Contains("게임") ? "M5,6 L19,6 C24,6 26,21 21,21 L16,16 L8,16 L3,21 C-2,21 0,6 5,6 Z M5,9 L5,15 M2,12 L8,12 M17,10 L17,11 M20,13 L20,14"
            : s.Contains("stock") || s.Contains("주식") ? "M2,2 L2,22 L23,22 M5,17 L10,12 L14,15 L22,5 M17,5 L22,5 L22,10"
            : s.Contains("housing") || s.Contains("청약") ? "M1,11 L12,1 L23,11 M4,9 L4,23 L20,23 L20,9 M9,23 L9,15 L15,15 L15,23"
            : s.Contains("hub") || s.Contains("관리") || s.Contains("control") ? "M1,1 L10,1 L10,10 L1,10 Z M14,1 L23,1 L23,10 L14,10 Z M1,14 L10,14 L10,23 L1,23 Z M14,14 L23,14 L23,23 L14,23 Z"
            : "M1,5 L10,5 L12,8 L23,8 L23,22 L1,22 Z M1,8 L1,3 L9,3 L11,5";
        _path.Data = Geometry.Parse(shape);
    }
}

/// <summary>Color and symbol supplement an explicit status label. Unverified is never green.</summary>
public sealed class StatusBadge : Border
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(StatusBadge), new PropertyMetadata("", Changed));
    public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StatusBadge), new PropertyMetadata(false, Changed));
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public bool HasError { get => (bool)GetValue(HasErrorProperty); set => SetValue(HasErrorProperty,value); }
    private readonly TextBlock _label = new() { FontSize = 12, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
    public StatusBadge() { CornerRadius = new CornerRadius(7); Padding = new Thickness(9,5,9,5); Child = _label; Update(); }
    private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((StatusBadge)d).Update();
    private void Update()
    {
        var value = Value ?? "";
        Visibility = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;
        var s = value.ToLowerInvariant(); var tone = "Neutral"; var symbol = "○";
        if (new[] { "오류", "실패", "막힘", "초과", "error", "failed", "blocked", "404" }.Any(s.Contains)) { tone = "Error"; symbol = "!"; }
        else if (new[] { "미확인", "필요", "미설치", "unknown", "notinstalled", "notconfigured", "대기", "queued", "waiting", "확인 중", "점검 중" }.Any(s.Contains)) { tone = "Warning"; symbol = "◷"; }
        else if (new[] { "중지", "중단", "종료", "실행 전", "stopped", "cancelled", "paused", "disconnected" }.Any(s.Contains)) { symbol = "■"; }
        else if (new[] { "실행 중", "처리 중", "연결됨", "running", "connected" }.Any(s.Contains)) { tone = "Running"; symbol = "●"; }
        else if (new[] { "완료", "확인됨", "확인", "적용", "ready", "succeeded", "applied" }.Any(s.Contains)) { tone = "Success"; symbol = "✓"; }
        if (value.EndsWith(" 0명",StringComparison.Ordinal) || value.EndsWith(" 0개 프로젝트",StringComparison.Ordinal)) { tone = "Neutral"; symbol = "○"; }
        if (HasError) { tone = "Error"; symbol = "!"; }
        SetResourceReference(BackgroundProperty, "Status" + tone + "Face"); _label.SetResourceReference(TextBlock.ForegroundProperty, "Status" + tone + "Ink");
        _label.Text = symbol + "  " + value;
        System.Windows.Automation.AutomationProperties.SetName(this, value);
    }
}
