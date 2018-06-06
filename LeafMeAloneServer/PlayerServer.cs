using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Packet;
using SlimDX;

namespace Server
{
    public class PlayerServer : PhysicsObject, IPlayer
    {

        public struct Stats
        {
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

        // Stats for the player.
        public Stats playerStats;

        // Constant values of the player.
        public const float PLAYER_MASS = 0.1f;
        public const float PLAYER_RADIUS = 3.0f;
        public const float PLAYER_SPEED = 25.0f;
        public const float THROWER_SPEED = 10.0f;

        private float currentSpeed = PLAYER_SPEED;

        public Team Team { get; set; }
        public bool Dead { get; set; }
        public ToolType ToolEquipped { get; set; }

        // If the user is using the primary function of their tool or secondary
        public ToolMode ActiveToolMode { get; set; }

        public Vector3 moveRequest;

        private Stopwatch deathClock = new Stopwatch();

        public PlayerServer(Team team) : base(ObjectType.PLAYER, Constants.PLAYER_HEALTH, PLAYER_MASS, 0.0f, true)
        {
            Team = team;
            ToolEquipped = ToolType.BLOWER;
            Burnable = true;
            Radius = PLAYER_RADIUS;
            colliderType = ColliderType.CIRCLE;
            JumpToRandomSpawn();
        }

        private void JumpToRandomSpawn()
        {
            Transform.Position = Team.GetNextSpawnPoint();
            foreach (ColliderObject obj in GameServer.instance.gameObjectDict.Values)
            {
                if (obj is PlayerServer || obj is TreeServer)
                {
                    if (obj != this && IsColliding(obj))
                    {
                        Transform.Position = Team.GetNextSpawnPoint();
                    }

                }
            }
        }

        /// <summary>
        /// Updates the object, runs every frame.
        /// </summary>
        /// <param name="deltaTime">Time since lsat frame, in seconds.</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            // Move the player in accordance with requests
            Vector3 newPlayerPos = Transform.Position + moveRequest * currentSpeed * deltaTime;
            newPlayerPos.Y = Constants.FLOOR_HEIGHT;
            TryMoveObject(newPlayerPos);
            TryRespawn();

            // If player isn't burning and is low on health.
            if (!Burning && Health < Constants.PLAYER_HEALTH)
            {
                // Regen health.
                Health += Constants.HEALTH_REGEN_RATE * deltaTime;
            }

            //Console.WriteLine(GetStatsString());

        }

        /// <summary>
        /// checks if the player is ready to respawn and does so.
        /// </summary>
        private void TryRespawn()
        {
            if (Dead && deathClock.Elapsed.Seconds > Constants.DEATH_TIME)
            {
                deathClock.Reset();
                Reset();
            }
        }

        /// <summary>
        /// Affects any object's within range of the player's tool.
        /// </summary>
        /// <param name="allObjects">A list of all objects in the game.</param>
        public void AffectObjectsInToolRange(List<GameObjectServer> allObjects)
        {
            // Iterate through all objects.
            for (int j = 0; j < allObjects.Count; j++)
            {
                //Get the current object.
                GameObjectServer gameObject = allObjects[j];

                if (ActiveToolMode == ToolMode.PRIMARY || ActiveToolMode == ToolMode.SECONDARY)
                {
                    // Check if it's within tool range, and that it's not the current player.
                    if (gameObject != this && gameObject.IsInPlayerToolRange(this))
                    {
                        // Hit the object.
                        gameObject.HitByTool(this, GetToolTransform(), ToolEquipped, ActiveToolMode);

                    }

                    if (ToolEquipped == ToolType.THROWER)
                    {

                        currentSpeed = THROWER_SPEED;

                    }
                    else
                    {
                        currentSpeed = PLAYER_SPEED;
                    }
                }
                else
                {
                    currentSpeed = PLAYER_SPEED;
                }
            }
        }

        /// <summary>
        /// Gets the transform of the active tool.
        /// </summary>
        /// <returns>Transform of the tool.</returns>
        public Transform GetToolTransform()
        {
            Matrix mat = Matrix.RotationX(Transform.Rotation.X) *
                       Matrix.RotationY(Transform.Rotation.Y) *
                       Matrix.RotationZ(Transform.Rotation.Z);

            Transform toolTransform = new Transform();
            toolTransform.Position = Transform.Position + Vector3.TransformCoordinate(Constants.PlayerToToolOffset, mat);
            toolTransform.Rotation = Transform.Rotation;

            // TODO: Make this the actual tool transform.
            // Currently just the player transform.

            return toolTransform;

        }


