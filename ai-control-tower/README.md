# AI Control Tower

Windows에서 Desktop Commander Remote, Jev, Codex, GitHub CLI, n8n, AI Ops Runner와 전달 체인의 상태를 실제 확인 근거로 표시하는 .NET 9 WPF 관제탑입니다.

## 현재 배포 상태

최신 소스에는 좌측 탐색 메뉴, 한 개의 전체 세로 스크롤 영역, 별도 Jev 작업 제어 창이 포함되어 있습니다. 2026-09-20 최종 단일 EXE publish와 `%LocalAppData%\AIControlTower\AIControlTower.exe` 설치본 교체를 완료했고, 두 파일의 SHA-256은 동일합니다: `89A4B961CA2F92EFBE53B1B872E18F843BB6478BDB418071BDA2A4B7DBCD4C3B`. 설치본 실행 프로세스 PID `29428`도 확인했습니다.

배포 대상 경로:

- 빌드 출력: `artifacts/win-x64/AIControlTower.exe`
- 권한 제한 시 설치 대상: `%LocalAppData%\AIControlTower\AIControlTower.exe`

최종 publish 뒤에는 다음 명령으로 아티팩트와 설치본의 SHA-256이 같은지 확인합니다.

```powershell
Get-FileHash artifacts\win-x64\AIControlTower.exe -Algorithm SHA256
Get-FileHash "$env:LocalAppData\AIControlTower\AIControlTower.exe" -Algorithm SHA256
```

```powershell
.\AIControlTower.exe
```

## 주요 기능

- 5초 간격 상태 자동 갱신 및 수동 갱신
- Desktop Commander, Jev, Codex, GitHub CLI, n8n, AI Ops Runner 상태 표시
- 외부 GPT 세션처럼 로컬에서 증명할 수 없는 구간은 `Unknown`으로 표시
- AA_01·AA_02 참고의 화이트·블루 대시보드, 상태 배지, 다크 실시간 작업 콘솔
- 고정 좌측 메뉴와 하나의 전체 세로 스크롤로 대시보드·도구 상태·설치/복구 구역 이동
- Jev 작업 제어는 대시보드에서 분리된 단일 별도 창으로 열림
- 로컬 Jev 실행은 사용자 OpenAI 계정 사용 방침에 따라 기본적으로 차단
- 비밀번호·API 키·토큰 값 비표시
- 설치, 제거, Desktop Commander 시작 구성 복구

## 설치와 복구

앱의 **설치** 버튼은 실행 중인 EXE를 우선 `C:\Program Files\_My\AI\Applications\AIControlTower`에 복사하고, 권한 오류일 때 `%LocalAppData%\AIControlTower`를 사용합니다. 현재 사용자 시작 프로그램에 관제탑을 등록합니다.

기존 `DesktopCommanderRemote.cmd`가 존재하면 설치 전 해당 파일을 앱 설치 폴더의 `backups\desktop-commander`에 백업합니다. 이후 기존 표시형 시작 파일은 비활성화하고 숨김 VBS 시작 런처로 교체합니다. **Desktop Commander 복구**는 이 백업이 있을 때만 기존 시작 파일을 되돌립니다.

제거는 관제탑 자동 시작과 숨김 런처를 제거합니다. 설치된 EXE 자체에서 제거를 누른 경우에는 실행 중인 파일을 즉시 삭제할 수 없으므로 앱을 종료한 뒤 설치 폴더를 지웁니다.

## 알려진 한계

- GPT 외부 세션에서 Desktop Commander/Jev로 실제 메시지가 전달됐는지는 로컬 PC만으로 증명할 수 없어 `Unknown`으로 표시됩니다.
- Jev는 로컬 OpenAI/ChatGPT 계정 사용을 막기 위해 명령·환경 확인과 작업 실행을 하지 않고 비활성 상태로 표시합니다.
- 각 도구의 CLI 로그인 상태는 현재 PC의 비대화형 명령 결과에 의존합니다.

## 개발 검증

```powershell
dotnet test tests\AIControlTower.Tests\AIControlTower.Tests.csproj -c Release
dotnet build AIControlTower.sln -c Release
dotnet publish src\AIControlTower\AIControlTower.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts\win-x64
```

탐색 화면 변경 기준으로 `dotnet test tests\AIControlTower.Tests\AIControlTower.Tests.csproj -c Release`는 11개 통과, `dotnet build AIControlTower.sln -c Release`는 경고·오류 0개를 기록했고 최종 publish도 성공했습니다. artifact와 설치본의 SHA-256 일치는 위 배포 상태에 기록했습니다. CUA에는 native app surface가 없어 자동 화면 캡처는 하지 못했으므로, 다음 화면 모양은 사용자가 직접 확인해야 합니다.

- 좌측 메뉴가 보이고 모든 주 구역이 하나의 세로 스크롤에서 접근되는지
- Jev 작업 제어가 별도 창으로 열리고 실행 버튼이 정책 설명과 함께 비활성인지
- GPT 스케줄러·예약·브라우저·권한 자동 클릭 UI가 없는지

## GPT 작업 큐

`%LocalAppData%\AIControlTower\queue\inbox`에 작업당 하나의 `.txt` 파일을 만들 수 있습니다. 단, 현재 로컬 Jev 실행 정책이 비활성화되어 있으므로 관제탑은 파일을 `processing`으로 이동하거나 실행하지 않습니다. 정책을 별도로 변경할 때에만 원자 이동·완료 이력(`archive`)·결과(`result`) 흐름을 사용합니다.


## 실제 설치 검증 기록

2026-09-20에 %LocalAppData%\AIControlTower 설치, 현재 사용자 자동 시작, AIControlTower-DesktopCommanderSilent.vbs 생성 및 기존 Desktop Commander 시작 CMD 백업을 확인했습니다. 설치를 다시 실행하면 원본 CMD가 이미 없으므로 전환 대상이 없다는 안내가 나올 수 있습니다. 최신 탐색 화면 단일 EXE로 설치본을 교체하고 SHA-256 일치 및 PID `29428` 실행까지 확인했으며, 화면 모양은 사용자가 직접 확인해야 합니다.

