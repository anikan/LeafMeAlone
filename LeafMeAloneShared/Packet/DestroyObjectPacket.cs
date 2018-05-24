using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    /// <summary>
    /// Packet that gets sent to the client when an object is to be destroyed
    /// </summary>
    [ProtoContract]
    public class DestroyObjectPacket : BasePacket, IIdentifiable
    {
        [ProtoMember(1)]
        public IdPacket idData;

        public DestroyObjectPacket() : base (PacketType.DestroyObjectPacket) { }
        public DestroyObjectPacket(IdPacket idData) : base (PacketType.DestroyObjectPacket)
        {
            this.idData = idData;
        }

        public int GetId()
        {
            return idData.ObjectId;
        }
    }
}
