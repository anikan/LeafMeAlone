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
        public int _ProtoObjId
        {
            get { return ObjectId; }
            set { ObjectId = value; }
        }

        [ProtoMember(2)]
        public float InitialX;

        [ProtoMember(3)]
        public float InitialY;

        [ProtoMember(4)]
        public ObjectType objectType;

        public CreateObjectPacket() : base(PacketType.CreateObjectPacket)
        { }

        public CreateObjectPacket(GameObject gameObject) :
            base(PacketType.CreateObjectPacket)
        {
            ObjectId = gameObject.Id;
            InitialX = gameObject.Transform.Position.X;
            InitialY = gameObject.Transform.Position.Y;
            objectType = gameObject.ObjectType;
        }
    }
}
