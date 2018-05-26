
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;

namespace Shared
{
    public class Match
    {

        public static Match DefaultMatch
        {
            get
            {
                if (_DefaultMatch == null)
                {
                    CreateDefaultMatch();
                }

                return _DefaultMatch;
            }
        }

        private static Match _DefaultMatch;

        public int numTeams = 2;

        public List<TeamSection> teamSections;
        private TeamSection NoMansLand;

        public Match()
        {


        }

        public static Match CreateDefaultMatch()
        {

            Match newMatch = new Match();

            newMatch.NoMansLand = new TeamSection
            {

                leftX = -(Constants.MAP_WIDTH * Constants.NO_MANS_LAND_PERCENT) / 2.0f,
                rightX = (Constants.MAP_WIDTH * Constants.NO_MANS_LAND_PERCENT) / 2.0f,
                upZ = (Constants.MAP_HEIGHT / 2.0f),
                downZ = -(Constants.MAP_HEIGHT / 2.0f),
                sectionColor = new Vector3(0.0f, 0.0f, 0.0f)


            };



            newMatch.teamSections = new List<TeamSection>();

            newMatch.teamSections.Add(new TeamSection
            {
                leftX = -(Constants.MAP_WIDTH / 2.0f),
                rightX = newMatch.NoMansLand.leftX,
                upZ = (Constants.MAP_HEIGHT / 2.0f),
                downZ = -(Constants.MAP_HEIGHT / 2.0f),
                sectionColor = new Vector3(1.0f, 0.0f, 0.0f)

            });

            newMatch.teamSections.Add(new TeamSection
            {
                leftX = newMatch.NoMansLand.rightX,
                rightX = Constants.MAP_WIDTH / 2.0f,
                upZ = Constants.MAP_HEIGHT / 2.0f,
                downZ = -Constants.MAP_HEIGHT / 2.0f,
                sectionColor = new Vector3(0.0f, 0.0f, 1.0f)
            });

            Console.WriteLine(newMatch.teamSections[0]);
            Console.WriteLine(newMatch.teamSections[1]);
            Console.WriteLine(newMatch.NoMansLand);

            _DefaultMatch = newMatch;
            return newMatch;


        }

        public void CountObjectsOnSides(List<GameObject> objects)
        {

            foreach (TeamSection square in teamSections)
            {
                square.CountObjectsInBounds(objects);
            }

            NoMansLand.CountObjectsInBounds(objects);
        }

        public int GetTeamLeaves(int teamIndex, List<GameObject> objects)
        {
            teamSections[teamIndex].CountObjectsInBounds(objects);
            return teamSections[teamIndex].numLeaves;
        }

        public override string ToString()
        {

            string returnString = "[Current Match Status] ";

            for (int i = 0; i < teamSections.Count; i++)
            {

                //    Console.WriteLine(teamSections[i]);
                returnString += string.Format("Team {0}: {1}", i, teamSections[i].numLeaves);

                returnString += " | ";
            }

            returnString += string.Format("No Man's Leaves: {0}", NoMansLand.numLeaves);

            return returnString;



        }
    }
}
