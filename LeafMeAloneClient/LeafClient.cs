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
        private int _srcBurning;

        public LeafClient(CreateObjectPacket createPacket) :
            base(createPacket, Constants.LeafModel)
        {
            _srcBurning = -1;
        }

        public override void UpdateFromPacket(BasePacket packet)
        {
            bool prevBurning = Burning;
            base.UpdateFromPacket(packet);
            bool currBurning = Burning;

            // start burning audio
            if (!prevBurning && currBurning)
            {
                _srcBurning = AudioManager.UseNextPoolSource(GameClient.instance.leafAudioPoolId);
                if (_srcBurning != -1)
                {
                    AudioManager.StopAudio(_srcBurning);
                    AudioManager.QueueAudioToSource(_srcBurning, Constants.LeafIgniting, false);
                    AudioManager.QueueAudioToSource(_srcBurning, Constants.LeafBurning, true);
                }
            }
            // start putoff audio
            else if (prevBurning && !currBurning && _srcBurning != -1)
            {
                AudioManager.RemoveSourceQueue(_srcBurning);
                AudioManager.PlayPoolSourceThenFree(GameClient.instance.leafAudioPoolId, _srcBurning, Constants.LeafPutoff);
                _srcBurning = -1;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            if (_srcBurning != -1)
            {
                AudioManager.RemoveSourceQueue(_srcBurning);
                AudioManager.PlayPoolSourceThenFree(GameClient.instance.leafAudioPoolId, _srcBurning, Constants.LeafBurnup);
                _srcBurning = -1;
            }
        }
    }
}
