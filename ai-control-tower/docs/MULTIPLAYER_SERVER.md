# 멀티의 신 서버 연결·관측 근거

확인일: **2026-10-05 KST**, PC KJW. 기존 게임·서버 코드, 실행 프로세스, DB, 터널 설정은 변경하지 않았습니다. 관리창이나 시작·종료·재시작 버튼도 실행하지 않았습니다. 아래 값은 조사 시점의 관측이며 현재 상태는 다시 조회해야 합니다.

## 개발·배포·실행 위치

| 대상 | 위치 | 역할 |
|---|---|---|
| 현재 개발 원본 | [PhoneLoL_02-Source](https://github.com/kimjae134679/PhoneLoL_02-Source) | 게임 소스·서버 변경 소스·현재 인수인계 |
| 공개 배포 | [PhoneLoL_02](https://github.com/kimjae134679/PhoneLoL_02) | 사용자 설치파일과 확인 완료본 |
| 과거 이력 | [PhoneLOL](https://github.com/kimjae134679/PhoneLOL) | 이전 개발·복구 이력 |
| 실제 게임 작업 | `D:\A_KJ\AI\PhoneLoL_02` | Unity 프로젝트는 하위 `PhoneLOL-02` |
| 실제 서버 운영 | `C:\Users\user\Documents\MultiGod\PhoneLOL_LocalRuntime` | 서버·기존 제어 실행기·운영 자료 |
| 실행 서버 코드 | 서버 루트의 `recovery\01_server_v0965_snapshot\APK` | 이름이 APK여도 휴대폰 설치파일이 아닌 Python 서버 코드 |

실제 PC에서 Git 최상위 경로가 `D:\A_KJ\AI\PhoneLoL_02`, origin 저장소가 `kimjae134679/PhoneLoL_02-Source`임을 확인했습니다. 원격 주소에 포함될 수 있는 인증 정보는 출력하거나 기록하지 않았습니다.

확인 당시 개발 README의 추가 작업은 **0.23.6 / build 256**, 공개 배포 README의 사용자 확인 완료본은 **0.23.4 / build 254**였습니다. 서로 다른 의미입니다. 과거 0.23.5 테스트 결과도 현재 작업 완료를 대신하지 않습니다. 현재 버전은 해당 저장소에서 다시 확인합니다.

개발 원본의 [README](https://github.com/kimjae134679/PhoneLoL_02-Source/blob/main/README.md), [HANDOFF](https://github.com/kimjae134679/PhoneLoL_02-Source/blob/main/HANDOFF.md), [Automation/Server/README](https://github.com/kimjae134679/PhoneLoL_02-Source/blob/main/Automation/Server/README.md)를 기준으로 합니다. 서버 변경 소스는 기존 모듈·실행기·환경·DB·터널을 전제로 합니다. 옛 저장소의 APK 폴더를 현재 런타임에 통째로 복사하지 않습니다. 서버 코드 복구와 계정 DB 복구는 별개이며 이번 조사에서는 둘 다 하지 않았습니다. 클라우드 이전은 확인한 서버 안내에서 보류 중이므로 현재 운영이 클라우드로 이전됐다고 표시하지 않습니다.

## 실제 PC 관측

| 확인 시각 KST | 확인한 내용 | 의미 |
|---|---|---|
| 18:23:31 | `0.0.0.0:29000` 수신 대기, 소유 PID `18088` | 이 포트를 실제 점유한 프로세스 확인 |
| 18:23:31 | `C:\TempPy13\python.exe`, 서버 스크립트 `server_central_authority_v33.py`, 알려진 런타임 경로 일치 | 단순 Python 실행 여부보다 구체적인 서버 식별 근거 |
| 18:25:46 | 로컬 온라인 API HTTP 200; 온라인·준비·플레이 각각 0 | 해당 순간의 정상 응답과 집계. 이후 인원이 없다는 뜻이 아님 |
| 18:25:46 | 프로세스 시작 14:17:05 KST; Working Set 29,290,496 bytes, Private Memory 33,333,248 bytes | 한 번 측정한 메모리. 평균·최대·장기 사용량이 아님 |
| 18:25:46 | 터널 실행파일 `tools\portwarp\pwrp.exe`, PID `26904` | 기존 터널 프로세스 존재. 외부 게임 플레이 완료를 뜻하지 않음 |
| 18:27:07 | 로컬 정책 revision 9, 업데이트 안내 build 241 / version 0.22.1, 최소 build 0, 차단 build 목록 비어 있음 | 정책 안내 값. 현재 게임 개발·공개 APK 버전과 다름 |

외부 정책 주소 `http://uko9ef6n.free.pwrp.cc:10045/phonelol-policy/v1`도 조사 중 HTTP 200을 확인했습니다. 이는 그때의 외부 HTTP 접근 관측이며 영구 주소·터널 안정성·게임 소켓 정상 플레이를 보장하지 않습니다. 관리 앱의 일상 조회는 같은 PC의 로컬 주소를 사용합니다. 정책의 오래된 업데이트 안내를 이번 작업에서 임의 수정하지 않았습니다.

## 화면에 보여줄 집계

읽기 전용 조회:

- 접속 집계: `GET http://127.0.0.1:29000/phonelol-online/v1`
- 운영 정책: `GET http://127.0.0.1:29000/phonelol-policy/v1`

온라인 응답의 실제 구조는 다음과 같습니다. 예시 숫자는 18:25:46 KST 관측값입니다.

```json
{
  "schemaVersion": 1,
  "observedAt": "2026-10-05T09:25:46+00:00",
  "total": {"online": 0, "preparing": 0, "playing": 0},
  "modes": {
    "ranked": {"online": 0, "preparing": 0, "playing": 0},
    "ranked1v1": {"online": 0, "preparing": 0, "playing": 0},
    "modeBattle": {"online": 0, "preparing": 0, "playing": 0},
    "mode": {"online": 0, "preparing": 0, "playing": 0},
    "urf": {"online": 0, "preparing": 0, "playing": 0},
    "urf5": {"online": 0, "preparing": 0, "playing": 0}
  }
}
```

`online`은 전체 접속 집계이고 `preparing`과 `playing`은 그중 준비·플레이 상태의 부분집합입니다. 세 값을 합쳐 총인원으로 표시하지 않습니다. 집계 코드는 살아 있는 인증 계정을 기준으로 중복 계정의 가장 진행된 상태를 사용합니다. 응답 시각도 표시하고, 조회 실패·필드 누락·오래된 값은 0명으로 바꾸지 않습니다. 원본 영어 키는 내부 연결에 사용하고 일반 화면에는 접속·준비·플레이 등 사람용 이름을 사용합니다.

실제 게임의 `Assets/Scripts/Compatibility/PhoneLOLLobbyNotice.cs`와 `PhoneLOLLobbyRosterCore.cs`에서 모드 이름도 확인했습니다.

| 내부 키 | 게임에서 쓰는 이름 |
|---|---|
| `ranked` | 랭크대전 |
| `ranked1v1` | 1vs1대전 |
| `modeBattle` | 모드대전 |
| `mode` | 모드 |
| `urf` | URF |
| `urf5` | URF5 |

정책에서 revision은 최상위 `revision`, 업데이트 안내는 `update.latestBuild`·`update.latestVersion`, 접근 제한은 `access.minimumBuild`·`access.blockedBuilds`입니다. 운영 정책의 업데이트 안내와 최신 APK 버전을 섞지 않습니다.

인증이 필요한 `/phonelol-roster/v1`의 계정·닉네임 목록을 일반 관리 집계용으로 수집하지 않습니다. 진단·FPS의 POST는 로그를 바꿀 수 있어 이번 읽기 전용 조회에 사용하지 않았습니다. 집계 API에는 활성 방 수가 없으므로 온라인 숫자에서 방 수를 추정하지 않습니다.

## 기존 서버 관리창

실제 위치:

```text
C:\Users\user\Documents\MultiGod\PhoneLOL_LocalRuntime\00_PHONELOL_TEST_HERE\04_OPEN_MONITOR.cmd
```

이 파일은 `monitor\RUN_MONITOR.cmd`를 열고, 런처는 `C:\TempPy13\pythonw.exe`가 있으면 사용해 `monitor\phone_lol_monitor_v38.py`를 실행하도록 구성돼 있습니다. 파일 존재와 실행 연결을 확인했으며 실제 창을 열어 사용 완료를 검증한 것은 아닙니다.

| 코드에서 확인한 기능 | 현재 판단 |
|---|---|
| 서버 시작·종료·재시작 버튼 | 기능 연결은 존재하지만 아래 버전 불일치가 있어 현재 서버의 안전한 제어로 인정할 수 없음 |
| 현재 연결·활성 방·방 입장 전 연결·최근 종료 | 기존 서버 로그를 파싱해 표시. 온라인 집계 API와 같은 기준임을 확인하지 않음 |
| 최근 주요 상황 | 로그의 최근 이벤트를 한국어로 변환해 표시 |
| 서버 포트·인터넷 터널·폰 진단 상태 | 기존 관측 코드 존재. 실제 창의 표시 정확도·UI는 미검증 |

**제어 경로 불일치:** v38은 `recovery\CENTRAL_AUTHORITY_CONTROL_V30.ps1`로 버튼을 연결합니다. 실제 파일 내부 대상은 `server_central_authority_v30.py`인데 조사 시점 실행 서버는 **v33**입니다. 재시작 버튼은 Stop 후 Start를 호출합니다. 따라서 기존 관리창을 연결했다는 이유로 현재 서버의 시작·재시작·종료가 검증됐다고 표시하지 않습니다. 먼저 최신 원본·런타임 실행기·프로세스 식별 규칙을 담당자가 확인해야 합니다. 이번 작업에서는 제어 스크립트를 실행하거나 고치지 않았습니다.

기존 `02_STATUS.cmd`도 `PHONELOL_TEST_V213.ps1 -Action Status`를 호출합니다. 이 스크립트의 출력에는 상세 명령이 포함될 수 있어 관제 화면에 그대로 복사하지 않습니다. 이번 프로세스 조사는 명령 전체·환경변수·키를 출력하지 않고 실행파일·스크립트명·알려진 경로 일치 여부만 확인했습니다.

## 관제 및 공지 연결 범위

게임 개발 공지 대상은 `PhoneLOL`, 서버 운영 대상은 **`PhoneLOL-Server`**입니다. 서버는 같은 게임의 운영 기능이지만 실제 루트가 다르므로 서버 루트에 별도의 `_통합소통`을 연결합니다. 클라이언트 확인 기록을 서버 담당자의 읽음·적용으로 대신 처리하지 않습니다. 로컬 배포·수집·중앙 동기화·담당자 확인 상태도 구분합니다.

통합 관제의 기본 확인은 현재 서버 프로세스 식별, 로컬 API 응답, 응답 시각·집계·정책 상태입니다. 일반 Python·Node 프로세스가 있다는 이유로 게임 서버 정상이라고 표시하지 않습니다. 관리 앱이 닫혔거나 PC·경로·인증·통신이 연결되지 않았으면 확인 대기로 둡니다. 메모리 추세나 방 집계가 필요하면 현재 런타임에서 안전한 근거를 별도로 확인합니다. Source에 메모리 표본·로그 코드가 있다는 사실만으로 실제 장기 기록이 수집 중이라고 주장하지 않습니다.

이번 문서는 조사 근거입니다. 관리 앱의 새 서버 화면·반복 조회·자동 소통의 실제 설치 및 UI 검증 결과는 담당자의 구현·검증 기록에서 확인합니다. 문서 작성이나 파일 연결만으로 전체 서버 관리 완료를 선언하지 않습니다.
