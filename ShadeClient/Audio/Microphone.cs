using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadeClient.Audio
{
    public class Microphone
    {
        private WaveInEvent _waveIn;

        public Microphone()
        {
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 1),
                DeviceNumber = 0
            };
            _waveIn.DataAvailable += OnDataAvailable;
        }

        public void StartRecording()
        {
            _waveIn.StartRecording();
        }

        public void StopRecording()
        {
            _waveIn.StopRecording();
        }

        public event EventHandler<byte[]> DataAvailable;

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            DataAvailable.Invoke(this, e.Buffer);
        }
    }
}

