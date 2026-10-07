# 프로젝트 기록 공유

**현재 상태: 0.9.6 스테이징 완료·운영 미적용(activated=false).** 아래 0.9.5 일회 headless 공유 이력은 당시 실제 확인입니다. 현재 0.9.3 앱의 반복 수집/공유는 아직 활성화하지 않았습니다. 0.9.5 publish/스테이징은 완료했지만 운영 미적용입니다(source `7c1b896e943883899a76891ec386c3321417fe43`, exe SHA-256 `908e090ddb5ce8af78b207b779730e7371b6dd2d9b74e608d7b466f6e1503729`, activated=false). 현재 0.9.3 PID 16092의 PC/tray true와 기존 원격을 유지합니다. 이전 0.9.4 스테이징은 당시 이력입니다. GUI 교체나 사용자 화면 검증 완료를 뜻하지 않습니다.

## 최신 사용자 지시 · main 병합 절차 진행(완료 미확인)

사용자 최신 지시로 [허브 #24](https://github.com/kimjae134679/project-operations-hub/pull/24)·[허브 #26](https://github.com/kimjae134679/project-operations-hub/pull/26)·[Voice #2](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/pull/2)의 main 병합 절차를 진행합니다. 이 문서 정정 시점에는 3개 PR의 병합 성공을 확인하지 않았고 완료로 단정하지 않습니다. 최신 성공/실패 원본은 GitHub PR 실제 상태이며 아래 미merge/main 미반영/open·draft는 **2026-10-07 12:19~12:28 병합 전 업로드 점검**의 스냅샷입니다.

main 코드/메타데이터 병합은 현재 앱의 0.9.6 활성화·보호 Bridge 업그레이드·중앙 clone 복구가 아닙니다. 기존 0.9.3·원격·dirty를 유지하고 자동 append/상시 반복은 미완료입니다. 새 코드/runtime·모델·계정·registry 변경과 리뷰 실행 없이 receipt-driven review disabled/unmanaged를 유지합니다.

## 2026-10-07 12:19~12:28 병합 전 업로드 점검

정확 0.9.6 exe로 사용자 명시 “일단 GitHub 자체 다 업로드하고 정리”의 한정 게시를 실행했습니다. 새 코드 변경 없이 이전 .NET 529/Python 68 검증과 source `84840a1` 미활성 스테이징을 유지합니다. 일회 게시와 GUI/자동 반복 활성화는 별개입니다.

| 대상 | 병합 전 업로드 점검 당시 상태 | 보존·미완료 경계 |
|---|---|---|
| 허브 | [PR #26](https://github.com/kimjae134679/project-operations-hub/pull/26), 구조 메타데이터 84개, open/draft | old #25의 76개 경로/blob SHA 모두 그대로+새 8개, added only·삭제 0·main 불변 |
| old 허브 #25 | 본인 draft에 superseded 설명 후 closed/unmerged | branch 삭제 0, 과거 성공 증거 보존 |
| 허브 exact replay | 같은 #26·같은 batch/head | 이번 API count 미계측; POST 0으로 주장하지 않음 |
| Voice exact replay | 기존 [PR #2](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/pull/2), 같은 head·10개 파일 | 기존 dirty 1·제작 보존; 새 제작/원본 소스 commit 없음 |
| Threads exact call | exit 1·held, failure stage record_validation | 기존 원본 3개 검증 오류 동일, 업로드 API 전에 보류; dirty 11+3 보존·원본 소스 commit/push 0 |
| PhoneLOL | D 원본/mailbox ID·private repository ID 1378550871·main/push 일치, task JSON 0 | registry 변경 명령 CreateProcess 정책 거절: 실행 0·파일 쓰기 0·재시도/우회 0, 미등록·게시 0 |
| origin 미확인 3개 | 등록 안 함 | 폴더명/유사 이름으로 추측 등록 0 |

허브 #26 head는 `5316147acc0402c27d69f29d3b928ee3ffe28925`, batch는 `7a28924188791a414f906e1fa17589b7f19a39d80a77afc8a169f2ddf16dbe03`입니다. main의 확인 접두 `629afc…`는 불변입니다. Voice head는 기존 `e8b11994c53610add9bf2932ebbc66ac8caeb7d9`입니다.

[소스 PR #24](https://github.com/kimjae134679/project-operations-hub/pull/24)의 정리 전 remote HEAD는 `10039da`이며 제목은 “통합관제탑0.9.6코드와운영경계정리”입니다. GitHub 코드/문서 업로드는 main merge가 아닙니다. 모든 원본 프로젝트 소스를 commit/push한 것으로 확대하지 않습니다. 새 공지 ACK·모델·GUI·원격 config/clone·예약/review 변경은 0이고 반복 publisher active=false를 유지합니다. 변경 batch의 기존 PR 재사용은 여전히 미구현이며 이번 수동 supersede 정리를 해당 기능의 구현으로 표시하지 않습니다.

아래는 이전 #25/509검사 등의 당시 이력입니다. 당시 GET 6회/POST 0회는 그때의 계측 근거이며 최신 replay의 API count로 재사용하지 않습니다.

## 이전 일회 게시 확인 · 2026-10-07

| 대상 | 실제 결과 | 남은 경계 |
|---|---|---|
| 허브 | 124건 로컬 수집·수집 오류 0, 구조 메타데이터 76개로 [draft PR #25](https://github.com/kimjae134679/project-operations-hub/pull/25) 생성 | 당시 draft/open/unmerged, main 변경 없음; 최신 closed 상태는 위 스냅샷 |
| 허브 동일 실행 재확인 | GET 6회·POST 0회, 같은 PR 유지 | 업무 재실행·추가 업로드 아님 |
| Voice | 메타데이터 10개로 [draft PR #2](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/pull/2) 생성 | draft/open/unmerged; 기존 제작 작업 생존·무변경, 새 업무 실행 0 |
| Threads | 13건 수집·수집 오류 0, task JSON 3개 모두 검증 보류 | API 쓰기 전 중단; 원본 3개 hash 불변 |

당시 확인한 PR head: 허브 `bdd7adcedae0f42b0d77c30a39a9d1ee5766d373`, Voice `e8b11994c53610add9bf2932ebbc66ac8caeb7d9`. 모델 호출 0, 자동 merge 0입니다. 실제 readonly GitHub 조회에서 허브 identity·base main을 대조했으며 인증/계정 권한을 바꾸지 않았습니다.

0.9.5 당시 단계 표시 수정 후 부모 최종 fresh 코드 검사: .NET **509 PASS/0 FAIL**(`record-publishing-full-final.trx`), scripts **68 PASS/0 FAIL**, 17.897초(`record-publishing-full-green.log`). 최초 병렬 Git fixture 실패는 LocalExecutionPolicyTests를 비병렬 collection으로 옮긴 뒤 전체 재검사했습니다. 실제 PR 파일도 각각 메타데이터 76/10개 ADDED·삭제 0임을 확인했습니다. 이 검사는 운영 앱의 새 반복 경로 활성화를 뜻하지 않습니다.

Threads 2개는 completed에 남은 필수 작업/막힘이 함께 있었고 1개는 필수 필드 5개 누락·지원하지 않는 상태였습니다. parser 완화·가짜 완료·원본 자동 수정 없이 본인 작성자의 새 revision을 기다립니다. 보호 Bridge 설치 및 기존 clone Git lock 막힘도 그대로이며 이 PR을 해당 복구 완료로 표시하지 않습니다. 예약 PAUSED·review disabled/unmanaged를 유지합니다.

## 최신 소스와 반복 게시 보류

최신 소스 버전은 0.9.6입니다. 안전한 2인자 manual 실행/launcher 소스 결합의 focused 검사 **20 PASS/0 FAIL**(`manual-source-binding-green.trx`)을 확인했습니다. CheckOnly는 descriptor·실제 ProductVersion/commit·SHA-256·UTC 24시간 유효성을 확인하며 start/probe를 실행하지 않습니다. .NET SHA를 사용해 PowerShell 5.1의 Get-FileHash cmdlet 부재에 의존하지 않습니다. 부모 fresh 전체 .NET 검사 **529 PASS/0 FAIL**(8초, `safe-continuation-full-green.trx`)을 확인했습니다. Window.Show 휠 검사 3개는 제외·미실행입니다. 최신 Python 재검사도 68 PASS/0 FAIL(16.781초, `safe-continuation-full-green.log`)이며 0.9.6 publish/스테이징은 완료했고 activated=false·운영 미적용입니다. 기존 CS8602/xUnit 경고를 별도로 남기며 live UI 검증으로 확대하지 않습니다.

### 정확 0.9.6 스테이징 · 운영 전환 없음

- source commit: `84840a130d8a3fb6911b50fe528ab3709e1c76e1`(code 8파일 commit/push), publish exit 0. FileVersion `0.9.6.0`, ProductVersion 확인 접두 `0.9.6+84840a…`.
- exe SHA-256: `2fbc5b312f114cbc60a064cf5caa167251e3ac63bb226522c5a33fa18a82443d`, 크기 `135828359` bytes. D Applications의 `versions/0.9.6-20261007` DEPLOYMENT는 `activated=false`입니다.
- 정확 exe의 `--verify-work-dashboard`는 exit 0, Windows 0·settings load false·MainVM false·downstream 0·sync false·stop 0입니다. `--publish-records-once unregistered`는 exit 2·GUI 없음입니다.
- 정확 0.9.6 source guard의 `-CheckOnly`는 exit 0·descriptor_only·started=false·instanceProbed=false입니다. 실제 launcher NonCheck·GUI 활성화는 0이며 `START_LATEST_VIEW.cmd`의 0.9.3 대상은 그대로입니다. 현재 0.9.3 PID 16092·PC/tray true와 기존 원격을 보존합니다.

동일 batch 재확인에서 기존 성공/같은 PR을 유지한 것과 **변경 batch가 기존 PR을 재사용하는 것**은 다릅니다. 후자는 미구현입니다. 현재 branch 이름은 batchHash별로 나뉘므로 매 변경마다 새 PR을 만드는 반복 게시를 실제 활성화하지 않습니다. 기존 성공 증거를 변경 batch/상시 운영 성공으로 확대하지 않습니다.

append 테스트 파일 작성은 도구 정책 거절로 **작성 0·production 변경 0·GitHub 쓰기 0**, 우회 0입니다. 당시 스테이징 점검의 새로운 valid 게시·메타데이터 GitHub 쓰기·모델 호출은 0이었습니다. 이후 사용자 명시 한정 게시는 위 최신 스냅샷으로 구분하며 등록 publisher active=false는 유지합니다. 현재 GUI 종료·전환, 보호된 원격 config/clone locks 우회는 하지 않으며 예약 PAUSED·review disabled/unmanaged를 유지합니다.

## 무엇을 공유하나요

- GPT 없이 기존 로컬 task_exchange 기록을 수집합니다. 다른 AI의 답변·공지 확인을 대신 작성하지 않습니다.
- 자동 export는 `schemaVersion: 1`, `recordType: task_exchange_metadata`, `projectId`, 불투명 hash인 `recordKey`, `revision`, 허용 `status`, UTC `receivedAt`/`updatedAt`, `verificationCounts`의 pass/fail/not_run/partial 개수뿐입니다.
- 받은 명령·실제 답변·제목·요약·설명·다음 일·evidence 등 자유 본문, actor/session 원문과 개인 경로는 앱/로컬에 남깁니다. 원문 전체·일반 파일·채팅 백업을 GitHub에 올리는 기능이 아닙니다.
- 해당 프로젝트의 별도 `ai-records/` branch와 draft PR을 사용합니다. main 직접 반영·자동 merge·다른 프로젝트 코드 변경은 하지 않습니다.

## 기록 남기기

실제 프로젝트 `_통합소통`에서 기존 기록도우미를 사용합니다. 예시 파일과 ID는 본인 실제 값으로 바꿉니다.

```text
py -3.11 기록도우미.py validate --input 작업기록.json
py -3.11 기록도우미.py record --input 작업기록.json
py -3.11 기록도우미.py render --input 작업기록.json
py -3.11 기록도우미.py latest --project 본인프로젝트ID --actor 본인AIID --record-id 본인기록ID
```

같은 recordId는 revision을 올리며 이전 기록을 보존합니다. 저장 결과가 불명확하면 latest로 먼저 확인하고 업무를 다시 실행하지 않습니다. 공지 **N-0007**의 본인 기록 의무가 적용됩니다. 공지 check/ack와 task_exchange record는 별도이며 다른 담당자의 실제 읽음·적용·기록 작성은 미확인입니다.

## 등록 경계

고정 파일: `D:\A_KJ\AI\ControlTowerData\record-publishing\registrations.json`.

등록 envelope 계약은 `SchemaVersion: 1`, 실제 D 경로인 `BoardRoot`, `Registrations` 배열입니다. 프로젝트별 필드는 다음과 같습니다.

| 필드 | 확인할 내용 |
|---|---|
| ProjectId, RootPath | 실제 프로젝트 ID·원본 폴더 |
| OriginUrl | 해당 원본에서 확인한 GitHub origin |
| RepositoryId, RepositoryFullName | 원격에서 확인한 저장소 identity |
| BaseBranch | 명시한 base branch; 구현 계약의 기본 등록 대상은 main |
| SharedPrefix | 해당 프로젝트만의 `04_COMMUNICATION/shared-records/<ProjectId>` |
| Enabled | 명시 활성화 여부; 미등록·불일치·비활성은 보류 |

실제 고정 D registry에는 허브·Threads·Voice 3개를 Enabled로 등록했습니다. 이 등록은 현재 0.9.3 앱의 반복 실행 활성화를 뜻하지 않습니다. 폴더명으로 origin을 추측하지 않고 실제 원본/원격 identity를 대조합니다. 기존 dirty·index·clone·Git lock·설정·공지·원본 outbox는 보존합니다. 이 기능을 기존 clone 복구나 새 Git mirror 우회로 사용하지 않습니다.

구현 계약의 branch는 `ai-records/<소문자 ProjectId>-<64hex batchHash>`, 파일은 허용 prefix 아래 `<불투명 recordKey>/r<revision>.json`입니다. 동일 identity/revision 파일은 불변이며 다른 내용으로 덮어쓰지 않습니다.

## 성공을 구분하기

| 단계 | 뜻 |
|---|---|
| local_collected | 독립 D 영역에 로컬 수집됨; 중앙 공유 아님 |
| branch_uploaded | 해당 별도 branch 업로드 확인; PR·merge 아님 |
| pr_open | draft PR 존재 확인; merge 아님 |
| already_present | 같은 구조 기록이 이미 있음; 중복 업로드하지 않음 |
| pr_closed | 기존 PR 닫힘 확인; 자동 재생성·merge로 간주하지 않음 |
| pr_merged | 별도 실제 merge 확인; publisher가 자동 merge한 것이 아님 |
| held / empty | 조건 미충족으로 보류 / 공유할 구조 기록 없음 |
| upload_uncertain / pr_uncertain | 응답 불명확; 읽기 전용 원격 대조가 필요 |

응답 불명확 시 branch 내용·기록 hash·PR을 읽어 재확인합니다. 확인 전 무작정 재시도하거나 같은 branch/PR을 중복 생성하지 않습니다. 기록의 작업 상태 completed, 로컬 수집, 업로드, PR 생성, merge, 공지 읽음/적용은 서로 다른 증거입니다. backend의 중앙 공유 여부도 branch 업로드만으로 true로 바꾸지 않습니다.

`local_collected`는 수집 단계 설명입니다. publisher 상태와 합치지 않고 최종 merge도 실제 원격 증거가 있을 때만 표시합니다.
