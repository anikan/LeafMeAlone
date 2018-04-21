using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Packet to send or receive from the server
    /// </summary>
    public abstract class Packet
    {

        // Was this packet changed?
        public bool Modified;

        // ID of the object this packet is associated with.
        public int ObjectID;

        /// <summary>
        /// Creates the packet
        /// </summary>
        /// <param name="id"></param>
        public Packet(int id)
        {
            ObjectID = id;
        }

        /// <summary>
        /// Send function for the packet
        /// </summary>
        public abstract void Send();

        /// <summary>
        /// Receive function for the packet, coming from the server
        /// </summary>
        public abstract void Receive();

    }
}
