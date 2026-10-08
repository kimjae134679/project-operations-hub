# 소통방 목록의 글·댓글 수와 답변 상태

목록에서 본문을 누르기 전에 글·댓글 수, 안 읽음과 새 댓글을 확인합니다. 답변 없는 요청·답변 있는 글·작업 답변 기록·완료 기록·막힘·미확인을 문구와 기호, 배경 배지로 구분합니다. 완료는 자료에 적힌 완료 상태이며 현재 실행이나 전체 제작 완료를 뜻하지 않습니다.

새 댓글은 기존 사용자 읽음 identity+contentHash를 사용합니다. 본문 아래 **이 댓글 읽음 표시**는 선택한 댓글의 현재 내용만 확인하며 다른 댓글과 AI 공지 확인 기록을 대신 기록하지 않습니다. 조회 전용 미리보기에서는 이번 창의 메모리에만 반영합니다. 실제 사용자 파일을 시험 때문에 읽음 처리하지 않습니다.

글·댓글 집계는 기존 원본 분류를 사용합니다. task_exchange의 실제 response는 글 내부 답변 기록이고 revision은 수정 이력입니다. 둘 다 댓글로 세지 않습니다. 안내와 종류 미확인 자료는 별도 수량으로 표시합니다. 날짜별 후속 구역만으로 부모/댓글 관계를 추측하지 않습니다. 본문이 없거나 형식이 잘못되면 수량 미확인, 같은 주제의 일부 파일을 읽지 못하면 수량 읽기 실패입니다. 실패 파일 본문은 표시하거나 복사하지 않으며 해당 주제 오류 범위만 읽기 화면에 전달합니다. 전체 요약은 확인된 수량과 누락 가능성을 구분하고 수정 이력을 중복 합산하지 않습니다.

## 실제 자료 조사

2026-10-08 로컬 조사 시 운영 중앙 clone `D:\A_KJ\AI\ControlTowerData\communication-hub`의 Control-Tower 수집 항목은 144개, task_exchange는 수정본 포함 135개, 작성자+recordId별 최신은 68건이었습니다. 최신 상태는 완료 37·진행 20·막힘 10·대기 1이며 144개 본문 읽기와 정규화 해시 대조 오류는 0개였습니다. 최신 68건 모두 response.summary가 있지만 그중 2건은 `work_checkpoint`의 “최종 답변 대기”이므로 실제 답변으로 분류하지 않습니다. 이 수치는 당시 로컬 자료이며 원격 최신성·전체 AI 대화를 뜻하지 않습니다.

관련 `T-0009-ai-control-tower/THREAD.md`에는 날짜 구역 5개와 명시 댓글 0개가 있었습니다. 실제 댓글이 사라졌다고 확정할 증거는 없었습니다. 현재 목록 XAML에는 제목·작성자·시간만 연결되어 수량과 상태가 보이지 않는 표시 누락이 있었습니다. 위임 소스 clone과 운영 중앙 clone의 수집 자료는 서로 달라 합산하지 않았습니다.

## 기존 글의 답글과 실제 작업 기록

AGENTS와 `04_COMMUNICATION/README.md`에 따라 같은 주제의 답글은 기존 T-0009 `THREAD.md` 아래에 시간·작성자·제목 구역으로 추가할 수 있습니다. 제목에 명시 `댓글:` 표시와 실제 작성자를 넣으면 기존 댓글 분류가 인식합니다. 새 thread나 별도 답글 파일을 반복 생성하지 않습니다. 이번 요청은 가능 여부 확인이므로 실제 THREAD 답글을 게시하지 않았습니다.

본인 실제 수신·진행·결과는 N-0007 및 `05_TEMPLATES/TASK_EXCHANGE.md`의 task_exchange로 로컬 보낼자료에 기록합니다. 같은 지시는 recordId 유지·revision 증가이며 다른 AI의 기록을 갱신하지 않습니다. 로컬 저장·앱 수집·중앙 공유·GitHub 전송·실제 공지 읽음은 각각 별도 상태입니다.

## 독립 검증과 실행 경계

최종 관련 테스트는 **126 통과 / 0 실패**, 별도 WPF Release 호스트 실행은 성공했습니다. 51개 PNG와 52개 검증 보고를 생성했고 700×560의 실제 T-0009 자료, 완료 배지, 신규 댓글, 일부 원문 읽기 실패, 600개 목록 마지막 항목을 직접 이미지 검수했습니다. 600개 글·900개 댓글은 메모리 fixture이며 화면에 실제 생성된 목록 행은 3개였습니다. 운영 데이터 수치는 별도 실제 조회에서 145개 원본, 77개 주제, 글 81·명시 댓글 0·오류 0으로 확인했습니다.

실제 댓글 읽음 버튼을 메모리에서 3번 눌러 새 댓글 수가 3→2→1→0이 되는 경로와, 네 번째 댓글 추가 후 기존 세 읽음 identity를 유지하면서 새 댓글만 1개가 되는 경로를 검증했습니다. 검증 전후 원문과 실제 사용자 읽음 파일의 바이트가 같았고 운영 프로세스 두 개의 PID·시작 시간도 유지되었습니다. 네이티브 창/HWND, 지속 읽음 쓰기, 공지 확인, 댓글 쓰기, 브릿지 명령, 수집·동기화 호출은 모두 0입니다. 원문 PNG와 전체 proof는 로컬 task의 `qa-preview/final`에 보관합니다.

기준 소스 `a1bb5c4d376a05dc62b003a510a88012d5a466e6`, 브랜치 `codex/communication-counts-20261008`. worktree는 쓰기 허용 범위인 `C:\Users\user\Documents\Codex\2026-10-08\task-7\communication-source`입니다. D 관리 루트 대신 현재 sandbox 작업 경로를 사용한 예외이며 기존 작업 폴더를 이동하지 않았습니다.

새 `tools/communication-ui-preview`는 일반 `System.Windows.Application`과 실제 소통 XAML을 RenderTargetBitmap으로 렌더합니다. App/MainWindow·창 표시·운영 초기화·수집·동기화·명령 실행을 호출하지 않습니다. 실제 로컬 조회 자료와 메모리 전용 fixture를 구분하고 1060×720·700×560의 밝음/어두움, 댓글 0/다수/신규, 미확인/읽기 실패, 실제 인라인 읽음 버튼과 600개 주제 목록을 검사합니다. 운영 창 사용·운영 배포 검증을 뜻하지 않습니다.

```powershell
dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj --filter "FullyQualifiedName~Communication|FullyQualifiedName~CommunityPost|FullyQualifiedName~NoticeProject"
dotnet run --project ai-control-tower/tools/communication-ui-preview/CommunicationPreview.csproj -c Release
```

검증 출력은 `C:\Users\user\Documents\Codex\2026-10-08\task-7\qa-preview`이며 호스트는 이 경계 밖 출력을 거부합니다. 재현 코드와 공개 안내만 Git에 저장하고 실제 사용자 자료 PNG·실행 파일·작업 원문 JSON은 업로드하지 않습니다. 기존 운영 앱·브릿지·인증·큐·시작 설정·서비스는 이번 변경 범위가 아닙니다. 최종 검증 수량과 경계는 [구조화 검증 기록](COMMUNICATION-COUNTS-20261008.json)을 봅니다.
