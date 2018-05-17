using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    class LeafClient : NetworkedGameObjectClient
    {
        private static ParticleSystem Fire;
        public LeafClient(CreateObjectPacket createPacket) : 
            base(createPacket, FileManager.LeafModel)
        {
            if (Fire == null)
            {
                Fire = new FlameThrowerParticleSystem();
                //Fire.Transform.Rotation.Y = 90f.ToRadians();
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Fire.Transform.Position = Transform.Position;
            Fire.EnableGeneration(true);
        }

        public void UpdateFromPacket(LeafPacket packet)
        {
            Transform.Position.X = packet.MovementX;
            Transform.Position.Z = packet.MovementZ;
            Transform.Rotation.Y = packet.Rotation;
        }

        public override void UpdateFromPacket(Packet packet)
        {
            UpdateFromPacket(packet as LeafPacket);
        }
    }
}
