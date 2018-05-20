using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Shared
{

    /// <summary>
    /// Packet to send or receive from the server
    /// </summary>
    [ProtoContract]
    public class IdPacket : Packet
    {
        // ID of the object this packet is associated with.
        [ProtoMember(1)]
        public int ObjectId;

        public IdPacket(int objectId) : base(PacketType.IdPacket)
        {
            ObjectId = objectId;
        }
    }
}
