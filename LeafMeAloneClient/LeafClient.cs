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
        public LeafClient(CreateObjectPacket createPacket) :
            base(createPacket, Constants.LeafModel)
        {}
     

        /// <summary>
        /// Update from a server packet.
        /// </summary>
        /// <param name="packet">Packet from the server.</param>
        public override void UpdateFromPacket(BasePacket packet)
        {
            ObjectPacket objPacket = packet as ObjectPacket;
            // Set the initial positions of the object.

            Transform.Position.X = objPacket.PositionX;
            Transform.Position.Z = objPacket.PositionZ;
            Transform.Rotation.Y = objPacket.Rotation;
            // Set the initial burning status.
            Burning = objPacket.Burning;

            var oldHealth = Health;
            Health = objPacket.Health;

            //If the player is burning then change the leaf color.
            if (Burning)
            {
                //change of health
                var deltaHealth = oldHealth-Health;
                var maxHealth = 5.0f;
                CurrentTint -= new Vector3((deltaHealth / maxHealth), (deltaHealth / maxHealth), (deltaHealth / maxHealth));

            }

        }
    }
}
