using System;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Client
{
  

    public class Particle
    {

        /// <summary>
        /// Creation origin of the particle in world space.
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// Current position of the particle in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Current Velocity (Derivative of Position)
        /// </summary>
        public Vector3 Velocity;

        public Vector3 InitialVelocity;

        /// <summary>
        /// Current force exerted on particle. Note: Gets reset after every update loop.
        /// </summary>
        public Vector3 Force;

        /// <summary>
        /// Current Acceleration (Derivative of Velocity)
        /// </summary>
        private Vector3 Acceleration;

        /// <summary>
        /// Normal for object
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The Current mass of the object. 
        /// </summary>
        public float Mass;

        /// <summary>
        /// Remaining Life of the particle.
        /// </summary>
        public float LifeRemaining;


        public Particle(Vector3 position, Vector3 velocity, float mass, float lifeRemaining)
        {
            Origin = position;
            Position = position;
            Velocity = velocity;
            InitialVelocity = velocity;
            LifeRemaining = lifeRemaining;
            Mass = mass;
            Force = Vector3.Zero;

        }
        public void Update(float deltaTime, bool fixedVelocity = false)
        {
            if (!fixedVelocity)
            {
                Acceleration = (1.0f / Mass) * Force;
                Velocity += Acceleration * deltaTime;
            }

            Position += Velocity * deltaTime;
            Force = Vector3.Zero;
            LifeRemaining -= deltaTime;
        }
    }
}