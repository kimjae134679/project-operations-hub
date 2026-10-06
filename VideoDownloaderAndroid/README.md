# 영상다운로더 Android

설치파일: [Galaxy S22·64비트 Android용 APK 1.0.2](https://github.com/kimjae134679/project-operations-hub/releases/download/video-downloader-android-v1.0.2/VideoDownloader-1.0.2-arm64-v8a.apk). [전체 배포 파일](https://github.com/kimjae134679/project-operations-hub/releases/tag/video-downloader-android-v1.0.2)에서 32비트 APK와 SHA256 체크섬도 확인할 수 있습니다.

휴대폰에서 링크를 입력하고 이름 있는 목록으로 관리한 뒤 영상·음원을 직접 저장하는 앱입니다. PC 서버가 필요 없습니다. Android 10 이상이며 Galaxy S22는 arm64-v8a 설치파일을 사용합니다.

- [설치·자료·사용 안내](프로젝트_사용안내.md)
- [Android 소스](app/src/main/)
- [화면 QA](qa/)
- [실제 검증 범위와 남은 한계](검증결과.md)
- 검증 후 생성되는 설치파일·화면·검증 근거: `dist/`

## 기능

링크 한 개/여러 개 입력, 다른 앱에서 링크 공유, 목록 생성/이름 변경/삭제, 검색·상태 필터, 다중 선택 다운로드/이동/삭제, 진행 알림, 일시정지/이어받기, 파일 열기/공유, TXT·JSON 가져오기와 JSON 내보내기, 밝은/어두운 테마를 제공합니다.

yt-dlp Android 0.18.1, QuickJS, FFmpeg를 앱에 포함합니다. 최신 추출기는 하루에 한 번 확인하고 설정에서 수동 업데이트할 수 있습니다. MP4를 우선 저장하며 불가능하면 MKV/원본 형식으로 재시도합니다. MP3 변환이 실패하면 원본 음원으로 재시도합니다. HLS/DASH, 직접 미디어 URL, 지원 사이트와 공개 HTML/iframe 추출을 사용합니다. 다운로드 길이 필터는 없습니다. 길이를 확인하지 못해도 저장을 시도합니다.

동적 페이지·로그인 영상은 브라우저에서 재생 후 찾은 미디어를 목록에 추가하거나 로그인 후 원래 주소를 재시도할 수 있습니다. 외부 웹페이지에는 앱의 권한 있는 JavaScript 브리지를 노출하지 않습니다. 로그인 쿠키는 앱 내부에 호스트 범위를 지정해 저장하고 일반 목록 백업에서 제외합니다.

모든 사이트의 성공을 보장하지 않습니다. DRM, 권한 없는 비공개 영상, 사이트 차단, 만료된 링크, 실제 스트림이 노출되지 않는 페이지는 실패할 수 있습니다. 일부만 성공한 재생목록은 성공 파일을 보존하고 나머지 재시도를 표시합니다. 저장 완료는 미디어 파일이 다운로드 폴더에 공개된 뒤에만 표시합니다.

## 빌드와 검증

JDK 21, Android SDK 35, Gradle 8.14.3:

```text
gradle :app:assembleDebug :app:testDebugUnitTest :app:lintDebug
```

ABI별 APK를 생성합니다. 개발 검증용 서명이며 Play Store 출시용 서명이 아닙니다. CI의 최초 서명키는 실행별로 달라질 수 있으므로 다음 업데이트 배포 때 서명 유지가 필요합니다. 앱을 삭제하면 내부 목록은 지워지므로 먼저 설정에서 JSON 백업을 내보내세요. 다운로드 폴더의 공개 파일은 유지됩니다.

GitHub Actions는 컴파일·단위 검사·Android lint, 360/412/800px UI·다크모드 검사, Android 35 에뮬레이터의 실제 MP4/HLS/DASH·짧은 영상·동명 파일·중단 복구 검증을 수행합니다. 실제 Galaxy S22 설치와 모든 외부 사이트의 다운로드는 별도 확인 범위입니다.

디자인 참고: [Seal](https://github.com/JunkFood02/Seal)의 링크 추가/다운로드 상태 흐름, [Material](https://m3.material.io/)의 큰 터치 영역/하단 메뉴/선택 동작. 디자인·레이아웃은 이 프로젝트에서 작성했습니다.

라이브러리 출처·라이선스: [yt-dlp](https://github.com/yt-dlp/yt-dlp), [youtubedl-android](https://github.com/yausername/youtubedl-android), [FFmpeg](https://ffmpeg.org/legal.html), [jsoup](https://jsoup.org/license).
