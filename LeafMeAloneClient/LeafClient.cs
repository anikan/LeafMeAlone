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
                tint.X = 1.0f/(Health/5.0f);
//                tint.X += .1f;
                SetTint(tint);
           // }
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
