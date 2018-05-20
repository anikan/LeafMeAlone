using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    /// <summary>
    /// A leaf on the client, mainly for rendering.
    /// </summary>
    class LeafClient : NetworkedGameObjectClient
    {
        
        /// <summary>
        /// Create a new leaf client, from a server packet.
        /// </summary>
        /// <param name="createPacket"></param>
        public LeafClient(CreateObjectPacket createPacket) : base(createPacket, Constants.LeafModel)
        {
        }

        public override void UpdateFromPacket(Packet packet )
        {
            ObjectPacket objPacket = packet as ObjectPacket;
            // Set the initial positions of the object.
            Transform.Position.X = objPacket.PositionX;
            Transform.Position.Z = objPacket.PositionZ;
            Transform.Rotation.Y = objPacket.Rotation;
            // Set the initial burning status.
            Burning = objPacket.Burning;
        }
    }
}
