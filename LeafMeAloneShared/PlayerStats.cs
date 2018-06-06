using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PlayerStats
    {

        public int playerId;

        // How many leaves were set on fire. STATUS: DONE.
        public int numLeavesSetOnFire;

        // How many players you set on fire. STATUS: 
        public int numEnemiesSetOnFire;

        public int numTeammateSetOnFire;

        // How many leaves were destroyed. STATUS: DONE.
        public int numLeavesDestroyed;

        // Number of leaves the player extinguished. STATUS: DONE
        public int numLeavesExtinguished;

        // Number of leaves stolen. STATUS: DONE
        public int numLeavesStolen;

        // Number of leaves claimed. STATUS: DONE
        public int numLeavesClaimed;

        // Number of players killed. STATUS: DONE
        public int numEnemyKills;

        public int numTeammateKills;

        // Number of times died. STATUS: DONE
        public int numDeaths;

        // Fire damage dealt to players. STATUS: DONE
        public float fireDamageDealtToEnemies;

        public float fireDamageDealtToTeammates;

        // Fire damage dealt to leaves. STATUS: DONE
        public float fireDamageDealtToLeaves;

        // Total damage taken. STATUS: DONE
        public float damageTaken;

        // Times extinguished teammate. STATUS: DONE
        public int timesTeammateExtinguished;

        // Times you blew away your own leaves. STATUS: DONE.
        public int numberOfOwnLeavesBlownAway;

        // Times you burned your own leaves. STATUS: DONE.
        public int numberOfOwnLeavesDestroyed;

        // How many leaves you pushed to the other player's team.
        public int numLeavesClaimedForEnemy;

        // Number of times you've been killed by your teammate.
        public int timesKilledByTeammate;

        // Number of times you were killed by the enemy.
        public int timesKilledByEnemy;

    }
}
