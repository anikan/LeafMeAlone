using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    public class PlayerServer : Player
    {
        /// <summary>
        /// Data on the character's position, rotation ...
        /// </summary>
        public Transform transform;
        /// <summary>
        /// Whether the player is using a tool
        /// </summary>
        public bool UsingTool;

        /// <summary>
        /// Currently Equipped Tool
        /// </summary>
        public PlayerPacket.ToolType ToolEquipped;

        /// <summary>
        /// Whether the player is Dead or not
        /// </summary>
        public bool Dead;

        public void UpdateFromPacket(PlayerPacket packet)
        {
            transform.Position += new Vector3(packet.Movement, 0.0f);

            transform.Direction.Y = packet.Rotation;
        }
    }
}
