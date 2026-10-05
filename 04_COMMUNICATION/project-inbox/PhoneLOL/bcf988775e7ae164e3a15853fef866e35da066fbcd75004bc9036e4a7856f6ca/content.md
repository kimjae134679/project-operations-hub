## 최신 배포 · 0.24.5 / build265 · Gold265

- 0.24.4 관전·양팀 핑·원본 접속/탈주 음성 배포와 서버 검증을 완료한 뒤 추가 변경.
- 관전 준비 중 연결 전에 취소하면 WatchingEntry/RestoreEmpty/RestoringWorld와 로컬 watch intent 해제. 기존 기다리기 나가기 ACK 경로 보존. 실제 원본 Lobby 검사66 통과 후 골드 변경.
- URF102·URF5 103 시작 골드4,000→1,000. 일반0/10/20/100/101은500 유지. Actor 신규 생성·InitForReuse 공통 StartingMoney 경로; 기존 경기 체크포인트 자산을 초기화하지 않음.
- 원본 Lobby native room 모드 바인딩 및 취소 회귀73 검사 통과. 이전 관전 TCP76/양맵 핑·음성/카메라/목록/채팅 검증은0.24.4 증거 참조; 이번 서버 파일 변경 없음.
- source v0.24.5-r1=5a461cca19be43247fe7e436b8eaec78d982a248. APK·Xcode ZIP 신규 공개, remote SHA/패키지265/기존 signer 검사. 기존 APK/Xcode/태그 보존. iOS 서명 IPA 아님.
- 실행 서버0.24.4의9개 소스 해시/HTTP3경로/relay/SQLite integrity/원래 공지 전체 복구 확인. 이번 변경은 클라이언트 규칙으로 추가 서버 재시작 없음. 테스트 시 모든 참가자는0.24.5 사용.
- 물리폰 전체 다인전·체감 음량 미검증. 기존 마지막 실참가자가 나간 후 관전자만 남는 방 수명 정리 미해결; 새 관전 입장 차단.
- 사용자 AGENTS/_통합소통 stage/삭제 없음, 본인 완료 기록만 작성. Astra/서브에이전트 사용 없음.

---

