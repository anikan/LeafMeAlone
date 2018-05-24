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

        // Rate (in seconds) that health decrements if an object is on fire.
        public const float HEALTH_DECREMENT_RATE = 1.0f;

        // Timer for how long this object has been burning.
        private Stopwatch BurnTimer;

        // Timer to keep track of when health should decrement.
        private Stopwatch HealthDecrementTimer;

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
            BurnTimer = new Stopwatch();
            HealthDecrementTimer = new Stopwatch();
        }

        /// <summary>
        /// Update step for this game object. Runs every tick.
        /// </summary>
        /// <param name="deltaTime">Time since last frame.</param>
        public override void Update(float deltaTime)
        {


            // If this object is burning.
            if (Burning)
            {
                // Check if the health decrement rate has been passed and if so, decrement health.
                if (HealthDecrementTimer.ElapsedMilliseconds / 1000.0f >= HEALTH_DECREMENT_RATE)
                {

                    // Get fire damage from the flamethrower.
                    float fireDamage = Tool.GetToolInfo(ToolType.THROWER).Damage;

                    // Decrease health by burn damage.
                    Health -= fireDamage;

                    // Restart the decrement timer.
                    HealthDecrementTimer.Restart();

                }

                // If health goes negative, destroy the object.
                if (Health <= 0)
                {
                    // Destroy.
                    Destroy();
                }
            }
        }

        /// <summary>
        /// Catch this object on fire.
        /// </summary>
        public void CatchFire()
        {

            Burning = true;
            BurnTimer.Restart();
            HealthDecrementTimer.Restart();
        }

        /// <summary>
        /// Extinguish this object.
        /// </summary>
        public void Extinguish()
        {
            Burning = false;
            BurnTimer.Stop();
            BurnTimer.Reset();
        }

        /// <summary>
        /// Function called when this object is hit by the player's active tool (in range)
        /// </summary>
        /// <param name="playerPosition">Position of the player. </param>
        /// <param name="toolType">Type of the tool hit by.</param>
        /// <param name="toolMode">Mode (primary or secondary) the tool was in.</param>
        public virtual void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode)
        {

            // Get information about the tool that was used on this object.
            ToolInfo toolInfo = Tool.GetToolInfo(toolType);

            if (toolType == ToolType.THROWER)
            {

                // If it's the primary flamethrower function
                if (toolMode == ToolMode.PRIMARY)
                {

                    // If it's not already burning.
                    if (!Burning)
                    {
                        // Set the object on fire.
                        CatchFire();
                    }
                }
            }

            // If this is a blower.
            else if (toolType == ToolType.BLOWER)
            {

                // If this is the primary function of the blower.
                if (toolMode == ToolMode.PRIMARY)
                {

                    // Extinguish the leaf.
                    Burning = false;
                }
            }
        }

        /// <summary>
        /// Gets the distance of this object to the player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>The distance from the object to the player</returns>
        public float GetDistanceToPlayer(PlayerServer player)
        {

            // Get the vector between the two.
            Vector3 VectorBetween = Transform.Position - player.Transform.Position;
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

            // Check if the leaf is within range of the player.
            if (GetDistanceToPlayer(player) <= equippedToolInfo.Range)
            {
                // Get the forward vector of the player.
                // TODO: Have an actual Transform.Forward
                Vector3 PlayerForward = player.Transform.Forward;
                PlayerForward.Y = 0.0f;

                // Get the vector from the player to the leaf.
                Vector3 PlayertoObject = Transform.Position - player.Transform.Position;
                PlayertoObject.Y = 0.0f;

                // Get dot product of the two vectors and the product of their magnitudes.
                float dot = Vector3.Dot(PlayerForward, PlayertoObject);
                float mag = PlayerForward.Length() * PlayertoObject.Length();

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
            Id = GameServer.instance.gameObjectDict.Count();

            GameServer.instance.gameObjectDict.Add(Id, this);
        }

        /// <summary>
        /// Destroys this object and removes references.
        /// </summary>
        public override void Destroy()
        {
            GameServer.instance.Destroy(this);
        }
    }
}