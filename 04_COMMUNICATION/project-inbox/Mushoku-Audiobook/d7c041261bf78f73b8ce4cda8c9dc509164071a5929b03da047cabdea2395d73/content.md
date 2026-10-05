# VoiceStudio 로컬 기능 검증 완료

작성자 /root/voicestudio_setup, 세션 20261005-voice-parallel.

VoiceStudio0.5.6을 공식 릴리스 검증 후 D:\A_KJ\AI\Applications\VoiceStudio에 수동 추출 설치했습니다. 설치기를 실행하지 않았고 바탕화면을 변경하지 않았습니다. 기존 Qwen 환경을 변경하지 않은 별도 runtime과 D 데이터/캐시를 사용합니다.

검증된 D:\A_KJ\AI\Models\OmniVoice 최종 파일을 공유 경로로 연결하고 GPU 잠금 안에서 warmup 완료를 기다렸습니다. 소설 원문이 아닌 원본 시험 문장을 공식 /generate로 실제 생성했습니다: HTTP200, mono24kHz,4.98초,생성4.296초. CPUWhisper-base문구유사도0.923/경고없음. 연기·목소리 유사도·사용자 청취승인은 별개입니다. WAV/MP3와 실제 GUI/API/VRAM 기록은 project output에만 보존합니다.

공식 unload 모델0개/idle 확인 후 소유 앱/backend 정상종료까지 확인했고 GPU전체 used6294→4549→2443MiB로 반환했습니다. 비교전2172MiB대비271MiB차이는 다른창을 포함한 전체GPU측정입니다. 무거운 후속작업을 위해 앱은 닫았습니다.

MCP TASK_STATUS_COMPLETED import mismatch, grpc누락 inbound node, AudioSeal미설치는 제한으로 문서화했습니다. 통계 동의를 변경하지 않았습니다. 클라우드/결제없음. SDK선택기능까지 정상이라고 주장하지 않습니다.

Zonos2 첫 한국어CLI 결과는 Windows CP949 argv를 UTF-8 tokenizer에 넣은 오류로 확인했습니다. 공식 서버UTF-8 JSON 방식으로8개 다시 만들고 공통CPU검사와 최대토큰반복 제외 후3개만 비교대상으로 유지했습니다. 나머지와첫결과는 진단용으로보존합니다. 원래 실행파일/시스템로캘을 변경하지 않았습니다.

공지6개 최신본문을 실제읽고 이작성자의 receipt를 local helper로 기록한 상태입니다. N1–5적용/N6PhoneLOL은 이프로젝트에해당없음. 다른AI기록을 대신작성하지 않았습니다. 로컬 확인 기록은 중앙 수집 완료를 뜻하지 않으며 자동수집 확인은 별도입니다. 이요약에는원문/오디오/모델/토큰을포함하지않습니다.
