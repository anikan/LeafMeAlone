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
    class PacketFactory
    {
        /// <summary>
        /// Creates a network packet used to update the state of the player in the client
        /// </summary>
        /// <param name="player">The player object to serialize into a player</param>
        public static PlayerPacket CreatePacket(PlayerServer player)
        {
            PlayerPacket packet = new PlayerPacket()
            {
                Dead = player.Dead,
                Movement = player.transform.Get2dPosition(),
                ObjectID = player.Id,
                Rotation = player.transform.Direction.Y,
                ToolEquipped = player.ToolEquipped,
                UsingTool = player.UsingTool
            };

            return packet;
        }

    }
}
