using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafMeAloneShared
{
    abstract class Request
    {
        public enum RequestType
        {
            NONE,
            MOVE,
            ROTATE,
            ACTION
        }

        public RequestType type;
    }
}

