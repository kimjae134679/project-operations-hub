using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AIControlTower.Services;

/// <summary>Isolated visual timing check; this does not claim a successful server request.</summary>
public static class RefreshMotionVerification
{
    public static async Task<object> RunAsync(MainWindow window)
    {
        var icon = (TextBlock)window.FindName("RosterSpinner");
        var activeBinding = BindingOperations.GetBindingBase(icon, RefreshMotion.ActiveProperty);
        var reduceBinding = BindingOperations.GetBindingBase(icon, RefreshMotion.ReduceMotionProperty);
        try
        {
            BindingOperations.ClearBinding(icon, RefreshMotion.ActiveProperty);
            BindingOperations.ClearBinding(icon, RefreshMotion.ReduceMotionProperty);
            RefreshMotion.SetReduceMotion(icon, true);
            RefreshMotion.SetActive(icon, false);
            if (!SystemParameters.ClientAreaAnimation) return new { MotionDisabledByWindows = true, OneTurnVerified = false };
            RefreshMotion.SetReduceMotion(icon, false);
            RefreshMotion.SetActive(icon, true);
            await Task.Delay(120);
            var before = ((RotateTransform)icon.RenderTransform).Angle;
            RefreshMotion.SetActive(icon, false);
            await Task.Delay(140);
            var after = ((RotateTransform)icon.RenderTransform).Angle;
            if (before <= 0 || after <= before) throw new InvalidOperationException("Short refresh snapped or stopped before one full turn.");
            await Task.Delay((int)RefreshMotion.GetDurationMilliseconds(icon)+150);
            var completed = ((RotateTransform)icon.RenderTransform).Angle;
            if (Math.Abs(completed) > 0.01) throw new InvalidOperationException("Refresh did not return naturally to idle.");
            // Sample through a complete wrap to catch per-turn pauses, reversals and frame stepping.
            RefreshMotion.SetActive(icon, true);
            var samples = new List<double>();
            var speed = RefreshMotion.GetDurationMilliseconds(icon);
            for (var i = 0; i < 18; i++)
            {
                await Task.Delay((int)(speed / 12));
                samples.Add(((RotateTransform)icon.RenderTransform).Angle);
            }
            var deltas = samples.Zip(samples.Skip(1), (a,b) => (b-a+360)%360).ToArray();
            if (deltas.Any(d => d < 8 || d > 85)) throw new InvalidOperationException("Continuous clock lost smooth forward motion across a turn.");
            RefreshMotion.SetActive(icon, true);
            await Task.Delay(90);
            RefreshMotion.SetReduceMotion(icon, true);
            var reduced = ((RotateTransform)icon.RenderTransform).Angle;
            if (reduced != 0) throw new InvalidOperationException("Reduce motion preference did not stop refresh animation.");
            return new { MotionDisabledByWindows = false, OneTurnVerified = true, ContinuousClockVerified = true, AngleBeforeRequestEnd = before, AngleAfterRequestEnd = after, IdleAngle = completed, ReducedMotionAngle = reduced, MinimumSampleAdvance = deltas.Min(), MaximumSampleAdvance = deltas.Max() };
        }
        finally
        {
            RefreshMotion.SetActive(icon, false);
            RefreshMotion.SetReduceMotion(icon, true);
            if (activeBinding is not null) BindingOperations.SetBinding(icon, RefreshMotion.ActiveProperty, activeBinding);
            if (reduceBinding is not null) BindingOperations.SetBinding(icon, RefreshMotion.ReduceMotionProperty, reduceBinding);
        }
    }
}
