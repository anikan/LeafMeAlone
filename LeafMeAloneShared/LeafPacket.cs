using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [ProtoContract]
    public class LeafPacket : Packet
    {
        [ProtoMember(1)]
        public int _ProtoObjId
        {
            get { return ObjectId; }
            set { ObjectId = value; }
        }

        [ProtoMember(2)]
        public float MovementX;

        [ProtoMember(3)]
        public float MovementZ;

        [ProtoMember(4)]
        public float Rotation;

        [ProtoMember(5)]
        public bool Burning;

        /// <summary>
        /// Creates a new packet for a leaf
        /// </summary>
        public LeafPacket()  : base(PacketType.LeafPacket)
        { }
    }
}
