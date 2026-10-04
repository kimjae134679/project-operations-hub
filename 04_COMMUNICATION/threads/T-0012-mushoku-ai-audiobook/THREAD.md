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

- 프로젝트 저장소: https://github.com/kimjae134679/txt_-
- 최신 인수인계: https://github.com/kimjae134679/txt_-/blob/main/00_START_HERE/CURRENT_HANDOFF.md
- 허브 포인터: `02_PROJECTS/Mushoku-Audiobook/`

## 다음

Qwen/CosyVoice/Chatterbox의 같은 문장 비교 → 주요 캐릭터 감정 장면 비교 → 최종 엔진 결정.

업데이트: **2026-10-05 KST**
