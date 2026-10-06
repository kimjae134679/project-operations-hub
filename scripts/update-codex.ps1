param(
 [ValidatePattern('^[a-f0-9]{40}$')][string]$BridgeCommit,
 [ValidatePattern('^[a-f0-9]{64}$')][string]$ManifestSha256
)
$ErrorActionPreference='Stop'
$root='D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks'
New-Item -ItemType Directory -Path $root -Force|Out-Null
$report=@{requestedModel='gpt-6.1-sol';astraAllowed=$false;state='updating';startedAt=[DateTimeOffset]::UtcNow.ToString('o')}
function Save-Report { $report|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $root 'codex-update-result.json') -Encoding UTF8 }
function Test-BridgeControl {
 $endpointPath='D:\A_KJ\AI\Applications\ProjectBridge\state\local_endpoint.json'
 $deadline=[DateTime]::UtcNow.AddSeconds(40);$endpoint=$null;$status=$null
 do {
  try {
   $endpoint=Get-Content -LiteralPath $endpointPath -Raw -Encoding UTF8|ConvertFrom-Json
   $headers=@{'X-ProjectBridge-Token'=$endpoint.token}
   $status=Invoke-RestMethod -Uri ($endpoint.baseUrl+'/v1/status') -Headers $headers -TimeoutSec 5
   if($status.localReady){break}
  }catch{}
  Start-Sleep -Milliseconds 250
 }while([DateTime]::UtcNow -lt $deadline)
 if(!$status -or !$status.localReady){throw 'Bridge local channel did not become ready'}
 $jobId='repair-check-'+[guid]::NewGuid().ToString('N')
 # Python 3.10 ISO parsing accepts six fractional digits, not .NET's seven.
 $timeFormat="yyyy-MM-dd'T'HH:mm:ss.ffffffzzz"
 $job=@{id=$jobId;target='PC';deviceId=$status.device;projectId='Control-Tower';toolId='codex-repair-self-check';createdAt=[DateTimeOffset]::UtcNow.ToString($timeFormat,[Globalization.CultureInfo]::InvariantCulture);expiresAt=[DateTimeOffset]::UtcNow.AddMinutes(5).ToString($timeFormat,[Globalization.CultureInfo]::InvariantCulture);action='run_command';args=@{cwd=$root;python='print("REMOTE_CONTROL_OK")';timeoutSeconds=20}}
 Invoke-RestMethod -Method Post -Uri ($endpoint.baseUrl+'/v1/jobs') -Headers $headers -ContentType 'application/json' -Body ($job|ConvertTo-Json -Depth 8 -Compress) -TimeoutSec 10|Out-Null
 $deadline=[DateTime]::UtcNow.AddSeconds(35)
 do {
  $result=Invoke-RestMethod -Uri ($endpoint.baseUrl+'/v1/jobs/'+$jobId) -Headers $headers -TimeoutSec 5
  if($result.result){break}
  Start-Sleep -Milliseconds 250
 }while([DateTime]::UtcNow -lt $deadline)
 if($result.result.outcome -ne 'completed' -or !$result.result.data.succeeded -or $result.result.data.stdout -notmatch 'REMOTE_CONTROL_OK'){throw ('Bridge command self-check failed; job '+$jobId+' state '+$result.state+' outcome '+$result.result.outcome)}
 return @{localReady=$true;commandRoundtrip='pass';relayConnected=[bool]$status.relayConnected;jobId=$jobId}
}
function Invoke-Quiet([string]$file,[string[]]$arguments,[string]$name,[int]$timeout=600){
 $info=New-Object Diagnostics.ProcessStartInfo
 $info.FileName=$file;$info.UseShellExecute=$false;$info.CreateNoWindow=$true;$info.WindowStyle='Hidden';$info.WorkingDirectory=$root
 foreach($arg in $arguments){if($arg.Contains('"') -or $arg.Contains([char]0)){throw 'Invalid argument'}}
 $info.Arguments=($arguments|ForEach-Object{'"'+$_+'"'}) -join ' '
 $info.RedirectStandardOutput=$true;$info.RedirectStandardError=$true
 $process=New-Object Diagnostics.Process;$process.StartInfo=$info
 if(!$process.Start()){throw 'Process start failed'}
 $stdout=$process.StandardOutput.ReadToEndAsync();$stderr=$process.StandardError.ReadToEndAsync()
 if(!$process.WaitForExit($timeout*1000)){throw ('Operation still running; do not replay. PID '+$process.Id)}
 $stdout.Result|Set-Content -LiteralPath (Join-Path $root ($name+'.private.stdout.log')) -Encoding UTF8
 $stderr.Result|Set-Content -LiteralPath (Join-Path $root ($name+'.private.stderr.log')) -Encoding UTF8
 $exit=$process.ExitCode;$process.Dispose()
 if($exit -ne 0){throw ($name+' failed: exit '+$exit)}
 return $stdout.Result
}
Save-Report
try {
 $node=(Get-Command node.exe -ErrorAction Stop).Source
 $npm=(Get-Command npm.cmd -ErrorAction Stop).Source
 $npmScript=Join-Path (Split-Path $npm) 'node_modules\npm\bin\npm-cli.js'
 if(!(Test-Path -LiteralPath $npmScript)){throw 'npm CLI entry missing'}
 $report.installedVersion='0.160.1'
 $package=Join-Path $env:APPDATA 'npm\node_modules\@openai\codex'
 $packageJson=Join-Path $package 'package.json'
 $existingVersion=$null
 if(Test-Path -LiteralPath $packageJson){try{$existingVersion=(Get-Content -LiteralPath $packageJson -Raw|ConvertFrom-Json).version}catch{}}
 if($existingVersion -ne '0.160.1'){
  $report.phase='npm_update';Save-Report
  Invoke-Quiet $node @($npmScript,'install','-g','@openai/codex@0.160.1','--no-audit','--no-fund','--fetch-retries=0','--fetch-timeout=30000') 'npm-update'|Out-Null
 } else { $report.updateSkipped='already_current';Save-Report }
 $metadata=Get-Content -LiteralPath $packageJson -Raw|ConvertFrom-Json
 if($metadata.version -ne '0.160.1'){throw 'Updated global package version mismatch'}
 $exe=Join-Path $package 'node_modules\@openai\codex-win32-x64\vendor\x86_64-pc-windows-msvc\bin\codex.exe'
 if(!(Test-Path -LiteralPath $exe)){throw 'Updated native Codex executable missing'}
 $report.actualVersion=(Invoke-Quiet $exe @('--version') 'codex-version' 30).Trim()
 $report.state='updated';Save-Report
 if($BridgeCommit){
  if(!$ManifestSha256){throw 'Manifest SHA required for bridge repair'}
  $url='https://raw.githubusercontent.com/kimjae134679/project-operations-hub/'+$BridgeCommit+'/04_COMMUNICATION/remote-bridge/releases/20261006-v3/install.ps1'
  $report.phase='bridge_download';Save-Report
  $installerPath=Join-Path $root ('bridge-install-'+$BridgeCommit+'.ps1')
  Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $installerPath -TimeoutSec 30
  $installer=[IO.File]::ReadAllText($installerPath,[Text.Encoding]::UTF8).TrimStart([char]0xFEFF)
  $tokens=$null;$parseErrors=$null
  $ast=[System.Management.Automation.Language.Parser]::ParseInput($installer,[ref]$tokens,[ref]$parseErrors)
  if(@($parseErrors).Count -gt 0 -or !$ast.ParamBlock){throw 'Downloaded installer has no valid parameter block'}
  $report.phase='bridge_install';Save-Report
  & ([scriptblock]::Create($installer)) -Commit $BridgeCommit -ManifestSha256 $ManifestSha256 -StartAtLogin
  $report.bridgeRepair='installer_completed_check_local_and_relay_separately'
  $report.phase='bridge_control_self_check';Save-Report
  $report.bridgeControl=Test-BridgeControl;Save-Report
 }
 $report.phase='model_smoke';Save-Report
 $smoke=Invoke-Quiet $exe @('exec','--ignore-user-config','--ephemeral','--json','--model','gpt-6.1-sol','--sandbox','workspace-write','--skip-git-repo-check','-C',$root,'Return exactly CODEX_6_1_OK. Do not use tools or change files.') 'model-smoke' 120
 if($smoke -notmatch 'CODEX_6_1_OK' -or $smoke -match '"type"\s*:\s*"turn.failed"'){throw '6.1 model smoke did not complete'}
 $report.modelSmoke='pass';Save-Report
 $report.state='completed';$report.finishedAt=[DateTimeOffset]::UtcNow.ToString('o');Save-Report
 Write-Output 'Codex updated and 6.1 model smoke passed. Detailed results are saved in the existing work folder.'
} catch {
 $report.state='blocked';$report.reason=$_.Exception.Message;$report.errorId=$_.FullyQualifiedErrorId;$report.errorLine=$_.InvocationInfo.ScriptLineNumber;$report.errorPosition=$_.InvocationInfo.PositionMessage;$report.finishedAt=[DateTimeOffset]::UtcNow.ToString('o');Save-Report
 throw
}
