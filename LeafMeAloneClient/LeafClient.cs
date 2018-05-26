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
    /// A leaf on the client, mainly for rendering.
    /// </summary>
    class LeafClient : NetworkedGameObjectClient
    {
        public LeafClient(CreateObjectPacket createPacket) :
            base(createPacket, Constants.LeafModel)
        {

        }

        private Vector3 tint = new Vector3(0, 1, 1);
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);


            //todo fix based on health
            //// if (Health > 0)
            // {
            tint.X = 1.0f / (Health / 5.0f);
            //                tint.X += .1f;
            SetTint(tint);
            // }
        }

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
            Health = objPacket.Health;
        }
    }
}
