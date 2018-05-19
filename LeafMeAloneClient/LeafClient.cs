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
        
        public LeafClient(CreateObjectPacket createPacket) : 
            base(createPacket, FileManager.LeafModel)
        {
        }

        public void UpdateFromPacket(ObjectPacket packet)
        {
            Transform.Position.X = packet.MovementX;
            Transform.Position.Z = packet.MovementZ;
            Transform.Rotation.Y = packet.Rotation;
            Burning = packet.Burning;
        }

        public override void UpdateFromPacket(Packet packet)
        {
            UpdateFromPacket(packet as ObjectPacket);
        }
    }
}
