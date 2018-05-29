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
    public class MatchStartPacket : BasePacket
    {
        // ID of the object this packet is associated with.
        [ProtoMember(1)]
        public float gameTime;

        public MatchStartPacket() : base(PacketType.MatchStartPacket) { }
        public MatchStartPacket(float matchTime) : base(PacketType.MatchStartPacket)
        {
            gameTime = matchTime;
        }
    }
}
