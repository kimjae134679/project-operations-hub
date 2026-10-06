#!/usr/bin/env bash
set -euo pipefail
mkdir -p ../dist/qa
adb install -r ../app/build/outputs/apk/debug/app-x86_64-debug.apk
adb shell pm grant kr.co.kjw.videodownloader android.permission.POST_NOTIFICATIONS || true
adb shell am instrument -w kr.co.kjw.videodownloader/.SmokeInstrumentation | tee ../dist/qa/native.txt
grep -q 'PASS: UI bridge' ../dist/qa/native.txt
if grep -q 'FAIL:' ../dist/qa/native.txt; then exit 1; fi
adb shell am start -n kr.co.kjw.videodownloader/.MainActivity
adb shell screencap -p /sdcard/vd-qa.png
adb pull /sdcard/vd-qa.png ../dist/qa/android.png
adb logcat -d -t 300 > ../dist/qa/android-logcat.txt
