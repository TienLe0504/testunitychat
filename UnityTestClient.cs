using System;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;
using System.Threading;

namespace UnityTestClient
{
    // Data classes for JSON serialization
    public class PlayerData
    {
        public string playerId { get; set; }
        public string playerName { get; set; }
    }

    public class ConnectionInfo
    {
        public string ip { get; set; }
        public int port { get; set; }
    }

    public class QueueStatus
    {
        public string message { get; set; }
        public int position { get; set; }
    }

    public class MatchFoundData
    {
        public string matchId { get; set; }
        public OpponentInfo opponent { get; set; }
        public string yourRole { get; set; }
        public string message { get; set; }
    }

    public class OpponentInfo
    {
        public string playerId { get; set; }
        public string playerName { get; set; }
    }

    public class StartP2PData
    {
        public string matchId { get; set; }
        public P2PPlayerInfo opponent { get; set; }
        public P2PPlayerInfo yourInfo { get; set; }
        public string message { get; set; }
    }

    public class P2PPlayerInfo
    {
        public string playerId { get; set; }
        public string playerName { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
    }

    public class P2PMessage
    {
        public string targetPlayerId { get; set; }
        public string message { get; set; }
        public string senderName { get; set; }
    }

    public class ReceivedP2PMessage
    {
        public string senderName { get; set; }
        public string message { get; set; }
        public long timestamp { get; set; }
    }

    public class GameClient
    {
        private SocketIO socket;
        private string serverUrl;
        private string playerId;
        private string playerName;
        private bool isConnected = false;
        private bool isInMatch = false;
        private P2PPlayerInfo opponentInfo;
        private P2PPlayerInfo myInfo;
        private string currentMatchId;
        private UdpP2PClient udpP2PClient;

        public GameClient(string url, string name)
        {
            serverUrl = url;
            playerName = name;
            playerId = Guid.NewGuid().ToString();
            
            // Khởi tạo UDP P2P client
            var random = new Random();
            var localPort = random.Next(15000, 25000);
            udpP2PClient = new UdpP2PClient(name, localPort);
            
            // Đăng ký event để nhận tin nhắn P2P
            udpP2PClient.MessageReceived += OnP2PMessageReceived;
        }

        public async Task ConnectAsync()
        {
            try
            {
                socket = new SocketIO(serverUrl);

                // Event handlers
                socket.OnConnected += OnConnected;
                socket.OnDisconnected += OnDisconnected;
                
                // Matchmaking events
                socket.On("queueStatus", OnQueueStatus);
                socket.On("matchFound", OnMatchFound);
                socket.On("requestConnectionInfo", OnRequestConnectionInfo);
                socket.On("startP2P", OnStartP2P);
                socket.On("opponentDisconnected", OnOpponentDisconnected);
                
                // Chat events
                socket.On("p2pMessage", OnP2PMessage);

                await socket.ConnectAsync();
                
                Console.WriteLine($"🔌 Connecting to server: {serverUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
            }
        }

        private async void OnConnected(object sender, EventArgs e)
        {
            isConnected = true;
            Console.WriteLine($"✅ Connected to server! Socket ID: {socket.Id}");
            Console.WriteLine($"👤 Player: {playerName} (ID: {playerId})");
            
            // Auto join match after connection
            await Task.Delay(1000);
            await FindMatchAsync();
        }

        private void OnDisconnected(object sender, string e)
        {
            isConnected = false;
            isInMatch = false;
            Console.WriteLine($"❌ Disconnected from server: {e}");
        }

        private void OnQueueStatus(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<QueueStatus>();
                Console.WriteLine($"📊 Queue Status: {data.message} (Position: {data.position})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing queue status: {ex.Message}");
            }
        }

        private async void OnMatchFound(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<MatchFoundData>();
                currentMatchId = data.matchId;
                
                Console.WriteLine($"🎮 Match Found!");
                Console.WriteLine($"   Match ID: {data.matchId}");
                Console.WriteLine($"   Your Role: {data.yourRole}");
                Console.WriteLine($"   Opponent: {data.opponent.playerName} ({data.opponent.playerId})");
                
                // Simulate NAT discovery (getting public IP)
                await Task.Delay(2000);
                await SendConnectionInfoAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing match found: {ex.Message}");
            }
        }

