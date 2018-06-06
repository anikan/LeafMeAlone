using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Client
{
    class InverseParticleSystem : BaseParticleSystem
    {
        public bool Enabled = true;

        //list of particles in the system.
        private List<Particle> Particles = new List<Particle>();

        //create buffers
        private Buffer VBO_Verts, VBO_Tex, VBO_Origin;
        private Buffer EBO;
        private DataStream Tex, Faces;

        //create texture
        private ShaderResourceView TexSRV;


        //how fast particles are emitted
        public int emissionRate = 1;

        //max number of particles.
        public int maxParticles = 1000;


        public int Num_CurrentlyActiveParticles = 0;

        private readonly int size = Vector3.SizeInBytes * 4;

        //size of particles.
        private float delta;

        //random for use in forces.
        private static Random r;

        // where the particle is being absorbed
        private Vector3 EndPosition;
        private Vector3 InitVelocity;
        private float Speed;
        private float ConeSize;
        private float CutoffDist;
        private float CutOffSpeed;
        private float ShrinkSpeed;
        private float OriginDist;
        private int AlphaCutoffOnly;
        private bool DisableRewind;

        private bool ShouldGenerate;
        private bool ShouldRender;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TexturePath"> The filepath to the texture file used for the particle system </param>
        /// <param name="end_pos"> the location that all particles end up disappearing to </param>
        /// <param name="velocity"> the velocity of the particles (also used to determine direction and thus starting location) </param>
        /// <param name="alpha_cutoff_only"> only let particles to have alpha cutoff </param>
        /// <param name="disable_rewind"> disable particles to flow backwards </param>
        /// <param name="cone_radius"> cone radius of the particle system </param>
        /// <param name="initial_size"> the initial size of the particles </param>
        /// <param name="cutoff_dist"> the cutoff distance </param>
        /// <param name="cutoff_speed"> the cutoff speed </param>
        /// <param name="shrink_speed"> the shrinking speed </param>
        /// <param name="origin_dist"> the length  of the beam of particle system </param>
        /// <param name="emissionrate"> the rate of emission per frame </param>
        /// <param name="maxparticles"> the max number of particles allowed at a time </param>
        public InverseParticleSystem(string TexturePath, 
            Vector3 end_pos, 
            Vector3 velocity,
            bool alpha_cutoff_only = false, 
            bool disable_rewind = true, 
            float cone_radius = 10, 
            float initial_size = 2.5f, 
            float cutoff_dist = 0, 
            float cutoff_speed = 0.2f, 
            float shrink_speed = 0.07f, 
            float origin_dist = 10, 
            int emissionrate = 1, 
            int maxparticles = 100) 
        {
            Transform.Scale = new Vector3(1, 1, 1);
            delta = initial_size;
            emissionRate = emissionrate;
            maxParticles = maxparticles;

            InitVelocity = velocity;
            Speed = Vector3.Distance(Vector3.Zero, InitVelocity);
            EndPosition = end_pos;
            ConeSize = cone_radius;
            CutOffSpeed = cutoff_speed;
            CutoffDist = cutoff_dist;
            ShrinkSpeed = shrink_speed;
            OriginDist = origin_dist;
            AlphaCutoffOnly = alpha_cutoff_only ? 1 : 0;
            DisableRewind = disable_rewind;
            ShouldGenerate = true;
            ShouldRender = true;

            if (r == null)
                r = new Random();

            Vector3 offset = Vector3.Normalize(-velocity) * OriginDist;

            for (int i = 0; i < maxParticles; i++)
            {
                Vector3 coneOffset = Vector3.Normalize(Vector3.Cross(-velocity, Vector3.UnitY)) * (ConeSize * r.NextFloat());
                Matrix rotation = Matrix.RotationAxis(Vector3.Normalize(-velocity), r.NextFloat() * 2 * (float) Math.PI);
                Vector3 origin = EndPosition + offset + Vector3.TransformCoordinate(coneOffset, rotation);
                
                Particles.Add(new Particle(origin, InitVelocity, 1.0f, 0f));
            }

            Faces = new DataStream(Particles.Count * sizeof(uint) * 6, true, true);
            Tex = new DataStream(Particles.Count * size, true, true);

            var vert_desc = new BufferDescription(Particles.Count * size, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            var orig_desc = new BufferDescription(Particles.Count * size, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            VBO_Verts = new Buffer(GraphicsRenderer.Device, vert_desc);
            VBO_Origin = new Buffer(GraphicsRenderer.Device, orig_desc);

            //calculate for quad
            for (var index = 0; index < Particles.Count; index++)
            {

                Tex.Write(new Vector3(0f, 0f, 0f));
                Tex.Write(new Vector3(1f, 1f, 0f));
                Tex.Write(new Vector3(0f, 1f, 0f));
                Tex.Write(new Vector3(1f, 0f, 0f));

                Faces.Write((uint)(index * 4));
                Faces.Write((uint)((index * 4) + 2));
                Faces.Write((uint)((index * 4) + 1));
                Faces.Write((uint)((index * 4)));
                Faces.Write((uint)((index * 4) + 1));
                Faces.Write((uint)((index * 4) + 3));

            }
            Faces.Position = 0;
            Tex.Position = 0;

            VBO_Tex = new Buffer(GraphicsRenderer.Device, Tex, Particles.Count * size, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            EBO = new Buffer(GraphicsRenderer.Device, Faces, Particles.Count * 6 * sizeof(uint), ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            TexSRV = Utility.CreateTexture(TexturePath, GraphicsRenderer.Device);
        }

        private Vector3 findRandOrigin(Vector3 endPoint, Vector3 velocity, float coneRadius, float dist)
        {

            Vector3 offset = Vector3.Normalize(-velocity) * dist;
            Vector3 coneOffset = Vector3.Normalize(Vector3.Cross(-velocity, Vector3.UnitY)) * ( r.NextFloat() * coneRadius );
            Matrix rotation = Matrix.RotationAxis(Vector3.Normalize(-velocity), r.NextFloat() * 2 * (float)Math.PI);
            Vector3 origin = endPoint + offset + Vector3.TransformCoordinate(coneOffset, rotation);
            return origin;
        }

        public void DrawTransform(Transform parent)
        {
            Transform.CopyToThis(parent);
            Draw();
        }

        /// <summary>
        /// Draw to the screen.
        /// </summary>
        public override void Draw()
        {
            if (!Enabled) return;
            if (!ShouldRender) return;

            GraphicsRenderer.DeviceContext.InputAssembler.InputLayout = ParticleSystemManager.InputLayout;
            GraphicsRenderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBO_Verts, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(VBO_Tex, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(2, new VertexBufferBinding(VBO_Origin, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetIndexBuffer(EBO, Format.R32_UInt, 0);

            ParticleSystemManager.Effects.GetVariableByName("AlphaCutoffOnly").AsScalar().Set(AlphaCutoffOnly);
            ParticleSystemManager.Effects.GetVariableByName("AlphaCutoffOnly").AsScalar().Set(AlphaCutoffOnly);
            ParticleSystemManager.Effects.GetVariableByName("CutoffSpeed").AsScalar().Set(CutOffSpeed);
            ParticleSystemManager.Effects.GetVariableByName("CutoffDist").AsScalar().Set(CutoffDist);
            ParticleSystemManager.Effects.GetVariableByName("gWorldViewProj").AsMatrix().SetMatrix(Transform.AsMatrix() * GraphicsManager.ActiveCamera.m_ViewMatrix * GraphicsRenderer.ProjectionMatrix);
            ParticleSystemManager.Effects.GetVariableByName("tex_diffuse").AsResource().SetResource(TexSRV);

            //set blend state
            GraphicsRenderer.DeviceContext.OutputMerger.BlendState = GraphicsRenderer.BlendState;

            //apply pass
            ParticleSystemManager.Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
            GraphicsRenderer.DeviceContext.DrawIndexed(Particles.Count * 6, 0, 0);

            GraphicsRenderer.DeviceContext.OutputMerger.BlendState = null;
        }

        /// <summary>
        /// Update the buffers.
        /// </summary>
        public void UpdateBuffer()
        {

            var vertBox = GraphicsRenderer.DeviceContext.MapSubresource(VBO_Verts, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            var originBox = GraphicsRenderer.DeviceContext.MapSubresource(VBO_Origin, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);


            foreach (var pt in Particles)
            {
                var initPos = pt.Position;

                // change the size of the particle after the cutoff
                float delta_factor = 1.0f;
                float distance = Vector3.Distance(initPos, pt.Origin);

                // if the distance is so great that we need to stop rendering, make it super small
                if (distance > OriginDist)
                {
                    delta_factor = 0.0f;
                }

                // if the distance is greater than cutoff, increase the size for a puff-up effect
                else if (distance > CutoffDist)
                {
                    delta_factor = delta_factor * (1f / (1f + (distance - CutoffDist) * ShrinkSpeed));
                }

                // find the positions of the particles
                float resized_delta = delta * delta_factor;
                Vector3 topLeft_Both = new Vector3(initPos.X - resized_delta, initPos.Y + resized_delta, initPos.Z);
                Vector3 bottomRight_Both = new Vector3(initPos.X + resized_delta, initPos.Y - resized_delta, initPos.Z);
                Vector3 topRight = new Vector3(initPos.X + resized_delta, initPos.Y + resized_delta, initPos.Z);
                Vector3 bottomLeft = new Vector3(initPos.X - resized_delta, initPos.Y - resized_delta, initPos.Z);

                vertBox.Data.Write(topLeft_Both);
                vertBox.Data.Write(bottomRight_Both);
                vertBox.Data.Write(topRight);
                vertBox.Data.Write(bottomLeft);

                originBox.Data.Write(EndPosition);
                originBox.Data.Write(EndPosition);
                originBox.Data.Write(EndPosition);
                originBox.Data.Write(EndPosition);
            }
            GraphicsRenderer.DeviceContext.UnmapSubresource(VBO_Verts, 0);
            GraphicsRenderer.DeviceContext.UnmapSubresource(VBO_Origin, 0);
        }


        /// <summary>
        /// Update the particles.
        /// </summary>
        public override void Update(float deltaTime)
        {
            if (!Enabled)
                return;

            int emissionThisFrame = 0;
            Num_CurrentlyActiveParticles = 0;
            foreach (Particle particle in Particles)
            {

                //This should be true if we want to prevent particles from moving behind us.
                if (DisableRewind)
                {
                    float cosAngle = Vector3.Dot(Vector3.Normalize(particle.InitialVelocity), Vector3.Normalize(particle.Velocity));
                    //Console.WriteLine(cosAngle);
                    if (cosAngle < 0f)
                    {
                        particle.Force = Vector3.Zero;
                        particle.Velocity = Vector3.Zero;
                        particle.LifeRemaining = -1;
                    }
                }

                float front = Vector3.Dot(Vector3.Normalize(particle.InitialVelocity),
                    Vector3.Normalize(EndPosition - particle.Position) );
                if (particle.LifeRemaining <= 0 || front < 0f)
                {
                    if (emissionThisFrame < emissionRate && ShouldGenerate)
                    {
                        particle.Origin = findRandOrigin(EndPosition, InitVelocity, ConeSize, OriginDist);
                        particle.Position = particle.Origin;
                        particle.Velocity = Vector3.Normalize(EndPosition - particle.Position) * Speed;
                        particle.InitialVelocity = particle.Velocity;
                        particle.LifeRemaining = r.Range(10f);
                        emissionThisFrame++;
                        Num_CurrentlyActiveParticles++;
                    }
                }
                else
                {
                    Num_CurrentlyActiveParticles++;
                }
                
                particle.Velocity = Vector3.Normalize(EndPosition - particle.Position) * Speed;
                particle.Update(deltaTime, true);
            }

            ShouldRender = (Num_CurrentlyActiveParticles != 0);
            UpdateBuffer();
        }

        /// <summary>
        /// Reset the particle system so that the particles are reset to the origin
        /// </summary>
        public void ResetSystem()
        {
            foreach (var particle in Particles)
            {
                particle.Origin = findRandOrigin(EndPosition, InitVelocity, ConeSize, OriginDist);
                particle.Position = particle.Origin;
                particle.Velocity = InitVelocity;
                particle.LifeRemaining = r.Range(10f);
                particle.InitialVelocity = InitVelocity;
            }

            UpdateBuffer();
        }

        #region GettersAndSetters
        /// <summary>
        /// Set the origin of the particles in the system
        /// </summary>
        /// <param name="pos"></param>
        public void SetEndposition(Vector3 pos)
        {
            EndPosition = pos;
        }

        /// <summary>
        /// Set the velocity of the particles in the system
        /// </summary>
        /// <param name="vel"></param>
        public void SetVelocity(Vector3 vel)
        {
            InitVelocity = vel;
            Speed = Vector3.Distance(Vector3.Zero, vel);
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


        #endregion
    }


}
