using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    public class TreeClient : NetworkedGameObjectClient
    {

        public TreeClient(CreateObjectPacket createPacket) : base(createPacket, Constants.TreeModel)
        {
           // Console.WriteLine("Making tree at position" + Transform.Position);
           // Transform.Rotation.Z = 90.0f;
           // Transform.Scale = new Vector3(3.0f, 3.0f, 3.0f);
        }

        public override void UpdateFromPacket(Packet packet)
        {
           // throw new NotImplementedException();
        }
    }
}
