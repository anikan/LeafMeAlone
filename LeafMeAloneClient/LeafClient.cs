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

        public LeafClient(CreateObjectPacket createPacket) : base(createPacket, Constants.LeafModel)
        {
            _audioBurning = -1;
            Burnable = true;
        }

        public override void UpdateFromPacket(BasePacket packet)
        {
            bool prevBurning = Burning;
            base.UpdateFromPacket(packet);
            bool currBurning = Burning;

            EvaluateAudio(prevBurning, currBurning);
        }

        /// <summary>
        /// Evaluate audio logic
        /// </summary>
        /// <param name="prevBurning"> leaf previously burning? </param>
        /// <param name="currBurning"> leaf currently burning? </param>
        public void EvaluateAudio(bool prevBurning, bool currBurning)
        {
            // start burning audio
            if (!prevBurning && currBurning)
            {
                _audioBurning = AudioManager.UseNextPoolSource(GameClient.instance.leafAudioPoolId);
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
                AudioManager.RemoveSourceQueue(_audioBurning);
                AudioManager.PlayPoolSourceThenFree(GameClient.instance.leafAudioPoolId, _audioBurning, Constants.LeafPutoff);
                _audioBurning = -1;
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
                AudioManager.PlayPoolSourceThenFree(GameClient.instance.leafAudioPoolId, _audioBurning, Constants.LeafBurnup);
                _audioBurning = -1;
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            PlayBurnupAudio();
        }
    }
}
