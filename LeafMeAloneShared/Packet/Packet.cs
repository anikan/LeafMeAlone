using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Packet
{
    /// <summary>
    /// Empty base class for packet which is extended by all packet subclasses.
    /// </summary>
    public class BasePacket
    {
        public PacketType packetType;
        public BasePacket(PacketType type)
        {
            packetType = type;
        }
    }
}
