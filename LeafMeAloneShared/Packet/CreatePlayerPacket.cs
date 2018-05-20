using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum Team
    {
        RED,
        BLUE
    };

    [ProtoContract]
    public class CreatePlayerPacket : Packet
    {
        [ProtoMember(1)]
        CreateObjectPacket createPacket;

        [ProtoMember(2)]
        Team team;

        public CreatePlayerPacket(CreateObjectPacket createPacket, Team team) : base(PacketType.CreatePlayerPacket)
        {
            this.createPacket = createPacket;
            this.team = team;
        }
    }
}
