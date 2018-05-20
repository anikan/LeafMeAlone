using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    /// <summary>
    /// GameObject class for objects that are only client-side and not networked at all.
    /// </summary>
    public abstract class NonNetworkedGameObjectClient : GraphicGameObject
    {
        /// <summary>
        /// Constructor to create a new object with no model (mainly for particle systems).
        /// </summary>
        /// <param name="startTransform">Starting position of the object.</param>
        public NonNetworkedGameObjectClient() : base()
        {
        }

        /// <summary>
        /// Constructor to create a new object with a model and startTransform.
        /// </summary>
        /// <param name="modelPath">Path to this object's model</param>
        /// <param name="startTransform">Starting transform of this object.</param>
        public NonNetworkedGameObjectClient(string modelPath) : base(modelPath)
        {
        }
    }
}
