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
        $remoteNodes=@($nodes | Where-Object {$_.CommandLine -match '(?i)\bremote\b'})
        $ids=@{}; foreach($n in $remoteNodes){$ids[[int]$n.ProcessId]=$true}
        $parents=@{}; foreach($p in $all){$parents[[int]$p.ProcessId]=[int]$p.ParentProcessId}
        $roots=0
        foreach($n in $remoteNodes){
            $parent=[int]$n.ParentProcessId; $child=$false; $seen=@{}
            while($parent -gt 0 -and $parents.ContainsKey($parent) -and !$seen.ContainsKey($parent)){
                $seen[$parent]=$true
                if($ids.ContainsKey($parent)){$child=$true;break}
                $parent=$parents[$parent]
            }
            if(!$child){$roots++}
        }
        $processes=0
        foreach($n in $nodes){
            $parent=[int]$n.ProcessId; $seen=@{}
            while($parent -gt 0 -and $parents.ContainsKey($parent) -and !$seen.ContainsKey($parent)){
                $seen[$parent]=$true
                if($ids.ContainsKey($parent)){$processes++;break}
                $parent=$parents[$parent]
            }
        }
        $startup=[Environment]::GetFolderPath('Startup')
        $task=Get-ScheduledTask -TaskName 'AIControlTower-RemoteRecovery' -ErrorAction SilentlyContinue
        $count=@(Get-ChildItem -LiteralPath $startup -File | Where-Object {$_.Name -in 'DesktopCommanderRemote.cmd','AIControlTower-DesktopCommanderSilent.vbs','AIControlTower-RemoteBridge.vbs'}).Count
        if($task -and $task.State -ne 'Disabled'){$count++}
        @{RootCount=$roots;ProcessCount=$processes;StartupCount=$count;ProbeSucceeded=$true}|ConvertTo-Json -Compress
        """;
    public async Task<RemoteBridgeSnapshot> CheckAsync(CancellationToken cancellationToken)
    {
        var result = await _runner.RunHiddenAsync("powershell.exe", EncodedArguments(ProbeScript), TimeSpan.FromSeconds(8), cancellationToken);
        try { return result.ExitCode == 0 ? JsonSerializer.Deserialize<RemoteBridgeSnapshot>(result.StandardOutput) ?? new(0, 0, 0, false) : new(0, 0, 0, false); }
        catch (JsonException) { return new(0, 0, 0, false); }
    }
    public async Task<string> EnsureRunningAsync(CancellationToken cancellationToken)
    {
        RemoteSupervisor.SetEnabled(true);
        var started = RemoteSupervisor.EnsureSupervisorStarted();
        var current = await CheckAsync(cancellationToken);
        if (!started) return "원격 자동 복구 실행 파일을 찾지 못했습니다. 설치 경로를 확인하세요.";
        return current.RootCount > 0
            ? "기존 원격 연결을 유지하고 자동 복구 감시를 켰습니다."
            : "원격 연결과 자동 복구를 시작했습니다. 다음 상태 갱신에서 연결을 확인합니다.";
    }

    public string ConsolidateStartup(string startupDirectory, string dataDirectory, string? executablePath = null)
    {
        var legacy = new[] { "DesktopCommanderRemote.cmd", "AIControlTower-DesktopCommanderSilent.vbs" };
        var backup = Path.Combine(dataDirectory, "backups", "remote-startup", DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..6]);
        Directory.CreateDirectory(backup);
        var script = Path.Combine(dataDirectory, "RemoteBridge.ps1");
        var launcher = Path.Combine(startupDirectory, LauncherName);
        if (File.Exists(script)) File.Copy(script, Path.Combine(backup, "RemoteBridge.ps1.bak"));
        if (File.Exists(launcher)) File.Copy(launcher, Path.Combine(backup, LauncherName + ".bak"));
        var executable = Path.GetFullPath(executablePath ?? Environment.ProcessPath ?? throw new InvalidOperationException("관리 앱 경로를 찾지 못했습니다."));
        if (!File.Exists(executable)) throw new FileNotFoundException("자동 복구 실행 파일이 없습니다.", executable);
        var command = '"' + executable + '"' + " --remote-supervisor";
        File.WriteAllText(launcher, "Set shell = CreateObject(\"WScript.Shell\")\r\nshell.Run \"" + command.Replace("\"", "\"\"") + "\", 0, False\r\n", Encoding.Unicode);
        foreach (var name in legacy)
        {
            var path = Path.Combine(startupDirectory, name);
            if (File.Exists(path)) { File.Copy(path, Path.Combine(backup, name + ".bak")); File.Delete(path); }
        }
        return "시작 등록을 공유 중계기 하나로 통합했습니다. 기존 실행 프로세스는 유지했습니다. 백업: " + backup;
    }
    public string RestoreStartup(string startupDirectory, string dataDirectory)
    {
        var backups = Path.Combine(dataDirectory, "backups", "remote-startup");
        var backup = Directory.Exists(backups) ? Directory.EnumerateFiles(backups, "DesktopCommanderRemote.cmd.bak", SearchOption.AllDirectories)
            .OrderByDescending(Path.GetDirectoryName, StringComparer.Ordinal).FirstOrDefault() : null;
        if (backup is null)
        {
            var oldBackups = Path.Combine(dataDirectory, "backups", "desktop-commander");
            backup = Directory.Exists(oldBackups) ? Directory.EnumerateFiles(oldBackups, "DesktopCommanderRemote.cmd.*.bak").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault() : null;
        }
        if (backup is null) throw new FileNotFoundException("복구할 기존 Remote 시작 등록 백업이 없습니다.");
        var target = Path.Combine(startupDirectory, "DesktopCommanderRemote.cmd");
        File.Copy(backup, target, true);
        foreach (var name in new[] { LauncherName, "AIControlTower-DesktopCommanderSilent.vbs" })
        {
            var path = Path.Combine(startupDirectory, name);
            if (File.Exists(path)) File.Delete(path);
        }
        return "기존 Remote 시작 등록 하나를 복구했습니다. 현재 연결은 유지하고 이번 로그인의 새 자동 복구 감시는 멈췄습니다.";
    }
    public async Task<string> ConfigureNativeStartupAsync(string executable, CancellationToken cancellationToken)
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(Path.GetFullPath(executable)));
        var script = """
            $ErrorActionPreference='Stop';$ProgressPreference='SilentlyContinue'
            $exe=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('__EXE__'))
            $user=[Security.Principal.WindowsIdentity]::GetCurrent().Name
            $action=New-ScheduledTaskAction -Execute $exe -Argument '--remote-supervisor' -WorkingDirectory ([IO.Path]::GetDirectoryName($exe))
            $trigger=New-ScheduledTaskTrigger -AtLogOn -User $user
            $settings=New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RestartCount 999 -RestartInterval (New-TimeSpan -Minutes 1) -ExecutionTimeLimit ([TimeSpan]::Zero)
            $principal=New-ScheduledTaskPrincipal -UserId $user -LogonType Interactive -RunLevel Limited
            Register-ScheduledTask -TaskName 'AIControlTower-RemoteRecovery' -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description 'Hidden remote connection recovery; user stop is managed inside AIControlTower.' -Force | Out-Null
            Start-ScheduledTask -TaskName 'AIControlTower-RemoteRecovery'
            'registered'
            """.Replace("__EXE__", encoded, StringComparison.Ordinal);
        var result = await _runner.RunHiddenAsync("powershell.exe", EncodedArguments(script), TimeSpan.FromSeconds(12), cancellationToken);
        if (result.ExitCode != 0) return "예약 작업 등록 실패: 숨김 시작 파일을 유지했습니다.";
        var launcher = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), LauncherName);
        if (File.Exists(launcher)) File.Delete(launcher);
        return "로그인 즉시 숨김 자동 시작과 감시 프로그램 복구를 등록했습니다.";
    }
    public static string EncodedArguments(string script) => "-NoProfile -ExecutionPolicy Bypass -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
}
