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
    public class ServerPacketFactory
    {
        /// <summary>
        /// Creates a network packet used to update the state of the player in the client
        /// </summary>
        /// <param name="player">The player object to serialize into a player</param>
        public static PlayerPacket CreatePacket(IPlayer player)
        {
            PlayerPacket packet = new PlayerPacket()
            {
                Dead = player.Dead,
                MovementX = player.GetTransform().Get2dPosition().X,
                MovementY = player.GetTransform().Get2dPosition().Y,
                ObjectID = player.Id,
                Rotation = player.GetTransform().Direction.Y,
                ToolEquipped = player.ToolEquipped,
                UsingTool = player.UsingTool
            };

            return packet;
        }

    }
}
