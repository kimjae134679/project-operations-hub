using System.Reflection;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using AIControlTower.Services;

namespace AIControlTower.Tests;

/// <summary>Detached timeline/policy/source fixtures only. No windows, settings, clocks on user controls or I/O writes.</summary>
public sealed class RefreshMotionTests
{
    [Fact]
    public void RosterSpinnerBindsConfiguredTurnDuration()
    {
        var source=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../src/AIControlTower/MainWindow.xaml"));
        var xml=XDocument.Load(source);XNamespace x="http://schemas.microsoft.com/winfx/2006/xaml";XNamespace service="clr-namespace:AIControlTower.Services";
        var spinner=Assert.Single(xml.Descendants(),e=>(string?)e.Attribute(x+"Name")=="RosterSpinner");
        Assert.Equal("{Binding RefreshTurnMilliseconds}",(string?)spinner.Attribute(service+"RefreshMotion.DurationMilliseconds"));
        Assert.Equal("{Binding IsUpdatingRoster}",(string?)spinner.Attribute(service+"RefreshMotion.Active"));
    }

    [Fact]
    public void DetachedRotationTimelineKeepsLinearVelocityAcrossWholeTurns()
    {
        InSta(()=>
        {
            var animation=Animation(90,1400,false);
            Assert.Equal(90d,animation.From);Assert.Equal(450d,animation.To);
            Assert.Equal(TimeSpan.FromMilliseconds(1400),animation.Duration.TimeSpan);
            Assert.Equal(RepeatBehavior.Forever,animation.RepeatBehavior);Assert.Null(animation.EasingFunction);
            var clock=animation.CreateClock();Assert.NotNull(clock); // Never attached to a user control/window.
            Assert.Equal(360d/1400,(animation.To!.Value-animation.From!.Value)/animation.Duration.TimeSpan.TotalMilliseconds,10);
        });
    }

    [Fact]
    public void ActiveRateChangeReplacesClockAtCurrentAngleWithoutJump()
    {
        Assert.True(Replace(true,false,true,1400,1000));
        Assert.False(Replace(true,false,true,1400,1400));
        InSta(()=>
        {
            var replacement=Animation(215.5,1000,false);
            Assert.Equal(215.5,replacement.From);Assert.Equal(575.5,replacement.To);
            Assert.Equal(TimeSpan.FromMilliseconds(1000),replacement.Duration.TimeSpan);
        });
    }

    [Fact]
    public void FinishingTurnRateChangePreservesRemainingDistanceAndVelocity()
    {
        Assert.True(Replace(true,true,false,1400,1000));
        Assert.False(Replace(true,true,false,1400,1400));
        Assert.True(Replace(true,false,false,1400,1400));
        Assert.True(Replace(true,true,true,1400,1400));
        Assert.False(Replace(false,false,false,1400,1400));
        InSta(()=>
        {
            var finish=Animation(270,1000,true);
            Assert.Equal(270d,finish.From);Assert.Equal(360d,finish.To);
            Assert.Equal(TimeSpan.FromMilliseconds(250),finish.Duration.TimeSpan);Assert.Null(finish.EasingFunction);
            Assert.Equal(360d/1000,(finish.To!.Value-finish.From!.Value)/finish.Duration.TimeSpan.TotalMilliseconds,10);
            Assert.NotEqual(RepeatBehavior.Forever,finish.RepeatBehavior);
        });
    }

    [Theory]
    [InlineData(true,false,true,true)]
    [InlineData(false,false,true,false)]
    [InlineData(true,true,true,false)]
    [InlineData(true,false,false,false)]
    public void LoadedReduceMotionAndOSPreferenceRemainHardBoundaries(bool loaded,bool reduced,bool systemAnimations,bool expected)
        => Assert.Equal(expected,(bool)Method("ShouldAnimate").Invoke(null,[loaded,reduced,systemAnimations])!);

    [Fact]
    public void InvalidDurationAndAnglesHaveFiniteBoundedPlans()
    {
        InSta(()=>
        {
            Assert.Equal(TimeSpan.FromMilliseconds(1400),Animation(0,double.NaN,false).Duration.TimeSpan);
            Assert.Equal(TimeSpan.FromMilliseconds(600),Animation(0,-100,false).Duration.TimeSpan);
            Assert.Equal(TimeSpan.FromMilliseconds(3000),Animation(0,100000,false).Duration.TimeSpan);
            Assert.Equal(0d,Animation(double.PositiveInfinity,1400,false).From);
            Assert.Equal(359d,Animation(-1,1400,false).From);
        });
    }

    private static DoubleAnimation Animation(double angle,double milliseconds,bool finishing)
        => (DoubleAnimation)Method("CreateRotationAnimation").Invoke(null,[angle,milliseconds,finishing])!;
    private static bool Replace(bool running,bool finishing,bool active,double previous,double requested)
        => (bool)Method("ShouldReplaceClock").Invoke(null,[running,finishing,active,previous,requested])!;
    private static MethodInfo Method(string name) { var method=typeof(RefreshMotion).GetMethod(name);Assert.NotNull(method);return method!; }
    private static void InSta(Action action)
    {
        Exception? error=null;var thread=new Thread(()=>{try{action();}catch(Exception ex){error=ex;}}){IsBackground=true};
        thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();
        if(error is not null)System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
    }
}
