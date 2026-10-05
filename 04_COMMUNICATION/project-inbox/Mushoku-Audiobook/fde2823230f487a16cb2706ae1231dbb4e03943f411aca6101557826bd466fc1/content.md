# FireRed 설치 및 검증 종료
Actor: /root/firered_bench
Session: 20261005-voice-parallel
2026-10-05

공식 Instruct 및 RedAE 가중치를 고정 HF snapshot SHA256으로 검증해 D:/A_KJ/AI/Models/FireRedTTS3에 설치했다. 외부 source와 격리 venv는 D:/A_KJ/AI/Workspace/VoiceBench에 유지했다. 기존 Qwen 환경은 변경하지 않았다. 바탕화면은 건드리지 않았다.

최초 9개 WAV는 실행/컨테이너 검사에 성공했지만 실제 한국어 내용 일치에 실패했다. 파일을 보존하고 사용자 청취 후보 채택을 보류했다. CPU FP32 codec 한국어 참조 왕복 ASR은 1.0, mixed precision 한국어 smoke는 0.0769였다. 완전 FP32 core 진단에서 영어 design 1.0, 한국어 clone 0.0476, 한국어 design 0.0563이다. state-dict 누락/불일치 없음; patch/DiT RoPE 버퍼 공식 수식과 일치.

현재 Instruct 한국어 경로 실패로 기록한다. Base 한국어 전체 성능 한계로 일반화하지 않는다. Base 추가 다운로드 및 실대사 8개 반복 생성은 중단했다. verified candidates 파일은 생성하지 않았다. Confucius4는 공식 문서 조건 조사만 완료했다.

공개 재현 코드: benchmark_firered.py, benchmark_firered_precision.py. 근거/설치 설명: docs/FIRERED_LOCAL.md. 원문·참조음성·결과 WAV·모델·private plan은 Git 제외. 통합소통 6공지 실제 읽기 및 본인 actor receipt 기록 완료; 다른 actor 기록은 수정하지 않았다. 중앙 collector의 수집/반영 여부는 별도 확인 대상이다.
