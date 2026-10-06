param(
 [ValidatePattern('^[a-f0-9]{40}$')][string]$Commit,
 [ValidatePattern('^[a-f0-9]{64}$')][string]$ManifestSha256,
 [string]$SourceDirectory,
 [string[]]$ApprovedBundleSha256=@(),
 [switch]$StartAtLogin
)
$ErrorActionPreference='Stop'
$installMutex=New-Object -TypeName System.Threading.Mutex -ArgumentList @($false,'Local\ProjectBridge_Install')
try{$hasInstallLock=$installMutex.WaitOne(0)}catch [System.Threading.AbandonedMutexException]{$hasInstallLock=$true}
if(!$hasInstallLock){ $installMutex.Dispose();throw 'Another ProjectBridge install is running' }
$installTouched=$false;$installationCommitted=$false;$backup=$null;$priorFlags=@{}
function Invoke-HiddenTool([string]$File,[string[]]$Arguments,[string]$WorkingDirectory){
 $info=New-Object Diagnostics.ProcessStartInfo
 $info.FileName=$File;$info.UseShellExecute=$false;$info.CreateNoWindow=$true;$info.WindowStyle='Hidden'
 $info.WorkingDirectory=$WorkingDirectory;$info.RedirectStandardOutput=$true;$info.RedirectStandardError=$true
 $info.StandardOutputEncoding=New-Object Text.UTF8Encoding($false);$info.StandardErrorEncoding=New-Object Text.UTF8Encoding($false)
 # All arguments below are fixed switches or Windows file paths (which cannot
 # contain a quote). Reject rather than shell-expand an unexpected argument.
 foreach($value in $Arguments){if($value.Contains('"') -or $value.Contains([char]0)){throw 'Invalid native tool argument'}}
 $info.Arguments=($Arguments|ForEach-Object{'"'+$_+'"'}) -join ' '
 $info.EnvironmentVariables['PYTHONIOENCODING']='utf-8'
 $process=New-Object Diagnostics.Process;$process.StartInfo=$info
 try {
  if(!$process.Start()){throw 'Cannot start hidden native tool'}
  $stdout=$process.StandardOutput.ReadToEndAsync();$stderr=$process.StandardError.ReadToEndAsync()
  $process.WaitForExit();$result=@{ExitCode=$process.ExitCode;Output=$stdout.Result;Error=$stderr.Result}
  return $result
 }finally{$process.Dispose()}
}
$target='D:\A_KJ\AI\Applications\ProjectBridge'
$statePath=Join-Path $target 'state'
$exePath=Join-Path $target '프로젝트연결.exe'
$allowed=@('bridge_worker.py','BridgeLauncher.cs','DesktopAutomation.cs','universal_worker.py','universal_actions.py','process_runner.py','local_api.py','bridge_mcp.py','README.md','UNIVERSAL_PROTOCOL.md','프로젝트_사용안내.md','tests/test_bridge.py','tests/test_universal_worker.py','tests/test_universal_actions.py','tests/test_desktop_helper_source.py','tests/test_shared_scheduler.py','tests/test_github_client.py','tests/test_local_api.py')
foreach($hash in $ApprovedBundleSha256){if($hash -notmatch '^[a-f0-9]{64}$'){throw 'Invalid approved bundle hash'}}
$tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$stage=Join-Path $tempRoot ('ProjectBridge3_'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage|Out-Null
try {
 if($SourceDirectory){
  $source=[IO.Path]::GetFullPath($SourceDirectory)
  $manifestFile=Join-Path $source 'release_manifest.json'
  if(!(Test-Path -LiteralPath $manifestFile -PathType Leaf)){throw 'Embedded release manifest missing'}
  Copy-Item -LiteralPath $manifestFile -Destination (Join-Path $stage 'release_manifest.json')
 }else{
  if(!$Commit -or !$ManifestSha256){throw 'Pinned commit and manifest SHA required for network installation'}
  [Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
  $base='https://raw.githubusercontent.com/kimjae134679/project-operations-hub/'+$Commit+'/04_COMMUNICATION/remote-bridge/releases/20261006-v3/'
  Invoke-WebRequest -UseBasicParsing -Uri ($base+'release_manifest.json') -OutFile (Join-Path $stage 'release_manifest.json')
 }
 $manifestFile=Join-Path $stage 'release_manifest.json'
 if($ManifestSha256 -and (Get-FileHash -LiteralPath $manifestFile -Algorithm SHA256).Hash.ToLowerInvariant() -ne $ManifestSha256){throw 'Manifest hash mismatch'}
 $manifest=Get-Content -LiteralPath $manifestFile -Raw -Encoding UTF8|ConvertFrom-Json
 if($manifest.schemaVersion -ne 3 -or $manifest.version -ne '3.0.0'){throw 'Unsupported release manifest'}
 $seen=@{}
 foreach($row in $manifest.files){
  if($row.path -notin $allowed -or $row.sha256 -notmatch '^[a-f0-9]{64}$' -or $seen.ContainsKey($row.path)){throw 'Unexpected release file'}
  $seen[$row.path]=$true;$file=Join-Path $stage $row.path
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $file)|Out-Null
  if($SourceDirectory){Copy-Item -LiteralPath (Join-Path $source $row.path) -Destination $file}else{Invoke-WebRequest -UseBasicParsing -Uri ($base+$row.path) -OutFile $file}
  if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $row.sha256){throw 'Release file hash mismatch'}
 }
 if(@($manifest.files).Count -ne $allowed.Count){throw 'Incomplete release manifest'}
 # Validate startup ownership before stopping anything or touching install files.
 # A conflicting task/shortcut cannot leave a half-updated, stopped bridge.
 $taskName='ProjectBridge_Login';$shortcutPath=Join-Path ([Environment]::GetFolderPath('Startup')) 'ProjectBridge_Login.lnk'
 if($StartAtLogin){
  $task=Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
  if($task -and (@($task.Actions).Count -ne 1 -or $task.Actions[0].Execute -ne $exePath -or $task.Actions[0].Arguments -ne '--background')){throw 'Existing login task is not owned by this bridge'}
  if(Test-Path -LiteralPath $shortcutPath){
   $shell=New-Object -ComObject WScript.Shell;$existingShortcut=$shell.CreateShortcut($shortcutPath)
   if($existingShortcut.TargetPath -ne $exePath -or $existingShortcut.Arguments -ne '--background'){throw 'Unowned startup shortcut preserved'}
  }
 }
 $configPath=Join-Path $target 'config.json';$cfg=@{}
 if(Test-Path -LiteralPath $configPath){
  $old=Get-Content -LiteralPath $configPath -Raw -Encoding UTF8|ConvertFrom-Json
  foreach($property in $old.PSObject.Properties){$cfg[$property.Name]=$property.Value}
 }
 if(!$cfg.deviceId){$cfg.deviceId='KJW-'+[Guid]::NewGuid().ToString('N').Substring(0,12)}
 if(!$cfg.projectRoot){$cfg.projectRoot='D:\AI\VoiceAudiobook'}
 if(!$cfg.ContainsKey('audioEnabled')){$cfg.audioEnabled=Test-Path -LiteralPath $cfg.projectRoot -PathType Container}
 if(!$cfg.python -or !(Test-Path -LiteralPath $cfg.python)){
  $cfg.python='D:\AI\envs\cosyvoice\python.exe'
  if(!(Test-Path -LiteralPath $cfg.python)){$cfg.python=(Get-Command python.exe -ErrorAction Stop).Source}
 }
 $cfg.schemaVersion=3;$cfg.hubRepository='kimjae134679/project-operations-hub'
 if(!$cfg.privateRepository){$cfg.privateRepository='kimjae134679/Mushoku-Tensei-AI-Audiobook'}
 if(!$cfg.privateBranch){$cfg.privateBranch='remote/pc-bridge'}
 if(!$cfg.maxWorkers){$cfg.maxWorkers=4}
 $cfg.uiHelper=Join-Path $target 'DesktopAutomation.exe'
 $cfg.protectedRoots=@($cfg.protectedRoots)+@([Environment]::GetFolderPath('Desktop'))|Sort-Object -Unique
 if(!$cfg.pollSeconds){$cfg.pollSeconds=90}
 $cfg.approvedBundleSha256=@(@($cfg.approvedBundleSha256)+@($ApprovedBundleSha256)|Where-Object{$_}|Sort-Object -Unique)
 $python=[string]$cfg.python
 $csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
 if(!(Test-Path -LiteralPath $csc)){$csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'}
 if(!(Test-Path -LiteralPath $csc)){throw '.NET Framework compiler missing'}
 foreach($pair in @(@('BridgeLauncher.cs','프로젝트연결.exe'),@('DesktopAutomation.cs','DesktopAutomation.exe'))){
  $compiled=Invoke-HiddenTool $csc @('/nologo','/target:winexe','/reference:System.Windows.Forms.dll','/reference:System.Drawing.dll','/reference:System.Web.Extensions.dll',(('/out:')+(Join-Path $stage $pair[1])),(Join-Path $stage $pair[0])) $stage
  if($compiled.ExitCode -ne 0 -or !(Test-Path -LiteralPath (Join-Path $stage $pair[1]))){throw 'Native bridge compile failed'}
 }
 $priorLocation=Get-Location
 try {
  Set-Location -LiteralPath $stage
  $tested=Invoke-HiddenTool $python @('-m','unittest','discover','-s','tests','-p','test_*.py') $stage
  if($tested.Output){Write-Output $tested.Output};if($tested.Error){Write-Output $tested.Error}
  if($tested.ExitCode -ne 0){throw 'Bridge release tests failed; existing installation remains untouched'}
 }finally{Set-Location -LiteralPath $priorLocation.Path}
 # All downloads, hashes, compilation and tests have passed before stopping v2.
 New-Item -ItemType Directory -Force -Path $target,$statePath|Out-Null
 $backup=Join-Path $statePath ('install-backups\'+[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss_ffff'))
 New-Item -ItemType Directory -Path $backup -Force|Out-Null
 foreach($name in @($allowed)+@('release_manifest.json','config.json','프로젝트연결.exe','DesktopAutomation.exe')){
  $existing=Join-Path $target $name
  if(Test-Path -LiteralPath $existing){$saved=Join-Path $backup $name;New-Item -ItemType Directory -Force -Path (Split-Path -Parent $saved)|Out-Null;Copy-Item -LiteralPath $existing -Destination $saved}
 }
 foreach($name in @('stop.flag','disconnected.flag','stopped_logon.txt')){
  $flag=Join-Path $statePath $name;$priorFlags[$name]=if(Test-Path -LiteralPath $flag){[IO.File]::ReadAllText($flag)}else{$null}
 }
 $installTouched=$true
 [IO.File]::WriteAllText((Join-Path $statePath 'stop.flag'),'installation pause')
 $ticketPath=Join-Path $statePath 'launcher_child.json';$child=$null
 if(Test-Path -LiteralPath $ticketPath){
  $ticket=Get-Content -LiteralPath $ticketPath -Raw -Encoding UTF8|ConvertFrom-Json
  $candidate=Get-Process -Id ([int]$ticket.pid) -ErrorAction SilentlyContinue
  $info=Get-CimInstance Win32_Process -Filter ('ProcessId='+[int]$ticket.pid) -ErrorAction SilentlyContinue
  $pythonPaths=@($python,(Join-Path (Split-Path -Parent $python) 'pythonw.exe'))
  if($candidate -and $candidate.StartTime.ToUniversalTime().ToString('o') -eq [string]$ticket.startedAt -and $candidate.Path -in $pythonPaths -and $info.CommandLine -and $info.CommandLine.Contains((Join-Path $target 'bridge_worker.py'))){$child=$candidate}
 }
 # The recorded bridge child is separate from audiobook production; only this
 # exact child and the native launcher at this installation path may be stopped.
 foreach($info in @(Get-CimInstance Win32_Process -Filter "Name='프로젝트연결.exe'" -ErrorAction SilentlyContinue)){
  if($info.ExecutablePath -eq $exePath){
   $candidate=Get-Process -Id $info.ProcessId -ErrorAction SilentlyContinue
   if($candidate){$birth=$candidate.StartTime.ToUniversalTime().ToString('o');$live=Get-Process -Id $candidate.Id -ErrorAction SilentlyContinue;if($live -and $live.Path -eq $exePath -and $live.StartTime.ToUniversalTime().ToString('o') -eq $birth){Stop-Process -Id $live.Id -Force}}
  }
 }
 if($child -and !$child.WaitForExit(10000)){
  $live=Get-Process -Id $child.Id -ErrorAction SilentlyContinue
  if($live -and $live.StartTime.ToUniversalTime().ToString('o') -eq [string]$ticket.startedAt -and $live.Path -in $pythonPaths){Stop-Process -Id $live.Id -Force}
 }
 try {
  foreach($name in @($allowed)+@('release_manifest.json','프로젝트연결.exe','DesktopAutomation.exe')){
   $destination=Join-Path $target $name;New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination)|Out-Null;Copy-Item -LiteralPath (Join-Path $stage $name) -Destination $destination -Force
  }
  [IO.File]::WriteAllText($configPath,($cfg|ConvertTo-Json -Depth 12),(New-Object Text.UTF8Encoding($false)))
 }catch{
  throw
 }
 if($StartAtLogin){
  $action=New-ScheduledTaskAction -Execute $exePath -Argument '--background' -WorkingDirectory $target
  $identity=[Security.Principal.WindowsIdentity]::GetCurrent().Name
  $trigger=New-ScheduledTaskTrigger -AtLogOn -User $identity
  $principal=New-ScheduledTaskPrincipal -UserId $identity -LogonType Interactive -RunLevel Limited
  $settings=New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero) -RestartCount 999 -RestartInterval (New-TimeSpan -Minutes 1) -StartWhenAvailable
  try{Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description 'ProjectBridge 3.0 hidden shared executor; local jobs do not require GitHub' -Force|Out-Null}
  catch{
   $shell=New-Object -ComObject WScript.Shell;$shortcut=$shell.CreateShortcut($shortcutPath)
   if((Test-Path -LiteralPath $shortcutPath) -and ($shortcut.TargetPath -ne $exePath -or $shortcut.Arguments -ne '--background')){throw 'Unowned startup shortcut preserved'}
   $shortcut.TargetPath=$exePath;$shortcut.Arguments='--background';$shortcut.WorkingDirectory=$target;$shortcut.Save()
  }
 }
 Start-Process -FilePath $exePath -ArgumentList '--resume' -WorkingDirectory $target -WindowStyle Hidden
 $installationCommitted=$true
 Write-Output 'Installed ProjectBridge 3.0. Check local connection and remote relay separately.'
 Write-Output 'Existing device ID, config, production work and journals were preserved; no desktop files were created.'
}finally{
 # Normal exceptions and PowerShell pipeline cancellation restore checked
 # backups and the previous user's pause/stop choices. Hard OS termination is
 # inherently not catchable; retained backups permit an explicit repair.
 if($installTouched -and !$installationCommitted -and $backup){
  foreach($saved in @(Get-ChildItem -LiteralPath $backup -File -Recurse)){
   $relative=$saved.FullName.Substring($backup.Length).TrimStart('\');$restore=[IO.Path]::GetFullPath((Join-Path $target $relative))
   if(!$restore.StartsWith(($target+'\'),[StringComparison]::OrdinalIgnoreCase)){throw 'Backup path outside install root'}
   Copy-Item -LiteralPath $saved.FullName -Destination $restore -Force -ErrorAction SilentlyContinue
  }
  foreach($name in $priorFlags.Keys){
   $flag=Join-Path $statePath $name
   if($null -eq $priorFlags[$name]){Remove-Item -LiteralPath $flag -ErrorAction SilentlyContinue}else{[IO.File]::WriteAllText($flag,[string]$priorFlags[$name])}
  }
  if((Test-Path -LiteralPath (Join-Path $backup '프로젝트연결.exe')) -and (Test-Path -LiteralPath $exePath)){Start-Process -FilePath $exePath -ArgumentList '--background' -WorkingDirectory $target -WindowStyle Hidden -ErrorAction SilentlyContinue}
 }
 $resolved=[IO.Path]::GetFullPath($stage)
 if($resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path -Leaf $resolved) -like 'ProjectBridge3_*'){Remove-Item -LiteralPath $resolved -Recurse -Force -ErrorAction SilentlyContinue}
 $installMutex.ReleaseMutex();$installMutex.Dispose()
}
