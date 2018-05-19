using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;

namespace Server
{
    /// <summary>
    /// An object that responds to physics and has physics operations.
    /// </summary>
    public abstract class PhysicsObject : ColliderObject
    {

        // Universal gravity constant
        private const float GRAVITY = -9.81f;

        // Factor that affects how fast objects slow down (larger values slow faster).
        // This should never be greater than 1.0f
        private const float FAKE_FRICTION_FACTOR = 0.1f;

        // Minimum allowed force. Just to round out some floats that could be changing forever.
        private const float MIN_FORCE = 0.01f;
        
        // Minimum allowed velocity. Just to round out some floats that could be changing forever.
        private const float MIN_VELOCITY = 0.001f;

        // Mass of this object.
        public float Mass;

        // Force, applied each tick and then reset.
        // Affects through the ApplyForce() function.
        private Vector3 Force;

        // Acceleration and Velocity, calculated from force.
        private Vector3 Acceleration;
        private Vector3 Velocity;

        /// <summary>
        /// Creates a new physics object, given an object type and optionally mass.
        /// </summary>
        /// <param name="objectType">Type of this object.</param>
        /// <param name="mass">Optional mass of the object, default 1</param>
        public PhysicsObject(ObjectType objectType, float health, float mass, float radius) : base(objectType, health, radius)
        {
            Mass = mass;
        }

        /// <summary>
        /// Apply a force to this physics object.
        /// </summary>
        /// <param name="force">The force to apply.</param>
        public void ApplyForce(Vector3 force)
        {
            // Add in the force.
            Force += force;

            // Prevent super small forces by bounding it to a min force.
            Force = RoundVectorToZero(force, MIN_FORCE);

        }

        /// <summary>
        /// Update step for this physics objects. When all the physics calculations are done.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // Calculate accelaration from F = ma
            Acceleration = Force / Mass;

            // Calculate velocity given acceleration and a delta time in seconds
            Velocity = Velocity + (Acceleration * deltaTime);

            // Set position based on the new velocity
            Transform.Position = Transform.Position + (Velocity * deltaTime);

            // Zero out force, it's been used
            Force = Vector3.Zero;

            // Apply some fake friction so that the object slows down
            ApplyFakeFriction();

        }
       
        /// <summary>
        /// Slowly moves velocity to zero, to simulate super fake friction.
        /// </summary>
        public void ApplyFakeFriction()
        {

            // Get the current velocity (or round it to zero if it's really small).
            Velocity = RoundVectorToZero(Velocity, MIN_VELOCITY);

            // Decrease the velocity slightly and apply it back
            Vector3 FakeFriction = -Velocity * FAKE_FRICTION_FACTOR;
            Velocity += FakeFriction;

        }

        /// <summary>
        /// Checks if a vector's values are less than a specified minimum. If so, sets that value to zero.
        /// </summary>
        /// <param name="vector">Vector to check.</param>
        /// <param name="minBeforeZero">Minimum value before it's set to zero.</param>
        /// <returns>Vector with any values rounded to zero, if applicable.</returns>
        private Vector3 RoundVectorToZero(Vector3 vector, float minBeforeZero)
        {
            // Checks each value and sets to zero if less than minimum.
            if (Math.Abs(vector.X) < minBeforeZero) vector.X = 0.0f;
            if (Math.Abs(vector.Y) < minBeforeZero) vector.Y = 0.0f;
            if (Math.Abs(vector.Z) < minBeforeZero) vector.Z = 0.0f;

            return vector;
        }

        /// <summary>
        /// Function called when this object is hit by a player's tool.
        /// </summary>
        /// <param name="playerPosition">Position of the player that hit this object.</param>
        /// <param name="toolType">Type of tool equipped.</param>
        /// <param name="toolMode">Mode (primary or secondary) of tool equipped.</param>
        public override void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode)
        {
            // Call the base's HitByTool function (burns the object)
            base.HitByTool(playerPosition, toolType, toolMode);

            // Get information about the tool that was used on this object.
            ToolInfo toolInfo = Tool.GetToolInfo(toolType);

            // If this is a leafblower.
            if (toolType == ToolType.BLOWER)
            {

                // If this is the leafblower's primary tool.
                if (toolMode == ToolMode.PRIMARY)
                {


                    // Extinguish any objects that get blowed by the leaf blower.
                    Extinguish();

                    // Get the force of this tool.
                    float toolForce = toolInfo.Force;

                    // Get the vector from the player to the object.
                    Vector3 playerToObj = Transform.Position - playerPosition;
                    playerToObj.Y = 0.0f;
                    float distance = playerToObj.Length();

                    // Divide the vector by the range of the tool to normalize it.
                    playerToObj /= toolInfo.Range;

                    // Multiply tool force by distance so that it's stronger on objects that are closer.
                    // Also make sure denominator can't be zero.
                    toolForce /= Math.Max(0.001f, distance);

                    // Apply a force in the direction of the player -> object.
                    Vector3 force = playerToObj * toolForce;
                    ApplyForce(force);

                    // Console.WriteLine("Blowing object {0} {1} with force {2}", this.GetType().ToString(), Id, force);


                }
            }
        }
    }
}
