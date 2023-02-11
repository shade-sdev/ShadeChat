using NAudio.Utils;
using NAudio.Wave;
using ShadeClient.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadeClient.Network
{
    internal class Client
    {

        private IPEndPoint ServerEndPoint;
        private UdpClient _client;

        private TcpClient _chatClient;
        private NetworkStream _stream;

        public Client(string ipAddress, int port)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        public void Connect()
        {
            _client = new UdpClient();
            _client.Connect(ServerEndPoint);

            _chatClient = new TcpClient(ServerEndPoint.Address.ToString(), ServerEndPoint.Port);
            _stream = _chatClient.GetStream();
        }

        public void SendAudio()
        {
            Microphone microphone = new Microphone();
            microphone.StartRecording();
            Task.Factory.StartNew(() =>
            {
                microphone.DataAvailable += (s, e) =>
                {
                    byte[] buffer = e;
                    _client.SendAsync(buffer, buffer.Length);
                };
            });
        }

        public void ReceiveAudio()
        {
            Speaker speaker = new Speaker();
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    UdpReceiveResult result = await _client.ReceiveAsync();
                    speaker.Play(result.Buffer);
                }
            });
        }

        public void ReceiveMessage()
        {
            Thread receiveThread = new Thread(ReceiveMessageSlave);
            receiveThread.Start();
        }

        private void ReceiveMessageSlave()
        {
            while (true)
            {
                if (_stream.DataAvailable)
                {
                    byte[] messageWithHeader = new byte[_chatClient.ReceiveBufferSize];
                    int bytesRead = _stream.Read(messageWithHeader, 0, _chatClient.ReceiveBufferSize);

                    byte messageType = messageWithHeader[0];
                    Console.WriteLine(messageType);
                    byte[] messageData = new byte[bytesRead - 1];
                    Array.Copy(messageWithHeader, 1, messageData, 0, bytesRead - 1);

                    string message = Encoding.UTF8.GetString(messageData);
                    Console.WriteLine("Received message: " + message);
                }
            }
        }

        public void SendMessage(byte messageType, string message)
        {
            byte[] messageData = System.Text.Encoding.UTF8.GetBytes(message);
            byte[] messageWithHeader = new byte[messageData.Length + 1];
            messageWithHeader[0] = messageType;
            Array.Copy(messageData, 0, messageWithHeader, 1, messageData.Length);

            _stream.Write(messageWithHeader, 0, messageWithHeader.Length);
        }
    }
}

