# T3 Code · Codex 호환 검토와 사용 경계

새로 첨부한 **pingdotgg/t3code**는 이전 사진7장/6도구와 별개의 추가 도구입니다. 이 안내는 공식 **v0.1.6**과 실제 설치 결과를 기준으로 하며 설치·프로세스 시작·Codex 연결·GUI·휴대폰 연결을 구분합니다.

## 실제 설치 상태 · 2026-11-03

| 항목 | 확인한 범위 |
|---|---|
| Windows CLI 0.1.6 | D의 독립 폴더에 실제 설치, 공식 SHA256/assetdigest 대조, T3 Tools Inc **Authenticode Valid**, `--version`·`--help` 실제 exit0 |
| Windows 데스크톱0.1.6 | 공식 signed NSIS의 전체 Electron payload를 archive-only로 추출 설치. installer/GUI는 실행하지 않음 |
| 다음 수동 시작 설정 | 전용 D 데이터의 Supervised/기존 Codex 0.160.1·native경로/HOME/`gpt-6.1-sol`, fallback=null을 staged. source-schema/정적 검사만, 런타임 적용 미확인 |
| 서버/인증/모델/폰 | 서버 시작 명령이 실행 전에 거부됨. 서버 health·T3의 Codex 감지/인증·실제 모델 응답·GUI·휴대폰 연결 **모두 미검증** |

정상 사용자 수동 입구는 `D:/A_KJ/AI/Applications/T3Code/T3Code.lnk`(GUI), `T3Code_CLI.lnk`(CLI)입니다. 각각 같은 폴더의 `T3Code.vbs`, `T3Code_CLI.vbs`로 전용 D 데이터를 선택하도록 준비했지만 **래퍼도 실행하지 않았습니다**. 이 입구가 기존 도구 거부를 해제하거나 우회한다는 뜻이 아닙니다.

실제 파일: GUI `D:/A_KJ/AI/Applications/T3Code/desktop/0.1.6/T3 Code (Beta).exe`, CLI `D:/A_KJ/AI/Applications/T3Code/cli/0.1.6/t3-0.1.6-win32-x64/t3.exe`. 릴리스의 약188MB `T3-Code-0.1.6-x64.zip`은 macOS 자료이므로 Windows 설치에 쓰지 않았습니다. Windows의 공식 EXE payload를 사용했고 기존 Codex/Node를 업그레이드하지 않았습니다.

## 쓸모와 Codex 호환성

