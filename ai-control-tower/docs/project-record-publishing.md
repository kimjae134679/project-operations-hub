# 프로젝트 기록 공유

**상태: 0.9.5 소스 구현·코드 검사·실제 일회 headless 공유 확인.** 현재 0.9.3 앱의 반복 수집/공유는 아직 활성화하지 않았습니다. 0.9.5 publish는 대기이며 기존 미활성 0.9.4 스테이징과 운영 0.9.3 앱은 그대로입니다. GUI 교체나 사용자 화면 검증 완료를 뜻하지 않습니다.

## 실제 확인 · 2026-10-07

| 대상 | 실제 결과 | 남은 경계 |
|---|---|---|
| 허브 | 124건 로컬 수집·수집 오류 0, 구조 메타데이터 76개로 [draft PR #25](https://github.com/kimjae134679/project-operations-hub/pull/25) 생성 | draft/open/unmerged, main 변경 없음 |
| 허브 동일 실행 재확인 | GET 6회·POST 0회, 같은 PR 유지 | 업무 재실행·추가 업로드 아님 |
| Voice | 메타데이터 10개로 [draft PR #2](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/pull/2) 생성 | draft/open/unmerged; 기존 제작 작업 생존·무변경, 새 업무 실행 0 |
| Threads | 13건 수집·수집 오류 0, task JSON 3개 모두 검증 보류 | API 쓰기 전 중단; 원본 3개 hash 불변 |

확인한 PR head: 허브 `bdd7adcedae0f42b0d77c30a39a9d1ee5766d373`, Voice `e8b11994c53610add9bf2932ebbc66ac8caeb7d9`. 모델 호출 0, 자동 merge 0입니다. 실제 readonly GitHub 조회에서 허브 identity·base main을 대조했으며 인증/계정 권한을 바꾸지 않았습니다.

단계 표시 수정 후 부모 최종 fresh 코드 검사: .NET **509 PASS/0 FAIL**(`record-publishing-full-final.trx`), scripts **68 PASS/0 FAIL**, 17.897초(`record-publishing-full-green.log`). 최초 병렬 Git fixture 실패는 LocalExecutionPolicyTests를 비병렬 collection으로 옮긴 뒤 전체 재검사했습니다. 실제 PR 파일도 각각 메타데이터 76/10개 ADDED·삭제 0임을 확인했습니다. 이 검사는 운영 앱의 새 반복 경로 활성화를 뜻하지 않습니다.

Threads 2개는 completed에 남은 필수 작업/막힘이 함께 있었고 1개는 필수 필드 5개 누락·지원하지 않는 상태였습니다. parser 완화·가짜 완료·원본 자동 수정 없이 본인 작성자의 새 revision을 기다립니다. 보호 Bridge 설치 및 기존 clone Git lock 막힘도 그대로이며 이 PR을 해당 복구 완료로 표시하지 않습니다. 예약 PAUSED·review disabled/unmanaged를 유지합니다.

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
