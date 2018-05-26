using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// Manages all audio stuffs with a singleton audio system
    /// </summary>
    public static class AudioManager
    {
        private static AudioSystem _audio;
        
        private static List<AudioSourcePool> _allPools;

        private static List<int> _freeSourceAfterPlay;
        private static List<int> _freeSourcePoolAfterPlay;

        private static int _countPools = 0;
        private static int _srcBgm;
        
        public const byte GenericToolStart = 1;
        public const byte GenericToolLoop = 2;
        public const byte GenericToolEnd = 3;
        public const byte GenericToolInactive = 4;

        public const byte GenericLoopOnlyActiveState = 1;
        public const byte GenericLoopOnlyInactiveState = 2;

        public const byte GenericBurningObjectIgnite = 1;
        public const byte GenericBurningObjectLoop = 2;
        public const byte GenericBurningObjectBurnup = 3;
        public const byte GenericBurningObjectPutoff = 4;
        public const byte GenericBurningObjectInactive = 5;

        /// <summary>
        /// Initialize fields
        /// </summary>
        public static void Init()
        {
            _audio = new AudioSystem();
            _srcBgm = _audio.GenSource();
            _allPools = new List<AudioSourcePool>();
            _freeSourcePoolAfterPlay = new List<int>();
            _freeSourceAfterPlay = new List<int>();
        }

        private const int MAX_FRAME_PER_UDPATE = 15;

        /// <summary>
        /// Play an audio from a pool then free that source automatically
        /// </summary>
        /// <param name="poolId"> the ID of the pool </param>
        /// <param name="src"> the source to play </param>
        /// <param name="fileName"> the filename of the audio to play </param>
        public static void PlayAudioThenFree(int poolId, int src, string fileName)
        {
            ReusePoolSource(poolId, src, fileName, false);
            _freeSourceAfterPlay.Add(src);
            _freeSourcePoolAfterPlay.Add(poolId);
        }

        /// <summary>
        /// Used for evaluating burning object audio logic
        /// </summary>
        /// <param name="burning"> whether or not the object is burning </param>
        /// <param name="burnup"> whether or not the object is burned up </param>
        /// <param name="src"> the audio source to play the sound </param>
        /// <param name="currentState"> the current state of the object </param>
        /// <param name="igniteFile"> the audio file for ignition </param>
        /// <param name="loopFile"> the audio file for looping/burning </param>
        /// <param name="burnupFile"> the audio file for burned up </param>
        /// <param name="putoffFile"> the audio file for putting off </param>
        /// <returns> the next state </returns>
        public static byte EvaluateBurningObjectAudio(bool burning, bool burnup, int src, byte currentState, 
            string igniteFile, string loopFile, string burnupFile, string putoffFile)
        {
            byte nextState = currentState;
            switch (currentState)
            {
                case GenericBurningObjectInactive:
                {
                    break;
                }
                case GenericBurningObjectIgnite: 
                {
                    if (!_audio.IsPlaying(src) )
                    {
                        if (loopFile != null)
                        {
                            _audio.Play(src, loopFile, true);
                        }

                        nextState = GenericBurningObjectLoop;
                    }
                    else if (burnup)
                    {
                        if (burnupFile != null)
                        {
                            _audio.Play(src, burnupFile, false);
                        }

                        nextState = GenericBurningObjectBurnup;
                    }
                    else if (!burning)
                    {
                        if (putoffFile != null)
                        {
                            _audio.Play(src, putoffFile, false);
                        }

                        nextState = GenericBurningObjectPutoff;
                    }
                    break;
                }
                case GenericBurningObjectLoop:
                {
                    if (burnup)
                    {
                        if (burnupFile != null)
                        {
                            _audio.Play(src, burnupFile, false);
                        }

                        nextState = GenericBurningObjectBurnup;
                    }
                    else if (!burning)
                    {
                        if (putoffFile != null)
                        {
                            _audio.Play(src, putoffFile, false);
                        }

                        nextState = GenericBurningObjectPutoff;
                    }
                    break;
                }
                case GenericBurningObjectPutoff:
                {
                    if (!_audio.IsPlaying(src))
                    {
                        nextState = GenericBurningObjectInactive;
                    }
                    break;
                }
                case GenericBurningObjectBurnup:
                {
                    if (!_audio.IsPlaying(src))
                    {
                        nextState = GenericBurningObjectBurnup;
                    }
            
                    break;
                }
            }

            return nextState;
        }


        /// <summary>
        /// Used for evaluating looping audio logic
        /// </summary>
        /// <param name="looping"> Whether or not the looping music should be played </param>
        /// <param name="src"> The source to play the looping music </param>
        /// <param name="currentState"> The current state of the audio logic </param>
        /// <param name="loopFile"> The looping audio filed to be played; null if not needed </param>
        /// <returns> the next state </returns>
        public static byte EvaluateLoopOnlyAudio(bool looping, int src, byte currentState, string loopFile)
        {
            byte nextState = currentState;
            switch (currentState)
            {
                case GenericLoopOnlyInactiveState:
                {
                    if (looping)
                    {
                        nextState = GenericLoopOnlyActiveState;
                        if (loopFile != null)
                        {
                            _audio.Play(src, loopFile, true);
                        }
                    }
                    break;
                }
                case GenericLoopOnlyActiveState:
                {
                    if (!looping)
                    {
                        nextState = GenericLoopOnlyInactiveState;
                        _audio.Stop( src );
                    }
                    break;
                }
            }

            return nextState;
        }

        /// <summary>
        /// Used for evaluating tool audio, and playing them
        /// </summary>
        /// <param name="inUse"> whether the tool is being used or not </param>
        /// <param name="src"> the audio source that is generated and stored </param>
        /// <param name="currentState"> the current state </param>
        /// <param name="startFile"> the audio file of the start sequence; null if no audio is to be played </param>
        /// <param name="loopFile"> the audio file of the loop sequence; null if no audio is to be played </param>
        /// <param name="endFile"> the audio file of the end sequence; null if no audio is to be played </param>
        /// <returns> the evaluated state </returns>
        /// <side_effect> audio file will be played accordingly </side_effect>
        public static byte EvaluateToolAudio(bool inUse, int src, byte currentState, string startFile, string loopFile, string endFile)
        {
            byte nextState = currentState;

            switch (currentState)
            {
                case GenericToolInactive:
                {
                    if (inUse)
                    {
                        if (startFile != null)
                        {
                            _audio.Play(src, startFile, false);
                        }

                        nextState = GenericToolStart;
                    }
                    break;
                }
                case GenericToolStart:
                {
                    if (inUse)
                    {
                        if (! _audio.IsPlaying( src ) )
                        {
                            if (loopFile != null)
                            {
                                _audio.Play(src, loopFile, true);
                            }

                            nextState = GenericToolLoop;
                        }
                    }
                    else
                    {
                        if (endFile != null)
                        {
                            _audio.Play(src, endFile, false);
                        }

                        nextState = GenericToolEnd;
                    }
                    break;
                }
                case GenericToolLoop:
                {
                    if (!(inUse))
                    {
                        if (endFile != null)
                        {
                            _audio.Play(src, endFile, false);
                        }

                        nextState = GenericToolEnd;
                    }
                    break;
                }
                case GenericToolEnd:
                {
                    if (!_audio.IsPlaying( src ))
                    {
                        nextState = GenericToolInactive;
                    }
                    else if (inUse)
                    {
                        if (startFile != null)
                        {
                            _audio.Play(src, startFile, false);
                        }

                        nextState = GenericToolStart;
                    }
                    break;
                }
            }

            return nextState;
        }

        /// <summary>
        /// Update audio effects
        /// </summary>
        public static void Update()
        {
            // update the listener orientation
            _audio.UpdateListener(GraphicsManager.ActiveCamera.CameraPosition, GraphicsManager.ActiveCamera.CameraLookAt,
                GraphicsManager.ActiveCamera.CameraUp);

            // If some audio needs to be freed automatically...
            for (int i = 0; i < _freeSourceAfterPlay.Count; i++)
            {
                if (!IsSourcePlaying(_freeSourceAfterPlay[i]))
                {
                    FreeSource( _freeSourcePoolAfterPlay[i], _freeSourceAfterPlay[i] );
                    _freeSourceAfterPlay.RemoveAt(i);
                    _freeSourcePoolAfterPlay.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Check if the source is playing
        /// </summary>
        /// <param name="src"> the source to be checked </param>
        /// <returns> true if the source is playing something, false otherwise </returns>
        public static bool IsSourcePlaying(int src)
        {
            return _audio.IsPlaying(src);
        }

        /// <summary>
        /// Generate a new source to use
        /// </summary>
        /// <returns> the new source </returns>
        public static int GetNewSource()
        {
            return _audio.GenSource();
        }

        /// <summary>
        /// Find a new source to use and play it
        /// </summary>
        /// <param name="poolID"> specify which pool to use </param>
        /// <param name="audioFile"> audiofile to be played; null if don't need to play </param>
        /// <param name="loop"> whether or not the audio is looped </param>
        /// <returns> the source ID that is being used </returns>
        public static int UseNextPoolSource(int poolID, string audioFile = null, bool loop = false)
        {
            return _allPools[poolID].UseNextPoolSource(audioFile, loop);
        }

        /// <summary>
        /// Play a source with a source that is already allocated
        /// </summary>
        /// <param name="poolID"> specify which pool to use </param>
        /// <param name="src"> the source to reuse </param>
        /// <param name="audioFile"> the audio file to be played </param>
        /// <param name="loop"> whether or not the audio is looped </param>
        public static void ReusePoolSource(int poolID, int src, string audioFile, bool loop)
        {
            _allPools[poolID].PlayThisSource(src, audioFile, loop);
        }

        /// <summary>
        /// Free the source so that it can be reused by others
        /// </summary>
        /// <param name="poolID"> The ID of the pool </param>
        /// <param name="src"> The source to be freed </param>
        public static void FreeSource(int poolID, int src)
        {
            _allPools[poolID].FreeSource(src);
        }

        /// <summary>
        /// Create a new pool of audio sources
        /// </summary>
        /// <param name="capacity"> capacity of the audio source list to be used </param>
        /// <returns> the ID of the new pool </returns>
        public static int NewSourcePool(int capacity)
        {
            _allPools.Add(new AudioSourcePool(capacity, _audio));
            
            _countPools++;
            return _countPools - 1;
        }
    }

    /// <summary>
    /// An audio pool, to help managing pooling multiple sources
    /// </summary>
    public class AudioSourcePool
    {
        /// <summary>
        /// The list of all sources that are pooled
        /// </summary>
        public List<int> Src;

        /// <summary>
        /// The list that indicates which sources are available
        /// </summary>
        public List<bool> Avail;

        /// <summary>
        /// The map from source to list index of the previous two lists
        /// </summary>
        public Dictionary<int, int> SrcMap;

        /// <summary>
        /// The audio system to use
        /// </summary>
        private AudioSystem _audio;


        /// <summary>
        /// Constructor: initialize the pool with the desired number of sources
        /// </summary>
        /// <param name="capacity"> the number of sources in this pool </param>
        /// <param name="audio"> the audio system that needs to be used </param>
        public AudioSourcePool(int capacity, AudioSystem audio)
        {
            Src = new List<int>();
            Avail = new List<bool>();
            SrcMap = new Dictionary<int, int>();

            for (int i = 0; i < capacity; i++)
            {
                int src = audio.GenSource();
                Src.Add(src);
                Avail.Add(true);
                SrcMap[src] = i;
            }

            _audio = audio;
        }

        /// <summary>
        /// Play the source with some audio
        /// </summary>
        /// <param name="src"> source to use </param>
        /// <param name="audioFile"> audio file to play </param>
        /// <param name="loop"> whether the audio is looped or not </param>
        public void PlayThisSource(int src, string audioFile, bool loop)
        {
            int srcIndex = SrcMap[src];
            Avail[srcIndex] = false;

            _audio.Play(src, audioFile, loop);
        }
        
        /// <summary>
        /// Find a new source to allocate so that another entity can use it 
        /// </summary>
        /// <param name="audioFile"> play the audio if not null </param>
        /// <param name="loop"> if played, specify if it should be repeated </param>
        /// <returns> the new source to be used </returns>
        public int UseNextPoolSource(string audioFile, bool loop)
        {
            int nextSrcIndex = Avail.IndexOf(true);
            if (nextSrcIndex == -1) return -1;

            int src = Src[nextSrcIndex];
            Avail[nextSrcIndex] = false;

            if (audioFile != null)
            {
                _audio.Play(src, audioFile, loop);
            }

            return src;
        }

        /// <summary>
        /// Free the source so that it can be reused by others
        /// </summary>
        /// <param name="src"> source to be freed </param>
        public void FreeSource(int src)
        {
            int srcIndex = SrcMap[src];
            Avail[srcIndex] = true;
            _audio.Stop(src);
        }
    }
}
