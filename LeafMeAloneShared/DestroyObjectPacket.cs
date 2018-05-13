using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Packet that gets sent to the client when an object is to be destroyed
    /// </summary>
    [ProtoContract]
    public class DestroyObjectPacket : Packet
    {
        [ProtoMember(1)]
        public int _ProtoObjId
        {
            get { return ObjectId; }
            set { ObjectId = value; }
        }

        [ProtoMember(2)]
        public ObjectType objectType;

        public DestroyObjectPacket() : base(PacketType.DestroyObjectPacket)
        { }

        public DestroyObjectPacket(GameObject gameObject) :
            base(PacketType.CreateObjectPacket)
        {
            ObjectId = gameObject.Id;
            objectType = gameObject.ObjectType;
        }
    }
}
