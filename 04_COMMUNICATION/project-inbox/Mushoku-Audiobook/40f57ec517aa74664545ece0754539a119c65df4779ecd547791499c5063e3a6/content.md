# VoiceStudio 로컬 설치·검토 요약

작성자 /root/voicestudio_setup · 세션20261005-voice-parallel · 무직전생 오디오북

VoiceStudio0.5.6을 D:\A_KJ\AI\Applications\VoiceStudio 에 수동 압축 설치했습니다. 공식 설치기 SHA256 확인 후 app-64.7z와 별도 데모5개를 구성했으며 NSIS설치기 실행과 바탕화면 변경은 없습니다. 실행은 VoiceStudio_DriveD.exe, 새 환경은 D:\A_KJ\AI\Workspace\VoiceBench\voicestudio-env, 실제 데이터/캐시는 D드라이브입니다. 기존 오디오북 D:\AI\VoiceAudiobook와 환경/원문은 유지했습니다.

cryptography 누락 오류를 별도 환경에서 해결하고 실제health200/RTXCUDA인식/API스키마/GUI초록시스템점검/포트3900backend1개를 확인했습니다. TTS모델 적재·합성·사용자 청취 승인은 아직미검증입니다. 통계동의와 클라우드계정/결제를 변경하지 않았습니다. 실제TF5.18/FastAPI0.136.3/MCP1.30/Torch2.6cu124이며 기존Qwen환경의CUDA라이브러리를읽기참조합니다.

중복OmniVoice와 불필요한추가모델팩5개 다운로드를 공식취소하고 정상앱종료/재시작하여jobs=[]확인. 부분캐시보존. 공유Omni가중치 최종검증과 GPU벤치마크종료 후 공식로컬모델경로로연결해야하며 연결후기본자동warmup의GPU적재에주의합니다. 자동업데이트다운로드/종료시자동설치는false지만버전체크는남아있습니다.

공용문서docs/VOICESTUDIO_LOCAL.md 및 재현용실행기소스tools/voicestudio/VoiceStudio_DriveD.cs 작성. 프로젝트Git privacy/syntax preflight에서 현재56실파일·23Python컴파일통과,stage없음,원문/오디오/모델/installer노출후보없음. 상세검증JSON/화면은로컬output에있으며Git에서제외합니다. 공지6개본인읽음/적용기록은로컬도우미로남깁니다. 본자료의로컬작성과중앙수집/동기화완료는별개이며수집확인은대기입니다.
