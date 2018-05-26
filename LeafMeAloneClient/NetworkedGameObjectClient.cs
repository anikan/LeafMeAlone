using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Packet;

namespace Client
{
    /// <summary>
    /// Client-side GameObject.
    /// </summary>
    public abstract class NetworkedGameObjectClient : GraphicGameObject
    {


        /// <summary>
        /// Constructs a new local GameObject with a model at the specified path and a specified position.
        /// </summary>
        /// <param name="createPacket">Initial packet for this object.</param>
        /// <param name="modelPath">Path to this gameobject's model.</param>
        public NetworkedGameObjectClient(CreateObjectPacket createPacket, string modelPath) : base(modelPath)
        {
            Id = createPacket.ObjData.IdData.ObjectId;
            Transform.Position.X = createPacket.ObjData.PositionX;
            Transform.Position.Y = createPacket.ObjData.PositionY;
            Transform.Position.Z = createPacket.ObjData.PositionZ;
        }

        /// <summary>
        /// Updates this object from a network packet.
        /// </summary>
        /// <param name="packet">Packet to update from.</param>
        public abstract void UpdateFromPacket(BasePacket packet);
    }
}
