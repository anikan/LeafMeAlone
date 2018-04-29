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
    public class CreateObjectPacket : Packet
    {
        public enum ObjectType
        {
            ACTIVE_PLAYER,
            PLAYER,
            LEAF
        };

        [ProtoMember(1)]
        public long Id;

        [ProtoMember(2)]
        public float InitialX;

        [ProtoMember(3)]
        public float InitialY;

        [ProtoMember(4)]
        public ObjectType objectType;

        public CreateObjectPacket(GameObject gameObject, ObjectType type)
        {
            Id = gameObject.Id;
            InitialX = gameObject.Transform.Position.X;
            InitialY = gameObject.Transform.Position.Y;
            objectType = type;
        }

        /// <summary>
        /// Serializes the packet object into an array of bytes
        /// </summary>
        /// <returns>the serialized packet</returns>
        public static byte[] Serialize(PlayerPacket packet)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, packet);
            return ms.ToArray();
        }

        /// <summary>
        /// Deserializes a byte array into a player packet object.
        /// </summary>
        /// <param name="data">The byte array of the player packet</param>
        /// <returns>The deserialized playerpacket</returns>
        public static PlayerPacket Deserialize(byte[] data)
        {
            return Serializer.Deserialize<PlayerPacket>(new MemoryStream(data));
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
