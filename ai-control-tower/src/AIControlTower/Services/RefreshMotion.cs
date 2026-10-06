using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace AIControlTower.Services;
/// <summary>A single compositor clock repeats continuously; stopping finishes the current turn.</summary>
public static class RefreshMotion
{
 public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached("Active",typeof(bool),typeof(RefreshMotion),new PropertyMetadata(false,Changed));
 public static readonly DependencyProperty ReduceMotionProperty = DependencyProperty.RegisterAttached("ReduceMotion",typeof(bool),typeof(RefreshMotion),new PropertyMetadata(false,Changed));
 public static readonly DependencyProperty DurationMillisecondsProperty = DependencyProperty.RegisterAttached("DurationMilliseconds",typeof(double),typeof(RefreshMotion),new PropertyMetadata(1200d,Changed));
 private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached("State",typeof(State),typeof(RefreshMotion));
 public static void SetActive(DependencyObject target,bool value) => target.SetValue(ActiveProperty,value);
 public static bool GetActive(DependencyObject target) => (bool)target.GetValue(ActiveProperty);
 public static void SetReduceMotion(DependencyObject target,bool value) => target.SetValue(ReduceMotionProperty,value);
 public static bool GetReduceMotion(DependencyObject target) => (bool)target.GetValue(ReduceMotionProperty);
 public static void SetDurationMilliseconds(DependencyObject target,double value) => target.SetValue(DurationMillisecondsProperty,value);
 public static double GetDurationMilliseconds(DependencyObject target) => (double)target.GetValue(DurationMillisecondsProperty);
 private sealed class State { public RotateTransform Rotation { get; } = new(); public bool Running; public bool Finishing; public int Generation; }
 private static void Changed(DependencyObject target,DependencyPropertyChangedEventArgs args)
 {
  if (target is not FrameworkElement element) return;
  if (element.GetValue(StateProperty) is not State state)
  {
   state = new State(); element.SetValue(StateProperty,state); element.RenderTransformOrigin = new Point(.5,.5); element.RenderTransform = state.Rotation;
   element.Unloaded += (_,_) => Stop(state); element.Loaded += (_,_) => Update(element,state);
  }
  Update(element,state);
 }
 private static void Update(FrameworkElement element,State state)
 {
  if (!element.IsLoaded || GetReduceMotion(element) || !SystemParameters.ClientAreaAnimation) { Stop(state); return; }
  if (GetActive(element))
  {
   if (state.Running && !state.Finishing) return;
   var angle = state.Rotation.Angle % 360; state.Generation++; state.Running = true; state.Finishing = false;
   // Linear interpolation keeps angular velocity at every wrap; no timer, easing or per-turn restart.
   var animation = new DoubleAnimation(angle,angle+360,TimeSpan.FromMilliseconds(Math.Clamp(GetDurationMilliseconds(element),600,3000))) { RepeatBehavior = RepeatBehavior.Forever };
   state.Rotation.BeginAnimation(RotateTransform.AngleProperty,animation,HandoffBehavior.SnapshotAndReplace);
  }
  else if (state.Running && !state.Finishing)
  {
   state.Finishing = true; var generation = ++state.Generation; var angle = state.Rotation.Angle % 360;
   var animation = new DoubleAnimation(angle,360,TimeSpan.FromMilliseconds(Math.Clamp(GetDurationMilliseconds(element),600,3000)*(360-angle)/360)) { FillBehavior = FillBehavior.HoldEnd };
   animation.Completed += (_,_) => { if (generation == state.Generation) Stop(state); };
   state.Rotation.BeginAnimation(RotateTransform.AngleProperty,animation,HandoffBehavior.SnapshotAndReplace);
  }
 }
 private static void Stop(State state)
 {
  state.Generation++; state.Running=false; state.Finishing=false; state.Rotation.BeginAnimation(RotateTransform.AngleProperty,null); state.Rotation.Angle=0;
 }
}
