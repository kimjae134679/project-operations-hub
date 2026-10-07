using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace AIControlTower.Services;
/// <summary>A single compositor clock repeats continuously; stopping finishes the current turn.</summary>
public static class RefreshMotion
{
 public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached("Active",typeof(bool),typeof(RefreshMotion),new PropertyMetadata(false,Changed));
 public static readonly DependencyProperty ReduceMotionProperty = DependencyProperty.RegisterAttached("ReduceMotion",typeof(bool),typeof(RefreshMotion),new PropertyMetadata(false,Changed));
 public static readonly DependencyProperty DurationMillisecondsProperty = DependencyProperty.RegisterAttached("DurationMilliseconds",typeof(double),typeof(RefreshMotion),new PropertyMetadata(1400d,Changed));
 private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached("State",typeof(State),typeof(RefreshMotion));
 public static void SetActive(DependencyObject target,bool value) => target.SetValue(ActiveProperty,value);
 public static bool GetActive(DependencyObject target) => (bool)target.GetValue(ActiveProperty);
 public static void SetReduceMotion(DependencyObject target,bool value) => target.SetValue(ReduceMotionProperty,value);
 public static bool GetReduceMotion(DependencyObject target) => (bool)target.GetValue(ReduceMotionProperty);
 public static void SetDurationMilliseconds(DependencyObject target,double value) => target.SetValue(DurationMillisecondsProperty,value);
 public static double GetDurationMilliseconds(DependencyObject target) => (double)target.GetValue(DurationMillisecondsProperty);
 private sealed class State { public RotateTransform Rotation { get; } = new(); public bool Running; public bool Finishing; public int Generation; public double AppliedDuration; }
 private static double NormalizeDuration(double milliseconds)=>double.IsFinite(milliseconds)?Math.Clamp(milliseconds,600,3000):1400;
 public static bool ShouldAnimate(bool loaded,bool reduceMotion,bool systemAnimations)=>loaded && !reduceMotion && systemAnimations;
 public static bool ShouldReplaceClock(bool running,bool finishing,bool active,double applied,double requested)
 {
  if(!running)return active;
  var rateChanged=NormalizeDuration(applied)!=NormalizeDuration(requested);
  return active?finishing || rateChanged:!finishing || rateChanged;
 }
 public static DoubleAnimation CreateRotationAnimation(double angle,double milliseconds,bool finishing)
 {
  angle=double.IsFinite(angle)?((angle%360)+360)%360:0;
  var duration=NormalizeDuration(milliseconds);
  return finishing
   ? new DoubleAnimation(angle,360,TimeSpan.FromMilliseconds(duration*(360-angle)/360)) { FillBehavior=FillBehavior.HoldEnd }
   : new DoubleAnimation(angle,angle+360,TimeSpan.FromMilliseconds(duration)) { RepeatBehavior=RepeatBehavior.Forever };
 }
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
  if (!ShouldAnimate(element.IsLoaded,GetReduceMotion(element),SystemParameters.ClientAreaAnimation)) { Stop(state); return; }
  var active=GetActive(element);var duration=NormalizeDuration(GetDurationMilliseconds(element));
  if(!ShouldReplaceClock(state.Running,state.Finishing,active,state.AppliedDuration,duration))return;
  if (active)
  {
   var angle = state.Rotation.Angle; state.Generation++; state.Running = true; state.Finishing = false;state.AppliedDuration=duration;
   // Linear interpolation keeps angular velocity at every wrap; no timer, easing or per-turn restart.
   var animation = CreateRotationAnimation(angle,duration,false);
   state.Rotation.BeginAnimation(RotateTransform.AngleProperty,animation,HandoffBehavior.SnapshotAndReplace);
  }
  else if (state.Running)
  {
   state.Finishing = true; state.AppliedDuration=duration;var generation = ++state.Generation; var angle = state.Rotation.Angle;
   var animation = CreateRotationAnimation(angle,duration,true);
   animation.Completed += (_,_) => { if (generation == state.Generation) Stop(state); };
   state.Rotation.BeginAnimation(RotateTransform.AngleProperty,animation,HandoffBehavior.SnapshotAndReplace);
  }
 }
 private static void Stop(State state)
 {
  state.Generation++; state.Running=false; state.Finishing=false; state.Rotation.BeginAnimation(RotateTransform.AngleProperty,null); state.Rotation.Angle=0;
 }
}
