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
        // Small offset for floating point errors
        public const float FLOAT_RANGE = 0.01f;

        // Direction of movement the player is requesting. Should be between -1 and 1 each axis.
        public Vector2 MovementRequested;

        private PlayerPacket.ToolType toolEquipped;

        // Requests for the use of primary/secondary features of tools.
        public bool UseToolPrimaryRequest;
        public bool UseToolSecondaryRequest;

        public bool Dead { get; set; }
        public PlayerPacket.ToolType ToolEquipped { get; set; }
        public bool usingToolPrimary { get; set; }
        public bool usingToolSecondary { get; set; }

        public PlayerClient() : base()
        {
            SetModel(@"../../Models/Version1.fbx");
            Transform.Rotation.Y += 180f.ToRadians();
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
        /// Request the use of the primary function of the equipped tool.
        /// </summary>
        public void RequestUsePrimary()
        {
            // Set request bool to true.
            UseToolPrimaryRequest = true;

        }

        /// <summary>
        /// Request the use of the secondary function of the equipped tool.
        /// </summary>
        public void RequestUseSecondary()
        {
            // Set request bool to true.
            UseToolSecondaryRequest = true;
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
        public void ResetRequests()
        {
            MovementRequested = Vector2.Zero;
            UseToolPrimaryRequest = false;
            UseToolSecondaryRequest = false;
        }

        /// <summary>
        /// Updates the player's values based on a received packet.
        /// </summary>
        /// <param name="packet">The packet to update from.</param>
        public void UpdateFromPacket(PlayerPacket packet)
        {
            Dead = packet.Dead;
            toolEquipped = packet.ToolEquipped;
            usingToolPrimary = packet.UsingToolPrimary;
            usingToolSecondary = packet.UsingToolSecondary;
            Transform.Position.X = packet.MovementX;
            Transform.Position.Y = packet.MovementY;
            Transform.Rotation.Y = packet.Rotation;
        }
    }
}
