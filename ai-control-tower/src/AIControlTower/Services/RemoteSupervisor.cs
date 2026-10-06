using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIControlTower.Services;

public sealed record RemoteSupervisorRequest(bool Enabled, int SessionId, string LogonId, DateTimeOffset CreatedUtc);
public sealed record RemoteSupervisorLaunch(string NodePath, string EntryPath, string Version);
public sealed record RemoteSupervisorStatus(bool Enabled, bool Running, int RootCount, int RestartCount, int ErrorCount, string State, DateTimeOffset UpdatedUtc);

/// <summary>Separate, windowless WinExe watchdog. It never owns unrelated AI jobs.</summary>
public sealed class RemoteSupervisor
{
    public const string MutexName = @"Local\AIControlTower.RemoteSupervisor";
    public const string RemoteCommandPattern = "^\\s*(?:\\\"[^\\\"]+\\\"|\\S+)\\s+(?:\\\"(?<entry>[^\\\"]+)\\\"|(?<entry>\\S+))\\s+remote(?:\\s|$)";
    private const string GateName = @"Local\AIControlTower.RemoteSupervisor.Control";
    public static string RequestPath => Path.Combine(ControlTowerSettings.DataDirectory, "remote-supervisor-request.json");
    public static string StatusPath => Path.Combine(ControlTowerSettings.DataDirectory, "remote-supervisor-status.json");
    public static string LaunchPath => Path.Combine(ControlTowerSettings.DataDirectory, "remote-supervisor-launch.json");
    private readonly RemoteBridgeService _bridge;
    private readonly ProcessRunner _runner = new();
    private int _restartCount, _errorCount;
    public RemoteSupervisor(RemoteBridgeService bridge) => _bridge = bridge;

    public static bool IsEnabledForCurrentSession => IsRequestEnabled(ReadRequest(), Process.GetCurrentProcess().SessionId,
        CurrentLogonId(), DateTimeOffset.UtcNow.AddMilliseconds(-Environment.TickCount64));

    public static bool IsRequestEnabled(RemoteSupervisorRequest? request, int sessionId, string logonId, DateTimeOffset bootUtc) =>
        request is null || request.SessionId != sessionId || request.LogonId != logonId ||
        request.CreatedUtc < bootUtc.AddSeconds(-5) || request.Enabled;

    public static TimeSpan RetryDelay(int consecutiveErrors) =>
        TimeSpan.FromSeconds(consecutiveErrors <= 0 ? 5 : consecutiveErrors == 1 ? 10 : consecutiveErrors == 2 ? 20 : 30);

    public static void SetEnabled(bool enabled) =>
        WriteJson(RequestPath, new RemoteSupervisorRequest(enabled, Process.GetCurrentProcess().SessionId, CurrentLogonId(), DateTimeOffset.UtcNow));

    public static ProcessStartInfo SupervisorStartInfo(string executablePath)
    {
        var info = new ProcessStartInfo(executablePath)
        {
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = Path.GetDirectoryName(executablePath) ?? AppContext.BaseDirectory
        };
        info.ArgumentList.Add("--remote-supervisor");
        return info;
    }

    public static bool EnsureSupervisorStarted(string? executablePath = null)
    {
        using var mutex = new Mutex(false, MutexName);
        if (!Acquire(mutex, TimeSpan.Zero)) return true;
        mutex.ReleaseMutex();
        var path = executablePath ?? Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        using var process = Process.Start(SupervisorStartInfo(path));
        return process is not null;
    }

