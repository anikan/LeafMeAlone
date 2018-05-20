using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    /// <summary>
    /// Handles the generation of packets on the server
    /// </summary>
    public class ServerPacketFactory : PacketFactory
    {

        internal static PlayerPacket CreatePlayerPacket(PlayerServer player)
        {
            return new PlayerPacket(CreateObjectPacket(player), player.ActiveToolMode, player.ToolEquipped,
                player.Dead);
        }
    }
}
