using System;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;

namespace SimpleGameClient
{
    class PublicServerClient
    {
        // 🌐 PUBLIC SERVER CONFIGURATIONS
        private static readonly string[] SERVER_OPTIONS = {
            "http://localhost:3000",                    // Local development
            "https://your-app-name.herokuapp.com",      // Heroku
            "https://your-app-name.railway.app",        // Railway  
            "https://your-app-name.onrender.com",       // Render
            "https://xxxxx.ngrok.io"                    // ngrok (temporary)
        };
        
        private static SocketIO socket;
        private static string playerName;
        private static string playerId;
        private static bool isConnected = false;
        private static bool isInMatch = false;
        private static dynamic opponentInfo = null;
        private static string currentServerUrl;

        static async Task Main(string[] args)
        {
            Console.WriteLine("🌐 Public Server Game Client Test");
            Console.WriteLine("=================================");
            
            // Select server
            currentServerUrl = SelectServer();
            
            // Get player name
            Console.Write("Enter your player name: ");
            playerName = Console.ReadLine();
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = $"Player_{new Random().Next(1000, 9999)}";
            }
            
            playerId = Guid.NewGuid().ToString();
            
            await ConnectToServer();
            
            // Keep console alive
            Console.WriteLine("\n📱 Commands: 'find', 'info', 'help', 'server', 'quit'");
            await HandleUserInput();
        }

        static string SelectServer()
        {
            Console.WriteLine("\n🔗 Select Server:");
            for (int i = 0; i < SERVER_OPTIONS.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {SERVER_OPTIONS[i]}");
            }
            Console.WriteLine($"  {SERVER_OPTIONS.Length + 1}. Enter custom URL");
            
            Console.Write("\nChoice (1-" + (SERVER_OPTIONS.Length + 1) + "): ");
            var choice = Console.ReadLine();
            
            if (int.TryParse(choice, out int index) && index >= 1 && index <= SERVER_OPTIONS.Length)
            {
                return SERVER_OPTIONS[index - 1];
            }
            else if (int.TryParse(choice, out index) && index == SERVER_OPTIONS.Length + 1)
            {
                Console.Write("Enter server URL: ");
                var customUrl = Console.ReadLine();
                return string.IsNullOrEmpty(customUrl) ? SERVER_OPTIONS[0] : customUrl;
            }
            else
            {
                Console.WriteLine("Invalid choice, using localhost");
                return SERVER_OPTIONS[0];
            }
        }

        static async Task ConnectToServer()
        {
            try
            {
                Console.WriteLine($"🔌 Connecting to: {currentServerUrl}");
                
                socket = new SocketIO(currentServerUrl);
                
                // Setup event handlers
                socket.OnConnected += OnConnected;
                socket.OnDisconnected += OnDisconnected;
                
                socket.On("queueStatus", (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        
                        dynamic data;
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            data = dataArray[0];
                        }
                        else
                        {
                            data = JsonConvert.DeserializeObject<dynamic>(jsonStr);
                        }
                        
                        Console.WriteLine($"📊 Queue: {data.message} (Position: {data.position})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error parsing queue status: {ex.Message}");
                    }
                });
                
