# React Bits 참조와 부분 디자인 예시
2026-10-05 사용자 요청: React Bits GitHub를 받아 쓸 만한 부분을 골라 적용하거나 부분 예시 여러 개를 먼저 보여 달라. 웹사이트 기반처럼 보인다는 지적 포함.

원본 다운로드 확인: D:\A_KJ\AI\Tools\react-bits
원본 GitHub: https://github.com/DavidHDev/react-bits
확인한 커밋: ca44b3f9ee180676a06d7de8ec6bea84cddff85b
선별해 읽은 원본: SpotlightCard, TiltedCard, AnimatedList, StaggeredMenu JSX/CSS.

현재 컨트롤타워는 .NET 9 WPF다. React 소스를 직접 붙일 수 없다. 빛 추종은 RadialGradientBrush/마우스 좌표, 목록 전환과 메뉴는 WPF Storyboard/TranslateTransform 등의 방식으로 다시 구현할 수 있다. 아직 해당 효과를 운영 앱에 구현하거나 네이티브 성능을 검증한 것은 아니다. Windows 실행 파일을 유지하면서 웹 UI를 WebView2 등에 담는 대안도 있으나, 비교 예시만으로 기술 전환을 결정하지 않는다.

채팅에서 프로젝트 선반(빛 반응+약한 깊이), 접속자 패널(선택/갱신 시 변화), 펼쳐지는 메뉴(면이 순차적으로 펼쳐짐)의 부분 비교 예시를 제공한다. 비교용 접속자는 가상 데이터이며 실제 서버 데이터나 실기기 캡처가 아니다. 장식이 과해 정보/휠/키보드/지연이 악화되지 않도록 실제 적용 단계에서 다시 검증한다. 사용자 피드백 전 디자인 완료로 기록하지 않는다.

원본 LICENSE.md는 MIT + Commons Clause다. 앱 일부로 사용 시 저작권/허가 문구를 유지하고 컴포넌트 자체 또는 이식판을 별도 묶음으로 재배포하지 않는다. 출처는 원본 포인터로 남기고 다른 프로젝트는 실제 최신 라이선스와 코드를 다시 확인한다.
