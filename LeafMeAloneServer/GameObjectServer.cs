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
    public abstract class GameObjectServer : GameObject
    {


        // How long this object can burn before being destroying.
        private float BurnTimeBeforeDestroy;

        // Timer for how long this object has been burning.
        private Stopwatch BurnTimer;

        /// <summary>
        /// Constructor for a GameObject that's on the server, with default instantiation position.
        /// </summary>
        /// <param name="objectType">Type of this object.</param>
        /// <param name="burnTime">Amount of time this object can burn before destroy.</param>
        public GameObjectServer(ObjectType objectType, float burnTime) : base()
        {
            // Call the initialize function with correct arguments.
            Initialize(objectType, burnTime);
        }

        /// <summary>
        /// Constructor for a GameObject that's on the server, with a specified position.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="startPosition">Location the object should be created.</param>
        /// <param name="burnTime">Time this object can burn before destroy.</param>
        public GameObjectServer(ObjectType objectType, Transform startPosition, float burnTime) : base(startPosition)
        {
            Initialize(objectType, burnTime);
        }

        /// <summary>
        /// Initializes this object's values and data structures.
        /// </summary>
        /// <param name="objectType">Type of this object.</param>
        /// <param name="burnTime">Time this object can burn before destroy.</param>
        public void Initialize(ObjectType objectType, float burnTime)
        {
            ObjectType = objectType;
            BurnTimeBeforeDestroy = burnTime;
            BurnTimer = new Stopwatch();
        }

        /// <summary>
        /// Update step for this game object. Runs every tick.
        /// </summary>
        /// <param name="deltaTime">Time since last frame.</param>
        public override void Update(float deltaTime)
        {
            // Destroy the object after it's done burning.
            if (BurnTimer.ElapsedMilliseconds / 1000.0f > BurnTimeBeforeDestroy)
            {
                // Omg how are we going to do destroy asdf?
            }
        }

        /// <summary>
        /// Catch this object on fire.
        /// </summary>
        public void CatchFire()
        {
            Burning = true;
            BurnTimer.Restart();
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
        public abstract void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode);

        /// <summary>
        /// Gets the distance of this object to the player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>The distance from the object to the player</returns>
        public float GetDistanceToPlayer(PlayerServer player)
        {

            // Get the vector between the two.
            Vector3 VectorBetween = Transform.Position - player.Transform.Position;

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

                // Return true if the leaf is within the cone angle, false otherwise. 
                return (angleBetween <= (equippedToolInfo.ConeAngle / 2.0f));

            }

            return false;

        }
    }
}