param(
    [string]$CurrentExecutable,
    [string]$ExpectedCurrentSha256,
    [string]$ExpectedCurrentVersion = '0.9.6',
    [string]$ExpectedCurrentSourceCommit = '84840a130d8a3fb6911b50fe528ab3709e1c76e1',
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
        FileVersion = $info.FileVersion
        Hash = Get-SwitchSha256 $Path
    }
}
function Get-SwitchCurrentDeployment($Request) {
    $path = Join-Path ([IO.Path]::GetDirectoryName($Request.CurrentExecutable)) 'DEPLOYMENT.json'
    Assert-SwitchNoReparse $path
    if ((Get-Item -LiteralPath $path).Length -gt 16384) { throw 'invalid_current_descriptor' }
    return ([IO.File]::ReadAllText($path) | ConvertFrom-Json)
}
function Test-SwitchExitCapability($Request) {
    # Positive build tuples only. Root registers verified IPC-capable package
    # bytes here after build verification; no runtime CLI capability override.
    $knownCapableBuilds = @(
        [pscustomobject]@{Version='0.9.7';SourceCommit='5b0f3296d259ce03882e16eba1d8f93604af570f';Sha256='2615d4a88f53410d963403cbb4dafabd7f6abf003c3a95737d4138b362cb244f'}
        [pscustomobject]@{Version='0.9.8';SourceCommit='c4f9669fcc287dcb47d74c97dd1f3661e621b488';Sha256='484cdc3e25a9e3b71fa183898a65bf58165c97f97f3fcabb0d74e5a95667dd2b'}
        [pscustomobject]@{Version='0.9.8';SourceCommit='8630f1bac112ded73cce20883309fc9dfaae7ee4';Sha256='4d0601c700dd12c7e217bd52f3e994b0ab51fa26bfff6c86d1ef04805daf05c9'}
        [pscustomobject]@{Version='0.9.8';SourceCommit='5e79fa1db693982a876187c455d06369b4762511';Sha256='606d81c0d7bf4b42559420b7fca255ceff3e98b66c32c0a1f3d541012652772a'}
    )
    foreach ($build in $knownCapableBuilds) {
        if ($Request.ExpectedCurrentVersion -ceq $build.Version -and $Request.ExpectedCurrentSourceCommit -ieq $build.SourceCommit -and
            $Request.ExpectedCurrentSha256 -ieq $build.Sha256) { return $true }
    }
    return $false
}
function Get-SwitchCanonicalVersion([string]$Value) {
    # Match the actual three numeric FileVersion components, not arbitrary CLI
    # text, prerelease aliases or culture-dependent numeric forms.
    if ($Value -notmatch '\A(?:0|[1-9][0-9]{0,4})\.(?:0|[1-9][0-9]{0,4})\.(?:0|[1-9][0-9]{0,4})\z') { throw 'invalid_request' }
    foreach ($part in $Value.Split('.')) {
        if ([int]$part -gt 65535) { throw 'invalid_request' }
    }
    return [Version]::Parse($Value)
}
function Assert-SwitchRequest($Request) {
    $versions = 'D:\A_KJ\AI\Applications\AIControlTower\versions'
    $checks = 'D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks'
    $data = 'D:\A_KJ\AI\ControlTowerData\version-replacement'
    $approvedOldHash = '2fbc5b312f114cbc60a064cf5caa167251e3ac63bb226522c5a33fa18a82443d'
    $approvedGuardHash = 'a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730'
    $currentVersion = Get-SwitchCanonicalVersion $Request.ExpectedCurrentVersion
    $targetVersion = Get-SwitchCanonicalVersion $Request.ExpectedVersion
    if ($Request.CurrentMode -notin @('application','manual-control') -or $targetVersion -lt [Version]'0.9.7' -or $targetVersion -lt $currentVersion -or
        ($Request.ExpectedCurrentVersion -cne '0.9.6' -and $currentVersion -lt [Version]'0.9.7') -or $Request.ExpectedCurrentSha256 -notmatch '^[a-fA-F0-9]{64}$' -or
        $Request.ExpectedCurrentSourceCommit -notmatch '^[a-fA-F0-9]{40}$' -or
        ($Request.ExpectedCurrentVersion -eq '0.9.6' -and ($Request.ExpectedCurrentSha256 -ine $approvedOldHash -or $Request.ExpectedCurrentSourceCommit -cne '84840a130d8a3fb6911b50fe528ab3709e1c76e1')) -or
        $Request.ExpectedGuardSha256 -ine $approvedGuardHash -or $Request.ExpectedSourceCommit -notmatch '^[a-fA-F0-9]{40}$' -or
        $Request.ExpectedSha256 -notmatch '^[a-fA-F0-9]{64}$' -or $Request.WaitSeconds -lt 1 -or $Request.WaitSeconds -gt 1800 -or
        $Request.PollSeconds -lt 1 -or $Request.PollSeconds -gt 30) { throw 'invalid_request' }
    $old = [IO.Path]::GetFullPath($Request.CurrentExecutable)
    $root = [IO.Path]::GetFullPath($Request.VersionRoot).TrimEnd('\')
    $oldRoot = [IO.Path]::GetDirectoryName($old)
    $oldRootPattern = '\A'+[regex]::Escape($Request.ExpectedCurrentVersion)+'-[a-zA-Z0-9_-]+\z'
    $targetRootPattern = '\A'+[regex]::Escape($Request.ExpectedVersion)+'-[a-zA-Z0-9_-]+\z'
    if ([IO.Path]::GetDirectoryName($oldRoot) -ine $versions -or [IO.Path]::GetFileName($oldRoot) -notmatch $oldRootPattern -or
        [IO.Path]::GetFileName($old) -ine 'AIControlTower.exe' -or [IO.Path]::GetDirectoryName($root) -ine $versions -or
        [IO.Path]::GetFileName($root) -notmatch $targetRootPattern -or $oldRoot -ieq $root) { throw 'invalid_request' }
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
    if ($identity.Version -cne $Request.ExpectedCurrentVersion -or $identity.ProductVersion -cne ($Request.ExpectedCurrentVersion+'+'+$Request.ExpectedCurrentSourceCommit) -or
        $identity.Hash -ine $Request.ExpectedCurrentSha256) { throw 'old_executable_mismatch' }
    $Request.CurrentExecutable = $old
    $Request.VersionRoot = $root
    $Request.StatusPath = $status
    if ($Request.ExpectedCurrentVersion -cne '0.9.6') {
        $descriptor = Get-SwitchCurrentDeployment $Request
        if ($descriptor.version -cne $Request.ExpectedCurrentVersion -or $descriptor.sourceCommit -ine $Request.ExpectedCurrentSourceCommit -or
            $descriptor.fileSha256 -ine $identity.Hash -or $descriptor.fileVersion -cne $identity.FileVersion -or $descriptor.productVersion -cne $identity.ProductVersion) { throw 'current_descriptor_mismatch' }
        # An already-running old package may have an expired launch descriptor;
        # only identity fields apply here. Target launch expiry remains enforced.
    }
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
function Get-SwitchMonotonicSeconds { return [double][Diagnostics.Stopwatch]::GetTimestamp()/[double][Diagnostics.Stopwatch]::Frequency }
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
function Get-SwitchGuardRemainingMilliseconds($Watch) {
    return [int][Math]::Max(0,20000-$Watch.ElapsedMilliseconds)
}
function Read-SwitchGuardLine($Reader,$Watch) {
    $buffer = New-Object char[] 1024
    $text = New-Object Text.StringBuilder
    while ($true) {
        $read = $Reader.ReadAsync($buffer,0,$buffer.Length)
        $remaining = Get-SwitchGuardRemainingMilliseconds $Watch
        if ($remaining -le 0 -or !$read.Wait($remaining)) { throw 'guard_completion_unknown' }
        $count = $read.GetAwaiter().GetResult()
        if ($count -eq 0) { throw 'invalid_guard_result' }
        if ($text.Length+$count -gt 16384) { throw 'invalid_guard_result' }
        $null = $text.Append($buffer,0,$count)
        $value = $text.ToString()
        $newline = $value.IndexOf("`n")
        if ($newline -ge 0) {
            return [pscustomobject]@{Line=$value.Substring(0,$newline).TrimEnd("`r");Tail=$value.Substring($newline+1);Characters=$value.Length}
        }
    }
}
function New-SwitchGuardTail($Reader,[int]$Characters=0) {
    $buffer = New-Object char[] 1024
    return [pscustomobject]@{Reader=$Reader;Buffer=$buffer;Read=$Reader.ReadAsync($buffer,0,$buffer.Length);Characters=$Characters;Ended=$false}
}
function Read-SwitchGuardTail($Capture,$Watch,[int]$ProbeMilliseconds=0) {
    while (!$Capture.Ended) {
        $remaining = Get-SwitchGuardRemainingMilliseconds $Watch
        if ($remaining -le 0) { throw 'guard_completion_unknown' }
        $probe = [Math]::Min($ProbeMilliseconds,$remaining)
        # A pending read after the guard exits may belong to its descendant's
        # inherited pipe. Available bytes are checked; EOF is never required.
        if (!$Capture.Read.Wait($probe)) { return }
        $count = $Capture.Read.GetAwaiter().GetResult()
        if ($count -eq 0) { $Capture.Ended=$true;return }
        $Capture.Characters += $count
        if ($Capture.Characters -gt 16384 -or ![string]::IsNullOrWhiteSpace([string]::new($Capture.Buffer,0,$count))) { throw 'invalid_guard_result' }
        $Capture.Read = $Capture.Reader.ReadAsync($Capture.Buffer,0,$Capture.Buffer.Length)
    }
}
function Read-SwitchChildProtocol($Start) {
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $child = [Diagnostics.Process]::Start($Start)
    if ($null -eq $child) { throw 'child_start_failed' }
    try {
        $errorCapture = New-SwitchGuardTail $child.StandardError
        $frame = Read-SwitchGuardLine $child.StandardOutput $watch
        if (![string]::IsNullOrWhiteSpace($frame.Tail)) { throw 'invalid_guard_result' }
        $outputCapture = New-SwitchGuardTail $child.StandardOutput $frame.Characters
        $result = $frame.Line | ConvertFrom-Json
        if ($result -isnot [pscustomobject]) { throw 'invalid_guard_result' }
        while (!$child.WaitForExit(0)) {
            Read-SwitchGuardTail $errorCapture $watch
            Read-SwitchGuardTail $outputCapture $watch
            $remaining = Get-SwitchGuardRemainingMilliseconds $watch
            if ($remaining -le 0) { throw 'guard_completion_unknown' }
            $null = $child.WaitForExit([Math]::Min(50,$remaining))
        }
        Read-SwitchGuardTail $errorCapture $watch 100
        Read-SwitchGuardTail $outputCapture $watch 100
        $result | Add-Member -NotePropertyName NativeExitCode -NotePropertyValue $child.ExitCode
        return $result
    } finally { $child.Dispose() }
}
function Invoke-SwitchGuard($Request,[bool]$CheckOnly) {
    # Fixed side-by-side guard only. No caller-selectable executable, script or fixture override.
    $guard = Join-Path $Request.VersionRoot 'start_manual_control.ps1'
    Assert-SwitchNoReparse $guard
    if ((Get-Item -LiteralPath $guard).Length -gt 16384) { throw 'guard_changed' }
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
    $result = Read-SwitchChildProtocol $start
    if ($result.status -notin @('verified','held','started') -or $result.started -isnot [bool]) { throw 'invalid_guard_result' }
    $result | Add-Member -NotePropertyName ExitCode -NotePropertyValue $result.NativeExitCode
    return $result
}
function Invoke-SwitchExitClient($Request,[double]$Deadline) {
    if ($Request.CurrentMode -ne 'manual-control' -or !(Test-SwitchExitCapability $Request)) { throw 'exit_capability_unverified' }
    Assert-SwitchRequest $Request
    # Keep the fixed reader budget unchanged. Never start an exit client when
    # fewer than its full 20 seconds remain in the common replacement deadline.
    if ($Deadline-(Get-SwitchMonotonicSeconds) -lt 20) {
        return [pscustomobject]@{schemaVersion=1;status='held_timeout';reason='held_timeout';exitCode=3;NativeExitCode=3}
    }
    # Single-file extraction happens before managed headless entry. Bind only
    # the already-verified OLD package; never inherit a helper's C temp paths.
    $oldRoot = [IO.Path]::GetDirectoryName($Request.CurrentExecutable)
    $bundleRoot = Join-Path $oldRoot 'bundle-extract'
    $tempRoot = Join-Path $oldRoot 'runtime-temp'
    foreach ($directory in @($bundleRoot,$tempRoot)) {
        Assert-SwitchNoReparse $directory
        if (!(Get-Item -LiteralPath $directory -Force).PSIsContainer) { throw 'invalid_runtime_directory' }
    }
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = $Request.CurrentExecutable
    $start.Arguments = '--request-manual-exit'
    $start.WorkingDirectory = $oldRoot
    $start.UseShellExecute = $false;$start.CreateNoWindow = $true
    $start.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
    $start.RedirectStandardOutput = $true;$start.RedirectStandardError = $true
    $start.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR'] = $bundleRoot
    $start.EnvironmentVariables['TEMP'] = $tempRoot
    $start.EnvironmentVariables['TMP'] = $tempRoot
    return (Read-SwitchChildProtocol $start)
}
function Assert-SwitchExitReply($Reply) {
    $codes = @{graceful_exit_accepted=0;invalid_request=2;held_jobs=3;held_timeout=3;held_unknown=3;held_request_pending=3;pending_writes=3;unsupported=4;identity_rejected=5;outcome_unknown=6}
    if ($Reply.schemaVersion -ne 1 -or ($Reply.schemaVersion -isnot [int] -and $Reply.schemaVersion -isnot [long]) -or $Reply.status -isnot [string] -or
        !($codes.Keys -ccontains $Reply.status) -or $Reply.reason -cne $Reply.status -or ($Reply.exitCode -isnot [int] -and $Reply.exitCode -isnot [long]) -or
        $Reply.exitCode -ne $codes[$Reply.status] -or $Reply.NativeExitCode -ne $Reply.exitCode) { throw 'invalid_exit_reply' }
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
    $exitAttempted = $false
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
        $deadline = (Get-SwitchMonotonicSeconds)+$Request.WaitSeconds
        $initial = Get-SwitchProcessState @(Get-SwitchProcesses) $Request $null
        if (!$initial.Allowed) {
            $result = New-SwitchOutcome 'held' $initial.Reason
            Write-SwitchStatus $Request $result
            return $result
        }
        $original = $initial.Old
        $state = $initial
        $requestExit = $Request.CurrentMode -eq 'manual-control' -and (Test-SwitchExitCapability $Request)
        $exitAccepted = $false
        $exitReason = if ($Request.CurrentMode -eq 'manual-control') { 'manual_exit_required' } else { 'application_exit_required' }
        if ($null -ne $original) { Write-SwitchStatus $Request (New-SwitchOutcome 'waiting' $exitReason $true) }
        while ($null -ne $state.Old) {
            if ((Get-SwitchMonotonicSeconds) -ge $deadline) {
                $result = New-SwitchOutcome 'held' ('wait_timeout_'+$exitReason) $true
                Write-SwitchStatus $Request $result
                return $result
            }
            if ($requestExit -and !$exitAccepted) {
                # Rebind the same live instance before every clean retry.
                $state = Get-SwitchProcessState @(Get-SwitchProcesses) $Request $original
                if (!$state.Allowed) { $result=New-SwitchOutcome 'held' $state.Reason;Write-SwitchStatus $Request $result;return $result }
                if ($null -eq $state.Old) { break }
                $exitAttempted = $true
                $reply = Invoke-SwitchExitClient $Request $deadline
                Assert-SwitchExitReply $reply
                switch ($reply.status) {
                    'graceful_exit_accepted' {$exitAccepted=$true}
                    'unsupported' {$requestExit=$false;$exitAttempted=$false}
                    'outcome_unknown' {$result=New-SwitchOutcome 'outcome_unknown' 'manual_exit_outcome_unknown_no_retry';Write-SwitchStatus $Request $result;return $result}
                    {$_ -in @('held_jobs','held_timeout','pending_writes')} {$exitAttempted=$false}
                    default {$result=New-SwitchOutcome 'held' ('manual_exit_'+$reply.status+'_no_retry');Write-SwitchStatus $Request $result;return $result}
                }
            }
            if ((Get-SwitchMonotonicSeconds) -ge $deadline) {
                $result = New-SwitchOutcome 'held' ('wait_timeout_'+$exitReason) $true
                Write-SwitchStatus $Request $result
                return $result
            }
            $remaining = [Math]::Max(1,[int][Math]::Ceiling($deadline-(Get-SwitchMonotonicSeconds)))
            Wait-SwitchPoll ([Math]::Min($Request.PollSeconds,$remaining))
            $state = Get-SwitchProcessState @(Get-SwitchProcesses) $Request $original
            if (!$state.Allowed) {
                $result = New-SwitchOutcome 'held' $state.Reason
                Write-SwitchStatus $Request $result
                return $result
            }
        }
        $exitAttempted=$false
        if ((Get-SwitchMonotonicSeconds) -ge $deadline) {
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
            elseif ($launchAttempted -or $exitAttempted) { New-SwitchOutcome 'outcome_unknown' 'request_outcome_unknown_no_retry' }
            else { New-SwitchOutcome 'held' $(if($valid){'replacement_verification_failed_no_retry'}else{'invalid_request'}) }
        if ($valid -and $null -ne $lease) { try { Write-SwitchStatus $Request $result } catch {} }
        return $result
    } finally { if ($null -ne $lease) { Close-SwitchLease $lease } }
}

$request = [pscustomobject]@{
    CurrentExecutable=$CurrentExecutable;ExpectedCurrentSha256=$ExpectedCurrentSha256;CurrentMode=$CurrentMode
    ExpectedCurrentVersion=$ExpectedCurrentVersion;ExpectedCurrentSourceCommit=$ExpectedCurrentSourceCommit
    VersionRoot=$VersionRoot;ExpectedVersion=$ExpectedVersion;ExpectedSourceCommit=$ExpectedSourceCommit
    ExpectedSha256=$ExpectedSha256;ExpectedGuardSha256=$ExpectedGuardSha256;StatusPath=$StatusPath
    WaitSeconds=$WaitSeconds;PollSeconds=$PollSeconds;CheckOnly=[bool]$CheckOnly
}
$outcome = Invoke-ControlVersionSwitch $request
[Console]::Out.WriteLine(($outcome | ConvertTo-Json -Compress))
if ($outcome.status -in @('started','verified')) { exit 0 }
exit 3
