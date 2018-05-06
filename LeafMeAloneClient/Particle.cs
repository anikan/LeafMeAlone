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
        //create buffers
        public Buffer VBO_Verts, VBO_Tex;
        public Buffer EBO;
        public DataStream Verts, Tex, Faces;
        //public InputElement[] Elements;

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

        /// <summary>
        /// Current force exerted on particle. Note: Gets reset after every update loop.
        /// </summary>
        public Vector3 Force;

        /// <summary>
        /// Current Acceleration (Derivative of Velocity)
        /// </summary>
        public Vector3 Acceleration;

        /// <summary>
        /// Initial Acceleration (Derivative of Velocity)
        /// </summary>
        public Vector3 InitAcceleration;

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

        public Particle(Vector3 position, Vector3 velocity, Vector3 acceleration, float mass, float lifeRemaining)
        {
            InitAcceleration = acceleration;
            Origin = position;
            Position = position;
            Velocity = Vector3.Zero;
            LifeRemaining = lifeRemaining;
            Mass = mass;
            Force = Vector3.Zero;


        }


        public void Update(float deltaTime)
        {
            Acceleration = (1.0f / Mass) * Force;
            Velocity += Acceleration * deltaTime;
            Position += Velocity * deltaTime;
            Force = Vector3.Zero;
            LifeRemaining -= deltaTime;
        }



    }
}