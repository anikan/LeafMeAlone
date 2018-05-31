using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Shared.Packet
{

    /// <summary>
    /// Packet to send or receive from the server
    /// </summary>
    [ProtoContract]
    public class IdPacket : BasePacket
    {
        // ID of the object this packet is associated with.
        [ProtoMember(1)]
        public int ObjectId;

        public IdPacket() : base(PacketType.IdPacket) { }
        public IdPacket(int objectId) : base(PacketType.IdPacket)
        {
            ObjectId = objectId;
        }

        public static bool operator ==(IdPacket thisPacket, IdPacket otherPacket)
        {
            return thisPacket.ObjectId == otherPacket.ObjectId;
        }

        public static bool operator !=(IdPacket thisPacket, IdPacket otherPacket)
        {
            return !(thisPacket == otherPacket);
        }

    }
}
