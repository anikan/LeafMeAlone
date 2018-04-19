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


        private Geometry m_ActiveGeo;
        private Shader m_ActiveShader;
        private Matrix m_ModelMatrix;

        // holds the transformation properties of the model
        public TransformProperties m_Properties;

        // This is a duplicate used to check if there is a need to update the matrix
        public TransformProperties m_PrevProperties;

        // creates a new model; duplicate filepath will be used to detect
        // if a geometry already exists
        public Model(string filePath, Shader shader)
        {
            Load(filePath);

            // set the properties and update the model matrix
            m_ActiveShader = shader;
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

        // set the model matrix and draw 
        public void Draw()
        {
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.InputLayout = m_ActiveGeo.InputLayout;
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_ActiveGeo.VBO, Vector3.SizeInBytes, 0));

            m_ActiveGeo.Effect.GetVariableByName("gWorld").AsMatrix().SetMatrix(m_ModelMatrix);
            m_ActiveGeo.Effect.GetVariableByName("gView").AsMatrix()
                .SetMatrix(GraphicsManager.ActiveCamera.m_ViewMatrix);
            m_ActiveGeo.Effect.GetVariableByName("gProj").AsMatrix().SetMatrix(GraphicsRenderer.ProjectionMatrix);

            m_ActiveGeo.Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
            GraphicsRenderer.Device.ImmediateContext.Draw((int)m_ActiveGeo.Vertices.Length/3,0);
        
        }

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

                m_Properties.Direction =
                    Vector3.Normalize(m_Properties
                        .Direction); // ensure the direction is normalized so that its length is 1
                Vector3 rotationAxis =
                    Vector3.Cross(defaultDirection, m_Properties.Direction); // to get the rotational axis

                // a dot b = |a|*|b|*cos(theta) = cos(theta) when |a| = |b| = 1
                // we can use dot product to find the angle of rotation
                // NOTE: Not sure if we are using radian or degree
                float rotationAngle = (float)Math.Acos(Vector3.Dot(defaultDirection, m_Properties.Direction));

                // set the rotation and translation of the model
                m_ModelMatrix = Matrix.RotationAxis(rotationAxis, rotationAngle) * m_ModelMatrix;
                m_ModelMatrix = Matrix.Translation(m_Properties.Position) * m_ModelMatrix;
            }
        }

    }
}
