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
using SlimDX.XAudio2;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Client
{
    public enum ParticleSystemType
    {
        FIRE,
        WIND
    }

    /// <summary>
    /// Particle system object, only exists client-side.
    /// </summary>
    public class ParticleSystem : NonNetworkedGameObjectClient
    {
        public bool Enabled = true;

        //list of particles in the system.
        private List<Particle> Particles = new List<Particle>();

        //create buffers
        private Buffer VBO_Verts, VBO_Tex, VBO_Origin;
        private Buffer EBO;
        private DataStream Verts, Tex, Faces, StartingLocations;
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
        private Vector3 GenerationOrigin;
        private Vector3 Acceleration;
        private Vector3 InitVelocity;
        private float ConeSize;
        private float CutoffDist;
        private float CutOffSpeed;
        private float EnlargeSpeed;
        private float StopDist;
        private int AlphaCutoffOnly;
        private bool DisableRewind;
        private ParticleSystemType Type;

        private bool ShouldGenerate;
        private bool ShouldRender;

        /// <summary>
        /// Create a new particle system
        /// </summary>
        /// <param name="type"> Specify the type of the particle system needed (WIND or FIRE) </param>
        /// <param name="init_pos"> Specify the generation origin of the particles </param>
        /// <param name="acceleration"> Specify the acceleration of the particles as a vector </param>
        /// <param name="init_velocity"> Specify the initial velocity of the particles </param>
        /// <param name="alpha_cutoff_only"> Disable cutting off color other than alpha </param>
        /// <param name="disable_rewind"> Disable particles from moving backwards from the direction of initial velocity </param>
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
            Vector3 acceleration, 
            Vector3 init_velocity,
            bool alpha_cutoff_only = false,
            bool disable_rewind = true,
            float cone_radius = 20.0f,
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

            GenerationOrigin = init_pos;
            Acceleration = acceleration;
            InitVelocity = init_velocity;
            ConeSize = cone_radius;
            CutOffSpeed = cutoff_speed;
            CutoffDist = cutoff_dist;
            EnlargeSpeed = enlarge_speed;
            StopDist = stop_dist;
            AlphaCutoffOnly = alpha_cutoff_only? 1:0;
            DisableRewind = disable_rewind;
            ShouldGenerate = true;
            ShouldRender = true;
            Type = type;

            r = new Random();

            for (int i = 0; i < maxParticles; i++)
            {
                Particles.Add(new Particle(GenerationOrigin, InitVelocity, Acceleration, 1.0f, 0f));
            }

            Verts = new DataStream(Particles.Count * size, true, true);
            StartingLocations = new DataStream(Particles.Count * size, true, true);
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

                StartingLocations.Write(pt.Origin);
                StartingLocations.Write(pt.Origin);
                StartingLocations.Write(pt.Origin);
                StartingLocations.Write(pt.Origin);

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
            StartingLocations.Position = 0;

            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VBO_Tex = new Buffer(GraphicsRenderer.Device, Tex, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VBO_Origin = new Buffer(GraphicsRenderer.Device, StartingLocations, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            EBO = new Buffer(GraphicsRenderer.Device, Faces, Particles.Count * 6 * sizeof(int), ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            var btcode = ShaderBytecode.CompileFromFile(FileManager.ParticleShader, "VS", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(FileManager.ParticleShader, "PS", "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(btcode);

            Effects = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effects.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 1),
                new InputElement("ORIGIN", 0, Format.R32G32B32_Float, 2)
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);

            switch (type)
            {
                case ParticleSystemType.FIRE:
                    TexSRV = CreateTexture(FileManager.FireTexture);
                    break;
                case ParticleSystemType.WIND:
                    TexSRV = CreateTexture(FileManager.WindTexture);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            ResetSystem();
        }

        /// <summary>
        /// Update the buffers.
        /// </summary>
        public void UpdateBuffer()
        {
            Verts.Position = 0;
            StartingLocations.Position = 0;
            
            for (var index = 0; index < Particles.Count; index++)
            {
                var pt = Particles[index];
                var initPos = pt.Position;

                // change the size of the particle after the cutoff
                float delta_factor = 1.0f;
                float distance = Vector3.Distance(initPos, pt.Origin);

                // if the distance is so great that we need to stop rendering, make it super small
                if (distance > StopDist)
                {
                    delta_factor = 0.0f;
                }

                // if the distance is greater than cutoff, increase the size for a puff-up effect
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

                StartingLocations.Write(pt.Origin);
                StartingLocations.Write(pt.Origin);
                StartingLocations.Write(pt.Origin);
                StartingLocations.Write(pt.Origin);
            }
            Verts.Position = 0;
            StartingLocations.Position = 0;

            VBO_Verts.Dispose();
            VBO_Verts = new Buffer(GraphicsRenderer.Device, Verts, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VBO_Origin.Dispose();
            VBO_Origin = new Buffer(GraphicsRenderer.Device, StartingLocations, Particles.Count * size, ResourceUsage.Default, BindFlags.None, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        /// <summary>
        /// Draw to the screen.
        /// </summary>
        public override void Draw()
        {
            if (!Enabled) return;
            if (!ShouldRender) return;

            GraphicsRenderer.DeviceContext.InputAssembler.InputLayout = InputLayout;
            GraphicsRenderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBO_Verts, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(VBO_Tex, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(2, new VertexBufferBinding(VBO_Origin, Vector3.SizeInBytes, 0));
            //GraphicsRenderer.DeviceContext.InputAssembler.SetIndexBuffer(EBO,Format.R32_UInt,0);

            Effects.GetVariableByName("AlphaCutoffOnly").AsScalar().Set(AlphaCutoffOnly);
            Effects.GetVariableByName("CutoffSpeed").AsScalar().Set(CutOffSpeed);
            Effects.GetVariableByName("CutoffDist").AsScalar().Set(CutoffDist);
            Effects.GetVariableByName("gOrigin").AsVector().Set(GenerationOrigin);
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
        public override void Update(float deltaTime)
        {
            if (!Enabled)
                return;
            //if (!ShouldRender && !ShouldGenerate)
            //    return;

            int emissionThisFrame = 0;
            int activeParticles = 0;
            foreach (Particle particle in Particles)
            {
                if (particle.LifeRemaining <= 0 || Vector3.Distance(particle.Position, particle.Origin) > StopDist)
                {
                    if ( emissionThisFrame < emissionRate && ShouldGenerate)
                    {
                        particle.InitAcceleration = Acceleration;
                        particle.Origin = GenerationOrigin;
                        particle.Position = GenerationOrigin;
                        particle.Acceleration = Vector3.Zero;
                        particle.Velocity = InitVelocity;
                        particle.InitVelocity = InitVelocity;
                        particle.LifeRemaining = r.Range(10f);
                        emissionThisFrame++;
                        activeParticles++;
                    }
                }
                else
                {
                    activeParticles++;
                }

                Vector3 prevForce = particle.Force;
                particle.Force = new Vector3(
                    ConeSize * (r.NextFloat() - 0.5f) + particle.InitAcceleration.X + prevForce.X,
                    ConeSize * (r.NextFloat() - 0.5f) + particle.InitAcceleration.Y + prevForce.Y,
                    ConeSize * (r.NextFloat() - 0.5f) + particle.InitAcceleration.Z + prevForce.Z);

                if (DisableRewind)
                {
                    float cosAngle = Vector3.Dot(Vector3.Normalize(particle.InitVelocity), Vector3.Normalize(particle.Velocity));
                    if (cosAngle < 0f)
                    {
                        particle.Force = Vector3.Zero;
                        particle.Velocity = Vector3.Zero;
                        particle.LifeRemaining = 0;
                    }
                }
                particle.Update(deltaTime);
            }

            ShouldRender = (activeParticles != 0);
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

        /// <summary>
        /// Reset the particle system so that the particles are reset to the origin
        /// </summary>
        public void ResetSystem()
        {
            foreach (var particle in Particles)
            {
                particle.InitAcceleration = Acceleration;
                particle.Origin = GenerationOrigin;
                particle.Position = GenerationOrigin;
                particle.Acceleration = Vector3.Zero;
                particle.Velocity = InitVelocity;
                particle.LifeRemaining = r.Range(10f);
            }

            UpdateBuffer();
        }

        /// <summary>
        /// Set the origin of the particles in the system
        /// </summary>
        /// <param name="pos"></param>
        public void SetOrigin(Vector3 pos)
        {
            GenerationOrigin = pos;
        }

        /// <summary>
        /// Set the velocity of the particles in the system
        /// </summary>
        /// <param name="vel"></param>
        public void SetVelocity(Vector3 vel)
        {
            InitVelocity = vel;
        }

        /// <summary>
        /// Set the acceleration of the particles in the system
        /// </summary>
        /// <param name="acc"></param>
        public void SetAcceleration(Vector3 acc)
        {
            Acceleration = acc;
        }

        /// <summary>
        /// Use this to specify if you want the particle system to start generation or not
        /// </summary>
        /// <param name="enable"></param>
        public void EnableGeneration(bool enable)
        {
            //Enabled = enable;
            ShouldGenerate = enable;
        }

        /// <summary>
        /// Use this to see the particle system is currently generating particles or not
        /// </summary>
        /// <returns></returns>
        public bool IsGenerating()
        {
            return ShouldGenerate;
        }

        /// <summary>
        /// Use this to see if the particle system is currently rendering any particle
        /// This will be independent of whether Draw() is called or not
        /// </summary>
        /// <returns></returns>
        public bool IsRendering()
        {
            return ShouldRender;
        }
    }
}