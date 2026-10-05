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


## 사용자 후속 피드백: Vox 전투·실피 후보 완료
Actor: /root/firered_bench / Session: 20261005-voice-parallel
신규20개 실제 생성과 raw WAV/원문/voice/refSHA 검증을 완료했다. Combat10은 같은4원문을 6상황타입으로 비교하며 지시문과 performance phonetics를 별도 기록했다. 실피4·록시1·기레느2는 기존Korean reference, 에리스3은 사용자가 선호한 Qwen original3.76초 SHA검증 reference를 재사용했다. 회상 속 대면과 현재 전투 문맥을 구분했다. 실피 긴 설명은 울면서 반론하는 회상으로 연출했다.
기존 Vox공격 기본/attack source·tts·reference 동등성과 WAV SHA를 확인했다. 통일표기 기대문자 기준 ASR는 Vox기본0.25/attack0.75, CosyVC0.25/instruct0.0이다. 사용자가 선호한 엔진과 정확variant 승인 여부를 구분했다. 모든 신규 human_approved=false 유지.
Private evidence: output/battle_feedback_20261005/vox_feedback_candidates.json, vox_feedback_verification.json, vox_attack_existing_audit.json. Generic renderer render_vox_feedback.py는 source literal0/AST/safeimport 검증 후 수정 동결했다. 총47.52초 음성/67.957초 생성, load제외weightedRTF1.430. GPUguard를 모델load전부터 획득했고 두10개batch 종료 후 ownedmodel/GPU작업 해제했다. 추가모델설치/다운로드/전체오디오교체/게시/Git작업 없음. 함성 continuity RMS지표는 듣기우선순위 도움일 뿐 하울링 여부나 연기 승인 근거가 아니다.


## 本문完成 단계: 선호참조 에리스 전체와 오르스테드 무음 교정
Actor: /root/firered_bench / Session: 20261005-voice-parallel
root 추가승인으로25개를 생성했다. 에리스26개는 선호Qwen3.76초원본을 참조한 Vox로 일관(초기검증3reuse+23new). 재회1개는 정확선호Qwen원본그대로 유지하여 총27발화구성이 가능하다. 27개원문과source manifest를 대조했고 voice/refSHA/전체WAV/metadata 검증을통과했다. 내면4는전술적긴장, 집·훈련회상은각대면문맥, 현장짧은반응3은압축된짧은육체적발성으로분리했다.
오르스테드 거의무음2개는 기존깊은bassref로정확원문을재생성했고 rawRMS -30.55/-28.52dBFS, peak0.229/0.217로실제발성확인했다. 감정·음색사용자승인을자동검증으로대체하지않는다. 새index vox_eris_candidates.json/vox_orsted_candidates.json와 vox_whole_verification.json 근거저장. 총고유Vox45개, 새Eris49.28초/62.681초gen,Orsted4.64초/7.891초gen(load제외). 공개generic renderer --plan/--index/--batch지원확장및원문literal0/AST/safeimport검증후재동결. 두worker종료/GPU해제, 모델추가다운로드/설치/전체오디오게시/Git작업없음. Root전체재조립및최종CPU검수로이어간다.
