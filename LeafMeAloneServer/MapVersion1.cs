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

            // Get no man's left info.
            TeamSection rightSection = MatchHandler.match.teams[1].teamSection;

            float leftUBound = rightSection.leftX + Constants.MAP_WIDTH / 8.0f;
            float uHeight = Constants.MAP_HEIGHT / 3.0f;
            float uWidth = Constants.MAP_WIDTH / 7.0f;

            // Basically just create a U. Don't question it.
            for (float x = leftUBound; x < (leftUBound + uWidth); x+=Constants.TREE_RADIUS)
            {

                for (float y = (-uHeight / 2.0f); y < uHeight / 2.0f; y += Constants.TREE_RADIUS)
                {

                    bool createTree = false;

                    if (x <= leftUBound)
                    {
                        createTree = true;
                    }

                    if (y <= -uHeight / 2.0f)
                    {
                        createTree = true;
                    }

                    if (uHeight / 2.0f <= y + Constants.TREE_RADIUS)
                    {
                        createTree = true;
                    }

                    if (createTree)
                    {

                        returnList.Add(new Vector3(x, Constants.FLOOR_HEIGHT, y));

                    }

                }

            }



            return returnList;

        }
    }
}
