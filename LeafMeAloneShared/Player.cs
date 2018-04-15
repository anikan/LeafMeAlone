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
    public class Player
    {
        // Movement speed.
        public const float SPEED = 1.0f;

        // Position of the player.
        private Vector3 Position;

        // Different possible movement directions.
        public enum MoveDirection
        {
            NORTH,
            EAST,
            SOUTH,
            WEST
        };

        /// <summary>
        /// Initialize the player with an initial position.
        /// </summary>
        /// <param name="initialPosition"></param>
        public Player (Vector3 initialPosition)
        {
            Position = initialPosition;
        }

        /// <summary>
        /// Moves the player in a specified direction (NESW)
        /// </summary>
        /// <param name="dir"></param>
        public void Move(MoveDirection dir)
        {

            // Delta to determine where the player should move
            Vector3 delta = Vector3.Zero;

            // Check each direction and set the delta base on that direction.
            switch (dir)
            {
                case MoveDirection.NORTH:
                    delta = new Vector3(0, 1, 0);
                    break;
                case MoveDirection.EAST:
                    delta = new Vector3(1, 0, 0);
                    break;
                case MoveDirection.SOUTH:
                    delta = new Vector3(0, -1, 0);
                    break;
                case MoveDirection.WEST:
                    delta = new Vector3(-1, 0, 0);
                    break;
            }

            // Add in the delta to the player's position
            Position = Vector3.Add(delta, Position);
        }

        /// <summary>
        /// Set the absolute position of the player
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector3 pos)
        {
            // Set the position of the player directly.
            Position = pos;
        }
    }
}
