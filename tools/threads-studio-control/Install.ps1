param(
  [string]$ProjectRoot='D:\A_KJ\AI\Projects\Threads',
  [string]$InstallRoot='D:\A_KJ\AI\Scripts\ThreadsUploadStudio',
  [string]$CatalogPath=(Join-Path $PSScriptRoot '../../project.catalog.json')
)
$ErrorActionPreference='Stop'
[Console]::OutputEncoding=[System.Text.UTF8Encoding]::new()
$manifestPath=Join-Path $ProjectRoot 'project.control.json'
$prior=if(Test-Path -LiteralPath $manifestPath){[IO.File]::ReadAllText($manifestPath)}else{$null}
$recovery=Join-Path $InstallRoot ('recovery/'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ'))
[IO.Directory]::CreateDirectory($recovery) | Out-Null
if($null -ne $prior){[IO.File]::WriteAllText((Join-Path $recovery 'project.control.before.json'),$prior)}
foreach($file in @('Control.ps1','Lifecycle.psm1','ApplyWhenIdle.ps1')) {
  $target=Join-Path $InstallRoot $file
  if(Test-Path -LiteralPath $target){Copy-Item -LiteralPath $target -Destination (Join-Path $recovery $file)}
  Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $target -Force
}
if($null -ne $prior){$manifest=$prior | ConvertFrom-Json}else{
  $entry=((Get-Content -LiteralPath $CatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json).projects | Where-Object id -eq 'Threads')
  if(!$entry){throw 'Threads catalog registration missing'}
  $functions=@(foreach($f in $entry.functions){
    @{id=$f.id;name=$f.name;programs=@(foreach($p in $f.programs){
      # Commands are already absolute installed entries; keep the producer/editor entry points.
      $entryPath=if($p.path -and (Test-Path -LiteralPath (Join-Path $ProjectRoot $p.path))){$p.path}else{'.'}
      @{id=$p.id;name=$p.name;kind=$p.kindLabel;path=$entryPath;workingDirectory='.';description=$p.description;commands=@($p.commands | Where-Object {$null -ne $_})}
    })}
  })
  $manifest=[pscustomobject]@{schemaVersion=1;id='Threads';name='인스타 수익화 · 콘텐츠 제작';description='새 제작물 검토 앱을 켜고 끄거나 열며, 기존 제작·편집 도구와 자료를 찾습니다.';functions=$functions}
}
if($manifest.id -ne 'Threads' -or $manifest.schemaVersion -ne 1){throw 'Unexpected project manifest'}
$controller=Join-Path $InstallRoot 'Control.ps1'
function Command($label,$action){
  @{name=$label;fileName='powershell.exe';arguments=@('-NoProfile','-ExecutionPolicy','Bypass','-File',$controller,'-Action',$action);timeoutSeconds=90}
}
$programs=@(
  @{id='upload-studio-start';name='검토 앱 켜기';kind='검토 앱 관리';path='.';workingDirectory='.';description='검토 서버를 켭니다. 이미 켜져 있으면 같은 서버를 유지합니다. 새 제작물 자동 연결도 함께 작동합니다.';commands=@((Command '검토 앱 켜기' 'start'))},
  @{id='upload-studio-open';name='검토 화면 열기';kind='검토 앱 관리';path='.';workingDirectory='.';description='켜져 있는 검토 앱을 기본 브라우저에서 엽니다. 꺼져 있다면 켜기를 먼저 실행하세요.';commands=@((Command '검토 화면 열기' 'open'))},
  @{id='upload-studio-stop';name='검토 앱 끄기';kind='검토 앱 관리';path='.';workingDirectory='.';description='이 검토 서버와 새 제작물 자동 연결을 끕니다. 이미지·판정·게시 기록은 보존하며 다른 제작 프로그램과 원격 연결은 계속 실행합니다.';commands=@((Command '검토 앱 끄기' 'stop'))},
  @{id='upload-studio-status';name='검토 앱 실행 상태';kind='검토 앱 상태';path='.';workingDirectory='.';description='서버 실행 주체와 실제 응답 상태를 확인합니다. 선택 중에는 상태가 자동 갱신됩니다.';commands=@();control=@{status=(Command '검토 앱 상태 확인' 'status');fields=@(
    @{label='검토 앱';path='stateLabel'},
    @{label='서버 응답';path='healthy';trueLabel='정상';falseLabel='응답 없음'},
    @{label='실행 주체';path='foreign';trueLabel='확인 필요';falseLabel='확인됨'}
  );options=@()}}
)
$function=[pscustomobject]@{id='upload-studio-controls';name='새 제작물 검토 앱 · 켜기 / 열기 / 끄기';programs=$programs}
$manifest.functions=@($function)+@($manifest.functions | Where-Object id -ne 'upload-studio-controls')
$text=$manifest | ConvertTo-Json -Depth 30
$now=if(Test-Path -LiteralPath $manifestPath){[IO.File]::ReadAllText($manifestPath)}else{$null}
if($now -cne $prior){throw 'Manifest changed during installation; retry after rereading'}
$temporary=$manifestPath+'.studio.tmp'
[IO.File]::WriteAllText($temporary,$text,[Text.UTF8Encoding]::new($false))
Move-Item -LiteralPath $temporary -Destination $manifestPath -Force
[Console]::Out.WriteLine((@{installed=$true;projectManifest=$manifestPath;controller=$controller;recovery=$recovery;programs=$programs.Count;otherFunctionsPreserved=$true} | ConvertTo-Json -Compress))
