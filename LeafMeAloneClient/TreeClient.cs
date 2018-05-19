using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// A tree on the client-side.
    /// </summary>
    public class TreeClient : NetworkedGameObjectClient
    {

        /// <summary>
        /// Create a new tree, based on a packet sent from the server.
        /// </summary>
        /// <param name="createPacket">Packet of a tree being created.</param>
        public TreeClient(CreateObjectPacket createPacket) : base(createPacket, Constants.TreeModel)
        {
            // Scale the tree to make it slightly bigger.
            Transform.Scale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        /// <summary>
        /// Update this object from a server packet.
        /// </summary>
        /// <param name="packet">Packet from server.</param>
        public override void UpdateFromPacket(Packet packet)
        {
           // throw new NotImplementedException();
        }
    }
}