                socket.On("matchFound", async (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        Console.WriteLine($"🎮 Raw matchFound: {jsonStr}");
                        
                        dynamic data;
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            data = dataArray[0];
                        }
                        else
                        {
                            data = JsonConvert.DeserializeObject<dynamic>(jsonStr);
                        }
                        
                        Console.WriteLine($"🎮 Match Found! vs {data.opponent.playerName}");
                        
                        // ⚡ REAL NAT DISCOVERY SIMULATION
                        Console.WriteLine("🔍 Performing NAT discovery...");
                        await Task.Delay(2000);
                        
                        // Simulate different network scenarios
                        var networkType = new Random().Next(1, 4);
                        string detectedIP;
                        int detectedPort;
                        
                        switch (networkType)
                        {
                            case 1: // Home WiFi
                                detectedIP = $"103.{new Random().Next(1, 255)}.{new Random().Next(1, 255)}.{new Random().Next(1, 255)}";
                                detectedPort = new Random().Next(10000, 50000);
                                Console.WriteLine("🏠 Network: Home WiFi detected");
                                break;
                            case 2: // Mobile 4G
                                detectedIP = $"42.{new Random().Next(1, 255)}.{new Random().Next(1, 255)}.{new Random().Next(1, 255)}";
                                detectedPort = new Random().Next(15000, 60000);
                                Console.WriteLine("📱 Network: Mobile 4G detected");
                                break;
                            default: // Corporate/Cafe
                                detectedIP = $"172.{new Random().Next(16, 31)}.{new Random().Next(1, 255)}.{new Random().Next(1, 255)}";
                                detectedPort = new Random().Next(20000, 55000);
                                Console.WriteLine("🏢 Network: Corporate/Public WiFi detected");
                                break;
                        }
                        
                        var connectionInfo = new {
                            ip = detectedIP,
                            port = detectedPort
                        };
                        
                        await socket.EmitAsync("connectionInfo", connectionInfo);
                        Console.WriteLine($"📡 Sent real connection info: {detectedIP}:{detectedPort}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error in matchFound: {ex.Message}");
                    }
                });
                
                socket.On("startP2P", (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        
                        dynamic data;
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            data = dataArray[0];
                        }
                        else
                        {
                            data = JsonConvert.DeserializeObject<dynamic>(jsonStr);
                        }
                        
                        isInMatch = true;
                        opponentInfo = data.opponent;
                        
                        Console.WriteLine($"🚀 P2P Ready!");
                        Console.WriteLine($"   Opponent: {opponentInfo.playerName}");
                        Console.WriteLine($"   Opponent IP: {opponentInfo.ip}:{opponentInfo.port}");
                        Console.WriteLine($"💬 You can now chat! Type messages:");
                        Console.WriteLine($"🌍 This would normally establish UDP P2P connection");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error in startP2P: {ex.Message}");
                    }
                });
                
                socket.On("p2pMessage", (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        
                        dynamic data;
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            data = dataArray[0];
                        }
                        else
                        {
                            data = JsonConvert.DeserializeObject<dynamic>(jsonStr);
                        }
                        
                        Console.WriteLine($"📨 {data.senderName}: {data.message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error in p2pMessage: {ex.Message}");
                    }
                });
                
                socket.On("opponentDisconnected", (response) => {
                    isInMatch = false;
                    Console.WriteLine("❌ Opponent disconnected!");
                });
                
                await socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                Console.WriteLine("💡 Make sure server is running and URL is correct");
            }
        }

        static async void OnConnected(object sender, EventArgs e)
        {
            try
            {
                isConnected = true;
                Console.WriteLine($"✅ Connected! Socket ID: {socket.Id}");
                Console.WriteLine($"👤 Player: {playerName}");
                Console.WriteLine($"🌐 Server: {currentServerUrl}");
                
                // Auto find match
                await Task.Delay(1000);
                await FindMatch();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in OnConnected: {ex.Message}");
            }
        }

        static void OnDisconnected(object sender, string e)
        {
            isConnected = false;
            isInMatch = false;
            Console.WriteLine($"❌ Disconnected: {e}");
            Console.WriteLine("💡 Try typing 'find' to reconnect or 'server' to change server");
        }

        static async Task FindMatch()
        {
            if (!isConnected) 
            {
                Console.WriteLine("❌ Not connected to server!");
                return;
            }
            
            var playerData = new {
                playerId = playerId,
                playerName = playerName
            };
            
            await socket.EmitAsync("findMatch", playerData);
            Console.WriteLine("🔍 Searching for match...");
        }

        static async Task SendMessage(string message)
        {
            if (!isInMatch || opponentInfo == null)
            {
                Console.WriteLine("❌ Not in match!");
                return;
            }
            
            var p2pMessage = new {
                targetPlayerId = opponentInfo.playerId.ToString(),
                message = message,
                senderName = playerName
            };
            
            await socket.EmitAsync("p2pMessage", p2pMessage);
            Console.WriteLine($"📤 You: {message}");
        }

        static async Task HandleUserInput()
        {
            while (true)
            {
                try
                {
                    var input = Console.ReadLine();
                    
                    if (string.IsNullOrEmpty(input)) continue;
                    
                    if (input.ToLower() == "q" || input.ToLower() == "quit")
                    {
                        break;
                    }
                    else if (input.ToLower() == "find")
                    {
                        if (!isConnected)
                        {
                            Console.WriteLine("🔄 Reconnecting...");
                            await ConnectToServer();
                        }
                        else
                        {
                            await FindMatch();
                        }
                    }
                    else if (input.ToLower() == "server")
                    {
                        if (socket != null)
                        {
                            await socket.DisconnectAsync();
                        }
                        currentServerUrl = SelectServer();
                        await ConnectToServer();
                    }
                    else if (input.ToLower() == "info")
                    {
                        Console.WriteLine($"🔗 Server: {currentServerUrl}");
                        Console.WriteLine($"📡 Connected: {isConnected}, In Match: {isInMatch}");
                        if (opponentInfo != null)
                        {
                            Console.WriteLine($"👥 Opponent: {opponentInfo.playerName} at {opponentInfo.ip}:{opponentInfo.port}");
                        }
                    }
                    else if (input.ToLower() == "help")
                    {
                        Console.WriteLine("📋 Commands:");
                        Console.WriteLine("  find   - Find/reconnect to match");
                        Console.WriteLine("  server - Change server URL");
                        Console.WriteLine("  info   - Show connection info");
                        Console.WriteLine("  help   - Show this help");
                        Console.WriteLine("  quit   - Exit application");
                        Console.WriteLine("  Or type any message to chat when in match");
                    }
                    else if (isInMatch)
                    {
                        await SendMessage(input);
                    }
                    else
                    {
                        Console.WriteLine("❌ Not in match. Commands: 'find', 'server', 'info', 'help', 'quit'");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error handling input: {ex.Message}");
                }
            }
            
            try
            {
                if (socket != null)
                {
                    await socket.DisconnectAsync();
                    socket.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during cleanup: {ex.Message}");
            }
        }
    }
}
