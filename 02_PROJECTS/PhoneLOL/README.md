# PhoneLOL — 멀티의 신

**작업 시작·재개 시 필독:** [공지사항방](../../04_COMMUNICATION/announcements/README.md)의 새·변경 공지를 실제로 읽고 본인 프로젝트·AI 확인 기록을 남깁니다. 이미 확인한 동일 내용은 반복 조회하지 않습니다.

## 어디서 무엇을 하나

| 구분 | 현재 원본 | 하는 일 |
|---|---|---|
| 게임 개발 | [PhoneLoL_02-Source](https://github.com/kimjae134679/PhoneLoL_02-Source) | Unity 소스·현재 작업·게임과 서버 변경의 인수인계 |
| 사용자 설치파일 | [PhoneLoL_02](https://github.com/kimjae134679/PhoneLoL_02) | APK·배포 결과와 사용자 확인 완료본. 현재 개발 소스 저장소가 아님 |
| 과거 개발 기록 | [PhoneLOL](https://github.com/kimjae134679/PhoneLOL) | 이전 조사·복구·개발 이력. 현재 서버에 옛 코드를 복사하는 기준으로 쓰지 않음 |

최신 작업은 개발 원본의 [README](https://github.com/kimjae134679/PhoneLoL_02-Source/blob/main/README.md), [HANDOFF](https://github.com/kimjae134679/PhoneLoL_02-Source/blob/main/HANDOFF.md), [서버 안내](https://github.com/kimjae134679/PhoneLoL_02-Source/blob/main/Automation/Server/README.md)를 확인합니다. 이 허브 포인터에 최신 버전·프로세스 번호를 고정하지 않습니다.

## 게임 폴더와 서버 폴더

| 위치 | 역할 | 공지 대상 |
|---|---|---|
| `D:\A_KJ\AI\PhoneLoL_02` | 게임 주개발 폴더. Unity 프로젝트는 하위 `PhoneLOL-02`; APK는 휴대폰 설치파일 | `PhoneLOL` |
| `C:\Users\user\Documents\MultiGod\PhoneLOL_LocalRuntime` | 현재 PC에서 실제 실행하는 멀티플레이 서버·기존 실행기·운영 자료 | `PhoneLOL-Server` |

서버는 게임과 관련된 별도 운영 항목입니다. 서로 다른 실제 루트이므로 소통 폴더도 각 루트에 연결합니다. 클라이언트 담당자의 확인을 서버 담당자의 확인으로 자동 승계하지 않습니다. 연결·배포 여부와 본인 읽음·적용 기록은 따로 확인합니다.

[서버 연결·관측 근거](../../ai-control-tower/docs/MULTIPLAYER_SERVER.md)에는 실제 실행 위치, 조회 API, 기존 관리창의 한계를 기록합니다. `Automation/Server`는 기존 런타임·실행기·데이터를 전제로 하는 변경 소스이며 독립 서버 전체를 대체하는 폴더가 아닙니다. 목록 정리를 이유로 서버·계정 DB·게임 원본을 임의 이동하거나 삭제하지 않습니다.

사용자 요구는 1인·다인 모두 정상 게임 진행입니다. 구현·빌드·서버 반영·실제 휴대폰 플레이·사용자 확인 완료를 구분하고 완료본을 보호합니다. 서버 조회와 관리 연결이 게임 코드 변경·서버 재시작·배포·클라우드 이전 권한까지 뜻하지 않습니다.

- [PhoneLOL 소통방](../../04_COMMUNICATION/rooms/PhoneLOL/README.md)
- [통합 인수인계 T-0010](../../04_COMMUNICATION/threads/T-0010-phonelol-arm64-recovery/THREAD.md)
- [배포 저장소의 열린 이슈](https://github.com/kimjae134679/PhoneLoL_02/issues)

연결 근거 확인: **2026-10-05 KST**. 재개 시 현재 원본과 실제 PC 상태를 다시 확인합니다.
