# AI 도구 현황 — 위치와 확인 수준

정리일: 2026-11-12 KST.
2026-11-11~12 설치·검증 기록과 11월 12일 읽기 확인을 구분해 정리했다.
이번 정리에서 새 모델 추론·그림 생성·웹검색·앱 실행 시험을 수행한 것은 아니다.

## 현재 로컬 AI 구성
| 도구 | 실제 위치·구성 | 확인 수준·용도 |
|---|---|---|
| Ollama 0.40.7 | 모델 <MODEL_ROOT>/models/ollama | 11월 12일 tags 읽기에서 gemma4:12b·bonsai-2:27b 딱 2개 확인. 기본 Gemma. 정리 중 ps 모델 0 |
| Cline | Plan/Act gemma4:12b, 컨텍스트 16384 | 11월 12일 설정 읽기 확인. 자동 승인 OFF. 승인 없이 파일·명령을 자동 실행하도록 바꾸지 않음 |
| FreeClaudeCode | <LOCAL_AI_ROOT>/FreeClaudeCode, 로컬 18082 | 기존 로컬 응답 검증 기록. 클라우드 Claude 무료 이용을 뜻하지 않음 |
| ComfyUI | <LOCAL_AI_ROOT>/ComfyUI | 필요할 때 실행. 프로그램 기동 확인과 실제 그림 생성·품질 검증은 별개 |
| Strata | <MODEL_ROOT>/Strata/Models/coder-IQ1_M | 큰 전문 모델·기존 설정 보존. 기본 대화 모델이 아니며 이번 정리에서 새로 로드하지 않음 |
| Vane | <LOCAL_AI_ROOT>/LocalAI-Setup/Vane | WSL만 설치된 기록. 재부팅·Docker·Vane 준비 미완료. 재부팅·후속 설치 자동 실행 없음 |

## 설치 위치 원칙
- <AI_ROOT>는 관리 자료·작업의 기본 루트이지 큰 모델을 D HDD에 강제하는 규칙이 아니다.
- 큰 모델은 확인한 E SSD 저장 위치를 사용한다. 현재 Ollama 모델 경로·환경은 E로 확인했다.
- 공식 앱·업데이터·환경이 관리하는 C/AppData 위치는 유지한다.
- 이 정리에서 기존 앱·환경·프로젝트·모델을 일괄 이동하거나 제거하지 않았다.
- 사용 설명·이미지·참고 파일은 해당 폴더나 보관 원문과 함께 유지한다.

## 기존 검증 기록
- 모델 비교 실제 답변·측정 (PC 로컬 참고 위치: <LOCAL_AI_ROOT>/LocalAI-Setup/model-evaluation-20261112/report.md)
- 설치 준비 상태 (PC 로컬 참고 위치: <LOCAL_AI_ROOT>/LocalAI-Setup/finish-20261112/final-readiness.json)
- Vane 미완료 상태 (PC 로컬 참고 위치: <LOCAL_AI_ROOT>/LocalAI-Setup/finish-20261112/vane-final-status.json)

## 이전 등록 도구
과거 MoneyPrinterTurbo·n8n·HyperFrames·Aider·Jev·관제탑·ProjectBridge·Unity·오디오북 등의 원문 등록은 아래에 그대로 보존했다.
이 도구들을 전부 지웠거나 현재 실행·인증까지 다시 검증했다는 뜻이 아니다.
실제로 재사용할 때 현재 경로·버전·권한·실행 상태를 확인한다.

[정리 전 등록·설치 이력 전체](../99_ARCHIVE/2026-11-12/rules-before-034346/01_CONTROL/AI_INSTALLATIONS.md)

## 현재 스킬·접근 도구
- 핵심 도구 4개: agent-desktop-windows, systematic-debugging, verification-before-completion, unslop.
- 앱 재현 단계 11개: 재현·클론 맥락에서 필요한 단계만 선택한다. 추가 10개 절차와 예전 과잉 절차는 날짜 보관하며 자동 적용하지 않는다.
- ProjectBridge는 현재 연결된 PC 파일·설정·명령 작업의 우선 도구다. 실행 접수와 실제 완료는 구분한다.
- agent-desktop은 허가된 Windows 앱 UI 작업용이다. 최근 version 명령 읽기 성공은 모든 앱 조작 성공·품질 보장이 아니다.
- cloud에 Windows CLI나 계정 연결이 있다고 가정하지 않는다. 실제 노출된 연결 capability와 사용자 승인 범위를 확인한다.
- 선별한 공개 자료의 일회 GitHub 동기화는 현재 승인 범위다. 성공 여부는 실제 commit/PR 증거로 구분하고 상시 자동 동기화로 확대하지 않는다.
