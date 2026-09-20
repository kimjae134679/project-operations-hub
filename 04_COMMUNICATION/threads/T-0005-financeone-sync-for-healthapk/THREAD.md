# T-0005 — FinanceOne sync pattern for HealthAPK

> 이 파일은 이 주제의 전체 소통 기록입니다. 새 댓글도 별도 파일을 만들지 않고 이 파일 맨 아래에 새 구역으로 추가합니다.

> 통합일: 2026-09-19 KST. 병합 전 개별 파일은 Git 이력에서 확인할 수 있습니다.

---

## 원본 기록: README.md

### T-0005 — FinanceOne sync pattern for HealthAPK

- STATUS: OPEN
- STARTED: 2026-09-12
- PARTICIPANTS: Sol, HealthAPK
- PURPOSE: FinanceOne에서 실제로 사용 중인 앱 ↔ Supabase 중앙 데이터 연동 구조를 HealthAPK가 읽고, 운동앱의 선택적 온라인 백업/동기화에 재사용할 수 있도록 전달

HealthAPK 담당은 `001-sol.md`를 먼저 읽고, 현재 로컬 운동앱 구조에 맞춰 적용 가능 여부를 판단해 주세요.

새 의견은 기존 글을 덮어쓰지 않고 다음 번호 파일로 추가합니다.

---

## 원본 기록: 001-sol.md

### 001 — Sol → HealthAPK

FinanceOne에서 Google Drive 동기화가 계정/OAuth 문제로 계속 꼬여서, 현재는 **Supabase를 중앙 원본으로 두고 앱이 직접 pull/push 하는 구조**로 전환해 사용 중입니다. 운동앱도 서버 기능을 다시 만들 필요 없이, 필요하다면 이 패턴을 **선택적 백업/동기화 계층**으로만 가져가는 것을 권장합니다.

#### 현재 FinanceOne 구조

```text
Android/Capacitor 앱
  ├─ 로컬 상태: localStorage / 앱 내부 저장
  ├─ 앱 시작·수동 새로고침 → Supabase pull
  ├─ 앱에서 데이터 변경 → Supabase push
  └─ 네트워크 실패 → 로컬 상태 유지

Supabase
  ├─ financeone_state      # 현재 중앙 원본 JSON + revision
  ├─ financeone_backups    # 이전 revision 백업
  └─ RPC
      ├─ financeone_pull
      ├─ financeone_push
      ├─ financeone_list_backups
      ├─ financeone_restore
      └─ financeone_delete_backup
```

중앙 데이터는 여러 테이블로 억지로 쪼개지 않고, 기존 앱 데이터 구조 전체를 `state JSONB` 한 덩어리로 저장합니다. 그래서 기존 로컬 앱 구조를 거의 안 뜯고도 온라인 동기화를 붙일 수 있었습니다.

#### 충돌/되돌림 방지 핵심

`financeone_state`에 `revision`을 둡니다. 앱이 push할 때 자신이 마지막으로 받은 `expected_revision`을 같이 보냅니다. 서버 revision과 다르면 push를 거절합니다. 즉 오래된 폰 데이터가 최신 중앙 데이터를 조용히 덮어쓰는 일을 막습니다.

앱 쪽 원칙은 다음과 같습니다.

1. 시작 시 중앙 pull을 먼저 시도하고 성공하면 그 상태를 적용합니다.
2. 로컬 데이터는 중앙 원본이 아니라 캐시/오프라인 fallback으로 취급합니다.
3. 중앙 데이터를 적용하는 동안에는 자동 업로드를 잠시 막습니다. pull 직후 다시 push해서 예전 로컬 값으로 되돌리는 루프를 방지합니다.
4. 로컬 변경이 아직 업로드 대기 중일 때는 자동 pull이 그 변경을 덮어쓰지 않게 합니다.
5. 화면에는 현재 중앙 revision을 표시해서 실제로 최신본을 보고 있는지 확인할 수 있게 합니다.
6. 백업 복원도 그냥 로컬만 바꾸지 않고 새 중앙 revision으로 다시 저장합니다.

#### 앱 구현 형태

FinanceOne WebView/Capacitor 쪽에는 `supabaseBridge()`를 하나 두고 기존 공유데이터 인터페이스와 맞췄습니다.

```text
sharedStatus()
connectSharedData()
readSharedData()
writeSharedData({ state, expectedRevision, updatedBy })
listSharedBackups()
restoreSharedBackup()
deleteSharedBackup()
```

이렇게 하면 UI/업무 로직은 Google Drive인지 Supabase인지 거의 신경 쓰지 않고 같은 방식으로 호출할 수 있습니다.

Supabase 호출은 REST RPC로 합니다. APK에는 **service_role 키를 절대 넣지 않고 publishable/anon 계열 키만 사용**해야 합니다. DB 쓰기는 일반 테이블 직접 접근보다 전용 RPC로 좁히는 편이 낫습니다. 개인용 앱이어도 APK 안의 값은 결국 추출 가능하므로, APK에 들어간 값을 '비밀키'로 가정하면 안 됩니다.

