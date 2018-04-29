using System;
using System.Collections.Generic;
using System.IO;
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
        private static Vector3 defaultDirection = new Vector3(0, 0, 0);

        // active geometry and shader in use
        private Geometry m_ActiveGeo;
        private Shader m_ActiveShader;
        private string m_ActiveShaderPath;

        // model matrix used for the rendering
        private Matrix m_ModelMatrix;

        // holds the transformation properties of the model
        public Transform m_Properties;

        // This is a duplicate used to check if there is a need to update the matrix
        private Transform m_PrevProperties;

        /// <summary>
        /// creates a new model; duplicate filepath will be used to detect
        /// if a geometry already exists. A default shader will be used if not specified
        /// </summary>
        /// <param name="filePath"></param>

        public Model(string filePath)
        {
            //confirm the file exists
            System.Diagnostics.Debug.Assert(File.Exists(filePath));

            Load(filePath);
            m_ModelMatrix = Matrix.Identity;
            // set the properties and update the model matrix
            //m_ActiveShader = shader;
            m_Properties.Rotation = new Vector3(0, 0, 0);
            m_Properties.Position = new Vector3(0, 0, 0);
            m_Properties.Scale = new Vector3(1, 1, 1);

            m_PrevProperties.Rotation = new Vector3(0, 0, 0);
            m_PrevProperties.Position = new Vector3(0, 0, 0);
            m_PrevProperties.Scale = new Vector3(0, 0, 0);
            Update();

            setShader(@"../../defaultShader.fx");
        }

        /// <summary>
        /// Create a new model and specify some particular shader to use for this model
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="shaderPath"></param>
        public Model(string filepath, string shaderPath) : this(filepath)
        {
            setShader(shaderPath);
        }

        /// <summary>
        /// Set the shader for this model. Creates a new shader with default settings if necessary
        /// </summary>
        /// <param name="shaderPath"> filepath to the shader to be set to </param>
        public void setShader(string shaderPath)
        {
            // initialize shader if necessary
            if (GraphicsManager.DictShader.ContainsKey(shaderPath))
            {
                m_ActiveShader = GraphicsManager.DictShader[shaderPath];
            }
            else
            {
                // by default the VS_name is "VS", PS_name is "PS", and we have the position,normal,texcoord element layout
                m_ActiveShader = new Shader(shaderPath);
                GraphicsManager.DictShader[shaderPath] = m_ActiveShader;
            }
        }

        /// <summary>
        /// Can be used to set the active shader to some custom shader that does not use the default setting
        /// </summary>
        /// <param name="shader"> shader to be set to </param>
        public void setShader(Shader shader)
        {
            m_ActiveShader = shader;
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
            m_ActiveShader.UseShader();
            m_ActiveGeo.Draw(m_ModelMatrix, m_ActiveShader);
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
                m_ModelMatrix = m_ModelMatrix * Matrix.RotationX(m_Properties.Rotation.X) * 
                                Matrix.RotationY(m_Properties.Rotation.Y) * 
                                Matrix.RotationZ(m_Properties.Rotation.Z);

                // set the translation based on the position
                m_ModelMatrix = m_ModelMatrix * Matrix.Translation(m_Properties.Position) ;
            }
        }

    }
}
