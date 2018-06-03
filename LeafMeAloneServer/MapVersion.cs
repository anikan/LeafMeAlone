using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Server
{
    /// <summary>
    /// Version of a map.
    /// </summary>
    public abstract class MapVersion
    {
        // Obstacles for the RIGHT side of the map.
        public List<Vector3> sideObstacles;

        // Obstacles for no man's land.
        public List<Vector3> noMansObstacles;

        // Creates the obstacles for each side.
        public abstract List<Vector3> SetupNoMansLandObstacles();
        public abstract List<Vector3> SetupRightSideObstacles();

        public MapVersion()
        {

            sideObstacles = SetupRightSideObstacles();
            noMansObstacles = SetupNoMansLandObstacles();

        }
    }
}
