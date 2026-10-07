using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AIControlTower.Services;

/// <summary>Fakeable native boundary; operations affect only this host's icon and hidden notification HWND.</summary>
public interface ISystemTrayHost : IDisposable
{
    bool TryAdd(Action showWindow,Action requestExit,Action explorerRestarted);
    bool TryRestore();
    void Remove();
}

/// <summary>Lazy tray registration. This service never hides/disposes windows, stops jobs or shuts down remotes.</summary>
public sealed class SystemTrayService : IDisposable
{
    private readonly ISystemTrayHost _host;
    private bool _disposed,_available;
    public bool IsAvailable=>_available;
    public string LastError { get; private set; } = "";
    public event EventHandler? AvailabilityChanged;
    public SystemTrayService(ISystemTrayHost? host=null)=>_host=host??new NativeSystemTrayHost();
    public bool TryStart(Action showWindow,Action requestExit)
    {
        if(_disposed)return false;
        if(IsAvailable)return true;
        ArgumentNullException.ThrowIfNull(showWindow);ArgumentNullException.ThrowIfNull(requestExit);
        bool added;
        try { added=_host.TryAdd(()=>{if(!_disposed)showWindow();},()=>{if(!_disposed)requestExit();},()=>RestoreAfterExplorerRestart()); }
        catch { added=false; }
        LastError=added?"":"트레이 등록 실패 · 창을 유지합니다.";
        SetAvailability(added);return added;
    }
    public bool RestoreAfterExplorerRestart()
    {
        if(_disposed)return false;
        bool restored;try { restored=_host.TryRestore(); }catch { restored=false; }
        LastError=restored?"":"트레이 복구 실패 · 창에서 계속 확인합니다.";
        SetAvailability(restored);return restored;
    }
    private void SetAvailability(bool value)
    {
        if(_available==value)return;_available=value;AvailabilityChanged?.Invoke(this,EventArgs.Empty);
    }
    public void Dispose()
    {
        if(_disposed)return;_disposed=true;_available=false;AvailabilityChanged=null;
        try { _host.Remove(); }finally { _host.Dispose(); }
    }

    /// <summary>Selects bounded embedded ICO image data before any native decode. No native calls.</summary>
    public static byte[] SelectIconResource(byte[] bytes)
    {
        if(bytes is null || bytes.Length<6 || bytes.Length>1024*1024 || BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(0,2))!=0
            || BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(2,2))!=1)throw new InvalidDataException("icon header");
        var count=BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4,2));
        if(count is <1 or >32 || bytes.Length<6+16*count)throw new InvalidDataException("icon directory");
        var entries=new List<(int Score,int Offset,int Size)>();
        for(var i=0;i<count;i++)
        {
            var entry=bytes.AsSpan(6+16*i,16);var size=BinaryPrimitives.ReadUInt32LittleEndian(entry.Slice(8,4));var offset=BinaryPrimitives.ReadUInt32LittleEndian(entry.Slice(12,4));
            if(entry[3]!=0 || size==0 || offset<(uint)(6+16*count) || (ulong)offset+size>(ulong)bytes.Length)throw new InvalidDataException("icon bounds");
            var width=entry[0]==0?256:entry[0];var depth=BinaryPrimitives.ReadUInt16LittleEndian(entry.Slice(6,2));
            entries.Add((Math.Abs(width-32)*100-Math.Min((int)depth,64),(int)offset,(int)size));
        }
        var selected=entries.OrderBy(e=>e.Score).First();return bytes.AsSpan(selected.Offset,selected.Size).ToArray();
    }
}

