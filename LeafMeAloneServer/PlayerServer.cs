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
        public PlayerPacket.ToolType ToolEquipped { get; set; }
        public bool UsingToolPrimary { get; set; }
        public bool UsingToolSecondary { get; set; }

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

        public void UpdateFromPacket(PlayerPacket packet)
        {
            Transform.Position += new Vector3(packet.MovementX, packet.MovementY, 0.0f) * GameServer.TICK_TIME_S;

            Transform.Rotation.Y = packet.Rotation;
        }
    }
}
