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
        private float FrictionCoefficient = 0.1f;

        private Vector3 Force;

        public PhysicsObject() : base()
        {

        }

        public PhysicsObject(Transform startTransform) : base(startTransform)
        {

        }

        public void ApplyForce(Vector3 force)
        {
            Force += force;
        }

        public override void Update(float deltaTime)
        {

            AddFriction();

            Acceleration = Force / Mass;

            Velocity = Velocity + (Acceleration * deltaTime);

            Transform.Position = Transform.Position + (Velocity * deltaTime);

            Force = Vector3.Zero;

        }

        public void AddFriction()
        {

            Vector3 NormalForce = Mass * new Vector3(0.0f, Gravity, 0.0f);
            Vector3 Friction = FrictionCoefficient * NormalForce;
            ApplyForce(Friction);

        }
    }
}