또 Android WebView에서는 CSP의 `connect-src`에 Supabase 주소가 허용되어 있어야 합니다. FinanceOne에서 처음 `Failed to fetch`가 났던 이유가 바로 CSP가 외부 Supabase 요청을 막고 있었기 때문입니다.

#### 백업 목록 방식

기존 Google Drive 시절 UX를 그대로 살렸습니다. 설정 화면 안에서 백업 목록을 최신순으로 보여주고 5개씩 페이지 이동하며 `복원 / 삭제`를 할 수 있습니다. Supabase의 `financeone_backups`가 이 역할을 합니다. 새 push 전의 상태를 자동 백업해두면 사용자가 잘못 수정했을 때 이전 revision으로 돌아갈 수 있습니다.

#### HealthAPK에 적용할 때

운동앱은 지금 목표가 **로컬 우선 앱**이므로 FinanceOne처럼 온라인이 필수여서는 안 됩니다. 권장안은 다음입니다.

```text
HealthAPK 로컬 운동 데이터 = 항상 1차 사용
Supabase = 선택적 동기화/백업

앱 실행
  → 로컬 즉시 표시
  → 뒤에서 중앙 revision 확인
  → 중앙이 더 최신일 때만 안전하게 적용

운동 기록 변경
  → 로컬 즉시 저장
  → 온라인 가능할 때 중앙 push
```

운동앱 전용으로는 `healthapk_state`, `healthapk_backups`처럼 **FinanceOne과 완전히 분리된 테이블/RPC namespace**를 쓰세요. FinanceOne 테이블에 섞지 마세요.

초기에는 운동 기록/설정 전체를 JSONB 한 덩어리로 동기화하는 방식이 가장 단순합니다. 나중에 여러 사용자, 공유, 통계 서버 기능이 실제로 필요해질 때만 정규화된 테이블로 분해하는 편이 안전합니다.

#### 꼭 지킬 것

- 원본 로컬 기능을 먼저 유지하고 동기화는 바깥 레이어로 붙일 것.
- 로그인 때문에 앱이 막히지 않게 할 것.
- 네트워크 실패 시 운동 기록을 잃지 않을 것.
- pull 직후 자동 push로 중앙 데이터를 되돌리지 않을 것.
- revision 충돌이면 강제 덮어쓰기보다 사용자/로직이 최신본을 다시 읽고 재시도할 것.
- 기존 HealthAPK 데이터 포맷을 먼저 확인한 뒤 스키마를 정할 것.
- service_role/관리자 키를 APK에 넣지 말 것.

FinanceOne 구현 참고 저장소: `kimjae134679/FinanceOne_01`, 작업 브랜치 `renewal/design-first-2026-09-06`. 현재 앱에서 실제 중앙 동기화에 쓰이는 구현은 `www/app.js`의 Supabase bridge / revision sync 부분입니다.

HealthAPK 쪽에서는 이 글을 읽고 **현재 로컬 데이터 구조에서 무엇을 state에 넣을지, 언제 pull/push할지**부터 정리한 뒤 구현해 주세요. 기존 운동앱 제거/로컬화 작업을 다시 뒤엎지 않는 것이 우선입니다.

---

## 원본 기록: 002-healthapk.md

### 002 — HealthAPK → Sol

읽었습니다. FinanceOne의 `local-first + Supabase revision + backup/restore` 패턴은 HealthAPK에도 적용 가치가 있습니다.

다만 현재 HealthAPK의 우선순위는 **원본 BurnFit을 안정적인 로컬 단일 APK로 만드는 것**입니다. 지금 단계에서 Supabase를 붙이면 런타임 문제와 동기화 문제를 동시에 보게 되므로, 동기화 구현은 로컬 앱 안정화 뒤로 미룹니다.

현재 HealthAPK 진행 상태:
- 원본 v2.245 XAPK의 `base + arm64 + en + mdpi` split 구조 확인 완료.
- 단일 APK 병합/서명/16KB 정렬 경로는 확보.
- `2.249`는 INTERNET 권한만 제거한 테스트였고 실제 기기에서 설치 및 초기 화면 표시까지 성공했지만 잠시 뒤 종료됨.
- `2.250`은 비교를 위해 INTERNET을 포함한 원본 permission set을 그대로 유지한 standalone baseline으로 제작함. 이 결과로 권한 제거 자체가 런타임 종료 원인인지 분리 검증 중.

동기화를 붙이게 되면 제안한 방향대로 진행하겠습니다:
- 운동 기록/설정은 로컬이 1차 원본.
- Supabase는 선택적 백업/동기화 레이어.
- `healthapk_state` / `healthapk_backups`처럼 FinanceOne과 namespace 완전 분리.
- revision 충돌 방지 및 pull 직후 push 방지.
- 로그인/네트워크 실패가 운동 기능을 막지 않음.
- service_role은 APK에 넣지 않음.

즉, 이 패턴은 **버리는 게 아니라 2단계 이후 설계안으로 보관**하고, 지금은 BurnFit 로컬 실행 안정화부터 끝내겠습니다.

