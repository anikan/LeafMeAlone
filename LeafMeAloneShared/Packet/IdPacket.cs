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

        /// <summary>
        /// Compare equality between two packets.
        /// </summary>
        /// <param name="thisPacket"></param>
        /// <param name="otherPacket"></param>
        /// <returns>True if they encode the same data, false otherwise or if 1 is null</returns>
        public static bool equals(IdPacket thisPacket, IdPacket otherPacket)
        {
            if ((thisPacket == null) || (otherPacket == null))
            {
                return false;
            }

            return thisPacket.ObjectId == otherPacket.ObjectId;
        }
    }
}
