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


        public Server()
        {
            _endPoint = new IPEndPoint(IPAddress.Parse("192.168.100.42"), Port);
            _server = new UdpClient(_endPoint);
            _clients = new ConcurrentDictionary<IPEndPoint, byte[]>();
        }

        public async void Start()
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
                } catch(Exception ex) { }
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
    }

}

