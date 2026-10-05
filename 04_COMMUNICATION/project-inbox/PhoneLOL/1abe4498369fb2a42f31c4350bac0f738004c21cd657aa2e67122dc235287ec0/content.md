## 최신 배포 · 0.24.7 / build267
- 관전 재입장이 공통 HNA 접속 초기화를 사용: 이전 leave ACK/result callback/intentional-close/room failure 상태 초기화. 관전 취소 시 연결·입장 flags/deadline도 정리.
- 관전 skillPlus HUD 재활성화 방지, 7개 NGUI click alias 및 Actor.SkillLevelUp 차단. 참가자/AI의 기존 학습과 원격 스킬 상태 수신 유지.
- mainBody에 정확히 "- 관전기능 추가" 추가. 다른 공지/layout/roster/maintenance 항목은 동일.
- 실제 Unity world135·TCP25(같은 UID 동일 solo 경기3회+새소켓 재접속)·일반 재접속34·로비 검사 통과. 실폰 전체 경기는 미검증.
- source v0.24.7=64bfbbe22c46a2fafc9a7f872ec8628810ca5ac6; APK267 manifest/CRC/원래 signer·Xcode267 plist/CRC·공개 digest 검증. 이전 assets·stable latest0.23.4 보존.
- 서버 code9파일 변경/재시작 없음; local/public HTTP3경로·SQLite integrity 확인. 관전자만 남는 방 정리는 기존 미해결 별도 사항.
- 사용자의 요청대로 Unity GUI 테스트 요청·실행 없음. 본인 공지 확인 기록만 작성하며 중앙 수집은 미검증.
---
