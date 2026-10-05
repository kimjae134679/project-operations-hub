# Higgs 음성 후보 비교 진행

작성자: /root/higgs_bench. 세션: 20261005-voice-parallel.

실제 프로젝트는 D:/AI/VoiceAudiobook이고 기존 자료·환경을 유지합니다. 새 비교 환경은 D:/A_KJ/AI/Workspace/VoiceBench/higgs-env, 모델은 D:/A_KJ/AI/Models에 둡니다. 바탕화면 변경 없음. GPU 작업은 프로젝트 gpu_guard로 순차 사용하며 모델 다운로드/CPU검사는 다른 담당 작업과 병렬로 진행합니다.

Higgs 공식 모델과 Windows Transformers 포트 코드를 검토했습니다. 독립 환경 Torch2.6cu124/Transformers5.5는 실제 import·tiny forward·autoregressive sampler API 검사를 통과했습니다. 별도 codec805MB는 immutable revision과 LFS SHA256 검증을 완료했고 CPU1초 encode/decode 왕복은 유효한24000샘플을 반환했습니다. 이는 연기·발음 승인과 별개입니다.

본체9.31GB 다운로드는 진행 중입니다. 부분파일 크기를 다운로드 완료로 취급하지 않습니다. 모든 고정된 파일 검증/완료 후 자동 benchmark가 GPU 잠금을 얻어8표본을 합성합니다. 실제 생성8개 완료는 아직 아닙니다. 원문과 참조 계획은 output/battle_action_20261005의 비공개 JSON, 결과는 같은 폴더의 higgs_candidates.json으로 연결합니다. 모델/원문/오디오는 GitHub에 올리지 않습니다.

공용 코드 benchmark_higgs.py, 설명 docs/HIGGS_TTS3_BENCHMARK.md에는 원문을 넣지 않습니다. 기존 설치, 서비스, 게임, 독립 채팅을 임의 수정/종료하지 않습니다. 최신 공지6개를 본인이 실제 읽었고 자기 기록을 로컬로 남깁니다. 관리 앱의 수집·중앙 동기화 완료는 별도 미확인입니다.
