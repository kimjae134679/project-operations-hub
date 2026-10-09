# Threads 공통 외부 전달 규격 — 공개 문서용 초안

확인일: 2026-10-10(KST). 목적: 수집 → 이미지·문안 제작 → 최종 검토 → Buffer 담당이 기존 파일을 그대로 이어 읽게 한다. 새 운영 스키마나 전송 API를 도입하지 않는다. `contract.example.json`은 기존 파일별 가짜 예시를 한 문서에 모은 것으로, 통째로 어떤 앱에 import하는 파일이 아니다. 실제 자료·이미지·문안·계정·승인을 담지 않는다.

최신 사용자 지시 반영: **기존 게시물과 예약20건을 유지하고, 올릴 것을 먼저 올린 뒤 레이아웃을 수정한다.** 즉시 게시 테스트는 양 플랫폼 각각5건을 마무리하도록 요청됐다. 보류된 기존 예약이 있으면 먼저 provider 상태를 조회하고 원래 미래 시각과 provider ID를 보존해 복원하도록 요청됐다. 보충 자동화 재활성화도 지시됐다. 이 문서 작성자는 게시·예약 복원·자동화 실행을 하지 않았으며, **지시와 실제 실행 확인을 구분한다.** 기존 자산·판정을 조용히 덮어쓰거나 통합 개선 때문에 현재 게시를 새로 차단하지 않는다.

이전 지시 이력(현재 기준으로 사용하지 않음): 실제 게시에서 정사각 표지의 세로 프로필 격자 좌우 잘림과 캐러셀 본문 글씨 잘림이 발견되어 제작 프로그램 우선 수정·전체 준비 이미지 재출력, 새 게시/예약 보충 일시 보류, 기존 미게시 예약의 가역적 보류가 요청됐었다. 이 중 **게시·예약 보류 우선 조건은 위 최신 게시 우선 지시로 대체됐다.** 실제 provider 보류가 적용됐는지는 별도 확인이 필요하다.

제작·검토·통합 개선은 게시 진행과 별도 흐름으로 계속한다. 새로운 세로 render는 검증을 마친 뒤 구버전과 연결해 반영하며, 구버전·원본·이미 게시물을 삭제하지 않는다. 사용자가 승인한 레이아웃 한정 판정 승계 조건은 유지하고 의미 변경은 재검토한다. 최종 지원 비율·크기·안전 여백은 제작 담당의 검증 결과가 필요하다.

## 경계와 작성 소유권

| 단계 | 단일 작성자와 소유 데이터 | 다음 단계가 하는 일 |
|---|---|---|
| 수집·검증 | 수집/검증 담당: 기존 candidate ID, 원제목, 원출처, 사실·권리 근거 | 원래 ID와 원문 근거를 읽고 보존한다. 뒤 단계에서 사실·평가를 바꾸지 않는다. |
| 이미지·문안 제작 | 제작 담당: `status.json`, 각 outputFolder의 `production-plan.json`, `publish-captions.json`, rendered 이미지 | 검토 모듈이 읽고 검증한 뒤 자신의 자산 저장소로 가져온다. 원본은 쓰지 않는다. |
| 최종 검토 | 검토 담당 검토 역할: `.local/state.json`, `.local/final-review-decisions.json`, immutable handoff | Buffer 담당은 현재 유효한 통과만 읽는다. 사용자 점수는 통과 판정으로 바꾸지 않는다. |
| Buffer 결과 | Buffer 담당 Buffer 역할: 자기 예약/게시 결과·승인 연결 기록 | 통합 앱은 플랫폼별 결과를 읽는다. 검토 파일·제작 원본을 고치지 않는다. |
| 통합 표시 | 통합 담당: 등록 진입점·명시적 입력 경로와 읽기 결과 | 각 소유자의 자료를 다시 읽어 표시한다. 예약/게시/새 승인 권한을 얻지 않는다. |

실제 제작 기준 경로: `<config.materialRoot>/06_자동 제작 결과`. 실제 검토 모듈: `<config.studioRoot>`, 화면 `<config.reviewServiceUrl>`. 경로 등록은 원본 이동을 뜻하지 않는다. 예전 설치·포인터·사용안내와 현재 소유자의 최신 안내를 구분한다. 등록 설정은 실제 값을 게시하지 말고 아래처럼 가짜 경로와 역할 참조를 사용한다.

