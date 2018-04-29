using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

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
    }
}
