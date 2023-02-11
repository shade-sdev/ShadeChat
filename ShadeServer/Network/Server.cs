using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadeServer.Network
{
    internal class Server
    {
        private const int Port = 9090;
        private readonly IPEndPoint _endPoint;
        private readonly UdpClient _server;
        private readonly ConcurrentDictionary<IPEndPoint, byte[]> _clients;

        private readonly TcpListener _chatServer;
        private readonly List<TcpClient> _tcpClients;
        private readonly object lockObject;
        private readonly SemaphoreSlim _semaphore;

        public Server()
        {
            _endPoint = new IPEndPoint(IPAddress.Parse("192.168.100.42"), Port);
            _server = new UdpClient(_endPoint);
            _clients = new ConcurrentDictionary<IPEndPoint, byte[]>();

            _chatServer = new TcpListener(IPAddress.Parse("192.168.100.42"), Port);
            _tcpClients = new List<TcpClient>();
            lockObject = new object();
            _semaphore= new SemaphoreSlim(1);
        }

        public void Start()
        {
            VoiceServerStart();
            ChatServerStart();
        }

        private async void ChatServerStart()
        {
            _chatServer.Start();
            Console.WriteLine("Chat Server Started...");

            while (true)
            {

                TcpClient client = await _chatServer.AcceptTcpClientAsync();
                Console.WriteLine(client.Client.RemoteEndPoint.ToString());
                lock (lockObject)
                {
                    _tcpClients.Add(client);
                }
                _ = HandleClientAsync(client);
            }
        }

        private async void VoiceServerStart()
        {
            Console.WriteLine("Voice Chat Server started.");
            Console.WriteLine("Listening on: " + _endPoint);

            while (true)
            {
                try
                {
                    var receiveResult = await _server.ReceiveAsync();
                    var client = receiveResult.RemoteEndPoint;
                    var data = receiveResult.Buffer;

                    if (!_clients.ContainsKey(client))
                    {
                        Console.WriteLine("New client connected: " + client);
                        _clients.TryAdd(client, data);
                    }
                    else
                    {
                        _clients[client] = data;
                    }

                    // Receive audio from client in a separate thread
                    await Task.Run(() => ReceiveAudio(client, data));
                }
                catch (Exception ex) { }
            }
        }

        private async Task ReceiveAudio(IPEndPoint client, byte[] data)
        {
            Console.WriteLine("Received audio from: " + client);
            // Send audio to all other clients except the sender
            await SendAudio(client, data);
        }

        private async Task SendAudio(IPEndPoint sender, byte[] data)
        {
            Console.WriteLine("Sending audio to all clients except: " + sender);
            foreach (var client in _clients.Where(c => !c.Key.Equals(sender)))
            {
                Console.WriteLine("Sending audio to: " + client.Key);
                await _server.SendAsync(data, data.Length, client.Key);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }

                byte messageType = buffer[0];
                byte[] messageData = new byte[bytesRead - 1];
                Array.Copy(buffer, 1, messageData, 0, messageData.Length);

                switch (messageType)
                {
                    case 1:
                        // Handle text message
                        string textMessage = System.Text.Encoding.UTF8.GetString(messageData);
                        Console.WriteLine("Received text message: " + textMessage);
                        await SendMessageToAllClientsAsync(messageType, messageData);
                        break;
                    case 2:
                        // Handle picture message
                        Console.WriteLine("Received picture message");
                        await SendMessageToAllClientsAsync(messageType, messageData);
                        break;
                    case 3:
                        // Handle audio message
                        Console.WriteLine("Received audio message");
                        await SendMessageToAllClientsAsync(messageType, messageData);
                        break;
                    default:
                        Console.WriteLine("Received unknown message type");
                        break;
                }
            }

            lock (lockObject)
            {
                _tcpClients.Remove(client);
            }
            stream.Close();
            client.Close();
        }

        private async Task SendMessageToAllClientsAsync(byte messageType, byte[] messageData)
        {
            foreach (TcpClient client in _tcpClients)
            {
                NetworkStream stream = client.GetStream();

                byte[] messageWithHeader = new byte[messageData.Length + 1];
                messageWithHeader[0] = messageType;
                Array.Copy(messageData, 0, messageWithHeader, 1, messageData.Length);

                await stream.WriteAsync(messageWithHeader, 0, messageWithHeader.Length);
            }
        }
    }

}

