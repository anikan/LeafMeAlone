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
        public bool updateFire = false;
        public LeafClient(CreateObjectPacket createPacket) :
            base(createPacket, FileManager.LeafModel)
        {
            if (Fire == null)
            {
                Fire = new FlameThrowerParticleSystem(2, 20,2.5f);
                Fire.emissionRate = 1;
                Fire.EnableGeneration(true);
                Console.Write("New Fire!");
                //GraphicsManager.ParticleSystems.Add(Fire);
                updateFire = true;
                // OnDestroy += () => Fire.Destroy();
                //Fire.Transform.Rotation.Y = 90f.ToRadians();
            }
        }

        public override void Draw()
        {
            base.Draw();
            Fire.DrawMe(Transform);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Fire.Enabled = true;
            Fire.SetOrigin(Vector3.Zero);//Transform.Position + Vector3.TransformCoordinate(FlameThrowerParticleSystem.PlayerToFlamethrowerOffset, Fire.Transform.AsMatrix()));
            //Fire.SetVelocity(Transform.Forward * FlameThrowerParticleSystem.FlameInitSpeed);
            //Fire.SetAcceleration(Transform.Forward * FlameThrowerParticleSystem.FlameAcceleration);
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
