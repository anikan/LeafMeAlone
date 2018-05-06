using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    /// <summary>
    /// Client-side GameObject.
    /// </summary>
    public abstract class GameObjectClient : GameObject
    {
        // Model that's associated with this object.
        private Model model;

        /// <summary>
        /// Constructs a new local GameObject with a model at the specified path.
        /// </summary>
        /// <param name="modelPath">Path to this gameobject's model.</param>
        protected GameObjectClient(string modelPath) : base()
        {
            SetModel(modelPath);
        }

        /// <summary>
        /// Constructs a new local GameObject with a model at the specified path and a specified position.
        /// </summary>
        /// <param name="modelPath">Paht to this gameobject's model.</param>
        /// <param name="startTransform">Starting transform of this object.</param>
        protected GameObjectClient(string modelPath, Transform startTransform) : base(startTransform)
        {
            SetModel(modelPath);
        }

        /// <summary>
        /// Sets the model of this object.
        /// </summary>
        /// <param name="filePath">Path of the model.</param>
        public void SetModel(string filePath)
        {
            model = new Model(filePath);
            Name = filePath.Split('.')[0];
        }

        /// <summary>
        /// Update step of this object.
        /// </summary>
        /// <param name="deltaTime">Time since last frame.</param>
        public override void Update(float deltaTime)
        {
            if (model == null)
                return;
            model.m_Properties = Transform;
            model.Update();
        }

        /// <summary>
        /// Draw step of the object, renders it on screen.
        /// </summary>
        public virtual void Draw()
        {
            model?.Draw();
        }

        /// <summary>
        /// Updates this object from a network packet.
        /// </summary>
        /// <param name="packet">Packet to update from.</param>
        public abstract void UpdateFromPacket(Packet packet);

        /// <summary>
        /// Destroys this object and removes any references.
        /// </summary>
        public override void Destroy()
        {
            // What to do?!
        }
    }
}
