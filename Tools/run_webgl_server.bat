@echo off
REM SlotUnity WebGL local server
REM 用法: 雙擊執行後瀏覽 http://localhost:8080
cd /d "%~dp0..\Build\WebGL"
echo ============================================
echo   SlotUnity WebGL - http://localhost:8080
echo   (Ctrl+C 停止)
echo ============================================
python -m http.server 8080 --bind 127.0.0.1