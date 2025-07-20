# 🚂 Deploy với Railway (Đơn giản hơn Heroku)

## Bước 1: Tạo GitHub Repository
1. Vào https://github.com và tạo repository mới
2. Upload code của bạn lên GitHub

## Bước 2: Deploy với Railway
1. Vào https://railway.app
2. Đăng nhập bằng GitHub
3. Click "New Project" 
4. Chọn "Deploy from GitHub repo"
5. Chọn repository vừa tạo
6. Railway sẽ tự động deploy!

## Bước 3: Lấy URL
- Sau khi deploy xong, copy URL từ Railway dashboard
- URL sẽ có dạng: https://your-app.railway.app

## Hoặc dùng ngrok (Test nhanh):
```bash
# Terminal 1: Start server
node server.js

# Terminal 2: Create tunnel  
ngrok http 3000
```

Copy URL từ ngrok (https://xxxxx.ngrok.io) để test ngay!