```json
{"materialRoot":"C:/EXAMPLE_ONLY/materials","studioRoot":"C:/EXAMPLE_ONLY/review","sourceRoot":"C:/EXAMPLE_ONLY/source","reviewServiceUrl":"http://127.0.0.1:9999","owners":{"production":"production-role","review":"review-role","buffer":"buffer-role"}}
```

위 객체는 설정 참조의 설명 예시이며 실제 통합 앱의 등록 스키마로 제출하지 않는다.

## 기존 파일을 잇는 공통 식별자

| 의미 | 제작 파일 | 검토 상태/판정 | approved handoff / Buffer 결과 |
|---|---|---|---|
| 글 ID | `entries[].id`, captions `postId` | `posts[].post_id`; 결정 파일 `postId` | `postId` |
| 제작 버전 | 계산된 `outputVersion` | `output_version` | `outputVersion` |
| 파일 형식 버전 | captions `schema:"threads-publish-captions-v1"` | 결정 파일 `schema:1`; 상태는 기존 domain 계약 | handoff `schema:1,type:"threads-final-review-handoff"`; provider 결과 `schema:1` |
| 원제목 | status `title`; captions `originalTitle` | `source.original_title` | `source.original_title` |
| 게시용 제목 | plan `coverTitle`, captions `publicationTitle` | `source.cover_title/display_title`, `publication_title` | Threads `captions.threads`는 현재 실제 표지 제목 |
| Instagram 문안 | `platformCaptions.instagram` | `platform_captions.instagram`, legacy `caption` alias | `captions.instagram`는 태그를 붙인 최종 문안 |
| 태그 | captions `topicTags`, `threadsTopicTag` | `common_tags/topic_tags/threads_topic_tag`; legacy `tags` | `tags.common/topic/legacy/threads_topic` |
| 순서·바이트 | status `images[].name/sha256` | `images[].asset_id/order/mime` | `images[].order/sha256/mime/local_file/approved_image_url` |
| 검토 판정·버전·메모 | 과거/현재 제작 평가와 별도 | 결정 파일 `decision/outputVersion/fingerprint/reviewedAt/note/noteUpdatedAt`; 실제 최종 판정은 `final_review` | `review_statuses[].decision/current/reviewedRevision`; `approval.status/reviewedAt/fingerprint` |
| 플랫폼별 예약·게시 | 작성하지 않는다 | 로컬 jobs는 외부 예약 증거가 아니다 | results `platform/providerPostId/status/scheduledAt/publishedAt/externalUrl/providerVerifiedAt` |

제작 버전 계산은 현재 코드와 같은 순서로 `SHA256(JSON.stringify([sourceFingerprint, outputSha256, ruleVersion, images.map(i=>i.sha256), ...(reviewRound ? [reviewRound] : [])]))`를 사용한다. 임의 증가 숫자나 파일 수정시각으로 대체하지 않는다. `reviewRound`는 truthy일 때만 포함된다. 문안 identity는 글 ID·이 버전·sourceFingerprint·원제목·원출처가 현재 status와 모두 일치해야 한다.

### 내용 버전과 렌더 버전 — 기존 계약과 추가 구현 구분

공통 규격의 의미에서는 **content version**(원문 기반 의미·제목·문안·내용·이미지 구성/순서의 기준)과 **render version**(같은 내용을 그리는 비율·안전 여백·줄바꿈·레이아웃·파일 바이트의 기준)을 구분해야 한다. 그러나 확인한 기존 스키마에는 두 버전을 독립적으로 받는 필드가 없다. 현재 `outputVersion`은 `outputSha256`, 렌더 이미지 해시 및 ruleVersion 등을 포함하므로 렌더만 달라져도 바뀐다. 이 사실을 유지하며 기존 `outputVersion`을 내용 버전이라고 재명명하거나 계산에서 이미지 해시를 빼지 않는다.

