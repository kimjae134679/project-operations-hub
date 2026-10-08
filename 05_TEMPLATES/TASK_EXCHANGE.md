# 받은 명령과 AI 답변 기록

AI는 작업 시작 때 받은 지시를, 진행·완료·중단 때 실제 답변과 결과를 이어 기록합니다. 다음 AI는 최신 버전과 실제 저장소를 함께 확인합니다.

사용자 화면에는 **받은 지시 → AI가 답한 내용 → 한 일 → 검증 → 남은 일** 순서로 요약합니다. AI가 읽는 공식 원본은 task_exchange JSON입니다. 같은 내용을 별도 Markdown 원본으로 복제하지 않습니다. 도우미 render가 JSON에서 사람용 요약을 만들어 출력합니다.

## 작성 순서

같은 요청의 진행·질문·차단·완료를 원글 댓글로 이어 소통하는 규칙은 [N-0007](../04_COMMUNICATION/announcements/notices/N-0007.md)을 따릅니다. 새로운 독립 주제만 새 글로 만들고 실제 수신·담당·검증·남은 일을 구분합니다. 댓글은 원글에 연결된 소통이며 이 JSON의 복제 원본이 아닙니다. 같은 recordId의 revision 갱신과 원본 이력 보존은 계속 필요합니다. 형식 댓글 자동 대량 생성·과거 기록 재작성·다른 담당자 읽음/수신 작성은 하지 않습니다.

1. [템플릿](TASK_EXCHANGE.template.json)을 작업용 임시 JSON으로 복사하고 본인 프로젝트·AI·세션과 실제 지시를 적습니다. 예시값을 실제 기록처럼 보내지 않습니다.
2. 시간에는 시간대를 포함합니다. receivedAt은 지시를 실제 받은 시각, updatedAt은 기록 갱신 시각입니다. 출처를 모르면 unknown으로 적고 채팅에서 받았으면 current_chat로 적습니다.
3. response에는 이미 전달한 답변만 적습니다. 아직 최종 답변 전이면 실제 진행 답변을 요약하거나 “최종 답변 대기”로 표시합니다. 답변 전 예상 문구를 완료 답변으로 기록하지 않습니다.
4. 아래 명령으로 검증한 뒤 보낼자료에 등록합니다. root는 실제 _통합소통 폴더입니다. Python 3.10 이상을 사용합니다. Windows에서 python이 실행 별칭이면 py -3.11 또는 확인된 인터프리터 경로를 사용합니다.

```text
py -3.11 기록도우미.py validate --input 작업기록.json
py -3.11 기록도우미.py record --input 작업기록.json
py -3.11 기록도우미.py render --input 작업기록.json
py -3.11 기록도우미.py latest --project 본인프로젝트ID --actor 본인AIID --record-id 본인기록ID
```

scripts/project_notice.py를 중앙에서 실행할 때는 --root 실제프로젝트/_통합소통을 명령 앞에 지정합니다. 템플릿 입력은 작업용이므로 최종 등록 뒤 그 회차가 만든 임시 입력만 정리합니다.

## 버전과 상태

- 같은 지시의 결과가 바뀌면 recordId·받은 명령·receivedAt을 유지하고 revision을 하나 올립니다. 이전 버전은 보존합니다. 같은 버전·동일 JSON 내용의 재등록은 파일 바이트·수정 시각·속성을 바꾸지 않고 already_stored로 반환하며, 다른 내용은 거부합니다. actorId와 projectId까지 같아야 같은 기록입니다.
- 새 사용자 지시는 새 recordId를 사용합니다. 이전 지시를 대체하면 새 기록 supersedes에 이전 recordId를 적고 이전 기록의 새 버전 상태를 superseded로 갱신합니다.
- pending: 아직 시작하지 못한 일. in_progress: 진행 중. completed: 요청 범위를 완료. blocked: 막힌 이유 필요. superseded: 새 지시로 대체.
- 진행·대기·막힘에는 nextActions를 적습니다. completed에는 남은 필수 작업이나 blocker를 남기지 않습니다. 일부만 끝났으면 in_progress 또는 blocked입니다.
- verification의 result는 pass/fail/not_run/partial입니다. 수행한 검증은 evidence에 명령·결과 파일·확인 근거를 적습니다. 코드·빌드·실사용·실기기를 구분합니다.
- 제목과 summary는 짧고 쉬운 한국어로 적고 details에는 제약·근거를 남깁니다. 모든 필드가 필요하며 배열은 없으면 []를 사용합니다.
- [JSON 스키마](TASK_EXCHANGE.schema.json)와 도우미가 필드·상태·시간·크기(1MiB)를 검사합니다. record는 소통함의 실제 프로젝트 ID와 이력도 확인합니다. validate는 입력 형식 검증이므로 저장·수집 성공을 뜻하지 않습니다.

