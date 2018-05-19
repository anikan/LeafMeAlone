using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    public class TreeClient : NetworkedGameObjectClient
    {

        public TreeClient(CreateObjectPacket createPacket) : base(createPacket, FileManager.TreeModel)
        {
            Console.WriteLine("Making tree at position" + Transform.Position);
        }

        public override void UpdateFromPacket(Packet packet)
        {
           // throw new NotImplementedException();
        }
    }
}
