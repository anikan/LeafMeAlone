using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;
using System.IO;

namespace Shared
{
    /// <summary>
    /// Packet of player information, to send or receive from the server.
    /// </summary>
    [ProtoContract]
    public class PlayerPacket : Packet
    {
        // Associated Object data of the playerpacket
        [ProtoMember(1)]
        public ObjectPacket ObjData;

        // If the player is actively using their tool this frame.
        [ProtoMember(2)]
        public ToolMode ActiveToolMode;

        // Currently equipped tool.
        [ProtoMember(3)]
        public ToolType ToolEquipped;

        // Is the player dead? RIP.
        [ProtoMember(4)]
        public bool Dead;

        public PlayerPacket(ObjectPacket objData, ToolMode activeToolMode, ToolType toolEquipped, bool dead) : base(PacketType.PlayerPacket)
        {
            ObjData = objData;
            ActiveToolMode = activeToolMode;
            ToolEquipped = toolEquipped;
            Dead = dead;
        }
    }
}
