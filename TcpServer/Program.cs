using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TcpServer
{

    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private const int PollInterval = 5;
        private readonly string _clientId;
        private bool _isConnected = true;
        private readonly Timer _statusTimer;

        public ClientHandler(TcpClient client, string clientId)
        {
            _client = client;
            _stream = client.GetStream();
            _clientId = clientId;

            // Poll for status 2 secs after connect and every 5 seconds onwards
            _statusTimer = new Timer(RequestStatus, null, 2_000, PollInterval * 1_000);
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client: {_clientId} connected from {_client.Client.RemoteEndPoint}");
        }

        public async Task HandleClientAsync()
        {
            var buffer = new byte[1024];
            
            try
            {
                while (_isConnected && _client.Connected)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client: {_clientId} disconnected");
                        break;
                    }

                    var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessReceivedMessage(receivedData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error communicating with client {_clientId}: {ex.Message}");
            }
            finally
            {
                Cleanup();
            }
        }

        private void ProcessReceivedMessage(string data)
        {
            try
            {
                var message = MessageFactory.ParseMessage(data);
                if (message == null) return;

                switch (message.Type)
                {
                    case MessageType.Ack:
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Received Ack from Client: {_clientId}");
                        break;
                    case MessageType.StatusResponse: 
                        PrintButtonStates(message.Content); 
                        break;
                    case MessageType.ClientBusy:
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Received Busy from Client: {_clientId}");
                        break;
                    default:
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Unsupported message type received from {_clientId}: {message.Type}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error parsing message from {_clientId}: {ex.Message}");
            }
        }

        private void PrintButtonStates(bool[] buttonStates)
        {
            Console.Write($"[{DateTime.Now:HH:mm:ss}] Client {_clientId}'s Button Status: ");
            
            for (int i = 0; i < buttonStates.Length; i++)
            {
                Console.Write($"Btn{i + 1}:{(buttonStates[i] ? "ON" : "OFF")} ");
            }
            Console.WriteLine();
        }

        private async void RequestStatus(object? state)
        {
            if (!_isConnected || !_client.Connected) return;

            try
            {
                var statusRequest = MessageFactory.CreateStatusRequestMessage();

                var jsonMessage = JsonSerializer.Serialize(statusRequest);
                var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                await _stream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error requesting status from {_clientId}: {ex.Message}");
                _isConnected = false;
            }
        }

        private void Cleanup()
        {
            _isConnected = false;
            _statusTimer.Dispose();
            _stream.Close();
            _client.Close();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client: {_clientId}'s handler cleaned up");
        }
    }

    public class TcpServerApp
    {
        // Will add support for viewing a list of connected clients through a UI
        // private readonly List<ClientHandler> _clients = [];
        // private readonly object _clientsLock = new();  // there's no built-in concurrent list in C#. So using manual lock
        
        private TcpListener? _listener;
        private bool _isRunning;
        private const int Port = 8080;

        public void StartServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
                _isRunning = true;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Server listening on port {Port}"); 
                
                // Client handling thread
                var task = Task.Run(AcceptClientsAsync);
                task.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Server error: {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync()
        {
            var clientCounter = 0;

            while (_isRunning)
            {
                try
                {
                    var tcpClient = await _listener!.AcceptTcpClientAsync();
                    clientCounter++;
                    
                    var clientHandler = new ClientHandler(tcpClient, $"Client-{clientCounter}");

                    // Handle client in a separate thread
                    _ = Task.Run(async () =>
                    {
                        await clientHandler.HandleClientAsync();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error accepting client: {ex.Message}");
                }
            }
        }

        public void StopServer()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Shutting down server...");
            
            _isRunning = false;
            _listener?.Stop(); 
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Server stopped");
        }
    }

    class Program
    {
        public static void Main()
        {
            Console.WriteLine("TCP Server Application");
            Console.WriteLine("=======================");

            var server = new TcpServerApp();
            
            // This is good habits - handle Ctrl+C gracefully
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                server.StopServer();
                Environment.Exit(0);
            };

            server.StartServer();
        }
    }
}