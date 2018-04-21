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
        /// Initialize the player with an initial position.
        /// </summary>
        public Player ()
        {

        }

        public abstract void UpdateFromPacket(Packet packet);

    }
}
