using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Server
{
    public class PlayerServer : Player
    {
        public override void UpdateFromPacket(Packet packet)
        {
            throw new NotImplementedException();
        }
    }
}
