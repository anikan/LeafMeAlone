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
        public static byte[] GetHeader(Packet packet, PacketType type)
        {
            byte[] header;
            header = new byte[] {(byte) type};

            return header;
        }

        /// <summary>
        /// Strips the header of a packet
        /// </summary>
        /// <param name="data">The byte array of the packet</param>
        /// <returns>The byte[] without the header.</returns>
        public static byte[] RemoveHeader(byte[] data)
        {
            byte[] resizedBuffer = new byte[data.Length - 1];
            Buffer.BlockCopy(data, 1, resizedBuffer, 0, data.Length - 1);

            return resizedBuffer;
        }
    }
}
