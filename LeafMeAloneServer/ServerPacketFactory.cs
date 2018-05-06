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
        public static Packet CreatePacket(GameObjectServer gameObj)
        {
            if (gameObj is PlayerServer player)
            {
                return new PlayerPacket()
                {
                    Dead = player.Dead,
                    MovementX = player.Transform.Get2dPosition().X,
                    MovementY = player.Transform.Get2dPosition().Y,
                    _ProtoObjId = player.Id,
                    Rotation = player.Transform.Rotation.Y,
                    ToolEquipped = player.ToolEquipped,
                    UsingToolPrimary =
                    player.ActiveToolMode == ToolMode.PRIMARY,
                    UsingToolSecondary =
                    player.ActiveToolMode == ToolMode.SECONDARY
                };
            }
            else if (gameObj is LeafServer leaf)
            {
                return new LeafPacket()
                {
                    MovementX = leaf.Transform.Get2dPosition().X,
                    MovementY = leaf.Transform.Get2dPosition().Y,
                    ObjectId = leaf.Id,
                    Rotation = leaf.Transform.Rotation.Y,
                };
            }
            return null;
        }

    }
}
