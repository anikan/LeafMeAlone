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
        public static ParticleSystem Fire;
        private bool updateFire = false;
        public LeafClient(CreateObjectPacket createPacket) :
            base(createPacket, Constants.LeafModel)
        {
            if (Fire == null)
            {
                Fire = new FlameThrowerParticleSystem(2, 20,2.5f);
                Fire.emissionRate = 1;
                Fire.EnableGeneration(true);
                updateFire = true;
                Fire.Transform.Rotation.Y = 90f.ToRadians();
            }
        }

        public override void Draw()
        {
            base.Draw();
            if(Burning)
                Fire.DrawMe(Transform);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Fire.Enabled = true;
            Fire.SetOrigin(Vector3.Zero);
            if (updateFire)
                Fire.Update(deltaTime);
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
