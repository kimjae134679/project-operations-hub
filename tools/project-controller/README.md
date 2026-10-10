# 공통 규칙 컨트롤러

공통 권위는 기존 [project.catalog.json](../../project.catalog.json)의 `controller`와 각 프로젝트 `integration`이다. 기존 [presentation 규칙](../../01_CONTROL/PROJECT_PRESENTATION_RULES.md)을 유지하며, 추가 공통 계약은 이 문서와 catalog에 둔다. 별도 master registry를 만들지 않는다. 각 프로젝트의 `project.control.json`은 담당이 작성한 현재 실행·입출력 계약이며, catalog의 과거 기능·명령보다 우선한다.

검증기는 운영을 변경하는 실행기가 아니라 읽기 전용 gate다. 경로·Git·원본 SHA256·계약·잠금·완료 증거가 불명확하면 보류하며 서버, 제작, 게시, 예약, 인증을 실행하지 않는다. stdout에는 프로젝트 ID·상태·오류 코드·수만 출력한다. 실제 PC 위치·실행 상태·개인 자료·증거 파일은 공개 문서에 넣지 않는다.

```text
node tools/project-controller/validate.mjs --catalog project.catalog.json
node tools/project-controller/validate.mjs --catalog project.catalog.json --local
node --test tools/project-controller/validate.test.mjs
dotnet run --project tools/project-controller/manifest-tests/ManifestTests.csproj
```

구조 검사 성공은 `registered`이며 PC 적용 완료가 아니다. 로컬 검사는 catalog의 source/operating/data/build/cache/backup 역할을 확인한다. sourceCommit·expectedArtifacts·lockPaths·runtimeStatus·계약 파일 해시를 미리 고정하고, 프로젝트 담당의 비공개 `.local/project-controller.json`과 실제 바이트를 대조한다. 이 receipt는 관찰 증거이며 별도 registry가 아니다.

| 공통 catalog 필드 | 검증 의미 |
|---|---|
| `owner`, `writable`, `forbidden`, `dependencies` | 공통 담당과 프로젝트별 담당의 수정 범위 및 선행 작업 |
| `roles`, `repository`, `sourceRef`, `sourceCommit` | 소스·운영·자료·빌드·캐시·복구 역할과 실제 Git 기준 |
| `expectedArtifacts[{role,path,sha256,sourceCommit}]` | 현재 소스에서 검증된 운영 산출물의 고정 기대값 |
| `lockPaths` | 담당 계약에서 확정한 잠금 위치. 임의로 없는 잠금을 기재해 완료시키지 않음 |
| `runtimeStatus{role,path,checkedAtField,activeJobsField}` | 담당이 소유한 실제 상태 파일의 15초 이내 확인 시각과 작업 수 0 |
| `contracts[{owner,reference,role,path,sha256}]` | 고정 계약 링크와 로컬 계약 파일의 실제 해시 일치 |
| `requiresRevision`, `revisionStatus{role,path,field}` | 현재 revision과 receipt의 전후 revision 일치 |

receipt 필수 필드는 schemaVersion=1, projectId, owner, checkedAt(UTC), sourceCommit, complete, locks, artifacts, contracts다. 적용 파일의 sha256과 appliedSha256이 다르면 BOM/줄바꿈 정규화로 통과시키지 않는다. 15분을 넘긴 receipt, 소유자/commit 불일치, link/reparse, 임시 운영 의존, 잠금 미확인, 검사 중 HEAD·상태·revision·receipt 변경은 완료가 아니다. 종료 직전에도 실제 잠금·상태와 신선도를 다시 확인한다. 등록·코드 검사·PC 적용·native UI 검증은 서로 다른 증거다.

| 담당 | 수정 범위 | 선행 작업과 완료 조건 | 금지 |
|---|---|---|---|
| 통합 | 공통 catalog·규칙·검증기·catalog/discovery 서비스와 회귀 검사 | primary 하나, manifest의 기능·제어·상태가 재탐색 뒤 보존됨 | 다른 담당 원본/state/결과, 공유 큐, main merge, 실행 중 폴더·프로세스 |
| Threads 제작 | 본인 제작 소스·출력 계약·고정 SHA256/path mapping | checkpoint roots와 내부 절대 target/source_bundle를 함께 검증한 복사·복구 mapping을 검토 담당에게 전달 | 원본 삭제·강제 이동, 새 제작·재렌더, 판정, Hub 공통 파일 |
| Threads 검토 | 본인 검토 코드·연동 config·프로젝트 manifest·receipt | 제작 mapping 후 config 백업/hash CAS 전환; 현재 state·사용자 판정·메모·자산 보존; 자동 연동 및 전체 본문/이미지 보기 이동 연결 | 과거 미평가 상태로 초기화, 제작 원본, Buffer 결과, Hub 공통 파일, 게시·예약 |
| Buffer | 본인 로컬 소비 계약·결과 schema·오프라인 검사 | 정확한 현재 post/version/fingerprint/platform 연결; 구버전 결과 격리; 미완료 상태 구분 | API 한도 소진 상태의 호출, 게시·예약·새 인증, 판정/state, Hub 공통 파일 |
| 다른 프로젝트 | 본인 계약과 receipt | 기존 소스 하나와 경로 역할·담당 근거를 통합 담당에게 제출 | 별도 승인 없는 중지된 제작·서버 재개, 다른 담당 파일, 공통 정의 복제 |

기존 primary entry/relatedFolders 규칙은 유지한다. 화면 정리만을 위해 원본을 이동하지 않는다. 실제 운영 경로 정착은 담당의 SHA256/크기/경로 mapping과 복구책을 검증한 복사 및 소비 참조 전환으로 진행한다. 이미 안내한 위치가 바뀌면 대응표와 최종 사용자 안내를 함께 제공한다. 전체 보기와 검토의 최신 진입점은 담당 manifest로 확인하며 구버전을 최신으로 추측하지 않는다.

공통 코드 기준은 `341c5025c576a3df56f41988678eaf9112d382b2`이며 검증기 SHA-256은 `8561fcfeefa9045129249cf02b60fffc2b51a6ccbe73aaf3ccd44accd7d5fd33`이다. 이 catalog와 계약은 사용자에게 실제 경로·명령·연결 규칙의 공개 범위를 확인받아 별도 후속 커밋으로 추가한다. 공개 등록, 로컬 검증기 정착, 앱 전체 설치와 native UI 검증은 별개다. 기존 catalog에 controller가 없으면 검증기는 지원되지 않은 계약으로 보류한다.


## 접근 우선순위

GitHub 직접 연결 → 기존 승인된 ProjectBridge → 기존 cloud task의 Codex 로컬 파일/셸 → 허용된 원격 제어 순서다. 앞 경로가 지원하는 작업은 우선 사용한다. 사용자 PC는 기존 cloud task를 통해 접근하며 GitHub 파일만으로 PC 설치 상태를 판단하지 않는다. Desktop Commander/remote_desktop_commander는 읽기·상태조회 포함 사용 금지이며 마지막 원격 제어 순서는 재사용 허가가 아니다. 새 인증·권한 변경·거절·도구 한도 우회는 허용하지 않는다. 실행 전 setup refresh 초기화 실패의 원인이 미확인이면 앱 실제 오류와 구분한다. 중지 상태와 파일별 소유권을 유지하며, 이후 사용자의 명시적 1회차 승인 등 범위 변경은 해당 담당에게만 적용한다.
