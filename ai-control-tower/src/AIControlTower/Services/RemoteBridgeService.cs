using System.Text;
using System.Text.Json;

namespace AIControlTower.Services;

public sealed record RemoteBridgeSnapshot(int RootCount, int ProcessCount, int StartupCount, bool ProbeSucceeded);

public sealed class RemoteBridgeService
{
    public const string LauncherName = "AIControlTower-RemoteBridge.vbs";
    public static string ScriptPath => Path.Combine(ControlTowerSettings.DataDirectory, "RemoteBridge.ps1");
    private readonly ProcessRunner _runner;
    public RemoteBridgeService(ProcessRunner runner) => _runner = runner;
    // Only counts/identifiers leave this probe. Never return full command lines.
    private const string ProbeScript = """
        $ErrorActionPreference='Stop'
        $all=@(Get-CimInstance Win32_Process)
        $nodes=@($all | Where-Object { $_.Name -eq 'node.exe' -and $_.CommandLine -match '(?i)desktop-commander' -and $_.CommandLine -match '(?i)(\bremote\b|dist[\\/]index\.js)' })
        $ids=@{}; foreach($n in $nodes){$ids[[int]$n.ProcessId]=$true}
        $parents=@{}; foreach($p in $all){$parents[[int]$p.ProcessId]=[int]$p.ParentProcessId}
        $roots=0
        foreach($n in $nodes){
            $parent=[int]$n.ParentProcessId; $child=$false; $seen=@{}
            while($parent -gt 0 -and $parents.ContainsKey($parent) -and !$seen.ContainsKey($parent)){
                $seen[$parent]=$true
                if($ids.ContainsKey($parent)){$child=$true;break}
                $parent=$parents[$parent]
            }
            if(!$child){$roots++}
        }
        $startup=[Environment]::GetFolderPath('Startup')
        $count=@(Get-ChildItem -LiteralPath $startup -File | Where-Object {$_.Name -in 'DesktopCommanderRemote.cmd','AIControlTower-DesktopCommanderSilent.vbs','AIControlTower-RemoteBridge.vbs'}).Count
        @{RootCount=$roots;ProcessCount=$nodes.Count;StartupCount=$count;ProbeSucceeded=$true}|ConvertTo-Json -Compress
        """;
    public async Task<RemoteBridgeSnapshot> CheckAsync(CancellationToken cancellationToken)
    {
        var result = await _runner.RunHiddenAsync("powershell.exe", EncodedArguments(ProbeScript), TimeSpan.FromSeconds(8), cancellationToken);
        try { return result.ExitCode == 0 ? JsonSerializer.Deserialize<RemoteBridgeSnapshot>(result.StandardOutput) ?? new(0, 0, 0, false) : new(0, 0, 0, false); }
        catch (JsonException) { return new(0, 0, 0, false); }
    }
    public async Task<string> EnsureRunningAsync(CancellationToken cancellationToken)
    {
        var current = await CheckAsync(cancellationToken);
        if (!current.ProbeSucceeded) return "리모트 상태를 확인하지 못해 추가 실행하지 않았습니다.";
        if (current.RootCount > 0) return $"기존 리모트 연결 {current.RootCount}개를 재사용합니다. 부모·자식 {current.ProcessCount}개는 별도 연결로 세지 않습니다.";
        if (!File.Exists(ScriptPath)) return "먼저 리모트 시작 경로 통합을 실행하세요.";
        var info = new ProcessStartInfo("powershell.exe") { UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", ScriptPath }) info.ArgumentList.Add(argument);
        using var process = Process.Start(info);
        return "공유 리모트 중계기에 연결 시작을 요청했습니다. 다음 갱신에서 실제 프로세스 상태를 확인합니다.";
    }
    public string ConsolidateStartup(string startupDirectory, string dataDirectory)
    {
        var legacy = new[] { "DesktopCommanderRemote.cmd", "AIControlTower-DesktopCommanderSilent.vbs" };
        var backup = Path.Combine(dataDirectory, "backups", "remote-startup", DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..6]);
        Directory.CreateDirectory(backup);
        var script = Path.Combine(dataDirectory, "RemoteBridge.ps1");
        var launcher = Path.Combine(startupDirectory, LauncherName);
        if (File.Exists(script)) File.Copy(script, Path.Combine(backup, "RemoteBridge.ps1.bak"));
        if (File.Exists(launcher)) File.Copy(launcher, Path.Combine(backup, LauncherName + ".bak"));
        File.WriteAllText(script, SupervisorScript, Encoding.UTF8);
        var command = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File " + '"' + script + '"';
        File.WriteAllText(launcher, "Set shell = CreateObject(\"WScript.Shell\")\r\nshell.Run \"" + command.Replace("\"", "\"\"") + "\", 0, False\r\n", Encoding.ASCII);
        foreach (var name in legacy)
        {
            var path = Path.Combine(startupDirectory, name);
            if (File.Exists(path)) { File.Copy(path, Path.Combine(backup, name + ".bak")); File.Delete(path); }
        }
        return "시작 등록을 공유 중계기 하나로 통합했습니다. 기존 실행 프로세스는 유지했습니다. 백업: " + backup;
    }
    public static string EncodedArguments(string script) => "-NoProfile -ExecutionPolicy Bypass -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
    public const string SupervisorScript = """
        param([switch]$CheckOnly)
        $ErrorActionPreference = 'Stop'
        $mutex = [System.Threading.Mutex]::new($false, 'Local\AIControlTower.RemoteBridge')
        $owns = $false
        try {
            try { $owns = $mutex.WaitOne(0) } catch [System.Threading.AbandonedMutexException] { $owns = $true }
            if (!$owns) { Write-Output 'shared-supervisor-already-running'; exit 0 }
            function Test-Remote {
                $found = @(Get-CimInstance Win32_Process -ErrorAction Stop | Where-Object {
                    $_.Name -eq 'node.exe' -and $_.CommandLine -match '(?i)desktop-commander' -and $_.CommandLine -match '(?i)\bremote\b'
                })
                return $found.Count -gt 0
            }
            if (Test-Remote) { Write-Output 'existing-remote-reused'; exit 0 }
            if ($CheckOnly) { Write-Output 'no-remote'; exit 0 }
            $npx = Get-Command npx.cmd -ErrorAction Stop
            while ($true) {
                if (Test-Remote) { Write-Output 'existing-remote-reused'; exit 0 }
                & $npx.Source --no-install '@wonderwhy-er/desktop-commander@latest' remote
                if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
                Start-Sleep -Seconds 5
            }
        } finally {
            if ($owns) { $mutex.ReleaseMutex() }
            $mutex.Dispose()
        }
        """;
}
