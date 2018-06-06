using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    /// <summary>
    /// Leaf class on the game server. Is a physics object.
    /// </summary>
    public class LeafServer : PhysicsObject
    {
        /// <summary>
        /// Create a new leaf on the server.
        /// </summary>
        public LeafServer() : base(ObjectType.LEAF, Constants.LEAF_HEALTH, Constants.LEAF_MASS, Constants.LEAF_BOUNCIENESS)
        {
            Burnable = true;
            Radius = Constants.LEAF_RADIUS;
            colliderType = ColliderType.CIRCLE;
        }

        /// <summary>
        /// Leaf was hit by tool.
        /// </summary>
        /// <param name="toolTransform">Transform of the tool.</param>
        /// <param name="toolType">Type of tool equipped.</param>
        /// <param name="toolMode">Mode of current tool.</param>
        public override void HitByTool(PlayerServer player, Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {
            // Call the base hit by tool.
            base.HitByTool(player, toolTransform, toolType, toolMode);

            // If this is a leaf blower.
            if (toolType == ToolType.BLOWER)
            {

                // Get the forward of the tool.
                Vector3 toolForward = toolTransform.Forward;

                // Get the vector from the tool to the lefa.
                Vector3 toolToObj = Transform.Position - toolTransform.Position;

                // Get the angle between the two vectors.
                float angleBetween = Utility.AngleBetweenVectors(toolForward, toolToObj);

                // If the angle is negative, rotate the leaf left.
                if (angleBetween < 0.0f)
                {

                    RandomRotateLeft();
                }

                // If the angle is postiive, rotate the leaf right.
                else
                {
                    RandomRotateRight();
                }

            }
        }

        /// <summary>
        /// Randomly rotate the leaf to the left.
        /// </summary>
        public void RandomRotateLeft()
        {
            Transform.Rotation.Y += Utility.RandomInRange(0.0f, Constants.LEAF_ROTATE_SPEED);

        }

        /// <summary>
        /// Randomly rotate the leaf to the right.
        /// </summary>
        public void RandomRotateRight()
        {

            Transform.Rotation.Y -= Utility.RandomInRange(0.0f, Constants.LEAF_ROTATE_SPEED);
        }

        /// <summary>
        /// Removes the game object from the gameserver
        /// </summary>
        public override void Die()
        {

            base.Die();
            GameServer.instance.Destroy(this);
        }

    }
}