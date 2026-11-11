# 스킬·규칙 구성

2026-11-12 정리. 이 저장소를 작업 환경으로 열 때 .agents/skills의 핵심 4개와 앱 재현 단계 11개를 목적에 맞게 선택한다.
이는 다른 cloud 앱·계정의 전역 설정을 설치하거나 PC용 실행 파일을 배포한 것이 아니다.

## 도구 경계
- PC 파일·설정·명령은 연결된 우리 ProjectBridge 우선.
- UI는 사용자에게 허가된 앱·현재 host 표면에서 agent-desktop 우선, 불가하면 허용된 CUA.
- cloud에 Windows CLI가 있다고 가정하지 않는다. 실제 PC 연결 capability가 제공되지 않으면 PC 조작을 시작하지 않는다.
- 명시적 거절·UAC·잠금·인증을 우회하지 않고 실행 불명은 동일 작업 ID로 확인한다.
- replica는 앱 재현 맥락의 필요한 단계에만 사용한다. 다음 단계 추천은 배포·게시·결제 승인이 아니다.

## 스킬·원문·라이선스
- [agent-desktop-windows](../../.agents/skills/agent-desktop-windows/SKILL.md) · [원문 라이선스](../../.agents/skills/agent-desktop-windows/LICENSE)
- [systematic-debugging](../../.agents/skills/systematic-debugging/SKILL.md) · [원문 라이선스](../../.agents/skills/systematic-debugging/LICENSE)
- [verification-before-completion](../../.agents/skills/verification-before-completion/SKILL.md) · [원문 라이선스](../../.agents/skills/verification-before-completion/LICENSE)
- [unslop](../../.agents/skills/unslop/SKILL.md) · [원문 라이선스](../../.agents/skills/unslop/LICENSE)
- [replica-architect](../../.agents/skills/replica-architect/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-architect/LICENSE)
- [replica-backend](../../.agents/skills/replica-backend/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-backend/LICENSE)
- [replica-brand](../../.agents/skills/replica-brand/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-brand/LICENSE)
- [replica-build](../../.agents/skills/replica-build/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-build/LICENSE)
- [replica-deploy](../../.agents/skills/replica-deploy/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-deploy/LICENSE)
- [replica-design](../../.agents/skills/replica-design/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-design/LICENSE)
- [replica-diff](../../.agents/skills/replica-diff/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-diff/LICENSE)
- [replica-entrepreneur](../../.agents/skills/replica-entrepreneur/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-entrepreneur/LICENSE)
- [replica-launch](../../.agents/skills/replica-launch/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-launch/LICENSE)
- [replica-recon](../../.agents/skills/replica-recon/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-recon/LICENSE)
- [replica-test](../../.agents/skills/replica-test/SKILL.md) · [원문 라이선스](../../.agents/skills/replica-test/LICENSE)

unslop의 설명·참고 이미지 5개·원문 스크립트·CC BY 4.0 저작자 표시는 함께 보존한다. snapshot/story helper는 원문 보관용이며 활성화하지 않는다.
agent-desktop는 Apache-2.0, replica는 MIT, 두 Superpowers core는 MIT 고지를 포함한다. 설치 당시 core commit은 미기록이며 추정하지 않았다.
파일별 원본 SHA와 공개 export SHA·일반화 여부는 manifest.json에서 구분한다.

## 현행 문서와 과거 원문
- [AGENTS.md](../../AGENTS.md)
- [01_CONTROL/USER_POLICIES.md](../../01_CONTROL/USER_POLICIES.md)
- [01_CONTROL/WORK_PRIORITIES.md](../../01_CONTROL/WORK_PRIORITIES.md)
- [01_CONTROL/AI_INSTALLATIONS.md](../../01_CONTROL/AI_INSTALLATIONS.md)
- [04_COMMUNICATION/README.md](../../04_COMMUNICATION/README.md)
- [04_COMMUNICATION/announcements/PROTOCOL.md](../../04_COMMUNICATION/announcements/PROTOCOL.md)
- [00_SYSTEM/GOVERNANCE.md](../../00_SYSTEM/GOVERNANCE.md)
- [04_COMMUNICATION/announcements/notices/N-0006.md](../../04_COMMUNICATION/announcements/notices/N-0006.md)
- [04_COMMUNICATION/announcements/manifest.json](../../04_COMMUNICATION/announcements/manifest.json)
- [공통 portable 규칙](global-AGENTS.md)
- [공개용 정리 전 원문](../../99_ARCHIVE/2026-11-12/rules-before-034346/AGENTS.md)과 같은 보관함의 remote-main-before는 서로 다른 출처다. 보관일은 제작일/출시일이 아니며 날짜만으로 자동 삭제하지 않는다.

## 제외한 것
PC config.toml·auth·토큰·계정 설정·메모리/대화 DB·사용자 비공개 첨부·모델·환경·native 실행 파일은 포함하지 않는다.
공개용 복사본의 PC 전용 경로와 기기 ID는 일반화했다. N-0006의 이미 공개된 과거 경로만 기준 원문과 비교한 예외로 유지한다.
GitHub 전송·PR·병합과 로컬 수집·문서 준비를 구분한다. 이 자료 준비가 다른 AI의 읽음/적용을 뜻하지 않는다.

## 기록 규칙
통합소통을 작성·갱신할 때만 [단일 기록 기준](../../04_COMMUNICATION/RECORDING_RULES.md)을 사용한다. 분류·날짜·설명·원문과 그림의 보존은 AI가 정리하고 사용자에게 JSON 학습을 요구하지 않는다.
현재 구성은 핵심 4개와 재현 단계 11개이며, 추가 10개 절차는 날짜 보관했다. 선별 일회 GitHub 동기화만 승인됐고 완료 여부는 실제 commit/PR로 판단한다.
