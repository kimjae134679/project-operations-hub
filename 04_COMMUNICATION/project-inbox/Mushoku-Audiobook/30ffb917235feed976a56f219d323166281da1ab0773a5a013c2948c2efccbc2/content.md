# FireRed 음성 비교 진행

작성자 /root/firered_bench, 세션 20261005-voice-parallel.

기존 프로젝트 D:/AI/VoiceAudiobook·원문·환경·바탕화면은 유지합니다.
새 소스/환경은 D:/A_KJ/AI/Workspace/VoiceBench, 가중치는 D:/A_KJ/AI/Models/FireRedTTS3.
공식 Instruct 체크포인트와 공유 codec만 내려받고 Base 중복 가중치는 제외했습니다.
기존 Torch2.6cu124를 읽기 재사용하는 분리 환경과 Transformers5.6.2를 설치했습니다.
실제 CUDA bfloat16·SDPA·KV cache 작은 검증은 통과했습니다. 본 음성 합성과 사용자 연기 승인은 아직 별개입니다.
가중치 헤더상 합계 약 30.6억 파라미터, bfloat16 약 5.70GiB이며 실제 최대 VRAM/생성 속도는 합성 때 기록합니다.
공식 HF 체크포인트의 크기·SHA256과 같은 공식 ModelScope 미러를 확인해 이어받고 있습니다.
다운로드 총 12연결, 두 모델 병렬. codec 검증 완료 후 연결을 남은 모델로 재배치합니다.
완료된 가중치의 전체 SHA256 검증 뒤 GPU 가드로 합성해 다른 모델과 동시 GPU 적재를 막습니다.
원문은 private plan에 보존하며 연기용 입력의 반복 모음 정규화는 별도 tts_text로 기록합니다.
결과/상태는 output/battle_action_20261005/firered_candidates.json·firered_status.json, 설명 docs/FIRERED_LOCAL.md.
Confucius4는 한국어·참조 감정 전달 공식 조건을 검토했지만 본 PC 합성은 미실측이며 추가 다운로드 보류입니다.
현재 N-0001r2,N-0002~4r1,N-0005r2,N-0006r1을 본인이 실제 읽고 로컬 본인 기록을 남겼습니다. 중앙 수집·GitHub 공유 완료는 별도 확인 대상입니다.
