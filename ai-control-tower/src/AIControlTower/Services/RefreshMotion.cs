using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AIControlTower.Services;

/// <summary>Finish the current turn instead of snapping to zero when a short request ends.</summary>
public static class RefreshMotion
{
    public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached("Active", typeof(bool), typeof(RefreshMotion), new PropertyMetadata(false, Changed));
    public static readonly DependencyProperty ReduceMotionProperty = DependencyProperty.RegisterAttached("ReduceMotion", typeof(bool), typeof(RefreshMotion), new PropertyMetadata(false, Changed));
    public static readonly DependencyProperty TurnMillisecondsProperty = DependencyProperty.RegisterAttached("TurnMilliseconds", typeof(int), typeof(RefreshMotion), new PropertyMetadata(1200));
    public static void SetTurnMilliseconds(DependencyObject target, int value) => target.SetValue(TurnMillisecondsProperty, value);
    public static int GetTurnMilliseconds(DependencyObject target) => (int)target.GetValue(TurnMillisecondsProperty);
    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached("State", typeof(State), typeof(RefreshMotion));
    public static void SetActive(DependencyObject target, bool value) => target.SetValue(ActiveProperty, value);
    public static bool GetActive(DependencyObject target) => (bool)target.GetValue(ActiveProperty);
    public static void SetReduceMotion(DependencyObject target, bool value) => target.SetValue(ReduceMotionProperty, value);
    public static bool GetReduceMotion(DependencyObject target) => (bool)target.GetValue(ReduceMotionProperty);

    private sealed class State
    {
        public RotateTransform Rotation { get; } = new();
        public bool Running;
        public int Generation;
    }
    private static void Changed(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is not FrameworkElement element) return;
        if (element.GetValue(StateProperty) is not State state)
        {
            state = new State();
            element.SetValue(StateProperty, state);
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform = state.Rotation;
            element.Unloaded += (_, _) => Stop(state);
            element.Loaded += (_, _) => Update(element, state);
        }
        Update(element, state);
    }
    private static void Update(FrameworkElement element, State state)
    {
        if (GetReduceMotion(element) || !SystemParameters.ClientAreaAnimation) { Stop(state); return; }
        if (GetActive(element) && element.IsLoaded && !state.Running) Turn(element, state);
        // Active=false deliberately leaves the existing animation alive until 360 degrees.
    }
    private static void Stop(State state)
    {
        state.Generation++;
        state.Running = false;
        state.Rotation.BeginAnimation(RotateTransform.AngleProperty, null);
        state.Rotation.Angle = 0;
    }
    private static void Turn(FrameworkElement element, State state)
    {
        state.Running = true;
        var generation = ++state.Generation;
        var animation = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(Math.Clamp(GetTurnMilliseconds(element), 600, 3000))) { FillBehavior = FillBehavior.HoldEnd };
        animation.Completed += (_, _) =>
        {
            if (generation != state.Generation) return;
            state.Running = false;
            if (GetActive(element) && element.IsLoaded && !GetReduceMotion(element) && SystemParameters.ClientAreaAnimation)
                Turn(element, state);
            else Stop(state); // 360 and 0 have the same visual orientation.
        };
        state.Rotation.BeginAnimation(RotateTransform.AngleProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }
}