        private async void OnRequestConnectionInfo(SocketIOResponse response)
        {
            Console.WriteLine("📡 Server requesting connection info...");
            await SendConnectionInfoAsync();
        }

        private void OnStartP2P(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<StartP2PData>();
                isInMatch = true;
                opponentInfo = data.opponent;
                myInfo = data.yourInfo;
                
                Console.WriteLine($"🚀 P2P Connection Ready!");
                Console.WriteLine($"   Your Info: {myInfo.ip}:{myInfo.port}");
                Console.WriteLine($"   Opponent: {opponentInfo.playerName}");
                Console.WriteLine($"   Opponent IP: {opponentInfo.ip}:{opponentInfo.port}");
                
                // Khởi động UDP P2P client
                _ = Task.Run(async () =>
                {
                    await udpP2PClient.StartAsync();
                    
                    // Set địa chỉ đối thủ
                    udpP2PClient.SetRemoteEndpoint(opponentInfo.ip, opponentInfo.port);
                    
                    // Test kết nối P2P
                    await Task.Delay(2000);
                    await udpP2PClient.TestConnectionAsync();
                });
                
                Console.WriteLine($"💬 You can now send messages! Type 'help' for commands.");
                Console.WriteLine($"🔥 REAL P2P UDP connection established!");
                
                // Start chat interface
                StartChatInterface();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing start P2P: {ex.Message}");
            }
        }

