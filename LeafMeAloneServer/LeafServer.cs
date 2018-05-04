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
    public class LeafServer : PhysicsObject, ILeaf
    {
        public bool Burning { get; set; }
        public float TimeBurning { get; set; }

        public LeafServer() : base(ObjectType.LEAF)
        {

        }

        public LeafServer(Transform startTransform) : base (ObjectType.LEAF, startTransform)
        {

        }

        public LeafServer(Transform startTransform, float mass) : base(ObjectType.LEAF, startTransform, mass)
        {


        }



        public override void HitByTool(ToolType toolType, ToolMode toolMode)
        {
            // TODO
        }
    }
}