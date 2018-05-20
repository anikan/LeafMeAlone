using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [ProtoContract]
    public class RequestPacket : Packet
    {
        [ProtoMember(1)]
        public float DeltaX;
        [ProtoMember(2)]
        public float DeltaZ;
        [ProtoMember(3)]
        public float DeltaRot;
        [ProtoMember(4)]
        public IdPacket IdData;

        public RequestPacket(float deltaX, float deltaZ, float deltaRot, IdPacket idData) : base(PacketType.RequestPacket)
        {
            DeltaX = deltaX;
            DeltaZ = deltaZ;
            DeltaRot = deltaRot;
            IdData = idData;
        }
    }
}
