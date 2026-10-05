# Higgs 음성 후보 비교 완료 · 청취 승인 대기

작성자: /root/higgs_bench. 세션: 20261005-voice-parallel.

실제 프로젝트 D:/AI/VoiceAudiobook와 기존 자료·환경을 보존했습니다. 새 환경은 D:/A_KJ/AI/Workspace/VoiceBench/higgs-env, 모델은 D:/A_KJ/AI/Models입니다. 바탕화면 변경 없음. GPU는 gpu_guard로 순차 사용했고 다운로드와 CPU 검사는 병렬로 진행했습니다. 공유 원격 연결·독립 앱·서비스를 종료하지 않았습니다.

Higgs Windows Transformers 포트와 별도 codec을 고정 revision/LFS SHA256으로 검증했습니다. 독립 Torch 2.6 cu124·Transformers 5.5 환경에서 실제 WAV 8개 생성 완료, 오류 0개. WAV 전체 디코딩·SHA·프레임 수·원문 메타데이터·대상 ID 순서·중복 바이트 검사 통과. 원문·키·가중치·오디오를 공유 자료나 Git에 넣지 않았습니다.

실제 BF16 backbone CUDA + FP32 codec CPU 구성입니다. 이 프로세스의 생성 중 GPU allocated peak는 8,261,553,152 bytes(7.69 GiB), 모델 적재는 7.197초였습니다. 음성 15.72초 / 측정 생성 88.634초 = 가중 RTF 5.638이며 참조 인코딩·초기 적재를 제외합니다. 초기·후기 표본의 본문이 달라 확정된 예열 효과로 주장하지 않습니다. OOM이 없어 구현한 CPU offload fallback은 실측하지 않았습니다. missing head 경고는 같은 loader의 추가 검사에서 embedding과 저장 공간·값이 같다는 것을 확인했습니다.

공용 코드 benchmark_higgs.py, 설명 docs/HIGGS_TTS3_BENCHMARK.md, 비공개 결과 output/battle_action_20261005/higgs_candidates.json 및 higgs_verification.json. ASR·MP3·청취 색인은 root 담당으로 연결했고 human_approved=false를 유지합니다. 파일·모델 실행 성공은 한국어 연기·감정의 사용자 청취 승인과 별개입니다.

최신 공지 6개를 실제 읽고 본인 확인 기록 6개를 로컬로 남겼습니다. 중앙 수집·동기화 완료는 별도로 미확인입니다.


162/163 후속 교체 합성 완료: 에리스 27개와 전투 4개, 총 31개. 검증된 이전 7개 재사용, 신규24개 생성(0036 고유명사 재시도 포함), 오류0. 신규53.40초/생성86.7672초, 전체 교체audio67.04초. 새 실행 loader96.43초/codec포함98.36초, 신규Torch allocated peak8,234,084,864 bytes. 결과 `output/battle_expressive_20261005/replacement_records.json`이며 human_approved=false, lexical review pending. 원본 소설/manifest SHA, full WAV/24k/finite, unique ID/path/hash 및 원래 발화순서/시간구간 검증 통과. GPU worker 종료 및 lock 해제. MP3/reader/ASR 승격은 root가 수행한다. 개인용 synthetic Korean young Eris ref만 사용했고 강제 피치/속도 후처리를 하지 않았다.

Root CPU ASR/파형31개 점검완료: wording검토3개(163-0094=.6000,0098=.6000,0177=.5882), severe0. 0036 재생성=.7500/warning0. 음성연기/고유명사 발음 human listening승인=false 유지. 기록파일의 ASR pending은 합성시점 metadata이며 진단결과는 별도 speech_checks.json. 실제 오류 근거가 없으므로 추가 재합성하지 않았다.


상황/전투 설명 후속 완료: 완전한 기존 발화12블록/1710자(나레이터6,루디내면6,각원래ref/identity유지),원문·TTStext그대로,감정/외침/속도/피치태그0. 기존31+설명12=43원시교체records. context audio270.04초/valid generation406.9265초→RTF1.5069(load/refencoding/rejectedtakes제외),기존239.4초보다자연duration12.8%증가. acceptedpeak8,401,349,120B. loadLM/full3회실측124.775/126.920,4.104/4.462,3.877/4.148초는통제비교아님. 고립된0004/0099첫take만55.72초cap발생,diagnostic보존·승격제외하고seed변경/uniquevariant로21.96/17.40초재생성. 최종context cap0,maxdelay746/limit1400. 새로운협력pauseflag와boundedresidentretry추가. output/battle_situation_20261005의replacement_records/situation_verification/replacement_verification으로full WAV/SHA/원문해시/ID/path/원래순서검증통과. root ASR/MP3/reader승격담당,humanapproval=false. 두모델fullfileSHA를읽기전용재계산해actual==pinnedLFS확인,각모델dir local_sha256_verification.json와runaggregate receipt저장. readonlyupstreammetadata변경없음. GPUworkers정상종료.

최종 정합성 확인: root CPU speech_checks.json 실제읽기 기준43점검완료,severe0,context12 ASR범위 .9082~.9890/warning0,재생성0004=.9082/0099=.9733,이전짧은에리스3개의wordingreview만남음,humanfalse. root최종게시 완료 보고: 현재폴더20261005_162-163_전투와상황설명,16226:29/16333:19,desktop17/mobile17검사통과. 문서에43/context12/270.04초/RTF1.5069/peak8,401,349,120B와phase별load를구별했고,rejected cap승격없음/SHAreceipt존재를확인. 게시시경로수정으로serialized manifestSHA는바뀔수있으나원문bytes/source_text/화자/순서는보존됨을명시. public코드·문서수정종료,추가생성·Git작업하지않음.

