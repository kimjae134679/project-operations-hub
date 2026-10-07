param(
    [string]$CurrentExecutable,
    [string]$ExpectedCurrentSha256,
    [string]$CurrentMode = 'application',
    [string]$VersionRoot,
    [string]$ExpectedVersion = '0.9.7',
    [string]$ExpectedSourceCommit,
    [string]$ExpectedSha256,
    [string]$ExpectedGuardSha256 = 'a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730',
    [string]$StatusPath,
    [int]$WaitSeconds = 1800,
    [int]$PollSeconds = 2,
    [switch]$CheckOnly
)
# Legacy 0.9.6 has no shutdown IPC. This utility waits for its normal user exit;
# it never closes, signals or terminates the existing application.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Assert-SwitchNoReparse([string]$Path) {
    $item = Get-Item -LiteralPath $Path -Force
    while ($null -ne $item) {
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'reparse' }
        if ($item -is [IO.FileInfo]) { $item = $item.Directory } else { $item = $item.Parent }
    }
}
function Get-SwitchSha256([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','') }
    finally { $sha.Dispose(); $stream.Dispose() }
}
function Get-SwitchFileIdentity([string]$Path) {
    $info = [Diagnostics.FileVersionInfo]::GetVersionInfo($Path)
    return [pscustomobject]@{
        Version = ('{0}.{1}.{2}' -f $info.FileMajorPart,$info.FileMinorPart,$info.FileBuildPart)
        ProductVersion = $info.ProductVersion
        Hash = Get-SwitchSha256 $Path
    }
}
function Assert-SwitchRequest($Request) {
    $versions = 'D:\A_KJ\AI\Applications\AIControlTower\versions'
    $checks = 'D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks'
    $data = 'D:\A_KJ\AI\ControlTowerData\version-replacement'
    $approvedOldHash = '2fbc5b312f114cbc60a064cf5caa167251e3ac63bb226522c5a33fa18a82443d'
    $approvedGuardHash = 'a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730'
    if ($Request.CurrentMode -notin @('application','manual-control') -or $Request.ExpectedVersion -cne '0.9.7' -or $Request.ExpectedCurrentSha256 -ine $approvedOldHash -or
        $Request.ExpectedGuardSha256 -ine $approvedGuardHash -or $Request.ExpectedSourceCommit -notmatch '^[a-fA-F0-9]{40}$' -or
        $Request.ExpectedSha256 -notmatch '^[a-fA-F0-9]{64}$' -or $Request.WaitSeconds -lt 1 -or $Request.WaitSeconds -gt 1800 -or
        $Request.PollSeconds -lt 1 -or $Request.PollSeconds -gt 30) { throw 'invalid_request' }
    $old = [IO.Path]::GetFullPath($Request.CurrentExecutable)
    $root = [IO.Path]::GetFullPath($Request.VersionRoot).TrimEnd('\')
    $oldRoot = [IO.Path]::GetDirectoryName($old)
    if ([IO.Path]::GetDirectoryName($oldRoot) -ine $versions -or [IO.Path]::GetFileName($oldRoot) -notmatch '^0\.9\.6-[a-zA-Z0-9_-]+$' -or
        [IO.Path]::GetFileName($old) -ine 'AIControlTower.exe' -or [IO.Path]::GetDirectoryName($root) -ine $versions -or
        [IO.Path]::GetFileName($root) -notmatch '^0\.9\.7-[a-zA-Z0-9_-]+$' -or $oldRoot -ieq $root) { throw 'invalid_request' }
    $status = [IO.Path]::GetFullPath($Request.StatusPath)
    if ([IO.Path]::GetExtension($status) -ine '.json' -or
        !($status.StartsWith($checks+'\',[StringComparison]::OrdinalIgnoreCase) -or $status.StartsWith($data+'\',[StringComparison]::OrdinalIgnoreCase))) { throw 'invalid_status_path' }
    # Do not create directories while validating. The caller explicitly prepares its owned status directory.
    Assert-SwitchNoReparse ([IO.Path]::GetDirectoryName($status))
    if ([IO.File]::Exists($status)) { Assert-SwitchNoReparse $status }
    Assert-SwitchNoReparse $old
    Assert-SwitchNoReparse $root
    Assert-SwitchNoReparse (Join-Path $root 'start_manual_control.ps1')
    $identity = Get-SwitchFileIdentity $old
    if ($identity.Version -cne '0.9.6' -or $identity.ProductVersion -cne '0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1' -or
        $identity.Hash -ine $Request.ExpectedCurrentSha256) { throw 'old_executable_mismatch' }
    $Request.CurrentExecutable = $old
    $Request.VersionRoot = $root
    $Request.StatusPath = $status
}
function Open-SwitchLease {
    $mutex = New-Object Threading.Mutex($false, 'Local\AIControlTower.ManualControl.VersionSwitch')
    $owned = $false
    try { try { $owned = $mutex.WaitOne(0) } catch [Threading.AbandonedMutexException] { $owned = $true } }
    catch { $mutex.Dispose(); throw }
    if (!$owned) { $mutex.Dispose(); return $null }
    return [pscustomobject]@{ Mutex=$mutex; Owned=$true }
}
function Close-SwitchLease($Lease) {
    if ($null -ne $Lease) { try { $Lease.Mutex.ReleaseMutex() } finally { $Lease.Mutex.Dispose() } }
}
function Get-SwitchClock { return [DateTimeOffset]::UtcNow }
function Wait-SwitchPoll([int]$Seconds) { Start-Sleep -Seconds $Seconds }
function Get-SwitchSessionId { return [Diagnostics.Process]::GetCurrentProcess().SessionId }
function Get-SwitchProcesses { return @(Get-CimInstance -ClassName Win32_Process -Filter "Name='AIControlTower.exe'" -OperationTimeoutSec 5) }
function Get-SwitchProcessState($Rows,$Request,$Original) {
    $old = @()
    $rootSupervisor = 'D:\A_KJ\AI\Applications\AIControlTower\AIControlTower.exe'
    foreach ($row in $Rows) {
        if ([string]::IsNullOrWhiteSpace($row.ExecutablePath) -or [string]::IsNullOrWhiteSpace($row.CommandLine) -or
            $null -eq $row.CreationDate -or [int]$row.ProcessId -le 0) { return [pscustomobject]@{Allowed=$false;Reason='instance_unknown';Old=$null} }
        $path = [IO.Path]::GetFullPath($row.ExecutablePath)
        # Only the exact fixed root supervisor is an unrelated, recognized survivor.
        $prefix = '(?i)^\s*(?:"'+[regex]::Escape($path)+'"|'+[regex]::Escape($path)+')\s+'
        if ($path -ieq $rootSupervisor -and $row.CommandLine -match ($prefix+'--remote-supervisor\s*$')) { continue }
        $oldPattern = if ($Request.CurrentMode -eq 'manual-control') { $prefix+'--manual-control(?:\s+--no-activate-existing)?\s*$' }
            else { '(?i)^\s*(?:"'+[regex]::Escape($path)+'"|'+[regex]::Escape($path)+')\s*$' }
        if ($path -ine $Request.CurrentExecutable -or $row.CommandLine -notmatch $oldPattern -or
            [int]$row.SessionId -ne (Get-SwitchSessionId)) { return [pscustomobject]@{Allowed=$false;Reason='instance_unknown';Old=$null} }
        $stamp = ([DateTimeOffset]$row.CreationDate).ToUniversalTime().Ticks
        $old += [pscustomobject]@{Id=[int]$row.ProcessId;Created=$stamp;Path=$path;Session=[int]$row.SessionId}
    }
    if ($old.Count -gt 1) { return [pscustomobject]@{Allowed=$false;Reason='instance_unknown';Old=$null} }
    $current = $null
    if ($old.Count -eq 1) { $current = $old[0] }
    if ($null -ne $Original -and $null -ne $current -and ($current.Id -ne $Original.Id -or $current.Created -ne $Original.Created -or
        $current.Path -ine $Original.Path -or $current.Session -ne $Original.Session)) { return [pscustomobject]@{Allowed=$false;Reason='old_instance_changed';Old=$null} }
    return [pscustomobject]@{Allowed=$true;Reason='known';Old=$current}
}
function Invoke-SwitchGuard($Request,[bool]$CheckOnly) {
    # Fixed side-by-side guard only. No caller-selectable executable, script or fixture override.
    $guard = Join-Path $Request.VersionRoot 'start_manual_control.ps1'
    Assert-SwitchNoReparse $guard
    if ((Get-SwitchSha256 $guard) -ine $Request.ExpectedGuardSha256) { throw 'guard_changed' }
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = Join-Path ([Environment]::GetFolderPath('Windows')) 'System32\WindowsPowerShell\v1.0\powershell.exe'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.Arguments = '-NoLogo -NoProfile -NonInteractive -File "'+$guard+'" -VersionRoot "'+$Request.VersionRoot+'" -ExpectedVersion '+$Request.ExpectedVersion+' -ExpectedSourceCommit '+$Request.ExpectedSourceCommit+' -ExpectedSha256 '+$Request.ExpectedSha256
    if ($CheckOnly) { $start.Arguments += ' -CheckOnly' }
    $child = [Diagnostics.Process]::Start($start)
    if ($null -eq $child) { throw 'guard_start_failed' }
    try {
        $output = $child.StandardOutput.ReadToEndAsync()
        $errorOutput = $child.StandardError.ReadToEndAsync()
        if (!$child.WaitForExit(20000)) { throw 'guard_completion_unknown' }
        $text = $output.GetAwaiter().GetResult()
        $errors = $errorOutput.GetAwaiter().GetResult()
        if (![string]::IsNullOrWhiteSpace($errors) -or $text.Length -gt 16384) { throw 'invalid_guard_result' }
        $result = $text | ConvertFrom-Json
        if ($result.status -notin @('verified','held','started') -or $result.started -isnot [bool]) { throw 'invalid_guard_result' }
        $result | Add-Member -NotePropertyName ExitCode -NotePropertyValue $child.ExitCode
        return $result
    } finally { $child.Dispose() }
}
function New-SwitchOutcome([string]$Status,[string]$Reason,[bool]$RequiresUserExit=$false) {
    $started = if ($Status -eq 'outcome_unknown') { $null } else { $Status -eq 'started' }
    return [pscustomobject]@{schemaVersion=1;status=$Status;reason=$Reason;started=$started;operatingConfirmed=$false;requiresUserExit=$RequiresUserExit;updatedAtUtc=(Get-SwitchClock).ToString('O')}
}
function Write-SwitchStatus($Request,$Outcome) {
    Assert-SwitchNoReparse ([IO.Path]::GetDirectoryName($Request.StatusPath))
    if ([IO.File]::Exists($Request.StatusPath)) { Assert-SwitchNoReparse $Request.StatusPath }
    $temp = $Request.StatusPath+'.tmp'
    if ([IO.File]::Exists($temp)) { Assert-SwitchNoReparse $temp }
    [IO.File]::WriteAllText($temp,($Outcome | ConvertTo-Json -Compress))
    Move-Item -LiteralPath $temp -Destination $Request.StatusPath -Force
}
function Invoke-ControlVersionSwitch($Request) {
    $lease = $null
    $valid = $false
    $launchAttempted = $false
    $observedLaunch = $false
    try {
        Assert-SwitchRequest $Request
        $valid = $true
        $lease = Open-SwitchLease
        if ($null -eq $lease) { return (New-SwitchOutcome 'held' 'replacement_waiter_running') }
        $preflight = Invoke-SwitchGuard $Request $true
        if ($preflight.ExitCode -ne 0 -or $preflight.status -ne 'verified') {
            $result = New-SwitchOutcome 'held' 'target_preflight_failed'
            Write-SwitchStatus $Request $result
            return $result
        }
        if ($Request.CheckOnly) {
            $result = New-SwitchOutcome 'verified' 'descriptor_only_no_process_probe'
            Write-SwitchStatus $Request $result
            return $result
        }
        $deadline = (Get-SwitchClock).AddSeconds($Request.WaitSeconds)
        $initial = Get-SwitchProcessState @(Get-SwitchProcesses) $Request $null
        if (!$initial.Allowed) {
            $result = New-SwitchOutcome 'held' $initial.Reason
            Write-SwitchStatus $Request $result
            return $result
        }
        $original = $initial.Old
        $state = $initial
        $exitReason = if ($Request.CurrentMode -eq 'manual-control') { 'manual_exit_required' } else { 'application_exit_required' }
        if ($null -ne $original) { Write-SwitchStatus $Request (New-SwitchOutcome 'waiting' $exitReason $true) }
        while ($null -ne $state.Old) {
            if ((Get-SwitchClock) -ge $deadline) {
                $result = New-SwitchOutcome 'held' ('wait_timeout_'+$exitReason) $true
                Write-SwitchStatus $Request $result
                return $result
            }
            $remaining = [Math]::Max(1,[int][Math]::Ceiling(($deadline-(Get-SwitchClock)).TotalSeconds))
            Wait-SwitchPoll ([Math]::Min($Request.PollSeconds,$remaining))
            $state = Get-SwitchProcessState @(Get-SwitchProcesses) $Request $original
            if (!$state.Allowed) {
                $result = New-SwitchOutcome 'held' $state.Reason
                Write-SwitchStatus $Request $result
                return $result
            }
        }
        if ((Get-SwitchClock) -ge $deadline) {
            $result = New-SwitchOutcome 'held' 'wait_deadline_elapsed_no_launch'
            Write-SwitchStatus $Request $result
            return $result
        }
        Assert-SwitchRequest $Request
        # Fresh package verification and deadline evaluation are provided by the immutable guard.
        $fresh = Invoke-SwitchGuard $Request $true
        if ($fresh.ExitCode -ne 0 -or $fresh.status -ne 'verified') {
            $result = New-SwitchOutcome 'held' 'target_recheck_failed'
            Write-SwitchStatus $Request $result
            return $result
        }
        $launchAttempted = $true
        $launch = Invoke-SwitchGuard $Request $false
        $result = if ($launch.ExitCode -eq 0 -and $launch.status -eq 'started' -and $launch.started) {
            $observedLaunch = $true
            New-SwitchOutcome 'started' 'launch_requested_not_operating_confirmation'
        } else { New-SwitchOutcome 'held' 'launch_guard_held_no_retry' }
        Write-SwitchStatus $Request $result
        return $result
    } catch {
        # Never expose exception bodies, paths, process command lines or configuration.
        $result = if ($observedLaunch) { New-SwitchOutcome 'started' 'launch_requested_status_write_failed' }
            elseif ($launchAttempted) { New-SwitchOutcome 'outcome_unknown' 'launch_outcome_unknown_no_retry' }
            else { New-SwitchOutcome 'held' $(if($valid){'replacement_verification_failed_no_retry'}else{'invalid_request'}) }
        if ($valid -and $null -ne $lease) { try { Write-SwitchStatus $Request $result } catch {} }
        return $result
    } finally { if ($null -ne $lease) { Close-SwitchLease $lease } }
}

$request = [pscustomobject]@{
    CurrentExecutable=$CurrentExecutable;ExpectedCurrentSha256=$ExpectedCurrentSha256;CurrentMode=$CurrentMode
    VersionRoot=$VersionRoot;ExpectedVersion=$ExpectedVersion;ExpectedSourceCommit=$ExpectedSourceCommit
    ExpectedSha256=$ExpectedSha256;ExpectedGuardSha256=$ExpectedGuardSha256;StatusPath=$StatusPath
    WaitSeconds=$WaitSeconds;PollSeconds=$PollSeconds;CheckOnly=[bool]$CheckOnly
}
$outcome = Invoke-ControlVersionSwitch $request
[Console]::Out.WriteLine(($outcome | ConvertTo-Json -Compress))
if ($outcome.status -in @('started','verified')) { exit 0 }
exit 3
