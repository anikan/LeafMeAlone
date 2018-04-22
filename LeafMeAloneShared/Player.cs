using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Class for an actual player of the game.
    /// </summary>
    public abstract class Player : GameObject
    {
        /// <summary>
        /// Whether the player is using a tool
        /// </summary>
        public bool UsingTool;

        /// <summary>
        /// Whether the player is Dead or not
        /// </summary>
        public bool Dead;

        /// <summary>
        /// Currently Equipped Tool
        /// </summary>
        public PlayerPacket.ToolType ToolEquipped;


        /// <summary>
        /// Initialize the player with an initial position.
        /// </summary>
        public Player ()
        {

        }

        public abstract void UpdateFromPacket(Packet packet);

    }
}