## 최신 사용자 평가 적용 — CPU 문맥 감사

- actor: /root/higgs_bench; session: 20261005-voice-parallel.
- Higgs/ZONOS2의 에리스 행방 확인 표본과 옛 한국어 에리스 기준은 거부. 기존 31/43개 Higgs 배치는 파일·ASR 검증 checkpoint일 뿐 사용자 승인 아님. Higgs 기본 에리스 교체를 중단하는 정책을 사적 plan과 공개 feedback 문서에 반영함.
- Qwen3 VoiceDesign eris_young_1: 실제 3.76초/seed202610051 선호, eris_young_2: 2.88초/seed202610052 거부. Cosy 젊은 출력의 음질 선호와 전투에 부적합한 평안한 연기 거부를 별도 기록. Cosy 입력 eris_young_ref 5.60초는 선호3.76초와 다른 파일이며 독립 승인 없음. 네 참고음 파일을 SHA-256으로 식별.
- 에리스 전27발화 405자: 현재 전투22, 집 회상2, 검술 회상2, 전투 중 집 발화를 떠올리는 회상1. 내면4·짧은 반응·기술호명 구분. 원래 침묵3개 보존. 앞뒤 문맥과 대상·긴장도·장면 의도를 사적 metadata로 기록하고 source_text/text/source_parts/speaker/voice_id와 현재 manifest 동일 검사 통과.
- 실피4/록시1도 검토: 집 회상에서의 울먹임, 두려움 속 결심, 긴급 철수, 보호·불신 경고, 해소되지 않은 협상 불안을 구분함. Qwen Base는 instruct API가 없어 acting intent를 지원 기능으로 과장하지 않음.
- 사적 결과: output/battle_feedback_20261005/{eris_context_review,reference_identities,supplemental_context_review,candidate_feedback,feedback_verification}.json. 기존 eris_context_plan.json은 worker가 사용 중인 입력으로 보존; refined review는 같은 source/IDs를 유지. 12개 실제 비교후보에 최신 평가 적용, 신규 생성물 human_approved=false.
- docs/VOICE_FEEDBACK_20261005.md 신규, docs/HIGGS_TTS3_BENCHMARK.md 최신 평가 notice 추가. 소설 원문은 공개 문서/코드에 넣지 않음. 기존 공개 코드 변경·GPU 작업·추가 모델 설치·Git 작업 없음. Root 및 Qwen/Vox worker에 plan/구분 전달 완료.

## 2026-10-06 7배역 오디션 원문 선정 완료

- actor /root/higgs_bench. AGENTS 및 CURRENT_HANDOFF 재독, 현재 공지 check --read에서 본인 적용·해당없음 기록 존재 확인.
- output/cast_audition_20261006/selection_plan.json: 루데우스/루데우스 내면/나레이터/올스테드/에리스/실피/록시 각2개, 총14개·1023자·최대158자. 완결된 원문 발화/설명 블록을 그대로 선택하고 임의문장절단 없음. 기존 동일 ID cache13개 실제존재, 록시13-03929는 cache 없음.
- 실제13.txt 및16.txt SHA·원문일치·참고음7개 voice_id 일치 확인. 배역 display명과 source_speaker를 분리해 내면·나레이터·올스테드의 원래POV/배역을 유지함. source_parts·source_ids·원본/record SHA·앞뒤문맥을 사적 plan에 보존.
- 록시13-03929/03940, 루디13-05739, 실피13-05740은 실제13.txt 고유행·주변명시 행동·응답교대를 직접 확인. 전권 자동 화자 semantic hold 해제 아님. chapter127/131 및162/163에서만 제한적으로 검토.
- 실피는 신규 수용대화와 이전 긴0029 동일문장 개선시험을 함께 선택. 에리스는 침착한 전술대화와 긴급 역할분담명령을 구분하고 평안한 전투 연기를 배제하는 의도를 별도 metadata로 기록. 새 합성은 root worker가 맡고 human_approved=false 유지.
- source_selection_verification.json:14개/최대158자/모든baseline경로존재/plan SHA 기록. GPU·모델 설치·공개코드 수정·Git 작업 없음. 원문 포함 JSON/문맥 자료는 사적 output이며 공개전송하지 않음.


## 원문 화자 검수 완료 — /root/higgs_bench

- 담당 35개 회차: 151–154, 200–212, 217–228, 900090, 260–261, 900125–900127. 23권은 병렬 담당 firered로 인계하여 중복 변경하지 않음.
- private audit: output/production_15_24/audits/chapter_<번호>.json
- 검증 기록: output/production_15_24/audits/higgs_manual_review_completion.json
- 원문 문맥을 직접 읽고 화자·내면/나레이터 시점·회상/서신·집단/비언어 반응·본문 완료 경계를 기록. 인용 대사 시작 3,271개; 인용 시작 누락 0, null 화자 0. 원문 15/20.0/21/24 전체 SHA-256 일치.
- 익명 발화자는 현장 역할로만 지정하고 성별 불명확은 unspecified. 실제 집단은 collective=true; 동물은 sound_effect/tts_generation=false. 기존 고정 배역은 재사용.
- hold는 기존 및 신규 위치와 이유 메타만 보존하며 실행하지 않음. 원문 내용은 이 공용 기록에 복사하지 않음.
- 원문 TXT·오디오·모델·GPU·공개 코드·Git·Desktop 변경 없음. 음성의 인간 청취 승인과 실제 생성 완료를 이 검수 결과로 주장하지 않음.
