@echo off
echo 🚀 Simple Heroku Deploy
echo =======================

set HEROKU=D:\workspace\heroku\bin\heroku.cmd

echo Step 1: Creating unique app name...
set APP_NAME=p2p-server-%date:~10,4%%date:~4,2%%date:~7,2%%time:~0,2%%time:~3,2%
set APP_NAME=%APP_NAME: =%

echo App name: %APP_NAME%
echo.

echo Step 2: Creating Heroku app...
%HEROKU% create %APP_NAME%

echo.
echo Step 3: Deploying...
git push heroku master

echo.
echo Step 4: Opening app...
%HEROKU% open

echo.
echo ✅ Deploy complete!
echo Your server URL: https://%APP_NAME%.herokuapp.com
echo.
pause
