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
        /// Creates a network packet used to update the state of the player 
        /// in the client
        /// </summary>
        /// <param name="player">The player object to serialize into a player
        /// </param>
        public static Packet CreatePacket(GameObjectServer gameObj)
        {
            if (gameObj is PlayerServer player)
            {
                return new PlayerPacket()
                {
                    Dead = player.Dead,
                    MovementX = player.Transform.Position.X,
                    MovementZ = player.Transform.Position.Z,
                    _ProtoObjId = player.Id,
                    Rotation = player.Transform.Rotation.Y,
                    ToolEquipped = player.ToolEquipped,
                    ActiveToolMode = player.ActiveToolMode
                };
            }
            else if (gameObj is GameObjectServer obj)
            {
                return new ObjectPacket()
                {
                    MovementX = obj.Transform.Position.X,
                    MovementZ = obj.Transform.Position.Z,
                    ObjectId = obj.Id,
                    Rotation = obj.Transform.Rotation.Y,
                    Burning = obj.Burning
                };
            }

            return null;
        }

        internal static Packet CreateDestroyPacket(GameObject gameObj)
        {
            return new DestroyObjectPacket(gameObj);
        }
    }
}