    // Mutex ownership stays on this dedicated thread across all asynchronous probes.
    public Task RunAsync(CancellationToken cancellationToken) => Task.Factory.StartNew(
        () => RunLoop(cancellationToken), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private void RunLoop(CancellationToken cancellationToken)
    {
        using var mutex = new Mutex(false, MutexName);
        if (!Acquire(mutex, TimeSpan.Zero)) return;
        var consecutiveErrors = 0;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var delay = TimeSpan.FromSeconds(5);
                try
                {
                    if (!IsEnabledForCurrentSession)
                    {
                        Publish(false, false, 0, "stopped-by-user");
                    }
                    else
                    {
                        var snapshot = _bridge.CheckAsync(cancellationToken).GetAwaiter().GetResult();
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!snapshot.ProbeSucceeded) throw new IOException("remote-probe-failed");
                        if (snapshot.RootCount > 0)
                        {
                            consecutiveErrors = 0;
                            Publish(true, true, snapshot.RootCount, "running");
                        }
                        else
                        {
                            using var gate = new Mutex(false, GateName);
                            var owns = Acquire(gate, TimeSpan.FromSeconds(2));
                            if (!owns) throw new IOException("remote-control-busy");
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                if (IsEnabledForCurrentSession)
                                {
                                    // Recheck under the launch gate: concurrent start requests cannot duplicate a root.
                                    snapshot = _bridge.CheckAsync(cancellationToken).GetAwaiter().GetResult();
                                    cancellationToken.ThrowIfCancellationRequested();
                                    if (!snapshot.ProbeSucceeded) throw new IOException("remote-probe-failed");
                                    if (snapshot.RootCount == 0)
                                    {
                                        StartCachedRemote();
                                        _restartCount++;
                                        Publish(true, false, 0, "starting");
                                        // Processes which immediately fail must back off as well.
                                        consecutiveErrors++;
                                        delay = RetryDelay(consecutiveErrors);
                                    }
                                }
                            }
                            finally { gate.ReleaseMutex(); }
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { break; }
                catch (Exception)
                {
                    _errorCount++;
                    consecutiveErrors++;
                    delay = RetryDelay(consecutiveErrors);
                    Publish(IsEnabledForCurrentSession, false, 0, "retrying");
                }
                try { Task.Delay(delay, cancellationToken).GetAwaiter().GetResult(); }
                catch (OperationCanceledException) { break; }
            }
        }
        finally
        {
            Publish(IsEnabledForCurrentSession, false, 0, "supervisor-ended");
            mutex.ReleaseMutex();
        }
    }

