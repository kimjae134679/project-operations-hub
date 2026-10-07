using System.Reflection;
using AIControlTower.Services;

namespace AIControlTower.Tests;

/// <summary>Pure lifetime decisions and injected fake tray host. No Win32/HWND, windows, user controls or jobs.</summary>
public sealed class BackgroundLifetimeTests
{
    [Theory]
    [InlineData(true,false,false,false,"HideToTray")]
    [InlineData(false,false,false,false,"KeepVisible")]
    [InlineData(true,true,true,false,"KeepVisible")]
    [InlineData(true,true,false,false,"Exit")]
    [InlineData(false,false,true,true,"Exit")]
    public void CloseHideExitAndOSShutdownRemainSeparate(bool tray,bool exit,bool busy,bool sessionEnding,string expected)
        => Assert.Equal(expected,Decision(tray,exit,busy,sessionEnding));

    [Fact]
    public void TrayAddFailureCannotAuthorizeHidingWindow()
    {
        var (service,host)=CreateTray();host.AddResult=false;
        Assert.False(Start(service));Assert.False(Available(service));
        Assert.Equal("KeepVisible",Decision(Available(service),false,false,false));
        Assert.Equal(0,host.ShowCalls);Assert.Equal(0,host.ExitCalls);Dispose(service);
    }

    [Fact]
    public void SuccessfulTrayRegistrationIsIdempotentAndCallbacksAreExplicit()
    {
        var (service,host)=CreateTray();Assert.True(Start(service));Assert.True(Start(service));
        Assert.Equal(1,host.AddCalls);Assert.Equal(0,host.ShowCalls);Assert.Equal(0,host.ExitCalls);
        host.ShowCallback!();host.ExitCallback!();
        Assert.Equal(1,host.ShowCalls);Assert.Equal(1,host.ExitCalls);Dispose(service);
    }

    [Fact]
    public void ExplorerRestartRestoresOwnedIconAndFailureLosesHideAuthority()
    {
        var (service,host)=CreateTray();Start(service);host.RestoreResult=true;
        host.RestartCallback!();Assert.True(Available(service));Assert.Equal(1,host.RestoreCalls);
        host.RestoreResult=false;host.RestartCallback!();Assert.False(Available(service));
        Assert.Equal("KeepVisible",Decision(Available(service),false,false,false));
        Assert.Equal(0,host.ExitCalls);Dispose(service);
    }

    [Fact]
    public void DisposeIsIdempotentAndNeverStartsAfterDisposal()
    {
        var (service,host)=CreateTray();Start(service);Dispose(service);Dispose(service);
        Assert.Equal(1,host.RemoveCalls);Assert.Equal(1,host.DisposeCalls);
        Assert.False(Start(service));Assert.False(Available(service));Assert.Equal(1,host.AddCalls);
    }

    [Fact]
    public void EmbeddedIconSelectionRejectsMalformedAndEscapingOffsets()
    {
        var select=Method(Type("SystemTrayService"),"SelectIconResource");
        var invalid=Assert.Throws<TargetInvocationException>(()=>select.Invoke(null,[new byte[5]]));Assert.IsType<InvalidDataException>(invalid.InnerException);
        var escaped=Icon();BitConverter.GetBytes(uint.MaxValue).CopyTo(escaped,18);
        var overflow=Assert.Throws<TargetInvocationException>(()=>select.Invoke(null,[escaped]));Assert.IsType<InvalidDataException>(overflow.InnerException);
    }

    [Fact]
    public void EmbeddedIconSelectionReturnsOnlyDeclaredBoundedImageResource()
    {
        var selected=(byte[])Method(Type("SystemTrayService"),"SelectIconResource").Invoke(null,[Icon()])!;
        Assert.Equal(new byte[] { 1,2 },selected);
    }

    private static byte[] Icon()
    {
        var bytes=new byte[24];bytes[2]=1;bytes[4]=1;bytes[6]=32;bytes[7]=32;bytes[10]=1;bytes[12]=32;
        BitConverter.GetBytes(2u).CopyTo(bytes,14);BitConverter.GetBytes(22u).CopyTo(bytes,18);bytes[22]=1;bytes[23]=2;return bytes;
    }
    private static string Decision(bool tray,bool exit,bool busy,bool shutdown)
        => Method(Type("BackgroundLifetimePolicy"),"EvaluateClose").Invoke(null,[tray,exit,busy,shutdown])!.ToString()!;
    private static Type Type(string name) { var type=typeof(JobManager).Assembly.GetType("AIControlTower.Services."+name);Assert.NotNull(type);return type!; }
    private static MethodInfo Method(Type type,string name) { var method=type.GetMethod(name);Assert.NotNull(method);return method!; }
    private static (object Service,FakeTrayHost Host) CreateTray()
    {
        var hostType=Type("ISystemTrayHost");var proxy=DispatchProxy.Create(hostType,typeof(FakeTrayHost));
        var host=(FakeTrayHost)proxy;var service=Activator.CreateInstance(Type("SystemTrayService"),[proxy])!;return(service,host);
    }
    private static bool Start(object service)
    {
        var host=(FakeTrayHost)service.GetType().GetField("_host",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(service)!;
        return (bool)Method(service.GetType(),"TryStart").Invoke(service,[new Action(()=>host.ShowCalls++),new Action(()=>host.ExitCalls++)])!;
    }
    private static bool Available(object service)=>(bool)service.GetType().GetProperty("IsAvailable")!.GetValue(service)!;
    private static void Dispose(object service)=>((IDisposable)service).Dispose();
    public class FakeTrayHost : DispatchProxy
    {
        public bool AddResult=true,RestoreResult=true;
        public int AddCalls,RestoreCalls,RemoveCalls,DisposeCalls,ShowCalls,ExitCalls;
        public Action? ShowCallback,ExitCallback,RestartCallback;
        protected override object? Invoke(MethodInfo? targetMethod,object?[]? args)
        {
            switch(targetMethod!.Name)
            {
                case "TryAdd":AddCalls++;ShowCallback=(Action)args![0]!;ExitCallback=(Action)args[1]!;RestartCallback=(Action)args[2]!;return AddResult;
                case "TryRestore":RestoreCalls++;return RestoreResult;
                case "Remove":RemoveCalls++;return null;
                case "Dispose":DisposeCalls++;return null;
                default:throw new InvalidOperationException("Unexpected fake-host operation: "+targetMethod.Name);
            }
        }
    }
}
