using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

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
            Id = createPacket.ObjectId;
            Transform.Position.X = createPacket.InitialX;
            Transform.Position.Y = createPacket.InitialY;
        }

        /// <summary>
        /// Updates this object from a network packet.
        /// </summary>
        /// <param name="packet">Packet to update from.</param>
        public abstract void UpdateFromPacket(Packet packet);

    }
}
