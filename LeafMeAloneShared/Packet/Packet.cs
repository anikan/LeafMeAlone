using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Empty base class for packet which is extended by all packet subclasses.
    /// </summary>
    public class Packet
    {
        PacketType packetType;
        public Packet(PacketType type)
        {
            packetType = type;
        }
    }
}
