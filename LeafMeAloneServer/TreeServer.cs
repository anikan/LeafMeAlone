using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    public class TreeServer : ColliderObject
    {

        public const float TREE_RADIUS = 1.0f;
        public const float TREE_HEALTH = 10000.0f;

        public TreeServer() : base(ObjectType.TREE, TREE_HEALTH, TREE_RADIUS)
        {
            Register();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode)
        {
            base.HitByTool(playerPosition, toolType, toolMode);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}
