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
    { public string Code; public RequestFailure(string code, string message) : base(message) { Code = code; } }
    static void Fail(string code, string message) { throw new RequestFailure(code, message); }
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
        DesktopAvailable();
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
        SetForegroundWindow(hwnd); Thread.Sleep(80);
        if (GetForegroundWindow() != hwnd) Fail("ui_permission_or_desktop_unavailable", "Windows denied foreground focus; select the window locally or retry after user input ends.");
        return new { hwnd = value, foreground = true };
    }
    static object Execute(string action, Dictionary<string, object> args)
    {
        DesktopAvailable();
        switch (action)
        {
            case "screen_capture": return Capture(args); case "window_list": return Windows(args);
            case "mouse_click": return Click(args); case "mouse_scroll": return Scroll(args);
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
            object result = Execute(action, args); writer.Write(json.Serialize(new { ok = true, result = result })); return 0;
        }
        catch (RequestFailure error)
        { writer.Write(json.Serialize(new { ok = false, error = new { code = error.Code, message = error.Message } })); return 1; }
        catch (Exception error)
        { writer.Write(json.Serialize(new { ok = false, error = new { code = "ui_permission_or_desktop_unavailable", message = error.GetType().Name + ": " + error.Message } })); return 1; }
    }
}
