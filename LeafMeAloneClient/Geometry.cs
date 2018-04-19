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
        public Buffer VBO, EBO;

        /// <summary>
        /// Data streams hold the actual Vertices and Faces.
        /// </summary>
        protected DataStream Vertices, Normals, Faces;

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
        public InputElement[] Elements;
        public InputLayout InputLayout;


        /// <summary>
        /// something to do with shaders
        /// </summary>
        public Effect Effect;
        public EffectPass Pass;

        /// <summary>
        /// Create a new geometry given filename
        /// </summary>
        /// <param name="fileName"></param>
        public Geometry(string fileName)
        {

            //Create new importer.
            importer = new AssimpContext();
            scene = importer.ImportFile(fileName);


            int vertSize = scene.Meshes[0].VertexCount * Vector3.SizeInBytes;
            int normSize = scene.Meshes[0].Normals.Count * Vector3.SizeInBytes;
            int faceSize = scene.Meshes[0].FaceCount * 3 * sizeof(int);


            if (scene == null)
            {
                throw new FileNotFoundException();
            }
            else
            {

                Vertices = new DataStream(vertSize, true, true);
                Normals = new DataStream(normSize, true, true);
                Faces = new DataStream(faceSize, true, true);
                

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
                Vertices.Position = 0;
                Normals.Position = 0;
                Faces.Position = 0;

                VBO = new Buffer(GraphicsRenderer.Device, Vertices, vertSize, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                EBO = new Buffer(GraphicsRenderer.Device, Faces, vertSize, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);


                var btcode = ShaderBytecode.CompileFromFile("C:\\Users\\CSVR\\Desktop\\CSE125\\LeafMeAlone\\LeafMeAloneClient\\tester.fx", "Render", "fx_5_0", ShaderFlags.None,
                    EffectFlags.None);

                Effect = new Effect(GraphicsRenderer.Device, btcode);
                EffectTechnique technique = Effect.GetTechniqueByIndex(0);
                Pass = technique.GetPassByIndex(0);

                Elements = new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32_Float, 0)//,
                   // new InputElement("NORMAL",0,Format.R32G32B32_Float, 1)
                };
                InputLayout = new InputLayout(GraphicsRenderer.Device,ShaderSignature.GetInputSignature(btcode), Elements);
            }
        }
    }
}
