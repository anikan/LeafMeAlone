using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    [ProtoContract]
    public class GameResultPacket : BasePacket
    {
        [ProtoMember(1)]
        public TeamName winningTeam;

        public GameResultPacket() : base(PacketType.GameResultPacket) { }
        public GameResultPacket(TeamName winningTeam) : base(PacketType.GameResultPacket)
        {
            this.winningTeam = winningTeam;
        }
    }
}
