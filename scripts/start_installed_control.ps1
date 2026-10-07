[CmdletBinding()]
param([switch]$CheckOnly)
# Explicit installed-user launch only. No arbitrary package, identity or descriptor overrides.
# This approved installed identity has no launch deadline; development/handoff leases are unchanged.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$probed = $false
$versions = 'D:\A_KJ\AI\Applications\AIControlTower\versions'
$package = '0.9.10-20261008-relay-repair'
$expectedVersion = '0.9.10'
$expectedSource = 'dc5e99ce848037a2c98398901693c38b29069330'
$expectedHash = '464e4fb128f2dd9b93175c49b8bcf1dad24000fe35b3acf54a428a8c69e9d63f'
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
    $root = [IO.Path]::GetFullPath((Join-Path $versions $package)).TrimEnd('\')
    if (-not (InBoundary $root $versions) -or [IO.Path]::GetDirectoryName($root) -ine $versions -or [IO.Path]::GetFileName($root) -cne $package) {Finish 2 'held' 'invalid_version_root'}
    $exe = Join-Path $root 'AIControlTower.exe'
    $manifest = Join-Path $root 'DEPLOYMENT.json'
    AssertNoReparse $root
    AssertNoReparse $exe
    AssertNoReparse $manifest
    if ((Get-Item -LiteralPath $manifest).Length -gt 16384) {Finish 2 'held' 'invalid_descriptor'}
    $manifestHash = HashFile $manifest
    $descriptor = [IO.File]::ReadAllText($manifest) | ConvertFrom-Json
    foreach ($field in @('version','sourceCommit','fileSha256','fileVersion','productVersion','expiresAtUtc')) {
        if ($descriptor.PSObject.Properties.Name -notcontains $field -or $descriptor.$field -isnot [string] -or [string]::IsNullOrWhiteSpace($descriptor.$field)) {Finish 2 'held' 'invalid_descriptor'}
    }
    if ($descriptor.PSObject.Properties.Name -notcontains 'activated' -or $descriptor.activated -isnot [bool] -or -not $descriptor.activated) {Finish 2 'held' 'package_not_activated'}
    # Retain the original UTC metadata contract, but never extend/rewrite or compare its deadline here.
    $originalDeadline = [DateTimeOffset]::Parse($descriptor.expiresAtUtc,[Globalization.CultureInfo]::InvariantCulture)
    if ($originalDeadline.Offset -ne [TimeSpan]::Zero) {Finish 2 'held' 'invalid_descriptor'}
    $info = [Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
    $fileVersion = '{0}.{1}.{2}' -f $info.FileMajorPart,$info.FileMinorPart,$info.FileBuildPart
    $productParts = $info.ProductVersion.Split('+')
    if ($productParts.Length -ne 2 -or $productParts[0] -cne $expectedVersion -or $productParts[1] -ine $expectedSource) {Finish 2 'held' 'source_revision_mismatch'}
    if ($descriptor.version -cne $expectedVersion -or $fileVersion -cne $expectedVersion -or $descriptor.sourceCommit -ine $expectedSource -or $descriptor.fileSha256 -ine $expectedHash -or $descriptor.fileVersion -cne $info.FileVersion -or $descriptor.productVersion -cne $info.ProductVersion) {Finish 2 'held' 'descriptor_mismatch'}
    if ((HashFile $exe) -ine $expectedHash -or (HashFile $manifest) -ine $manifestHash) {Finish 2 'held' 'executable_mismatch'}
    # CheckOnly establishes identity only, not absence of instances or operating readiness.
    if ($CheckOnly) {Finish 0 'verified' 'installed_identity_only'}
    $probed = $true
    $processes = @(Get-CimInstance -ClassName Win32_Process -Filter "Name='AIControlTower.exe'")
    foreach ($process in $processes) {
        if ([string]::IsNullOrWhiteSpace($process.ExecutablePath) -or [string]::IsNullOrWhiteSpace($process.CommandLine)) {Finish 3 'held' 'instance_unknown'}
        $processPath = [IO.Path]::GetFullPath($process.ExecutablePath)
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
    # Recheck links, approved bytes and unchanged activation descriptor immediately before spawning.
    AssertNoReparse $root
    AssertNoReparse $exe
    AssertNoReparse $manifest
    if ((HashFile $exe) -ine $expectedHash -or (HashFile $manifest) -ine $manifestHash) {Finish 2 'held' 'verification_changed'}
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
    # Never disclose exception text, command lines, tokens or settings.
    Finish 2 'held' 'verification_failed'
}
