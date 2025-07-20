@echo off
echo 🚀 Quick Deploy Script
echo ======================

echo.
echo Choose deployment method:
echo 1. Deploy to Heroku (requires Heroku CLI)
echo 2. Deploy to Railway (requires Railway CLI)  
echo 3. Deploy to Render (manual via Git)
echo 4. Use ngrok for temporary tunnel
echo 5. Show deployment commands only

set /p choice="Enter choice (1-5): "

if "%choice%"=="1" goto heroku
if "%choice%"=="2" goto railway
if "%choice%"=="3" goto render
if "%choice%"=="4" goto ngrok
if "%choice%"=="5" goto commands
goto end

:heroku
echo.
echo 📦 Deploying to Heroku...
echo.
echo Step 1: Login to Heroku
heroku login
echo.
echo Step 2: Create app (change 'your-game-server' to unique name)
heroku create your-game-server-p2p
echo.
echo Step 3: Deploy
git add .
git commit -m "Deploy P2P game server"
git push heroku main
echo.
echo ✅ Deployment complete! Your server URL will be shown above.
goto end

:railway
echo.
echo 🚂 Deploying to Railway...
echo.
echo Step 1: Login to Railway
railway login
echo.
echo Step 2: Create project
railway new
echo.
echo Step 3: Deploy
railway up
echo.
echo ✅ Deployment complete! Check Railway dashboard for URL.
goto end

:render
echo.
echo 🎨 Deploy to Render (Manual):
echo.
echo 1. Go to https://render.com and sign up
echo 2. Connect your GitHub repo
echo 3. Create new "Web Service"
echo 4. Set Build Command: npm install
echo 5. Set Start Command: npm start
echo 6. Deploy!
echo.
pause
goto end

:ngrok
echo.
echo 🌐 Using ngrok tunnel...
echo.
echo Make sure ngrok is installed: https://ngrok.com/download
echo.
start cmd /k "echo Starting server... && node server.js"
timeout /t 3
start cmd /k "echo Creating tunnel... && ngrok http 3000"
echo.
echo ✅ Server and tunnel started in separate windows!
echo Copy the ngrok URL (https://xxxxx.ngrok.io) to use in client.
goto end

:commands
echo.
echo 📋 Manual Deployment Commands:
echo.
echo === Heroku ===
echo heroku login
echo heroku create your-unique-app-name
echo git add .
echo git commit -m "Deploy server"
echo git push heroku main
echo.
echo === Railway ===  
echo railway login
echo railway new
echo railway up
echo.
echo === Render ===
echo 1. Push code to GitHub
echo 2. Connect repo at render.com
echo 3. Build: npm install
echo 4. Start: npm start
echo.
echo === ngrok ===
echo node server.js (in one terminal)
echo ngrok http 3000 (in another terminal)
echo.
pause
goto end

:end
echo.
echo 🎯 Next steps after deployment:
echo 1. Copy your public server URL
echo 2. Update PublicServerClient.cs SERVER_OPTIONS
echo 3. Test P2P with real internet connections!
echo.
pause
