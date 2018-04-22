using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

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

        public void UpdateFromPacket(Packet packet)
        {
            throw new NotImplementedException();
        }
    }
}
