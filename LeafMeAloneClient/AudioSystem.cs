using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using libsndfile.NET;
using SlimDX;

namespace Client
{
    // Note: only playing WAV files for now
    class AudioSystem : IDisposable
    {
        private AudioContext _context;
        private Dictionary<String, int> _audioBuffers;

        public AudioSystem()
        { 
            _context = new AudioContext();  // use default audio device
            _audioBuffers = new Dictionary<string, int>();
        }

        /// <summary>
        /// Generate an audio source and return its ID
        /// Note that 3D audio effects might only be available for mono file formats
        /// </summary>
        /// <returns> ID of the source, save it for source manipulations and play </returns>
        public int GenSource()
        {
            return AL.GenSource();
        }

        /// <summary>
        /// Move the sound source to the specified src_position in the world coordinate system
        /// This is necessary for 3D audio effects.
        /// </summary>
        /// <param name="soundSource"> ID of the source </param>
        /// <param name="src_position"> Location of the source </param>
        public void UpdateSourcePosition(int soundSource, Vector3 src_position)
        {
            AL.Source(soundSource, ALSource3f.Position, src_position.X, src_position.Y, src_position.Z);
        }

        public void UpdateListenerPosition(Vector3 src_position)
        {

        }

        private void GenBuffer(string fileName)
        {
            // create a new file descriptor
            var wavfile = SndFile.OpenRead(fileName);

            // size of the wav file
            long bufferSize = wavfile.Format.Channels * wavfile.Frames;

            // read the sound data
            short[] soundData = new short[bufferSize];
            wavfile.ReadFrames(soundData, wavfile.Frames);

            // type of the wave file
            ALFormat subtype = 0;
            if (wavfile.Format.Subtype == SfFormatSubtype.PCM_16)
            {
                if (wavfile.Format.Channels == 1)
                {
                    subtype = ALFormat.Mono16;
                }
                else if (wavfile.Format.Channels == 2)
                {
                    subtype = ALFormat.Stereo16;
                }
            }
            else if (wavfile.Format.Subtype == SfFormatSubtype.PCM_S8)
            {
                if (wavfile.Format.Channels == 1)
                {
                    subtype = ALFormat.Mono8;
                }
                else if (wavfile.Format.Channels == 2)
                {
                    subtype = ALFormat.Stereo8;
                }
            }

            // frequency of the sound file
            int freq = wavfile.Format.SampleRate;

            // dispose file descriptor
            wavfile.Close();
            wavfile.Dispose();

            // create a openAL buffer
            int buffer = AL.GenBuffer();
            AL.BufferData(buffer, subtype, soundData, (int) bufferSize, freq);

            // assign it
            _audioBuffers[fileName] = buffer;
        }

        public void Play(string fileName, int soundSource, bool repeat = false)
        {
            // stop whatever it is playing right now
            AL.SourceStop(soundSource);

            // find the buffer
            int buffer = -1;
            if (!_audioBuffers.ContainsKey(fileName))
            {
                GenBuffer(fileName);
            }
            buffer = _audioBuffers[fileName];

            // bind the buffer to the source
            AL.BindBufferToSource(soundSource, buffer);

            // specify if the sound is repeating
            AL.Source(soundSource, ALSourceb.Looping, repeat);

            // play the source
            AL.SourcePlay(soundSource);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }


    }
}
