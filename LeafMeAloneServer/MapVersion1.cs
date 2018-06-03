using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;

namespace Server
{
    public class MapVersion1 : MapVersion
    {

        public override List<Vector3> SetupNoMansLandObstacles()
        {
            List<Vector3> returnList = new List<Vector3>();


            TeamSection NoMansLand = MatchHandler.match.NoMansLand;

            float leftX = NoMansLand.leftX;
            float rightX = NoMansLand.rightX;

            float yStart = -Constants.MAP_HEIGHT / 2.0f;
            float yEnd = Constants.MAP_HEIGHT / 2.0f;

            int obstacleCount = 0;

            int totalObstacles = (int)Math.Floor((yEnd - yStart) / Constants.TREE_RADIUS);

            int extraTopObstacles = 3;
            int gapSize = 3;

            for (float y = yStart; y < yEnd; y += Constants.TREE_RADIUS)
            {

                if (obstacleCount < extraTopObstacles || obstacleCount > extraTopObstacles + gapSize)
                {
                    returnList.Add(new Vector3(leftX, Constants.FLOOR_HEIGHT, y));
                }

                if (obstacleCount < (totalObstacles - (extraTopObstacles + gapSize)) || obstacleCount > (totalObstacles - extraTopObstacles))
                {
                    returnList.Add(new Vector3(rightX, Constants.FLOOR_HEIGHT, y));

                }

                obstacleCount++;
                
            }

            return returnList;



        }

        public override List<Vector3> SetupRightSideObstacles()
        {

            List<Vector3> returnList = new List<Vector3>();


            return returnList;

        }
    }
}
