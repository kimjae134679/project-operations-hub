# Work Schedule

마지막 갱신: **2026-09-15**

이 문서는 당장 끝내지 않아도 되는 후속 작업을 쌓는 작업 스케줄입니다.
시간 예약이 아니라 **다음 작업 우선순위/대기열**입니다.

## 환경/관리 위치
- [x] AI 설치·작업 기본 루트를 `C:\Program Files\_My\AI`로 정책화
- [x] 기존 `C:\KJ\Tools\AI`를 같은 하위 폴더 체계로 staging 정리
- [x] MoneyPrinterTurbo/설치원본/스크립트/launcher를 staging 안에서 분류 이동
- [x] n8n/HyperFrames/Aider는 패키지 매니저 기본 위치 유지 예외로 기록
- [ ] 관리자 승인 후 staging을 `C:\Program Files\_My\AI`로 최종 마이그레이션하고 launcher 실행 검증

## 완료/로컬 준비됨
- [x] MoneyPrinterTurbo v1.3.7 Windows Portable 설치 및 WebUI HTTP 200 확인
- [x] n8n v2.38.7 설치 및 local WebUI `127.0.0.1:5678` HTTP 200 확인
- [x] n8n `AI Ops Hub - Local Task Bridge` 생성·publish·재시작 후 webhook 실제 응답 검증
- [x] n8n 호출 helper `Scripts\Runtime\Send-N8nTask.ps1` 실제 호출 PASS
- [x] HyperFrames v0.8.40 설치, CLI help 확인
- [x] Aider v0.86.2 설치, CLI help 확인

## 다음 검증
- [ ] MoneyPrinterTurbo — 실제 20~30초 9:16 영상 1개 생성
- [ ] HyperFrames — 최소 9:16 샘플 1개 preview/render
- [ ] Aider — 테스트 repo에서 작은 diff + lint/test + rollback 확인
- [ ] n8n — 실제 프로젝트 자동화 하나를 붙일 때 credential/실패 재시도/로그 정책 검증

## 설치 후보 — 지금은 보류
- [ ] VoxCPM2 — 별도 Python/CUDA 환경에서 한국어 30초 TTS 및 VRAM 측정
- [ ] browser-use — 기존 브라우저 자동화와 중복 비교 후 격리 테스트
- [ ] Hermes Agent — memory/Skill/shell/cron을 한꺼번에 열지 말고 단계별 테스트
- [ ] OpenHands — Docker 또는 격리 backend 준비 후 테스트 repo 1개만 연결

보류 항목은 사용자 승인 없는 외부 게시/메시지/결제/계정 연결을 자동 실행하지 않습니다.
