# Deploy Script for Game Server

## Option 1: Deploy to Heroku (Free)

### Prerequisites:
- Install Heroku CLI: https://devcenter.heroku.com/articles/heroku-cli
- Create Heroku account

### Steps:
```bash
# 1. Login to Heroku
heroku login

# 2. Create Heroku app
heroku create your-game-server-name

# 3. Set buildpack to Node.js
heroku buildpacks:set heroku/nodejs

# 4. Deploy
git add .
git commit -m "Deploy game server"
git push heroku main

# 5. Get public URL
heroku open
```

## Option 2: Deploy to Railway (Recommended)

### Steps:
1. Go to https://railway.app
2. Sign up with GitHub
3. Create new project from GitHub repo
4. Railway will auto-deploy
5. Get public URL from Railway dashboard

## Option 3: Deploy to Render (Free)

### Steps:
1. Go to https://render.com
2. Sign up with GitHub
3. Create new Web Service
4. Connect GitHub repo
5. Set build command: `npm install`
6. Set start command: `npm start`
7. Deploy and get public URL

## Option 4: Local with ngrok (For testing)

### Steps:
```bash
# 1. Install ngrok: https://ngrok.com/download
# 2. Run server locally
npm start

# 3. In another terminal, expose to public
ngrok http 3000

# 4. Copy the https://xxx.ngrok.io URL
```

## Files needed for deployment:

### package.json (already exists)
### Procfile (for Heroku)
web: npm start

### .env (environment variables)
PORT=3000
NODE_ENV=production
