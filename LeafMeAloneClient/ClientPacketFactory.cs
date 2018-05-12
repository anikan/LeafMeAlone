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
    public class ClientPacketFactory
    {
        /// <summary>
        /// Creates a network packet used to update the state of the player in the client
        /// </summary>
        /// <param name="player">The player object to serialize into a player</param>
        public static PlayerPacket CreatePacket(PlayerClient player)
        {
            PlayerPacket packet = new PlayerPacket()
            {
                Dead = player.Dead,
                MovementX = player.PlayerRequests.MovementRequested.X,
                MovementZ = player.PlayerRequests.MovementRequested.Y,
                _ProtoObjId = player.Id,
                ToolEquipped = player.PlayerRequests.EquipToolRequest,
                Rotation = player.PlayerRequests.RotationRequested,
                UsingToolPrimary = player.PlayerRequests.UseToolPrimaryRequest,
                UsingToolSecondary = player.PlayerRequests.UseToolSecondaryRequest
            };

            return packet;
        }

    }
}
