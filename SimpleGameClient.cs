using System;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;
using System.Threading;

namespace SimpleGameClient
{
    class Program
    {
        private static SocketIO socket;
        private static string playerName;
        private static string playerId;
        private static bool isConnected = false;
        private static bool isInMatch = false;
        private static dynamic opponentInfo = null;

        static async Task Main(string[] args)
        {
            Console.WriteLine("🎮 Simple Game Client Test");
            Console.WriteLine("=========================");
            
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
            Console.WriteLine("\nPress 'q' to quit, or type messages to send:");
            await HandleUserInput();
        }

        static async Task ConnectToServer()
        {
            try
            {
                socket = new SocketIO("http://localhost:3000");
                
                // Setup event handlers
                socket.OnConnected += OnConnected;
                socket.OnDisconnected += OnDisconnected;
                
                socket.On("queueStatus", (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        Console.WriteLine($"📊 Raw queueStatus: {jsonStr}");
                        
                        // Try parse as array first, then object
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            if (dataArray.Length > 0)
                            {
                                var data = dataArray[0];
                                Console.WriteLine($"📊 Queue: {data.message} (Position: {data.position})");
                            }
                        }
                        else
                        {
                            var data = JsonConvert.DeserializeObject<dynamic>(jsonStr);
                            Console.WriteLine($"📊 Queue: {data.message} (Position: {data.position})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error parsing queue status: {ex.Message}");
                        Console.WriteLine($"Raw: {response}");
                    }
                });
                
                socket.On("matchFound", async (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        Console.WriteLine($"🎮 Raw matchFound: {jsonStr}");
                        
                        dynamic data;
                        // Handle array response
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            data = dataArray[0]; // Get first element
                        }
                        else
                        {
                            data = JsonConvert.DeserializeObject<dynamic>(jsonStr);
                        }
                        
                        Console.WriteLine($"🎮 Match Found! vs {data.opponent.playerName}");
                        
                        // Send fake connection info
                        await Task.Delay(2000);
                        var connectionInfo = new {
                            ip = $"192.168.1.{new Random().Next(100, 254)}",
                            port = new Random().Next(10000, 50000)
                        };
                        await socket.EmitAsync("connectionInfo", connectionInfo);
                        Console.WriteLine($"📡 Sent connection info: {connectionInfo.ip}:{connectionInfo.port}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error in matchFound: {ex.Message}");
                        Console.WriteLine($"Raw response: {response}");
                    }
                });
                
                socket.On("startP2P", (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        Console.WriteLine($"🚀 Raw startP2P: {jsonStr}");
                        
                        dynamic data;
                        // Handle array response
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            data = dataArray[0]; // Get first element
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error in startP2P: {ex.Message}");
                        Console.WriteLine($"Raw response: {response}");
                    }
                });
                
                socket.On("p2pMessage", (response) => {
                    try 
                    {
                        var jsonStr = response.ToString();
                        
                        dynamic data;
                        // Handle array response
                        if (jsonStr.Trim().StartsWith("["))
                        {
                            var dataArray = JsonConvert.DeserializeObject<dynamic[]>(jsonStr);
                            data = dataArray[0]; // Get first element
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
                        Console.WriteLine($"Raw: {response}");
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
            }
        }

        static async void OnConnected(object sender, EventArgs e)
        {
            try
            {
                isConnected = true;
                Console.WriteLine($"✅ Connected! Socket ID: {socket.Id}");
                Console.WriteLine($"👤 Player: {playerName}");
                
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
            Console.WriteLine("💡 Try typing 'find' to reconnect or 'quit' to exit");
        }

        static async Task FindMatch()
        {
            if (!isConnected) return;
            
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
                    else if (input.ToLower() == "info")
                    {
                        Console.WriteLine($"Connected: {isConnected}, In Match: {isInMatch}");
                        if (opponentInfo != null)
                        {
                            Console.WriteLine($"Opponent: {opponentInfo.playerName} at {opponentInfo.ip}:{opponentInfo.port}");
                        }
                    }
                    else if (input.ToLower() == "help")
                    {
                        Console.WriteLine("📋 Commands:");
                        Console.WriteLine("  find  - Find/reconnect to match");
                        Console.WriteLine("  info  - Show connection info");
                        Console.WriteLine("  help  - Show this help");
                        Console.WriteLine("  quit  - Exit application");
                        Console.WriteLine("  Or type any message to chat when in match");
                    }
                    else if (isInMatch)
                    {
                        await SendMessage(input);
                    }
                    else
                    {
                        Console.WriteLine("❌ Not in match. Commands: 'find', 'info', 'help', 'quit'");
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
