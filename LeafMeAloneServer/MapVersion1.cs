using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;

namespace Server
{
    /// <summary>
    /// First version of the map.
    /// </summary>
    public class MapVersion1 : MapVersion
    {

        /// <summary>
        /// Creates all no man's land obstacles.
        /// </summary>
        /// <returns></returns>
        public override List<Vector3> SetupNoMansLandObstacles()
        {
            // List to return.
            List<Vector3> returnList = new List<Vector3>();


            // Get no man's left info.
            TeamSection NoMansLand = MatchHandler.match.NoMansLand;

            // Left border of no man's land.
            float leftX = NoMansLand.leftX;

            // Right border of no man's land.
            float rightX = NoMansLand.rightX;

            // Where to start creating trees.
            float yStart = -Constants.MAP_HEIGHT / 2.0f;

            // Where to end creating trees.
            float yEnd = Constants.MAP_HEIGHT / 2.0f;

            // Number of obstacles created.
            int obstacleCount = 0;

            // Total obstacles that will be created.
            int totalObstacles = (int)Math.Floor((yEnd - yStart) / Constants.TREE_RADIUS);

            // Obstacles that should be at the top.
            int extraTopObstacles = 3;

            // Gap in tree size.
            int gapSize = 3;

            // Itereate through all y values.
            for (float y = yStart; y < yEnd; y += Constants.TREE_RADIUS)
            {

                // If this is not a gap area on the left side.
                if (obstacleCount < extraTopObstacles || obstacleCount > extraTopObstacles + gapSize)
                {
                    returnList.Add(new Vector3(leftX, Constants.FLOOR_HEIGHT, y));
                }
                
                // If this is not a gap area on the right side.
                if (obstacleCount < (totalObstacles - (extraTopObstacles + gapSize)) || obstacleCount > (totalObstacles - extraTopObstacles))
                {
                    returnList.Add(new Vector3(rightX, Constants.FLOOR_HEIGHT, y));

                }

                // Increase number of obstacles.
                obstacleCount++;
                
            }

            return returnList;

        }

        /// <summary>
        /// Create all side obstacles.
        /// </summary>
        /// <returns></returns>
        public override List<Vector3> SetupRightSideObstacles()
        {

            // List to return.
            List<Vector3> returnList = new List<Vector3>();

            return returnList;

        }
    }
}
