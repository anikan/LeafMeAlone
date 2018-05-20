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
        public ObjectPacket ObjData;

        [ProtoMember(2)]
        public ObjectType ObjectType;

        public CreateObjectPacket(ObjectPacket objData, ObjectType objectType) : base(PacketType.CreateObjectPacket)
        {
            ObjData = objData;
            ObjectType = objectType;
        }
    }
}
