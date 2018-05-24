using System;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using libsndfile.NET;
using SlimDX;

namespace Client
{
    // Note: only playing WAV 8/16 bits mono/stereo files for now
    // Note: For 3D sound effects, must use mono wav files
    class AudioSystem : IDisposable
    {
        private AudioContext _context;
        private Dictionary<String, int> _audioBuffers;
        private static int sourceCount = 0;

        /// <summary>
        /// Create a new audio system
        /// </summary>
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
            int src = AL.GenSource();
            sourceCount++;

            ALError err = AL.GetError();
            if ( err != ALError.NoError)
            {
                Console.WriteLine("Generating " + sourceCount + "th source, encountered error: " + AL.GetErrorString(err));
            }

            return src;
        }

        /// <summary>
        /// Move the sound source to the specified srcPosition in the world coordinate system
        /// This is necessary for 3D audio effects.
        /// </summary>
        /// <param name="soundSource"> ID of the source </param>
        /// <param name="srcPosition"> Location of the source </param>
        public void UpdateSourcePosition(int soundSource, Vector3 srcPosition)
        {
            AL.Source(soundSource, ALSource3f.Position, srcPosition.X, srcPosition.Y, srcPosition.Z);
        }

        /// <summary>
        /// Update the position of the listener, typically the position of the camera
        /// </summary>
        /// <param name="listenerPosition"> new position of the listener </param>
        /// <param name="listenerLookat"> new look-at position of the listener </param>
        /// <param name="listenerUp"> new up direction of the listener </param>
        public void UpdateListener(Vector3 listenerPosition, Vector3 listenerLookat, Vector3 listenerUp)
        {
            AL.Listener(ALListener3f.Position, listenerPosition.X, listenerPosition.Y, listenerPosition.Z);
            Vector3 lookAtDirection = Vector3.Normalize(listenerPosition - listenerLookat);
            OpenTK.Vector3 dir = new OpenTK.Vector3(lookAtDirection.X, lookAtDirection.Y, lookAtDirection.Z);
            OpenTK.Vector3 up = new OpenTK.Vector3(listenerUp.X, listenerUp.Y, listenerUp.Z);
            AL.Listener(ALListenerfv.Orientation, ref dir, ref up);
        }

        /// <summary>
        /// Generate a new buffer by reading in an audio file
        /// </summary>
        /// <param name="fileName"></param>
        private void GenBuffer(string fileName)
        {
            // create a new file descriptor
            var wavfile = SndFile.OpenRead(fileName);
            if (wavfile.GetError() != SfError.NoError)
            {
                return;
            }

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

            // unsupported file type, so return
            if (subtype == 0) return;

            // create a openAL buffer
            int buffer = AL.GenBuffer();
            AL.BufferData(buffer, subtype, soundData, (int) bufferSize * sizeof(short), freq);

            // assign it
            _audioBuffers[fileName] = buffer;
        }

        /// <summary>
        /// Play the source with the audio data stored in the specified file name
        /// </summary>
        /// <param name="fileName"> file to be played </param>
        /// <param name="soundSource"> source where the sound is played </param>
        /// <param name="repeat"> loop the play infinitely or not </param>
        public void Play(int soundSource, string fileName, bool repeat = false)
        {
            // stop whatever it is playing right now
            AL.SourceStop(soundSource);

            BindBufferToSource(soundSource, fileName);

            // specify if the sound is repeating
            AL.Source(soundSource, ALSourceb.Looping, repeat);

            // play the source
            AL.SourcePlay(soundSource);
        }

        /// <summary>
        /// Play the source with whatever buffer that is bound to it previously
        /// </summary>
        /// <param name="soundSource"> source where the sound is played </param>
        /// <param name="repeat"> loop the play infinitely or not </param>
        public void Play(int soundSource, bool repeat = false)
        {
            // stop whatever it is playing right now
            AL.SourceStop(soundSource);

            // specify if the sound is repeating
            AL.Source(soundSource, ALSourceb.Looping, repeat);

            // play the source
            AL.SourcePlay(soundSource);
        }

        public void BindBufferToSource(int soundSource, string fileName)
        {
            // find the buffer
            int buffer = -1;
            if (!_audioBuffers.ContainsKey(fileName))
            {
                GenBuffer(fileName);
                if (!_audioBuffers.ContainsKey(fileName))
                {
                    Console.WriteLine("Invalid Audio File: " + fileName);
                    return;
                }

                buffer = _audioBuffers[fileName];
            }
            else
            {
                buffer = _audioBuffers[fileName];
            }

            // bind the buffer to the source
            AL.BindBufferToSource(soundSource, buffer);
        }

        /// <summary>
        /// Stop playing the sound source
        /// </summary>
        /// <param name="soundSource"> source ID to be stopped </param>
        public void Stop(int soundSource)
        {
            AL.SourceStop(soundSource);
        }

        /// <summary>
        /// Pause playing the sound source
        /// </summary>
        /// <param name="soundSource"> source ID to be paused </param>
        public void Pause(int soundSource)
        {
            AL.SourcePause(soundSource);
        }

        /// <summary>
        /// Resume Playing the sound source
        /// </summary>
        /// <param name="soundSource"> source ID to be resumed </param>
        public void Resume(int soundSource)
        {
            if (AL.GetSourceState(soundSource) == ALSourceState.Paused)
            {
                AL.SourcePlay(soundSource);
            }
        }

        /// <summary>
        /// Rewind the sound source
        /// </summary>
        /// <param name="soundSource"> source ID to be rewinded </param>
        public void Rewind(int soundSource)
        {
            AL.SourceRewind(soundSource);
        }

        /// <summary>
        /// Check if the sound source is playing something
        /// </summary>
        /// <param name="soundSource"> check if the sound source is playing </param>
        /// <returns> true if it is playing, false otherwise </returns>
        public bool IsPlaying(int soundSource) => AL.GetSourceState(soundSource) == ALSourceState.Playing;

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }


    }
}
