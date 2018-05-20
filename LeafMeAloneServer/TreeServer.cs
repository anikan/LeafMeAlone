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
    /// Tree on the server, mainly used for calculating physics.
    /// </summary>
    public class TreeServer : ColliderObject
    {
        // Constants for the tree.
        public const float TREE_RADIUS = 3.0f;
        public const float TREE_HEALTH = 10000.0f;

        /// <summary>
        /// Create a new tree on the server, and register it as an object.
        /// </summary>
        public TreeServer() : base(ObjectType.TREE, TREE_HEALTH, TREE_RADIUS)
        {
            Register();
        }

        /// <summary>
        /// Destroy the tree.
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
        }

        /// <summary>
        /// Have the tree react to being hit by a tool.
        /// </summary>
        /// <param name="playerPosition">Position of the player hitting this object.</param>
        /// <param name="toolType">Type of tool hit by.</param>
        /// <param name="toolMode">Mode the tool was in.</param>
        public override void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode)
        {
            base.HitByTool(playerPosition, toolType, toolMode);
        }

        /// <summary>
        /// Update the tree.
        /// </summary>
        /// <param name="deltaTime">Time since last frame.</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}
