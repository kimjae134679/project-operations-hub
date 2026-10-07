param(
    [string]$VersionRoot,
    [string]$ExpectedVersion,
    [string]$ExpectedSourceCommit,
    [string]$ExpectedSha256,
    [switch]$CheckOnly,
    [string]$ManifestPath
)
# Preparation/explicit user launch only. Never replaces files, saves settings or activates another instance.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$probed = $false
function Finish([int]$Code,[string]$Status,[string]$Reason) {
    [Console]::Out.WriteLine((@{status=$Status;reason=$Reason;started=($Status -eq 'started');instanceProbed=$probed} | ConvertTo-Json -Compress))
    exit $Code
}
function InBoundary([string]$Path,[string]$Root) {
    return $Path.StartsWith($Root.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase)
}
function HashFile([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $sha = [Security.Cryptography.SHA256]::Create()
    try {return ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','')} finally {$sha.Dispose();$stream.Dispose()}
}
function AssertNoReparse([string]$Path) {
    $item = Get-Item -LiteralPath $Path -Force
    while ($null -ne $item) {
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {throw 'reparse'}
        if ($item -is [IO.FileInfo]) {$item = $item.Directory} else {$item = $item.Parent}
    }
}
try {
    if ($ExpectedVersion -notmatch '^\d+\.\d+\.\d+$' -or $ExpectedSourceCommit -notmatch '^[a-fA-F0-9]{40}$' -or $ExpectedSha256 -notmatch '^[a-fA-F0-9]{64}$') {Finish 2 'held' 'invalid_request'}
    $root = [IO.Path]::GetFullPath($VersionRoot).TrimEnd('\')
    $versions = 'D:\A_KJ\AI\Applications\AIControlTower\versions'
    $checks = 'D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks'
    $fixture = -not [string]::IsNullOrEmpty($ManifestPath)
    if ($fixture) {
        if (-not $CheckOnly -or -not (InBoundary $root $checks)) {Finish 2 'held' 'fixture_requires_check_only'}
        $manifest = [IO.Path]::GetFullPath($ManifestPath)
        if ($manifest -ne (Join-Path $root 'DEPLOYMENT.json')) {Finish 2 'held' 'invalid_descriptor_path'}
    } else {
        if (-not (InBoundary $root $versions) -or [IO.Path]::GetDirectoryName($root) -ne $versions) {Finish 2 'held' 'invalid_version_root'}
        $manifest = Join-Path $root 'DEPLOYMENT.json'
    }
    AssertNoReparse $root
    $exe = Join-Path $root 'AIControlTower.exe'
    AssertNoReparse $exe
    AssertNoReparse $manifest
    if ((Get-Item -LiteralPath $manifest).Length -gt 16384) {Finish 2 'held' 'invalid_descriptor'}
    $descriptor = [IO.File]::ReadAllText($manifest) | ConvertFrom-Json
    foreach ($field in @('version','sourceCommit','fileSha256','fileVersion','productVersion','expiresAtUtc')) {
        if ($descriptor.PSObject.Properties.Name -notcontains $field -or $descriptor.$field -isnot [string] -or [string]::IsNullOrWhiteSpace($descriptor.$field)) {Finish 2 'held' 'invalid_descriptor'}
    }
    $deadline = [DateTimeOffset]::Parse($descriptor.expiresAtUtc,[Globalization.CultureInfo]::InvariantCulture)
    $now = [DateTimeOffset]::UtcNow
    if ($deadline.Offset -ne [TimeSpan]::Zero -or $deadline -le $now -or $deadline -gt $now.AddHours(24)) {Finish 2 'held' 'descriptor_expired'}
    $info = [Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
    $fileVersion = '{0}.{1}.{2}' -f $info.FileMajorPart,$info.FileMinorPart,$info.FileBuildPart
    # Bind the expected source revision to the executable's actual build metadata,
    # not merely to two matching caller/descriptor claims.
    $productParts = $info.ProductVersion.Split('+')
    if ($productParts.Length -ne 2 -or $productParts[0] -cne $ExpectedVersion -or $productParts[1] -notmatch '^[a-fA-F0-9]{40}$' -or $productParts[1] -ine $ExpectedSourceCommit) {Finish 2 'held' 'source_revision_mismatch'}
    if ($descriptor.version -cne $ExpectedVersion -or $fileVersion -cne $ExpectedVersion -or $descriptor.sourceCommit -ine $ExpectedSourceCommit -or $descriptor.fileSha256 -ine $ExpectedSha256 -or $descriptor.fileVersion -cne $info.FileVersion -or $descriptor.productVersion -cne $info.ProductVersion) {Finish 2 'held' 'descriptor_mismatch'}
    $hash = HashFile $exe
    if ($hash -ine $ExpectedSha256) {Finish 2 'held' 'executable_mismatch'}
    # CheckOnly is intentionally a descriptor check, NOT an instance-free/operating readiness claim.
    if ($CheckOnly) {Finish 0 'verified' 'descriptor_only'}
    $probed = $true
    $processes = @(Get-CimInstance -ClassName Win32_Process -Filter "Name='AIControlTower.exe'")
    foreach ($process in $processes) {
        if ([string]::IsNullOrWhiteSpace($process.ExecutablePath) -or [string]::IsNullOrWhiteSpace($process.CommandLine)) {Finish 3 'held' 'instance_unknown'}
        $processPath = [IO.Path]::GetFullPath($process.ExecutablePath)
        # Canonical executable path and current command line bind the live process, never a saved PID.
        if ($process.CommandLine -match '(?i)(?:^|\s)--manual-control(?:\s|$)') {
            if (-not (InBoundary $processPath $versions) -or [IO.Path]::GetFileName($processPath) -ine 'AIControlTower.exe') {Finish 3 'held' 'instance_unknown'}
            Finish 3 'held' 'manual_instance_running'
        }
        if ($process.CommandLine -notmatch '(?i)(?:^|\s)--(?:remote-supervisor|background|local-view)(?:\s|$)') {Finish 3 'held' 'instance_unknown'}
    }
    $mutex = $null
    $owns = $false
    try {
        try {$mutex = [Threading.Mutex]::OpenExisting('Local\AIControlTower.ManualControl')} catch [Threading.WaitHandleCannotBeOpenedException] {}
        if ($null -ne $mutex) {
            try {$owns = $mutex.WaitOne(0)} catch [Threading.AbandonedMutexException] {$owns = $true}
            if (-not $owns) {Finish 3 'held' 'manual_instance_running'}
        }
    } finally {
        if ($owns) {$mutex.ReleaseMutex()}
        if ($null -ne $mutex) {$mutex.Dispose()}
    }
    # The new child contract closes the preflight-to-launch race without signaling any activation event.
    # Recheck exact bytes/deadline before spawning. No current instance is stopped or signaled.
    if ((HashFile $exe) -ine $ExpectedSha256 -or [DateTimeOffset]::UtcNow -ge $deadline) {Finish 2 'held' 'verification_changed'}
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = $exe
    $start.Arguments = '--manual-control --no-activate-existing'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.WorkingDirectory = $root
    $start.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR'] = Join-Path $root 'bundle-extract'
    $start.EnvironmentVariables['TEMP'] = Join-Path $root 'runtime-temp'
    $start.EnvironmentVariables['TMP'] = Join-Path $root 'runtime-temp'
    $start.EnvironmentVariables['AI_CONTROL_TOWER_DATA'] = Join-Path $env:LOCALAPPDATA 'AIControlTower'
    if (-not [IO.File]::Exists((Join-Path $start.EnvironmentVariables['AI_CONTROL_TOWER_DATA'] 'settings.json'))) {Finish 2 'held' 'settings_missing'}
    $child = [Diagnostics.Process]::Start($start)
    if ($null -eq $child) {Finish 3 'held' 'start_failed'}
    $child.Dispose()
    Finish 0 'started' 'launch_requested_not_operating_confirmation'
} catch {
    # Do not expose exception text, command lines, private paths or settings.
    Finish 2 'held' 'verification_failed'
}
