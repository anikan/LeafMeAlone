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
            float deltaX = player.PlayerRequests.MovementRequested.X;
            float deltaZ = player.PlayerRequests.MovementRequested.Y;
            float deltaRot = player.PlayerRequests.RotationRequested;
            return new RequestPacket(
                deltaX, deltaZ, deltaRot, new IdPacket(player.Id), 
                player.PlayerRequests.EquipToolRequest,
                player.PlayerRequests.ActiveToolMode
                );
        }

    }
}
