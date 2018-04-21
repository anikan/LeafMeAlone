using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;
using SlimDX.Direct3D11;

namespace Client
{
    // A wrapper of the Geometry class; used to manage the mesh that is used for rendering
    // Also manages the Model class
    class Model
    {
        private static Vector3 defaultDirection = new Vector3(0, 0, -1);

        // active geometry and shader in use
        private Geometry m_ActiveGeo;
        private Shader m_ActiveShader;

        // model matrix used for the rendering
        private Matrix m_ModelMatrix;

        // holds the transformation properties of the model
        public Transform m_Properties;

        // This is a duplicate used to check if there is a need to update the matrix
        private Transform m_PrevProperties;

        // creates a new model; duplicate filepath will be used to detect
        // if a geometry already exists
        public Model(string filePath)
        {
            Load(filePath);

            // set the properties and update the model matrix
            //m_ActiveShader = shader;
            m_Properties.Direction = new Vector3(0, 0, -1);
            m_Properties.Position = new Vector3(0, 0, 0);
            m_Properties.Scale = new Vector3(1, 1, 1);

            m_PrevProperties.Direction = new Vector3(0, 0, 0);
            m_PrevProperties.Position = new Vector3(0, 0, 0);
            m_PrevProperties.Scale = new Vector3(0, 0, 0);
            Update();

        }

        // load the geometry if it is not available, or find
        // reference to the existing geometry if it is found
        public void Load(string filePath)
        {
            if (GraphicsManager.DictGeometry.ContainsKey(filePath))
            {
                m_ActiveGeo = GraphicsManager.DictGeometry[filePath];
            }
            else
            {
                m_ActiveGeo = new Geometry(filePath);
                GraphicsManager.DictGeometry[filePath] = m_ActiveGeo;
            }
        }

        // pass the model matrix to the shader and draw the active geometry
        public void Draw()
        {
           m_ActiveGeo.Draw(m_ModelMatrix);
        }

        // the public interface of the Update function
        // takes in a 'properties', which is used to generate the model matrix
        //public void Update(Transform properties)
        //{
        //    m_Properties = properties;
        //    Update();
        //}

        // update the model matrix based on the properties
        // assume by default the model is facing (0, 0, -1)
        public void Update()
        {
            // update the matrix only if the properties has changes
            if (!m_Properties.Equals(m_PrevProperties))
            {
                // prev properties = current properties
                m_PrevProperties.copyToThis(m_Properties);

                m_ModelMatrix = Matrix.Scaling(m_Properties.Scale); // set the scaling of the model

                // set the rotation based on the three directions
                m_ModelMatrix = Matrix.RotationX(m_Properties.Direction.X) * 
                                Matrix.RotationY(m_Properties.Direction.Y) * 
                                Matrix.RotationZ(m_Properties.Direction.Z) * m_ModelMatrix;

                // set the translation based on the position
                m_ModelMatrix = Matrix.Translation(m_Properties.Position) * m_ModelMatrix;
            }
        }

    }
}
