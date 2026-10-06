param([string]$RuntimeDirectory)
$ErrorActionPreference='Stop'
if(!$RuntimeDirectory){$RuntimeDirectory=Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) '04_COMMUNICATION\remote-bridge\releases\20261006-v3'}
$RuntimeDirectory=[IO.Path]::GetFullPath($RuntimeDirectory)
$tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixture=Join-Path $tempRoot ('ProjectBridgeUpgradeTest_'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture|Out-Null
try{
 $source=[IO.File]::ReadAllText((Join-Path $RuntimeDirectory 'install.ps1'),[Text.Encoding]::UTF8).TrimStart([char]0xFEFF)
 $tokens=$null;$errors=$null
 $ast=[System.Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
 if(@($errors).Count){throw 'Installer parse errors'}
 # Only root and startup are replaced; all actual staging, hashes, compilation,
 # release tests, file writes and backup paths use the production installer.
 $fn=$ast.Find({param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -eq 'Start-InstalledBridge'},$true)
 $source=$source.Remove($fn.Extent.StartOffset,$fn.Extent.EndOffset-$fn.Extent.StartOffset).Insert($fn.Extent.StartOffset,'function Start-InstalledBridge([bool]$Registered){Write-OwnedUtf8 (Join-Path $target "fixture-started.txt") "requested"}')
 $source=$source.Replace('$target=''D:\A_KJ\AI\Applications\ProjectBridge''',('$target='''+$fixture.Replace("'","''")+''''))
 if($source.Contains('$target=''D:\A_KJ\AI\Applications\ProjectBridge''')){throw 'Root substitution failed'}
 $cfgPath=Join-Path $fixture 'config.json'
 $python='D:\AI\envs\cosyvoice\python.exe'
 if(!(Test-Path -LiteralPath $python)){$python=(Get-Command python.exe).Source}
 $initial=@{deviceId='upgrade-fixture';python=$python;protectedRoots='C:\Users\user\Desktop';audioEnabled=$false;unknownSetting='preserve-me'}
 [IO.File]::WriteAllText($cfgPath,($initial|ConvertTo-Json),(New-Object Text.UTF8Encoding($false)))
 $before=Get-Item -LiteralPath $cfgPath -Force
 $before.Attributes=$before.Attributes -bor [IO.FileAttributes]::Hidden
 $originalFailed=$false
 try{[IO.File]::WriteAllText($cfgPath,'invalid')}catch [UnauthorizedAccessException]{$originalFailed=$true}
 if(!$originalFailed){throw 'Original hidden-file failure did not reproduce'}
 $first=[scriptblock]::Create($source)
 & $first -SourceDirectory $RuntimeDirectory
 if(!(Test-Path -LiteralPath (Join-Path $fixture 'fixture-started.txt'))){throw 'First install did not reach startup'}
 & $first -SourceDirectory $RuntimeDirectory
 $cfg=Get-Content -LiteralPath $cfgPath -Raw -Encoding UTF8|ConvertFrom-Json
 if($cfg.unknownSetting -ne 'preserve-me' -or $cfg.deviceId -ne 'upgrade-fixture'){throw 'Existing config changed unrelated data'}
 if($cfg.protectedRoots -isnot [Array] -or $cfg.protectedRoots.Count -ne 1){throw 'Scalar migration did not produce one-item array'}
 if(((Get-Item -LiteralPath $cfgPath -Force).Attributes -band [IO.FileAttributes]::Hidden) -eq 0){throw 'Upgrade lost Hidden'}
 Write-Output 'PASS: actual install and repeated upgrade of Hidden config; one-item array, device ID and unknown settings preserved.'
 # Read-only must fail before stop flag changes or any installed file writes.
 $configInfo=Get-Item -LiteralPath $cfgPath -Force
 $configInfo.Attributes=$configInfo.Attributes -bor [IO.FileAttributes]::ReadOnly
 $oldHash=(Get-FileHash -LiteralPath $cfgPath -Algorithm SHA256).Hash
 $workerFile=Join-Path $fixture 'Runtime\universal_actions.py'
 $oldWorkerHash=(Get-FileHash -LiteralPath $workerFile -Algorithm SHA256).Hash
 $flags=Join-Path $fixture 'state\stop.flag'
 [IO.File]::WriteAllText($flags,'prior user pause')
 $blocked=$false
 try{& $first -SourceDirectory $RuntimeDirectory}catch{if($_.Exception.Message -notmatch 'read-only'){throw};$blocked=$true}
 if(!$blocked){throw 'Read-only upgrade unexpectedly succeeded'}
 if([IO.File]::ReadAllText($flags) -ne 'prior user pause' -or (Get-FileHash -LiteralPath $cfgPath -Algorithm SHA256).Hash -ne $oldHash -or (Get-FileHash -LiteralPath $workerFile -Algorithm SHA256).Hash -ne $oldWorkerHash){throw 'Read-only preflight changed installation'}
 if(((Get-Item -LiteralPath $cfgPath -Force).Attributes -band [IO.FileAttributes]::ReadOnly) -eq 0){throw 'Read-only attribute changed'}
 Write-Output 'PASS: read-only destination rejected before stopping or modifying installation; no ACL or attribute overrides.'
 # Same production writer must truncate old contents when new UTF8 is shorter.
 $writer=$ast.Find({param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -eq 'Write-OwnedUtf8'},$true)
 $check=$ast.Find({param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -eq 'Assert-OwnedFileWritable'},$true)
 . ([scriptblock]::Create($check.Extent.Text+"`n"+$writer.Extent.Text))
 $short=Join-Path $fixture 'shorter.json'
 [IO.File]::WriteAllText($short,'previous much longer contents')
 (Get-Item -LiteralPath $short -Force).Attributes=[IO.FileAttributes]::Hidden
 Write-OwnedUtf8 $short 'ok'
 if([IO.File]::ReadAllText($short) -ne 'ok'){throw 'Writer did not truncate'}
 Write-Output 'PASS: actual production writer truncates shorter replacement and preserves Hidden.'
}finally{
 $resolved=[IO.Path]::GetFullPath($fixture)
 if($resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path -Leaf $resolved) -like 'ProjectBridgeUpgradeTest_*'){
  Remove-Item -LiteralPath $resolved -Recurse -Force
 }
}
