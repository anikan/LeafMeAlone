using System;
using System.Collections.Generic;
using System.Drawing;
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
    public enum ParticleSystemType
    {
        FIRE,
        WIND
    }

    public class ParticleSystem : GameObject
    {
        public bool Enabled = true;


        //list of particles in the system.
        private List<Particle> Particles = new List<Particle>();


        //create buffers
        private Buffer VBO_Verts, VBO_Tex;
        private Buffer EBO;
        private DataStream Verts, Tex,Faces;
        private InputElement[] Elements;


        //create shader effects
        private InputLayout InputLayout;
        private Effect Effects;
        private EffectPass Pass;

        //create texture
        private ShaderResourceView TexSRV;

        
        //how fast particles are emitted
        public int emissionRate = 1;

        //max number of particles.
        public int maxParticles = 1000;

        private readonly int size = Vector3.SizeInBytes * 4;

        //size of particles.
        private float delta;

        //random for use in forces.
        private Random r;

        // where the particle is being spit out
        private Vector3 Origin;
        private Vector3 Velocity;
        private float ConeSize;
        private float CutoffDist;
        private float CutOffSpeed;
        private float EnlargeSpeed;
        private float StopDist;

        /// <summary>
        /// Create a new particle system
        /// </summary>
        /// <param name="type"> Specify the type of the particle system needed (WIND or FIRE) </param>
        /// <param name="init_pos"> Specify the generation origin of the particles </param>
        /// <param name="velocity"> Specify the velocity of the particles as a vector </param>
        /// <param name="cone_radius"> Specify the cone radius of the overall cone of the particles </param>
        /// <param name="initial_size"> Specify the size of the particle initially </param>
        /// <param name="cutoff_dist"> Specify the cutoff distance, where the particle starts getting larger and darker </param>
        /// <param name="cutoff_speed"> Specify the cutoff speed, the rate at which particles get darker </param>
        /// <param name="enlarge_speed"> Specify the enlarge speed, the rate at which particles get larger </param>
        /// <param name="stop_dist"> Specify the distance, where the particles stop showing up </param>
        /// <param name="emissionrate"> Specify the emission rate of the particle system </param>
        /// <param name="maxparticles"> Specify the max number of particles emitted at a time </param>
        public ParticleSystem(ParticleSystemType type, 
            Vector3 init_pos,  
            Vector3 velocity, 
            float cone_radius = 2.0f,
            float initial_size = 1.0f,
            float cutoff_dist = 10.0f,
            float cutoff_speed = 0.2f,
            float enlarge_speed = 0.075f,
            float stop_dist = 50.0f,
            int emissionrate = 2, 
            int maxparticles = 1000)
        {
            delta = initial_size;
            emissionRate = emissionrate;
            maxParticles = maxparticles;

            Origin = init_pos;
            Velocity = velocity;
            ConeSize = cone_radius;
            CutOffSpeed = cutoff_speed;
            CutoffDist = cutoff_dist;
            EnlargeSpeed = enlarge_speed;
            StopDist = stop_dist;

            r = new Random();

            for (int i = 0; i < maxParticles; i++)
            {
                Particles.Add(new Particle(Origin, Vector3.UnitY, 1.0f, 0f));
            }

            Verts = new DataStream(Particles.Count * size, true, true);
            Faces = new DataStream(Particles.Count * sizeof(int) * 6,true,true);
            Tex = new DataStream(Particles.Count * size,true,true);

            //calculate for quad
            for (var index = 0; index < Particles.Count; index++)
            {
                var pt = Particles[index];
                var initPos = pt.Position;

                //delta = (float)r.NextDouble() - .7f;

                Vector3 topLeft_Both = new Vector3(initPos.X - delta,initPos.Y + delta, initPos.Z);
                Vector3 bottomRight_Both = new Vector3(initPos.X + delta, initPos.Y - delta, initPos.Z); 
                Vector3 topRight = new Vector3(initPos.X + delta, initPos.Y + delta, initPos.Z);
                Vector3 bottomLeft = new Vector3(initPos.X - delta, initPos.Y - delta, initPos.Z);
                

                Verts.Write(topLeft_Both);
                Verts.Write(bottomRight_Both);
                Verts.Write(topRight);
                Verts.Write(bottomLeft);

                Tex.Write(new Vector3(0f, 0f, 0f));
                Tex.Write(new Vector3(1f, 1f, 0f));
                Tex.Write(new Vector3(0f, 1f, 0f));
                Tex.Write(new Vector3(1f, 0f, 0f));
                
                Faces.Write(index * 4);
                Faces.Write((index * 4) + 2);
                Faces.Write((index * 4) + 1);
                Faces.Write((index * 4));
                Faces.Write((index * 4) + 1);
                Faces.Write((index * 4) + 3);

            }
            Verts.Position = 0;
            Faces.Position = 0;
            Tex.Position = 0;

            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VBO_Tex = new Buffer(GraphicsRenderer.Device, Tex, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            EBO = new Buffer(GraphicsRenderer.Device, Faces, Particles.Count * 6 * sizeof(int), ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            var btcode = ShaderBytecode.CompileFromFile(@"../../Shaders/particle.fx", "VS", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(@"../../Shaders/particle.fx", "PS", "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(btcode);

            Effects = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effects.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 1)
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);

            switch (type)
            {
                case ParticleSystemType.FIRE:
                    TexSRV = CreateTexture(@"../../Particles/fire_red.png");
                    break;
                case ParticleSystemType.WIND:
                    TexSRV = CreateTexture(@"../../Particles/wind.png");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
        }

        /// <summary>
        /// Update the buffers.
        /// </summary>
        public void UpdateBuffer()
        {
            Verts.Position = 0;
            
            for (var index = 0; index < Particles.Count; index++)
            {
                var pt = Particles[index];
                var initPos = pt.Position;

                // change the size of the particle after the cutoff
                float delta_factor = 1.0f;
                float distance = Vector3.Distance(initPos, Origin);

                if (distance > StopDist)
                {
                    delta_factor = 0.0f;
                }
                else if (distance > CutoffDist)
                {
                    delta_factor = delta_factor * (1f + (distance-CutoffDist) * EnlargeSpeed);
                }
                

                // find the positions of the particles
                float resized_delta = delta * delta_factor;
                Vector3 topLeft_Both = new Vector3(initPos.X - resized_delta, initPos.Y + resized_delta, initPos.Z);
                Vector3 bottomRight_Both = new Vector3(initPos.X + resized_delta, initPos.Y - resized_delta, initPos.Z);
                Vector3 topRight = new Vector3(initPos.X + resized_delta, initPos.Y + resized_delta, initPos.Z);
                Vector3 bottomLeft = new Vector3(initPos.X - resized_delta, initPos.Y - resized_delta, initPos.Z);

 
                Verts.Write(topLeft_Both);
                Verts.Write(bottomRight_Both);
                Verts.Write(topRight);
                Verts.Write(bottomLeft);

            }
            Verts.Position = 0;

            VBO_Verts.Dispose();
            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

        }

        /// <summary>
        /// Draw to the screen.
        /// </summary>
        public override void Draw()
        {
            if (!Enabled) return;

            GraphicsRenderer.DeviceContext.InputAssembler.InputLayout = InputLayout;
            GraphicsRenderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBO_Verts, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(VBO_Tex, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetIndexBuffer(EBO,Format.R32_UInt,0);


            Effects.GetVariableByName("CutoffSpeed").AsScalar().Set(CutOffSpeed);
            Effects.GetVariableByName("CutoffDist").AsScalar().Set(CutoffDist);
            Effects.GetVariableByName("gOrigin").AsVector().Set(Origin);
            Effects.GetVariableByName("gWorldViewProj").AsMatrix().SetMatrix(Matrix.Identity * GraphicsManager.ActiveCamera.m_ViewMatrix * GraphicsRenderer.ProjectionMatrix);
            Effects.GetVariableByName("tex_diffuse").AsResource().SetResource(TexSRV);

            //var blendFactor = new Color4(1,1,1,1);
            GraphicsRenderer.DeviceContext.OutputMerger.BlendState = GraphicsRenderer.BlendState;
            //GraphicsRenderer.DeviceContext.OutputMerger.BlendFactor = blendFactor;
            //GraphicsRenderer.DeviceContext.OutputMerger.BlendSampleMask = ~0;


            //turn off depth for now
            GraphicsRenderer.DeviceContext.OutputMerger.DepthStencilState = GraphicsRenderer.DepthStateOff;

            //apply pass
            Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
            GraphicsRenderer.DeviceContext.DrawIndexed(Particles.Count * 6,0,0);

            //turn back on depth 
            GraphicsRenderer.DeviceContext.OutputMerger.DepthStencilState = GraphicsRenderer.DepthState;
            GraphicsRenderer.DeviceContext.OutputMerger.BlendState = null;
        }

        /// <summary>
        /// Update the particles.
        /// </summary>
        public override void Update()
        {
            if (!Enabled)
                return;

            int emissionThisFrame = 0;
            foreach (Particle particle in Particles)
            {
                if (emissionThisFrame < emissionRate)
                {
                    if (particle.LifeRemaining <= 0)
                    {
                        particle.Position = Origin;
                        particle.Acceleration = Vector3.Zero;
                        particle.Velocity = Vector3.Zero;
                        particle.LifeRemaining = r.Range(10f);
                        emissionThisFrame++;
                    }
                }

                Vector3 prevForce = particle.Force;
                particle.Force = new Vector3(
                    10* ConeSize * (r.NextFloat()-0.5f) + Velocity.X + prevForce.X, 
                    10* ConeSize * (r.NextFloat()-0.5f) + Velocity.Y + prevForce.Y, 
                    10* ConeSize * (r.NextFloat()-0.5f) + Velocity.Z + prevForce.Z);

                particle.Update(.001f);
            }
            UpdateBuffer();
        }


        /// <summary>
        /// Create new texture.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private ShaderResourceView CreateTexture(string fileName)
        {
            return File.Exists(fileName) ? ShaderResourceView.FromFile(GraphicsRenderer.Device, fileName) : null;
        }

    }
}