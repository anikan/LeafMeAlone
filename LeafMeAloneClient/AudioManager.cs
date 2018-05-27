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
        private static Dictionary<int, Queue<string>> _allQueueFiles;
        private static Dictionary<int, Queue<bool>> _allQueueRepeats;

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
            _allQueueFiles = new Dictionary<int, Queue<string>>();
            _allQueueRepeats = new Dictionary<int, Queue<bool>>();
        }

        private const int MAX_FRAME_PER_UDPATE = 15;


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
        /// Start playing some audio file
        /// </summary>
        /// <param name="source"> the source used for playing the clip </param>
        /// <param name="fileName"> filepath to the clip </param>
        /// <param name="repeat"> the audio should be repeated forever or not </param>
        public static void PlayAudio(int source, string fileName, bool repeat = false)
        {
            _audio.Play(source, fileName, repeat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"> the source used for playing the clip </param>
        /// <param name="fileName"> the path to the clip to be queued </param>
        /// <param name="repeat"> whether or not it should be repeated </param>
        public static void QueueAudioToSource(int source, string fileName, bool repeat = false)
        {
            if (!_allQueueFiles.ContainsKey(source))
            {
                _allQueueFiles[source] = new Queue<string>();
                _allQueueRepeats[source] = new Queue<bool>();
            }
            _allQueueFiles[source].Enqueue(fileName);
            _allQueueRepeats[source].Enqueue(repeat);
        }

        /// <summary>
        /// Remove the queue associated to this source
        /// </summary>
        /// <param name="source"> the source whose queue is to be removed </param>
        /// <param name="stopPlaying"> whether or not to stop playing the source </param>
        public static void RemoveSourceQueue(int source, bool stopPlaying = true)
        {
            if (stopPlaying)
            {
                _audio.Stop(source);
            }

            if (_allQueueFiles.ContainsKey(source))
            {
                _allQueueFiles.Remove(source);
                _allQueueRepeats.Remove(source);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public static void StopAudio(int source)
        {
            _audio.Stop(source);
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

            // Play all queues if needed
            foreach (var queue in _allQueueFiles)
            {
                // quit if queue is empty
                if (queue.Value.Count == 0) continue;
                
                // quit if still playing
                if (IsSourcePlaying(queue.Key)) continue;

                int src = queue.Key;
                string fileName = queue.Value.Dequeue();
                bool repeat = _allQueueRepeats[src].Dequeue();

                PlayAudio(src, fileName, repeat);
            }
            
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

        /// <summary>
        /// Play an audio from a pool then free that source automatically
        /// </summary>
        /// <param name="poolId"> the ID of the pool </param>
        /// <param name="src"> the source to play </param>
        /// <param name="fileName"> the filename of the audio to play </param>
        public static void PlayPoolSourceThenFree(int poolId, int src, string fileName)
        {
            ReusePoolSource(poolId, src, fileName, false);
            _freeSourceAfterPlay.Add(src);
            _freeSourcePoolAfterPlay.Add(poolId);
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
