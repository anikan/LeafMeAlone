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


        /// <summary>
        /// Create a new tree on the server, and register it as an object.
        /// </summary>
        public TreeServer() : base(ObjectType.TREE, Constants.TREE_HEALTH)
        {
            colliderType = ColliderType.BOX;
            Radius = Constants.TREE_RADIUS;
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
        /// <param name="toolTransform">Position of the player hitting this object.</param>
        /// <param name="toolType">Type of tool hit by.</param>
        /// <param name="toolMode">Mode the tool was in.</param>
        public override void HitByTool(Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {
            base.HitByTool(toolTransform, toolType, toolMode);
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
