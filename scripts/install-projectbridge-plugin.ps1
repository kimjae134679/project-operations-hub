param(
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{40}$')][string]$Commit,
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{64}$')][string]$ManifestSha256
)
$ErrorActionPreference='Stop'
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$base='https://raw.githubusercontent.com/kimjae134679/project-operations-hub/'+$Commit+'/'
$bridge='D:\A_KJ\AI\Applications\ProjectBridge'
$package=Join-Path $bridge 'PluginMarketplace'
$installer=Join-Path ([IO.Path]::GetTempPath()) ('ProjectBridgeInstall_'+[guid]::NewGuid().ToString('N')+'.ps1')
Invoke-WebRequest -UseBasicParsing -Uri ($base+'04_COMMUNICATION/remote-bridge/releases/20261006-v3/install.ps1') -OutFile $installer
$source=[IO.File]::ReadAllText($installer,[Text.Encoding]::UTF8).TrimStart([char]0xFEFF)
$tokens=$null;$errors=$null;$ast=[System.Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
if(@($errors).Count -or !$ast.ParamBlock){throw 'Pinned installer parse failure'}
& ([scriptblock]::Create($source)) -Commit $Commit -ManifestSha256 $ManifestSha256 -StartAtLogin
Remove-Item -LiteralPath $installer
# Fetch only the declarative plugin configuration, never an arbitrary executable.
foreach($relative in @('.codex-plugin/marketplace.json','plugins/projectbridge-local/.codex-plugin/plugin.json','plugins/projectbridge-local/.mcp.json')){
 $destination=Join-Path $package $relative
 New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force|Out-Null
 Invoke-WebRequest -UseBasicParsing -Uri ($base+$relative) -OutFile $destination
 $null=Get-Content -LiteralPath $destination -Raw -Encoding UTF8|ConvertFrom-Json
}
$config=Get-Content -LiteralPath (Join-Path $bridge 'config.json') -Raw -Encoding UTF8|ConvertFrom-Json
$mcpPath=Join-Path $package 'plugins/projectbridge-local/.mcp.json'
$mcp=Get-Content -LiteralPath $mcpPath -Raw -Encoding UTF8|ConvertFrom-Json
$mcp.mcpServers.projectbridge.command=$config.python
[IO.File]::WriteAllText($mcpPath,($mcp|ConvertTo-Json -Depth 10),(New-Object Text.UTF8Encoding($false)))
$deadline=[DateTime]::UtcNow.AddSeconds(45);$status=$null
do{
 try{
  $endpoint=Get-Content -LiteralPath (Join-Path $bridge 'state/local_endpoint.json') -Raw -Encoding UTF8|ConvertFrom-Json
  $headers=@{'X-ProjectBridge-Token'=$endpoint.token}
  $status=Invoke-RestMethod -Uri ($endpoint.baseUrl+'/v1/status') -Headers $headers -TimeoutSec 5
  if($status.localReady){break}
 }catch{}
 Start-Sleep -Milliseconds 250
}while([DateTime]::UtcNow -lt $deadline)
if(!$status.localReady){throw 'Updated bridge is not ready; plugin was prepared but not registered'}
$codex=Get-Command codex.exe -ErrorAction SilentlyContinue
if(!$codex){$codex=Get-Command codex -ErrorAction Stop}
& $codex.Source plugin marketplace add $package --json
if($LASTEXITCODE -ne 0){throw 'Marketplace registration failed; repaired bridge remains installed'}
& $codex.Source plugin add 'projectbridge-local@projectbridge-personal' --json
if($LASTEXITCODE -ne 0){throw 'Plugin installation failed; repaired bridge remains installed'}
$report=@{installedAt=[DateTimeOffset]::UtcNow.ToString('o');commit=$Commit;bridgeReady=[bool]$status.localReady;pluginCommandSucceeded=$true;hostPermissionsChanged=$false;newSessionRequired=$true}
$report|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $package 'install-result.json') -Encoding UTF8
Write-Output 'Bridge repaired and local plugin installation command completed. Start a new session and verify pc_status then an actual command.'
