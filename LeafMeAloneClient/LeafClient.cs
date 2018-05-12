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

        public const string LeafModelPath = @"../../Models/Leaf_V1.fbx";

        public LeafClient(CreateObjectPacket createPacket) : 
            base(createPacket, LeafModelPath)
        {
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
