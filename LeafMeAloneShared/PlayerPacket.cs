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

        // Movement info in the packet. 
        // Note: When sending the packet, this is just a direction.
        // When receiving, this will be an absolute position.
        [ProtoMember(1)]
        public float MovementX;

        /// <summary>
        /// Y movement info for the packet. Server sends absolute, client sends
        /// delta
        /// </summary>
        [ProtoMember(2)]
        public float MovementY;

        // Rotation of the player.
        [ProtoMember(3)]
        public float Rotation;

        // If the player is actively using their tool this frame.

        [ProtoMember(4)]
        public bool UsingToolPrimary;

        // If the player is using the secondary ability of their tool this frame.
        [ProtoMember(5)]
        public bool UsingToolSecondary;

        // Currently equipped tool.
        [ProtoMember(6)]
        public ToolType ToolEquipped;

        // Is the player dead? RIP.
        [ProtoMember(7)]
        public bool Dead;

        /// <summary>
        /// Initializes a player packet, calls base constructor.
        /// </summary>
        /// <param name="id"></param>
        public PlayerPacket()
        {
        }

        /// <summary>
        /// Serializes the packet object into an array of bytes
        /// </summary>
        /// <returns>the serialized packet</returns>
        public static byte[] Serialize(PlayerPacket packet)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, packet);
            byte[] serializedObject = ms.ToArray();

            serializedObject =
                PrependHeader(serializedObject, PacketType.PlayerPacket);

            return serializedObject;
        }

        public override string ToString()
        {

            string printString = string.Format(
                "Player packet info: Movement=({0}, {1}), Rotation={2}, UseToolPrimary={3}, UseToolSecondary={4}",
                MovementX, MovementY, Rotation, UsingToolPrimary, UsingToolSecondary);

            return printString;

        }
    }
}
