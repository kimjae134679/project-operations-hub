#!/usr/bin/env bash
set -euo pipefail
mkdir -p ../dist/qa
capture_evidence() {
  adb shell screencap -p /sdcard/vd-qa.png || true
  adb pull /sdcard/vd-qa.png ../dist/qa/android.png || true
  adb logcat -d -t 1500 > ../dist/qa/android-logcat.txt || true
  adb shell dumpsys power > ../dist/qa/android-power.txt || true
  grep -E 'FATAL|ANR|chromium|WebView|Choreographer|PowerManager' ../dist/qa/android-logcat.txt | tail -80 || true
  grep -E 'mWakefulness|mScreenOffTimeoutSetting|Display Power' ../dist/qa/android-power.txt || true
}
trap capture_evidence EXIT
adb shell settings put system screen_off_timeout 1800000
adb shell svc power stayon true
adb install -r ../app/build/outputs/apk/debug/app-x86_64-debug.apk
adb shell pm grant kr.co.kjw.videodownloader android.permission.POST_NOTIFICATIONS || true
adb shell am instrument -w kr.co.kjw.videodownloader/.SmokeInstrumentation | tee ../dist/qa/native.txt
grep -q 'PASS: UI bridge' ../dist/qa/native.txt
if grep -q 'FAIL:' ../dist/qa/native.txt; then exit 1; fi
adb shell am start -n kr.co.kjw.videodownloader/.MainActivity
for attempt in $(seq 1 30); do
  adb shell uiautomator dump /sdcard/vd-window.xml >/dev/null
  adb pull /sdcard/vd-window.xml ../dist/qa/android-window.xml >/dev/null
  if grep -q '링크 추가' ../dist/qa/android-window.xml; then break; fi
  sleep 1
done
grep -q '링크 추가' ../dist/qa/android-window.xml
