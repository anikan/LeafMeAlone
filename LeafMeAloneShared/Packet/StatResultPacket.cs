using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    [ProtoContract]
    public class StatResultPacket : BasePacket
    {
        [ProtoMember(1)]
        PlayerStats stats;

        public StatResultPacket() : base(PacketType.StatResultPacket) { }
        public StatResultPacket(PlayerStats stats) : base(PacketType.StatResultPacket)
        {
            this.stats = stats;
        }
    }
}
