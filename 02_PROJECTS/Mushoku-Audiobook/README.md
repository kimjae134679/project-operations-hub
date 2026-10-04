# Mushoku Tensei AI Audiobook

- 실제 저장소: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook
- 로컬 작업 폴더: `D:\AI\VoiceAudiobook`
- 실행: `RUN_Audiobook.bat` / 바탕화면 무직전생 AI 오디오북
- 자료 전체 연결: `assets/README.md`, 로컬 `assets/local`
- 최종 출력 언어: 한국어

## 재개 순서

1. 실제 저장소의 `00_START_HERE/CURRENT_HANDOFF.md`.
2. `docs/VALIDATION_20261005.md`와 필요한 코드.
3. 복구 대본 `dubbing_ready_v2`, 기존 참조 `voice_resource_pack`.
4. 필요한 경우 T-0012 소통 기록.

## 현재 상태

기존 대사 따옴표 판정 손상을 복구했습니다. 이전의 전권 화자 정리 완료 표현은 정정하며, 남은 화자를 문맥 검수 중입니다. 기존 대본과 원문은 보존합니다.

CosyVoice3 캐릭터별 실행기와 기본/감정 비교 장면을 실제 생성·검증했습니다. Qwen VoiceDesign+VC 비교본도 보존합니다. 감정·발음·목소리 닮음의 최종 청취 판단과 전권 MP3는 미완료입니다.

검수는 불확실 구간만 대상으로 로컬 처리하며, 캐시·체크포인트·GPU 작업 순서 제어를 사용합니다. 실제 코드와 진행 수치는 프로젝트 저장소 및 로컬 progress.json을 우선합니다.

사용자 지시에 따라 기존 D:\AI를 기준으로 환경·자료를 재사용합니다. Ollama 모델도 D:\AI\models\ollama에 있습니다. 일반 C드라이브 설치 규칙의 프로젝트별 예외입니다.

마지막 정리: 2026-10-05.
