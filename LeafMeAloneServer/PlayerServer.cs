using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    public class PlayerServer : GameObjectServer, IPlayer
    {

        public bool Dead { get; set; }
        public ToolType ToolEquipped { get; set; }

        // If the user is using the primary function of their tool or secondary
        public ToolMode ActiveToolMode { get; set; }

        public PlayerServer() : base(ObjectType.PLAYER)
        { }

        public Transform GetTransform()
        {
            return Transform;
        }

        public void SetTransform(Transform value)
        {
            Transform = value;
        }

        public override void Update(float deltaTime)
        {

        }

        public void AffectObjectsInToolRange(List<GameObjectServer> allObjects)
        {

            for (int j = 0; j < allObjects.Count; j++)
            {

                GameObjectServer gameObject = allObjects[j];
                if (gameObject.IsInPlayerToolRange(this))
                {

                    gameObject.HitByTool(ToolEquipped, ActiveToolMode);

                }
            }
        }

        public void UpdateFromPacket(PlayerPacket packet)
        {
            Transform.Position += new Vector3(packet.MovementX, packet.MovementY, 0.0f) * GameServer.TICK_TIME_S;

            Transform.Rotation.Y = packet.Rotation;
        }

        public override void HitByTool(ToolType toolType, ToolMode ActiveToolMode)
        {
            // TODO
        }
    }
}
