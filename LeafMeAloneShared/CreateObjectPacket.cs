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

        [ProtoMember(1)]
        public int Id;

        [ProtoMember(2)]
        public float InitialX;

        [ProtoMember(3)]
        public float InitialY;

        [ProtoMember(4)]
        public ObjectType objectType;

        public CreateObjectPacket(GameObject gameObject)
        {
            Id = gameObject.Id;
            InitialX = gameObject.Transform.Position.X;
            InitialY = gameObject.Transform.Position.Y;
            objectType = gameObject.ObjectType;
        }

        /// <summary>
        /// Serializes the packet object into an array of bytes
        /// </summary>
        /// <returns>the serialized packet</returns>
        public static byte[] Serialize(CreateObjectPacket packet)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, packet);

            // Add packetType as first byte, send over
            return (new byte[] { (byte)PacketType.CreateObjectPacket })
                            .Concat(ms.ToArray()).ToArray();
        }

        /// <summary>
        /// Deserializes a byte array into a player packet object.
        /// </summary>
        /// <param name="data">The byte array of the player packet</param>
        /// <returns>The deserialized playerpacket</returns>
        public static CreateObjectPacket Deserialize(byte[] data)
        {
            return Serializer.Deserialize<CreateObjectPacket>(new MemoryStream(data));
        }
    }
}
