using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectSound;
using SlimDX.XAudio2;
using SlimDX.Multimedia;
using BufferFlags = SlimDX.XAudio2.BufferFlags;
using PlayFlags = SlimDX.XAudio2.PlayFlags;

namespace Client
{
    // Note: only playing WAV files for now
    class AudioSystem : IDisposable
    {
        private XAudio2 _xaudio;
        private MasteringVoice _master;

        public AudioSystem()
        {
            _xaudio = new XAudio2();
            _master = new MasteringVoice(_xaudio);
        }

        public void Play(string fileName)
        {
            var s = System.IO.File.OpenRead(fileName);
            var stream = new WaveStream(s);
            s.Close();

            AudioBuffer buffer = new AudioBuffer();
            buffer.AudioData = stream;
            buffer.AudioBytes = (int) stream.Length;
            buffer.Flags = BufferFlags.EndOfStream;

            SourceVoice sourceVoice = new SourceVoice(_xaudio, stream.Format);
            
        }

        public void Dispose()
        {
            _master?.Dispose();
            _xaudio?.Dispose();
        }
    }
}
