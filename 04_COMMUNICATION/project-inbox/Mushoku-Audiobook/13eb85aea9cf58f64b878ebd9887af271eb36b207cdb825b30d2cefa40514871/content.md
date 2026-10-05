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
