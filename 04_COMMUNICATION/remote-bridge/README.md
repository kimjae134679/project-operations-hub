# 프로젝트 연결 — 최초 설치 대기

새 프로젝트 연결 프로그램과 전회차 청취 QA 코드가 준비됐습니다. PC 제어 플러그인의 월 한도로 실제 PC 설치·음원 생성은 미확인입니다. 이 기록을 연결 완료로 보지 않습니다.

## 한 번 설치하기

PC의 PowerShell에 아래 명령을 한 번 붙여넣고 실행합니다. 바탕화면에는 만들지 않습니다.

```powershell
irm 'https://raw.githubusercontent.com/kimjae134679/project-operations-hub/main/04_COMMUNICATION/remote-bridge/install.ps1' | iex
```

설치 프로그램과 코드의 정확한 SHA를 검사하고 Windows에서 실행 프로그램을 만듭니다. 프로그램은 D:/A_KJ/AI/Applications/ProjectBridge/프로젝트연결.exe, 관리 대상은 기존 D:/AI/VoiceAudiobook입니다. 창 없는 작업 자식·로그인 시작·현재 로그인 중 명시적 종료 보존을 제공합니다. 로그인 작업 권한이 없으면 현재 사용자 Startup 폴더의 전용 바로가기를 사용합니다. 다른 앱·작업은 수정하지 않습니다.

GitHub는 PC에 기존 로그인한 gh 또는 Git Credential Manager를 사용합니다. 인증이 없으면 PC의 GitHub 로그인을 한 번 완료해야 합니다. 토큰을 채팅에 붙이지 않습니다. OpenAI API 키와 추가 결제는 필요하지 않습니다. Windows 실제 컴파일·연결·브라우저 청취는 설치 후 확인합니다.

검토 소스: [오디오북 PR](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/pull/1). 고정 release commit: de2727f9740ae86ff022982d9ea257012193ba6d. 최초 코드 bundle SHA: 97ae1490c6d483e97eb511ddcf7f088ad24db3b5a954fe85c8b449f5324dfd35.

## 설치 뒤 진행할 일

1. 현재 회차 상태와 표본 자료 개수를 확인합니다.
2. 검토한 16개 코드/UI/테스트/안내를 기존 파일 SHA 검사·백업 후 적용합니다. 다른 로컬 수정이 있으면 덮어쓰지 않고 막힘으로 기록합니다.
3. 청취 QA 검사 → 듣기 폴더 정리·실행기 빌드·원본 참조 점검 → 목록 갱신을 수행합니다.
4. 나나호시·자노바·아리엘·엘리나리제의 다른 검수 회차 대사로 제한된 비교 후보를 생성합니다. 실제 화자가 확인된 대체 표본이 없으면 보류합니다. 전사·속도·디코딩 검사는 연기 승인과 다릅니다.
5. 페르기우스와 실제 본편 cast/anchors는 보존합니다. 기존 본편 자동 제작을 임의로 재시작하지 않습니다.

각 작업은 새 ID를 쓰고 완료 요청은 반복 실행하지 않습니다. 선행 작업이 실패하면 의존 작업도 막힙니다. GPU 비교는 기존 본편과 잠금을 공유하므로 기다릴 수 있습니다. 비교 음원은 실제 생성 후 comparison/01_CURRENT/20261006_음성QA_재수정/00_수정후보.html에 표시됩니다.

## 인물 듣기와 QA

제작판 → 인물 → 회차로 선택하며 준비된 128회차와 발행 증명이 확인된 이전 캠페인을 표시합니다. 미완성은 대기입니다. 대사 클릭·이전/다음·인물만 이어 듣기·같은 인물 다음 회차·점수 1~10·문제 태그·메모를 사용합니다. 기존 162/163의 다른 비공개 발행 증명은 확인 전이라 이전 폴더 링크를 유지합니다.

평가는 output/listening_feedback/events.jsonl에 PC 로컬 저장합니다. 미연결 평가는 브라우저에 보관했다가 재전송합니다. 소설 원문·대사·오디오·영상·모델·개인 메모·자격증명은 공개 허브에 올리지 않습니다.

## 통합소통과 결과

- [대기 중 실행 요청](queue.json): 허용 작업만 있는 별도 운영 큐. 기존 task_exchange를 임의 실행하지 않습니다.
- 실행 후 결과: results/<기기>/<작업>.json. 설치 전에는 결과 폴더가 없을 수 있습니다.
- 같은 실행 응답을 표준 task_exchange로 04_COMMUNICATION/project-inbox/Mushoku-Audiobook에 등록해 관리 앱의 통합소통에서 읽게 합니다. 프로그램 응답은 AI 읽음·연기 승인과 구분합니다.
- [프로그램 동작·복구·자료 보호](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/de2727f9740ae86ff022982d9ea257012193ba6d/bridge/README.md)
- [청취 QA 기준](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/de2727f9740ae86ff022982d9ea257012193ba6d/docs/AUDIO_QA_20261006.md)

현재 상태: 코드 게시·정적 검증 완료 / Windows 최초 설치·실제 연결·새 MP3·청취 검증 대기. 2026-10-06.
