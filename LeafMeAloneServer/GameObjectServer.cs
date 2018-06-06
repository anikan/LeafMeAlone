using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using System.Diagnostics;
using SlimDX;

namespace Server
{
    /// <summary>
    /// GameObject that exists on the server.
    /// </summary>
    public abstract class GameObjectServer : GameObject
    {

        // Is the object being actively burned this frame?
        private bool FlamethrowerActivelyBurning = false;

        // Rate (in seconds) that health decrements if an object is on fire.
        public const float HEALTH_DECREMENT_RATE = 1.0f;
        public const float BURNING_RAMP_RATE = 1.0f;
        public const float SECONDS_TO_EXTINGUISH = 0.5f;

        private Stopwatch ExtinguishTimer;

        public PlayerServer LastPlayerInteracted;
        public PlayerServer PlayerThatSetThisOnFire;

        //True if this object has been changed since the last update and needs to be sent to all clients.
        //Set when burning or when it moves.
        public bool Modified;

        /// <summary>
        /// Constructor for a GameObject that's on the server, with default instantiation position.
        /// </summary>
        /// <param name="objectType">Type of this object.</param>
        /// <param name="health">Health this object has.</param>
        public GameObjectServer(ObjectType objectType, float health) : base()
        {
            // Call the initialize function with correct arguments.
            Initialize(objectType, health);

        }

        /// <summary>
        /// Constructor for a GameObject that's on the server, with a specified position.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="startPosition">Location the object should be created.</param>
        /// <param name="health">Health this object has.</param>
        public GameObjectServer(ObjectType objectType, Transform startPosition, float health) : base(startPosition)
        {
            Initialize(objectType, health);
        }

        /// <summary>
        /// Initializes this object's values and data structures.
        /// </summary>
        /// <param name="objectType">Type of this object.</param>
        /// <param name="health">Amount of health this object has.</param>
        public void Initialize(ObjectType objectType, float health)
        {
            ObjectType = objectType;
            Health = health;
            ExtinguishTimer = new Stopwatch();

        }

        /// <summary>
        /// Update step for this game object. Runs every tick.
        /// </summary>
        /// <param name="deltaTime">Time since last frame.</param>
        public override void Update(float deltaTime)
        {

            CheckIfChangedSection();

            // If this object is burning.
            if (Burning || FlamethrowerActivelyBurning)
            {
                //The object took damage, it's been modified.
                Modified = true;

                // If it is being actively burned this frame.
                if (FlamethrowerActivelyBurning)
                {

                    // Increase the frames this object is burning.
                    burnFrames++;

                    // No longer burning next frame
                    FlamethrowerActivelyBurning = false;
                }

                // If not actively being burned this frame, set burn frames to just 1.
                else
                {
                    // Set to 1.
                    burnFrames = 1;
                }

                // If we've been blowing on the object for the desired period of time, extinguish it.
                if (blowFrames * deltaTime > SECONDS_TO_EXTINGUISH)
                {
                    // Stop the object from burning.
                    Extinguish();

                    // If there was a player that interacted with this object.
                    if (LastPlayerInteracted != null)
                    {

                        // If this is a leaf.
                        if (this is LeafServer)
                        {
                            // Increment the number of leaves that were extinguished.
                            LastPlayerInteracted.playerStats.numLeavesExtinguished++;
                        }

                        // If this is a Player.
                        else if (this is PlayerServer me && me.Team == LastPlayerInteracted.Team)
                        {
                            // Increment number of teammates that were extinguished.
                            LastPlayerInteracted.playerStats.timesTeammateExtinguished++;
                        }
                    }
                }


                // Get fire damage from the flamethrower.
                float fireDamage = Tool.GetToolInfo(ToolType.THROWER).Damage;
                float totalDamageToDeal = fireDamage * deltaTime * Math.Min(burnFrames * BURNING_RAMP_RATE, 10);

                // Decrease health by burn damage.
                Health -= totalDamageToDeal;

                // If there is a player that set this object on fire.
                if (PlayerThatSetThisOnFire != null)
                {
                    // If this is a leaf.
                    if (this is LeafServer)
                    {
                        // Increase player's damage to leaves.
                        PlayerThatSetThisOnFire.playerStats.fireDamageDealtToLeaves += totalDamageToDeal;
                    }
                    
                    // If this is a player.
                    else if (this is PlayerServer me)
                    {
                        // Increase damage to enemy players.
                        if (me.Team != PlayerThatSetThisOnFire.Team)
                        {
                            PlayerThatSetThisOnFire.playerStats.fireDamageDealtToEnemies += totalDamageToDeal;
                        }

                        // Increase damage to team players.
                        else
                        {
                            PlayerThatSetThisOnFire.playerStats.fireDamageDealtToTeammates += totalDamageToDeal;
                        }

                        // Increment damage taken.
                        me.playerStats.damageTaken += totalDamageToDeal;
                    }
                }

                // If health goes negative, destroy the object.
                if (Health <= 0)
                {
                    // Destroy.
                    Die();
                }

                if (ExtinguishTimer.ElapsedMilliseconds / 1000.0f >= Constants.MAX_SECONDS_BURNING)
                {
                    Extinguish();
                }

            }
        }

