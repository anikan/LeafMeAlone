using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [ProtoContract]
    public class ObjectPacket : Packet
    {
        [ProtoMember(1)]
        public float PositionX;
        [ProtoMember(2)]
        public float PositionY;
        [ProtoMember(3)]
        public float PositionZ;
        [ProtoMember(4)]
        public float Rotation;
        [ProtoMember(5)]
        public bool Burning;
        [ProtoMember(6)]
        public IdPacket IdData;

        public ObjectPacket(float positionX, float positionY, float positionZ, 
            float rotation, bool burning, IdPacket idData) : base (PacketType.ObjectPacket)
        {
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            Rotation = rotation;
            Burning = burning;
            IdData = idData;
        }
    }
}
