# Qwen 전투 피드백 — 대표 검증

작성자: /root/voicestudio_setup · 세션 20261005-voice-parallel

기존 D:/AI/VoiceAudiobook/models/Qwen3-TTS-12Hz-1.7B-Base(가중치 3,857,413,744바이트, tokenizer 682,293,092바이트)를 재사용했고 추가 모델 다운로드는 하지 않았습니다. 공식 QwenLM API와 설치 소스 확인 결과 Base는 감정 지시를 직접 지원하지 않으므로 ref_audio/ref_text ICL만 사용했습니다.

사용자가 선호한 eris_young_1 3.76초 원본은 SHA036ef024e5a01c816735e823b79aab8f5e6beaf3a550beb5bd076b6322a061e6로 그대로 보존했습니다. 2.88초 거부본과 5.60초 기존 Cosy 참조를 섞지 않았습니다.

대표 대사3개를 원선호 ICL과 별도 긴장 VoiceDesign synthetic 참조 ICL로 각각 생성했습니다. 참조 문장은 새로 만든 전술 지시이며 소설 원문이 아닙니다. 생성6개+원본1개, 토큰 제한 도달0개. 회상2개와 현재전투1개 문맥을 분리했습니다. 전체27/32개의 생성 또는 새 후보의 사용자 승인을 주장하지 않습니다.

최종 private 인덱스: output/battle_feedback_20261005/qwen_feedback_candidates_v2.json. Windows live JSON 읽기 잠금 때문에 초기 index는 보존하고 v2로 분리했습니다. immutable record들의 SHA와 인덱스7행을 대조했습니다. 상세 설치/API/시간 범위는 qwen_investigation.json입니다.

합성 호출 기준 가중 RTF는 원선호 ICL2.186, 긴장참조 ICL2.097입니다. 모델 적재/참조 추출/GPU큐/CPU검사는 제외됩니다. 공유 GPU guard 안에서 모델을 적재했고 생성 프로세스 종료와 guard 해제를 확인했습니다. 전역 GPU메모리0 또는 청취 승인으로 확대하지 않습니다.

새 generic 코드 render_qwen_feedback.py는 py_compile 통과했고 기존 root 코드·Git·완성 MP3 발행은 건드리지 않았습니다. 원문/개인 JSON/음성/모델은 Git 제외입니다. 바탕화면 작업 없음. 통합소통 수신6공지의 본인 확인기록은 유지하며, 다른 AI의 적용을 대리 확인하지 않습니다.

최종 추가 검증: 후보7개의 실제 WAV24kHz/mono/finite/길이/인덱스SHA 일치와 바이너리 중복0개를 확인했습니다. 긴장 synthetic 참조7.04초는 Whisper base CPU(정답 prompt 없음) 문구 유사도0.9286, clipping0, codec88/limit400으로 통과했습니다. 음색/감정 승인은 아닙니다. 근거 qwen_wave_integrity.json과 qwen_tense_reference_check.json. public render_qwen_feedback.py는 동결했고 SHA899237b65be6198128ca837671ed9b7a16d7672d1b12a2d2407fd9a1ee59778f입니다. 후속 전체Eris는 root가 선택한 동일참조Vox26으로 진행하며 이 Base6후보는 비교용으로 보존합니다.
