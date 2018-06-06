using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// A GameObject that renders on the screen (networked or non-networked).
    /// </summary>
    public abstract class GraphicGameObject : GameObject
    {

        //static particle system which is used for any graphics gameobject to be burned
        public static ParticleSystem Fire;

        // Model that's associated with this object.
        internal Model model;

        // Debug cube to find the pivots of objects.
        public NonNetworkedGameObjectClient PivotCube;


        private float minCap = .3f;


        public Vector3 CurrentTint
        {
            get => model.Tint;
            set
            {
                if (value.X > minCap)
                    model.Tint.X = value.X;
                if (value.Y > minCap)
                    model.Tint.Y = value.Y;
                if (value.Z > minCap)
                    model.Tint.Z = value.Z;
            }
        }
        public Vector3 CurrentHue
        {
            get => model.Hue;
            set
            {
                if (value.X > minCap)
                    model.Hue.X = value.X;
                if (value.Y > minCap)
                    model.Hue.Y = value.Y;
                if (value.Z > minCap)
                    model.Hue.Z = value.Z;
            }

        }




        /// <summary>
        /// Init the particle system for burning. This will only ever run once, if the particle system has not been initialized yet.
        /// </summary>
        private static void InitializeBurning()
        {
            //if the static fire is already initialized dont initialize it.
            if (Fire == null)
            {

                Fire = new FlameThrowerParticleSystem(320f, 2, 10, 0f, 1f, 5f, 1.0f)
                {
                    emissionRate = 5,
                    Enabled = true
                };
                Fire.EnableGeneration(true);
                Fire.Transform.Rotation.X = 90f.ToRadians();
                Fire.SetOrigin(Vector3.Zero);
            }
        }

        /// <summary>
        /// Creates a new graphic game object with no model. Eg a particle system. 
        /// Note: if you call initializeburning here you get infinite recursion because a particle system is also a graphics object.
        /// </summary>
        protected GraphicGameObject() : base()
        {

            // Check if debug mode is on and this isn't a particle system or another cube.
            if (Constants.PIVOT_DEBUG && !(this is ParticleSystem) && !(this is MapTile))
            {
                // Create a new cube at the pivot.
                PivotCube = new MapTile();
                PivotCube.Transform.Scale.Y = 20.0f;

            }


        }

        /// <summary>
        /// Creates a new graphic game object with specified model and position.
        /// </summary>
        /// <param name="modelPath">Path to the model for this GameObject.</param>
        protected GraphicGameObject(string modelPath) : base()
        {
            SetModel(modelPath);
            InitializeBurning();

            // Check if debug mode is on and this isn't a particle system or another cube.
            if (Constants.PIVOT_DEBUG && !(this is ParticleSystem) && !(this is MapTile))
            {

                // Create a new cube at the pivot.
                PivotCube = new MapTile();
                PivotCube.Transform.Scale.Y = 20.0f;

            }

        }

        /// <summary>
        /// Update step of this object.
        /// </summary>
        /// <param name="deltaTime">Time since last frame.</param>
        public override void Update(float deltaTime)
        {
            if (model != null)
            {
                model.m_Properties = Transform;
                model.Update(deltaTime);
            }

            // If we're debugging, move the pivot cube.
            if (PivotCube != null)
            {
                PivotCube.Transform.Position = Transform.Position;
                PivotCube.Update(deltaTime);
            }

        }

        /// <summary>
        /// Draw step of the object, renders it on screen.
        /// </summary>
        public virtual void Draw()
        {
            model?.Draw();
            
            // If we're debugging, draw the pivot cube.
            PivotCube?.Draw();
        }

        /// <summary>
        /// Sets the model of this object.
        /// </summary>
        /// <param name="filePath">Path of the model.</param>
        public void SetModel(string filePath)
        {
            //Console.WriteLine(File.Exists(filePath));
            model = new Model(filePath,false,true);
            Name = filePath.Split('.')[0];
        }

        /// <summary>
        /// Destroy this GameObject and remove any references.
        /// </summary>
        public override void Die()
        {
            GameClient.instance.Destroy(this);
        }
    }
}
