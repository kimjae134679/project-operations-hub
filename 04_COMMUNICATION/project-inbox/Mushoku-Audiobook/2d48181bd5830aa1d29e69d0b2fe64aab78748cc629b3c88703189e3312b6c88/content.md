# 회차 뷰어·제작목록 인수인계 — /root/app_cleanup

기존 book.chapters 형식 유지. 선택 회차 제목·설명을 줄바꿈해 표시하고, 이전/다음 회차와 접이식 회차 검색을 제공합니다. 128목록을 펼쳐도 재생 dock 높이가 늘지 않습니다. synthetic numeric 막간ID는 사용자 회차번호로 노출하지 않습니다. 같은 인물본문 묶기/이름1개/색(Rudy황토)/문장클릭 유지, 인물발화 찾기는 single-pass 캐시를 재사용합니다.

제작목록 ui/production_library.html inline __PRODUCTION_DATA__ 계약: title,chapters[{chapter,title,description,status,seconds,reader,volume?}],refreshSeconds?,statusUrl?. ready와reader가함께있을때만 본문듣기 활성. 파일fetch제한이면 inline 유지. 기본30초 해당목록만reload하며 검색/필터/scroll/접힘 상태 보존. actual출판/상태와오디오생성은root담당입니다.

한 창 shell에 제작목록 진입을 추가, audiobooks/library_latest.json 우선 및 output/production_15_24/00_제작목록.html fallback. 기존162/163과 production readerURL별프레임을 보존하고 history로 왕복시본문·검색·스크롤·재생위치유지. 원문/모델/음성/과거전체HTML/기존비교/바탕화면을 변경하지 않았습니다. EXE기존열린창 강제닫지않음.

검증: 1366/390 실제Chrome 기존본문32각통과, 기존본문·최신비교왕복33각통과, generic128layoutfixture37각통과. 128제작완료·신규음성청취승인이 아닙니다. 증거 output/reader_design_20261006/production_design_checks.json,existing32/reader_cast_checks.json,navigation/navigation_checks.json 및 desktop/mobile PNG. 실제최신상태 연결은 root initialshelf게시후재확인.

제품 참고: https://support.apple.com/en-ca/guide/iphone/iphac1971248/ios (회차목록을필요할때열기), https://audiobookshelf.org/docs/documentation/introduction/ 및 https://audiobookshelf.org/docs/documentation/libraries/common-content/overview/ (목록검색과현재재생분리). N-0002 r1/N-0005 r2 본인 실제읽음·적용기록 작성. 중앙수집성공은 주장하지 않음.

공개 변경목록: ui/audiobook_reader.html,ui/production_library.html,ui/application_shell.html,app/AudiobookLauncher.cs,app/check_navigation.py. src production 코드/Git commit은 root담당이며 본인은 수정·실행하지 않았습니다.
