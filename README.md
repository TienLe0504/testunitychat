# Unity Game Server Test

## Cách chạy test:

### 1. Chạy Server (Terminal 1)
```bash
cd d:\workspace\unity\serverchat
npm install
npm start
```

### 2. Chạy C# Client Test (Terminal 2)

**Nếu đã cài .NET SDK:**
```bash
cd d:\workspace\unity\serverchat
dotnet restore
dotnet run --project UnityTestClient.csproj
```

**Nếu chưa cài .NET SDK:**
1. **Tải .NET SDK**: https://dotnet.microsoft.com/download
2. **Hoặc dùng file .bat**:
```bash
cd d:\workspace\unity\serverchat
run-client.bat
```
3. **Hoặc dùng Visual Studio**: Mở `UnityTestClient.csproj` và nhấn F5

**Hoặc dùng Unity thật**: Import `UnityMatchmakingClient.cs` vào Unity project

### 3. Chạy C# Client thứ 2 (Terminal 3)
```bash
cd d:\workspace\unity\serverchat
dotnet run --project UnityTestClient.csproj
```

## Kết quả mong đợi:

1. **Server console sẽ hiển thị:**
```
🎮 Simple 1v1 Matchmaking Server Started
[CONNECT] Player abc123 connected
[QUEUE] TestPlayer_1234 joined queue
[CONNECT] Player def456 connected
[QUEUE] TestPlayer_5678 joined queue
[MATCH] Created match_xxx: TestPlayer_1234 vs TestPlayer_5678
[INFO] abc123 reported connection: 192.168.1.100:12345
[INFO] def456 reported connection: 192.168.1.101:54321
[READY] Match match_xxx - Both players ready!
[CHAT] TestPlayer_1234 -> def456: Hello from C# client!
```

2. **Client 1 console sẽ hiển thị:**
```
🎮 Unity Game Client Test
Enter your player name: TestPlayer_1234
✅ Connected to server! Socket ID: abc123
🔍 Searching for match...
📊 Queue Status: Searching for opponent... (Position: 1)
🎮 Match Found!
📡 Sent connection info: 192.168.1.100:12345
🚀 P2P Connection Ready!
💬 You can now send messages! Type 'help' for commands.
[TestPlayer_1234] > Hello!
📤 Sent: Hello!
📨 [10:30:15] TestPlayer_5678: Hi there!
```

3. **Client 2 console sẽ hiển thị:**
```
🎮 Unity Game Client Test
Enter your player name: TestPlayer_5678
✅ Connected to server! Socket ID: def456
🔍 Searching for match...
🎮 Match Found!
📡 Sent connection info: 192.168.1.101:54321
🚀 P2P Connection Ready!
💬 You can now send messages! Type 'help' for commands.
📨 [10:30:20] TestPlayer_1234: Hello!
[TestPlayer_5678] > Hi there!
📤 Sent: Hi there!
```

## Các lệnh chat:
- `help` - Hiển thị trợ giúp
- `info` - Hiển thị thông tin match
- `test` - Gửi tin nhắn test tự động
- `quit` - Thoát chat
- Hoặc gõ bất kỳ để gửi tin nhắn

## Test với Web Client:
Bạn cũng có thể mở `http://localhost:3000` để test C# client với web client.

## Các tính năng đã test:
✅ Kết nối server
✅ Matchmaking tự động  
✅ **REAL NAT Discovery** (STUN protocol)
✅ **REAL UDP P2P Chat** (không qua server)
✅ Backup chat qua server (fallback)
✅ Ping/Pong test P2P connection
✅ Xử lý disconnect
✅ Multiple clients

## ⚠️ QUAN TRỌNG - NAT Traversal:
- **STUN Discovery**: Tự động tìm IP public thật qua Google STUN server
- **UDP P2P**: Chat trực tiếp giữa clients, không qua server
- **Firewall**: Cần mở port UDP để P2P hoạt động
- **Router**: Một số router có thể chặn UDP P2P

## Test P2P thật:
1. Chạy trên 2 máy khác nhau (khác mạng)
2. Kiểm tra log "REAL P2P UDP connection"
3. Chat sẽ hiển thị "[REAL P2P]" nếu thành công
