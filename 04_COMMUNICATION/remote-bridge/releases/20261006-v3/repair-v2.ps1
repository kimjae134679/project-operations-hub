param(
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{40}$')][string]$Commit,
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{64}$')][string]$WorkerSha256
)
$ErrorActionPreference='Stop'
$homePath='D:\A_KJ\AI\Applications\ProjectBridge'
$statePath=Join-Path $homePath 'state'
$workerPath=Join-Path $homePath 'bridge_worker.py'
$exePath=Join-Path $homePath '프로젝트연결.exe'
$expectedOld='c2c87fdeb1a5ef0c13baa20ed176247e47e826928ee556be15bc8ef31ab5cbec'
if(!(Test-Path -LiteralPath $workerPath -PathType Leaf)){throw 'ProjectBridge 2.0 worker missing'}
$current=(Get-FileHash -LiteralPath $workerPath -Algorithm SHA256).Hash.ToLowerInvariant()
if($current -ne $expectedOld -and $current -ne $WorkerSha256){throw 'Local worker changed; refusing overwrite'}
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$tempPath=Join-Path ([IO.Path]::GetTempPath()) ('ProjectBridgeRepair_'+[guid]::NewGuid().ToString('N')+'.py')
try {
 Invoke-WebRequest -UseBasicParsing -Uri ('https://raw.githubusercontent.com/kimjae134679/project-operations-hub/'+$Commit+'/04_COMMUNICATION/remote-bridge/releases/20261006-v3/hotfix-v2-worker.py') -OutFile $tempPath
 if((Get-FileHash -LiteralPath $tempPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $WorkerSha256){throw 'Worker download hash mismatch'}
 # Record whether the user had deliberately stopped this login. Never erase it.
 $userStopped=Test-Path -LiteralPath (Join-Path $statePath 'stopped_logon.txt')
 [IO.File]::WriteAllText((Join-Path $statePath 'stop.flag'),'installer pause')
 $ticketPath=Join-Path $statePath 'launcher_child.json'
 $child=$null
 if(Test-Path -LiteralPath $ticketPath){
  $ticket=Get-Content -LiteralPath $ticketPath -Raw -Encoding UTF8|ConvertFrom-Json
  $candidate=Get-Process -Id ([int]$ticket.pid) -ErrorAction SilentlyContinue
  $cfg=Get-Content -LiteralPath (Join-Path $homePath 'config.json') -Raw -Encoding UTF8|ConvertFrom-Json
  $pythonPaths=@([string]$cfg.python,(Join-Path (Split-Path -Parent ([string]$cfg.python)) 'pythonw.exe'))
  $processInfo=Get-CimInstance Win32_Process -Filter ('ProcessId='+[int]$ticket.pid) -ErrorAction SilentlyContinue
  if($candidate -and $candidate.StartTime.ToUniversalTime().ToString('o') -eq [string]$ticket.startedAt -and $candidate.Path -in $pythonPaths -and $processInfo.CommandLine.Contains($workerPath)){$child=$candidate}
 }
 # Graceful worker exit has priority. This exact child contains the bridge,
 # never the independent audiobook production processes.
 if($child){
  if(!$child.WaitForExit(15000)){
   $live=Get-Process -Id $child.Id -ErrorAction SilentlyContinue
   if($live -and $live.StartTime.ToUniversalTime().ToString('o') -eq [string]$ticket.startedAt){Stop-Process -Id $live.Id -Force}
  }
 }
 # Patch a single checked worker; config, device ID, journals and audio queues stay.
 $backup=Join-Path $statePath ('install-backups\repair_'+[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss_ffff'))
 New-Item -ItemType Directory -Path $backup -Force|Out-Null
 Copy-Item -LiteralPath $workerPath -Destination (Join-Path $backup 'bridge_worker.py')
 Copy-Item -LiteralPath $tempPath -Destination $workerPath -Force
 if(!$userStopped){
  Remove-Item -LiteralPath (Join-Path $statePath 'stop.flag') -Force -ErrorAction SilentlyContinue
  # Existing native launcher recovers the exact owned child; opening again is
  # mutex protected and cannot start a duplicate production queue.
  Start-Process -FilePath $exePath -ArgumentList '--background' -WorkingDirectory $homePath -WindowStyle Hidden
 }
 Write-Output 'ProjectBridge metadata URL repaired. Installation is not proof of a working remote connection.'
 Write-Output 'No desktop files, credentials, production queue, or audio processes were changed.'
} finally {if(Test-Path -LiteralPath $tempPath){Remove-Item -LiteralPath $tempPath -Force}}
