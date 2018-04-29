using System;
using System.Collections.Generic;
using System.IO;
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

        private Buffer VBO_Verts, VBO_Colors, VBO_Tex;
        private Buffer EBO;
        private DataStream Verts, Colors, Tex,Faces;

        private InputElement[] Elements;

        private InputLayout InputLayout;
        private Effect Effects;
        private EffectPass Pass;



        //number of particles emitted per 100 frames.
        public int emissionRate = 1;
        public int maxParticles = 1;

        public int size = Vector3.SizeInBytes * 3;




        private Random r;
        public ParticleSystem()
        {
            r = new Random();

            for (int i = 0; i < maxParticles; i++)
            {
                Particles.Add(new Particle(Vector3.Zero, Vector3.Zero, 1.0f, 0f));
            }

            Verts = new DataStream(Particles.Count * size, true, true);
            Colors = new DataStream(Particles.Count * size, true, true);
            Faces = new DataStream(Particles.Count * sizeof(int) * 3,true,true);

            for (var index = 0; index < Particles.Count; index++)
            {
                var pt = Particles[index];
                Vector3 v2 = pt.Position;
                v2.X += 1.0f;
                Vector3 v3 = pt.Position;
                v3.Y -= 1.0f;


                Verts.Write(pt.Position);
                Verts.Write(v2);
                Verts.Write(v3);

                Colors.Write(new Vector3((float) r.NextDouble(), (float) r.NextDouble(), (float) r.NextDouble()));
                Colors.Write(new Vector3((float) r.NextDouble(), (float) r.NextDouble(), (float) r.NextDouble()));
                Colors.Write(new Vector3((float) r.NextDouble(), (float) r.NextDouble(), (float) r.NextDouble()));

                Faces.Write(index * 3);
                Faces.Write((index *3) +1);
                Faces.Write((index * 3) +2);

            }
            Verts.Position = 0;
            Colors.Position = 0;
            Faces.Position = 0;

            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VBO_Colors = new Buffer(GraphicsRenderer.Device, Colors, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            EBO = new Buffer(GraphicsRenderer.Device, Faces, Particles.Count * 3 * sizeof(int), ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
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

        public void UpdateBuffer()
        {
            Verts.Position = 0;
            Particles.ForEach(pt =>
            {
                Verts.Write(pt.Position);
                Vector3 v2 = pt.Position;
                v2.X += 1.0f;
                Verts.Write(v2);
                Vector3 v3 = pt.Position;
                v3.Y -= 1.0f;
                Verts.Write(v3);
            });
            Verts.Position = 0;

            VBO_Verts.Dispose();
            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

        }


        public override void Draw()
        {
            GraphicsRenderer.DeviceContext.InputAssembler.InputLayout = InputLayout;
            GraphicsRenderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBO_Verts, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(VBO_Colors, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetIndexBuffer(EBO,Format.R32_UInt,0);


            Effects.GetVariableByName("gWorld").AsMatrix().SetMatrix(Matrix.Identity);
            Effects.GetVariableByName("gView").AsMatrix()
                .SetMatrix(GraphicsManager.ActiveCamera.m_ViewMatrix);
            Effects.GetVariableByName("gProj").AsMatrix().SetMatrix(GraphicsRenderer.ProjectionMatrix);



            Pass.Apply(GraphicsRenderer.Device.ImmediateContext);

            GraphicsRenderer.DeviceContext.DrawIndexed(Particles.Count * 3,0,0);
           //GraphicsRenderer.DeviceContext.Draw(Particles.Count * 3, 0);
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
                        particle.LifeRemaining = r.Range(5f);
                        emissionThisFrame++;
                    }
                }

                particle.Force = new Vector3(r.NextFloat(), r.Range(10) - 5.0f, r.NextFloat());
                particle.Update(.001f);
            }
            UpdateBuffer();
        }

        private ShaderResourceView CreateTexture(string fileName)
        {
            return File.Exists(fileName) ? ShaderResourceView.FromFile(GraphicsRenderer.Device, fileName) : null;
        }

    }
}