# Mushoku Tensei AI Audiobook

- 저장소: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook
- 실제 루트: D:/AI/VoiceAudiobook
- 실행: RUN_Audiobook.bat / http://127.0.0.1:7862
- 최신 MP3: comparison/01_CURRENT/20261005_FishS2
- 선호기준: comparison/02_BASELINES/20261005_CosyVoice
- 과거: comparison/90_ARCHIVE
- 듣기: RUN_LatestMP3.bat / 바탕화면 '무직전생 최신 MP3 듣기'
- 자료: assets/local 13개 연결, docs/ASSET_INDEX.md
- Ollama 모델: D:/AI/models/ollama. 사용자 명시 D드라이브 예외 유지.

## 현재 실제 결과

Fish S2 Pro Q8 커뮤니티 C++ 구현을 기존VS2022/CUDA13.1로 빌드하고 RTX4070SUPER12GB에서 새MP3 19개를 생성했습니다. 동일대사13개, 원문대조 전체장면4개(02/03/07/09), 기존Cosy루데우스 유지 조합2개(07/09). 전체장면 길이합187.9초. 모든새파일 전체디코딩, 원문/ID/순서, 중복 검증 완료.

기존 비교34MP3를 복사 없이 이동하고 원음 해시를 검증했습니다. 선호기준6개/과거28개이며 새결과와 섞이지 않습니다. 사용자거부02/03감정B는 보관. 선호07/09 기본 및 에리스참조2/3/4·기존21Cosy전체는 보존합니다.

짧은 표본 실측: 음성102.864초 / 요청140.921초, 가중RTF1.370, 개별1.195~1.616. GPU 장치전체 최대표본10411MiB. 짧은13전체 및 전체장면4의 일부 전사를 확인했습니다. 자연스러운 연기·발음·음색의 사용자 청취 승인은 미완료입니다. Fish록시는 전사차이가 남아 기존Cosy선호를 유지하고 엔진을 전권기본으로 자동채택하지 않았습니다.

자동화자 제한검증25/47 및 수치오류로 전권검수/미검증대본렌더는 보류입니다. 전권MP3·최종연기 완성으로 보고하지 않습니다.25권원문/전처리/자동제안은 보존합니다.

## 재개

1. 실제 저장소 00_START_HERE/CURRENT_HANDOFF.md 및 docs/FISH_S2_LOCAL.md.
2. 청취: 새 조합07/09 → 전부Fish07/09 → 기존선호07/09.
3. 로컬 output/fish_s2_20261005/final_summary.json, 이전 output/listening_suite_20261005/context_accuracy.json.
4. T-0012. 전체대화/모델/자료를 반복검색하지 않고 요약·체크포인트를 재사용합니다.

최신코드:92f02f46b14176cd9a8ae3d1e61739ea92a09ae5. 업데이트:2026-10-05 KST.
