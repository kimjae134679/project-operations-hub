param(
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{40}$')][string]$Commit,
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{64}$')][string]$ManifestSha256,
 [string[]]$ApprovedBundleSha256=@(),
 [switch]$StartAtLogin
)
$ErrorActionPreference='Stop'
$target='D:\A_KJ\AI\Applications\ProjectBridge'
$project='D:\AI\VoiceAudiobook'
$audioEnabled=Test-Path -LiteralPath $project -PathType Container
$python='D:\AI\envs\cosyvoice\python.exe'
if(!(Test-Path -LiteralPath $python)){ $python=(Get-Command python.exe -ErrorAction Stop).Source }
foreach($hash in $ApprovedBundleSha256){if($hash -notmatch '^[a-f0-9]{64}$'){throw 'Invalid approved bundle hash'}}
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
# Commit pins the public hub release, not the private audiobook repository.
$base='https://raw.githubusercontent.com/kimjae134679/project-operations-hub/'+$Commit+'/04_COMMUNICATION/remote-bridge/releases/20261006-v2/'
$temp=Join-Path ([IO.Path]::GetTempPath()) ('ProjectBridge_'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
 $manifestPath=Join-Path $temp 'release_manifest.json'
 Invoke-WebRequest -UseBasicParsing -Uri ($base+'release_manifest.json') -OutFile $manifestPath
 if((Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $ManifestSha256){throw 'Manifest hash mismatch'}
 $manifest=Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
 if($manifest.schemaVersion -ne 2){throw 'Unsupported manifest'}
 $allowed=@('bridge_worker.py','BridgeLauncher.cs','DesktopAutomation.cs','universal_worker.py','universal_actions.py','process_runner.py','README.md','UNIVERSAL_PROTOCOL.md','프로젝트_사용안내.md','tests/test_bridge.py','tests/test_universal_worker.py','tests/test_universal_actions.py','tests/test_desktop_helper_source.py')
 $seen=@{}

 foreach($row in $manifest.files){
  if($row.path -notin $allowed -or $row.sha256 -notmatch '^[a-f0-9]{64}$' -or $seen.ContainsKey($row.path)){throw 'Unexpected install file'}
  $seen[$row.path]=$true
  $file=Join-Path $temp $row.path
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $file) | Out-Null
  Invoke-WebRequest -UseBasicParsing -Uri ($base+$row.path) -OutFile $file
  if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $row.sha256){throw 'Install file hash mismatch'}
 }
 if(@($manifest.files).Count -ne $allowed.Count){throw 'Incomplete manifest'}
 New-Item -ItemType Directory -Force -Path $target,(Join-Path $target 'state') | Out-Null
 $stamp=[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss_ffff')
 foreach($name in $allowed){
  $existing=Join-Path $target $name
  if(Test-Path -LiteralPath $existing){
   $backup=Join-Path $target ('state\install-backups\'+$stamp)
   New-Item -ItemType Directory -Force -Path $backup | Out-Null
   $backupFile=Join-Path $backup $name
   New-Item -ItemType Directory -Force -Path (Split-Path -Parent $backupFile) | Out-Null
   Copy-Item -LiteralPath $existing -Destination $backupFile
  }
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $existing) | Out-Null
  Copy-Item -LiteralPath (Join-Path $temp $name) -Destination $existing -Force
 }
 $configPath=Join-Path $target 'config.json'
 $device='KJW-'+([Guid]::NewGuid().ToString('N').Substring(0,12))
 $approved=@($ApprovedBundleSha256)
 if(Test-Path -LiteralPath $configPath){
  $old=Get-Content -LiteralPath $configPath -Raw -Encoding UTF8 | ConvertFrom-Json
  if($old.deviceId){$device=$old.deviceId}
  $approved=@($approved)+@($old.approvedBundleSha256)
 }
 $cfg=@{schemaVersion=2;deviceId=$device;projectRoot=$project;audioEnabled=$audioEnabled;python=$python;hubRepository='kimjae134679/project-operations-hub';privateRepository='kimjae134679/Mushoku-Tensei-AI-Audiobook';privateBranch='remote/pc-bridge';uiHelper=(Join-Path $target 'DesktopAutomation.exe');protectedRoots=@([Environment]::GetFolderPath('Desktop'));pollSeconds=90;approvedBundleSha256=@($approved|Sort-Object -Unique)}
 [IO.File]::WriteAllText($configPath,($cfg|ConvertTo-Json -Depth 5),(New-Object Text.UTF8Encoding($false)))
 $csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
 if(!(Test-Path -LiteralPath $csc)){$csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'}
 if(!(Test-Path -LiteralPath $csc)){throw '.NET Framework compiler missing'}
 $exe=Join-Path $target '프로젝트연결.exe'
 $compiled=Join-Path $temp '프로젝트연결.exe'
 & $csc /nologo /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll (('/out:')+$compiled) (Join-Path $target 'BridgeLauncher.cs')
 if($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $compiled)){throw 'Bridge native launcher compile failed'}
 if(Test-Path -LiteralPath $exe){
  $backup=Join-Path $target ('state\install-backups\'+$stamp)
  New-Item -ItemType Directory -Force -Path $backup|Out-Null
  Copy-Item -LiteralPath $exe -Destination (Join-Path $backup '프로젝트연결.exe')
 }
 Copy-Item -LiteralPath $compiled -Destination $exe -Force
 $uiCompiled=Join-Path $temp 'DesktopAutomation.exe'
 & $csc /nologo /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll (('/out:')+$uiCompiled) (Join-Path $target 'DesktopAutomation.cs')
 if($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $uiCompiled)){throw 'Desktop helper compile failed'}
 # Pure ABI/argument tests never capture or inject input into the real desktop.
 $oldLocation=Get-Location
 try {
  Set-Location -LiteralPath $target
  & $python -m unittest discover -s tests -p test_desktop_helper_source.py
  if($LASTEXITCODE -ne 0){throw 'Desktop helper ABI validation failed'}
 } finally {Set-Location -LiteralPath $oldLocation.Path}
 $uiExe=Join-Path $target 'DesktopAutomation.exe'
 if(Test-Path -LiteralPath $uiExe){
  $backup=Join-Path $target ('state\install-backups\'+$stamp)
  New-Item -ItemType Directory -Force -Path $backup | Out-Null
  Copy-Item -LiteralPath $uiExe -Destination (Join-Path $backup 'DesktopAutomation.exe')
 }
 Copy-Item -LiteralPath $uiCompiled -Destination $uiExe -Force

 if($StartAtLogin){
  $taskName='ProjectBridge_Login'
  $task=Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
  if($task){
   if(@($task.Actions).Count -ne 1 -or $task.Actions[0].Execute -ne $exe -or $task.Actions[0].Arguments -ne '--background'){throw 'Existing task is not owned by this bridge; refusing replacement'}
  } else {
   $action=New-ScheduledTaskAction -Execute $exe -Argument '--background' -WorkingDirectory $target
   $trigger=New-ScheduledTaskTrigger -AtLogOn -User ([Security.Principal.WindowsIdentity]::GetCurrent().Name)
   $settings=New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero)
   try {
    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Settings $settings -Description 'Project Bridge: explicit project queue, native windowless worker' | Out-Null
   } catch {
    # The current user's Startup directory works without admin elevation.
    # Preserve any unowned entry and keep the program itself on D drive.
    $startup=[Environment]::GetFolderPath('Startup')
    $shortcutPath=Join-Path $startup 'ProjectBridge_Login.lnk'
    $shell=New-Object -ComObject WScript.Shell
    $shortcut=$shell.CreateShortcut($shortcutPath)
    if((Test-Path -LiteralPath $shortcutPath) -and ($shortcut.TargetPath -ne $exe -or $shortcut.Arguments -ne '--background')){throw 'Existing login shortcut is not owned by this bridge'}
    $shortcut.TargetPath=$exe;$shortcut.Arguments='--background';$shortcut.WorkingDirectory=$target;$shortcut.Save()
    Write-Output 'Login startup uses your own Startup folder shortcut; no desktop shortcut is created.'
   }
  }
 }
 Start-Process -FilePath $exe -WorkingDirectory $target
 Write-Output ('Installed: '+$exe)
 Write-Output 'GitHub login uses your existing local gh/git credential manager. No token is printed or uploaded.'
} finally {Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue}
