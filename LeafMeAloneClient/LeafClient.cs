using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
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

        public void PlayDeathAudio(int audioPoolID)
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

        /// <summary>
        /// Update from a server packet.
        /// </summary>
        /// <param name="packet">Packet from the server.</param>
        public void UpdateFromPacket(ObjectPacket packet)
        {
            // Set the initial positions of the object.
            Transform.Position.X = packet.MovementX;
            Transform.Position.Z = packet.MovementZ;
            Transform.Rotation.Y = packet.Rotation;

            // Set the initial burning status.
            Burning = packet.Burning;
        }

        /// <summary>
        /// Update this object from a server packet.
        /// </summary>
        /// <param name="packet">Packet from server.</param>
        public override void UpdateFromPacket(Packet packet)
        {
            UpdateFromPacket(packet as ObjectPacket);
        }
    }
}
