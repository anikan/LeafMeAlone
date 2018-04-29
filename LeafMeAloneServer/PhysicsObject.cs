using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;

namespace Server
{
    public abstract class PhysicsObject : GameObjectServer
    {

        public const float Gravity = -9.81f;

        private Vector3 Acceleration;
        private Vector3 Velocity;
        private float Mass = 1.0f;

        private float FakeFrictionFactor = 0.1f;

        private Vector3 Force;

        public PhysicsObject(ObjectType objectType) : base(objectType)
        {

        }

        public PhysicsObject(ObjectType objectType, Transform startTransform) : base(objectType, startTransform)
        {

        }

        public void ApplyForce(Vector3 force)
        {
            Force += force;
        }

        public override void Update(float deltaTime)
        {

            Acceleration = Force / Mass;

            Velocity = Velocity + (Acceleration * deltaTime);

            Transform.Position = Transform.Position + (Velocity * deltaTime);

            Force = Vector3.Zero;

        }

        public void ApplyFakeFriction()
        {

            // Add in some friction stuff here.

        }
    }
}