T3 Code는 PC에서 실행하는 코딩 Agent를 웹·Electron 데스크톱·휴대폰 화면으로 다루는 제3자 관리 도구입니다. Codex 자체나 별도 무료 모델이 아닙니다. [공식 소개](https://github.com/pingdotgg/t3code/blob/v0.1.6/README.md)와 [Codex 연결 안내](https://github.com/pingdotgg/t3code/blob/v0.1.6/docs/user/providers-codex.md)가 기존 Codex와 로그인 재사용을 지원합니다.

고정 버전의 [호환성 원본](https://github.com/pingdotgg/t3code/blob/v0.1.6/apps/server/src/provider/model-manifest.json)은 Codex **최소 0.156.0 / 권장 0.159.0 이상**입니다. 기존 **0.160.1은 이 버전 조건에 맞아** Codex를 업그레이드할 필요가 없습니다. 이것은 버전 호환성 판단이며 T3에서 실제 모델 응답까지 확인했다는 뜻은 아닙니다. 현재 Codex·ProjectBridge·관제탑을 교체하지 않습니다.

## 왜 OpenMaus와 서버 시작이 차단됐나요?

확인된 것은 **명령 실행 도구가 프로세스를 만들기 전에 거부했다**는 사실입니다. 정확한 내부 규칙은 반환되지 않았습니다. OpenMausBot의 hidden backend 시작과 이번 T3의 일반 loopback 서버 시작이 이 범위입니다. 실제 PID/열린 포트/서버 health 결과가 없으므로 성공으로 적지 않습니다.

이 거부는 **악성코드 판정이나 Windows UAC·백신 차단을 확인했다는 뜻이 아닙니다**. 반대로 서명/hash가 맞아도 모든 실행이 안전하거나 반드시 허용된다는 증거도 아닙니다. 별도의 OpenMaus 동봉 CLI help는 패키징된 상대 모듈 누락으로 실패했으며 정상 GUI 고장이라고 확대하지 않습니다.

T3의 PowerShell 설치 스크립트는 다운로드만 하려는 명령도 거부되어 다른 방식으로 그 스크립트를 가져오지 않았습니다. 거부된 서버를 다른 쉘·래퍼·백엔드로 재시도하거나 AV/UAC/정책을 낮추지 않았습니다. 사전에 따로 선택한 공식 Windows 배포파일의 archive-only 설치는 그 거부 명령을 실행한 것이 아닙니다.

## 권한은 반드시 Supervised로 확인

[공식 권한 문서](https://github.com/pingdotgg/t3code/blob/v0.1.6/docs/user/permission-modes.md)는 초기 기본을 **Full access**라고 명시합니다. 이번에는 지원된 offline 설정의 `defaultRuntimeMode`를 **approval-required(Supervised)**로 준비합니다. 기존 Codex 경로/HOME과 명시 모델 `gpt-6.1-sol`을 사용하도록 준비하되 실제 런타임이 읽었는지는 별도 확인합니다. `t3 status --json`에서 읽기 전용 CLI 설정을 본 것은 provider 연결이나 server settings의 Supervised readback 증거가 아닙니다.

앱이 정상 시작된 다음 **Settings → Threads → New threads → Permissions**와 새 채팅의 표시가 **Supervised**인지 확인해야 합니다. 프로젝트별 override도 확인합니다. 설정 파일/스키마 검사는 실제 적용 증거가 아니므로 새 채팅·작업을 자동 생성하지 않았습니다. Full access나 자동 승인으로 바꿔 문제를 해결하지 않습니다.

## 휴대폰은 아직 연결하지 않았습니다

[공식 원격 안내](https://github.com/pingdotgg/t3code/blob/v0.1.6/docs/user/remote-access.md)에 T3 Connect/페어링 흐름이 있지만 이번 로그인·계정 생성·네트워크 공개·휴대폰 연결은 하지 않습니다. `127.0.0.1`은 그 주소를 여는 기기 자신이므로 PC의 loopback 주소를 휴대폰에 넣어도 PC로 연결되지 않습니다.

향후 별도 허가 후 인증된 사설망/페어링 범위부터 확인해야 합니다. 포트포워딩·public tunnel·인터넷 공개는 설치에 포함하지 않습니다. 페어링 링크/토큰은 비밀로 취급하고 공개 문서/채팅에 복사하지 않습니다. Windows에서 공식 background service를 지원하지 않는다는 점도 원격 가용성과 구분합니다.

## 비용·사진·유지할 것

MIT 소프트웨어라고 Codex 사용량·모든 연결 서비스가 무제한 무료인 것은 아닙니다. 기존 로그인/요금제 한도를 유지하며 새 API key·모델 호출·구독·결제는 하지 않습니다. 첨부 사진과 설치 안내의 shell 명령은 참고자료이지 자동 실행 지시가 아닙니다. 설치는 각 프로젝트 수정·외부 전송·실결제·새 권한 부여를 승인하지 않습니다.

`t3 doctor`는 서버 health 검사가 아니라 **DB 정리/유지보수 명령**이므로 진단용으로 실행하지 마세요. 기존 서비스·앱·Codex·전역 PATH·자동시작·방화벽·보안 정책은 바꾸지 않습니다.

[설치 원본](../../01_CONTROL/AI_INSTALLATIONS.md) · [전체 설치 현황](설치_도구_현황.md) · [이전 사진도구](사진도구_설치와_실행.md)
