using System;
using System.Threading.Tasks;
using UnityEngine;
using SocketIOClient;
using Newtonsoft.Json;

namespace UnityGameClient
{
    // Đây là version Unity MonoBehaviour
    public class UnityMatchmakingClient : MonoBehaviour
    {
        [Header("Server Settings")]
        public string serverURL = "http://localhost:3000";
        public string playerName = "UnityPlayer";
        
        [Header("Debug")]
        public bool enableDebugLogs = true;
        
        private SocketIOUnity socket;
        private string playerId;
        private bool isConnected = false;
        private bool isInMatch = false;
        private P2PPlayerInfo opponentInfo;
        private P2PPlayerInfo myInfo;
        
        // Events
        public System.Action<string> OnMatchFound;
        public System.Action<P2PPlayerInfo> OnP2PReady;
        public System.Action<string, string> OnMessageReceived;

        [System.Serializable]
        public class PlayerData
        {
            public string playerId;
            public string playerName;
        }

        [System.Serializable]
        public class P2PPlayerInfo
        {
            public string playerId;
            public string playerName;
            public string ip;
            public int port;
        }

        async void Start()
        {
            playerId = System.Guid.NewGuid().ToString();
            playerName = string.IsNullOrEmpty(playerName) ? $"UnityPlayer_{UnityEngine.Random.Range(1000, 9999)}" : playerName;
            
            await ConnectToServer();
        }

        public async Task ConnectToServer()
        {
            try
            {
                var uri = new System.Uri(serverURL);
                socket = new SocketIOUnity(uri);

                // Event handlers
                socket.OnConnected += OnConnected;
                socket.OnDisconnected += OnDisconnected;
                
                // Matchmaking events
                socket.On("queueStatus", OnQueueStatus);
                socket.On("matchFound", OnMatchFoundEvent);
                socket.On("requestConnectionInfo", OnRequestConnectionInfo);
                socket.On("startP2P", OnStartP2P);
                socket.On("opponentDisconnected", OnOpponentDisconnected);
                
                // Chat events
                socket.On("p2pMessage", OnP2PMessage);

                await socket.ConnectAsync();
                
                DebugLog($"🔌 Connecting to server: {serverURL}");
            }
            catch (System.Exception ex)
            {
                DebugLog($"❌ Connection failed: {ex.Message}");
            }
        }

        private async void OnConnected(object sender, System.EventArgs e)
        {
            isConnected = true;
            DebugLog($"✅ Connected to server! Socket ID: {socket.Id}");
            DebugLog($"👤 Player: {playerName} (ID: {playerId})");
            
            // Auto join match after 1 second
            await Task.Delay(1000);
            await FindMatch();
        }

        private void OnDisconnected(object sender, string e)
        {
            isConnected = false;
            isInMatch = false;
            DebugLog($"❌ Disconnected from server: {e}");
        }

        private void OnQueueStatus(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<dynamic>();
                DebugLog($"📊 Queue Status: {data.message} (Position: {data.position})");
            }
            catch (System.Exception ex)
            {
                DebugLog($"Error parsing queue status: {ex.Message}");
            }
        }

