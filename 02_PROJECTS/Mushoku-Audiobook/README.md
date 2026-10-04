# Mushoku Tensei AI Audiobook

- 저장소: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook
- 실제 루트: `D:\AI\VoiceAudiobook`
- 실행: RUN_Audiobook.bat / http://127.0.0.1:7862
- 새 비교 폴더: comparison/20261005_감정장면_재비교
- 듣기: RUN_EmotionSamples.bat / 바탕화면 무직전생 새 감정장면 듣기
- 자료 연결: assets/local의 기존 13개 연결과 docs/ASSET_INDEX.md
- Ollama 모델: D:\AI\models\ollama. 이 프로젝트는 사용자가 명시한 D드라이브 예외입니다.

## 현재 상태

원문을 대조한 10개 소설 장면의 슬픔·격변·전투·애정 MP3, 실피·에리스 참조 비교, 한국어 VoiceDesign 원본 비교를 생성했습니다. 개별 26개 중 시험본 1개는 반복 전사 이상으로 보류했습니다. 재생 폴더에는 개별 25개와 약 6분56초 기본 모아듣기가 있습니다. 전체 파일 디코딩과 일부 전사를 확인했으며 자연스러운 연기는 청취 승인이 필요합니다.

이전 감정본의 느림·실피 억양·하이브리드 이질감 피드백을 반영해 soft voice 강제 기본값을 제거하고 기본 감정 제어를 껐습니다. 기존 기본본은 보존합니다.

자동 화자 판정은 제한된 회귀 검증에서 25/47 및 별도 수치 오류로 미통과했습니다. 전권 자동 검수·미검증 대본 렌더는 보류입니다. 전권 MP3·최종 연기 품질이 완성됐다고 보고하지 않습니다. 25권 원문·전처리·자동 제안은 보존합니다.

## 재개

1. 실제 저장소의 00_START_HERE/CURRENT_HANDOFF.md.
2. docs/LISTENING_SUITE_20261005.md와 docs/LISTENING_FEEDBACK_20261005.md.
3. 로컬 output/listening_suite_20261005/final_summary.json 및 context_accuracy.json.
4. T-0012 기록. 전체 대화와 자료를 반복해 가져오지 않습니다.

최신 코드: b75ef633c598428c081a5ff0063612b9cda432da. 업데이트: 2026-10-05 KST.
