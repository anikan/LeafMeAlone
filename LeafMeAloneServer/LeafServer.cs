using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    class LeafServer : PhysicsObject, ILeaf
    {
        public bool Burning { get; set; }
        public float TimeBurning { get; set; }

        public LeafServer() : base(ObjectType.LEAF)
        {

        }

        public LeafServer(Transform startTransform) : base (ObjectType.LEAF, startTransform)
        {

        }

        /// <summary>
        /// Gets the distance of this leaf to the player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>The distance from the leaf to the player</returns>
        public float GetDistanceToPlayer(PlayerServer player)
        {

            // Get the vector between the two.
            Vector3 VectorBetween = Transform.Position - player.Transform.Position;

            // Calculate the length.
            return VectorBetween.Length();

        }

        /// <summary>
        /// Checks if this leaf is is within range of the player's tool, and should be affected.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>True if within range, false if not.</returns>
        public bool IsInPlayerToolRange(PlayerServer player)
        {
            
            // Get the player's equipped tool.
            ToolInfo equippedToolInfo = Tool.GetToolInfo(player.ToolEquipped);

            // Check if the leaf is within range of the player.
            if (GetDistanceToPlayer(player) <= equippedToolInfo.Range)
            {

                // Get the forward vector of the player.
                Vector3 PlayerForward = player.Transform.Forward;
                PlayerForward.Y = 0.0f;

                // Get the vector from the player to the leaf.
                Vector3 PlayerToLeaf = Transform.Position - player.Transform.Position;
                PlayerToLeaf.Y = 0.0f;

                // Get dot product of the two vectors and the product of their magnitudes.
                float dot = Vector3.Dot(PlayerForward, PlayerToLeaf);
                float mag = PlayerForward.Length() * PlayerToLeaf.Length();

                // Calculate the angle between the two vectors.
                float angleBetween = (float)Math.Acos(dot/mag);

                // Return true if the leaf is within the cone angle, false otherwise. 
                return (angleBetween <= (equippedToolInfo.ConeAngle / 2.0f));

            }

            return false;

        }
    }
}
