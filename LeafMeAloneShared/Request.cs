﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public abstract class Request
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

