using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;

namespace Client
{
    public class PlayerClient : GameObjectClient, IPlayer
    {

        public const float FLOAT_RANGE = 0.01f;

        public Vector2 MovementRequested;

        private PlayerPacket.ToolType toolEquipped;
        private Transform transform;
        private bool usingTool;
        private bool dead;

        public bool UsingTool { get => usingTool; set => usingTool = value; }
        Transform IPlayer.Transform { get => transform; set => transform = value; }
        public bool Dead { get => dead; set => dead = value; }
        public PlayerPacket.ToolType ToolEquipped { get => toolEquipped; set => toolEquipped = value; }

        public PlayerClient() : base()
        {
            SetModel(@"../../Models/Version1.fbx");
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

            // If the direction requested is non-zero in the X axis (account for floating point error).
            if (dir.X < 0.0f - FLOAT_RANGE || dir.X > 0.0f + FLOAT_RANGE)
            {
                // Request in the x direction.
                MovementRequested.X = dir.X;
            }

            // If the direction requested is non-zero in the Y axis (account for floating point error).
            if (dir.Y < 0.0f - FLOAT_RANGE || dir.Y > 0.0f + FLOAT_RANGE)
            {
                //// Request in the y direction.
                MovementRequested.Y = dir.Y;
            }

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

        /// <summary>
        /// Resets the player's requested movement.
        /// </summary>
        public void ResetRequestedMovement()
        {
            MovementRequested = Vector2.Zero;
        }

        public void UpdateFromPacket(PlayerPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}
