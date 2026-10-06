$ErrorActionPreference='Stop'
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$commit='de2727f9740ae86ff022982d9ea257012193ba6d'
$url='https://raw.githubusercontent.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/'+$commit+'/bridge/install.ps1'
$setup=Join-Path ([IO.Path]::GetTempPath()) ('ProjectBridgeSetup_'+[Guid]::NewGuid().ToString('N')+'.ps1')
try {
 Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $setup
 if((Get-FileHash -LiteralPath $setup -Algorithm SHA256).Hash.ToLowerInvariant() -ne '6e0f020ec156bc5ca76d94da516968982483819fe393978c5db4728d70ba4cf3'){throw 'Installer hash mismatch'}
 & ([scriptblock]::Create([IO.File]::ReadAllText($setup,[Text.Encoding]::UTF8))) -Commit $commit -ManifestSha256 '799c9b9fbd0e5f5fd74d8ad886a6be09cfcab9f9477af1727affdfba7cecfc75' -ApprovedBundleSha256 '97ae1490c6d483e97eb511ddcf7f088ad24db3b5a954fe85c8b449f5324dfd35' -StartAtLogin
} finally {if(Test-Path -LiteralPath $setup){Remove-Item -LiteralPath $setup -Force}}
