using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;

namespace Client
{
    public class PlayerClient : GameObjectClient, Player
    {

        public Vector2 MovementRequested;


        public PlayerClient() : base()
        {
            SetModel(@"../../Version1.fbx");
        }

        /// <summary>
        /// Updates the player's values based on a received packet.
        /// </summary>
        /// <param name="packet"></param>
        public void UpdateFromPacket(Packet packet)
        {

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
                MovementRequested.X = dir.X;
            }

            // If dir.Y is nonzero (range to account for floating point errors)
            if (dir.Y < -0.01f || dir.Y > 0.01f)
            {
                // Set the Y value of the movement packet
                MovementRequested.Y = dir.Y;
            }
        }

        /// <summary>
        /// Resets all transient state of the player object; e.g. the object's 
        /// requested movement.
        /// </summary>
        internal void ResetTransientState()
        {
            MovementRequested.X = 0;
            MovementRequested.Y = 0;
        }

        /// <summary>
        /// Set the absolute position of the player
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector2 pos)
        {
            // Set the position of the player directly.

            // Update Transform on GameObject

        }



    }



}
