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

        private WaveInEvent _waveIn;
        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _waveProvider;

        public Client(string ipAddress, int port)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        public void Connect()
        {
            _client = new UdpClient();
            _client.Connect(ServerEndPoint);
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
    }
}
