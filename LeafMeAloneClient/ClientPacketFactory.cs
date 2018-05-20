using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// Handles the generation of packets on the server
    /// </summary>
    public class ClientPacketFactory : PacketFactory
    {
        /// <summary>
        /// Creates a network packet used to update the state of the player in the client
        /// </summary>
        /// <param name="player">The player object to serialize into a player</param>
        public static RequestPacket CreateRequestPacket(PlayerClient player)
        {
            float deltaX = 
            return new RequestPacket(player.PlayerRequests.MovementRequested.X, 
                player.PlayerRequests.MovementRequested.Y, 
                player.PlayerRequests.RotationRequested, new IdPacket(player.Id));
        }

    }
}
