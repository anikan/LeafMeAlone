using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Packet that gets sent to the client when an object is to be destroyed
    /// </summary>
    [ProtoContract]
    public class DestroyObjectPacket : Packet
    {
        [ProtoMember(1)]
        public IdPacket idData;

        public DestroyObjectPacket(IdPacket idData) : base (PacketType.DestroyObjectPacket)
        {
            this.idData = idData;
        }
    }
}
