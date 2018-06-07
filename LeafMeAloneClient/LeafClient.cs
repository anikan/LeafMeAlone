using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Shared;
using Shared.Packet;
using SlimDX;

namespace Client
{

    /// <summary>
    /// A leaf on the client, mainly for rendering.
    /// </summary>
    class LeafClient : NetworkedGameObjectClient
    {
        private int _audioBurning;
        private int _audioMoving;

        public LeafClient(CreateObjectPacket createPacket) : base(createPacket, Constants.LeafModel)
        {
            _audioBurning = -1;
            _audioMoving = -1;
            Burnable = true;
        }

        public override void UpdateFromPacket(BasePacket packet)
        {
            bool prevBurning = Burning;
            bool prevMoving = Moving;
            base.UpdateFromPacket(packet);
            bool currBurning = Burning;
            bool currMoving = Moving;    

            if (_audioBurning != -1) AudioManager.UpdateSourceLocation(_audioBurning, Transform.Position);
            EvaluateAudio(prevBurning, currBurning, prevMoving, currMoving);
            
            //If the leaf is burning then change the leaf color.
            if (Burning)
            {
                CurrentTint = new Vector3(1, 1, 1) * ((Health / Constants.LEAF_HEALTH) * .7f + .3f);
            }
        }

        /// <summary>
        /// Evaluate audio logic
        /// </summary>
        /// <param name="prevBurning"> leaf previously burning? </param>
        /// <param name="currBurning"> leaf currently burning? </param>
        public void EvaluateAudio(bool prevBurning, bool currBurning, bool prevMoving, bool currMoving)
        {
            // start burning audio
            if (!prevBurning && currBurning)
            {
                _audioBurning = AudioManager.UseNextPoolSource(GameClient.instance.AudioPoolLeafBurning);
                if (_audioBurning != -1)
                {
                    AudioManager.StopAudio(_audioBurning);
                    AudioManager.QueueAudioToSource(_audioBurning, Constants.LeafIgniting, false);
                    AudioManager.QueueAudioToSource(_audioBurning, Constants.LeafBurning, true);
                }
            }
            // start putoff audio
            else if (prevBurning && !currBurning && _audioBurning != -1)
            {
                AudioManager.StopAudio(_audioBurning);
                AudioManager.PlayPoolSourceThenFree(GameClient.instance.AudioPoolLeafBurning, _audioBurning, Constants.LeafPutoff);
                _audioBurning = -1;
            }

            // play moving sounds
            if (!prevMoving && currMoving)
            {
                _audioMoving = AudioManager.UseNextPoolSource(GameClient.instance.AudioPoolLeafMoving);
                if (_audioMoving != -1)
                {
                    AudioManager.PlayAudio(_audioMoving, Constants.LeafMoving);
                }
            }
            // stop moving sounds
            else if (_audioMoving != -1 && !AudioManager.IsSourcePlaying(_audioMoving))
            {
                AudioManager.StopAudio(_audioMoving);
                AudioManager.FreeSource(GameClient.instance.AudioPoolLeafMoving, _audioMoving);
                _audioMoving = -1;
            }
        }

        /// <summary>
        /// Play the deathcry sound
        /// </summary>
        public void PlayBurnupAudio()
        {
            if (_audioBurning != -1)
            {
                AudioManager.RemoveSourceQueue(_audioBurning);
                AudioManager.PlayPoolSourceThenFree(GameClient.instance.AudioPoolLeafBurning, _audioBurning, Constants.LeafBurnup);
                _audioBurning = -1;
            }
        }

        public override void Draw()
        {
            base.Draw();

            //if the object is currently burning, draw the fire on them.
            if (Burning)
            {
                Transform t = new Transform { Position = Transform.Position, Scale = new Vector3(1, 1, 1) };
                GraphicsManager.DrawParticlesThisFrame(Fire, t);
            }

        }

        public override void Die()
        {
            base.Die();

            PlayBurnupAudio();
        }
    }
}
