using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using Assimp;
using Assimp.Configs;
using Shared;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;
using Texture2D = SlimDX.Direct3D10.Texture2D;

namespace Client
{
    public struct Material
    {
        public Vector4 diffuse;
        public Vector4 ambient;
        public Vector4 specular;
        public Vector4 emissive;
        public float shininess;
        public float opacity;
        public int texCount;   // will be 0 if there is no texture
        public int texID;      // the textureID of the texture created

        public void setTexCount(int t)
        {
            texCount = t;
        }

        public void setDiffuse(float x, float y, float z, float w)
        {
            diffuse = new Vector4(x,y,z,w);
        }

        public void setAmbient(float x, float y, float z, float w)
        {
            ambient = new Vector4(x, y, z, w);
        }

        public void setSpecular(float x, float y, float z, float w)
        {
            specular = new Vector4(x, y, z, w);
        }

        public void setEmissive(float x, float y, float z, float w)
        {
            emissive = new Vector4(x, y, z, w);
        }

        public void setShininess(float x)
        {
            shininess = x;
        }

        public void setOpacity(float x)
        {
            opacity = x;
        }

        public void setTexID(int id)
        {
            texID = id;
        }
    }

    class Geometry
    {

        /// <summary>
        /// Vertex Buffer,Normal Buffer Index Buffer
        /// </summary>
        private List<Buffer> VBOPositions, VBONormals, VBOTexCoords, EBO;

        //sizes for the loaded object.
        private List<int> vertSize, normSize, faceSize, texSize;

        /// <summary>
        /// Data streams hold the actual Vertices and Faces.
        /// </summary>
        private List<DataStream> Vertices, Normals, Faces, TexCoords;

        private List<Material> Materials;

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
            scene = importer.ImportFile(fileName,
                PostProcessSteps.CalculateTangentSpace | PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.SortByPrimitiveType);

            //make sure scene not null
            if (scene == null)
                throw new FileNotFoundException();

            //loop through sizes and count them.
            vertSize = new List<int>(scene.MeshCount);
            normSize = new List<int>(scene.MeshCount);
            faceSize = new List<int>(scene.MeshCount);
            texSize = new List<int>(scene.MeshCount);

            //add empty things to list
            vertSize.AddRange(Enumerable.Repeat(0, scene.MeshCount));
            normSize.AddRange(Enumerable.Repeat(0, scene.MeshCount));
            faceSize.AddRange(Enumerable.Repeat(0, scene.MeshCount));
            texSize.AddRange(Enumerable.Repeat(0, scene.MeshCount));

            //loop through and store sizes 
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                vertSize[idx] = scene.Meshes[idx].VertexCount * Vector3.SizeInBytes;
                normSize[idx] = scene.Meshes[idx].Normals.Count * Vector3.SizeInBytes;
                faceSize[idx] = scene.Meshes[idx].FaceCount * scene.Meshes[idx].Faces[0].IndexCount * sizeof(int);
                if (scene.Meshes[idx].HasTextureCoords(0))
                {
                    texSize[idx] = scene.Meshes[idx].TextureCoordinateChannels[0].Count * Vector2.SizeInBytes;
                }
            }

            //create lists of datastreams
            Vertices = new List<DataStream>(scene.MeshCount);
            Normals = new List<DataStream>(scene.MeshCount);
            Faces = new List<DataStream>(scene.MeshCount);
            TexCoords = new List<DataStream>(scene.MeshCount);

            //create lists of buffers
            VBOPositions = new List<Buffer>(scene.MeshCount);
            VBONormals = new List<Buffer>(scene.MeshCount);
            VBOTexCoords = new List<Buffer>(scene.MeshCount);
            EBO = new List<Buffer>(scene.MeshCount);

            //add empties to the lists of datastreams
            Vertices.AddRange(Enumerable.Repeat((DataStream) null, scene.MeshCount));
            Normals.AddRange(Enumerable.Repeat((DataStream)null, scene.MeshCount));
            Faces.AddRange(Enumerable.Repeat((DataStream)null, scene.MeshCount));
            TexCoords.AddRange(Enumerable.Repeat((DataStream)null, scene.MeshCount));

            //add empties to lists of buffers.
            VBOPositions.AddRange(Enumerable.Repeat((Buffer)null, scene.MeshCount));
            VBONormals.AddRange(Enumerable.Repeat((Buffer) null, scene.MeshCount));
            VBOTexCoords.AddRange(Enumerable.Repeat((Buffer)null, scene.MeshCount));
            EBO.AddRange(Enumerable.Repeat((Buffer)null, scene.MeshCount));

            // create empty lists of the material properties and textures
            Materials = new List<Material>(scene.MeshCount);

            
            // this will be used for the texture loading
            Vector2 tempVec2 = new Vector2();

            // main loading loop; copy cover the scene content into the datastreams and then to the buffers
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                //create new datastreams.
                Vertices[idx] = new DataStream(vertSize[idx], true, true);
                Normals[idx] = new DataStream(normSize[idx], true, true);
                Faces[idx] = new DataStream(faceSize[idx], true, true);

                // create a new material
                Materials.Add(new Material());

                // copy the buffers
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

                // check if the mesh has texture coordinates
                if (scene.Meshes[idx].HasTextureCoords(0))
                {
                    TexCoords[idx] = new DataStream(texSize[idx], true, true);
                    scene.Meshes[idx].TextureCoordinateChannels[0].ForEach(texture => {
                        tempVec2.X = texture.X;
                        tempVec2.Y = texture.Y;
                        TexCoords[idx].Write(tempVec2);
                    });
                    Materials[idx].setTexCount(1);
                }
                else
                {
                    Materials[idx].setTexCount(0);
                }


                // Parse material properties
                int matIndex;
                if ((matIndex = scene.Meshes[idx].MaterialIndex) >= 0)
                {
                    
                }

                // reset datastream positions
                Vertices[idx].Position = 0;
                Normals[idx].Position = 0;
                Faces[idx].Position = 0;
                TexCoords[idx].Position = 0;

                //create vertex vbo and faces ebo.
                VBOPositions[idx] = new Buffer(GraphicsRenderer.Device, Vertices[idx], vertSize[idx], ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                VBONormals[idx] = new Buffer(GraphicsRenderer.Device, Normals[idx], normSize[idx], ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                if (scene.Meshes[idx].HasTextureCoords(0))
                {
                    VBOTexCoords[idx] = new Buffer(GraphicsRenderer.Device, TexCoords[idx], texSize[idx], ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                }

                // buffer creation flags
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

            //GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBOPositions, Vector3.SizeInBytes, 0));
            //GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(VBONormals, Vector3.SizeInBytes, 0));
            //GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetIndexBuffer(EBO,Format.R32_UInt, 0);


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
