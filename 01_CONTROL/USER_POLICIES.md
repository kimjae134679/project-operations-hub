# User Policies

## 작업 방식
- 요청한 일은 끝까지 처리하고 부분 구현을 완료라고 부르지 않습니다.
- Known-Good를 보호하고 broad rewrite보다 작은 검증된 수정을 우선합니다.
- 기존 코드·플랫폼 기본 기능·이미 설치된 의존성을 먼저 검토합니다.
- placeholder, fake data, fake success로 실패를 숨기지 않습니다.
- 위험이 작고 되돌릴 수 있는 애매함은 합리적 기본값으로 진행합니다.

## UI/UX
- `현재 상태 → 지금 할 행동 → 결과 → 상세` 순서를 우선합니다.
- 대표 CTA를 명확히 하고 중복 버튼을 줄입니다.
- 모바일이라고 중요한 기능을 삭제하지 않습니다.
- 눈에 바로 보이는 단순 성공 토스트를 남발하지 않습니다.
- 위험한 삭제/덮어쓰기는 명시적으로 선택된 대상에만 적용합니다.

## 데이터/복구
- Unknown과 실제 0을 구분합니다.
- 설치파일과 사용자 데이터를 분리합니다.
- 배포된 ID/key/storage name을 호환성 계약처럼 다룹니다.
- migration 전 백업/복원 경로를 확보합니다.
- 핵심에 서버가 필요 없으면 local-first를 우선 검토합니다.

## 빌드/검증
- CI, build, install, 실사용, 실기기 PASS를 구분합니다.
- 실제 전달 파일과 검증 파일이 같아야 합니다.
- 변경 영향에 필요한 검증만 수행합니다.
- 중요한 산출물은 필요하면 SHA-256을 남깁니다.

## Windows/원격
- UTF-8, 한글 IME, 공백/한글 경로를 관련 변경에서 확인합니다.
- Remote Desktop이 online이면 작업 종료를 이유로 연결 서비스를 끄지 않습니다.
- child process 정리와 connection service 종료를 구분합니다.
- 실제 PC가 offline이면 local verification을 했다고 주장하지 않습니다.
- **Remote Desktop Commander/데스크톱 원격을 사용한 회차는 임시 빌드 산출물, 중간 APK/ZIP/IDSIG, cache, probe, `__pycache__`, 불필요 로그 등 그 작업이 만든 찌꺼기를 종료 전에 정리하는 것까지 완료 조건입니다.** 최종 산출물과 재현에 필요한 소스/스크립트는 남기고, 사용자 데이터·Known-Good·원본/reference 자료는 임의로 삭제하지 않습니다.