        private async void OnMatchFoundEvent(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<dynamic>();
                DebugLog($"🎮 Match Found!");
                DebugLog($"   Match ID: {data.matchId}");
                DebugLog($"   Your Role: {data.yourRole}");
                DebugLog($"   Opponent: {data.opponent.playerName}");
                
                OnMatchFound?.Invoke(data.opponent.playerName.ToString());
                
                // Simulate NAT discovery
                await Task.Delay(2000);
                await SendConnectionInfo();
            }
            catch (System.Exception ex)
            {
                DebugLog($"Error parsing match found: {ex.Message}");
            }
        }

        private async void OnRequestConnectionInfo(SocketIOResponse response)
        {
            DebugLog("📡 Server requesting connection info...");
            await SendConnectionInfo();
        }

        private void OnStartP2P(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<dynamic>();
                isInMatch = true;
                
                // Parse opponent info
                opponentInfo = new P2PPlayerInfo
                {
                    playerId = data.opponent.playerId,
                    playerName = data.opponent.playerName,
                    ip = data.opponent.ip,
                    port = data.opponent.port
                };
                
                // Parse my info
                myInfo = new P2PPlayerInfo
                {
                    playerId = data.yourInfo.playerId,
                    playerName = data.yourInfo.playerName,
                    ip = data.yourInfo.ip,
                    port = data.yourInfo.port
                };
                
                DebugLog($"🚀 P2P Connection Ready!");
                DebugLog($"   Your Info: {myInfo.ip}:{myInfo.port}");
                DebugLog($"   Opponent: {opponentInfo.playerName}");
                DebugLog($"   Opponent IP: {opponentInfo.ip}:{opponentInfo.port}");
                
                OnP2PReady?.Invoke(opponentInfo);
            }
            catch (System.Exception ex)
            {
                DebugLog($"Error parsing start P2P: {ex.Message}");
            }
        }

        private void OnOpponentDisconnected(SocketIOResponse response)
        {
            isInMatch = false;
            DebugLog("❌ Opponent disconnected!");
        }

        private void OnP2PMessage(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<dynamic>();
                var time = System.DateTime.Now.ToString("HH:mm:ss");
                string message = $"📨 [{time}] {data.senderName}: {data.message}";
                DebugLog(message);
                
                OnMessageReceived?.Invoke(data.senderName.ToString(), data.message.ToString());
            }
            catch (System.Exception ex)
            {
                DebugLog($"Error parsing P2P message: {ex.Message}");
            }
        }

        public async Task FindMatch()
        {
            if (!isConnected)
            {
                DebugLog("❌ Not connected to server!");
                return;
            }

            var playerData = new PlayerData
            {
                playerId = playerId,
                playerName = playerName
            };

            await socket.EmitAsync("findMatch", playerData);
            DebugLog("🔍 Searching for match...");
        }

        private async Task SendConnectionInfo()
        {
            // Simulate getting public IP (in real game, use STUN)
            var fakeIP = $"192.168.1.{UnityEngine.Random.Range(100, 254)}";
            var fakePort = UnityEngine.Random.Range(10000, 50000);

            var connectionInfo = new
            {
                ip = fakeIP,
                port = fakePort
            };

            await socket.EmitAsync("connectionInfo", connectionInfo);
            DebugLog($"📡 Sent connection info: {fakeIP}:{fakePort}");
        }

        public async Task SendMessage(string message)
        {
            if (!isInMatch || opponentInfo == null)
            {
                DebugLog("❌ Not in a match or no opponent!");
                return;
            }

            var p2pMessage = new
            {
                targetPlayerId = opponentInfo.playerId,
                message = message,
                senderName = playerName
            };

            await socket.EmitAsync("p2pMessage", p2pMessage);
            DebugLog($"📤 Sent: {message}");
        }

        // Public methods for UI
        [ContextMenu("Find Match")]
        public void FindMatchButton()
        {
            _ = FindMatch();
        }

        [ContextMenu("Send Test Message")]
        public void SendTestMessage()
        {
            _ = SendMessage("Hello from Unity!");
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[MatchmakingClient] {message}");
            }
        }

        async void OnDestroy()
        {
            if (socket != null)
            {
                await socket.DisconnectAsync();
                socket.Dispose();
            }
        }
    }

    // UI Controller cho Unity
    public class MatchmakingUI : MonoBehaviour
    {
        [Header("UI References")]
        public UnityEngine.UI.Button findMatchButton;
        public UnityEngine.UI.Button sendMessageButton;
        public UnityEngine.UI.InputField messageInput;
        public UnityEngine.UI.Text statusText;
        public UnityEngine.UI.Text chatText;
        
        private UnityMatchmakingClient client;

        void Start()
        {
            client = GetComponent<UnityMatchmakingClient>();
            
            // Setup events
            client.OnMatchFound += OnMatchFound;
            client.OnP2PReady += OnP2PReady;
            client.OnMessageReceived += OnMessageReceived;
            
            // Setup UI
            findMatchButton.onClick.AddListener(() => client.FindMatchButton());
            sendMessageButton.onClick.AddListener(SendMessage);
            
            sendMessageButton.interactable = false;
            statusText.text = "Connecting...";
        }

        private void OnMatchFound(string opponentName)
        {
            statusText.text = $"Match found! vs {opponentName}";
            findMatchButton.interactable = false;
        }

        private void OnP2PReady(UnityMatchmakingClient.P2PPlayerInfo opponent)
        {
            statusText.text = $"P2P Ready! Connected to {opponent.playerName}";
            sendMessageButton.interactable = true;
        }

        private void OnMessageReceived(string sender, string message)
        {
            chatText.text += $"\n{sender}: {message}";
        }

        private void SendMessage()
        {
            string message = messageInput.text.Trim();
            if (string.IsNullOrEmpty(message)) return;
            
            client.SendMessage(message);
            chatText.text += $"\nYou: {message}";
            messageInput.text = "";
        }
    }
}
