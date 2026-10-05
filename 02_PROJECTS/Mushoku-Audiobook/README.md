# 무직전생 한국어 오디오북

한국어 소설을 등장인물의 대사·내면·나레이션으로 나누어 제작하고, 본문을 클릭하며 음성을 듣는 프로젝트입니다.

- 실제 개발·제작 폴더: D:/AI/VoiceAudiobook
- 개발 저장소: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook
- 감상: D:/AI/VoiceAudiobook/무직전생.exe → 제작 목록
- 제작 제어: D:/AI/VoiceAudiobook/오디오북제작.exe
- 소통 기록: [오디오북 작업 스레드](../../04_COMMUNICATION/threads/T-0012-mushoku-ai-audiobook/THREAD.md)
- 필독 공지: [공지사항방](../../04_COMMUNICATION/announcements/README.md). 본인 프로젝트·작성자·세션의 실제 확인 기록만 작성합니다.

## 입력·현재 결과·보관

입력 TXT는 실제 프로젝트 input 폴더에서 보존합니다. 원본 영상·성우 음성·배역 참조·모델은 로컬 자료이며 GitHub 공유 대상이 아닙니다.

audiobooks/01_CURRENT는 현재 전체화 제작본, comparison/01_CURRENT는 배역 비교본입니다. 현재 제작 목록 포인터는 audiobooks/library_latest.json입니다. 과거 자료는 각 90_ARCHIVE에서 확인합니다. 기존 폴더를 정리하기 위해 원문·모델·환경을 임의 이동하지 않습니다.

D:/A_KJ/AI/txt_무직전생은 코드 보관본입니다. 모델과 별도 엔진 검토 환경이 D:/A_KJ/AI/Models 및 Workspace/VoiceBench에 있더라도 실제 제작 루트는 D:/AI/VoiceAudiobook입니다. 바탕화면은 사용자 금지 영역입니다.

## 현재 제작과 제한

사용자 선택 범위는 웹연재 본편 15~24와 20.5의 본편·막간·종장·에필로그 128편입니다. 대본 검수 준비와 MP3 완성은 별개입니다. 진행·완성·보류 상태는 실제 output/production_15_24/status.json과 제작 목록에서 확인합니다.

루데우스·내면·나레이터·에리스·실피는 VoxCPM2, 록시·올스테드는 CosyVoice3를 사용합니다. 배역별 원본 참조·엔진·모델을 고정하고 생성 음성을 다음 참조로 사용하지 않습니다. 실피 기준은 임시이며 감정 연기나 모든 배역의 사용자 승인이 완료된 것은 아닙니다.

원문 대조 대본을 자동 생성·검사·보정·MP3 저장·본문 등록하며 통과 캐시를 재사용합니다. 반복 실패는 별도 검토로 남기고 다른 회차를 진행합니다. 생성 제외 구간은 원문 위치와 일반 설정만 기록하고 실제 생성하지 않습니다. 이전의 미검증 전권 자동 화자 판정은 보류 상태를 유지합니다.

ChatGPT 연결과 독립된 로컬 감독 프로세스를 사용합니다. 같은 Windows 사용자 로그인 시 미완료 제작을 자동 재개하며, 사용자의 일시정지와 완료 상태를 존중합니다. PC가 꺼져 있거나 절전 중이면 생성할 수 없습니다.

## 이어받기

현재 코드와 실제 실행 상태가 원본입니다. 긴 대화 전체를 다시 읽기 전에 다음을 확인합니다.

- [제작·검사·복구 구조](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/main/docs/PRODUCTION_PIPELINE.md)
- 실제 폴더 00_START_HERE/본편15부터_설정과실행.txt 및 자동제작_연결끊김과복구.txt
- output/production_15_24의 상태·진행·감독·검토 기록
- [전체 TXT 내용과 순서](https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/main/docs/STORY_CATALOG.md)

이 문서는 실행 위치와 확인 경로를 안내합니다. 변동하는 완료 수·커밋·청취 평가는 작업 스레드와 실제 프로젝트에서 확인합니다.
