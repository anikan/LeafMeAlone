using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    [ProtoContract]
    public class ThePacketToEndAllPackets : BasePacket
    {
        [ProtoMember(1)]
        public Team winningTeam;

        public ThePacketToEndAllPackets() : base(PacketType.GameResultPacket) { }
        public ThePacketToEndAllPackets(Team winningTeam) : base(PacketType.GameResultPacket)
        {
            this.winningTeam = winningTeam;
        }
    }
}
