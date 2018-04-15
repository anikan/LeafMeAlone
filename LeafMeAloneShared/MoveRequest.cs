using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafMeAloneShared
{
    class MoveRequest : Request
    {
        public int id;

        public MoveRequest(GameObject gameObject, Vector3 movement)
        {
            id = gameObject.getId();


        }
    }
}
