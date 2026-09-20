[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$RunnerPath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "Invoke-AiOpsRunner.ps1"
$TaskName = "AI Ops Runner"

if (-not (Test-Path -LiteralPath $RunnerPath -PathType Leaf)) {
    throw "Runner script was not found: $RunnerPath"
}

$taskCommand = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$RunnerPath`""
& schtasks.exe /Create /SC MINUTE /MO 2 /TN $TaskName /TR $taskCommand /F | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "Failed to register the scheduled task."
}

Write-Host "Registered scheduled task: $TaskName"
Write-Host "The runner checks GitHub every 2 minutes while this Windows account is available."

