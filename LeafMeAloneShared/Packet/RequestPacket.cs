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

        public static bool operator ==(RequestPacket thisPacket, RequestPacket otherPacket)
        {
            return (thisPacket.DeltaX == otherPacket.DeltaX) && (thisPacket.DeltaZ == otherPacket.DeltaZ) &&
                   (thisPacket.DeltaRot == otherPacket.DeltaRot) && (thisPacket.IdData == otherPacket.IdData) &&
                   (thisPacket.ToolRequest == otherPacket.ToolRequest) && (thisPacket.ToolMode == otherPacket.ToolMode);
        }

        public static bool operator !=(RequestPacket thisPacket, RequestPacket otherPacket)
        {
            return !(thisPacket == otherPacket);
        }

    }
}
