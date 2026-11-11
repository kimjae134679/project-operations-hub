# 통합소통 허브 — 현재 작업 진입점

정리일: 2026-11-12 KST. 이 문서는 현재 허브의 진입점이며 다른 프로젝트·독립 채팅의 전역 규칙이 아니다.

## 먼저 확인할 것
- 현재 지침의 단일 원본: [사용자 정책](01_CONTROL/USER_POLICIES.md).
- 현재 요청과 남은 일: [작업 우선순위](01_CONTROL/WORK_PRIORITIES.md).
- 설치 위치와 확인 수준: [AI 설치·도구 현황](01_CONTROL/AI_INSTALLATIONS.md).
- 실제 코드·자료·실행 상태는 해당 프로젝트의 현재 README/AGENTS와 실제 파일을 대조한다.
- 요청과 관계없는 공지·긴 기록·후보 도구를 전부 읽거나 다시 실행하지 않는다.
- 문서 검토·제안은 읽기 전용이다. 구현·게시·삭제는 승인된 범위에서만 진행한다.

<!-- control-tower:communication-entry:begin -->
이 허브의 소통 ID는 Control-Tower다. 소통 구조는 [04_COMMUNICATION](04_COMMUNICATION/README.md)를 확인한다.
실제 새·변경 공지가 해당 작업에 적용될 때만 받은 목록과 본인 기록을 대조한다. 전달·수집·읽음·적용·GitHub 전송은 서로 다르다.
로컬 수집은 GitHub 업로드가 아니다. 전송에는 관리자의 명시적 일회 명령이 필요하고 다른 AI의 확인은 대필하지 않는다.
자동 생성된 관리 블록이나 과거 공지의 자동 공유 문구가 재삽입되어도 현재 USER_POLICIES의 범위·전송 기준이 우선한다.
<!-- control-tower:communication-entry:end -->

## 보존과 실제 결과
- 기존 수정·미저장 자료·실행 중 작업·원격 연결을 보호한다.
- 실행 접수·파일 존재·검사 통과·실제 사용 성공을 구분하며 미확인을 완료로 바꾸지 않는다.
- 승인 없는 추가 과금·계정/플랫폼 권한 변경·보안 우회·외부 게시를 하지 않는다.

[정리 전 원문](99_ARCHIVE/2026-11-12/rules-before-034346/AGENTS.md)

## 현재 스킬·PC 도구 선택
- 이 저장소의 [활성 스킬](.agents/skills)은 핵심 4개와 앱 재현 단계 11개다. 일반 개발이나 URL만으로 클론을 시작하지 않는다.
- PC 파일·설정·명령은 연결된 우리 ProjectBridge 우선, 허가된 앱 UI는 현재 host가 허용한 agent-desktop 또는 CUA 표면만 사용한다.
- cloud에 Windows CLI가 있다고 가정하지 않는다. 연결된 PC capability가 실제 제공될 때만 PC 작업을 수행한다.
- 정책·권한 거절을 다른 도구로 우회하지 않고 실행 불명은 동일 작업 ID로 확인해 중복 쓰기를 막는다.
- [구성과 자료·라이선스](tools/agent-setup/README.md), [portable 공통 규칙](tools/agent-setup/global-AGENTS.md)을 필요한 때만 참조한다.

## 통합소통 기록 기준
통합소통을 작성·갱신할 때의 분류·날짜·설명·첨부·보존은 [기록 규칙](04_COMMUNICATION/RECORDING_RULES.md)을 따른다. 매 턴 게시나 전체 공지 필독을 강제하지 않는다.
