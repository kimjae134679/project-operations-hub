# AI Control Tower

Windows에서 Desktop Commander Remote, Jev, Codex, GitHub CLI, n8n, AI Ops Runner와 전달 체인의 상태를 실제 확인 근거로 표시하는 .NET 9 WPF 관제탑입니다.

## 실행 파일

Release 단일 EXE: `artifacts/win-x64/AIControlTower.exe`

```powershell
.\AIControlTower.exe
```

## 주요 기능

- 5초 간격 상태 자동 갱신 및 수동 갱신
- Desktop Commander, Jev, Codex, GitHub CLI, n8n, AI Ops Runner 상태 표시
- 외부 GPT 세션처럼 로컬에서 증명할 수 없는 구간은 `Unknown`으로 표시
- 현재 사용자에게 보이는 Jev 콘솔에서 작업 실행·관제탑이 시작한 작업만 취소
- 비밀번호·API 키·토큰 값 비표시
- 설치, 제거, Desktop Commander 시작 구성 복구

## 설치와 복구

앱의 **설치** 버튼은 실행 중인 EXE를 우선 `C:\Program Files\_My\AI\Applications\AIControlTower`에 복사하고, 권한 오류일 때 `%LocalAppData%\AIControlTower`를 사용합니다. 현재 사용자 시작 프로그램에 관제탑을 등록합니다.

기존 `DesktopCommanderRemote.cmd`가 존재하면 설치 전 해당 파일을 앱 설치 폴더의 `backups\desktop-commander`에 백업합니다. 이후 기존 표시형 시작 파일은 비활성화하고 숨김 VBS 시작 런처로 교체합니다. **Desktop Commander 복구**는 이 백업이 있을 때만 기존 시작 파일을 되돌립니다.

제거는 관제탑 자동 시작과 숨김 런처를 제거합니다. 설치된 EXE 자체에서 제거를 누른 경우에는 실행 중인 파일을 즉시 삭제할 수 없으므로 앱을 종료한 뒤 설치 폴더를 지웁니다.

## 알려진 한계

- GPT 외부 세션에서 Desktop Commander/Jev로 실제 메시지가 전달됐는지는 로컬 PC만으로 증명할 수 없어 `Unknown`으로 표시됩니다.
- Jev 래퍼가 PowerShell 실행 정책에 의해 막히면 앱은 `Ready`로 표시하지 않고 오류로 보고합니다.
- 각 도구의 CLI 로그인 상태는 현재 PC의 비대화형 명령 결과에 의존합니다.

## 개발 검증

```powershell
dotnet test tests\AIControlTower.Tests\AIControlTower.Tests.csproj -c Release
dotnet build AIControlTower.sln -c Release
dotnet publish src\AIControlTower\AIControlTower.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts\win-x64
```

## GPT 작업 큐

%LocalAppData%\AIControlTower\queue\inbox에 작업당 하나의 .txt 파일을 만들면 관제탑이 5초 이내에 processing으로 원자적으로 이동한 뒤 Jev 작업으로 전달합니다. 완료 이력은 rchive, 결과는 esult에 남기도록 확장할 수 있습니다. 단일 파일을 덮어쓰거나 즉시 삭제하지 않아 중복 실행과 이력 유실을 막습니다.

