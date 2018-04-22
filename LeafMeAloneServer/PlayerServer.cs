using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    public class PlayerServer : IPlayer
    {
        /// <summary>
        /// Data on the character's position, rotation ...
        /// </summary>
        public Transform transform;
        public bool Dead;
        public bool UsingTool;
        public PlayerPacket.ToolType ToolEquipped;

        public int Id { get; internal set; }

        bool IPlayer.GetUsingTool()
        {
            return UsingTool;
        }

        void IPlayer.SetUsingTool(bool value)
        {
            UsingTool = value;
        }

        bool IPlayer.GetDead()
        {
            return Dead;
        }

        void IPlayer.SetDead(bool value)
        {
            Dead = value;
        }

        PlayerPacket.ToolType IPlayer.GetToolEquipped()
        {
            return ToolEquipped;
        }

        public void UpdateFromPacket(PlayerPacket packet)
        {
            transform.Position += new Vector3(packet.Movement, 0.0f);

            transform.Direction.Y = packet.Rotation;
        }

        void IPlayer.SetToolEquipped(PlayerPacket.ToolType value)
        {
            ToolEquipped = value;
        }
    }
}
