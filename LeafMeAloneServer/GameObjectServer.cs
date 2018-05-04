using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    public abstract class GameObjectServer : GameObject
    {

        public GameObjectServer(ObjectType objectType) : base()
        {
            ObjectType = objectType;
        }

        public GameObjectServer(ObjectType objectType, Transform startPosition) : base(startPosition)
        {
            ObjectType = objectType;
        }

        public abstract void HitByTool(ToolType toolType, ToolMode toolMode);

        /// <summary>
        /// Gets the distance of this object to the player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>The distance from the object to the player</returns>
        public float GetDistanceToPlayer(PlayerServer player)
        {

            // Get the vector between the two.
            Vector3 VectorBetween = Transform.Position - player.Transform.Position;

            // Calculate the length.
            return VectorBetween.Length();

        }

        /// <summary>
        /// Checks if this object is is within range of the player's tool, and should be affected.
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
                // TODO: Have an actual Transform.Forward
                Vector3 PlayerForward = player.Transform.Forward;
                PlayerForward.Y = 0.0f;

                // Get the vector from the player to the leaf.
                Vector3 PlayertoObject = Transform.Position - player.Transform.Position;
                PlayertoObject.Y = 0.0f;

                // Get dot product of the two vectors and the product of their magnitudes.
                float dot = Vector3.Dot(PlayerForward, PlayertoObject);
                float mag = PlayerForward.Length() * PlayertoObject.Length();

                // Calculate the angle between the two vectors.
                float angleBetween = (float)Math.Acos(dot / mag);

                // Return true if the leaf is within the cone angle, false otherwise. 
                return (angleBetween <= (equippedToolInfo.ConeAngle / 2.0f));

            }

            return false;

        }
    }
}
