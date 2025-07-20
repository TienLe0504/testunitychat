# 🚀 Quick Deploy Script for PowerShell
Write-Host "🚀 Quick Deploy Script" -ForegroundColor Green
Write-Host "======================" -ForegroundColor Green

Write-Host ""
Write-Host "Choose deployment method:" -ForegroundColor Yellow
Write-Host "1. Deploy to Heroku (requires Heroku CLI)" -ForegroundColor White
Write-Host "2. Deploy to Railway (requires Railway CLI)" -ForegroundColor White  
Write-Host "3. Deploy to Render (manual via Git)" -ForegroundColor White
Write-Host "4. Use ngrok for temporary tunnel" -ForegroundColor White
Write-Host "5. Show deployment commands only" -ForegroundColor White

$choice = Read-Host "Enter choice (1-5)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "📦 Deploying to Heroku..." -ForegroundColor Blue
        Write-Host ""
        Write-Host "Step 1: Login to Heroku" -ForegroundColor Yellow
        heroku login
        Write-Host ""
        Write-Host "Step 2: Create app (change 'your-game-server' to unique name)" -ForegroundColor Yellow
        heroku create your-game-server-p2p
        Write-Host ""
        Write-Host "Step 3: Deploy" -ForegroundColor Yellow
        git add .
        git commit -m "Deploy P2P game server"
        git push heroku main
        Write-Host ""
        Write-Host "✅ Deployment complete! Your server URL will be shown above." -ForegroundColor Green
    }
    
    "2" {
        Write-Host ""
        Write-Host "🚂 Deploying to Railway..." -ForegroundColor Blue
        Write-Host ""
        Write-Host "Step 1: Login to Railway" -ForegroundColor Yellow
        railway login
        Write-Host ""
        Write-Host "Step 2: Create project" -ForegroundColor Yellow
        railway new
        Write-Host ""
        Write-Host "Step 3: Deploy" -ForegroundColor Yellow
        railway up
        Write-Host ""
        Write-Host "✅ Deployment complete! Check Railway dashboard for URL." -ForegroundColor Green
    }
    
    "3" {
        Write-Host ""
        Write-Host "🎨 Deploy to Render (Manual):" -ForegroundColor Blue
        Write-Host ""
        Write-Host "1. Go to https://render.com and sign up" -ForegroundColor White
        Write-Host "2. Connect your GitHub repo" -ForegroundColor White
        Write-Host "3. Create new 'Web Service'" -ForegroundColor White
        Write-Host "4. Set Build Command: npm install" -ForegroundColor White
        Write-Host "5. Set Start Command: npm start" -ForegroundColor White
        Write-Host "6. Deploy!" -ForegroundColor White
        Write-Host ""
        Read-Host "Press Enter to continue"
    }
    
    "4" {
        Write-Host ""
        Write-Host "🌐 Using ngrok tunnel..." -ForegroundColor Blue
        Write-Host ""
        Write-Host "Make sure ngrok is installed: https://ngrok.com/download" -ForegroundColor Yellow
        Write-Host ""
        
        # Start server in background
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'Starting server...' -ForegroundColor Green; node server.js"
        Start-Sleep 3
        
        # Start ngrok tunnel
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'Creating tunnel...' -ForegroundColor Green; ngrok http 3000"
        
        Write-Host ""
        Write-Host "✅ Server and tunnel started in separate windows!" -ForegroundColor Green
        Write-Host "Copy the ngrok URL (https://xxxxx.ngrok.io) to use in client." -ForegroundColor Yellow
    }
    
    "5" {
        Write-Host ""
        Write-Host "📋 Manual Deployment Commands:" -ForegroundColor Blue
        Write-Host ""
        Write-Host "=== Heroku ===" -ForegroundColor Green
        Write-Host "heroku login" -ForegroundColor White
        Write-Host "heroku create your-unique-app-name" -ForegroundColor White
        Write-Host "git add ." -ForegroundColor White
        Write-Host "git commit -m 'Deploy server'" -ForegroundColor White
        Write-Host "git push heroku main" -ForegroundColor White
        Write-Host ""
        Write-Host "=== Railway ===" -ForegroundColor Green
        Write-Host "railway login" -ForegroundColor White
        Write-Host "railway new" -ForegroundColor White
        Write-Host "railway up" -ForegroundColor White
        Write-Host ""
        Write-Host "=== Render ===" -ForegroundColor Green
        Write-Host "1. Push code to GitHub" -ForegroundColor White
        Write-Host "2. Connect repo at render.com" -ForegroundColor White
        Write-Host "3. Build: npm install" -ForegroundColor White
        Write-Host "4. Start: npm start" -ForegroundColor White
        Write-Host ""
        Write-Host "=== ngrok ===" -ForegroundColor Green
        Write-Host "node server.js (in one terminal)" -ForegroundColor White
        Write-Host "ngrok http 3000 (in another terminal)" -ForegroundColor White
        Write-Host ""
        Read-Host "Press Enter to continue"
    }
    
    default {
        Write-Host "Invalid choice" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎯 Next steps after deployment:" -ForegroundColor Green
Write-Host "1. Copy your public server URL" -ForegroundColor White
Write-Host "2. Update PublicServerClient.cs SERVER_OPTIONS" -ForegroundColor White
Write-Host "3. Test P2P with real internet connections!" -ForegroundColor White
Write-Host ""
Read-Host "Press Enter to exit"
