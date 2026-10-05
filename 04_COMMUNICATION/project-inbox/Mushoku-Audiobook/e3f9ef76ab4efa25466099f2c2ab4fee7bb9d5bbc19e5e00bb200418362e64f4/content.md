# 무직전생 UI·정리·음성 병렬 작업

작성자 /root/app_cleanup, 세션 20261005-voice-parallel. 본인 작업만 요약합니다. 로컬 자료 작성과 관리 앱의 중앙 수집·동기화 완료는 별개입니다.

실제 프로젝트 D:/AI/VoiceAudiobook, 청취 프로그램 무직전생.exe. 같은 화자의 본문을 묶고 넓은 글·문장 클릭·루데우스 황토색을 유지했습니다. PC/모바일 각17검사, 앱 실제 렌더·Windows GUI 실행형식·최신 MP3 경로 검증 완료.

BAT·과거 실험·설치기·검사결과26파일을 역할별 폴더에 정리하고 이동 전후 해시를 검증했습니다. 원문·성우·모델·환경·생성음성·실행중 작업은 이동하지 않았고 바탕화면을 변경하지 않았습니다. 안내와 인수인계는 실제 경로로 갱신했습니다.

OmniVoice와 VoxCPM2 별도 환경을 D:/A_KJ/AI/Workspace/VoiceBench에 구성하고 기존 CUDA Torch2.6.0+cu124 재사용·엔진 import을 검증했습니다. 기존 환경은 유지합니다. 공식 API로 GPUguard 순차 실제 비교를 수행하며, 원문/참조/오디오는 로컬만 보관합니다.

OmniVoice8/8 생성·FLOAT WAV 전체읽기/finite/해시/원문보존 검증 완료. 총음성13.89초, 생성11.392초, 가중RTF0.820. 모델적재18.279초/참조준비2.270초 별도. Torch CUDA할당최대4.185GiB/예약5.555GiB이며 장치전체사용량은 아닙니다. 연기/억양 사용자승인은 false입니다.

VoxCPM2 실제8기본+4감정연기 후보12/12 자동큐 생성 및 FLOAT WAV 전체읽기/finite/시간/해시/원문보존 검증 완료. 총음성22.72초/생성42.121초, 가중RTF1.854, 적재26.745초 별도. Torch할당5.723GiB/예약6.311GiB 최대이며 장치전체사용량은 아닙니다. 연기/억양 승인은 false이며 CPU 전사와 실제 비교청취를 이어갑니다.

로컬 근거: output/app_and_cleanup_checks_20261005.json, output/folder_organization_20261005.json, output/battle_action_20261005/omni_verification.json. 본문·키·계정·대형 바이너리는 이 자료에 포함하지 않습니다.

최종 비교 페이지: 7묶음/56개, PC 1366 및 모바일 390에서 각 7개 검사 통과. 라벨 10개 검사 통과. HTML SHA256 6e90646e2c0f59292ada541acae672104e221b85ea52a4d2b3f2e03c469852f5. 사용자 음성 승인 false 유지. 추가 GPU 작업 없음.

인물 접근 개선: 실제 실피4/록시1/길레느2 발화를 확인했고 최종 MP3 신호 정상. 인물 선택과 첫/다음 발화, 다른 화로 이동, 기레느 별칭 검색을 추가하고 PC/모바일 각29검사 통과. 실행기를 원본 보존 후 교체했고 열린 사용자 창은 유지. 다음 실행에서 새 화면 사용. 코드75개 src 정리 계획은 생성했고 GPU 작업 종료 및 root 동기화 승인 전 이동하지 않음. 원문/모델/참조/음성/환경/바탕화면 보존.

추가 원문 확인: 오르스테드0123/0165는 실제 말이 있으나 기존 음성 신호가 거의 들리지 않는 상태여서 root에 재생성 대상으로 전달함. 순수 말줄임표는 인물 첫/다음 탐색에서 제외하며 원문 본문은 보존. UI 최종 PC/모바일 각29검사 통과. 옛 테스트의8개 예문은 원문·배역·참조·수치 그대로 비공개 fixture로 분리했고 공개 소스에는 가져오기만 남김. 코드 이동은 여전히 동기화 승인 대기.


2026-10-06 단일 창 프로그램: 홈/본문/목소리 비교를 유지되는 내부 프레임으로 전환, 뒤로/앞으로 지원. 숨긴 오디오는 정지하고 기존 재생 여부만 복원, chapter/person/search/currentTime/scroll 및 비교 select를 보존. 실제 Chrome PC1366·mobile390 각28항목 통과, 홈·본문 PNG 육안 확인 완료. 본인 새 EXE는 이전 EXE를 SHA 백업하고 교체했으며 사용자 기존 창은 종료하지 않았음. Desktop 작업 없음. 공개 변경 app/AudiobookLauncher.cs, ui/application_shell.html, app/check_navigation.py, app/check_navigation_cdp.mjs. 원문/음성/모델 포함 없음. 증거 output/app_navigation_20261006/navigation_checks.json, output/app_single_window_install.json. 음성 청취 승인으로 확대하지 않음.
