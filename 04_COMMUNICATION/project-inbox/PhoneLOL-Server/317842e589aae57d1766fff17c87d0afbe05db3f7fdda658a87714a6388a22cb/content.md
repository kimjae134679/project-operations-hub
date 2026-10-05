## 최신 게임 기준 · 0.24.4 / build264 · Spectator264

- 접속자 목록 v2의 관전 버튼: viewer-bound 30초 토큰·auth61036·동일 경기 재검증·읽기 전용 관전자 입장. 참가 자리/방장/epoch/ID/보상 명단/시계 보존.
- 전 모드 0/10/20/101/102/103, full URF6 및 1vs1 포함. 종료/초기로딩/저장없음/실참가자없음/다른 진행 중 경기 참가 상태에서는 관전 준비 거부. v1 접속목록 API 유지.
- 신규 관전은 양맵 초기화에서 RestoreEmpty 경로 강제. 새 체크포인트를 받은 뒤 월드 복원. 호스트264 즉시 캡처61038, 이전호스트262/263 정기5초 이용. 체크포인트 시작61039에서 이전 월드 패킷만 버리고 이후 실시간 패킷/결과/소유권 유지.
- 관전 양팀 핑 수신. mini/legacy large ping controls hide+send guard; 지도 보기 유지. 양팀 채팅/조이스틱/부드러운 자동 따라가기/회색 프로필 기존 동작 유지.
- sound/game/DisconnectUser 및 ReconnectUser 직접 2D PlayOneShot·효과음 음소거 존중. 역할 스냅샷의 active 변화를 빠짐없이 처리, 최초 명단/관전자 입퇴장 제외·실이탈 중복 억제.
- TCP33/대기방12/늦은관전15/목록인증16, 원본Lobby/양맵 핑·음성 실제 AudioSource.isPlaying/카메라 및 목록버튼 NGUI/채팅 검사 통과.
- v0.24.4=fdae6b7eeed3599f8b116f21afe55171e661b981. APK/Xcode ZIP 공개·remote SHA 및 signer 검증. 이전 릴리즈 보존. 다음 게임 변경0.24.5/build265.
- 서버65초 사전 공지·SQLite online backup·검증된 v33 PID만 교체. 런타임9파일 해시/DB integrity/테이블수/공지복구/HTTP3경로 확인. roster monitor 보존.
- 물리폰 전체 실서버 다인전과 체감 음량은 미검증. 기존 관전자만 남는 방 수명 정리 문제는 미해결이며 신규 입장 차단.
- 사용자 AGENTS/_통합소통 stage/삭제 없음. 본인 완료 기록만 작성. Astra 및 서브에이전트 사용 없음.

---

