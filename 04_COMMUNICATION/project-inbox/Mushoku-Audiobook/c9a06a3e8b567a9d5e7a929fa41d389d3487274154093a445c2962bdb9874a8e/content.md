# 무직전생 UI·정리·음성 병렬 작업

작성자 /root/app_cleanup, 세션 20261005-voice-parallel. 본인 작업만 요약합니다. 로컬 자료 작성과 관리 앱의 중앙 수집·동기화 완료는 별개입니다.

실제 프로젝트 D:/AI/VoiceAudiobook, 청취 프로그램 무직전생.exe. 같은 화자의 본문을 묶고 넓은 글·문장 클릭·루데우스 황토색을 유지했습니다. PC/모바일 각17검사, 앱 실제 렌더·Windows GUI 실행형식·최신 MP3 경로 검증 완료.

BAT·과거 실험·설치기·검사결과26파일을 역할별 폴더에 정리하고 이동 전후 해시를 검증했습니다. 원문·성우·모델·환경·생성음성·실행중 작업은 이동하지 않았고 바탕화면을 변경하지 않았습니다. 안내와 인수인계는 실제 경로로 갱신했습니다.

OmniVoice와 VoxCPM2 별도 환경을 D:/A_KJ/AI/Workspace/VoiceBench에 구성하고 기존 CUDA Torch2.6.0+cu124 재사용·엔진 import을 검증했습니다. 기존 환경은 유지합니다. 공식 API로 GPUguard 순차 실제 비교를 수행하며, 원문/참조/오디오는 로컬만 보관합니다.

OmniVoice8/8 생성·FLOAT WAV 전체읽기/finite/해시/원문보존 검증 완료. 총음성13.89초, 생성11.392초, 가중RTF0.820. 모델적재18.279초/참조준비2.270초 별도. Torch CUDA할당최대4.185GiB/예약5.555GiB이며 장치전체사용량은 아닙니다. 연기/억양 사용자승인은 false입니다.

VoxCPM2는 실제 모델 최종파일·토크나이저 대기 후8기본+4감정연기 후보를 자동 큐로 생성합니다. 설치·import 성공과 실제 합성 완료를 구분합니다. 결과 검증과 비교 청취 패키징은 이어서 갱신합니다.

로컬 근거: output/app_and_cleanup_checks_20261005.json, output/folder_organization_20261005.json, output/battle_action_20261005/omni_verification.json. 본문·키·계정·대형 바이너리는 이 자료에 포함하지 않습니다.