    public Task<string> StopAsync(CancellationToken cancellationToken) => Task.Factory.StartNew(() =>
    {
        // Persist immediately, then serialize with the in-flight launch before taking the stop snapshot.
        SetEnabled(false);
        using var gate = new Mutex(false, GateName);
        if (!Acquire(gate, TimeSpan.FromSeconds(12)))
            return "자동 복구는 껐지만 원격 실행 정리가 아직 진행 중입니다. 잠시 후 다시 끄세요.";
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pinned = ReadLaunch();
            var entries = BuildStopEntries(CachedLaunches(), pinned is not null && IsValidLaunch(pinned) ? pinned : null);
            if (entries.Length == 0) return "자동 복구를 껐습니다. 확인 가능한 설치 경로가 없어 실행 중인 다른 프로그램은 종료하지 않았습니다.";
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entries)));
            var script = StopScript.Replace("__ENTRIES__", payload, StringComparison.Ordinal);
            var result = _runner.RunHiddenAsync("powershell.exe", RemoteBridgeService.EncodedArguments(script),
                TimeSpan.FromSeconds(8), cancellationToken).GetAwaiter().GetResult();
            WriteJson(Path.Combine(ControlTowerSettings.DataDirectory, "remote-stop-result.json"),
                new { result.ExitCode, result.TimedOut, result.StandardOutput, result.StandardError, UpdatedUtc = DateTimeOffset.UtcNow });
            var remaining = _bridge.CheckAsync(cancellationToken).GetAwaiter().GetResult();
            return result.ExitCode == 0 && !result.TimedOut && remaining.ProbeSucceeded && remaining.RootCount == 0
                ? "원격 연결과 자동 복구를 껐습니다. 다시 켜거나 다음 로그인하면 자동 복구가 시작됩니다."
                : "자동 복구는 껐지만 원격 프로세스 종료를 확인하지 못했습니다.";
        }
        finally { gate.ReleaseMutex(); }
    }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    // A previously validated pinned install remains stoppable even if Node leaves PATH.
    public static string[] BuildStopEntries(IEnumerable<RemoteSupervisorLaunch> cached, RemoteSupervisorLaunch? validatedPinned) =>
        cached.Concat(validatedPinned is null ? Array.Empty<RemoteSupervisorLaunch>() : new[] { validatedPinned })
            .Select(item => item.EntryPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    public static bool IsManagedRemoteCommand(string processName, string? commandLine, IEnumerable<string> entries)
    {
        if (!processName.Equals("node.exe", StringComparison.OrdinalIgnoreCase) || commandLine is null) return false;
        var match = Regex.Match(commandLine, RemoteCommandPattern, RegexOptions.IgnoreCase);
        return match.Success && entries.Contains(match.Groups["entry"].Value, StringComparer.OrdinalIgnoreCase);
    }

    public static ProcessStartInfo RemoteStartInfo(RemoteSupervisorLaunch launch)
    {
        var info = new ProcessStartInfo(launch.NodePath)
        {
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true, RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(launch.EntryPath)!
        };
        info.ArgumentList.Add(launch.EntryPath);
        info.ArgumentList.Add("remote");
        return info;
    }

    private static void StartCachedRemote()
    {
        var launch = ReadLaunch();
        if (launch is null || !IsValidLaunch(launch))
        {
            launch = CachedLaunches().OrderByDescending(item => Version.TryParse(item.Version, out var version) ? version : new Version()).FirstOrDefault();
            if (launch is null) throw new FileNotFoundException("cached-desktop-commander-unavailable");
            WriteJson(LaunchPath, launch);
        }
        var process = Process.Start(RemoteStartInfo(launch)) ?? throw new IOException("remote-start-failed");
        _ = DrainAndDisposeAsync(process);
    }

    private static async Task DrainAndDisposeAsync(Process process)
    {
        try
        {
            // Discard raw output immediately: remote logs can contain credentials.
            await Task.WhenAll(process.StandardOutput.BaseStream.CopyToAsync(Stream.Null),
                process.StandardError.BaseStream.CopyToAsync(Stream.Null), process.WaitForExitAsync()).ConfigureAwait(false);
        }
        catch (Exception) { }
        finally { process.Dispose(); }
    }

    private static IEnumerable<RemoteSupervisorLaunch> CachedLaunches()
    {
        var node = EnvironmentProbe.FindCommand("node.exe");
        if (node is null) yield break;
        var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "npm-cache", "_npx");
        var roots = Directory.Exists(cache) ? Directory.EnumerateDirectories(cache).Select(path =>
            Path.Combine(path, "node_modules", "@wonderwhy-er", "desktop-commander")).ToList() : new List<string>();
        roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "node_modules", "@wonderwhy-er", "desktop-commander"));
        foreach (var root in roots)
        {
            var launch = ReadCachedPackage(root, node);
            if (launch is not null) yield return launch;
        }
    }

    private static RemoteSupervisorLaunch? ReadCachedPackage(string root, string node)
    {
        try
        {
            using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "package.json")));
            if (package.RootElement.GetProperty("name").GetString() != "@wonderwhy-er/desktop-commander") return null;
            var version = package.RootElement.GetProperty("version").GetString() ?? "";
            var entry = Path.Combine(root, "dist", "index.js");
            return File.Exists(entry) ? new(node, entry, version) : null;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException or KeyNotFoundException) { return null; }
    }

    private static bool IsValidLaunch(RemoteSupervisorLaunch launch)
    {
        if (!File.Exists(launch.NodePath) || !File.Exists(launch.EntryPath) || !Path.GetFileName(launch.NodePath).Equals("node.exe", StringComparison.OrdinalIgnoreCase)) return false;
        var root = Path.GetDirectoryName(Path.GetDirectoryName(launch.EntryPath));
        var cached = root is null ? null : ReadCachedPackage(root, launch.NodePath);
        return cached is not null && cached.Version == launch.Version &&
            cached.EntryPath.Equals(launch.EntryPath, StringComparison.OrdinalIgnoreCase);
    }

    private static RemoteSupervisorRequest? ReadRequest() => ReadJson<RemoteSupervisorRequest>(RequestPath);
    private static RemoteSupervisorLaunch? ReadLaunch() => ReadJson<RemoteSupervisorLaunch>(LaunchPath);
    private static T? ReadJson<T>(string path)
    {
        try { return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : default; }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return default; }
    }

    private static void WriteJson<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private void Publish(bool enabled, bool running, int roots, string state)
    {
        try { WriteJson(StatusPath, new RemoteSupervisorStatus(enabled, running, roots, _restartCount, _errorCount, state, DateTimeOffset.UtcNow)); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    private static bool Acquire(Mutex mutex, TimeSpan timeout)
    {
        try { return mutex.WaitOne(timeout); }
        catch (AbandonedMutexException) { return true; }
    }

    private static string CurrentLogonId()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var statistics = new TokenStatistics();
        if (!GetTokenInformation(identity.AccessToken.DangerousGetHandle(), 10, ref statistics,
            Marshal.SizeOf<TokenStatistics>(), out _)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        return statistics.AuthenticationId.HighPart.ToString("X8") + statistics.AuthenticationId.LowPart.ToString("X8");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Luid { public uint LowPart; public int HighPart; }
    [StructLayout(LayoutKind.Sequential)]
    private struct TokenStatistics
    {
        public Luid TokenId, AuthenticationId;
        public long ExpirationTime;
        public int TokenType, ImpersonationLevel;
        public uint DynamicCharged, DynamicAvailable, GroupCount, PrivilegeCount;
        public Luid ModifiedId;
    }
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTokenInformation(IntPtr token, int informationClass, ref TokenStatistics information,
        int informationLength, out int returnLength);

    private const string StopScript = """
        $ErrorActionPreference='Stop'
        $ProgressPreference='SilentlyContinue'
        $entries=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('__ENTRIES__'))|ConvertFrom-Json
        $known=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach($entry in $entries){[void]$known.Add($entry)}
        function Test-Target($p) {
            if($p.Name -ne 'node.exe'){return $false}
            if($p.CommandLine -notmatch '^\s*(?:"[^"]+"|\S+)\s+(?:"(?<entry>[^"]+)"|(?<entry>\S+))\s+remote(?:\s|$)'){return $false}
            return $known.Contains($Matches['entry'])
        }
        $all=@(Get-CimInstance Win32_Process)
        $targets=@($all|Where-Object {Test-Target $_})
        $selected=@{}
        foreach($target in $targets){$selected[[int]$target.ProcessId]=$target}
        do {
            $added=$false
            foreach($p in $all){if(!$selected.ContainsKey([int]$p.ProcessId) -and $selected.ContainsKey([int]$p.ParentProcessId)){$selected[[int]$p.ProcessId]=$p;$added=$true}}
        } while($added)
        # Verify creation time immediately before each targeted stop to exclude reused PIDs.
        foreach($p in @($selected.Values|Sort-Object CreationDate -Descending)){
            $fresh=Get-CimInstance Win32_Process -Filter ("ProcessId="+[int]$p.ProcessId)
            if($fresh -and $fresh.CreationDate -eq $p.CreationDate -and $fresh.ExecutablePath -eq $p.ExecutablePath){
                if(@($targets|Where-Object ProcessId -eq $p.ProcessId).Count -eq 0 -or (Test-Target $fresh)){Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue}
            }
        }
        $remaining=@(Get-CimInstance Win32_Process | Where-Object {Test-Target $_})
        if($remaining.Count -gt 0){throw 'remote-stop-incomplete'}
        '{"Stopped":true}'
        """;
}
