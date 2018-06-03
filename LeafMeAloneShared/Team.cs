using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    public enum TeamName
    {
        RED,
        BLUE
    };

    /// <summary>
    /// Object for storing data about a team and doing team things
    /// </summary>
    public class Team
    {
        public TeamName name;
        public TeamSection teamSection;
        private int spawnIndex = 0;

        public Team(TeamName teamName, TeamSection section)
        {
            name = teamName;
            teamSection = section;
        }

        /// <summary>
        /// Gets the next spawn location for the team
        /// </summary>
        /// <returns>A vector 3 for the spawn location</returns>
        public Vector3 GetNextSpawnPoint()
        {
            return teamSection.spawnPoints[spawnIndex++ % teamSection.spawnPoints.Count];
        }
    }
}
