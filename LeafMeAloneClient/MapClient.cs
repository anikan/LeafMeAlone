using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// Client-side game map. Mainly for just rendering a terrain.
    /// </summary>
    public class MapClient : NonNetworkedGameObjectClient
    {

        /// <summary>
        /// Creates a new map with a map model.
        /// </summary>
        /// <param name="modelPath"></param>
        public MapClient(string modelPath = Constants.DefaultMapModel) : base(modelPath)
        {

            // Create the terrain at floor height, slightly below.
            Transform.Position.Y = Constants.FLOOR_HEIGHT - 1.0f;
        }
    }
}
