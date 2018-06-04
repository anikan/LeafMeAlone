using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    [ProtoContract]
    public class SpectatorPacket : BasePacket
    {
        [ProtoMember(1)]
        int Useless = 133769420; // So that the packet does not deserialize to null
        public SpectatorPacket() : base(PacketType.SpectatorPacket) { }
    }
}
