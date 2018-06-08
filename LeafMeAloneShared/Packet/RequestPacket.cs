using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    [ProtoContract]
    public class RequestPacket : BasePacket, IIdentifiable
    {
        [ProtoMember(1)]
        public float DeltaX;
        [ProtoMember(2)]
        public float DeltaZ;
        [ProtoMember(3)]
        public float DeltaRot;
        [ProtoMember(4)]
        public IdPacket IdData;
        [ProtoMember(5)]
        public ToolType ToolRequest;
        [ProtoMember(6)]
        public ToolMode ToolMode;

        public RequestPacket() : base(PacketType.RequestPacket) { }
        public RequestPacket(float deltaX, float deltaZ, float deltaRot, IdPacket idData, 
            ToolType toolReq, ToolMode toolMode) : base(PacketType.RequestPacket)
        {
            DeltaX = deltaX;
            DeltaZ = deltaZ;
            DeltaRot = deltaRot;
            IdData = idData;
            ToolRequest = toolReq;
            ToolMode = toolMode;
        }

        public int GetId()
        {
            return IdData.ObjectId;
        }

        /// <summary>
        /// Compare equality between two packets.
        /// </summary>
        /// <param name="thisPacket"></param>
        /// <param name="otherPacket"></param>
        /// <returns>True if they encode the same data, false otherwise or if 1 is null</returns>
        public static bool equals(RequestPacket thisPacket, RequestPacket otherPacket)
        {
            if ((thisPacket == null) || (otherPacket == null))
            {
                return false;
            }

            return (thisPacket.DeltaX == otherPacket.DeltaX) && (thisPacket.DeltaZ == otherPacket.DeltaZ) &&
                   (thisPacket.DeltaRot == otherPacket.DeltaRot) && (IdPacket.equals(thisPacket.IdData, otherPacket.IdData)) &&
                   (thisPacket.ToolRequest == otherPacket.ToolRequest) && (thisPacket.ToolMode == otherPacket.ToolMode);
        }

    }
}
