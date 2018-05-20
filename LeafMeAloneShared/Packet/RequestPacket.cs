using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [ProtoContract]
    public class RequestPacket : Packet, IIdentifiable
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
    }
}
