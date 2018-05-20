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

        public override void UpdateFromPacket(ObjectPacket packet )
        {
            // Set the initial positions of the object.
            Transform.Position.X = packet.PositionX;
            Transform.Position.Z = packet.PositionZ;
            Transform.Rotation.Y = packet.Rotation;
            // Set the initial burning status.
            Burning = packet.Burning;
        }
    }
}
