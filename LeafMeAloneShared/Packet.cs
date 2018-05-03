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
        public int ObjectID;

        /// <summary>
        /// Creates the packet
        /// </summary>
        /// <param name="id"></param>
        public Packet()
        {

        }

        /// <summary>
        /// Returns the header of the packet.
        /// </summary>
        /// <returns>The packet header</returns>
        public static byte[] PrependHeader(byte[] data, PacketType type)
        {
            byte[] header;
            header = new byte[]{ (byte) type};
            byte[] packetSize = BitConverter.GetBytes(data.Length);

            header = header.Concat(packetSize).ToArray();

            data = data.Concat(header).ToArray();

            return data;
        }

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
        public static byte[] RemoveHeader( byte[] data )
        {
            int packetSize = BitConverter.ToInt32(data, 1);
            byte[] resizedBuffer = new byte[packetSize];
            Buffer.BlockCopy(data, 5, resizedBuffer, 0, packetSize);
            return data.Skip(5 + packetSize).ToArray();
        }
    }
}
