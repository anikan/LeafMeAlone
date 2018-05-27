using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shared.Packet;

namespace Shared
{
    public enum PacketType : byte
    {
        CreateObjectPacket,
        PlayerPacket,
        DestroyObjectPacket,
        CreatePlayerPacket,
        ObjectPacket,
        IdPacket,
        RequestPacket,
        GameResultPacket
    }

    public static class PacketUtil
    {
        public static Dictionary<PacketType, Type> packetClassMap =
            new Dictionary<PacketType, Type>() {
            { PacketType.CreateObjectPacket, typeof(CreateObjectPacket) },
            { PacketType.PlayerPacket, typeof(PlayerPacket) },
            { PacketType.DestroyObjectPacket, typeof(DestroyObjectPacket) },
            { PacketType.CreatePlayerPacket, typeof(CreatePlayerPacket) },
            { PacketType.ObjectPacket, typeof(ObjectPacket) },
            { PacketType.IdPacket, typeof(IdPacket) },
            { PacketType.RequestPacket, typeof(RequestPacket) },
            { PacketType.GameResultPacket, typeof(ThePacketToEndAllPackets) },
        };

        public const int PACK_HEAD_SIZE = 5;
        /// <summary>
        /// Returns the header of the packet.
        /// </summary>
        /// <returns>The packet header</returns>
        public static byte[] PrependHeader(byte[] data, PacketType type)
        {
            byte[] header = new byte[] { (byte)type };
            byte[] packetSize = BitConverter.GetBytes(data.Length);
            return header.Concat(packetSize).Concat(data).ToArray();
        }

        /// <summary>
        /// Strips the header of a packet
        /// </summary>
        /// <param name="data">The byte array of the packet</param>
        /// <returns>The byte[] without the header.</returns>
        public static byte[] RemoveHeader(byte[] data)
        {
            //Check whether we can get the size.
            if (data.Length <= PACK_HEAD_SIZE)
            {
                return null;
            }

            byte[] sizePortion = new byte[4];
            Buffer.BlockCopy(data, 1, sizePortion, 0, 4);
            //if (!BitConverter.IsLittleEndian)
            //{
            //    sizePortion = sizePortion.Reverse().ToArray();
            //}
            int packetSize = BitConverter.ToInt32(sizePortion, 0);

            //Check whether the full packet has arrived.
            if (data.Length < packetSize + PACK_HEAD_SIZE)
            {
                return null;
            }
            byte[] resizedBuffer = new byte[packetSize];
            Buffer.BlockCopy(data, PACK_HEAD_SIZE, resizedBuffer, 0, packetSize);
            return resizedBuffer;
        }

        /// <summary>
        /// Deserializes the packet type from the given data and outputs the 
        /// size of the packet
        /// </summary>
        /// <param name="data">the input buffer that begins with the 
        /// packet info</param>
        /// <param name="bytesRead">the number of bytes read in total</param>
        /// <returns>the packet deserialized</returns>
        public static BasePacket Deserialize(byte[] data)
        {
            Byte[] objectData =
                RemoveHeader(data);

            //If we haven't received the full packet, objectData will be null, return null.
            if (objectData == null)
            {
                return null;
            }

            packetClassMap.TryGetValue((PacketType)data[0], out Type packetType);
            if (packetType == null)
            {
                throw new Exception("Please add the new packet type to the packet class map!");
            }
            return (BasePacket) Serializer.Deserialize(packetType, new MemoryStream(objectData));
        }

        /// <summary>
        /// Serializes the packet to a byte array
        /// </summary>
        /// <returns>the serialized object</returns>
        public static byte[] Serialize(BasePacket packet)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, packet);
            byte[] serializedObject = ms.ToArray();

            return PrependHeader(serializedObject, packet.packetType);
        }
    }
}