        private void OnP2PMessageReceived(string sender, string message)
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"📨 [REAL P2P] [{time}] {sender}: {message}");
        }

        private void OnOpponentDisconnected(SocketIOResponse response)
        {
            isInMatch = false;
            Console.WriteLine("❌ Opponent disconnected!");
        }

        private void OnP2PMessage(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<ReceivedP2PMessage>();
                var time = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).ToString("HH:mm:ss");
                Console.WriteLine($"📨 [{time}] {data.senderName}: {data.message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing P2P message: {ex.Message}");
            }
        }

        public async Task FindMatchAsync()
        {
            if (!isConnected)
            {
                Console.WriteLine("❌ Not connected to server!");
                return;
            }

            var playerData = new PlayerData
            {
                playerId = playerId,
                playerName = playerName
            };

            await socket.EmitAsync("findMatch", playerData);
            Console.WriteLine("🔍 Searching for match...");
        }

        private async Task SendConnectionInfoAsync()
        {
            try
            {
                // THỰC HIỆN NAT DISCOVERY THẬT - Sử dụng STUN server
                Console.WriteLine("🔍 Performing real NAT discovery...");
                
                var publicEndpoint = await GetPublicEndpointAsync();
                
                if (publicEndpoint != null)
                {
                    var connectionInfo = new ConnectionInfo
                    {
                        ip = publicEndpoint.Item1,
                        port = publicEndpoint.Item2
                    };

                    await socket.EmitAsync("connectionInfo", connectionInfo);
                    Console.WriteLine($"📡 Sent REAL connection info: {publicEndpoint.Item1}:{publicEndpoint.Item2}");
                }
                else
                {
                    // Fallback to fake IP for testing
                    var random = new Random();
                    var fakeIP = $"192.168.1.{random.Next(100, 254)}";
                    var fakePort = random.Next(10000, 50000);

                    var connectionInfo = new ConnectionInfo
                    {
                        ip = fakeIP,
                        port = fakePort
                    };

                    await socket.EmitAsync("connectionInfo", connectionInfo);
                    Console.WriteLine($"📡 Sent FAKE connection info (fallback): {fakeIP}:{fakePort}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ NAT Discovery failed: {ex.Message}");
                
                // Fallback
                var random = new Random();
                var fakeIP = $"192.168.1.{random.Next(100, 254)}";
                var fakePort = random.Next(10000, 50000);

                var connectionInfo = new ConnectionInfo
                {
                    ip = fakeIP,
                    port = fakePort
                };

                await socket.EmitAsync("connectionInfo", connectionInfo);
                Console.WriteLine($"📡 Sent FAKE connection info (error fallback): {fakeIP}:{fakePort}");
            }
        }

        // THÊM PHƯƠNG THỨC NAT DISCOVERY THẬT
        private async Task<(string, int)?> GetPublicEndpointAsync()
        {
            try
            {
                using (var udpClient = new System.Net.Sockets.UdpClient())
                {
                    // Kết nối đến STUN server của Google
                    var stunServer = "stun.l.google.com";
                    var stunPort = 19302;
                    
                    await udpClient.ConnectAsync(stunServer, stunPort);
                    
                    // Tạo STUN request packet (simplified)
                    var stunRequest = CreateStunBindingRequest();
                    await udpClient.SendAsync(stunRequest, stunRequest.Length);
                    
                    // Nhận response
                    var result = await udpClient.ReceiveAsync();
                    var response = result.Buffer;
                    
                    // Parse STUN response để lấy public IP:port
                    var publicEndpoint = ParseStunResponse(response);
                    
                    if (publicEndpoint != null)
                    {
                        Console.WriteLine($"✅ STUN Discovery successful: {publicEndpoint.Value.Item1}:{publicEndpoint.Value.Item2}");
                        return publicEndpoint;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ STUN request failed: {ex.Message}");
            }
            
            return null;
        }

        private byte[] CreateStunBindingRequest()
        {
            // STUN Binding Request packet structure
            var packet = new byte[20];
            
            // Message Type: Binding Request (0x0001)
            packet[0] = 0x00;
            packet[1] = 0x01;
            
            // Message Length: 0 (no attributes)
            packet[2] = 0x00;
            packet[3] = 0x00;
            
            // Magic Cookie: 0x2112A442
            packet[4] = 0x21;
            packet[5] = 0x12;
            packet[6] = 0xA4;
            packet[7] = 0x42;
            
            // Transaction ID: 12 random bytes
            var random = new Random();
            for (int i = 8; i < 20; i++)
            {
                packet[i] = (byte)random.Next(256);
            }
            
            return packet;
        }

        private (string, int)? ParseStunResponse(byte[] response)
        {
            try
            {
                if (response.Length < 20) return null;
                
                // Check if it's a STUN Binding Success Response (0x0101)
                if (response[0] != 0x01 || response[1] != 0x01) return null;
                
                // Parse attributes to find MAPPED-ADDRESS or XOR-MAPPED-ADDRESS
                int offset = 20; // Skip STUN header
                
                while (offset < response.Length)
                {
                    if (offset + 4 > response.Length) break;
                    
                    // Attribute Type
                    int attrType = (response[offset] << 8) | response[offset + 1];
                    // Attribute Length
                    int attrLength = (response[offset + 2] << 8) | response[offset + 3];
                    
                    if (attrType == 0x0001 || attrType == 0x0020) // MAPPED-ADDRESS or XOR-MAPPED-ADDRESS
                    {
                        if (offset + 4 + attrLength <= response.Length && attrLength >= 8)
                        {
                            // Skip padding and address family
                            int portOffset = offset + 6;
                            int ipOffset = offset + 8;
                            
                            int port = (response[portOffset] << 8) | response[portOffset + 1];
                            
                            // For XOR-MAPPED-ADDRESS, need to XOR with magic cookie
                            if (attrType == 0x0020)
                            {
                                port ^= 0x2112; // XOR with first 2 bytes of magic cookie
                            }
                            
                            // Extract IP address (IPv4)
                            var ipBytes = new byte[4];
                            Array.Copy(response, ipOffset, ipBytes, 0, 4);
                            
                            // For XOR-MAPPED-ADDRESS, XOR IP with magic cookie
                            if (attrType == 0x0020)
                            {
                                ipBytes[0] ^= 0x21;
                                ipBytes[1] ^= 0x12;
                                ipBytes[2] ^= 0xA4;
                                ipBytes[3] ^= 0x42;
                            }
                            
                            var ip = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}.{ipBytes[3]}";
                            return (ip, port);
                        }
                    }
                    
                    // Move to next attribute
                    offset += 4 + attrLength;
                    // Pad to 4-byte boundary
                    while (offset % 4 != 0) offset++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing STUN response: {ex.Message}");
            }
            
            return null;
        }

        public async Task SendMessageAsync(string message)
        {
            if (!isInMatch || opponentInfo == null)
            {
                Console.WriteLine("❌ Not in a match or no opponent!");
                return;
            }

            // GỬI QUA UDP P2P TRỰC TIẾP (THẬT)
            await udpP2PClient.SendMessageAsync(message);
            
            // Đồng thời gửi qua server để backup (optional)
            var p2pMessage = new P2PMessage
            {
                targetPlayerId = opponentInfo.playerId,
                message = message,
                senderName = playerName
            };

            await socket.EmitAsync("p2pMessage", p2pMessage);
            Console.WriteLine($"📤 [BACKUP via Server] Sent: {message}");
        }

        public async Task LeaveQueueAsync()
        {
            await socket.EmitAsync("leaveQueue");
            Console.WriteLine("❌ Left queue");
        }

        private void StartChatInterface()
        {
            Task.Run(async () =>
            {
                while (isInMatch)
                {
                    Console.Write($"[{playerName}] > ");
                    var input = Console.ReadLine();
                    
                    if (string.IsNullOrEmpty(input)) continue;

                    if (input.ToLower() == "quit" || input.ToLower() == "exit")
                    {
                        break;
                    }
                    else if (input.ToLower() == "help")
                    {
                        ShowHelp();
                    }
                    else if (input.ToLower() == "info")
                    {
                        ShowMatchInfo();
                    }
                    else if (input.ToLower().StartsWith("test"))
                    {
                        await SendTestMessages();
                    }
                    else
                    {
                        await SendMessageAsync(input);
                    }
                }
            });
        }

        private void ShowHelp()
        {
            Console.WriteLine("🆘 Available commands:");
            Console.WriteLine("   help     - Show this help");
            Console.WriteLine("   info     - Show match information");
            Console.WriteLine("   test     - Send test messages");
            Console.WriteLine("   quit     - Exit chat");
            Console.WriteLine("   Or just type any message to send to opponent");
        }

        private void ShowMatchInfo()
        {
            Console.WriteLine("ℹ️ Match Information:");
            Console.WriteLine($"   Match ID: {currentMatchId}");
            Console.WriteLine($"   Your Name: {myInfo?.playerName ?? playerName}");
            Console.WriteLine($"   Your IP: {myInfo?.ip}:{myInfo?.port}");
            Console.WriteLine($"   Opponent: {opponentInfo?.playerName}");
            Console.WriteLine($"   Opponent IP: {opponentInfo?.ip}:{opponentInfo?.port}");
        }

        private async Task SendTestMessages()
        {
            var testMessages = new[]
            {
                "Hello from C# client!",
                "This is a test message",
                "P2P communication working!",
                "Ready to play?",
                "Good game!"
            };

            Console.WriteLine("🧪 Sending test messages...");
            
            foreach (var msg in testMessages)
            {
                await SendMessageAsync($"[TEST] {msg}");
                await Task.Delay(1000);
            }
        }

        public async Task DisconnectAsync()
        {
            if (socket != null)
            {
                await socket.DisconnectAsync();
                socket.Dispose();
            }
            
            udpP2PClient?.Stop();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🎮 Unity Game Client Test");
            Console.WriteLine("========================");
            
            // Get player name
            Console.Write("Enter your player name: ");
            var playerName = Console.ReadLine();
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = $"TestPlayer_{new Random().Next(1000, 9999)}";
            }

            // Create client
            var client = new GameClient("http://localhost:3000", playerName);
            
            try
            {
                await client.ConnectAsync();
                
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync();
            }
        }
    }
}
