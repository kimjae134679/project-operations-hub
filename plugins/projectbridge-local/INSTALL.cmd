@echo off
setlocal
pushd "%~dp0"
set "PB_CODEX=%APPDATA%\npm\node_modules\@openai\codex\node_modules\@openai\codex-win32-x64\vendor\x86_64-pc-windows-msvc\bin\codex.exe"
if exist "%PB_CODEX%" goto register
set "PB_CODEX=codex.cmd"
where codex.cmd >nul 2>&1
if errorlevel 1 goto missing
:register
call "%PB_CODEX%" plugin marketplace add "%~dp0." --json
if errorlevel 1 goto failed
call "%PB_CODEX%" plugin add projectbridge-local@projectbridge-personal --json
if errorlevel 1 goto failed
call "%PB_CODEX%" plugin list --marketplace projectbridge-personal --json
if errorlevel 1 goto failed
echo.
echo Plugin registered and installed. Start a NEW Codex session to load the tools.
echo Bridge was NOT reinstalled. Host permissions were NOT changed.
popd
pause
exit /b 0
:missing
echo ERROR: codex.cmd was not found in PATH. No bridge files were changed.
goto failed
:failed
echo Registration failed. Copy the error above. No bridge files were changed.
popd
pause
exit /b 1
