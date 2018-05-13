using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    /// <summary>
    /// A GameObject that renders on the screen (networked or non-networked).
    /// </summary>
    public abstract class GraphicGameObject : GameObject
    {
        // Model that's associated with this object.
        private Model model;

        /// <summary>
        /// Creates a new graphic game object with no model.
        /// </summary>
        public GraphicGameObject() : base()
        {
  
        }

        /// <summary>
        /// Creates a new graphic game object with specified model and position.
        /// </summary>
        /// <param name="modelPath">Path to the model for this GameObject.</param>
        public GraphicGameObject(string modelPath) : base()
        {
            SetModel(modelPath);
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
            model.Update(deltaTime);
        }

        /// <summary>
        /// Draw step of the object, renders it on screen.
        /// </summary>
        public virtual void Draw()
        {
            model?.Draw();
        }

        /// <summary>
        /// Sets the model of this object.
        /// </summary>
        /// <param name="filePath">Path of the model.</param>
        public void SetModel(string filePath)
        {
            //Console.WriteLine(File.Exists(filePath));
            model = new Model(filePath);
            Name = filePath.Split('.')[0];
        }

        /// <summary>
        /// Destroy this GameObject and remove any references.
        /// </summary>
        public override void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
