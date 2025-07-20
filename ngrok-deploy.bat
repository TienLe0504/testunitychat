@echo off
echo 🌐 ngrok Deploy - MIỄN PHÍ & NHANH
echo ===================================

echo.
echo Bước 1: Download ngrok
echo - Vào: https://ngrok.com/download
echo - Extract file ngrok.exe vào folder này
echo.

if not exist "ngrok.exe" (
    echo ❌ Chưa có ngrok.exe!
    echo Download và copy vào folder: %cd%
    echo.
    pause
    exit
)

echo ✅ Found ngrok.exe
echo.

echo Bước 2: Start server (nếu chưa chạy)
echo Checking if server is running...
netstat -an | find "3000" >nul
if %errorlevel%==0 (
    echo ✅ Server is running on port 3000
) else (
    echo ❌ Server not running. Starting...
    start "P2P Server" cmd /k "echo Starting server... && node server.js"
    timeout /t 3
)

echo.
echo Bước 3: Create ngrok tunnel
echo Creating public URL...
start "ngrok tunnel" cmd /k "echo Creating tunnel... && ngrok http 3000"

echo.
echo ✅ Done! 
echo 1. Copy HTTPS URL từ ngrok window (https://xxxxx.ngrok.io)
echo 2. Paste vào PublicServerClient.cs 
echo 3. Test P2P từ 2 máy khác nhau!
echo.
pause
