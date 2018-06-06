using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [ProtoContract]
    public class PlayerStats
    {

        [ProtoMember(1)]
        public int playerId;

        // How many leaves were set on fire. STATUS: DONE.
        [ProtoMember(2)]
        public int numLeavesSetOnFire;

        // How many players you set on fire. STATUS: 
        [ProtoMember(3)]
        public int numEnemiesSetOnFire;

        [ProtoMember(4)]
        public int numTeammateSetOnFire;

        // How many leaves were destroyed. STATUS: DONE.
        [ProtoMember(5)]
        public int numLeavesDestroyed;

        // Number of leaves the player extinguished. STATUS: DONE
        [ProtoMember(6)]
        public int numLeavesExtinguished;

        // Number of leaves stolen. STATUS: DONE
        [ProtoMember(7)]
        public int numLeavesStolen;

        // Number of leaves claimed. STATUS: DONE
        [ProtoMember(8)]
        public int numLeavesClaimed;

        // Number of players killed. STATUS: DONE
        [ProtoMember(9)]
        public int numEnemyKills;

        [ProtoMember(10)]
        public int numTeammateKills;

        // Number of times died. STATUS: DONE
        [ProtoMember(11)]
        public int numDeaths;

        // Fire damage dealt to players. STATUS: DONE
        [ProtoMember(12)]
        public float fireDamageDealtToEnemies;

        [ProtoMember(13)]
        public float fireDamageDealtToTeammates;

        // Fire damage dealt to leaves. STATUS: DONE
        [ProtoMember(14)]
        public float fireDamageDealtToLeaves;

        // Total damage taken. STATUS: DONE
        [ProtoMember(15)]
        public float damageTaken;

        // Times extinguished teammate. STATUS: DONE
        [ProtoMember(16)]
        public int timesTeammateExtinguished;

        // Times you blew away your own leaves. STATUS: DONE.
        [ProtoMember(17)]
        public int numberOfOwnLeavesBlownAway;

        // Times you burned your own leaves. STATUS: DONE.
        [ProtoMember(18)]
        public int numberOfOwnLeavesDestroyed;

        // How many leaves you pushed to the other player's team.
        [ProtoMember(19)]
        public int numLeavesClaimedForEnemy;

        // Number of times you've been killed by your teammate.
        [ProtoMember(20)]
        public int timesKilledByTeammate;

        // Number of times you were killed by the enemy.
        [ProtoMember(21)]
        public int timesKilledByEnemy;


        /// <summary>
        /// For fun printing of the stats. Can be used for UI too.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            string returnString = string.Format("\nPlayer {0} Stats -----------------------\n", playerId);

            returnString += "\n-- Leaf Stats --\n";

            returnString += string.Format("Leaves Claimed: {0}\n", numLeavesClaimed);
            returnString += string.Format("Leaves Stolen: {0}\n", numLeavesStolen);
            returnString += string.Format("Leaves Extinguished: {0}\n", numLeavesExtinguished);
            returnString += string.Format("Leaves Set On Fire: {0}\n", numLeavesSetOnFire);
            returnString += string.Format("Leaves Destroyed: {0}\n", numLeavesDestroyed);
            returnString += string.Format("Total Damage Done To Leaves: {0}\n", fireDamageDealtToLeaves);

            returnString += "\n-- Player Stats --\n";

            returnString += string.Format("Enemy Kills: {0}\n", numEnemyKills);
            returnString += string.Format("Enemies Set On Fire: {0}\n", numEnemiesSetOnFire);
            returnString += string.Format("Times Extinguished Teammate: {0}\n", timesTeammateExtinguished);
            returnString += string.Format("Fire Damage Done To Enemeis: {0}\n", fireDamageDealtToEnemies);
            returnString += string.Format("Deaths By Enemy: {0}\n", timesKilledByEnemy);
            returnString += string.Format("Total Damage Taken: {0}\n", damageTaken);

            returnString += "\n-- Shame Stats --\n";

            returnString += string.Format("Own Leaves Blown Out Of Team: {0}\n", numberOfOwnLeavesBlownAway);
            returnString += string.Format("Own Leaves Destroyed: {0}\n", numberOfOwnLeavesDestroyed);
            returnString += string.Format("Leaves Claimed For Enemy: {0}\n", numLeavesClaimedForEnemy);
            returnString += string.Format("Times Set Teammate on Fire: {0}\n", numTeammateSetOnFire);
            returnString += string.Format("Times Killed Teammate: {0}\n", numTeammateKills);
            returnString += string.Format("Times Killed By Teammate: {0}\n", timesKilledByTeammate);

            returnString += "\n---------------------------------------\n";

            return returnString;

        }

    }
}
