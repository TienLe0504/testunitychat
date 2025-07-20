const express = require('express');
const http = require('http');
const socketIo = require('socket.io');

const app = express();
const server = http.createServer(app);
const io = socketIo(server, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

// Serve static files cho test client
app.use(express.static('public'));

// API endpoints
app.get('/api/stats', (req, res) => {
    res.json({
        queue: waitingPlayers.length,
        activeMatches: activeMatches.size,
        timestamp: new Date().toISOString()
    });
});

app.get('/api/matches', (req, res) => {
    const matches = Array.from(activeMatches.entries()).map(([id, match]) => ({
        id,
        player1: match.player1.playerName,
        player2: match.player2.playerName,
        created: match.createdAt,
        ready: !!(match.player1.ip && match.player2.ip)
    }));
    res.json(matches);
});

// Lưu trữ đơn giản
const waitingPlayers = []; // Danh sách người chơi đang chờ
const activeMatches = new Map(); // matchId -> { player1, player2 }

// STUN servers cho NAT traversal thật
const STUN_SERVERS = [
    'stun:stun.l.google.com:19302',
    'stun:stun1.l.google.com:19302',
    'stun:stun2.l.google.com:19302',
    'stun:stun3.l.google.com:19302',
    'stun:stun4.l.google.com:19302'
];

console.log('🎮 Simple 1v1 Matchmaking Server Started');
console.log('Features: Match players + Share connection info');

io.on('connection', (socket) => {
    console.log(`[CONNECT] Player ${socket.id} connected`);
    
    // Tham gia tìm trận 1v1
    socket.on('findMatch', (playerData) => {
        const player = {
            socketId: socket.id,
            playerId: playerData.playerId || socket.id,
            playerName: playerData.playerName || `Player_${socket.id.slice(0, 5)}`,
            joinTime: Date.now()
        };
        
        console.log(`[QUEUE] ${player.playerName} joined queue`);
        
        // Thêm vào hàng đợi
        waitingPlayers.push(player);
        
        // Thông báo đã vào queue
        socket.emit('queueStatus', {
            message: 'Searching for opponent...',
            position: waitingPlayers.length
        });
        
        // Thử ghép cặp ngay
        tryMatchPlayers();
    });
    
    // Nhận thông tin kết nối (IP public) từ client
    socket.on('connectionInfo', (data) => {
        const { ip, port } = data;
        
        console.log(`[INFO] ${socket.id} reported connection: ${ip}:${port}`);
        
        // Tìm match của player này
        for (let [matchId, match] of activeMatches.entries()) {
            if (match.player1.socketId === socket.id) {
                match.player1.ip = ip;
                match.player1.port = port;
                checkIfBothPlayersReady(matchId);
                break;
            } else if (match.player2.socketId === socket.id) {
                match.player2.ip = ip;
                match.player2.port = port;
                checkIfBothPlayersReady(matchId);
                break;
            }
        }
    });
    
    // Rời khỏi queue
    socket.on('leaveQueue', () => {
        removeFromQueue(socket.id);
        console.log(`[LEAVE] ${socket.id} left queue`);
    });
    
    // Chat simulation (represents P2P messages)
    socket.on('p2pMessage', (data) => {
        const { targetPlayerId, message, senderName } = data;
        console.log(`[CHAT] ${senderName} -> ${targetPlayerId}: ${message}`);
        
        // Find target socket and forward message
        // In real P2P, this would go directly between clients
        for (let [matchId, match] of activeMatches.entries()) {
            if (match.player1.socketId === socket.id && match.player2.playerId === targetPlayerId) {
                io.to(match.player2.socketId).emit('p2pMessage', {
                    senderName: senderName,
                    message: message,
                    timestamp: Date.now()
                });
                break;
            } else if (match.player2.socketId === socket.id && match.player1.playerId === targetPlayerId) {
                io.to(match.player1.socketId).emit('p2pMessage', {
                    senderName: senderName,
                    message: message,
                    timestamp: Date.now()
                });
                break;
            }
        }
    });
    
    // Ngắt kết nối
    socket.on('disconnect', () => {
        console.log(`[DISCONNECT] Player ${socket.id} disconnected`);
        
        // Xóa khỏi queue
        removeFromQueue(socket.id);
        
        // Xóa khỏi active matches
        removeFromMatches(socket.id);
    });
});

// Hàm ghép cặp
function tryMatchPlayers() {
    // Cần ít nhất 2 người để ghép cặp
    if (waitingPlayers.length >= 2) {
        // Lấy 2 người đầu tiên
        const player1 = waitingPlayers.shift();
        const player2 = waitingPlayers.shift();
        
        // Tạo match ID
        const matchId = `match_${Date.now()}_${Math.random().toString(36).slice(2, 8)}`;
        
        // Lưu thông tin match
        const match = {
            player1: { ...player1, ip: null, port: null },
            player2: { ...player2, ip: null, port: null },
            createdAt: Date.now()
        };
        
        activeMatches.set(matchId, match);
        
        console.log(`[MATCH] Created ${matchId}: ${player1.playerName} vs ${player2.playerName}`);
        
        // Thông báo cho cả 2 player
        io.to(player1.socketId).emit('matchFound', {
            matchId: matchId,
            opponent: {
                playerId: player2.playerId,
                playerName: player2.playerName
            },
            yourRole: 'player1',
            message: 'Match found! Getting connection info...'
        });
        
        io.to(player2.socketId).emit('matchFound', {
            matchId: matchId,
            opponent: {
                playerId: player1.playerId,
                playerName: player1.playerName
            },
            yourRole: 'player2',
            message: 'Match found! Getting connection info...'
        });
        
        // Yêu cầu cả 2 gửi thông tin kết nối
        io.to(player1.socketId).emit('requestConnectionInfo');
        io.to(player2.socketId).emit('requestConnectionInfo');
    }
}

// Kiểm tra cả 2 player đã sẵn sàng chưa
function checkIfBothPlayersReady(matchId) {
    const match = activeMatches.get(matchId);
    if (!match) return;
    
    const player1Ready = match.player1.ip && match.player1.port;
    const player2Ready = match.player2.ip && match.player2.port;
    
    if (player1Ready && player2Ready) {
        console.log(`[READY] Match ${matchId} - Both players ready!`);
        console.log(`Player1: ${match.player1.ip}:${match.player1.port}`);
        console.log(`Player2: ${match.player2.ip}:${match.player2.port}`);
        
        // Gửi thông tin đối thủ cho từng người
        io.to(match.player1.socketId).emit('startP2P', {
            matchId: matchId,
            opponent: {
                playerId: match.player2.playerId,
                playerName: match.player2.playerName,
                ip: match.player2.ip,
                port: match.player2.port
            },
            yourInfo: {
                playerId: match.player1.playerId,
                playerName: match.player1.playerName,
                ip: match.player1.ip,
                port: match.player1.port
            },
            message: 'Start P2P connection!'
        });
        
        io.to(match.player2.socketId).emit('startP2P', {
            matchId: matchId,
            opponent: {
                playerId: match.player1.playerId,
                playerName: match.player1.playerName,
                ip: match.player1.ip,
                port: match.player1.port
            },
            yourInfo: {
                playerId: match.player2.playerId,
                playerName: match.player2.playerName,
                ip: match.player2.ip,
                port: match.player2.port
            },
            message: 'Start P2P connection!'
        });
    } else {
        // Thông báo đang chờ
        const waitingFor = !player1Ready ? 'Player 1' : 'Player 2';
        console.log(`[WAITING] Match ${matchId} - Waiting for ${waitingFor} connection info`);
    }
}

// Xóa khỏi queue
function removeFromQueue(socketId) {
    const index = waitingPlayers.findIndex(p => p.socketId === socketId);
    if (index !== -1) {
        waitingPlayers.splice(index, 1);
    }
}

// Xóa khỏi matches
function removeFromMatches(socketId) {
    for (let [matchId, match] of activeMatches.entries()) {
        if (match.player1.socketId === socketId || match.player2.socketId === socketId) {
            // Thông báo đối thủ
            const remainingPlayer = match.player1.socketId === socketId ? match.player2 : match.player1;
            io.to(remainingPlayer.socketId).emit('opponentDisconnected', {
                message: 'Your opponent disconnected'
            });
            
            // Xóa match
            activeMatches.delete(matchId);
            console.log(`[CLEANUP] Removed match ${matchId} due to disconnect`);
            break;
        }
    }
}

// Hiển thị stats định kỳ
setInterval(() => {
    console.log(`[STATS] Queue: ${waitingPlayers.length} | Active Matches: ${activeMatches.size}`);
}, 10000); // Mỗi 10 giây

const PORT = process.env.PORT || 3000;
const NODE_ENV = process.env.NODE_ENV || 'development';

server.listen(PORT, () => {
    console.log(`
╔══════════════════════════════════════════════╗
║          🎮 SIMPLE 1v1 SERVER 🎮              ║
╠══════════════════════════════════════════════╣
║  Port: ${PORT}                               ║
║  Environment: ${NODE_ENV}                    ║
║  Mode: 1v1 Only                              ║
║  Features: Auto-match + Share IP info        ║
╚══════════════════════════════════════════════╝
    `);
    
    if (NODE_ENV === 'production') {
        console.log('🌐 Server is running in PRODUCTION mode');
        console.log('🔗 Server should be accessible from internet');
    } else {
        console.log('🏠 Server is running in DEVELOPMENT mode');
        console.log('🔗 Server accessible at: http://localhost:' + PORT);
    }
});