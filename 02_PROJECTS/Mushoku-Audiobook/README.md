# Mushoku Tensei AI Audiobook
**작업 시작·재개 시 필독:** [공지사항방](../../04_COMMUNICATION/announcements/README.md)의 새·변경 공지를 읽고 본인 프로젝트/AI 확인 기록을 남깁니다. 이미 확인한 동일 내용은 반복 조회하지 않습니다.

- 저장소: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook
- 실제 루트: D:/AI/VoiceAudiobook
- 실행: 프로젝트 폴더의 RUN_Audiobook.bat / http://127.0.0.1:7862
- 최신 전체화 MP3: audiobooks/01_CURRENT/20261005_162-163_선호기준유지
- 이전 비교 MP3: comparison/01_CURRENT/20261005_FishS2
- 채택 방향: comparison/01_CURRENT/20261005_FishS2/02_선호기준유지_조합
- 기존 선호 기준: comparison/02_BASELINES/20261005_CosyVoice
- 과거: comparison/90_ARCHIVE
- 최신 듣기: 프로젝트 폴더의 RUN_LatestMP3.bat 또는 RUN_BattleChapters.bat → 구간별 듣기 안내
- 바탕화면: 사용자 금지 영역. 파일/폴더/바로가기 생성·수정·이동·삭제 금지.
- 자료: assets/local 13개 연결, docs/ASSET_INDEX.md
- Ollama 모델: D:/AI/models/ollama. 사용자 명시 D드라이브 위치 유지.

## 현재 요청 완료: 162·163화 전체 더빙
162 진흙탕 대 용신 26:33 / 163 광검왕 대 용신 31:24. 실제 화별 MP3 2개를 생성했습니다. 두 화를 합치거나 장면 MP3를 대량 복제하지 않고 내용별 제목/시간표13개를 붙였습니다. 00_구간별듣기.html에서 제목으로 이동하며 00_먼저읽기.txt/CUE도 제공합니다. 기존 comparison 자료와 분리합니다.
원문22,836자/928줄의 전체 포함·순서, 319발화 WAV 유효 샘플, 두 MP3 전체 디코딩과 서로 다른 해시 확인. 원본 불변. 일부21발화 전사 중 짧은 기합/인명/기술명3곳은 불명확하며 실제 발음/연기 승인이 아닙니다. 제한된 재생성 후 캐시·원본 보존 및 GPU worker 종료.
상세: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/main/docs/BATTLE_CHAPTERS_162_163.md

## 현재 제작 방향
2026-10-05 사용자는 선호기준유지 조합의 말이 그나마 깔끔하다고 평가하고, 그 기준으로 전체 이야기 제작으로 진행하기를 요청했습니다. 루데우스 대사/내면과 록시는 기존 Cosy 기본, 실피/에리스는 해당 조합의 Fish. 강한 감정 B를 기본으로 채택하지 않습니다. 추가 엔진 탐색을 중단합니다.
기본 실행 화면은 CosyVoice3이며, 혼합 생성기는 확인한 표본과162·163화 전체를 지원합니다. 전권 혼합 엔진 렌더/전권 화자 검수는 미완료입니다. 해당 조합의 제작 방향 채택을 모든 배역/전권 자연스러움 승인으로 확대하지 않습니다.

## TXT 전체 목록
본편25개는 웹연재01~24장+20.5이며 정발 라이트노벨 권수와 다릅니다. 외전17개 중 이야기14개와 안내3개. 이야기39개 전부 제목·줄거리·권장 읽기/제작 순서를 정리하고 원문42개 SHA256 불변 확인.
- 사용자 파일: D:/AI/VoiceAudiobook/00_START_HERE/TXT_내용과_순서.txt
- 상세: https://github.com/kimjae134679/Mushoku-Tensei-AI-Audiobook/blob/main/docs/STORY_CATALOG.md
- 기계용 색인: docs/STORY_CATALOG.json
- 본편: 01→…→19→20.0→20.5→21→…→24.
- 가족후일담: SS_04→05→06→07→08→09→10(옛웹판)→11→14→12.
- 세계관 SS_01→13, 계절보너스 SS_02→03.
SS_15의 추가단편3개는 링크만 있고 본문이 없습니다. SS_16/17과 설치테스트는 제작 제외. SS_12의 번역후기/일본어대조는 제작용 사본에서 분리하며 원본을 보존합니다.
첫 제작 추천은 본편01의 첫 장. 미확정 화자를 원문으로 확인한 구간 단위로 진행합니다. 이번 목록 정리에서 전체 생성 worker를 새로 시작하지 않았습니다.

## 이미 만든 비교 MP3와 실측
Fish S2 Pro Q8 커뮤니티 C++ 구현을 기존 VS2022/CUDA13.1로 빌드하고 RTX4070SUPER12GB에서 새 MP3 19개를 생성했습니다. 동일대사13, 원문대조 전체장면4(02/03/07/09), 기존Cosy루데우스 유지 조합2(07/09). 전체장면 길이합187.9초.
기존 비교34개는 선호기준6개/과거28개로 이동했고 원음 해시와 중복을 검증했습니다. 새결과와 섞이지 않으며 원본/캐시를 보존합니다.
짧은 실측: 음성102.864초/요청140.921초, 가중RTF1.370, 개별1.195~1.616. GPU장치전체 표본최대10411MiB. 19파일 전체디코딩, 짧은13전체 및 전체장면4 일부 전사 확인. Fish록시는 전사차이가 남아 기존Cosy기준 유지.

자동화자 제한검증25/47 및 수치오류로 미검증대본 렌더 semantic hold는 유지합니다. 전체 MP3 완료로 보고하지 않습니다.

## 재개
실제 저장소 00_START_HERE/CURRENT_HANDOFF.md와 docs/STORY_CATALOG.md/JSON을 먼저 읽습니다. 전체 대화/모델/원문을 반복검색하지 않고 요약과 체크포인트를 재사용합니다. 상세 기술 구성 docs/FISH_S2_LOCAL.md, 실제 표본 output/fish_s2_20261005/final_summary.json, 통합 기록 T-0012.
최신코드: 7add965. 업데이트: 2026-10-05 KST.
