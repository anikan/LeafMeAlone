using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    class MoveRequest : Request
    {
        private int Id;
        private Vector3 Position;


        public MoveRequest(GameObject gameObject, Vector3 position)
        {
            Id = gameObject.GetId();
            Position = position;
        }
    }
}
