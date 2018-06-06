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
    /// Object that has a collider (radius) around it that objects can't penetrate. Super barrier.
    /// </summary>
    public class ColliderObject : GameObjectServer
    {
        public bool Collidable = true;

        protected enum ColliderType
        {
            NONE,
            CIRCLE,
            BOX
        }

        protected ColliderType colliderType;

        private Random rnd;

        // Radius of this object for basic n00b collisions.
        public float Radius = 1.0f;

        /// <summary>
        /// Creates a new collider object.
        /// </summary>
        /// <param name="objectType">Type of this object.</param>
        /// <param name="health">Obhect health.</param>
        /// <param name="radius">Radius of the object for collisions.</param>
        public ColliderObject(ObjectType objectType, float health) : base(objectType, health)
        {

            rnd = new Random();
        }

        /// <summary>
        /// Destroys this object.
        /// </summary>
        public override void Die()
        {
            base.Die();
        }

        /// <summary>
        /// What happens when this object is hit by a tool.
        /// </summary>
        /// <param name="toolTransform"></param>
        /// <param name="toolType"></param>
        /// <param name="toolMode"></param>
        public override void HitByTool(PlayerServer player, Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {
            if (Collidable)
            {
                base.HitByTool(player, toolTransform, toolType, toolMode);
            }
        }

        /// <summary>
        /// Updates every server tick.
        /// </summary>
        /// <param name="deltaTime">Time since last tick.</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

        }

        /// <summary>
        /// Checks if this object is colliding with the collider of another object.
        /// </summary>
        /// <param name="other">Other collider object.</param>
        /// <returns>True if colliding, false otherwise.</returns>
        public bool IsColliding(ColliderObject other)
        {
            if (!other.Collidable || !Collidable || other is LeafServer)
            {
                return false;
            }


            // Get this position and the position of the other object, and make 2D.
            Vector3 colliderPos = Transform.Position;
            Vector3 otherPos = other.Transform.Position;
            colliderPos.Y = 0.0f;
            otherPos.Y = 0.0f;

            if (other.colliderType == ColliderType.CIRCLE)
            {

                // Get distance between objects.
                float distance = Vector3.Distance(colliderPos, otherPos);

                // Check if the objects are overlapping.
                if (other.Radius > 0.0f && distance <= Radius + other.Radius)
                {
                    // If overlapping, they're colliding. Return true.
                    return true;
                }
            }
            else if (other.colliderType == ColliderType.BOX)
            {

                float down = other.Transform.Position.Z - other.Radius;
                float up = other.Transform.Position.Z + other.Radius;
                float left = other.Transform.Position.X - other.Radius;
                float right = other.Transform.Position.X + other.Radius;


                if (Transform.Position.X > left && Transform.Position.X < right && Transform.Position.Z < up && Transform.Position.Z > down)
                {
                    return true;
                }
            }

            // If not overlapping, return false.
            return false;

        }

        /// <summary>
        /// Tries to move an object to a new position, based on collider positions.
        /// </summary>
        /// <param name="newPosition"></param>
        public bool TryMoveObject(Vector3 newPosition, int stacklevel = 0)
        {

            // Save the original position of this object.
            Vector3 OriginalPosition = Transform.Position;

            // First, update the position.
            Transform.Position = newPosition;

            // First, we need all the game objects on the server.
            List<GameObjectServer> allObjects = GameServer.instance.GetGameObjectList();

            // Iterate through all the objects.
            for (int i = 0; i < allObjects.Count; i++)
            {
                // Check if the object has a collider.
                if (allObjects[i] is ColliderObject obj)
                {
                    // If the object is colliding, just return. No movement.
                    if (obj != this && IsColliding(obj))
                    {
                        //    Console.WriteLine(string.Format("Cannot move {0} {1}. Colliding with {2} {3}, radius {4}", GetType(), Id, obj.GetType(), obj.Id, obj.Radius));

                        // Set the object back to it's original position.
                        Transform.Position = OriginalPosition;

                        // Try to move only in the X axis.
                        if (stacklevel < 1)
                        {
                            TryMoveObject(new Vector3(newPosition.X, OriginalPosition.Y, OriginalPosition.Z), stacklevel + 1);
                        }

                        // Try to move only in the Z axis.
                        if (stacklevel < 1)
                        {
                            TryMoveObject(new Vector3(OriginalPosition.X, OriginalPosition.Y, newPosition.Z), stacklevel + 1);
                        }

                        // If this is a physics object
                        if (this is PhysicsObject me)
                        {

                            // If the other object is also a physics object.
                            if (obj is PhysicsObject other)
                            {

                                // Push the other object.
                                me.Push(other);
                            }

                            // Bounce off the other object.
                            me.Bounce(obj);

                        }

                        EnsureSafePosition();

                        // couldn't move
                        return false;
                    }
                }
            }

            // Moved! 
            //The object moved, it's been modified.
            Modified = true;

            EnsureSafePosition();

            return true;
        }


        /// <summary>
        /// Checks if a vector's values are less than a specified minimum. If so, sets that value to zero.
        /// </summary>
        /// <param name="vector">Vector to check.</param>
        /// <param name="minBeforeZero">Minimum value before it's set to zero.</param>
        /// <returns>Vector with any values rounded to zero, if applicable.</returns>
        public Vector3 RoundVectorToZero(Vector3 vector, float minBeforeZero)
        {
            // Checks each value and sets to zero if less than minimum.
            if (Math.Abs(vector.X) < minBeforeZero) vector.X = 0.0f;
            if (Math.Abs(vector.Y) < minBeforeZero) vector.Y = 0.0f;
            if (Math.Abs(vector.Z) < minBeforeZero) vector.Z = 0.0f;

            return vector;
        }

        /// <summary>
        /// Force the object to a safe position.
        /// </summary>
        /// <param name="stackCount">Times this function has been recursively called.</param>
        public void EnsureSafePosition(int stackCount = 0)
        {

            // Is the position safe?
            bool safe = false;

            // Save the original position of this object.
            Vector3 OriginalPosition = Transform.Position;

            // First, we need all the game objects on the server.
            List<GameObjectServer> allObjects = GameServer.instance.GetGameObjectList();

            // Iterate through all the objects.
            for (int i = 0; i < allObjects.Count; i++)
            {
                // Check if the object has a collider.
                if (allObjects[i] is ColliderObject obj && (!(this is LeafServer && obj is PlayerServer)))
                {
                    // If the object is colliding, mark as unsafe.
                    if (obj != this && IsColliding(obj))
                    {
                        safe = false;
                        break;
                    }
                }

                // Position is safe.
                safe = true;
            }

            // If not safe (and preventing stack overflow)
            if (!safe && stackCount < 10)
            {

                // Get a random offset for the position.
                Vector3 newRandomOffset = new Vector3(Utility.RandomInRange(-5.0f, 5.0f), Utility.RandomInRange(-5.0f, 5.0f), Utility.RandomInRange(-5.0f, 5.0f));
                Vector3 newTestPosition = Transform.Position + newRandomOffset;

                // Try to move the object to the test position.
                TryMoveObject(newTestPosition);

                // Ensure safe position again.
                EnsureSafePosition(stackCount+1);

            }
        }
    }
}