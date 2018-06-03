using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    [ProtoContract]
    public class MatchResultPacket : BasePacket
    {
        [ProtoMember(1)]
        public TeamName winningTeam;

        public MatchResultPacket() : base(PacketType.GameResultPacket) { }
        public MatchResultPacket(TeamName winningTeam) : base(PacketType.GameResultPacket)
        {
            this.winningTeam = winningTeam;
        }
    }
}
