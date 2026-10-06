$ErrorActionPreference='Stop'
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$taskSetupPath=Join-Path ([IO.Path]::GetTempPath()) ('ProjectBridge_setup_'+[Guid]::NewGuid().ToString('N')+'.ps1')
try {
 $url='https://raw.githubusercontent.com/kimjae134679/project-operations-hub/e057a73092f152637f7406c74a7f5c2855e76c63/04_COMMUNICATION/remote-bridge/releases/20261006-v2/install.ps1'
 Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $taskSetupPath
 if((Get-FileHash -LiteralPath $taskSetupPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne '34cbd7c62a1fe09d673704dc5a83a8badd7138cb1a31e9350a80562dcdc12e0e'){throw 'ProjectBridge installer hash mismatch'}
 $taskInstaller=[ScriptBlock]::Create([IO.File]::ReadAllText($taskSetupPath,[Text.Encoding]::UTF8))
 & $taskInstaller -Commit 'e057a73092f152637f7406c74a7f5c2855e76c63' -ManifestSha256 'f7c40fa0a5a6eb3f80f7a44d0bdbec754b6294981acd44707033e97bea03eb5c' -ApprovedBundleSha256 '97ae1490c6d483e97eb511ddcf7f088ad24db3b5a954fe85c8b449f5324dfd35' -StartAtLogin
} finally {Remove-Item -LiteralPath $taskSetupPath -Force -ErrorAction SilentlyContinue}
