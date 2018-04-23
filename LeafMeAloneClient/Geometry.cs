using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class MyMaterial
    {
        public Vector4 diffuse;
        public Vector4 ambient;
        public Vector4 specular;
        public Vector4 emissive;
        public ShaderResourceView texSRV;
        public float shininess;
        public float opacity;
        public int texCount;   // will be 0 if there is no texture

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

        public void setDiffuseTexture(ShaderResourceView tex)
        {
            texSRV = tex;
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

        /// <summary>
        /// Holds the material properties of each mesh
        /// </summary>
        private List<MyMaterial> Materials;

        /// <summary>
        /// Holds references to the textures
        /// </summary>
        private Dictionary<String, ShaderResourceView> diffuseTextureSRV;

        /// <summary>
        /// stores the source file name of the input file
        /// </summary>
        private String sourceFileName;

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
        
        /// <summary>
        /// something to do with shaders
        /// </summary>
        private InputLayout InputLayout;
        private Effect Effects;
        private EffectPass Pass;

        /// <summary>
        /// Create a new geometry given filename
        /// </summary>
        /// <param name="fileName"> filepath to the 3D model file </param>
        public Geometry(string fileName)
        {
            sourceFileName = fileName;

            //Create new importer.
            importer = new AssimpContext();

            //import the file
            scene = importer.ImportFile(fileName,
                PostProcessSteps.CalculateTangentSpace | PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.SortByPrimitiveType |
                PostProcessSteps.GenerateUVCoords | PostProcessSteps.FlipUVs);

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
                    texSize[idx] = scene.Meshes[idx].TextureCoordinateChannels[0].Count * Vector3.SizeInBytes;
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
            Materials = new List<MyMaterial>(scene.MeshCount);
            diffuseTextureSRV = new Dictionary<string, ShaderResourceView>();

            // main loading loop; copy cover the scene content into the datastreams and then to the buffers
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                //create new datastreams.
                Vertices[idx] = new DataStream(vertSize[idx], true, true);
                Normals[idx] = new DataStream(normSize[idx], true, true);
                Faces[idx] = new DataStream(faceSize[idx], true, true);

                // create a new material
                Materials.Add(new MyMaterial());

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
                        TexCoords[idx].Write(texture);
                    });
                }

                // Parse material properties
                ApplyMaterial(scene.Materials[scene.Meshes[idx].MaterialIndex], Materials[idx]);

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

            Effects = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effects.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 1),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 2) 
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);

            #endregion
        }

        /// <summary>
        /// Create a texture resource using the complete filepath to the texture file
        /// </summary>
        /// <param name="fileName"> filepath to the texture file </param>
        /// <returns></returns>
        private ShaderResourceView CreateTexture(String fileName)
        {
            if (!diffuseTextureSRV.ContainsKey(fileName))
            {
                if (File.Exists(fileName))
                {
                    diffuseTextureSRV[fileName] = ShaderResourceView.FromFile(GraphicsRenderer.Device, fileName);
                }
                else
                {
                    return null;
                }
            }
            return diffuseTextureSRV[fileName];
        }

        /// <summary>
        /// Helper method, used to transfer information from Material to MyMaterial
        /// </summary>
        /// <param name="mat"> the source Material </param>
        /// <param name="myMat"> the destination MyMaterial </param>
        private void ApplyMaterial(Material mat, MyMaterial myMat)
        {
            if (mat.GetMaterialTextureCount(TextureType.Diffuse) > 0)
            {
                TextureSlot tex;
                if (mat.GetMaterialTexture(TextureType.Diffuse, 0, out tex))
                {
                    myMat.setDiffuseTexture( CreateTexture(Path.Combine(Path.GetDirectoryName(sourceFileName), tex.FilePath)) );
                    myMat.setTexCount(1);
                }
                else
                {
                    myMat.setDiffuseTexture( null );
                    myMat.setTexCount(1);
                }
            }

            // copies over all the material properties to the struct
            // sets the diffuse color
            Color4 color = new Color4(.8f, .8f, .8f, 1.0f); // default is light grey
            if (mat.HasColorDiffuse)
            {
                myMat.setDiffuse( mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A);
            }
            else
            {
                myMat.setDiffuse( color.Red, color.Green, color.Blue, color.Alpha );
            }

            // sets the specular color
            color = new Color4(0, 0, 0, 1.0f);  // default is non-specular 
            if (mat.HasColorSpecular)
            {
                myMat.setSpecular(mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A);
            }
            else
            {
                myMat.setSpecular(color.Red, color.Green, color.Blue, color.Alpha);
            }

            // sets the ambient color
            color = new Color4(.2f, .2f, .2f, 1.0f);    // default is dark grey
            if (mat.HasColorAmbient)
            {
                myMat.setAmbient(mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A);
            }
            else
            {
                myMat.setAmbient(color.Red, color.Green, color.Blue, color.Alpha);
            }

            // sets the emissive color
            color = new Color4(0, 0, 0, 1.0f);  // default is black
            if (mat.HasColorEmissive)
            {
                myMat.setEmissive(mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A);
            }
            else
            {
                myMat.setEmissive(color.Red, color.Green, color.Blue, color.Alpha);
            }

            // sets the shininess
            float shininess = 1;    // default is 1
            if (mat.HasShininess)
            {
                myMat.setShininess(mat.Shininess);
            }
            else
            {
                myMat.setShininess(shininess);
            }

            // sets the opacity
            float opacity = 1;      // default is 1
            if (mat.HasOpacity)
            {
                myMat.setOpacity(mat.Opacity);
            }
            else
            {
                myMat.setOpacity(mat.Opacity);
            }
        }

        /// <summary>
        /// Draw a model by using the modelmatrix it is assigned to
        /// </summary>
        /// <param name="modelMatrix"> describes how the object is viewed in the world space </param>
        public void Draw(Matrix modelMatrix)
        {
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.InputLayout = InputLayout;
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            Effects.GetVariableByName("gWorld").AsMatrix().SetMatrix(modelMatrix);
            Effects.GetVariableByName("gView").AsMatrix()
                .SetMatrix(GraphicsManager.ActiveCamera.m_ViewMatrix);
            Effects.GetVariableByName("gProj").AsMatrix().SetMatrix(GraphicsRenderer.ProjectionMatrix);


            for (int i = 0; i < scene.MeshCount; i++)
            {
                // pass vertices, normals, and indices into the shader
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(VBOPositions[i], Vector3.SizeInBytes, 0));
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(1,
                    new VertexBufferBinding(VBONormals[i], Vector3.SizeInBytes, 0));
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetIndexBuffer(EBO[i], Format.R32_UInt, 0);
				
                // pass texture coordinates into the shader if applicable
				if (Materials[i].texCount > 0)
				{
					// note that the raw parsed tex coords are in vec3, we just need the first 2 elements of the vector
					GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(2,
						new VertexBufferBinding(VBOTexCoords[i], Vector3.SizeInBytes, 0));
				}

                // pass texture resource into the shader if applicable
                if (Materials[i].texSRV != null)
                {
                    Effects.GetVariableByName("tex_diffuse").AsResource().SetResource(Materials[i].texSRV);
                }

                // pass material properties into the shader
                Effects.GetVariableByName("Diffuse").AsVector().Set(Materials[i].diffuse);
                Effects.GetVariableByName("Specular").AsVector().Set(Materials[i].specular);
                Effects.GetVariableByName("Ambient").AsVector().Set(Materials[i].ambient);
                Effects.GetVariableByName("Emissive").AsVector().Set(Materials[i].emissive);
                Effects.GetVariableByName("Shininess").AsScalar().Set(Materials[i].shininess);
                Effects.GetVariableByName("Opacity").AsScalar().Set(Materials[i].opacity);
                Effects.GetVariableByName("texCount").AsScalar().Set(Materials[i].texCount);

                // Draw the object using the indices
                Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
                GraphicsRenderer.Device.ImmediateContext.DrawIndexed(faceSize[i] / sizeof(int), 0, 0);
            }
        }
    }
}
