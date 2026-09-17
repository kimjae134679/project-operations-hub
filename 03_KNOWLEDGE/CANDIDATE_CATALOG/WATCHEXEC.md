# watchexec

검증일: 2026-09-17
공식 저장소: `watchexec/watchexec`

## 판정

| 축 | 판정 |
|---|---|
| ACCESS | INSTALL |
| 설치 없이 ChatGPT 연결 | 아니오 |
| AI 설치·설정 | 가능 |
| AUTONOMY | FULL |
| 사용자 도움 | NONE (일반 설치/사용 기준) |
| GUI 필요 | 없음 |
| OPERATOR | AI 중심 / BOTH 가능 |
| INSTALLED | UNKNOWN |
| ADOPTION | CANDIDATE |
| BURDEN | 🟢 LIGHT |
| COST | FREE_OSS |
| Cloud / self-host | 해당 없음, 로컬 CLI |
| 라이선스 | Apache-2.0 |

## 무엇을 하는가

파일 변경을 감지하면 지정한 명령을 자동 실행하는 범용 CLI다. 언어나 프레임워크에 묶이지 않아 소스 변경 후 test/lint/build/restart를 자동으로 반복할 수 있다.

공식 문서 기준 Windows/macOS/Linux를 지원하며 `.gitignore`/`.ignore`를 기본 활용하고, 여러 파일 이벤트를 묶어 처리하며, 실행 중인 프로세스 재시작과 변경 이벤트 JSON 출력도 지원한다. Windows에서는 PowerShell shell을 지정할 수 있고 prebuilt binary, Scoop, Chocolatey 등의 설치 경로가 있다.

## 우리 작업에서 가치

- 코드 수정 → 관련 테스트 자동 재실행
- 웹/서버 수정 → 개발 프로세스 자동 재시작
- 설정/문서 변경 → lint/build 자동 확인
- `just`와 조합해 `watchexec -- just test`처럼 프로젝트별 실제 명령을 한 곳에 유지 가능
- AI가 여러 파일을 연속 수정할 때는 너무 자주 빌드하지 않도록 debounce/필터를 조정하거나, 변경 묶음이 끝난 뒤 명시적으로 한 번 실행하는 방식과 비교한다.

## 주의

- 감시 대상과 실행 명령은 임의 코드를 실행할 수 있으므로 외부 저장소의 기존 script/justfile/package script를 먼저 확인한다.
- 생성물 폴더를 다시 감시하면 불필요한 반복 실행이나 루프가 생길 수 있어 `target`, `dist`, build output 등은 ignore를 확인한다.
- WSL에서 Windows 파일시스템을 교차 감시하는 특수 환경은 native Windows/Linux 감시와 동일하다고 가정하지 않는다.
- 단순히 한 번 build/test하면 되는 작업에는 도입하지 않는다. 반복 edit-test/restart가 실제 병목일 때 사용한다.

## 비용 정책 적합성

도구 자체는 Apache-2.0 무료 오픈소스이며 별도 계정·카드·무료체험·클라우드 요금제가 필요하지 않는다. 단, 실행하도록 지정한 명령이 외부 유료 API/클라우드를 호출한다면 그 실행비용은 `watchexec` 비용과 별도로 판단한다.
