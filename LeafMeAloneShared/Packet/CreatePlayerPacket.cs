using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{

    [ProtoContract]
    public class CreatePlayerPacket : BasePacket
    {
        [ProtoMember(1)]
        public CreateObjectPacket createPacket;

        [ProtoMember(2)]
        public Team team;

        public CreatePlayerPacket() : base(PacketType.CreatePlayerPacket) { }
        public CreatePlayerPacket(CreateObjectPacket createPacket, Team team) : base(PacketType.CreatePlayerPacket)
        {
            this.createPacket = createPacket;
            this.team = team;
        }
    }
}
