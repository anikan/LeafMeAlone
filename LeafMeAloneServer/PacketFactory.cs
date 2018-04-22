using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

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
        public PlayerPacket CreatePacket(PlayerServer player)
        {
            PlayerPacket packet = new PlayerPacket();
            packet.Dead = player.Dead;

            return packet;

        }

    }
}
