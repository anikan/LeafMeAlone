using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Shared
{
    public enum PacketType : byte
    {
        CreateObjectPacket,
        PlayerPacket,
        LeafPacket
    }

    /// <summary>
    /// Packet to send or receive from the server
    /// </summary>
    public abstract class Packet
    {
        public const int HEADER_SIZE = 5;

        // Was this packet changed?
        public bool Modified;

        // ID of the object this packet is associated with.
        public int ObjectId;

        public PacketType packetType;

        /// <summary>
        /// Creates the packet
        /// </summary>
        /// <param name="id"></param>
        public Packet(PacketType packetType)
        {
            this.packetType = packetType;
        }

        /// <summary>
        /// Returns the header of the packet.
        /// </summary>
        /// <returns>The packet header</returns>
        public static byte[] PrependHeader(byte[] data, PacketType type)
        {
            byte[] header = new byte[]{ (byte) type };
            byte[] packetSize = BitConverter.GetBytes(data.Length);
            return header.Concat(packetSize).Concat(data).ToArray();
        }

        /// <summary>
        /// Deserializes the packet type from the given data and outputs the 
        /// size of the packet
        /// </summary>
        /// <param name="data">the input buffer that begins with the 
        /// packet info</param>
        /// <param name="bytesRead">the number of bytes read in total</param>
        /// <returns>the packet deserialized</returns>
        public static Packet Deserialize(byte[] data, out int bytesRead)
        {
            Byte[] objectData =
                RemoveHeader( data );
            bytesRead = HEADER_SIZE + objectData.Length;
            switch ((PacketType) data[0])
            {
                case PacketType.CreateObjectPacket:
                    return Serializer.Deserialize<CreateObjectPacket>(
                        new MemoryStream(objectData)
                        );
                case PacketType.PlayerPacket:
                    return Serializer.Deserialize<PlayerPacket>(
                        new MemoryStream(objectData)
                        );
                case PacketType.LeafPacket:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Strips the header of a packet
        /// </summary>
        /// <param name="data">The byte array of the packet</param>
        /// <returns>The byte[] without the header.</returns>
        public static byte[] RemoveHeader( byte[] data)
        {
            byte[] sizePortion = new byte[4];
            Buffer.BlockCopy(data, 1, sizePortion, 0, 4);
            int packetSize = BitConverter.ToInt32(sizePortion, 0);
            byte[] resizedBuffer = new byte[packetSize];
            Buffer.BlockCopy(data, 5, resizedBuffer, 0, packetSize);
            return resizedBuffer;
        }

        /// <summary>
        /// Serializes the packet to a byte array
        /// </summary>
        /// <returns>the serialized object</returns>
        public byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, this);
            byte[] serializedObject = ms.ToArray();

            return PrependHeader( 
                serializedObject, packetType);
        }
    }
}