제작 담당·검토 담당·Buffer 담당이 실제 지원 필드와 검증을 합의한 뒤에만 별도 내용/렌더 버전 연결을 적용한다. 필요한 연결 의미는 동일 내용 근거, 이전/새 outputVersion, 순서별 이전/새 이미지 해시, 렌더 규칙, 변경 범위, 사용자의 명시 승인 출처, 기존 판정 fingerprint, 현재 자산 검증 결과다. 여기서는 임의의 새 운영 필드명이나 스키마를 확정하지 않는다.

양 플랫폼은 같은 글 ID·같은 원본 버전·같은 원본 이미지 해시의 순서 쌍으로 다룬다. Instagram 문안과 Threads 표지 제목이라는 플랫폼별 표현 차이는 유지한다. 플랫폼 제한 때문에 일부 이미지를 빼거나 순서를 바꿔 성공시키지 않는다. 한 플랫폼만 성공했으면 그 결과를 보존하고 다른 플랫폼은 대기/실패로 표시한다. 둘 다 `scheduled`/`sent`인지 별도로 계산한다. 현재 domain과 Buffer adapter는 플랫폼별 동작을 지원하지만 **쌍 단위 원자적 게시/전 플랫폼 성공 강제는 구현돼 있지 않다**. 통합 표시/담당 실행 계약에서 이 조건을 별도 확인해야 한다.

## 내부 자료와 공개 전송 필드

PC 원제목·원문·근거·내부 검토 메모·판정 기록·원본 이미지와 음원은 기존 로컬 위치에 둔다. `source`, `review`, `safety`, `blockers`, `local_file`, `consumer_contract.state_file`을 통째로 외부 API나 공개 GitHub에 보내지 않는다. handoff에는 이 내부 자료가 포함될 수 있으므로 **handoff 전체는 공개 파일이 아니다**.

