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

namespace Shared.Packet
{
    /// <summary>
    /// Packet which is used for creating new objects
    /// </summary>
    [ProtoContract]
    public class CreateObjectPacket : BasePacket
    {

        [ProtoMember(1)]
        public ObjectPacket ObjData;

        [ProtoMember(2)]
        public ObjectType ObjectType;

        public CreateObjectPacket() : base(PacketType.CreateObjectPacket)
        {

        }

        public CreateObjectPacket(ObjectPacket objData, ObjectType objectType) : base(PacketType.CreateObjectPacket)
        {
            ObjData = objData;
            ObjectType = objectType;
        }
    }
}
