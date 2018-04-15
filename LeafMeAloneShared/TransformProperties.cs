using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    public struct TransformProperties
    {
        Vector3 Position;  // location of the model in world coordinates
        Vector3 Direction; // unit vector pointing to the direction the model is facing
        Vector3 Scale;     // scale of the model
    }
}
