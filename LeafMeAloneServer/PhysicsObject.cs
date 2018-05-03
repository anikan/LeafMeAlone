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
    public abstract class PhysicsObject : GameObjectServer
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
        public PhysicsObject(ObjectType objectType, float mass = 1.0f) : base(objectType)
        {
            Mass = mass;
        }

        /// <summary>
        /// Creates a new physics object, given an object type and optionally mass.
        /// </summary>
        /// <param name="objectType">Type of this object.</param>
        /// <param name="startTransform">Starting position.</param>
        /// <param name="mass">Optional mass of the object, default 1.</param>
        public PhysicsObject(ObjectType objectType, Transform startTransform, float mass = 1.0f) : base(objectType, startTransform)
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

            Velocity = RoundVectorToZero(Velocity, MIN_VELOCITY);

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
            if (vector.X < minBeforeZero) vector.X = 0.0f;
            if (vector.Y < minBeforeZero) vector.Y = 0.0f;
            if (vector.Z < minBeforeZero) vector.Z = 0.0f;

            return vector;
        }
    }
}
