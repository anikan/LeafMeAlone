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
        // Constants for leafs. 
        public const float LEAF_HEALTH = 10.0f;
        public const float LEAF_MASS = 0.1f;
        public const float LEAF_RADIUS = 0.0f;
        public const float LEAF_BOUNCIENESS = 3.0f;

        /// <summary>
        /// Create a new leaf on the server.
        /// </summary>
        public LeafServer() : base(ObjectType.LEAF, LEAF_HEALTH, LEAF_MASS, LEAF_RADIUS, LEAF_BOUNCIENESS)
        {
            Burnable = true;
        }

    }
}