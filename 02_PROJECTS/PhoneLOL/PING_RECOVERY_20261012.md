# 멀티의 신 핑 복구 · 2026-10-12

브릿지로 실행 서버와 원본 런처를 수정했다. [소스 PR #17](https://github.com/kimjae134679/PhoneLoL_02-Source/pull/17)은 main에 병합됐다. 기준 커밋: `083da03aa650726f59e1aeca1dcbaa9117f078de`.

## 확인한 문제와 변경

- 통합 관리 watcher의 BelowNormal 우선순위를 서버 런처와 Python 서버가 물려받았다. 실행 중인 두 프로세스를 Normal로 복구했고, 런처가 시작할 때 자신의 우선순위를 Normal로 설정하도록 보완했다. watcher는 계속 BelowNormal이다.
- 공유기의 기존 게임용 TCP 29000 직접 연결 항목이 없었다(UPnP 714). 현재 PC LAN 주소로 복구하고 readback을 확인했다. 통합툴이 항목을 삭제했다는 근거는 없다.
- `Automation/Server/EnsureDirectRoute.py`를 운영 폴더의 `recovery/EnsureDirectRoute.py`에도 설치했다. 런처 시작 시 현재 LAN 주소와 UPnP control URL을 탐색한다. 없는 항목 또는 설명이 일치하는 기존 PhoneLOL 항목만 복구하고, 다른 프로그램 항목은 보존한다. 실패하면 기존 중계 경로를 유지하며 서버 시작을 막지 않는다.
- 서버 재시작 없이 적용했다. 게임·계정·DB 코드는 변경하지 않았다. 원본과 운영 런처의 환경값 차이를 그대로 보존했다. 새 APK가 필요하지 않다.

## 검증 범위

| 검사 | 결과 |
| --- | --- |
| 수정 전 직접 주소 HTTP | 연결 시간 초과 |
| 수정 후 직접 주소 HTTP | 12.9 / 1.6 / 1.4 / 1.4 ms |
| 같은 시점 중계 주소 HTTP | 688.3 / 728.8 / 755.3 / 679.8 ms |
| 서버 우선순위 및 재시작 여부 | Normal, PID와 시작 시각 유지 |
| 우선순위 상속 검사 | child NORMAL_PRIORITY_CLASS 32 |
| 경로 복구 단위 테스트 | 6개 통과 |
| 운영 PowerShell 구문 검사 | 통과 |
| 실제 휴대폰 게임 RTT | 미측정 |

HTTP 수치는 서버 PC에서 공인주소로 연결한 hairpin 검사이며 실제 휴대폰 게임 핑으로 해석하면 안 된다. 기존 중계 세션은 앱을 완전히 종료하고 다시 접속해야 직접 연결을 새로 시도한다.

## 통합 관리에서 보존할 사항

watcher가 낮은 우선순위로 실행되더라도 게임 서버 런처의 Normal 설정을 보존한다. 런처 배포 시 `EnsureDirectRoute.py`를 같은 recovery 폴더에 함께 복사한다. 서버 runtime manifest는 APK 폴더의 의존성 목록이므로 이 recovery sidecar와 구분한다. 새 watcher나 주기적 서버 재시작을 추가하지 않는다.

운영 및 소스 런처의 적용 전 백업은 각각 `.before-ping-priority-20261012.bak`, `.before-direct-route-20261012.bak`이다. 직접 연결 검사를 되돌리면 복구한 공유기 항목 자체가 자동 삭제되는 것은 아니다.