        /// <summary>
        /// Checks if this object changed sections this update.
        /// </summary>
        public void CheckIfChangedSection()
        {

            // Ensure prev section is set.
            if (prevSection == null)
            {
                prevSection = section;
            }

            // Changed section!
            if (section != prevSection)
            {

                // If this did not come from no man's land (came from red or blue side).
                if (prevSection.team.name != TeamName.NONE)
                {
                    // If it was taken from the other team
                    if (prevSection.team != LastPlayerInteracted.Team)
                    {
                        // Leaf was stolen from that player's section!
                        LastPlayerInteracted.playerStats.numLeavesStolen++;
                    }

                    // If it was taken from your own team.
                    else if (prevSection.team == LastPlayerInteracted.Team)
                    {
                        // Silly, you just blew away your own leaf.
                        LastPlayerInteracted.playerStats.numberOfOwnLeavesBlownAway++;
                    }
                }

                // If the NEW section is your team.
                if (section.team == LastPlayerInteracted.Team)
                {
                    // Congrats, you claimed a leaf!
                    LastPlayerInteracted.playerStats.numLeavesClaimed++;
                }

                // If the NEW section is the enemy team.
                else if (section.team != LastPlayerInteracted.Team && section.team.name != TeamName.NONE)
                {
                    LastPlayerInteracted.playerStats.numLeavesClaimedForEnemy++;
                }

                prevSection = section;
            }
        }

        /// <summary>
        /// Catch this object on fire.
        /// </summary>
        public void CatchFire()
        {
            if (Burnable)
            {
                FlamethrowerActivelyBurning = true;
                ExtinguishTimer.Restart();

            }
        }

        /// <summary>
        /// Extinguish this object.
        /// </summary>
        public void Extinguish()
        {

            // Extinguish the object by setting burn and blow frames to 0.
            burnFrames = 0;
            blowFrames = 0;
            ExtinguishTimer.Stop();
            PlayerThatSetThisOnFire = null;
        }