Buffer에 보내는 필드는 담당자가 선택한 실제 최종 `text`, 승인된 공개 자산 URL의 순서 배열, 지정 채널, 지정 시각, 필요한 플랫폼 metadata다. API는 로컬 파일 업로드를 받지 않는다. 승인된 게시 이미지만 기존 Cloudinary 전달 경로의 직접·지속 가능한 HTTPS URL을 사용한다. Library 재복사나 새 저장소/계정/권한에 의존하지 않는다. [Buffer 공식 media 계약](https://developers.buffer.com/guides/hosting-media.html).

GitHub에는 검증된 코드·테스트·이 문서·가짜 예시만 공유한다. 저장소 공개 여부와 기존 기능 브랜치를 먼저 확인해야 한다. 이 공개 문서 초안은 아직 GitHub에 게시되지 않았다. 실제 데이터 JSON, 내부 승인 파일, 이미지, 음원, 키/토큰은 공개 GitHub 대상으로 삼지 않는다.

## 검토와 승인 연결

현재 최종 판정은 `unreviewed/discard/revise/passed`다. 통과는 현재 output version, content basis, active 여부, reviewed revision/time이 맞을 때만 유효하다. 문안·태그·원제목/표지 제목·이미지·순서·플랫폼·계정·시각·권리/안전 상태를 바꾸면 통과와 기존 dry-run/publication approval이 무효화된다. 과거 평가나 예전 immutable handoff가 현재 통과를 대신하지 않는다.

handoff 소비 직전 최신 state revision, handoff ID, eligible 글 ID, 검토 fingerprint와 바이트 SHA-256/MIME를 재확인한다. `.local/final-review-decisions.json`의 메모를 점수나 승인으로 승격하지 않는다. 검토 통과·호스팅 승인·실제 예약/게시 승인은 각각 별도다.

현재 운영 Buffer 기록에는 제목 장식만 수정하도록 한 `explicit-title-edit-authorization-20261010.json`(schema 1, type `explicit-user-title-decoration-edit`)과 `titleEditBindings`가 있다. 원래 통과 outputVersion/fingerprint와 현재 버전/fingerprint, 플랫폼, providerPostId, authorizationFile, verifiedAt가 연결돼 있다. 이것은 검토 상태를 다시 통과로 덮어쓰는 계약이 아니다. 명시 승인한 필드·대상·변경만 허용하며 이미지 변경 등으로 범위를 확장하지 않는다. generic domain에는 임의의 한정 편집 승인을 처리하는 필드가 없으므로 **다른 변경에 재사용하려면 담당자의 별도 승인 연결 계약 확인이 필요하다**.

최신 사용자는 **레이아웃 보정만** 기존 검토 판정을 승계하도록 명시 승인했다. 따라서 세로 규격·안전 여백·줄바꿈 등의 승인된 보정이 의미·문안·내용·이미지 구성/순서를 바꾸지 않았다는 근거와 이전 판정→새 렌더의 승인 연결을 보존해야 한다. 의미 변경은 계속 재검토 대상이다. 현재 코드의 기본 동작은 렌더 해시 변경도 판정을 무효화하므로, 이 승계를 이미 지원한다고 보고하거나 판정 파일을 직접 덮어쓰지 않는다. 검토/제작/Buffer 담당의 호환 계약·검증 구현이 필요하며, 기존 제목 장식 승인 파일을 레이아웃 보정 권한으로 재사용하지 않는다.

## 경로·파일명·MIME와 입력 오류

제작 outputFolder는 `06_자동 제작 결과` 아래에서 해석하고, 이미지 name은 outputFolder 상대 `rendered/slide-001.png` 형태다. `\`는 검사 시 `/`로 정규화한다. 실제 경로/realpath가 해당 root 아래인지 확인하고 탈출·외부 junction을 거절한다. 임의 파일 검색으로 누락된 경로를 추측하지 않는다. 한글 파일명은 UTF-8 JSON 문자열로 보존하며 OS path API를 사용한다.

검토 자산 파일은 `.local/assets/<64자리 sha256>`처럼 확장자가 없다. 확장자를 덧붙이거나 파일명을 MIME 근거로 쓰지 않는다. `.json` metadata의 MIME와 실제 JPEG/PNG/WebP magic bytes 및 해시를 함께 확인한다. approved handoff의 local_file은 검증된 절대 경로이며 approved_image_url이 null이면 아직 공개 URL이 없는 것이다.

현재 제한: 제작 JSON 파일 20,000,000 bytes 이하, entries 10,000 이하, 글 ID 1~200자, 중복 ID 거절, 글당 원본 image 200 이하, 이미지 파일 25MiB 이하. 제작 selection은 1~20개, 중복 ID 불가. 제작 PNG 이름·64자리 소문자 SHA-256·실제 바이트 일치가 필요하다. captions는 두 플랫폼 문자열, 정제 publicationTitle, Instagram 첫 문단 `[ 제목 ]`와 본문 3~5문단이어야 한다. topicTags는 없거나 서로 다른 2개, Threads topic은 #/줄바꿈 없는 50자 이하다. 제한은 현재 로컬 코드의 입력 제한이며 provider 허용과 같다는 뜻은 아니다.

`production_version_changed`, `production_image_changed`, `production_title_contract_changed`, `production_caption_identity_changed`, `handoff_asset_hash_mismatch`, `handoff_asset_mime_mismatch`는 해당 글 보류 근거다. 누락 captions는 `awaiting-authored-captions`, 잘못된 captions는 `held-...`; 원문을 추측해 생성하지 않는다. stale CAS는 `revision_conflict`(409), 같은 버전 재import는 `same_version_import_conflict`, 중복 글은 `duplicate_post`다. 현재 파일·버전을 다시 읽고 담당자가 해결한다.

## 레이아웃 재출력과 파생 게시 이미지 — 승인된 방향, 아직 계약 구현 필요

사용자가 원본 내용을 자르지 않는 최소 상하 여백 파생 사본과 제작 최소 높이를 승인했다. 제작 코드 담당은 제작 역할, 실제 자산 보정·전송은 Buffer 담당이다. 원본 바이트/원본 경로/원본 제작 버전은 보존한다. 전송용 파생 파일은 별도 경로와 해시를 갖는다.

이후 실제 표지·본문 잘림이 발견되어 제작 프로그램 수정과 준비 이미지 전체의 세로 규격·안전 여백·줄바꿈 재출력이 요청됐다. 최신 지시는 기존 게시·예약을 유지하고 게시를 먼저 진행하면서 이 개선을 별도로 수행하는 것이다. 검증된 새 render만 구버전과 연결해 반영하고 원본·구버전을 삭제하지 않는다. **최종 지원 비율·출력 크기·안전 영역 수치는 제작 담당의 실제 도구/플랫폼 검증 결과를 받아 명시해야 한다.** 프로필 격자 미리보기, 피드 본문, 캐러셀 표시, Buffer 경로를 같은 비율로 가정하지 않는다. 이 문서는 최종 세로 규격의 숫자를 임의로 정하지 않는다.

현재 production/status 이미지 및 approved handoff 이미지 필드에는 원본↔파생 해시, padding량, 파생 승인 연결 필드가 없다. 기존 필드에 파생 해시를 몰래 원본 해시처럼 넣으면 승인 근거가 달라진다. **담당자 합의한 별도 sidecar 또는 호환 가능한 추가 필드·소비자 검증 구현이 필요하다.** 필요한 기록 의미는 원본 경로/해시/크기, 파생 경로/해시/MIME/크기, top/bottom/left/right padding, no-crop 변환 방식, 승인 출처/대상/시각, 원래 통과 version/fingerprint, 플랫폼별 결과다. 여기서는 새 필드 이름/스키마를 운영 계약으로 만들지 않는다.

기존 1080×552 오류 분석의 참고: 확인 당시 Buffer 공식 문서는 Instagram 이미지 범위를 `0.75 ≤ width/height ≤ 1.91`로 설명하고 경계 반올림을 허용하지 않았다. 그 1.91 경계만 계산하면 `ceil(1080/1.91)=566`이며 565px은 범위 밖이다. **566px은 특정 비율 오류의 수학적 참고 경계일 뿐, 이번 전체 재출력의 최종 제작 규격·세로 안전 영역·프로필 격자 무잘림 기준이 아니다.** 최종 지원 조건은 제작 담당 검증 결과가 필요하다. 실제 보정·재전송 성공도 이 계산으로 확인되지 않는다. [당시 확인한 Buffer 공식 비율 문서](https://support.buffer.com/articles/instagrams-accepted-aspect-ratio-ranges-Frc2Xqewbd).

Instagram 캐러셀은 첫 장 비율에 맞춰 뒤 이미지를 crop할 수 있다. 그래서 각 장의 최소 높이만 맞추는 것으로 전체 콘텐츠 무잘림을 보장하지 못한다. 한 글의 모든 장이 같은 canvas 비율에 들어가는 padding을 함께 검증해야 한다. Threads에 Instagram 경계나 캐러셀 crop 규칙을 동일 적용하지 않는다. [Buffer 플랫폼별 이미지 조건](https://support.buffer.com/articles/ideal-image-sizes-and-formats-for-your-buffer-posts-JxHNGZFvf9).

확인한 Buffer 도움말은 Threads 4장, 현재 local domain은 20장을 명시하여 제한 정보가 불일치한다. 이 차이를 임의로 통일하거나 분할하지 않는다. 실제 담당자가 사용하는 Buffer connector/API의 현재 제한과 응답을 확인해야 한다. Meta 공식 Instagram/Threads 문서는 이 실행환경 web 도구에서 직접 열리지 않았으므로 추가 숫자를 확정하지 않았다. 로컬 readiness는 Instagram JPEG만 요구하지만 Buffer 도움말은 PNG 등도 지원하므로 경로별 검증이 다름을 보존한다.

## 원자적 반영·중복·실패 후 재개

현재 StateStore는 소유 lock + CAS, 이전 state backup, 임시 파일 exclusive 생성 → write → fsync → rename 순서다. 복구 시 과거 통과/approval을 초기화하고 job을 stale로 둔다. approved handoff는 임시 파일→fsync→hard-link로 immutable 새 파일만 노출한다. 같은 semantic ID 재사용은 허용하고 다른 내용의 충돌은 `handoff_collision`로 거절한다. producer captions의 temp+atomic rename는 기존 문서의 생산자 계약이며 이번 문서 담당자가 실제 생산자 쓰기 수행을 시험하지 않았다.

LocalAssets는 해시 파일 exclusive 쓰기를 하며, 바이트+metadata 두 파일 전체를 하나의 transaction으로 쓰는 것은 아니다. 소비 시 둘 다 검증하므로 불완전 자산을 통과시켜서는 안 된다. 플랫폼별 Buffer reservation은 durable 중복 fence를 먼저 저장한다. provider idempotency를 가정하지 않는다. 응답 유실/부분 실패는 reconciliation으로 두고 providerPostId를 대조한 후 담당자가 재개한다. 이미 성공한 플랫폼을 재전송하거나 빈 queue를 전체 성공으로 해석하지 않는다.

## 실제 확인 범위와 남은 조율

- 실제 로컬 production/검토/handoff/Buffer adapter 코드와 운영 결과 JSON의 **필드명·개수·상태 종류만** 확인했다. 실제 문안·내부 메모·원본·비밀값은 예시에 사용하지 않았다.
- 예시는 실제 productionVersion/validateProductionCaptions 순수 함수로 검증한다. 예시 승인·결과는 실행 권한이 없고 실제 제출하지 않는다.
- 실시간 앱 재읽기, 최신 설치 반영, 양 플랫폼 쌍 gating, 파생 자산 sidecar, 실제 provider 제한·자산 보정 성공은 별도 구현/검증 결과가 필요하다.
- content/render 독립 버전 필드 및 명시 승인된 레이아웃 보정의 판정 승계 연결은 기존 스키마 미지원으로 추가 계약·구현이 필요하다. 최종 세로 출력 규격은 제작 담당 검증 대기다.
- 최신 요청은 기존 게시물·예약20건 유지, 즉시 테스트 양 플랫폼 각각5건 마무리, 보류된 예약의 조회 후 원래 미래 시각 복원, 보충 자동화 재활성화다. 실행 확인은 Buffer 담당 결과를 별도로 받아야 한다. 이전 보류 조건을 현재 차단 근거로 사용하지 않는다.
- 현재 `BUFFER_CONTRACT.md`는 offline adapter 설명이다. 운영 proof는 별도 Buffer 담당의 외부 작업 결과이므로 local API가 live라고 추론하지 않는다.
- 프로젝트 폴더 자체에 AGENTS/04/contracts가 없었다. 사용안내가 지시하는 소스 `review-improvements-20261008-Sol/AGENTS.md`, `docs/HANDOFF_CONTRACTS.md`, `04_REVIEW_PUBLISH/README.md`를 확인했다.

## 코드 근거

모듈 root `<config.studioRoot>` 기준:

- `production-input.mjs:10` 버전; `:13-16` root/경로/JSON/중복; `:20-21` captions/title; `:24-32` 순서·해시 재확인.
- `production-captions.mjs:5-17` schema, identity, 문안·태그 검증.
- `domain.mjs:6` 로컬 플랫폼 제한; `:21-30` 이미지/MIME/원제목/Threads 제목; `:39` basis; `:93-114` 편집과 승인 무효화; `:132` 최종 문안.
- `final-review.mjs:4-28` 판정·revision·현재 버전 유효성.
- `review-handoff.mjs:25-35` projection identity; `:90-104` 해시·MIME; `:120-132` 전달 필드·소비자 재검증; `:137-162` immutable 원자적 저장.
- `store.mjs:75-77` backup/원자 저장/복구; `local-assets.mjs:6-15` 내용기반 ID와 exclusive 자산 저장.
- `buffer.mjs:30-49` 플랫폼별 exact input/basis; `:72-104` 중복·reservation; `:111` 이후 provider 관측과 reconciliation.

운영 read-only schema 확인: `.local/final-review-decisions.json`, `.local/provider-delivery-log/verified-buffer-schedule-20261010.json`, 같은 폴더의 `explicit-title-edit-authorization-20261010.json`. 문서·예시 GitHub 링크는 부모 작업이 저장소 공개 여부 및 기존 기능 브랜치에서 검증·게시한 후 제공한다.


## 2026-10-10 운영 기록과 최신 담당 계약

이 절은 검토 담당이 최종 코드 `4b2fbbea604da37115f23c340a0e39138bd7b3e3`의 계약을 전달한 뒤 갱신했다. 검토 담당의 소유 문서는 [FINAL_REVIEW_HANDOFF.md](https://github.com/kimjae134679/Threads/blob/feature/upload-studio-offline-20261009/upload-studio/FINAL_REVIEW_HANDOFF.md), 코드 검토 진입점은 [Threads draft PR 10](https://github.com/kimjae134679/Threads/pull/10)이다. 담당이 보고한 Actions 일반 검사 37990979994, Windows 잠금 복구 37990979922, Linux Chromium 37990979875는 통과했다. 통합 담당이 이 검사를 재실행한 것은 아니다. 최신 전체 코드의 운영 PC Node+Chromium 재검증과 설치는 미완료다. 마지막 확인 운영 코드는 `ab3528cdbaeaf53d878ba0a3504fa38fe78f6350`이다. 저장소 코드와 운영 설치 버전을 구분한다.

### 읽기 계약과 소유권

검토 코드·판정·문안·메모·state의 작성자는 검토 담당이다. 통합 담당은 앱 틀을 유지하며 외부 등록과 상태를 읽는다. Buffer 담당만 플랫폼 전송 결과를 기록한다. 통합 쪽에서 판정이나 원본·자산·예약을 덮어쓰지 않는다.

`GET /api/state`의 state revision을 읽고 `GET /api/final-review/handoff`의 현재 state_revision/handoff_id/latest를 읽은 뒤 state를 다시 읽는다. 앞뒤 revision, thin `.local/final-review-decisions.json`의 revision, handoff revision 및 현재 지문이 모두 같을 때만 최신 전달본으로 등록할 수 있다. `latest.pending=true`면 이전 고정 파일을 최신으로 쓰지 않는다. 운영 구버전에 pending 필드가 없는 경우 false로 추정하지 말고 현재 revision·handoff identity·파일을 확인한다. 고정 파일명은 `.local/final-review-handoff/handoff.<revision>.<handoffId>.json`이다. snapshot은 자동 게시 승인이 아니다. 새 코드의 자동 thin 갱신·최신 handoff 생성 보강은 아직 운영 설치 전이다. 상태 표시 reader는 handoff를 등록하거나 생성하지 않는다.

제목 wrapper-only 승인은 검토 담당의 정확한 현재 판정·버전·지문 연결로 처리한다. 원래 reviewed_at/reviewed_revision과 이전 final_review를 보존하며 state의 `posts[].final_review.prior_final_review`와 `edit_approval`을 읽는다. 구버전 basis는 현재 내용으로 독립 재계산해 저장 basis 및 승인 fingerprint와 같아야 인정한다. 별도 source/safety/rights 검사를 통과로 만들지 않는다. 앞 절의 generic 승인 미지원 설명은 구 운영본 확인 시점의 제한이며, 이 최신 소유자 계약과 설치 여부를 함께 확인한다.

### 정식 Buffer 결과 소비

정식 입력은 `.local/final-review-results/*.json` 개별 schema1 기록이다. 필드는 `postId, outputVersion, fingerprint, platform, providerPostId, status, externalUrl, providerVerifiedAt, publishedAt, scheduledAt, recordedAt`이다. 현재 정확한 postId·outputVersion·fingerprint·선택 플랫폼에만 연결한다. 저장된 관측 이력에서 같은 글/플랫폼의 최신 recordedAt를 사용하고 같은 시각의 충돌이나 동일 플랫폼/providerPostId의 다른 글 귀속을 거절한다. 옛 날짜의 provider 요약은 과거 증거로 따로 보존한다.

`scheduled/draft/sending`은 완료가 아니다. `sent` 집계에는 providerPostId, provider 재조회 확인 시각, 실제 publishedAt, 자격정보 없는 HTTPS 게시 링크가 필요하다. 통합 reader는 담당자가 저장한 근거를 검사하고 provider를 독립 재조회하지 않으므로 실제 상태의 현재성은 Buffer 담당 확인이 필요하다. 선택 플랫폼이 모두 sent면 posted/게시완료, 일부만 sent면 partially_posted/일부게시다. 원래 사용자 passed 감사기록은 보존한다. 등록 표시 값 `threads.currentResults.sent/posted/partiallyPosted`는 이 정확한 현재 버전 결과 집계이며 새 전송 권한이 아니다.

레이아웃 한정 승계는 사용자가 승인했으나 제작 담당의 검증된 최종 매핑 계약은 대기 중이다. 필요한 근거는 이전 content/output 버전·지문, 별도 renderVersion/producerBuild, 원문·본문·플랫폼 문안·태그 의미 불변 증거, 순서별 기존↔새 자산 고정 참조·SHA256·MIME·크기·규격·검증 시각, layout-only 승인 사유다. 확정 전 이를 추측하여 state/판정/현재 자산을 재작성하지 않는다. 기존 원본·게시물·예약·판정을 보존하고 삭제하지 않는다. 새 렌더는 검증 후 구버전과 연결하며 의미 변경은 재검토한다. Buffer의 기존 게시·예약 작업을 통합 작업 때문에 중복 실행하거나 조용히 차단하지 않는다.

### 확인한 원인, 수정, 검증과 남은 단계

첫 reader는 날짜가 지정된 provider 요약만 읽어 정식 결과 폴더의 이후 관측을 표시하지 못했다. 정식 결과 소비를 추가하고 현재 버전·지문·플랫폼 대조, latest recordedAt 선택, sent 근거 검사와 플랫폼별 완료 집계를 넣었다. 과거 통과를 현재 통과로 인정하지 않으며 원문을 추측하지 않는다. 최신 Voice 듣기 등록 포인터를 우선 읽고 전체 제작 완료율로 바꾸지 않는다. 누락·잘못된 JSON·symlink·경쟁 중 source 변경은 빈 성공 값으로 바꾸지 않는다.

검증 이력: 신규 소비 테스트가 처음 4개 실패했고 구현 후 통과했다. 독립 검토에서 providerID 중복 귀속 과대 집계를 재현했으며 실패 테스트를 먼저 추가하고 수정했다. 최종 Node 15/15 PASS. 앞선 desktop generic 소비/기존 catalog 보존 검사 50/50 PASS는 기존 계약 검증이며, 새 reader의 운영 화면 픽셀 검사는 아니다. UTF-8 문자열 공유에서 한글 바이트 불일치를 발견해 base64 원본 바이트로 수정했고 Git blob 해시를 대조했다. PC 실제 등록·원문·평가·판정 원본·이미지·음원·비밀값은 공개 저장소에 넣지 않았다.

현재 ControlTower 0.9.18 수동 모드는 시작할 때 DiscoverAsync를 수행하고 선택된 외부 상태를 갱신한다. 일반 timer의 manifest 재탐색이 수동 모드에서 실행되지 않는다는 것을 코드에서 확인했다. 새 등록 파일이 이미 열린 앱 메모리에 들어왔는지와 실제 표시 픽셀은 미확인이다. 실행 중 앱/producer/Buffer를 재시작해 확인하지 않았다. 현재 환경의 native UI 도구는 비활성이며 이 제한은 승인 거절이나 앱 장애의 증거가 아니다. 원래 첨부 materialize 403도 자료 접근 실패로 별도 기록했다.

남은 단계와 최소 조건: (1) 허용된 native UI/앱 목록 읽기 수단에서 현재 창의 새 등록 반영을 확인한다. 기존 목록 다시 읽기 한 번만 필요하다면 요청 문구는 “현재 앱의 기존 프로젝트 목록 다시 읽기 1회, 새 인증·종료·게시 변경 없음”이며 도구 비활성은 사용자 승인만으로 풀리지 않는다. (2) 검토 담당이 최신 코드의 PC 검증·운영 적용을 마친다. (3) 제작 담당이 승인된 레이아웃 불변·자산 매핑 근거를 전달한다. 추가 포괄 승인을 요구하지 않고 이미 승인된 범위에서 담당자가 구현·검증한다.

재발 방지: 작성 소유자를 하나로 유지하고 읽은 revision/지문·출처 시각을 구분한다. 저장된 결과와 provider 현재 상태, code/CI와 PC 설치, 등록 파일과 실행 중 UI 반영을 각각 확인한다. 고정된 옛 파일로 현재를 추정하지 않는다. 공개 공유는 허용 목록과 바이트 검증을 통과한 코드·테스트·문서·가짜 예시만 사용한다. 다른 담당의 기록은 본인 프로젝트 링크와 요약으로 연결하고 공통 계약은 통합 담당이 관리한다.
