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
        public LeafServer() : base(ObjectType.LEAF, Constants.LEAF_HEALTH, Constants.LEAF_MASS, Constants.LEAF_RADIUS, Constants.LEAF_BOUNCIENESS)
        {
            Burnable = true;
        }

        public override void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode)
        {
            base.HitByTool(playerPosition, toolType, toolMode);


            if (toolType == ToolType.BLOWER)
            {


                Transform.Rotation.Y += Utility.RandomInRange(-0.1f, 0.1f);


            }
        }
    }
}