using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface ILeaf : INetworked
    {

        bool Burning { get; set; }
        float TimeBurning { get; set; }

    }
}

