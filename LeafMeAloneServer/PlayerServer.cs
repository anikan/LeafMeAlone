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

        public const float PLAYER_BURN_TIME = 10000.0f;

        public bool Dead { get; set; }
        public ToolType ToolEquipped { get; set; }

        // If the user is using the primary function of their tool or secondary
        public ToolMode ActiveToolMode { get; set; }

        public PlayerServer() : base(ObjectType.PLAYER, PLAYER_BURN_TIME)
        {

        }

        public override void Update(float deltaTime)
        {

        }

        public void AffectObjectsInToolRange(List<GameObjectServer> allObjects)
        {

            for (int j = 0; j < allObjects.Count; j++)
            {

                GameObjectServer gameObject = allObjects[j];
                if (gameObject != this && gameObject.IsInPlayerToolRange(this))
                {

                    gameObject.HitByTool(Transform.Position, ToolEquipped, ActiveToolMode);

                }
            }
        }

        public void UpdateFromPacket(PlayerPacket packet)
        {
            Transform.Position += new Vector3(packet.MovementX, 0.0f, packet.MovementZ) * GameServer.TICK_TIME_S;

            Transform.Rotation.Y = packet.Rotation;
        }

        public override void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode)
        {
            // TODO
        }
    }
}
