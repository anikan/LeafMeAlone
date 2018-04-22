using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Assimp.Configs;
using Shared;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;
namespace Client
{
    class Geometry
    {

        /// <summary>
        /// Vertex Buffer, Index Buffer
        /// </summary>
        private Buffer VBO, EBO;

        /// <summary>
        /// Data streams hold the actual Vertices and Faces.
        /// </summary>
        private DataStream Vertices, Normals, Faces;

        /// <summary>
        /// Assimp scene containing the loaded model.
        /// </summary>
        private Scene scene;

        /// <summary>
        /// Assimp importer.
        /// </summary>
        private AssimpContext importer;

        /// <summary>
        /// Elements are just used to put things into the shader.
        /// </summary>
        private InputElement[] Elements;

        private InputLayout InputLayout;


        /// <summary>
        /// something to do with shaders
        /// </summary>
        private Effect Effect;

        private EffectPass Pass;

        /// <summary>
        /// Create a new geometry given filename
        /// </summary>
        /// <param name="fileName"></param>
        public Geometry(string fileName)
        {

            //Create new importer.
            importer = new AssimpContext();

            //import the file
            scene = importer.ImportFile(fileName);

            //sizes for the loaded object.
            int vertSize = 0, normSize = 0, faceSize = 0;

            //loop through sizes and count them.
            scene.Meshes.ForEach(mesh =>
            {
                vertSize += mesh.VertexCount * Vector3.SizeInBytes;
                normSize += mesh.Normals.Count * Vector3.SizeInBytes;
                faceSize += mesh.FaceCount * mesh.Faces[0].IndexCount * sizeof(int);
            });

            //make sure scene not null
            if (scene == null)
                throw new FileNotFoundException();

            //create new datastreams.
            Vertices = new DataStream(vertSize, true, true);
            Normals = new DataStream(normSize, true, true);
            Faces = new DataStream(faceSize, true, true);

            //loop through vertices, normals, and faces and put them in the datastreams.
            foreach (Mesh sceneMesh in scene.Meshes)
            {
                sceneMesh.Vertices.ForEach(vertex =>
                {
                    Vertices.Write(vertex.ToVector3());
                });
                sceneMesh.Normals.ForEach(normal =>
                {
                    Normals.Write(normal.ToVector3());
                });
                sceneMesh.Faces.ForEach(face =>
                {
                    Faces.WriteRange(face.Indices.ToArray());
                });

            }

            //reset positions in data stream
            Vertices.Position = 0;
            Normals.Position = 0;
            Faces.Position = 0;


            //create vertex vbo and faces ebo.
            VBO = new Buffer(GraphicsRenderer.Device, Vertices, vertSize, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            EBO = new Buffer(GraphicsRenderer.Device, Faces, faceSize, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            #region Shader Code -- To Move

            var btcode = ShaderBytecode.CompileFromFile(@"../../tester.fx", "VShader", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(@"../../tester.fx", "Render", "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(btcode);

            Effect = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effect.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0)
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);

            #endregion
        }


        public void Draw(Matrix modelMatrix)
        {
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.InputLayout = InputLayout;
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBO, Vector3.SizeInBytes, 0));

            Effect.GetVariableByName("gWorld").AsMatrix().SetMatrix(modelMatrix);
            Effect.GetVariableByName("gView").AsMatrix()
                .SetMatrix(GraphicsManager.ActiveCamera.m_ViewMatrix);
            Effect.GetVariableByName("gProj").AsMatrix().SetMatrix(GraphicsRenderer.ProjectionMatrix);

            Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
            GraphicsRenderer.Device.ImmediateContext.Draw((int)Vertices.Length / 3, 0);
        }
    }
}
