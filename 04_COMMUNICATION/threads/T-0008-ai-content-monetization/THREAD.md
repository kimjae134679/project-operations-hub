# T-0008 — Threads 작업 이어가기

이 파일은 후속 작업을 위한 소통 기록입니다. 현재 구현·실행 상태는 실제 Threads 저장소와 열린 PR이 원본입니다. 기존 001~191 번호별 기록과 병행 Discovery 이력은 그대로 보존합니다.

## 2026-09-20 · 컷메이트 · 컷 편집기 마무리

사용자는 승인한 컷 편집기의 분할선 설명, 긴 원문 스크롤, 제목 테두리 강화와 단어별 색상 강조를 요청했습니다. Canva는 나중으로 미뤘습니다.

- 실제 변경: `app/source-cut-editor.html`과 모델·편집기·ZIP 코드, 기존 원문 입력 화면 연결. 표지/본문 선택·원본 좌표 분할·스크롤/확대·경계 드래그 자동 스크롤, 여러 원문 순서, 제목 테두리 기본 8px(2~20px), 기본색/강조색과 강조 단어.
- 저장/출력: 원본을 포함한 편집 JSON 복원, 전체 PNG ZIP. 출력 너비 1080px, 선택 비율 유지. 이미지당/합계 메모리 제한과 너무 긴 컷의 추가 분할 안내.
- 코드 기준: Threads `92bbbaec9fe09a94c1e0c5339aeb82952e4fe1cc`, [기존 PR #1](https://github.com/kimjae134679/Threads/pull/1). main 병합하지 않음.
- 검증: 로컬 `npm run check` 46개 suite 및 139개 JS 구문 검사 통과. 실제 스크립트와 native Canvas/DOM 더블로 스크롤·분할·제목·저장/복원·ZIP 생성 확인. 독립 Python ZIP 판독과 실제 HTTP 경로 200 확인. 이 커밋의 GitHub CI 실행은 기록 시점에 확인되지 않음.
- 미검증: 실제 브라우저 E2E/Windows 설치·다운로드. 미구현: URL 전체 자동 캡처, 기존 04 검수에 결과 되돌려 쓰기. 외부 계정 게시 없음. Canva 미연결.
- 첨부 원문 기반 예시 PNG의 공개 GitHub 업로드는 자동 승인 검토에서 외부 공개 승인 부족으로 차단되어 제외함. 코드는 사용자 요청 범위에서 PR에 반영.
- 다음 작업: 실제 브라우저에서 긴 원문 하나를 편집하고 ZIP 다운로드와 편집 JSON 복원 확인 → 기존 검수 단계 연결.
- 사용법 원본: Threads `docs/SOURCE_CUT_EDITOR.md`. 진행표: `docs/PRODUCTION_PROGRESS.md`.

작업 위치: 관리형 Linux `/workspace/scratch/7dc461d71eca/Threads`. Windows PC에 프로그램을 설치하거나 이동하지 않았으므로 `C:\Program Files\_My\AI` 경로 변경 없음. 새 외부 프로그램 설치 없음. 실행은 저장소 루트 `npm start`, 페이지는 `/app/source-cut-editor.html`; 이동 시 저장소 상대경로를 유지합니다.
