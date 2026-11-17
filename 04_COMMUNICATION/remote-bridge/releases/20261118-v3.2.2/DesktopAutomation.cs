// Local Windows helper: one bounded JSON request on stdin, one JSON result on stdout.
// Build: csc /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll
// Microsoft contracts: https://learn.microsoft.com/windows/win32/api/winuser/ns-winuser-input
// https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-sendinput
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

internal static class DesktopAutomation
{
    const int MaxRequestChars = 65536, MaxImageBase64 = 768 * 1024;
    const uint KEYUP = 0x0002, UNICODE = 0x0004, EXTENDED = 0x0001;
    const uint MOVE = 0x0001, ABSOLUTE = 0x8000, VIRTUALDESK = 0x4000;
    const uint LEFTDOWN = 0x0002, LEFTUP = 0x0004, RIGHTDOWN = 0x0008, RIGHTUP = 0x0010;
    const uint MIDDLEDOWN = 0x0020, MIDDLEUP = 0x0040, WHEEL = 0x0800, HWHEEL = 0x1000;

    [StructLayout(LayoutKind.Sequential)] internal struct MOUSEINPUT
    { public int dx, dy; public uint mouseData, dwFlags, time; public UIntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)] internal struct KEYBDINPUT
    { public ushort wVk, wScan; public uint dwFlags, time; public UIntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Explicit)] internal struct INPUTUNION
    { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }
    [StructLayout(LayoutKind.Sequential)] internal struct INPUT
    { public uint type; public INPUTUNION data; }
    [StructLayout(LayoutKind.Sequential)] struct RECT
    { public int left, top, right, bottom; }
    delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr data);
    [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint count, INPUT[] inputs, int size);
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll", SetLastError = true)] static extern IntPtr OpenInputDesktop(uint flags, bool inherit, uint access);
    [DllImport("user32.dll", SetLastError = true)] static extern bool CloseDesktop(IntPtr desktop);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool GetUserObjectInformation(IntPtr obj, int index, StringBuilder info, uint bytes, out uint needed);
    [DllImport("user32.dll", SetLastError = true)] static extern bool EnumWindows(EnumWindowsProc callback, IntPtr data);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool IsWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool IsIconic(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hwnd, int cmd);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int maximum);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

    sealed class RequestFailure : Exception
    { public string Code; public object Details;
      public RequestFailure(string code, string message, object details = null) : base(message) { Code = code; Details = details; } }
    static void Fail(string code, string message) { throw new RequestFailure(code, message); }
    static object FocusFailureDetails(string expected, string observed)
    { return new { scope = "window_activation", policy_denied = false, expected_hwnd = expected,
        observed_hwnd = observed, automatic_retry = false, other_pc_operations_available = true,
        next_action = "Refresh window_list once. Continue independent file/API work. Use another already-authorized UI surface only when available; do not repeat click requests or bypass an explicit policy denial." }; }
    static object Value(Dictionary<string, object> args, string key, object fallback)
    { object value; return args.TryGetValue(key, out value) ? value : fallback; }
    static string Text(Dictionary<string, object> args, string key, string fallback, int max)
    {
        object value = Value(args, key, fallback);
        if (!(value is string) || ((string)value).Length > max) Fail("invalid_arguments", key + " must be a bounded string.");
        return (string)value;
    }
    static int Integer(Dictionary<string, object> args, string key, int fallback, int min, int max)
    {
        object value = Value(args, key, fallback);
        if (!(value is int) && !(value is long) && !(value is decimal) && !(value is double))
            Fail("invalid_arguments", key + " must be an integer.");
        decimal number;
        try { number = Convert.ToDecimal(value, CultureInfo.InvariantCulture); }
        catch { Fail("invalid_arguments", key + " is outside the supported range."); return 0; }
        if (number != Decimal.Truncate(number) || number < min || number > max)
            Fail("invalid_arguments", key + " is outside the supported range.");
        return (int)number;
    }
    static bool Flag(Dictionary<string, object> args, string key, bool fallback)
    { object value = Value(args, key, fallback); if (!(value is bool)) Fail("invalid_arguments", key + " must be boolean."); return (bool)value; }
    static object Bounds(Rectangle r) { return new { x = r.X, y = r.Y, width = r.Width, height = r.Height }; }
    static void DesktopAvailable()
    {
        if (!Environment.UserInteractive) Fail("ui_permission_or_desktop_unavailable", "No interactive Windows desktop is available.");
        IntPtr desktop = OpenInputDesktop(0, false, 1); // DESKTOP_READOBJECTS; never switch desktops.
        if (desktop == IntPtr.Zero) Fail("ui_permission_or_desktop_unavailable", "The input desktop is locked or unavailable.");
        try
        {
            StringBuilder name = new StringBuilder(256); uint needed;
            if (!GetUserObjectInformation(desktop, 2, name, (uint)(name.Capacity * 2), out needed) ||
                !String.Equals(name.ToString(), "Default", StringComparison.OrdinalIgnoreCase))
                Fail("ui_permission_or_desktop_unavailable", "Secure or alternate desktops are not controlled.");
        }
        finally { CloseDesktop(desktop); }
    }
    static void ModifiersReleased()
    {
        foreach (int key in new int[] { 0x10, 0x11, 0x12, 0x5B, 0x5C })
            if ((GetAsyncKeyState(key) & 0x8000) != 0)
                Fail("user_input_in_progress", "A modifier key is held. Release it before remote input.");
    }
    static INPUT Key(ushort vk, ushort scan, uint flags)
    { INPUT input = new INPUT(); input.type = 1; input.data.ki.wVk = vk; input.data.ki.wScan = scan; input.data.ki.dwFlags = flags; return input; }
    static INPUT Mouse(uint flags, int x, int y, uint wheel)
    { INPUT input = new INPUT(); input.type = 0; input.data.mi.dx = x; input.data.mi.dy = y; input.data.mi.mouseData = wheel; input.data.mi.dwFlags = flags; return input; }
    static int Inject(List<INPUT> events, List<INPUT> cleanup)
    {
        if (events.Count == 0) return 0;
        DesktopAvailable(); CheckForeground();
        int size = Marshal.SizeOf(typeof(INPUT));
        if (size != (IntPtr.Size == 8 ? 40 : 28)) Fail("native_layout_error", "Native input structure size does not match Windows.");
        uint inserted = SendInput((uint)events.Count, events.ToArray(), size);
        if (inserted != events.Count)
        {
            int nativeError = Marshal.GetLastWin32Error();
            // Only release our requested buttons/keys; never reset the user's keyboard state.
            if (inserted > 0 && cleanup.Count > 0) SendInput((uint)cleanup.Count, cleanup.ToArray(), size);
            Fail("ui_permission_or_desktop_unavailable", "Input was not fully inserted (" + inserted + "/" + events.Count + ", Windows error " + nativeError + ").");
        }
        return (int)inserted;
    }
    static object Capture(Dictionary<string, object> args)
    {
        Rectangle virtualBounds = SystemInformation.VirtualScreen;
        Rectangle area = Flag(args, "primary", false) ? Screen.PrimaryScreen.Bounds : virtualBounds;
        object crop = Value(args, "crop", null);
        if (crop != null)
        {
            Dictionary<string, object> c = crop as Dictionary<string, object>;
            if (c == null) Fail("invalid_arguments", "crop must contain x, y, width and height in real desktop pixels.");
            area = new Rectangle(Integer(c, "x", 0, -100000, 100000), Integer(c, "y", 0, -100000, 100000),
                Integer(c, "width", 0, 1, 32000), Integer(c, "height", 0, 1, 32000));
            if (!virtualBounds.Contains(area)) Fail("invalid_arguments", "crop lies outside the virtual desktop.");
        }
        if (area.Width < 1 || area.Height < 1 || (long)area.Width * area.Height > 50000000)
            Fail("capture_too_large", "Select a smaller crop or the primary display.");
        int maxWidth = Integer(args, "maxWidth", 1920, 320, 1920), maxHeight = Integer(args, "maxHeight", 2160, 240, 2160);
        int quality = Integer(args, "quality", 60, 20, 85);
        byte[] jpeg = null; int width = 0, height = 0;
        ImageCodecInfo codec = null;
        foreach (ImageCodecInfo item in ImageCodecInfo.GetImageEncoders()) if (item.MimeType == "image/jpeg") { codec = item; break; }
        if (codec == null) Fail("capture_unavailable", "JPEG encoder is unavailable.");
        using (Bitmap full = new Bitmap(area.Width, area.Height, PixelFormat.Format24bppRgb))
        {
            using (Graphics g = Graphics.FromImage(full)) g.CopyFromScreen(area.Location, Point.Empty, area.Size, CopyPixelOperation.SourceCopy);
            double scale = Math.Min(1.0, Math.Min((double)maxWidth / area.Width, (double)maxHeight / area.Height));
            for (int attempt = 0; attempt < 12; attempt++)
            {
                width = Math.Max(1, (int)Math.Round(area.Width * scale)); height = Math.Max(1, (int)Math.Round(area.Height * scale));
                using (Bitmap resized = new Bitmap(width, height, PixelFormat.Format24bppRgb))
                {
                    using (Graphics g = Graphics.FromImage(resized)) { g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.DrawImage(full, 0, 0, width, height); }
                    using (MemoryStream stream = new MemoryStream())
                    using (EncoderParameters parameters = new EncoderParameters(1))
                    {
                        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                        resized.Save(stream, codec, parameters); jpeg = stream.ToArray();
                    }
                }
                if (((jpeg.Length + 2L) / 3L) * 4L <= MaxImageBase64) break;
                if (quality > 30) quality -= 10; else scale *= 0.75;
                jpeg = null;
            }
        }
        if (jpeg == null) Fail("capture_too_large", "The bounded screenshot could not be encoded; use a smaller crop.");
        return new { mime_type = "image/jpeg", jpeg_base64 = Convert.ToBase64String(jpeg), image_width = width,
            image_height = height, virtual_bounds = Bounds(virtualBounds), capture_bounds = Bounds(area),
            scale_x = (double)width / area.Width, scale_y = (double)height / area.Height,
            coordinate_space = "virtual_desktop_pixels", jpeg_quality = quality };
    }
    static object Windows(Dictionary<string, object> args)
    {
        int maximum = Integer(args, "limit", 100, 1, 200);
        List<object> windows = new List<object>(); IntPtr foreground = GetForegroundWindow(); bool truncated = false;
        EnumWindowsProc callback = delegate(IntPtr hwnd, IntPtr data)
        {
            if (!IsWindowVisible(hwnd)) return true;
            StringBuilder title = new StringBuilder(513); GetWindowText(hwnd, title, title.Capacity);
            if (title.Length == 0) return true;
            if (windows.Count >= maximum) { truncated = true; return false; }
            uint pid; GetWindowThreadProcessId(hwnd, out pid); RECT rectangle;
            bool boundsOk = GetWindowRect(hwnd, out rectangle);
            windows.Add(new { hwnd = hwnd.ToInt64().ToString(CultureInfo.InvariantCulture), title = title.ToString(),
                pid = pid, foreground = hwnd == foreground, minimized = IsIconic(hwnd),
                bounds = boundsOk ? Bounds(Rectangle.FromLTRB(rectangle.left, rectangle.top, rectangle.right, rectangle.bottom)) : null });
            return true;
        };
        bool enumerated = EnumWindows(callback, IntPtr.Zero);
        if (!enumerated && !truncated) Fail("ui_permission_or_desktop_unavailable", "Window enumeration failed.");
        GC.KeepAlive(callback);
        return new { windows = windows, truncated = truncated, virtual_bounds = Bounds(SystemInformation.VirtualScreen) };
    }
    static int AbsolutePixel(int coordinate, int origin, int extent)
    {
        // Target the pixel center; avoid a one-pixel left/up error after 16-bit quantization.
        return Math.Max(0, Math.Min(65535, (int)Math.Floor(((double)coordinate - origin + 0.5) * 65536 / extent)));
    }
    static object Click(Dictionary<string, object> args)
    {
        Rectangle bounds = SystemInformation.VirtualScreen;
        int x = Integer(args, "x", Int32.MinValue, -100000, 100000), y = Integer(args, "y", Int32.MinValue, -100000, 100000);
        bool onDisplay = false; foreach (Screen screen in Screen.AllScreens) if (screen.Bounds.Contains(x, y)) onDisplay = true;
        if (!bounds.Contains(x, y) || !onDisplay) Fail("invalid_arguments", "Click is outside an attached display.");
        string button = Text(args, "button", "left", 12).ToLowerInvariant();
        uint down = LEFTDOWN, up = LEFTUP;
        if (button == "right") { down = RIGHTDOWN; up = RIGHTUP; }
        else if (button == "middle") { down = MIDDLEDOWN; up = MIDDLEUP; }
        else if (button != "left") Fail("invalid_arguments", "button must be left, right or middle.");
        int clicks = Integer(args, "clicks", 1, 1, 2); ModifiersReleased();
        int absoluteX = AbsolutePixel(x, bounds.Left, bounds.Width);
        int absoluteY = AbsolutePixel(y, bounds.Top, bounds.Height);
        List<INPUT> events = new List<INPUT>(); events.Add(Mouse(MOVE | ABSOLUTE | VIRTUALDESK, absoluteX, absoluteY, 0));
        for (int i = 0; i < clicks; i++) { events.Add(Mouse(down, 0, 0, 0)); events.Add(Mouse(up, 0, 0, 0)); }
        int count = Inject(events, new List<INPUT> { Mouse(up, 0, 0, 0) });
        return new { x = x, y = y, button = button, clicks = clicks, input_events = count };
    }
    static object Scroll(Dictionary<string, object> args)
    {
        int amount = Integer(args, "amount", 0, -120, 120); bool horizontal = Flag(args, "horizontal", false);
        ModifiersReleased(); int count = Inject(new List<INPUT> { Mouse(horizontal ? HWHEEL : WHEEL, 0, 0, unchecked((uint)(amount * 120))) }, new List<INPUT>());
        return new { wheel_notches = amount, horizontal = horizontal, input_events = count };
    }
    static object TypeText(Dictionary<string, object> args)
    {
        string text = Text(args, "text", "", 4000); ModifiersReleased();
        List<INPUT> events = new List<INPUT>();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\r' || c == '\n' || c == '\t')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                ushort vk = (ushort)(c == '\t' ? 0x09 : 0x0D); events.Add(Key(vk, 0, 0)); events.Add(Key(vk, 0, KEYUP));
            }
            else
            {
                if (Char.IsControl(c)) Fail("invalid_arguments", "Unsupported control character in text.");
                if (Char.IsHighSurrogate(c) && (i + 1 >= text.Length || !Char.IsLowSurrogate(text[i + 1]))) Fail("invalid_arguments", "Invalid Unicode surrogate pair.");
                if (Char.IsLowSurrogate(c) && (i == 0 || !Char.IsHighSurrogate(text[i - 1]))) Fail("invalid_arguments", "Invalid Unicode surrogate pair.");
                events.Add(Key(0, (ushort)c, UNICODE)); events.Add(Key(0, (ushort)c, UNICODE | KEYUP));
            }
        }
        List<INPUT> release = new List<INPUT>();
        foreach (INPUT item in events) if ((item.data.ki.dwFlags & KEYUP) != 0) release.Add(item);
        int count = Inject(events, release);
        return new { utf16_units = text.Length, input_events = count, delivery = "SendInput Unicode; clipboard unchanged" };
    }
    static ushort VirtualKey(string key)
    {
        if (key.Length == 1 && ((key[0] >= 'A' && key[0] <= 'Z') || (key[0] >= '0' && key[0] <= '9'))) return (ushort)key[0];
        int function; if (key.StartsWith("F", StringComparison.Ordinal) && Int32.TryParse(key.Substring(1), out function) && function >= 1 && function <= 24) return (ushort)(0x70 + function - 1);
        switch (key)
        {
            case "CTRL": case "CONTROL": return 0x11; case "SHIFT": return 0x10; case "ALT": return 0x12; case "WIN": return 0x5B;
            case "ENTER": case "RETURN": return 0x0D; case "ESC": case "ESCAPE": return 0x1B; case "TAB": return 0x09; case "SPACE": return 0x20;
            case "BACKSPACE": return 0x08; case "DELETE": case "DEL": return 0x2E; case "INSERT": case "INS": return 0x2D;
            case "HOME": return 0x24; case "END": return 0x23; case "PAGEUP": case "PGUP": return 0x21; case "PAGEDOWN": case "PGDN": return 0x22;
            case "LEFT": return 0x25; case "UP": return 0x26; case "RIGHT": return 0x27; case "DOWN": return 0x28;
        }
        Fail("invalid_arguments", "Unsupported key name: " + key); return 0;
    }
    static uint KeyFlags(ushort key) { return (key >= 0x21 && key <= 0x2E) || key == 0x5B ? EXTENDED : 0; }
    static object SendKeys(Dictionary<string, object> args)
    {
        string combo = Text(args, "keys", "", 80).ToUpperInvariant(); string[] parts = combo.Split('+');
        if (parts.Length < 1 || parts.Length > 5) Fail("invalid_arguments", "Use one key or modifier+key.");
        List<ushort> keys = new List<ushort>();
        foreach (string part in parts)
        {
            ushort key = VirtualKey(part.Trim());
            if (keys.Contains(key)) Fail("invalid_arguments", "Duplicate key in combination.");
            keys.Add(key);
        }
        for (int i = 0; i + 1 < keys.Count; i++) if (keys[i] != 0x10 && keys[i] != 0x11 && keys[i] != 0x12 && keys[i] != 0x5B)
            Fail("invalid_arguments", "Only modifiers may precede the final key.");
        if (keys.Contains(0x11) && keys.Contains(0x12) && keys.Contains(0x2E)) Fail("invalid_arguments", "Secure attention sequences are not supported.");
        foreach(ushort key in keys) if((GetAsyncKeyState(key) & 0x8000)!=0) Fail("user_input_in_progress","A requested key is already held.");
        ModifiersReleased(); List<INPUT> events = new List<INPUT>(), release = new List<INPUT>();
        foreach (ushort key in keys) events.Add(Key(key, 0, KeyFlags(key)));
        for (int i = keys.Count - 1; i >= 0; i--) release.Add(Key(keys[i], 0, KeyFlags(keys[i]) | KEYUP));
        events.AddRange(release); int count = Inject(events, release);
        return new { keys = combo, input_events = count };
    }
    static object Focus(Dictionary<string, object> args)
    {
        string value = Text(args, "hwnd", "", 24); long number;
        if (!Int64.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number) || number == 0 || (IntPtr.Size == 4 && (number < Int32.MinValue || number > UInt32.MaxValue)))
            Fail("invalid_arguments", "hwnd must be an existing window handle from window_list.");
        IntPtr hwnd = IntPtr.Size == 4 ? new IntPtr(unchecked((int)number)) : new IntPtr(number);
        if (!IsWindow(hwnd) || !IsWindowVisible(hwnd)) Fail("window_not_available", "The requested window is no longer visible.");
        if (IsIconic(hwnd)) ShowWindow(hwnd, 9); // SW_RESTORE. Do not bypass foreground restrictions.
        // One activation request; poll its postcondition without stealing focus repeatedly.
        SetForegroundWindow(hwnd);
        var activationWait = System.Diagnostics.Stopwatch.StartNew();
        while (GetForegroundWindow() != hwnd && activationWait.ElapsedMilliseconds < 400) Thread.Sleep(20);
        IntPtr observed = GetForegroundWindow();
        if (observed != hwnd) throw new RequestFailure("foreground_activation_denied",
            "Windows did not activate this window. This result is limited to foreground activation; it does not deny all PC access or establish an application permission problem.",
            FocusFailureDetails(value, observed.ToInt64().ToString(CultureInfo.InvariantCulture)));
        return new { hwnd = value, foreground = true };
    }
    static IntPtr expectedWindow = IntPtr.Zero;
    static void SetExpectedWindow(Dictionary<string, object> args)
    {
        object value = Value(args, "expectedHwnd", null);
        if (value == null) return;
        long number = 0; string text = value as string;
        if (text == null || !Int64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out number) || number <= 0 ||
            (IntPtr.Size == 4 && number > UInt32.MaxValue)) Fail("invalid_arguments", "expectedHwnd must be a window_list handle string.");
        expectedWindow = IntPtr.Size == 4 ? new IntPtr(unchecked((int)number)) : new IntPtr(number);
        CheckForeground();
    }
    static void CheckForeground()
    {
        if (expectedWindow != IntPtr.Zero && (!IsWindow(expectedWindow) || GetForegroundWindow() != expectedWindow))
            throw new RequestFailure("foreground_window_changed", "The foreground window changed before input; no input was sent. Refresh window_list once and do not repeat the same input blindly.",
                FocusFailureDetails(expectedWindow.ToInt64().ToString(CultureInfo.InvariantCulture), GetForegroundWindow().ToInt64().ToString(CultureInfo.InvariantCulture)));
    }
    static void ButtonsReleased()
    {
        foreach (int key in new int[] { 0x01, 0x02, 0x04, 0x05, 0x06 })
            if ((GetAsyncKeyState(key) & 0x8000) != 0) Fail("user_input_in_progress", "A mouse button is held. Release it before remote input.");
    }
    static Point TargetPoint(Dictionary<string, object> args, string xKey, string yKey)
    {
        int x = Integer(args, xKey, Int32.MinValue, -100000, 100000), y = Integer(args, yKey, Int32.MinValue, -100000, 100000);
        foreach (Screen screen in Screen.AllScreens) if (screen.Bounds.Contains(x, y)) return new Point(x,y);
        Fail("invalid_arguments", "Coordinates must lie on an attached display."); return Point.Empty;
    }
    static INPUT MoveEvent(Point point)
    {
        Rectangle bounds = SystemInformation.VirtualScreen;
        return Mouse(MOVE | ABSOLUTE | VIRTUALDESK, AbsolutePixel(point.X,bounds.Left,bounds.Width), AbsolutePixel(point.Y,bounds.Top,bounds.Height),0);
    }
    static object Move(Dictionary<string, object> args)
    {
        Point point=TargetPoint(args,"x","y"); ModifiersReleased(); ButtonsReleased();
        int count=Inject(new List<INPUT> { MoveEvent(point) },new List<INPUT>());
        return new { x=point.X,y=point.Y,input_events=count,coordinate_space="virtual_desktop_pixels" };
    }
    static object Drag(Dictionary<string, object> args)
    {
        Point from=TargetPoint(args,"x","y"), to=TargetPoint(args,"toX","toY");
        int duration=Integer(args,"durationMs",500,50,3000);
        string button=Text(args,"button","left",12).ToLowerInvariant();
        uint down=LEFTDOWN,up=LEFTUP;
        if(button=="right"){down=RIGHTDOWN;up=RIGHTUP;}
        else if(button=="middle"){down=MIDDLEDOWN;up=MIDDLEUP;}
        else if(button!="left") Fail("invalid_arguments","button must be left, right or middle.");
        ModifiersReleased(); ButtonsReleased();
        int count=0; bool held=false; int steps=Math.Max(2,duration/16);
        try
        {
            count+=Inject(new List<INPUT> {MoveEvent(from),Mouse(down,0,0,0)},new List<INPUT> {Mouse(up,0,0,0)}); held=true;
            for(int i=1;i<=steps;i++)
            {
                Thread.Sleep(duration/steps); ModifiersReleased();
                Point point=new Point(from.X+(int)Math.Round((double)(to.X-from.X)*i/steps),from.Y+(int)Math.Round((double)(to.Y-from.Y)*i/steps));
                count+=Inject(new List<INPUT> {MoveEvent(point)},new List<INPUT> {Mouse(up,0,0,0)});
            }
            count+=Inject(new List<INPUT> {Mouse(up,0,0,0)},new List<INPUT> {Mouse(up,0,0,0)}); held=false;
        }
        finally { if(held) SendInput(1,new INPUT[] {Mouse(up,0,0,0)},Marshal.SizeOf(typeof(INPUT))); }
        return new { x=from.X,y=from.Y,toX=to.X,toY=to.Y,button=button,durationMs=duration,input_events=count };
    }
    static object Preview(Dictionary<string, object> args)
    {
        string path=Text(args,"path","",32767);
        if(!Path.IsPathRooted(path) || !File.Exists(path)) Fail("invalid_arguments","An existing absolute image path is required.");
        string extension=Path.GetExtension(path).ToLowerInvariant();
        if(extension!=".png" && extension!=".jpg" && extension!=".jpeg" && extension!=".bmp" && extension!=".gif" && extension!=".tif" && extension!=".tiff")
            Fail("unsupported_image_format","Preview supports PNG, JPEG, BMP, GIF and TIFF; use binary reads for other formats.");
        if(new FileInfo(path).Length>128L*1024*1024) Fail("image_too_large","Image source exceeds 128 MiB.");
        int maxWidth=Integer(args,"maxWidth",1920,320,1920), maxHeight=Integer(args,"maxHeight",2160,240,2160), quality=Integer(args,"quality",70,20,85);
        using(FileStream input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read))
        using(Image full=Image.FromStream(input,true,true))
        {
            if((long)full.Width*full.Height>50000000) Fail("image_too_large","Image exceeds 50 million pixels.");
            double scale=Math.Min(1.0,Math.Min((double)maxWidth/full.Width,(double)maxHeight/full.Height));
            ImageCodecInfo codec=null;
            foreach(ImageCodecInfo item in ImageCodecInfo.GetImageEncoders()) if(item.MimeType=="image/jpeg"){codec=item;break;}
            if(codec==null) Fail("preview_unavailable","JPEG encoder is unavailable.");
            for(int attempt=0;attempt<12;attempt++)
            {
                int width=Math.Max(1,(int)Math.Round(full.Width*scale)),height=Math.Max(1,(int)Math.Round(full.Height*scale));
                byte[] jpeg;
                using(Bitmap resized=new Bitmap(width,height,PixelFormat.Format24bppRgb))
                using(Graphics g=Graphics.FromImage(resized))
                using(MemoryStream stream=new MemoryStream())
                using(EncoderParameters parameters=new EncoderParameters(1))
                {
                    g.Clear(Color.White);g.InterpolationMode=InterpolationMode.HighQualityBilinear;g.DrawImage(full,0,0,width,height);
                    parameters.Param[0]=new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,(long)quality);
                    resized.Save(stream,codec,parameters);jpeg=stream.ToArray();
                }
                if(((jpeg.Length+2L)/3L)*4L<=MaxImageBase64) return new {mime_type="image/jpeg",jpeg_base64=Convert.ToBase64String(jpeg),image_width=width,image_height=height,source_width=full.Width,source_height=full.Height,preview=true,first_frame_only=true};
                if(quality>30)quality-=10;else scale*=0.75;
            }
        }
        Fail("preview_too_large","Preview could not be bounded.");return null;
    }

    static Type AutomationType;
    static object UiaProperty(object value, string name)
    { return value.GetType().GetProperty(name).GetValue(value, null); }
    static string UiaText(object value)
    { string text = value == null ? "" : value.ToString(); return text.Length > 500 ? text.Substring(0, 500) : text; }
    static void LoadAutomation()
    {
        if (AutomationType != null) return;
        string runtime = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var assembly = System.Reflection.Assembly.LoadFrom(Path.Combine(runtime, "WPF", "UIAutomationClient.dll"));
        AutomationType = assembly.GetType("System.Windows.Automation.AutomationElement", true);
    }
    static IntPtr AutomationWindow(Dictionary<string, object> args, bool mutation)
    {
        string text = Text(args, "hwnd", "", 24); long number = 0;
        if (!Int64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out number) || number <= 0 ||
            (IntPtr.Size == 4 && number > UInt32.MaxValue)) Fail("invalid_arguments", "hwnd must be a window_list handle.");
        IntPtr hwnd = IntPtr.Size == 4 ? new IntPtr(unchecked((int)number)) : new IntPtr(number);
        if (!IsWindow(hwnd) || !IsWindowVisible(hwnd)) Fail("window_not_available", "The requested window is not visible.");
        if (mutation) { expectedWindow = hwnd; CheckForeground(); ModifiersReleased(); ButtonsReleased(); }
        return hwnd;
    }
    static string UiaId(object element)
    {
        int[] runtime = (int[])AutomationType.GetMethod("GetRuntimeId").Invoke(element, null);
        return String.Join(",", Array.ConvertAll(runtime, delegate(int value) { return value.ToString(CultureInfo.InvariantCulture); }));
    }
    static IEnumerable<object> UiaWalk(object root, int depth, int maximum)
    {
        Type walkerType = AutomationType.Assembly.GetType("System.Windows.Automation.TreeWalker", true);
        object walker = walkerType.GetField("ControlViewWalker").GetValue(null);
        var first = walkerType.GetMethod("GetFirstChild", new Type[] { AutomationType });
        var next = walkerType.GetMethod("GetNextSibling", new Type[] { AutomationType });
        Queue<Tuple<object, int>> queue = new Queue<Tuple<object, int>>();
        queue.Enqueue(Tuple.Create(root, 0)); int visited = 0;
        while (queue.Count > 0 && visited < maximum)
        {
            var item = queue.Dequeue(); visited++; yield return item.Item1;
            if (item.Item2 >= depth) continue;
            object child = first.Invoke(walker, new object[] { item.Item1 });
            while (child != null && queue.Count + visited < maximum)
            {
                queue.Enqueue(Tuple.Create(child, item.Item2 + 1));
                child = next.Invoke(walker, new object[] { child });
            }
        }
    }
    static object UiaPattern(object element, string className)
    {
        Type pattern = AutomationType.Assembly.GetType("System.Windows.Automation." + className, true);
        object identity = pattern.GetField("Pattern").GetValue(null);
        try { return AutomationType.GetMethod("GetCurrentPattern").Invoke(element, new object[] { identity }); }
        catch (System.Reflection.TargetInvocationException) { Fail("ui_pattern_not_supported","This element does not support the requested pattern; refresh ui_elements."); return null; }
    }
    static object Elements(Dictionary<string, object> args)
    {
        IntPtr hwnd = AutomationWindow(args, false); LoadAutomation();
        object root = AutomationType.GetMethod("FromHandle").Invoke(null, new object[] { hwnd });
        int limit = Integer(args, "limit", 100, 1, 200), depth = Integer(args, "depth", 5, 1, 8);
        List<object> rows = new List<object>(); int visited = 0;
        foreach (object element in UiaWalk(root, depth, limit))
        {
            visited++;
            try
            {
                object info = UiaProperty(element, "Current");
                object type = UiaProperty(info, "ControlType"), rectangle = UiaProperty(info, "BoundingRectangle");
                List<string> actions = new List<string>();
                Array patterns = (Array)AutomationType.GetMethod("GetSupportedPatterns").Invoke(element, null);
                foreach (object pattern in patterns)
                {
                    string name = UiaProperty(pattern, "ProgrammaticName").ToString();
                    if (name == "InvokePatternIdentifiers.Pattern") actions.Add("ui_invoke");
                    if (name == "ValuePatternIdentifiers.Pattern" && !(bool)UiaProperty(info, "IsPassword")) actions.Add("ui_set_value");
                }
                rows.Add(new { elementId = UiaId(element), name = (bool)UiaProperty(info, "IsPassword") ? "[password]" : UiaText(UiaProperty(info, "Name")),
                    automationId = UiaText(UiaProperty(info, "AutomationId")),
                    controlType = UiaText(UiaProperty(type, "ProgrammaticName")),
                    enabled = (bool)UiaProperty(info, "IsEnabled"), offscreen = (bool)UiaProperty(info, "IsOffscreen"),
                    password = (bool)UiaProperty(info, "IsPassword"), supportedActions = actions,
                    bounds = new { x = UiaProperty(rectangle, "X"), y = UiaProperty(rectangle, "Y"), width = UiaProperty(rectangle, "Width"), height = UiaProperty(rectangle, "Height") } });
            }
            catch (System.Reflection.TargetInvocationException) { }
        }
        uint pid;GetWindowThreadProcessId(hwnd,out pid);
        return new { hwnd = hwnd.ToInt64().ToString(CultureInfo.InvariantCulture), processId = pid, elements = rows,
            limit_reached = visited >= limit, max_depth = depth, values_omitted = true,
            coordinate_space = "virtual_desktop_pixels", refresh_before_input = true };
    }
    static object ElementAction(string action, Dictionary<string, object> args)
    {
        IntPtr hwnd = AutomationWindow(args, true); uint actualPid; GetWindowThreadProcessId(hwnd,out actualPid);
        if(actualPid != Integer(args,"expectedProcessId",0,1,Int32.MaxValue)) Fail("ui_window_changed","Window process changed; refresh ui_elements."); LoadAutomation();
        string identity = Text(args, "elementId", "", 1024), expectedName = Text(args, "expectedName", "", 500);
        if (identity.Length == 0) Fail("invalid_arguments", "elementId from ui_elements is required.");
        object root = AutomationType.GetMethod("FromHandle").Invoke(null, new object[] { hwnd });
        foreach (object element in UiaWalk(root, 8, 1000))
        {
            if (UiaId(element) != identity) continue;
            object info = UiaProperty(element, "Current");
            if (UiaText(UiaProperty(info, "Name")) != expectedName) Fail("ui_element_changed", "Element name changed; refresh ui_elements.");
            if (!(bool)UiaProperty(info, "IsEnabled") || (bool)UiaProperty(info, "IsOffscreen")) Fail("ui_element_unavailable", "Element is disabled or offscreen.");
            CheckForeground();
            if (action == "ui_invoke")
            {
                object pattern = UiaPattern(element, "InvokePattern");
                CheckForeground(); pattern.GetType().GetMethod("Invoke").Invoke(pattern, null);
            }
            else
            {
                if ((bool)UiaProperty(info, "IsPassword")) Fail("ui_password_element_refused", "Password inputs are not read or set through this tool.");
                object pattern = UiaPattern(element, "ValuePattern");
                if ((bool)UiaProperty(UiaProperty(pattern, "Current"), "IsReadOnly")) Fail("ui_element_read_only", "Input is read-only.");
                string value = Text(args, "value", "", 4000);
                CheckForeground(); pattern.GetType().GetMethod("SetValue").Invoke(pattern, new object[] { value });
            }
            return new { hwnd = hwnd.ToInt64().ToString(CultureInfo.InvariantCulture), elementId = identity, action = action, dispatched = true, verify_after_action = true };
        }
        Fail("ui_element_not_found", "Element ID is stale or outside the bounded window tree; refresh ui_elements."); return null;
    }

    static object Execute(string action, Dictionary<string, object> args)
    {
        if (action == "image_preview") return Preview(args);
        DesktopAvailable();
        if (action != "screen_capture" && action != "window_list" && action != "focus_window") SetExpectedWindow(args);
        switch (action)
        {
            case "ui_elements": return Elements(args); case "ui_invoke": case "ui_set_value": return ElementAction(action,args);
            case "screen_capture": return Capture(args); case "window_list": return Windows(args);
            case "mouse_move": return Move(args); case "mouse_drag": return Drag(args); case "mouse_click": ButtonsReleased(); return Click(args); case "mouse_scroll": return Scroll(args);
            case "type_text": return TypeText(args); case "send_keys": return SendKeys(args); case "focus_window": return Focus(args);
        }
        Fail("unknown_action", "Unsupported desktop action."); return null;
    }
    [STAThread] static int Main()
    {
        JavaScriptSerializer json = new JavaScriptSerializer(); json.MaxJsonLength = 2 * 1024 * 1024;
        // A WinExe with CREATE_NO_WINDOW and redirected pipes has no console
        // codepage handle. Never call Console.InputEncoding/OutputEncoding.
        StreamReader reader = new StreamReader(Console.OpenStandardInput(), new UTF8Encoding(false), true);
        StreamWriter writer = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)); writer.AutoFlush = true;
        try
        {
            SetProcessDPIAware();
            try { SetThreadDpiAwarenessContext(new IntPtr(-4)); } catch (EntryPointNotFoundException) { }
            StringBuilder input = new StringBuilder(); char[] buffer = new char[2048]; int count;
            while ((count = reader.Read(buffer, 0, buffer.Length)) > 0)
            { if (input.Length + count > MaxRequestChars) Fail("invalid_request", "Request is too large."); input.Append(buffer, 0, count); }
            Dictionary<string, object> request = json.DeserializeObject(input.ToString()) as Dictionary<string, object>;
            if (request == null) Fail("invalid_request", "Request must be a JSON object.");
            string action = Text(request, "action", "", 40);
            object rawArgs = Value(request, "args", null);
            Dictionary<string, object> args = rawArgs == null ? new Dictionary<string, object>() : rawArgs as Dictionary<string, object>;
            if (args == null) Fail("invalid_arguments", "args must be a JSON object.");
            object result = Execute(action, args); writer.Write(json.Serialize(new { ok = true, helperVersion = "3.2.2", result = result })); return 0;
        }
        catch (RequestFailure error)
        { writer.Write(json.Serialize(new { ok = false, error = new { code = error.Code, message = error.Message, details = error.Details } })); return 1; }
        catch (Exception error)
        { writer.Write(json.Serialize(new { ok = false, error = new { code = "ui_permission_or_desktop_unavailable", message = error.GetType().Name + ": " + error.Message } })); return 1; }
    }
}
