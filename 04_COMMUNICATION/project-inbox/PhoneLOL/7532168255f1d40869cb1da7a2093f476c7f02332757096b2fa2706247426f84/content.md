## 최신 배포 · 0.24.6 / build266 · Spectator266

- 접속자 관전 버튼을 길드 아래 닉네임·상태 줄 중앙에 정렬. 원본 Lobby/NGUI PlayMode, 실제 글꼴, 854·1040·1280 해상도, 큰 글꼴·줄바꿈의 검사/캡처 검토.
- 관전 준비 ACK 뒤 별도 연결/입장 경로. native18 참가자 소유권·Eve group 검증이 끝나면 MultiGame/MtmGame 복원으로 바로 이동. native7 준비방 UI 표시 억제; 기존 10초 준비 카운트다운 경유하지 않음. 서버의 최신 체크포인트 수신 대기는 필수.
- 복원한 캐릭터/NPC에 관전자 소유권을 주던 오류 수정. 원래 참가자 owner 보존, 새 관전 때 오래된 복원 큐/실패 플래그 초기화. 일반 empty-room 참가자 소유권 복구는 기존대로 유지.
- 추가 요청 반영: 온라인 단독 경기도 처음부터 multiplayer bridge/live server relay 사용. 기존 num>1 최적화 때문에 나중에 들어온 관전자에게 이동 전송이 꺼져 있던 원인 제거. 명시적 오프라인 게임만 로컬. 게임 시뮬레이션은 기존 Eve 클라이언트 방식이고 서버가 방/전송/체크포인트를 관리; 서버 물리 시뮬레이터로 변경한 것은 아님.
- 실제 Unity 프리팹→native 이동/전체 상태24프레임 생성→임시 TCP 서버 관전 join/load/snapshot/world-ready→양팀 프레임 수신→fresh Unity Eve receiver/Actor에 연속 위치·HP 반영. world/native124검사, 일반 참가자 복원34검사, TCP23검사 통과. 실폰 전체 다인 경기 미검증.
- 플레이하는 쪽과 관전하는 쪽 모두0.24.6으로 업데이트 필요. 구버전 혼자 시작 경기의 클라이언트 전송 코드는 서버만으로 바뀌지 않음.
- 공개 APK/Xcode ZIP266의 크기·SHA·서명·manifest/plist266/CRC 확인. source v0.24.6-r1=a4dfbeab6d75dd55a29fa3080bcde1fa62f35dd6. 초기 candidate tag v0.24.6=058c707은 이동하지 않으며 공개 산출물은r1 소스. 기존0.24.5/그 이전 APK·Xcode·공개 태그·stable/latest0.23.4 보존.
- 이번 서버 파일 변경/재시작 없음. 실행 서버0.24.4의9파일 hash, HTTP3경로, relay, SQLite integrity, 원래 공지 보존 확인. iOS 서명 IPA 없음.
- 사용자가 요청한 대로 작업 후 Unity GUI를 열지 않음. AGENTS/_통합소통은 stage/삭제하지 않으며 본인 완료 기록만 작성. Astra/서브에이전트 사용 없음.

---

