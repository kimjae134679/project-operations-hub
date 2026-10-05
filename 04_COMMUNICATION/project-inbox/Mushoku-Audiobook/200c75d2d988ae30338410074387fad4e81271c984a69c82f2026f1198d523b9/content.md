# FireRed 설치 및 검증 종료
Actor: /root/firered_bench
Session: 20261005-voice-parallel
2026-10-05

공식 Instruct 및 RedAE 가중치를 고정 HF snapshot SHA256으로 검증해 D:/A_KJ/AI/Models/FireRedTTS3에 설치했다. 외부 source와 격리 venv는 D:/A_KJ/AI/Workspace/VoiceBench에 유지했다. 기존 Qwen 환경은 변경하지 않았다. 바탕화면은 건드리지 않았다.

최초 9개 WAV는 실행/컨테이너 검사에 성공했지만 실제 한국어 내용 일치에 실패했다. 파일을 보존하고 사용자 청취 후보 채택을 보류했다. CPU FP32 codec 한국어 참조 왕복 ASR은 1.0, mixed precision 한국어 smoke는 0.0769였다. 완전 FP32 core 진단에서 영어 design 1.0, 한국어 clone 0.0476, 한국어 design 0.0563이다. state-dict 누락/불일치 없음; patch/DiT RoPE 버퍼 공식 수식과 일치.

현재 Instruct 한국어 경로 실패로 기록한다. Base 한국어 전체 성능 한계로 일반화하지 않는다. Base 추가 다운로드 및 실대사 8개 반복 생성은 중단했다. verified candidates 파일은 생성하지 않았다. Confucius4는 공식 문서 조건 조사만 완료했다.

공개 재현 코드: benchmark_firered.py, benchmark_firered_precision.py. 근거/설치 설명: docs/FIRERED_LOCAL.md. 원문·참조음성·결과 WAV·모델·private plan은 Git 제외. 통합소통 6공지 실제 읽기 및 본인 actor receipt 기록 완료; 다른 actor 기록은 수정하지 않았다. 중앙 collector의 수집/반영 여부는 별도 확인 대상이다.


## 최종 설치 감사 추가 확인
Actor: /root/firered_bench / Session: 20261005-voice-parallel
기존 installation_audit_20261005.json은 역사로 보존했고 installation_audit_final_20261005.json을 별도 작성했다. 신규 모델5종 설치/필수파일 준비와 실제 생성 기록을 재대조했다. Higgs 모델·codec의 local_sha256_verification.json 및 aggregate all_verified 영수증을 실제 읽어 최종 SHA 증거 누락이 해소됐음을 확인했다. 43개 교체의 파형/원문 보존 검증과 162·163화 전체 MP3 파일 존재·크기·완료 기록을 확인했다. VoiceStudio /generate 한국어 HTTP200, 4.98초 WAV, 생성4.296초, ASR0.9231 및 앱/백엔드 정상종료 근거를 확인했다. 작은 WAV SHA도 성공 증거와 일치한다. 사용자 청취승인은 여전히 별도이며 human_approved=false를 유지했다. 다운로드/GPU로드/public코드/Git 작업은 하지 않았다.
