param(
 [ValidatePattern('^[a-f0-9]{40}$')][string]$Commit,
 [ValidatePattern('^[a-f0-9]{64}$')][string]$ManifestSha256,
 [string]$SourceDirectory,
 [string[]]$ApprovedBundleSha256=@(),
 [switch]$StartAtLogin
)
$ErrorActionPreference='Stop'
$runningInstaller=$null
if($MyInvocation.MyCommand -is [System.Management.Automation.ExternalScriptInfo]){$runningInstaller=$PSCommandPath}
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
function Start-InstalledBridge([bool]$Registered){
 # Installation is an authorized resume. Clear the same flags as --resume
 # before the task's --background action checks the current login stop state.
 foreach($name in @('stop.flag','disconnected.flag','stopped_logon.txt')){
  $flag=Join-Path $statePath $name
  if(Test-Path -LiteralPath $flag){Remove-Item -LiteralPath $flag -Force}
 }
 if($Registered){
  try{Start-ScheduledTask -TaskName $taskName -ErrorAction Stop;return}
  catch{Write-Output 'Login task could not start now; using hidden native resume.'}
 }
 Start-Process -FilePath $exePath -ArgumentList '--resume' -WorkingDirectory $target -WindowStyle Hidden
}
function Stage-InstallerSource([string]$EmbeddedSource,[string]$RunningInstaller,[string]$PinnedBase,[string]$Destination){
 if($EmbeddedSource){
  $candidate=Join-Path $EmbeddedSource 'install.ps1'
  if(!(Test-Path -LiteralPath $candidate -PathType Leaf)){throw 'Embedded installer source missing'}
  if($RunningInstaller -and (Test-Path -LiteralPath $RunningInstaller -PathType Leaf) -and (Get-FileHash -LiteralPath $candidate -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $RunningInstaller -Algorithm SHA256).Hash){throw 'Installer source binding mismatch'}
  Copy-Item -LiteralPath $candidate -Destination $Destination
 }elseif($RunningInstaller -and (Test-Path -LiteralPath $RunningInstaller -PathType Leaf)){
  Copy-Item -LiteralPath $RunningInstaller -Destination $Destination
 }else{
  if(!$PinnedBase){throw 'Pinned installer source required for inline invocation'}
  Invoke-WebRequest -UseBasicParsing -Uri ($PinnedBase+'install.ps1') -OutFile $Destination
 }
}
$exeName=([char[]]@(0xd504,0xb85c,0xc81d,0xd2b8,0xc5f0,0xacb0,0x2e,0x65,0x78,0x65) -join '')
$guideName=([char[]]@(0xd504,0xb85c,0xc81d,0xd2b8,0x5f,0xc0ac,0xc6a9,0xc548,0xb0b4,0x2e,0x6d,0x64) -join '')
$target='D:\A_KJ\AI\Applications\ProjectBridge'
$runtimePath=Join-Path $target 'Runtime'
$statePath=Join-Path $target 'state'
$exePath=Join-Path $target $exeName
$allowed=@('bridge_worker.py','BridgeLauncher.cs','DesktopAutomation.cs','universal_worker.py','universal_actions.py','process_runner.py','local_api.py','bridge_mcp.py','README.md','UNIVERSAL_PROTOCOL.md',$guideName,'tests/test_bridge.py','tests/test_universal_worker.py','tests/test_universal_actions.py','tests/test_desktop_helper_source.py','tests/test_shared_scheduler.py','tests/test_github_client.py','tests/test_local_api.py')
foreach($hash in $ApprovedBundleSha256){if($hash -notmatch '^[a-f0-9]{64}$'){throw 'Invalid approved bundle hash'}}
$tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$stage=Join-Path $tempRoot ('ProjectBridge3_'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage|Out-Null
try {
 $base=$null
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
 # The installer is the trusted entry script, outside the payload manifest.
 # Its tests need the exact same source in the otherwise isolated stage.
 Stage-InstallerSource $SourceDirectory $runningInstaller $base (Join-Path $stage 'install.ps1')
 # The user sees one entry program and its guide. Implementation files live
 # together in Runtime; config/state keep legacy absolute paths for live jobs.
 $destinations=@{}
 foreach($name in $allowed){$destinations[$name]=if($name -eq $guideName){$name}else{Join-Path 'Runtime' $name}}
 $destinations['release_manifest.json']='Runtime\release_manifest.json'
 $destinations[$exeName]=$exeName
 $destinations['DesktopAutomation.exe']='Runtime\DesktopAutomation.exe'
 $legacyHashes=@{}
 foreach($row in $manifest.files){$legacyHashes[$row.path]=@([string]$row.sha256)}
 # Original immutable v2 manifest hashes plus the narrow metadata URL hotfix.
 $v2Hashes=@{
  'bridge_worker.py'=@('c2c87fdeb1a5ef0c13baa20ed176247e47e826928ee556be15bc8ef31ab5cbec','b832694ec80a847962d07552b014cae1580242e1ead6f02344754c21e6603373');
  'BridgeLauncher.cs'=@('535bb89b6c2b8861ce9b92d007bf25c23daf9c51026db17e6bf1c0c1744c1b91');
  'DesktopAutomation.cs'=@('4b8dd583eef433a4a387e4ea756f9b53a1cd9167c81621ebb68cc88904e46c08');
  'universal_worker.py'=@('fc6146616aacdea1ad7d3615c1a1a1ff5f428bc9dfde352725e074c0e16bd5ea');
  'universal_actions.py'=@('8aaf6ce34c0d4003df59ee8a6f155f5dac990c0278ad3359e7d2bdda9b67b7bf');
  'process_runner.py'=@('9306abdee8989c27a894b2f5d5a9b50fb8e494495281ad8a5aa1077f74dd1e1d');
  'README.md'=@('b8d4b3851ff95102c3e3b27848af0f81a189a7e77da8a430b4b533dd6eb926c8');
  'UNIVERSAL_PROTOCOL.md'=@('c44ab670de0ec14d60b848d4c49c73bbfb2c2627c327b11a190a97394ea85979');
  'tests/test_bridge.py'=@('0f61dce6109e576d1daca09fa72a97272a0c8ccf6ff6833f5885b1a5746d2f4d');
  'tests/test_universal_worker.py'=@('dd49039e89671d10cb36c94f203205ab220cab911daa3fb971ef2637d882b463');
  'tests/test_universal_actions.py'=@('cc2e053259b720d11d832bfe7f5448340620af9d266a0d17d362614cec1247f1');
  'tests/test_desktop_helper_source.py'=@('d92a0cb02d6b09ca13c5e5cd53e264970f2d2d2485a9d5141ada7d65517a2836')
 }
 foreach($name in $v2Hashes.Keys){$legacyHashes[$name]=@($legacyHashes[$name])+@($v2Hashes[$name])}
 $priorManifest=Join-Path $target 'release_manifest.json'
 if(Test-Path -LiteralPath $priorManifest){
  try {
   $prior=Get-Content -LiteralPath $priorManifest -Raw -Encoding UTF8|ConvertFrom-Json
   if($prior.schemaVersion -in @(2,3)){
    foreach($row in $prior.files){if($row.path -in $allowed -and $row.sha256 -match '^[a-f0-9]{64}$'){$legacyHashes[$row.path]=@($legacyHashes[$row.path])+@([string]$row.sha256)}}
   }
  }catch{}
 }
 # Validate startup ownership before stopping anything or touching install files.
 # A conflicting task/shortcut cannot leave a half-updated, stopped bridge.
 if(Test-Path -LiteralPath $runtimePath){
  $runtimeInfo=Get-Item -LiteralPath $runtimePath -Force
  if(!$runtimeInfo.PSIsContainer -or ($runtimeInfo.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Runtime path is not a normal owned installation directory'}
 }
 $taskName='ProjectBridge_Login';$shortcutPath=Join-Path ([Environment]::GetFolderPath('Startup')) 'ProjectBridge_Login.lnk'
 $taskRegistered=$false
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
 $priorUiHelper=[string]$cfg.uiHelper
 $cfg.uiHelper=Join-Path $runtimePath 'DesktopAutomation.exe'
 $cfg.protectedRoots=@($cfg.protectedRoots)+@([Environment]::GetFolderPath('Desktop'))|Sort-Object -Unique
 if(!$cfg.pollSeconds){$cfg.pollSeconds=90}
 $cfg.approvedBundleSha256=@(@($cfg.approvedBundleSha256)+@($ApprovedBundleSha256)|Where-Object{$_}|Sort-Object -Unique)
 $python=[string]$cfg.python
 $csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
 if(!(Test-Path -LiteralPath $csc)){$csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'}
 if(!(Test-Path -LiteralPath $csc)){throw '.NET Framework compiler missing'}
 foreach($pair in @(@('BridgeLauncher.cs',$exeName),@('DesktopAutomation.cs','DesktopAutomation.exe'))){
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
 New-Item -ItemType Directory -Force -Path $target,$statePath,$runtimePath|Out-Null
 $backup=Join-Path $statePath ('install-backups\'+[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss_ffff'))
 New-Item -ItemType Directory -Path $backup -Force|Out-Null
 $backupNames=@($destinations.Values)+@($allowed)+@('release_manifest.json','config.json','DesktopAutomation.exe')|Sort-Object -Unique
 foreach($name in $backupNames){
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
  $ownedWorkerCommand=$info.CommandLine -and ($info.CommandLine.Contains((Join-Path $target 'bridge_worker.py')) -or $info.CommandLine.Contains((Join-Path $runtimePath 'bridge_worker.py')))
  if($candidate -and $candidate.StartTime.ToUniversalTime().ToString('o') -eq [string]$ticket.startedAt -and $candidate.Path -in $pythonPaths -and $ownedWorkerCommand){$child=$candidate}
 }
 # The recorded bridge child is separate from audiobook production; only this
 # exact child and the native launcher at this installation path may be stopped.
 foreach($info in @(Get-CimInstance Win32_Process -Filter ("Name='"+$exeName+"'") -ErrorAction SilentlyContinue)){
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
  foreach($name in $destinations.Keys){
   $destination=Join-Path $target $destinations[$name];New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination)|Out-Null;Copy-Item -LiteralPath (Join-Path $stage $name) -Destination $destination -Force
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
  try{Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description 'ProjectBridge 3.0 hidden shared executor; local jobs do not require GitHub' -Force|Out-Null;$taskRegistered=$true}
  catch{
   $shell=New-Object -ComObject WScript.Shell;$shortcut=$shell.CreateShortcut($shortcutPath)
   if((Test-Path -LiteralPath $shortcutPath) -and ($shortcut.TargetPath -ne $exePath -or $shortcut.Arguments -ne '--background')){throw 'Unowned startup shortcut preserved'}
   $shortcut.TargetPath=$exePath;$shortcut.Arguments='--background';$shortcut.WorkingDirectory=$target;$shortcut.Save()
  }
 }
 Start-InstalledBridge $taskRegistered
 $installationCommitted=$true
 # Only remove checked copies from the old flat layout after the new worker
 # can be started. Unknown/user-edited files are preserved in place.
 foreach($name in $legacyHashes.Keys){
  if($name -eq $guideName){continue}
  $oldFile=[IO.Path]::GetFullPath((Join-Path $target $name))
  if(!$oldFile.StartsWith(($target+'\'),[StringComparison]::OrdinalIgnoreCase)){throw 'Legacy path outside install root'}
  if(Test-Path -LiteralPath $oldFile -PathType Leaf){
   $oldInfo=Get-Item -LiteralPath $oldFile -Force
   if(($oldInfo.Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0 -and (Get-FileHash -LiteralPath $oldFile -Algorithm SHA256).Hash.ToLowerInvariant() -in $legacyHashes[$name]){
    Remove-Item -LiteralPath $oldFile -Force -ErrorAction SilentlyContinue
   }
  }
 }
 # Remaining owned metadata is hidden, with its path unchanged. Do not delete
 # a compiled legacy helper without a verified executable hash.
 $hide=@($runtimePath,$statePath,$configPath)
 if(Test-Path -LiteralPath $priorManifest){$hide+=@($priorManifest)}
 if($priorUiHelper -eq (Join-Path $target 'DesktopAutomation.exe') -and (Test-Path -LiteralPath $priorUiHelper)){$hide+=@($priorUiHelper)}
 $legacyTests=Join-Path $target 'tests'
 if(Test-Path -LiteralPath $legacyTests){$hide+=@($legacyTests)}
 $legacyCache=Join-Path $target '__pycache__'
 if(Test-Path -LiteralPath $legacyCache -PathType Container){
  $cacheEntries=@(Get-ChildItem -LiteralPath $legacyCache -Force)
  $ownedStems=@($allowed|Where-Object{$_ -like '*.py' -and $_ -notlike 'tests/*'}|ForEach-Object{[IO.Path]::GetFileNameWithoutExtension($_)})
  $unknownCache=@($cacheEntries|Where-Object{$_.PSIsContainer -or $_.Extension -ne '.pyc' -or ($_.BaseName.Split('.')[0] -notin $ownedStems)})
  if($unknownCache.Count -eq 0){$hide+=@($legacyCache)}
 }
 foreach($path in $hide){
  try{$info=Get-Item -LiteralPath $path -Force;$info.Attributes=$info.Attributes -bor [IO.FileAttributes]::Hidden}catch{}
 }
 Write-Output 'Installed ProjectBridge 3.0. Check local connection and remote relay separately.'
 Write-Output 'Existing device ID, config, production work and journals were preserved; no desktop files were created.'
}finally{
 # Normal exceptions and PowerShell pipeline cancellation restore checked
 # backups and the previous user's pause/stop choices. Hard OS termination is
 # inherently not catchable; retained backups permit an explicit repair.
 if($installTouched -and !$installationCommitted -and $backup){
  foreach($saved in @(Get-ChildItem -LiteralPath $backup -File -Recurse -Force)){
   $relative=$saved.FullName.Substring($backup.Length).TrimStart('\');$restore=[IO.Path]::GetFullPath((Join-Path $target $relative))
   if(!$restore.StartsWith(($target+'\'),[StringComparison]::OrdinalIgnoreCase)){throw 'Backup path outside install root'}
   Copy-Item -LiteralPath $saved.FullName -Destination $restore -Force -ErrorAction SilentlyContinue
  }
  foreach($name in $priorFlags.Keys){
   $flag=Join-Path $statePath $name
   if($null -eq $priorFlags[$name]){Remove-Item -LiteralPath $flag -ErrorAction SilentlyContinue}else{[IO.File]::WriteAllText($flag,[string]$priorFlags[$name])}
  }
  if((Test-Path -LiteralPath (Join-Path $backup $exeName)) -and (Test-Path -LiteralPath $exePath)){Start-Process -FilePath $exePath -ArgumentList '--background' -WorkingDirectory $target -WindowStyle Hidden -ErrorAction SilentlyContinue}
 }
 $resolved=[IO.Path]::GetFullPath($stage)
 if($resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path -Leaf $resolved) -like 'ProjectBridge3_*'){Remove-Item -LiteralPath $resolved -Recurse -Force -ErrorAction SilentlyContinue}
 $installMutex.ReleaseMutex();$installMutex.Dispose()
}
