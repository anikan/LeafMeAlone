using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Client
{
    public class ParticleSystem : GameObject
    {

        public List<Particle> Particles = new List<Particle>();

        private Buffer VBO_Verts, VBO_Colors;
        private DataStream Verts, Colors;

        private InputElement[] Elements;

        private InputLayout InputLayout;
        private Effect Effects;
        private EffectPass Pass;



        //number of particles emitted per 100 frames.
        public int emissionRate = 1;

        public int maxParticles = 10000;





        private Random r;
        public ParticleSystem()
        {
            r = new Random(0);
            for (int i = 0; i < maxParticles; i++)
            {
                Particles.Add(new Particle(Vector3.Zero, Vector3.Zero, 1.0f, 0f));
            }

            Verts = new DataStream(Particles.Count * Vector3.SizeInBytes, true, true);
            Colors = new DataStream(Particles.Count * Vector3.SizeInBytes, true, true);


            Particles.ForEach(pt =>
            {
                Verts.Write(pt.Position);
                Colors.Write(new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()));

            });
            Verts.Position = 0;
            Colors.Position = 0;

            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * Vector3.SizeInBytes, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VBO_Colors = new Buffer(GraphicsRenderer.Device, Colors, Particles.Count * Vector3.SizeInBytes, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var btcode = ShaderBytecode.CompileFromFile(@"../../particle.fx", "VS", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(@"../../particle.fx", "Render", "fx_5_0", ShaderFlags.None,
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

        public void UpdateBuffer()
        {
            Verts.Position = 0;
            Particles.ForEach(pt =>
            {
                Verts.Write(pt.Position);
            });
            Verts.Position = 0;

            VBO_Verts.Dispose();
            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * Vector3.SizeInBytes, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

        }


        public override void Draw()
        {
            GraphicsRenderer.DeviceContext.InputAssembler.InputLayout = InputLayout;
            GraphicsRenderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBO_Verts, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(1,
                new VertexBufferBinding(VBO_Colors, Vector3.SizeInBytes, 0));



            Effects.GetVariableByName("gWorld").AsMatrix().SetMatrix(Matrix.Identity);
            Effects.GetVariableByName("gView").AsMatrix()
                .SetMatrix(GraphicsManager.ActiveCamera.m_ViewMatrix);
            Effects.GetVariableByName("gProj").AsMatrix().SetMatrix(GraphicsRenderer.ProjectionMatrix);


            Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
            GraphicsRenderer.DeviceContext.Draw(Particles.Count, 0);
        }
        
        public override void Update()
        {
            int emissionThisFrame = 0;
            foreach (Particle particle in Particles)
            {
                if (emissionThisFrame < emissionRate)
                {
                    if (particle.LifeRemaining <= 0)
                    {
                        particle.Position = Vector3.Zero;
                        particle.Acceleration = Vector3.Zero;
                        particle.Velocity = Vector3.Zero;
                        particle.LifeRemaining = 5.0f;
                        emissionThisFrame++;
                    }
                }

                particle.Force = new Vector3(r.NextFloat(), r.NextFloat(), r.NextFloat());
                particle.Update(.001f);
            }
            UpdateBuffer();
        }
    }
}