보낼자료에 작성됨, 앱에 수집됨, 중앙에 공유됨은 별개입니다. 앱 종료·미연결·인증 문제는 공유 대기이며 규칙 공지나 도우미 설치만으로 다른 AI가 읽었거나 독립 채팅의 명령이 자동 기록됐다고 말하지 않습니다. 다른 AI를 대신 기록하거나 본인 답변을 추측해 만들지 않습니다. 오래된 기록은 실제 저장소의 현재 상태를 대신하지 않습니다. 비밀값·개인 대화 원문 전체는 넣지 않고 필요한 명령·답변을 요약합니다.

## 프로젝트 GitHub 공유 · 0.9.5 구현과 일회 실행 확인

승인된 자동 공유 경로는 **구조 메타데이터만** 내보냅니다: project ID, 불투명 기록 식별 hash, revision, 허용 상태/시각, verification의 pass/fail/not_run/partial 개수. request/response·명령·제목·요약·다음 일·evidence 본문, actor/session 원문과 개인 경로는 자동 업로드하지 않고 앱/로컬 원본에 유지합니다. GPT 호출은 필요하지 않습니다.

본인 record 등록은 위 도우미로 계속합니다. 이후 **로컬 수집 → 별도 branch 업로드 → draft PR 생성 → 별도 merge**를 각각 확인하며 record/validate/render 성공을 GitHub 공유 성공으로 바꾸지 않습니다. 응답 불명확 시 본인 latest와 원격 branch/PR을 조회해 대조하고 업무·업로드를 무작정 반복하지 않습니다. 실제 origin/저장소 identity/허용 prefix로 명시 등록된 프로젝트만 대상입니다. 기존 dirty/index/clone과 본인 확인기록은 변경하지 않습니다. [등록·보류 상태](../ai-control-tower/docs/project-record-publishing.md).

0.9.5 소스·코드 검사와 허브/Voice의 실제 headless draft PR을 확인했습니다. 두 PR은 미merge이며 현재 0.9.3 앱에 새 반복 수집/공유를 활성화한 것은 아닙니다. Threads는 task 기록 3개 검증 실패로 API 쓰기 전에 보류했습니다. completed에 남은 필수 작업·blocker를 함께 적거나 필수 필드/지원 상태가 빠지면 공유를 보류하며 parser를 완화하거나 원본을 자동 수정하지 않습니다. 본인 실제 결과에 맞춘 새 revision으로 갱신합니다. N-0007의 본인 작업 기록 의무와 공지 check/ack는 계속 별개입니다. 상세 검사·PR·배포 경계는 위 등록 안내에서 확인합니다.

## 중단 뒤 저장 여부 확인

record는 stored 또는 already_stored와 로컬 파일 경로·revision·contentSha256을 JSON으로 반환합니다. 통신이 끊겨 저장 결과가 불명확하면 먼저 latest로 본인 project·actor·record-id를 모두 지정해 조회합니다. found는 검증된 최고 revision·작업 상태·파일 원본 바이트 SHA-256이며, not_found는 exit 3, 충돌·손상·소유자 불일치는 held와 exit 1입니다. 조회는 파일을 만들거나 수정하지 않습니다. 같은 내용의 저장 재시도는 업무 재실행이 아닙니다.

중단 전에 명령·이미 전달한 답변·실제 한 일·검증·남은 일과 실행 중인 소유 작업 ID를 현재 필드에 기록합니다. 재개 때 실제 실행/산출물을 먼저 확인하며 기록·수집·중앙 공유 실패를 이유로 업무를 반복하지 않습니다. 이 도우미는 수동 요약 기록과 로컬 저장을 보조할 뿐 전체 대화 자동 백업이나 답변 전달 증명, GitHub 공유 완료를 제공하지 않습니다.
