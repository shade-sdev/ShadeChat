using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadeClient.Audio
{
    internal class Speaker
    {

        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _waveProvider;

        public Speaker()
        {
            _waveOut = new WaveOutEvent();
            _waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
            _waveOut.Init(_waveProvider);
            _waveOut.DeviceNumber = 0;
            _waveOut.DesiredLatency = 100;
        }

        public void Play(byte[] data)
        {
            _waveProvider.AddSamples(data, 0, data.Length);
            _waveOut.PlaybackStopped += (s, e) =>
            {
                _waveOut.Dispose();
                _waveOut = new WaveOutEvent();
            };
            _waveOut.Play();
            _waveOut.GetType().GetProperty("Volume").SetValue(_waveOut, 1.0f, null);
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = new WaveOutEvent();
                _waveOut.Init(new RawSourceWaveStream(new MemoryStream(data), new WaveFormat(44100, 1)));
            }
        }
    }
}
