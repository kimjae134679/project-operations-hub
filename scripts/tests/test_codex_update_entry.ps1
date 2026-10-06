$ErrorActionPreference='Stop'
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$installerPath=Join-Path $repoRoot '04_COMMUNICATION\remote-bridge\releases\20261006-v3\install.ps1'
$updatePath=Join-Path $repoRoot 'scripts\update-codex.ps1'
$payload=[IO.File]::ReadAllText($installerPath,[Text.Encoding]::UTF8)
$header=$payload.Substring(0,$payload.IndexOf("`n"+'$ErrorActionPreference='))
$probe=([string][char]0xFEFF)+$header+"`n"+'@{commit=$Commit;manifest=$ManifestSha256;login=[bool]$StartAtLogin}|ConvertTo-Json -Compress'
$rawPath=Join-Path ([IO.Path]::GetTempPath()) ('codex-bom-entry-'+[guid]::NewGuid().ToString('N')+'.ps1')
[IO.File]::WriteAllText($rawPath,$probe,(New-Object Text.UTF8Encoding($false)))
$broken=$false
try { & ([scriptblock]::Create($probe)) -Commit ('a'*40) -ManifestSha256 ('b'*64) -StartAtLogin } catch { $broken=$true;Write-Output ('REPRODUCED: '+$_.Exception.Message) }
if(!$broken){throw 'Original BOM path did not reproduce failure'}
$installer=[IO.File]::ReadAllText($rawPath,[Text.Encoding]::UTF8).TrimStart([char]0xFEFF)
$tokens=$null;$parseErrors=$null
$ast=[System.Management.Automation.Language.Parser]::ParseInput($installer,[ref]$tokens,[ref]$parseErrors)
if(@($parseErrors).Count -gt 0 -or !$ast.ParamBlock){throw 'Fixed downloaded entry missing parameter block'}
$result=& ([scriptblock]::Create($installer)) -Commit ('a'*40) -ManifestSha256 ('b'*64) -StartAtLogin
$value=$result|ConvertFrom-Json
if($value.commit -ne ('a'*40) -or $value.manifest -ne ('b'*64) -or !$value.login){throw 'Fixed invocation lost installer arguments'}
[System.Management.Automation.Language.Parser]::ParseFile($updatePath,[ref]$tokens,[ref]$parseErrors)|Out-Null
if(@($parseErrors).Count){throw 'Updated helper parse failure'}
$helper=[IO.File]::ReadAllText($updatePath)
if($helper -notmatch 'updateSkipped' -or $helper -notmatch 'already_current'){throw 'Already-updated retry would reinstall npm'}
Remove-Item -LiteralPath $rawPath
Write-Output 'PASS: Windows PowerShell reproduced original failure; fixed UTF-8 BOM entry preserved commit, manifest and login arguments; helper parsed; retry skips npm.'
