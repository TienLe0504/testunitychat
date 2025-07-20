using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnityTestClient
{
    public class UdpP2PClient
    {
        private UdpClient udpClient;
        private IPEndPoint localEndpoint;
        private IPEndPoint remoteEndpoint;
        private bool isListening = false;
        private string playerName;

        public event Action<string, string> MessageReceived; // sender, message

        public UdpP2PClient(string playerName, int localPort)
        {
            this.playerName = playerName;
            this.localEndpoint = new IPEndPoint(IPAddress.Any, localPort);
        }

        public async Task StartAsync()
        {
            try
            {
                udpClient = new UdpClient(localEndpoint);
                isListening = true;
                
                Console.WriteLine($"🔌 UDP P2P Client started on {localEndpoint}");
                
                // Start listening for incoming messages
                _ = Task.Run(ListenForMessages);
                
                await Task.Delay(100); // Give some time for setup
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to start UDP client: {ex.Message}");
            }
        }

        public void SetRemoteEndpoint(string ip, int port)
        {
            remoteEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Console.WriteLine($"🎯 Remote endpoint set to: {remoteEndpoint}");
        }

        public async Task SendMessageAsync(string message)
        {
            if (remoteEndpoint == null)
            {
                Console.WriteLine("❌ Remote endpoint not set!");
                return;
            }

            try
            {
                var p2pMessage = new
                {
                    type = "chat",
                    sender = playerName,
                    message = message,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                var jsonMessage = JsonConvert.SerializeObject(p2pMessage);
                var data = Encoding.UTF8.GetBytes(jsonMessage);

                await udpClient.SendAsync(data, data.Length, remoteEndpoint);
                Console.WriteLine($"📤 [UDP P2P] Sent to {remoteEndpoint}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send UDP message: {ex.Message}");
            }
        }

        public async Task SendPingAsync()
        {
            if (remoteEndpoint == null) return;

            try
            {
                var pingMessage = new
                {
                    type = "ping",
                    sender = playerName,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                var jsonMessage = JsonConvert.SerializeObject(pingMessage);
                var data = Encoding.UTF8.GetBytes(jsonMessage);

                await udpClient.SendAsync(data, data.Length, remoteEndpoint);
                Console.WriteLine($"🏓 [UDP P2P] Ping sent to {remoteEndpoint}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send ping: {ex.Message}");
            }
        }

        private async Task ListenForMessages()
        {
            while (isListening)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    var data = result.Buffer;
                    var senderEndpoint = result.RemoteEndPoint;

                    var jsonMessage = Encoding.UTF8.GetString(data);
                    var message = JsonConvert.DeserializeObject<dynamic>(jsonMessage);

                    string messageType = message.type;
                    string sender = message.sender;

                    switch (messageType)
                    {
                        case "chat":
                            string chatMessage = message.message;
                            Console.WriteLine($"📨 [UDP P2P] From {senderEndpoint}: {sender}: {chatMessage}");
                            MessageReceived?.Invoke(sender, chatMessage);
                            break;

                        case "ping":
                            Console.WriteLine($"🏓 [UDP P2P] Ping from {senderEndpoint}");
                            await SendPongAsync(senderEndpoint);
                            break;

                        case "pong":
                            Console.WriteLine($"🏓 [UDP P2P] Pong from {senderEndpoint}");
                            break;

                        default:
                            Console.WriteLine($"❓ [UDP P2P] Unknown message type: {messageType}");
                            break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // UDP client disposed, exit gracefully
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error receiving UDP message: {ex.Message}");
                }
            }
        }

        private async Task SendPongAsync(IPEndPoint target)
        {
            try
            {
                var pongMessage = new
                {
                    type = "pong",
                    sender = playerName,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                var jsonMessage = JsonConvert.SerializeObject(pongMessage);
                var data = Encoding.UTF8.GetBytes(jsonMessage);

                await udpClient.SendAsync(data, data.Length, target);
                Console.WriteLine($"🏓 [UDP P2P] Pong sent to {target}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send pong: {ex.Message}");
            }
        }

        public async Task TestConnectionAsync()
        {
            if (remoteEndpoint == null)
            {
                Console.WriteLine("❌ Cannot test connection: Remote endpoint not set!");
                return;
            }

            Console.WriteLine("🧪 Testing P2P connection...");
            
            // Send multiple pings to test connectivity
            for (int i = 0; i < 3; i++)
            {
                await SendPingAsync();
                await Task.Delay(1000);
            }

            // Send test message
            await SendMessageAsync($"[TEST] P2P connection test from {playerName}");
        }

        public void Stop()
        {
            isListening = false;
            udpClient?.Close();
            udpClient?.Dispose();
            Console.WriteLine("🛑 UDP P2P Client stopped");
        }
    }
}