        /// <summary>
        /// Function called when this object is hit by the player's active tool (in range)
        /// </summary>
        /// <param name="toolTransform">Position of the player. </param>
        /// <param name="toolType">Type of the tool hit by.</param>
        /// <param name="toolMode">Mode (primary or secondary) the tool was in.</param>
        public virtual void HitByTool(PlayerServer player, Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {
            // Get information about the tool that was used on this object.
            ToolInfo toolInfo = Tool.GetToolInfo(toolType);

            LastPlayerInteracted = player;

            if (toolType == ToolType.THROWER)
            {

                // If it's the primary flamethrower function
                if (toolMode == ToolMode.PRIMARY)
                {
                    CatchFire();
                    PlayerThatSetThisOnFire = player;

                    if (burnFrames == 0)
                    {
                        if (this is LeafServer)
                        {
                            player.playerStats.numLeavesSetOnFire++;
                        }
                        else if (this is PlayerServer me)
                        {

                            if (me.Team == player.Team)
                            {
                                player.playerStats.numTeammateSetOnFire++;
                            }
                            else
                            {
                                player.playerStats.numEnemiesSetOnFire++;
                            }
                        }
                    }
                }
            }

            if (toolType == ToolType.BLOWER)
            {

                // If this is the primary function of the blower.
                if (toolMode == ToolMode.PRIMARY && Burning)
                {

                    blowFrames++;

                }
                else
                {
                    blowFrames = 0;
                }
            }
            else
            {
                blowFrames = 0;
            }
        }

        /// <summary>
        /// Gets the distance of this object to the player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>The distance from the object to the player</returns>
        public float GetDistanceToTool(Transform toolTransform)
        {

            // Get the vector between the two.
            Vector3 VectorBetween = Transform.Position - toolTransform.Position;
            VectorBetween.Y = 0.0f;

            // Calculate the length.
            return VectorBetween.Length();

        }

        /// <summary>
        /// Checks if this object is is within range of the player's tool, and should be affected.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>True if within range, false if not.</returns>
        public bool IsInPlayerToolRange(PlayerServer player)
        {
            //  Console.WriteLine("Checking object with ID " + Id  + " and type " + this.GetType().ToString());
            // Get the player's equipped tool.
            ToolInfo equippedToolInfo = Tool.GetToolInfo(player.ToolEquipped);
            Transform toolTransform = player.GetToolTransform();

            // Check if the leaf is within range of the player.
            if (GetDistanceToTool(toolTransform) <= equippedToolInfo.Range)
            {
                // Get the forward vector of the player.
                Vector3 ToolForward = toolTransform.Forward;
                ToolForward.Y = 0.0f;

                // Get the vector from the tool to the leaf.
                Vector3 ToolToObject = Transform.Position - toolTransform.Position;
                ToolToObject.Y = 0.0f;

                // Get dot product of the two vectors and the product of their magnitudes.
                float dot = Vector3.Dot(ToolForward, ToolToObject);
                float mag = ToolForward.Length() * ToolToObject.Length();

                // Calculate the angle between the two vectors.
                float angleBetween = (float)Math.Acos(dot / mag);
                angleBetween *= (180.0f / (float)Math.PI);

                //  Console.WriteLine(string.Format("{0} {1}: Angle between is {2}, must be {3} before hit", this.GetType().ToString(), Id, angleBetween, equippedToolInfo.ConeAngle / 2.0f));

                // Return true if the leaf is within the cone angle, false otherwise. 
                return (angleBetween <= (equippedToolInfo.ConeAngle / 2.0f));

            }

            return false;
        }

        /// <summary>
        /// Add this GameObject to the server's list of GameObjects and set the id.
        /// </summary>
        public void Register()
        {
            Id = GameServer.instance.nextObjectId++;

            GameServer.instance.gameObjectDict.Add(Id, this);
        }

        /// <summary>
        /// Death method which should be overridden by child classes
        /// </summary>
        public override void Die()
        {

            // If there is a valid player that set this on fire.
            if (PlayerThatSetThisOnFire != null)
            {
                // If this is a leaf.
                if (this is LeafServer)
                {
                    // Increase leaf destroy count for the other players.
                    PlayerThatSetThisOnFire.playerStats.numLeavesDestroyed++;

                    // If this leaf was on the player's team
                    if (section.team == PlayerThatSetThisOnFire.Team)
                    {
                        // Oops! You destroyed your own leaf.
                        PlayerThatSetThisOnFire.playerStats.numberOfOwnLeavesDestroyed++;
                    }

                }
                
                // If this is a player.
                else if (this is PlayerServer me)
                {

                    // If your teammate killed you.
                    if (me.Team == PlayerThatSetThisOnFire.Team)
                    {
                        // INcrease teammate kill count 
                        PlayerThatSetThisOnFire.playerStats.numTeammateKills++;

                        // Increase times you were killed by teammate.
                        me.playerStats.timesKilledByTeammate++;

                    }

                    // If an enemy killed you.
                    else
                    {
                        // Increase their kill count.
                        PlayerThatSetThisOnFire.playerStats.numEnemyKills++;

                        // Increase times you were killed by an enemy.
                        me.playerStats.timesKilledByEnemy++;

                    }

                }
            }
        }

    }
}