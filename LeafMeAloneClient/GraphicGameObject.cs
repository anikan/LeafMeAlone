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
        public Model model;

        // Debug cube to find the pivots of objects.
        private NonNetworkedGameObjectClient PivotCube;


        private float minCap = .3f;
        public Vector3 CurrentTint
        {
            get => model.Tint;
            set
            {
                if(model.Tint.X > minCap)
                    model.Tint.X = value.X;
                if (model.Tint.Y > minCap)
                    model.Tint.Y = value.Y;
                if (model.Tint.Z > minCap)
                    model.Tint.Z = value.Z;
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

                Fire = new FlameThrowerParticleSystem(2, 10, 2.5f, 1f, 5f)
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

            //if the object is currently burning, draw the fire on them.
            if (Burning)
            {
                Transform t = new Transform {Position = Transform.Position,Scale =  new Vector3(1,1,1)};
                Fire?.DrawTransform(t);
            }

            // If we're debugging, draw the pivot cube.
            if (PivotCube != null)
            {
                PivotCube.Draw();
            }
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
        public override void Destroy()
        {
            GameClient.instance.Destroy(this);
        }
    }
}
