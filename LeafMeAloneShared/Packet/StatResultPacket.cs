﻿using ProtoBuf;
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
        public PlayerStats stats;

        [ProtoMember(2)]
        public int PlayerID;

        public StatResultPacket() : base(PacketType.StatResultPacket) { }
        public StatResultPacket(PlayerStats stats, int ID) : base(PacketType.StatResultPacket)
        {
            this.stats = stats;
            this.PlayerID = ID;
        }
    }
}
