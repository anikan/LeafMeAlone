using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [ProtoContract]
    class LeafPacket : Packet
    {
        [ProtoMember(1)]
        public float MovementX;

        [ProtoMember(2)]
        public float MovementY;

        [ProtoMember(3)]
        public float Rotation;
    }
}
