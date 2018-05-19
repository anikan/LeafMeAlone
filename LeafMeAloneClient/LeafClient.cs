using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    class LeafClient : NetworkedGameObjectClient
    {
        public static ParticleSystem Fire;
        private bool updateFire = false;
        public LeafClient(CreateObjectPacket createPacket) :
            base(createPacket, FileManager.LeafModel)
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

        public void UpdateFromPacket(LeafPacket packet)
        {
            Transform.Position.X = packet.MovementX;
            Transform.Position.Z = packet.MovementZ;
            Transform.Rotation.Y = packet.Rotation;
            //Fire.SetOrigin(Transform.Position + Vector3.TransformCoordinate(FlameThrowerParticleSystem.PlayerToFlamethrowerOffset, Fire.Transform.AsMatrix()));
        }

        public override void UpdateFromPacket(Packet packet)
        {
            UpdateFromPacket(packet as LeafPacket);
        }
    }
}