        /// <summary>
        /// Update the player based on a packet sent from the client.
        /// </summary>
        /// <param name="packet">Packet from client.</param>
        public void UpdateFromPacket(RequestPacket packet)
        {
            //Save movement request and normalize it so that we only move once per tick.
            moveRequest = new Vector3(packet.DeltaX, 0.0f, packet.DeltaZ);
            moveRequest.Normalize();

            Transform.Rotation.Y = packet.DeltaRot;

            if (packet.ToolRequest != ToolType.SAME)
            {
                ToolEquipped = packet.ToolRequest;
            }

            ActiveToolMode = packet.ToolMode;

            if (Dead)
            {
                ActiveToolMode = ToolMode.NONE;
            }
        }

        /// <summary>
        /// Function that determines what happens when the player is hit by another player's tool.
        /// </summary>
        /// <param name="toolTransform">Position of the other player.</param>
        /// <param name="toolType">Type of tool hit by.</param>
        /// <param name="toolMode">Tool mode hit by.</param>
        public override void HitByTool(PlayerServer player, Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {

            if (!Dead)
            {
                base.HitByTool(player, toolTransform, toolType, toolMode);
            }
        }

        /// <summary>
        /// Kill the player
        /// </summary>
        public override void Die()
        {

            base.Die();

            Dead = true;
            Burning = false;
            Burnable = false;
            Health = Constants.PLAYER_HEALTH;
            deathClock.Start();
            Collidable = false;
        }

        /// <summary>
        /// Resets the player to a specified position
        /// </summary>
        /// <param name="pos">The position to set the player to</param>
        internal void Reset()
        {
            Velocity = new Vector3();
            moveRequest = new Vector3();
            JumpToRandomSpawn();
            Health = Constants.PLAYER_HEALTH;
            Burning = false;
            Dead = false;
            Burnable = true;
            Collidable = true;
            ActiveToolMode = ToolMode.NONE;
        }

        /// <summary>
        /// For fun printing of the stats. Can be used for UI too.
        /// </summary>
        /// <returns></returns>
        public string GetStatsString()
        {

            string returnString = string.Format("\nPlayer {0} Stats -----------------------\n", Id);

            returnString += "\n-- Leaf Stats --\n";

            returnString += string.Format("Leaves Claimed: {0}\n", playerStats.numLeavesClaimed);
            returnString += string.Format("Leaves Stolen: {0}\n", playerStats.numLeavesStolen);
            returnString += string.Format("Leaves Extinguished: {0}\n", playerStats.numLeavesExtinguished);
            returnString += string.Format("Leaves Set On Fire: {0}\n", playerStats.numLeavesSetOnFire);
            returnString += string.Format("Leaves Destroyed: {0}\n", playerStats.numLeavesDestroyed);
            returnString += string.Format("Total Damage Done To Leaves: {0}\n", playerStats.fireDamageDealtToLeaves);

            returnString += "\n-- Player Stats --\n";

            returnString += string.Format("Enemy Kills: {0}\n", playerStats.numEnemyKills);
            returnString += string.Format("Enemies Set On Fire: {0}\n", playerStats.numEnemiesSetOnFire);
            returnString += string.Format("Times Extinguished Teammate: {0}\n", playerStats.timesTeammateExtinguished);
            returnString += string.Format("Fire Damage Done To Enemeis: {0}\n", playerStats.fireDamageDealtToEnemies);
            returnString += string.Format("Deaths By Enemy: {0}\n", playerStats.timesKilledByEnemy);
            returnString += string.Format("Total Damage Taken: {0}\n", playerStats.damageTaken);

            returnString += "\n-- Shame Stats --\n";

            returnString += string.Format("Own Leaves Blown Out Of Team: {0}\n", playerStats.numberOfOwnLeavesBlownAway);
            returnString += string.Format("Own Leaves Destroyed: {0}\n", playerStats.numberOfOwnLeavesDestroyed);
            returnString += string.Format("Leaves Claimed For Enemy: {0}\n", playerStats.numLeavesClaimedForEnemy);
            returnString += string.Format("Times Set Teammate on Fire: {0}\n", playerStats.numTeammateSetOnFire);
            returnString += string.Format("Times Killed Teammate: {0}\n", playerStats.numTeammateKills);
            returnString += string.Format("Times Killed By Teammate: {0}\n", playerStats.timesKilledByTeammate);

            returnString += "\n---------------------------------------\n";

            return returnString;

        }

    }
}
