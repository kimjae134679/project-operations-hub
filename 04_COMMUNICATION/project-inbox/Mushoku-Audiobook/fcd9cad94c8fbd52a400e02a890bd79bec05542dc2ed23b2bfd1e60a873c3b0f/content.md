# 회차 뷰어·제작목록 인수인계 — /root/app_cleanup

기존 book.chapters 형식 유지. 선택 회차 제목·설명을 줄바꿈해 표시하고, 이전/다음 회차와 접이식 회차 검색을 제공합니다. 128목록을 펼쳐도 재생 dock 높이가 늘지 않습니다. synthetic numeric 막간ID는 사용자 회차번호로 노출하지 않습니다. 같은 인물본문 묶기/이름1개/색(Rudy황토)/문장클릭 유지, 인물발화 찾기는 single-pass 캐시를 재사용합니다.

제작목록 ui/production_library.html inline __PRODUCTION_DATA__ 계약: title,chapters[{chapter,title,description,status,seconds,reader,volume?}],refreshSeconds?,statusUrl?. ready와reader가함께있을때만 본문듣기 활성. 파일fetch제한이면 inline 유지. 기본30초 해당목록만reload하며 검색/필터/scroll/접힘 상태 보존. actual출판/상태와오디오생성은root담당입니다.

한 창 shell에 제작목록 진입을 추가, audiobooks/library_latest.json 우선 및 output/production_15_24/00_제작목록.html fallback. 기존162/163과 production readerURL별프레임을 보존하고 history로 왕복시본문·검색·스크롤·재생위치유지. 원문/모델/음성/과거전체HTML/기존비교/바탕화면을 변경하지 않았습니다. EXE기존열린창 강제닫지않음.

검증: 1366/390 실제Chrome 기존본문32각통과, 기존본문·최신비교왕복33각통과, generic128layoutfixture37각통과. 128제작완료·신규음성청취승인이 아닙니다. 증거 output/reader_design_20261006/production_design_checks.json,existing32/reader_cast_checks.json,navigation/navigation_checks.json 및 desktop/mobile PNG. 실제최신상태 연결은 root initialshelf게시후재확인.

제품 참고: https://support.apple.com/en-ca/guide/iphone/iphac1971248/ios (회차목록을필요할때열기), https://audiobookshelf.org/docs/documentation/introduction/ 및 https://audiobookshelf.org/docs/documentation/libraries/common-content/overview/ (목록검색과현재재생분리). N-0002 r1/N-0005 r2 본인 실제읽음·적용기록 작성. 중앙수집성공은 주장하지 않음.

공개 변경목록: ui/audiobook_reader.html,ui/production_library.html,ui/application_shell.html,app/AudiobookLauncher.cs,app/check_navigation.py. src production 코드/Git commit은 root담당이며 본인은 수정·실행하지 않았습니다.


최종 앱·뷰어 확인 (2026-10-06 03:56 KST)

- `무직전생.exe`와 `오디오북제작.exe` 새 설치 완료. 이전 EXE는 SHA를 확인해 `app/launcher_archive`에 보존했으며 기존 열린 사용자 창을 닫지 않았습니다. 새 기능은 기존 창을 사용자가 닫은 뒤 다음 실행에 적용됩니다.
- 실제 전체 본문·최신 비교·128 제작목록 연결은 데스크톱 1366/모바일 390 각각 38개 검사를 통과했습니다. 검색/인물/회차/재생 위치/스크롤 복원, 뒤·앞 이동, 추가 창 없음, 숨긴 음성 정지와 이전 재생만 복귀를 확인했습니다.
- 128회차 테스트용 데이터는 각각 41개 검사 통과: 접이식 검색 목록이 본문 dock를 늘리지 않으며 제목/설명이 줄바꿈됩니다. 같은 사람을 묶고 문장 클릭을 유지합니다. aggregate 동일 HTML의 `?chapter=n`은 회차 선택으로 연결되며 다른 회차를 눌러도 같은 reader frame을 재사용합니다. 테스트용 데이터는 실제 제작 완료본으로 안내하지 않습니다.
- 제작 앱은 supervisor로 연결됩니다. 실제 캠페인 상태를 읽기만 하는 `--check` 두 앱이 통과했습니다. 시작/재개·작업 후 중지·중복 방지·paused 상태의 resume_requested 변경·본문 state 보존·부모 종료 후 로그는 별도 가짜 경로에서 검증했습니다. 실제 생성/중지/사용자 창 종료/바탕화면 변경은 하지 않았습니다.
- 변경 소스 8개: `ui/audiobook_reader.html`, `ui/production_library.html`, `ui/application_shell.html`, `app/AudiobookLauncher.cs`, `app/check_navigation.py`, `app/ProductionLauncher.cs`, `app/build_production.ps1`, `app/launch_production.py`. production src·모델·원문·기존 음성·현재판 포인터는 담당 root가 관리합니다.
- 증거: `output/reader_design_20261006/final_app_verification.json`, `final_navigation/navigation_checks.json`, `production_design_checks.json`, `production_controls_checks.json`; 실제 앱 `temp/app_check.json`, `temp/production_app_check.json`.
- 화면: `output/reader_design_20261006/final_navigation/desktop.png`, `mobile.png`, `production_desktop_library.png`, `production_mobile_128menu.png`, `temp/production_app_preview.png`.
- 새 144회차 실제 aggregate 본문은 publication 이후 별도로 확인할 예정입니다. 현재 128회차 전체 제작이 완료됐다는 주장은 하지 않습니다.
