# T-0012 — Mushoku Tensei AI Audiobook

상태: **Open**

## 목적

무직전생 한국어 TXT를 캐릭터별 AI 음성으로 읽는 로컬 오디오북 파이프라인 구축.

## 핵심 결정

- 최종 언어는 한국어.
- TXT 분석과 voice identity를 분리한다.
- 루데우스 실제 대사 / 루데우스 내면은 별도 voice slot.
- 실피 / 록시 / 에리스 로맨스 장면은 일반 장면보다 감정선과 관계 변화를 세밀하게 기록.
- 표본 없는 캐릭터는 고정 fallback voice 사용.
- 애니 원본 음성은 로컬 참조용으로만 사용하고 저장소에 올리지 않는다.

## 진행

- 본편 25권 기본 연출본 생성
- 화자 애매 대사 176개 재판정
- 애니 영상 중복 제거 31 → 17
- 주요 캐릭터 음성 표본 생성
- Chatterbox V3: 한국어 음절 끊김 문제 확인
- CosyVoice3: 연결감 개선 확인, 감정 instruction 비교
- Qwen3-TTS 1.7B Base: CUDA 환경 구축 및 비교 진행
- 참조음성 raw / mild denoise / Demucs 실험

## 실제 원본

- 프로젝트 저장소: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook
- 최신 인수인계: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/main/00_START_HERE/CURRENT_HANDOFF.md
- 허브 포인터: `02_PROJECTS/Mushoku-Audiobook/`

## 다음

Qwen/CosyVoice/Chatterbox의 같은 문장 비교 → 주요 캐릭터 감정 장면 비교 → 최종 엔진 결정.

업데이트: **2026-10-05 KST**

## 2026-10-05 04:41 — Codex — D드라이브 통합과 대사 분류 오류 정정

- 코드·실행기·기존 비교 실험·검증·인수인계를 실제 프로젝트 main에 반영했습니다. 커밋 ef1b3b5.
- 실제 루트 D:\AI\VoiceAudiobook. assets/local의 13개 junction으로 기존 원문·참조·영상·출력·모델·환경을 한곳에서 엽니다. GitHub에는 자료 위치 색인과 코드를 관리합니다.
- C에 남아 있던 Ollama 모델 7.7GB는 D:\AI\models\ollama로 이전. 12파일 SHA-256, 기존 경로 junction, 실제 8B 추론 검증 후 임시 C 복사본을 정리했습니다.
- 기존 따옴표 판정이 ?로 손상돼 대사가 나레이션으로 들어가던 오류를 복구. 25권 수정본은 원문 순서·내용을 보존하며 대사 26,071개를 검출했습니다.
- 이전 “전권 화자 정리 완료”는 정정합니다. 대사 화자 미확정 10,729개, 공란 포함 전체 화자 미지정 11,986개가 남았습니다. 기존 176개 재판정 자료는 대사 ID별로 재적용했습니다.
- 동일 8구간 CosyVoice 기본/감정 MP3, 주요 4인 Qwen 감정+VC 비교본, 실제 책 록시 첫 등장 3구간 MP3를 보존했습니다. 자동 전사 일부 차이가 있어 최종 청취 판단은 미완료입니다.
- 실행 화면의 25권 목록·오디오 반환·캐시 8발화 재사용·검수 일시정지/재개를 검증했습니다. 불확실 대상을 최대 8개씩 묶는 로컬 검수와 콘텐츠 캐시·체크포인트·GPU 잠금을 연결하고 검수 큐를 재개했습니다.
- 기존 원문과 대본을 덮어쓰지 않습니다. 자동 제안은 output/context_review에 저장하며 확신도만으로 정확도를 승인하지 않습니다. 전권 MP3는 미완료입니다.

상세 최신 상태: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/main/00_START_HERE/CURRENT_HANDOFF.md
