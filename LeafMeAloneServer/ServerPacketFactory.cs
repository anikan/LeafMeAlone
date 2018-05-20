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

        internal static Packet CreateUpdatePacket(GameObjectServer serverObject)
        {
                if (serverObject is PlayerServer player)
                {
                    return CreatePlayerPacket(player);
                } 
                return NewObjectPacket(serverObject);
        }
        internal static PlayerPacket CreatePlayerPacket(PlayerServer player)
        {
            return new PlayerPacket(NewObjectPacket(player), player.ActiveToolMode, player.ToolEquipped,
                player.Dead);
        }
    }
}
