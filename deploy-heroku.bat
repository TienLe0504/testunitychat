@echo off
echo 🚀 Heroku Deploy Script
echo =======================

echo.
echo Step 1: Install Heroku CLI (if not installed)
if not exist "heroku-installer.exe" (
    echo Downloading Heroku CLI...
    curl -o heroku-installer.exe https://cli-assets.heroku.com/heroku-x64.exe
)

echo.
echo Please install Heroku CLI by running: heroku-installer.exe
echo Then restart this script.
echo.
pause

echo.
echo Step 2: Login to Heroku
heroku login

echo.
echo Step 3: Create Heroku app
set /p appname="Enter unique app name (e.g. my-p2p-game-server): "
heroku create %appname%

echo.
echo Step 4: Deploy to Heroku
git add .
git commit -m "Deploy to Heroku"
git push heroku master

echo.
echo Step 5: Open your app
heroku open

echo.
echo ✅ Deployment complete!
echo Your server URL: https://%appname%.herokuapp.com
echo.
echo 🎯 Next steps:
echo 1. Copy the URL above
echo 2. Update PublicServerClient.cs SERVER_OPTIONS with your URL
echo 3. Test P2P connections!
echo.
pause
