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
                }


                // Get fire damage from the flamethrower.
                float fireDamage = Tool.GetToolInfo(ToolType.THROWER).Damage;

                // Decrease health by burn damage.
                Health -= fireDamage * deltaTime * Math.Min(burnFrames * BURNING_RAMP_RATE, 10);

//                Console.WriteLine("Health is " + Health);

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
        }

        /// <summary>
        /// Function called when this object is hit by the player's active tool (in range)
        /// </summary>
        /// <param name="toolTransform">Position of the player. </param>
        /// <param name="toolType">Type of the tool hit by.</param>
        /// <param name="toolMode">Mode (primary or secondary) the tool was in.</param>
        public virtual void HitByTool(Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {
            // Get information about the tool that was used on this object.
            ToolInfo toolInfo = Tool.GetToolInfo(toolType);

            if (toolType == ToolType.THROWER)
            {

                // If it's the primary flamethrower function
                if (toolMode == ToolMode.PRIMARY)
                {
                    CatchFire();

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
        { }

    }
}