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
        /// Vertex Buffer,Normal Buffer Index Buffer
        /// </summary>
        private List<Buffer> VBOPositions, VBONormals, EBO;

        /// <summary>
        /// Data streams hold the actual Vertices and Faces.
        /// </summary>
        private List<DataStream> Vertices, Normals, Faces;

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

        //sizes for the loaded object.
        private List<int> vertSize, normSize, faceSize;

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

            //make sure scene not null
            if (scene == null)
                throw new FileNotFoundException();

            //loop through sizes and count them.
            vertSize = new List<int>(scene.MeshCount);
            normSize = new List<int>(scene.MeshCount);
            faceSize = new List<int>(scene.MeshCount);
            vertSize.AddRange(Enumerable.Repeat(0, scene.MeshCount));
            normSize.AddRange(Enumerable.Repeat(0, scene.MeshCount));
            faceSize.AddRange(Enumerable.Repeat(0, scene.MeshCount));
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                vertSize[idx] = scene.Meshes[idx].VertexCount * Vector3.SizeInBytes;
                normSize[idx] = scene.Meshes[idx].Normals.Count * Vector3.SizeInBytes;
                faceSize[idx] = scene.Meshes[idx].FaceCount * scene.Meshes[idx].Faces[0].IndexCount * sizeof(int);
            }
            
            Vertices = new List<DataStream>(scene.MeshCount);
            Normals = new List<DataStream>(scene.MeshCount);
            Faces = new List<DataStream>(scene.MeshCount);
            VBOPositions = new List<Buffer>(scene.MeshCount);
            VBONormals = new List<Buffer>(scene.MeshCount);
            EBO = new List<Buffer>(scene.MeshCount);
            Vertices.AddRange(Enumerable.Repeat((DataStream) null, scene.MeshCount));
            Normals.AddRange(Enumerable.Repeat((DataStream)null, scene.MeshCount));
            Faces.AddRange(Enumerable.Repeat((DataStream)null, scene.MeshCount));
            VBOPositions.AddRange(Enumerable.Repeat((Buffer)null, scene.MeshCount));
            VBONormals.AddRange(Enumerable.Repeat((Buffer) null, scene.MeshCount));
            EBO.AddRange(Enumerable.Repeat((Buffer)null, scene.MeshCount));
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                //create new datastreams.
                Vertices[idx] = new DataStream(vertSize[idx], true, true);
                Normals[idx] = new DataStream(normSize[idx], true, true);
                Faces[idx] = new DataStream(faceSize[idx], true, true);

                scene.Meshes[idx].Vertices.ForEach(vertex =>
                {
                    Vertices[idx].Write(vertex.ToVector3());
                });
                scene.Meshes[idx].Normals.ForEach(normal =>
                {
                    Normals[idx].Write(normal.ToVector3());
                });
                scene.Meshes[idx].Faces.ForEach(face =>
                {
                    Faces[idx].WriteRange(face.Indices.ToArray());
                });

                //create vertex vbo and faces ebo.
                VBOPositions[idx] = new Buffer(GraphicsRenderer.Device, Vertices[idx], vertSize[idx], ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                VBONormals[idx] = new Buffer(GraphicsRenderer.Device, Normals[idx], normSize[idx], ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

                var ibd = new BufferDescription(
                    faceSize[idx],
                    ResourceUsage.Immutable,
                    BindFlags.IndexBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None,
                    0);
                EBO[idx] = new Buffer(GraphicsRenderer.Device, Faces[idx], ibd);
            }


            #region Shader Code -- To Move

            var btcode = ShaderBytecode.CompileFromFile(@"../../tester.fx", "VS", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(@"../../tester.fx", "Render", "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(btcode);

            Effect = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effect.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 1),
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);

            #endregion
        }


        public void Draw(Matrix modelMatrix)
        {
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.InputLayout = InputLayout;
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            Effect.GetVariableByName("gWorld").AsMatrix().SetMatrix(modelMatrix);
            Effect.GetVariableByName("gView").AsMatrix()
                .SetMatrix(GraphicsManager.ActiveCamera.m_ViewMatrix);
            Effect.GetVariableByName("gProj").AsMatrix().SetMatrix(GraphicsRenderer.ProjectionMatrix);

            for (int i = 0; i < scene.MeshCount; i++)
            {
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(VBOPositions[i], Vector3.SizeInBytes, 0));
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(1,
                    new VertexBufferBinding(VBONormals[i], Vector3.SizeInBytes, 0));
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetIndexBuffer(EBO[i], Format.R32_UInt, 0);

                Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
                GraphicsRenderer.Device.ImmediateContext.DrawIndexed(faceSize[i] / sizeof(int), 0, 0);
                //GraphicsRenderer.Device.ImmediateContext.Draw(vertSize / sizeof(float), 0);
            }
        }
    }
}