/// <summary>Created lazily by TryAdd on the owning UI thread; constructor performs no Win32 or resource I/O.</summary>
internal sealed class NativeSystemTrayHost : ISystemTrayHost
{
    private const uint CallbackMessage=0x8000+0x231;
    private HwndSource? _source;
    private IntPtr _icon;
    private uint _taskbarCreated;
    private bool _added,_version4,_disposed;
    private Action? _show,_exit,_restart;
    public bool TryAdd(Action showWindow,Action requestExit,Action explorerRestarted)
    {
        if(_disposed || !OperatingSystem.IsWindows())return false;
        _show=showWindow;_exit=requestExit;_restart=explorerRestarted;
        try
        {
            if(_source is null)
            {
                // Hidden top-level HWND, not HWND_MESSAGE: Explorer broadcasts TaskbarCreated to top-level windows.
                _source=new HwndSource(new HwndSourceParameters("AIControlTower Tray Notifications")
                { Width=0,Height=0,WindowStyle=unchecked((int)0x80000000),ExtendedWindowStyle=0x80,ParentWindow=IntPtr.Zero });
                _source.AddHook(WndProc);_taskbarCreated=RegisterWindowMessageW("TaskbarCreated");
                if(_taskbarCreated==0)throw new InvalidOperationException("TaskbarCreated registration failed");
                var resource=Application.GetResourceStream(new Uri("pack://application:,,,/Assets/control-tower.ico",UriKind.Absolute))
                    ?? throw new InvalidDataException("embedded icon missing");
                using(resource.Stream)
                {
                    using var bytes=new MemoryStream();var buffer=new byte[4096];int length;
                    while((length=resource.Stream.Read(buffer,0,buffer.Length))>0)
                    { if(bytes.Length+length>1024*1024)throw new InvalidDataException("embedded icon size");bytes.Write(buffer,0,length); }
                    var image=SystemTrayService.SelectIconResource(bytes.ToArray());
                    _icon=CreateIconFromResourceEx(image,(uint)image.Length,true,0x00030000,32,32,0);
                    if(_icon==IntPtr.Zero)throw new InvalidDataException("embedded icon decode");
                }
            }
            if(AddIcon())return true;
        }
        catch { }
        Release();return false;
    }
    public bool TryRestore()=>!_disposed && _source is not null && _icon!=IntPtr.Zero && AddIcon();
    private bool AddIcon()
    {
        if(_added)return true;
        var data=IconData();if(!Shell_NotifyIconW(0,ref data))return false;
        _added=true;data.uTimeoutOrVersion=4;_version4=Shell_NotifyIconW(4,ref data);return true;
    }
    private NotifyIconData IconData()=>new()
    {
        cbSize=(uint)Marshal.SizeOf<NotifyIconData>(),hWnd=_source?.Handle??IntPtr.Zero,uID=1,uFlags=1|2|4|128,uCallbackMessage=CallbackMessage,hIcon=_icon,
        szTip="통합 관제탑 · 열기 / 종료",szInfo="",szInfoTitle=""
    };
    private IntPtr WndProc(IntPtr hwnd,int message,IntPtr wParam,IntPtr lParam,ref bool handled)
    {
        if((uint)message==_taskbarCreated && _taskbarCreated!=0)
        {
            _added=false;_source?.Dispatcher.BeginInvoke(()=>_restart?.Invoke());handled=true;return IntPtr.Zero;
        }
        if((uint)message!=CallbackMessage)return IntPtr.Zero;
        var notification=_version4?(int)(lParam.ToInt64()&0xffff):(int)lParam.ToInt64();handled=true;
        if(notification is 0x203 or 0x400 or 0x401) _source?.Dispatcher.BeginInvoke(()=>_show?.Invoke());
        else if(notification==0x7b || !_version4 && notification==0x205)ShowMenu();
        return IntPtr.Zero;
    }
    private void ShowMenu()
    {
        if(_source is null || !GetCursorPos(out var point))return;
        var menu=CreatePopupMenu();if(menu==IntPtr.Zero)return;
        try
        {
            if(!AppendMenuW(menu,0,1,"열기") || !AppendMenuW(menu,0,2,"종료"))return;
            SetForegroundWindow(_source.Handle);
            var command=TrackPopupMenuEx(menu,0x100|0x80,point.X,point.Y,_source.Handle,IntPtr.Zero);
            PostMessageW(_source.Handle,0,IntPtr.Zero,IntPtr.Zero);
            if(command==1)_source.Dispatcher.BeginInvoke(()=>_show?.Invoke());
            else if(command==2)_source.Dispatcher.BeginInvoke(()=>_exit?.Invoke());
        }
        finally { DestroyMenu(menu); }
    }
    public void Remove()
    {
        if(!_added)return;var data=IconData();Shell_NotifyIconW(2,ref data);_added=false;
    }
    private void Release()
    {
        Remove();if(_icon!=IntPtr.Zero){DestroyIcon(_icon);_icon=IntPtr.Zero;}
        if(_source is not null){_source.RemoveHook(WndProc);_source.Dispose();_source=null;}
    }
    public void Dispose(){if(_disposed)return;_disposed=true;Release();_show=null;_exit=null;_restart=null;}

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
    private struct NotifyIconData
    {
        public uint cbSize;public IntPtr hWnd;public uint uID,uFlags,uCallbackMessage;public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string szTip;
        public uint dwState,dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=256)]public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=64)]public string szInfoTitle;
        public uint dwInfoFlags;public Guid guidItem;public IntPtr hBalloonIcon;
    }
    [StructLayout(LayoutKind.Sequential)]private struct NativePoint { public int X,Y; }
    [DllImport("shell32.dll",CharSet=CharSet.Unicode,ExactSpelling=true,SetLastError=true)][return:MarshalAs(UnmanagedType.Bool)]private static extern bool Shell_NotifyIconW(uint message,ref NotifyIconData data);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,ExactSpelling=true,SetLastError=true)]private static extern uint RegisterWindowMessageW(string message);
    [DllImport("user32.dll",SetLastError=true)]private static extern IntPtr CreateIconFromResourceEx([In]byte[] bytes,uint size,[MarshalAs(UnmanagedType.Bool)]bool icon,uint version,int width,int height,uint flags);
    [DllImport("user32.dll")][return:MarshalAs(UnmanagedType.Bool)]private static extern bool DestroyIcon(IntPtr icon);
    [DllImport("user32.dll")]private static extern IntPtr CreatePopupMenu();
    [DllImport("user32.dll",CharSet=CharSet.Unicode,ExactSpelling=true)][return:MarshalAs(UnmanagedType.Bool)]private static extern bool AppendMenuW(IntPtr menu,uint flags,nuint id,string text);
    [DllImport("user32.dll")][return:MarshalAs(UnmanagedType.Bool)]private static extern bool DestroyMenu(IntPtr menu);
    [DllImport("user32.dll")][return:MarshalAs(UnmanagedType.Bool)]private static extern bool GetCursorPos(out NativePoint point);
    [DllImport("user32.dll")][return:MarshalAs(UnmanagedType.Bool)]private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")]private static extern uint TrackPopupMenuEx(IntPtr menu,uint flags,int x,int y,IntPtr hwnd,IntPtr parameters);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,ExactSpelling=true)][return:MarshalAs(UnmanagedType.Bool)]private static extern bool PostMessageW(IntPtr hwnd,uint message,IntPtr wParam,IntPtr lParam);
}
