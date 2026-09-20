[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$TaskName = "AI Ops Runner"

& schtasks.exe /Delete /TN $TaskName /F | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "Failed to remove the scheduled task."
}

Write-Host "Removed scheduled task: $TaskName"

