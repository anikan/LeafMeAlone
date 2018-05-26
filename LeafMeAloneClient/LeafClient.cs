using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Packet;
using SlimDX;

namespace Client
{
    /// <summary>
    /// For controlling the burning sound of leaves
    /// </summary>
    public enum LeafState : byte
    {
        Ignite = 1,
        Loop = 2,
        Putoff = 3 ,
        Burnup = 4,
        Inactive = 5
    }

    /// <summary>
    /// A leaf on the client, mainly for rendering.
    /// </summary>
    class LeafClient : NetworkedGameObjectClient
    {
        private int _audioLeaf;
        private LeafState _leafState;
        private bool _isPlaying;

        public LeafClient(CreateObjectPacket createPacket) :
            base(createPacket, Constants.LeafModel)
        {
            _audioLeaf = -1;
            _leafState = LeafState.Inactive;
            _isPlaying = false;
        }

        /// <summary>
        /// Update the audio states, and play the sounds if necessary
        /// </summary>
        /// <param name="audioPoolID"> the pool to use </param>
        public void UpdateAudio(int audioPoolID)
        {
            if (_isPlaying)
            {
                _leafState = (LeafState) AudioManager.EvaluateBurningObjectAudio(Burning, false, _audioLeaf, (byte) _leafState, 
                    Constants.LeafIgniting, Constants.LeafBurning, Constants.LeafBurnup, Constants.LeafPutoff);
                if (_leafState == LeafState.Inactive)
                {
                    AudioManager.FreeSource(audioPoolID, _audioLeaf);
                    _isPlaying = false;
                }
            }
            else if (Burning)
            {
                _audioLeaf = AudioManager.UseNextPoolSource(audioPoolID, Constants.LeafIgniting, false);
                if (_audioLeaf != -1)
                {
                    _leafState = LeafState.Ignite;
                    _isPlaying = true;
                }
            }
        }

        /// <summary>
        /// Play the burnup audio, making sure to free the resouce afterwards
        /// </summary>
        /// <param name="audioPoolID"> the audio source pool to use </param>
        public void PlayBurnupAudio(int audioPoolID)
        {
            if (!_isPlaying)
            {
                _audioLeaf = AudioManager.UseNextPoolSource(audioPoolID, null, false);
                _isPlaying = _audioLeaf != -1;
            }

            _leafState = LeafState.Burnup;
            if (_audioLeaf != -1)
            {
                AudioManager.PlayAudioThenFree(audioPoolID, _audioLeaf, Constants.LeafBurnup);
            }
        }
    }
}
