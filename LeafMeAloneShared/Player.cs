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
    public class Player : GameObject
    {
        // Movement speed.
        public const float SPEED = 1.0f;

        // Position of the player.
        private Vector2 Position;

        public PlayerPacket PacketToSend;

        /// <summary>
        /// Initialize the player with an initial position.
        /// </summary>
        /// <param name="initialPosition"></param>
        public Player (Vector2 initialPosition)
        {
            Position = initialPosition;
            PacketToSend = new PlayerPacket(Id);
        }

        /// <summary>
        /// Updates the player's values based on a received packet.
        /// </summary>
        /// <param name="packet"></param>
        public void UpdateFromPacket(PlayerPacket packet)
        {

            Position = packet.Movement;

        }

        /// <summary>
        /// Moves the player in a specified direction (NESW)
        /// </summary>
        /// <param name="dir"></param>
        public void RequestMove(Vector2 dir)
        {

            // If dir.X is nonzero (range to account for floating point errors)
            if (dir.X < -0.01f || dir.X > 0.01f)
            {
                // Set the X value of the movement packet
                PacketToSend.Movement.X = dir.X;
            }

            // If dir.Y is nonzero (range to account for floating point errors)
            if (dir.Y < -0.01f || dir.Y > 0.01f)
            {
                // Set the Y value of the movement packet
                PacketToSend.Movement.Y = dir.Y;
            }
        }

        /// <summary>
        /// Set the absolute position of the player
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector2 pos)
        {
            // Set the position of the player directly.
            Position = pos;
        }
    }
}
