using System;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Client
{
    public abstract class Drawable
    {
        private const int TrianglePts = 3;

        protected Buffer VBO_Verts, VBO_Colors, VBO_Tex;
        private DataStream Verts, Colors, Tex;

        private InputElement[] Elements;

        private InputLayout InputLayout;
        private Effect Effects;
        private EffectPass Pass;


        protected void InitBuffer(Vector3 initPos, float delta, int size=1)
        {
            Verts = new DataStream(size * Vector3.SizeInBytes * TrianglePts, true, true);
            Colors = new DataStream(size * Vector3.SizeInBytes * TrianglePts, true, true);

            Verts.Write(initPos);
            Vector3 v2 = initPos;
            v2.X += delta;
            Verts.Write(v2);
            Vector3 v3 = initPos;
            v3.Y += delta;
            Verts.Write(v2);

            Random r = new Random();
            Colors.Write(new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()));

            Verts.Position = 0;
            Colors.Position = 0;

            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, size * Vector3.SizeInBytes * TrianglePts, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VBO_Colors = new Buffer(GraphicsRenderer.Device, Colors, size * Vector3.SizeInBytes * TrianglePts, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var btcode = ShaderBytecode.CompileFromFile(@"../../Shaders/particle.fx", "VS", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(@"../../Shaders/particle.fx", "Render", "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(btcode);

            Effects = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effects.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("COLOR", 0, Format.R32G32B32_Float, 1)
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);
        }

        void UpdateBuffer(Vector3 position, float delta, int size=1)
        {
            Verts.Position = 0;
            Verts.Write(position);
            Vector3 v2 = position;
            v2.X += delta;
            Verts.Write(v2);
            Vector3 v3 = position;
            v3.Y += delta;
            Verts.Write(v2);
            Verts.Position = 0;

            VBO_Verts.Dispose();
            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, size * Vector3.SizeInBytes * TrianglePts, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }
    }


    public class Particle : Drawable
    {
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