/* Author: Yiming, Nick
 * Last updated date: 5/12/2018
 */

using Assimp;
using Shared;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SlimDX.Direct3D9;
using Buffer = SlimDX.Direct3D11.Buffer;
using Format = SlimDX.DXGI.Format;
using Material = Assimp.Material;
using Mesh = Assimp.Mesh;
using Quaternion = SlimDX.Quaternion;

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

        /// <summary>
        /// set texCount, which tells us if a texture is loaded or not
        /// </summary>
        /// <param name="t"></param>
        public void setTexCount(int t)
        {
            texCount = t;
        }

        /// <summary>
        /// Sets the diffuse components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setDiffuse(float x, float y, float z, float w)
        {
            diffuse = new Vector4(x,y,z,w);
        }

        /// <summary>
        /// Sets the ambient components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setAmbient(float x, float y, float z, float w)
        {
            ambient = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Sets the specular components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setSpecular(float x, float y, float z, float w)
        {
            specular = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Sets the emissive components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setEmissive(float x, float y, float z, float w)
        {
            emissive = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// sets the shininess of the material
        /// </summary>
        /// <param name="x"> shininess </param>
        public void setShininess(float x)
        {
            shininess = x;
        }

        /// <summary>
        /// sets the opacity of the material; only usable if blending is enabled
        /// </summary>
        /// <param name="x"> opacity </param>
        public void setOpacity(float x)
        {
            opacity = x;
        }

        /// <summary>
        /// sets the texture of the material
        /// </summary>
        /// <param name="tex"> texture </param>
        public void setDiffuseTexture(ShaderResourceView tex)
        {
            texSRV = tex;
        }
    }

    /// <summary>
    /// To store per vertex information about the bones
    /// </summary>
    public class VertexBoneData
    {
        public const int MAX_BONES_PER_VERTEX = 4;

        public int[] BoneIndices;
        public float[] BoneWeights;

        private int count;

        public VertexBoneData()
        {
            count = 0;
            BoneIndices = new int[4];
            BoneWeights = new float[4];
        }

        public void AddBoneData(int boneIndex, float boneWeight)
        {
            if (count < MAX_BONES_PER_VERTEX)
            {
                BoneIndices[count] = boneIndex;
                BoneWeights[count] = boneWeight;
                count++;
            }
            else
            {
                // find the bone with the smallest weight
                int minIndex = 0;
                float minWeight = BoneWeights[0];
                for (int i = 1; i < MAX_BONES_PER_VERTEX; i++)
                {
                    if (BoneWeights[i] < minWeight)
                    {
                        minIndex = i;
                        minWeight = BoneWeights[i];
                    }
                }

                // replace with new bone if the new bone has greater weight
                if (boneWeight > minWeight)
                {
                    BoneIndices[minIndex] = boneIndex;
                    BoneWeights[minIndex] = boneWeight;
                }
            }
        }

        // to set the total weight to be ~1.0
        public void NormalizeBoneData()
        {
            float totalWeight = 0;
            for (int i = 0; i < MAX_BONES_PER_VERTEX; i++) totalWeight += BoneWeights[i];

            if (totalWeight < 0.1f)
            {
                BoneWeights[0] = 1.0f;
                BoneIndices[0] = 0;

                for (int i = 1; i < MAX_BONES_PER_VERTEX; i++) BoneWeights[i] = 0f;

                Console.WriteLine("No weight for this vertex");
            }

            else
            {
                if (totalWeight < 0.01f) totalWeight = 1.0f;
                for (int i = 0; i < MAX_BONES_PER_VERTEX; i++) BoneWeights[i] /= totalWeight;
            }
        }
    }

    /// <summary>
    /// To store information on each mesh
    /// </summary>
    public class MyMesh
    {
        //public const int MAX_BONES_PER_MESH = 100;
        //public const int BONE_TRANSFORM_STREAM_SIZE = MAX_BONES_PER_MESH * sizeof(float) * 16;

        public int CountVertices;

        public Buffer EBO, VBOPositions, VBONormals, VBOTexCoords, VBOBoneIDs, VBOBoneWeights;
        public int vertSize, normSize, faceSize, texSize, boneIDSize, boneWeightSize;
        public DataStream Vertices, Normals, Faces, TexCoords, DSBoneIDs, DSBoneWeights, boneTransformStream;
        public MyMaterial Materials;

        //public List<MyBone> Bones;
        //public Dictionary<string, int> BoneMappings;
        public List<VertexBoneData> VertexBoneDatas;  // this is per vertex
        
    }

    /// <summary>
    /// To store information on each bone
    /// </summary>
    public class MyBone
    {
        public string BoneName;

        // applied every frame
        public Matrix BoneOffset;

        public Matrix LocalTransform;
        public Matrix GlobalBindPoseTransform;
        public Matrix GlobalAnimatedTransform;
        public Matrix OriginalLocalTranform;

        public MyBone Parent;
        public List<MyBone> Children;

        // passed into shader for transforming the bone vertices
        public Matrix BoneFrameTransformation;

        public MyBone(string name)
        {
            BoneName = name;
            Children = new List<MyBone>();
        }
    }

    /// <summary>
    /// Copies over animation node information
    /// </summary>
    public class MyAnimationNode
    {
        public String Name;
        public List<Vector3> Translations;
        public List<Double> TranslationTime;

        public List<Quaternion> Rotations;
        public List<Double> RotationTime;

        public List<Vector3> Scalings;
        public List<Double> ScalingTime;

        public MyAnimationNode(String name)
        {
            Name = name;
        }
    }

    class Geometry
    {
        public const int VertexLoc = 0, NormalLoc = 1, TexLoc = 2, BoneIdLoc = 3, BoneWeightLoc = 4;

        /// <summary>
        /// Store information on all the meshes
        /// </summary>
        protected List<MyMesh> allMeshes;

        /// <summary>
        /// Holds references to the textures
        /// </summary>
        protected Dictionary<String, ShaderResourceView> diffuseTextureSRV;

        /// <summary>
        /// stores the source file name of the input file
        /// </summary>
        protected String sourceFileName;

        /// <summary>
        /// Assimp scene containing the loaded model.
        /// </summary>
        protected Scene scene;

        /// <summary>
        /// Assimp importer.
        /// </summary>
        protected AssimpContext importer;

        /// <summary>
        /// enable or disable rigging, read only for now
        /// </summary>
        public bool RiggingEnabled { get; }

        /// <summary>
        /// Lets users find the index of the animation they are trying to use
        /// The animation names (in strings) are defined by the artist
        /// </summary>
        public Dictionary<String, int> AnimationIndices { get; }

        /// <summary>
        /// For internal quick animation node lookup uses.
        /// Each dictionary represents one single animation, and each
        /// animation may have multiple animation channels.
        /// The animations are all indiced the same way as stored in AnimationIndices
        /// </summary>
        private List< Dictionary<String, MyAnimationNode> > _animationNodes;

        /// <summary>
        /// For inverting from the root node
        /// </summary>
        private Matrix _inverseGlobalTransform;

        /// <summary>
        /// Store information on the bones here
        /// </summary>
        public const int MAX_BONES_PER_GEO = 512;
        private List<MyBone> _allBones;
        private Dictionary<string, MyBone> _allBoneLookup;
        private Dictionary<string, int> _allBoneMappings;
        private MyBone _rootBone;
        private DataStream boneTransformStream;

        private int _ubindex = 0;
        private MyBone CreateBoneTree(Node node, MyBone parent)
        {
            MyBone internalNode = new MyBone(node.Name)
            {
                Parent = parent
            };

            if (internalNode.BoneName == "")
            {
                internalNode.BoneName = "UnnamedBone_" + _ubindex++;
            }

            _allBoneLookup[internalNode.BoneName] = internalNode;

            internalNode.LocalTransform = node.Transform.ToMatrix();
            internalNode.OriginalLocalTranform = node.Transform.ToMatrix();

            internalNode.GlobalBindPoseTransform = CalculateBoneToWorldTransform(internalNode);

            for (int i = 0; i < node.ChildCount; i++)
            {
                MyBone child = CreateBoneTree(node.Children[i], internalNode);
                if (child != null)
                {
                    internalNode.Children.Add(child);
                }
            }

            return internalNode;
        }

        private Matrix CalculateBoneToWorldTransform(MyBone bone)
        {
            Matrix global = bone.LocalTransform;
            MyBone parent = bone.Parent;
            while (parent != null)
            {
                global = global * parent.LocalTransform;
                parent = parent.Parent;
            }

            return global;
        }

        /// <summary>
        /// Load the bone information for each vertex
        /// </summary>
        /// <param name="assimpMesh"></param>
        /// <param name="mesh"></param>
        protected void LoadBoneWeights(Mesh assimpMesh, MyMesh mesh)
        {
            // create a new data structures to store the bones
            mesh.VertexBoneDatas = new List<VertexBoneData>(mesh.CountVertices);
            for (int i = 0; i < mesh.CountVertices; i++) mesh.VertexBoneDatas.Add(new VertexBoneData());

            // copy bone weights from the meshes for each vertex
            for (int boneIndex = 0; boneIndex < assimpMesh.BoneCount; boneIndex++)
            {
                Bone currBone = assimpMesh.Bones[boneIndex];

                for (int weighti = 0; weighti < currBone.VertexWeightCount; weighti++)
                {
                    VertexWeight vertexWeight = currBone.VertexWeights[weighti];
                    mesh.VertexBoneDatas[vertexWeight.VertexID].AddBoneData(_allBoneMappings[currBone.Name], vertexWeight.Weight);
                }
            }

            // create new datastream so that we can stream them to the VBOs
            mesh.boneIDSize = sizeof(int) * VertexBoneData.MAX_BONES_PER_VERTEX * mesh.CountVertices;
            mesh.boneWeightSize = sizeof(float) * VertexBoneData.MAX_BONES_PER_VERTEX * mesh.CountVertices;
            mesh.DSBoneIDs = new DataStream(mesh.boneIDSize, true, true);
            mesh.DSBoneWeights = new DataStream(mesh.boneWeightSize, true, true);

            // for each vertex, write the vertex buffer datastreams
            for (int i = 0; i < mesh.CountVertices; i++)
            {
                // normalize bone weights
                mesh.VertexBoneDatas[i].NormalizeBoneData();

                // write the data into datastreams
                for (int bonei = 0; bonei < VertexBoneData.MAX_BONES_PER_VERTEX; bonei++)
                {
                    mesh.DSBoneIDs.Write(mesh.VertexBoneDatas[i].BoneIndices[bonei]);
                    mesh.DSBoneWeights.Write(mesh.VertexBoneDatas[i].BoneWeights[bonei]);
                }
            }

            mesh.DSBoneWeights.Position = 0;
            mesh.DSBoneIDs.Position = 0;

            // create the datastreams
            mesh.VBOBoneIDs = new Buffer(GraphicsRenderer.Device, mesh.DSBoneIDs, mesh.boneIDSize, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            mesh.VBOBoneWeights = new Buffer(GraphicsRenderer.Device, mesh.DSBoneWeights, mesh.boneWeightSize, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        protected void UpdateBoneTransformStream()
        {
            boneTransformStream.Position = 0;

            for (int i = 0; i < MAX_BONES_PER_GEO; i++)
            {
                if (i >= _allBones.Count)
                {
                    boneTransformStream.Position = (MAX_BONES_PER_GEO - 1) * sizeof(float) * 16;
                    boneTransformStream.Write(Matrix.Identity);
                    break;
                }

                boneTransformStream.Write(_allBones[i].BoneOffset * _allBones[i].GlobalAnimatedTransform);
            }

            boneTransformStream.Position = 0;
        }

        /// <summary>
        /// Create a new geometry given filename
        /// </summary>
        /// <param name="fileName"> filepath to the 3D model file </param>
        public Geometry(string fileName, bool enableRigging = false)
        {
            RiggingEnabled = enableRigging;
            sourceFileName = fileName;

            //Create new importer.
            importer = new AssimpContext();

            //import the file
            scene = importer.ImportFile(fileName,
                PostProcessSteps.CalculateTangentSpace | PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.SortByPrimitiveType |
                PostProcessSteps.GenerateUVCoords | PostProcessSteps.FlipUVs | 
                PostProcessSteps.LimitBoneWeights | PostProcessSteps.Debone );

            //make sure scene not null
            if (scene == null)
                throw new FileNotFoundException();

            //loop through sizes and count them.
            allMeshes = new List<MyMesh>(scene.MeshCount);

            //loop through and store sizes 
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                MyMesh mesh = new MyMesh();
                allMeshes.Add(mesh);

                mesh.CountVertices = scene.Meshes[idx].VertexCount;

                mesh.vertSize = scene.Meshes[idx].VertexCount * Vector3.SizeInBytes;
                mesh.normSize = scene.Meshes[idx].Normals.Count * Vector3.SizeInBytes;
                mesh.faceSize = scene.Meshes[idx].FaceCount * scene.Meshes[idx].Faces[0].IndexCount * sizeof(int);
                if (scene.Meshes[idx].HasTextureCoords(0))
                {
                    mesh.texSize = scene.Meshes[idx].TextureCoordinateChannels[0].Count * Vector3.SizeInBytes;
                }
            }

            diffuseTextureSRV = new Dictionary<string, ShaderResourceView>();

            if (enableRigging)
            {
                _allBones = new List<MyBone>();
                _allBoneMappings = new Dictionary<string, int>();
                _allBoneLookup = new Dictionary<string, MyBone>();
                _rootBone = CreateBoneTree(scene.RootNode, null);

                // set each bone offset
                foreach (var sceneMesh in scene.Meshes)
                {
                    foreach (var rawBone in sceneMesh.Bones)
                    {
                        MyBone found;
                        if (!_allBoneLookup.TryGetValue(rawBone.Name, out found)) continue;

                        found.BoneOffset = rawBone.OffsetMatrix.ToMatrix();
                        _allBones.Add(found);
                        _allBoneMappings[found.BoneName] = _allBones.IndexOf(found);
                    }
                }

                // for bones not inside the meshes...?
                foreach (var boneName in _allBoneLookup.Keys.Where(b =>
                    _allBones.All(b1 => b1.BoneName != b) && b.StartsWith("Bone")))
                {
                    _allBoneLookup[boneName].BoneOffset = _allBoneLookup[boneName].Parent.BoneOffset;
                    _allBones.Add(_allBoneLookup[boneName]);
                    _allBoneMappings[boneName] = _allBones.IndexOf(_allBoneLookup[boneName]);
                }

                // load the bone weights
                for (int idx = 0; idx < scene.MeshCount; idx++)
                {
                    LoadBoneWeights(scene.Meshes[idx], allMeshes[idx]);
                }

                boneTransformStream = new DataStream(MAX_BONES_PER_GEO * sizeof(float) * 16, true, true);
            }

            // main loading loop; copy cover the scene content into the datastreams and then to the buffers
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                MyMesh mesh = allMeshes[idx];

                //create new datastreams.
                mesh.Vertices = new DataStream(mesh.vertSize, true, true);
                mesh.Normals = new DataStream(mesh.normSize, true, true);
                mesh.Faces = new DataStream(mesh.faceSize, true, true);

                // create a new material
                mesh.Materials = new MyMaterial();

                // copy the buffers
                scene.Meshes[idx].Vertices.ForEach(vertex =>
                {
                    mesh.Vertices.Write(vertex.ToVector3());
                });
                scene.Meshes[idx].Normals.ForEach(normal =>
                {
                    mesh.Normals.Write(normal.ToVector3());
                });
                scene.Meshes[idx].Faces.ForEach(face =>
                {
                    mesh.Faces.WriteRange(face.Indices.ToArray());
                });

                // check if the mesh has texture coordinates
                if (scene.Meshes[idx].HasTextureCoords(0))
                {
                    mesh.TexCoords = new DataStream(mesh.texSize, true, true);
                    scene.Meshes[idx].TextureCoordinateChannels[0].ForEach(texture => {
                        mesh.TexCoords.Write(texture);
                    });

                    mesh.TexCoords.Position = 0;
                }

                // Parse material properties
                ApplyMaterial(scene.Materials[scene.Meshes[idx].MaterialIndex], mesh.Materials);

                // reset datastream positions
                mesh.Vertices.Position = 0;
                mesh.Normals.Position = 0;
                mesh.Faces.Position = 0;

                //create vertex vbo and faces ebo.
                mesh.VBOPositions = new Buffer(GraphicsRenderer.Device, mesh.Vertices, mesh.vertSize, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                mesh.VBONormals = new Buffer(GraphicsRenderer.Device, mesh.Normals, mesh.normSize, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                if (scene.Meshes[idx].HasTextureCoords(0))
                {
                    mesh.VBOTexCoords = new Buffer(GraphicsRenderer.Device, mesh.TexCoords, mesh.texSize, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                }

                // buffer creation flags
                var ibd = new BufferDescription(
                    mesh.faceSize,
                    ResourceUsage.Immutable,
                    BindFlags.IndexBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None,
                    0);

                mesh.EBO = new Buffer(GraphicsRenderer.Device, mesh.Faces, ibd);
            }

            // set the animation related lookup tables
            AnimationIndices = new Dictionary<string, int>();
            _animationNodes = new List<Dictionary<string, MyAnimationNode>>(scene.AnimationCount);
            for (int i = 0; i < scene.AnimationCount; i++)
            {
                AnimationIndices[scene.Animations[i].Name] = i;
                _animationNodes.Add( new Dictionary<string, MyAnimationNode>() );

                for (int j = 0; j < scene.Animations[i].NodeAnimationChannelCount; j++)
                {
                    NodeAnimationChannel ch = scene.Animations[i].NodeAnimationChannels[j];
                    MyAnimationNode myNode = new MyAnimationNode(ch.NodeName);

                    _animationNodes[i][ch.NodeName] = myNode;
                    myNode.Translations = new List<Vector3>();
                    myNode.TranslationTime = new List<double>();
                    myNode.Rotations = new List<Quaternion>();
                    myNode.RotationTime = new List<double>();
                    myNode.Scalings = new List<Vector3>();
                    myNode.ScalingTime = new List<double>();

                    // copy over all the necessary information in the animation channels
                    for (int k = 0; k < ch.PositionKeyCount; k++)
                    {
                        myNode.Translations.Add( ch.PositionKeys[k].Value.ToVector3() );
                        myNode.TranslationTime.Add( ch.PositionKeys[k].Time );
                    }

                    for (int k = 0; k < ch.RotationKeyCount; k++)
                    {
                        myNode.Rotations.Add(ch.RotationKeys[k].Value.ToQuaternion());
                        myNode.RotationTime.Add(ch.RotationKeys[k].Time);
                    }

                    for (int k = 0; k < ch.ScalingKeyCount; k++)
                    {
                        myNode.Scalings.Add(ch.ScalingKeys[k].Value.ToVector3());
                        myNode.ScalingTime.Add(ch.ScalingKeys[k].Time);
                    }
                }
            }

            _inverseGlobalTransform = Matrix.Invert( scene.RootNode.Transform.ToMatrix() );
        }

        protected void SetBoneTransform(int AnimationIndex, double TimeInSeconds)
        {
            // number of ticks per second
            double TicksPerSecond = scene.Animations[AnimationIndex].TicksPerSecond;
            if (TicksPerSecond <= 0f) TicksPerSecond = 25.0f;

            // current animated time in ticks
            double TimeInTicks = TimeInSeconds * scene.Animations[AnimationIndex].TicksPerSecond;

            // animation time in ticks
            double AnimationTime = TimeInTicks % scene.Animations[AnimationIndex].DurationInTicks;

            // read node hierarchy
            // ReadNodeHierarchy( AnimationIndex, AnimationTime, scene.RootNode, Matrix.Identity );
            ResetLocalTransforms();
            Evaluate(AnimationTime, CurrentAnimationIndex);
            UpdateTransforms(_rootBone);
        }

        private void ResetLocalTransforms()
        {
            foreach (var bone in _allBones)
            {
                bone.LocalTransform = bone.OriginalLocalTranform;
            }
        }

        // Use this to recusively update the animation transform values
        private void UpdateTransforms(MyBone bone)
        {
            bone.GlobalAnimatedTransform = CalculateBoneToWorldTransform(bone);
            foreach (var child in bone.Children)
            {
                UpdateTransforms(child);
            }
        }

        private void Evaluate(double animationTime, int animationIndex)
        {
            Dictionary<string, MyAnimationNode> currentChannels = _animationNodes[animationIndex];
            foreach (var chpair in currentChannels)
            {
                if (!_allBoneLookup.ContainsKey(chpair.Key))
                {
                    Console.WriteLine("Did not find the bone node " + chpair.Key);
                    continue;
                }

                Vector3 vPosition = CalcInterpolateTranslation(animationTime, chpair.Value);
                Quaternion vQuaternion = CalcInterpolateRotation(animationTime, chpair.Value);
                Vector3 Scaling = CalcInterpolateScaling(animationTime, chpair.Value);
                
                Matrix r_mat = Matrix.RotationQuaternion(vQuaternion);
                Matrix s_mat = Matrix.Scaling(Scaling);
                Matrix t_mat = Matrix.Translation(vPosition);

                _allBoneLookup[chpair.Key].LocalTransform = s_mat * r_mat * t_mat;
            }
        }

        private Vector3 CalcInterpolateTranslation(double animationTime, MyAnimationNode animationNode)
        {
            if (animationNode.Translations.Count == 1) return animationNode.Translations[0];
            if (animationNode.TranslationTime[0] > animationTime) return animationNode.Translations[0];

            // find the key frame before or at the current frame
            int translationIndex = 0;
            for (int i = 0; i < animationNode.Translations.Count; i++)
            {
                if (animationNode.TranslationTime[i] > animationTime)
                {
                    translationIndex = i - 1;
                    break;
                }
            }

            if (translationIndex == -1) return animationNode.Translations[animationNode.Translations.Count - 1];
            int nextTranslationIndex = translationIndex + 1;

            double frameDuration = animationNode.TranslationTime[nextTranslationIndex] -
                                   animationNode.TranslationTime[translationIndex];
            double factor = (animationTime - animationNode.TranslationTime[translationIndex]) / frameDuration;

            if (factor <= 0.0) return animationNode.Translations[translationIndex];
            if (factor >= 1.0) return animationNode.Translations[nextTranslationIndex];

            return Vector3.Lerp(animationNode.Translations[translationIndex],
                animationNode.Translations[nextTranslationIndex], (float) factor);
        }

        private Quaternion CalcInterpolateRotation(double animationTime, MyAnimationNode animationNode)
        {
            if (animationNode.Rotations.Count == 1) return animationNode.Rotations[0];
            if (animationNode.RotationTime[0] > animationTime) return animationNode.Rotations[0];

            // find the key frame before or at the current frame
            int rotationIndex = -1;
            for (int i = 0; i < animationNode.Rotations.Count; i++)
            {
                if (animationNode.RotationTime[i] > animationTime)
                {
                    rotationIndex = i - 1;
                    break;
                }
            }

            if (rotationIndex == -1) return animationNode.Rotations[animationNode.Rotations.Count-1];

            int nextRotationIndex = rotationIndex + 1;

            double frameDuration = animationNode.RotationTime[nextRotationIndex] -
                                   animationNode.RotationTime[rotationIndex];
            double factor = (animationTime - animationNode.RotationTime[rotationIndex]) / frameDuration;

            if (factor <= 0.0) return animationNode.Rotations[rotationIndex];
            if (factor >= 1.0) return animationNode.Rotations[nextRotationIndex];

            return Quaternion.Lerp(animationNode.Rotations[rotationIndex],
                animationNode.Rotations[nextRotationIndex], (float)factor);
        }

        private Vector3 CalcInterpolateScaling(double animationTime, MyAnimationNode animationNode)
        {
            if (animationNode.Scalings.Count == 1) return animationNode.Scalings[0];
            if (animationNode.ScalingTime[0] > animationTime) return animationNode.Scalings[0];

            // find the key frame before or at the current frame
            int scalingIndex = -1;
            for (int i = 0; i < animationNode.Scalings.Count; i++)
            {
                if (animationNode.ScalingTime[i] > animationTime)
                {
                    scalingIndex = i - 1;
                    break;
                }
            }

            if (scalingIndex == -1) return animationNode.Scalings[animationNode.Scalings.Count - 1];

            int nextScalingIndex = scalingIndex + 1;

            double frameDuration = animationNode.ScalingTime[nextScalingIndex] -
                                   animationNode.ScalingTime[scalingIndex];
            double factor = (animationTime - animationNode.ScalingTime[scalingIndex]) / frameDuration;

            if (factor <= 0.0) return animationNode.Scalings[scalingIndex];
            if (factor >= 1.0) return animationNode.Scalings[nextScalingIndex];

            return Vector3.Lerp(animationNode.Scalings[scalingIndex],
                animationNode.Scalings[nextScalingIndex], (float)factor);
        }

        private double CurrentAnimationTime = 0;
        private int CurrentAnimationIndex = -1;
        private string CurrentAnimationName = null;
        private bool RepeatAnimation = false;

        // need to be called in order to use the skeletal animation
        public void Update(float delta_time)
        {
            if (CurrentAnimationIndex != -1 && RiggingEnabled)
            {
                // number of ticks per second
                double TicksPerSecond = scene.Animations[CurrentAnimationIndex].TicksPerSecond;
                if (TicksPerSecond <= 0f) TicksPerSecond = 25.0f;

                // current animated time in ticks
                double TimeInTicks = CurrentAnimationTime * scene.Animations[CurrentAnimationIndex].TicksPerSecond;

                // start animation if it is not done or in repeat mode
                if (scene.Animations[CurrentAnimationIndex].DurationInTicks > TimeInTicks || RepeatAnimation)
                {
                    SetBoneTransform(CurrentAnimationIndex, CurrentAnimationTime);
                    UpdateBoneTransformStream();
                }
                // stop the animation if it is done
                else
                {
                    CurrentAnimationIndex = -1;
                }

                // advance the animation
                CurrentAnimationTime += delta_time;
            }
            else CurrentAnimationIndex = -1;
        }

        // need to be called FIRST before an update, so that the skeletal animation can be drawn
        public void StartAnimationSequenceByName(string animationName, bool repeatAnimation = false)
        {
            CurrentAnimationTime = 0;
            CurrentAnimationIndex = AnimationIndices.ContainsKey(animationName) ? AnimationIndices[animationName] : -1 ;
            RepeatAnimation = repeatAnimation;
            CurrentAnimationName = animationName;
        }

        public void StartAnimationSequenceByIndex(int index, bool repeatAnimation = false)
        {
            if (index < -1 || index > scene.AnimationCount - 1) return;

            CurrentAnimationTime = 0;
            CurrentAnimationIndex = index;
            CurrentAnimationName = scene.Animations[index].Name;
            RepeatAnimation = repeatAnimation;
        }

        // Stop whatever animation that is taking place
        public void StopCurrentAnimation()
        {
            CurrentAnimationIndex = -1;
            CurrentAnimationName = null;
        }

        // find which animation is being set now
        public int GetCurrentAnimationIndex()
        {
            return CurrentAnimationIndex;
        }

        public string GetCurrentAnimationName()
        {
            return CurrentAnimationName;
        }

        /// <summary>
        /// Create a texture resource using the complete filepath to the texture file
        /// </summary>
        /// <param name="fileName"> filepath to the texture file </param>
        /// <returns></returns>
        protected ShaderResourceView CreateTexture(String fileName)
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
        protected void ApplyMaterial(Material mat, MyMaterial myMat)
        {
            if (mat.GetMaterialTextureCount(TextureType.Diffuse) > 0)
            {
                TextureSlot tex;
                if (mat.GetMaterialTexture(TextureType.Diffuse, 0, out tex))
                {
                    ShaderResourceView temp;
                    myMat.setDiffuseTexture( temp = CreateTexture(Path.Combine(Path.GetDirectoryName(sourceFileName), tex.FilePath)) );
                    myMat.setTexCount( temp == null ? 0 : 1);
                }
                else
                {
                    myMat.setDiffuseTexture( null );
                    myMat.setTexCount(1);
                }
            }

            // copies over all the material properties to the struct
            // sets the diffuse color
            Color4 color = new Color4(.4f, .4f, .4f, 1.0f); // default is light grey
            if (mat.HasColorDiffuse)
            {
                myMat.setDiffuse( mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A);
            }
            else
            {
                myMat.setDiffuse( color.Red, color.Green, color.Blue, color.Alpha );
            }

            // sets the specular color
            color = new Color4(0.1f, 0.1f, 0.1f, 1.0f);  // default is non-specular 
            if (mat.HasColorSpecular)
            {
                myMat.setSpecular(mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A);
            }
            else
            {
                myMat.setSpecular(color.Red, color.Green, color.Blue, color.Alpha);
            }

            // sets the ambient color
            color = new Color4(.3f, .3f, .3f, 1.0f);    // default is dark grey
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
        /// <param name="shader"> the shader that is used to draw the geometry </param>
        public void Draw(Matrix modelMatrix, Shader shader)
        {

            shader.ShaderEffect.GetVariableByName("gWorld").AsMatrix().SetMatrix(modelMatrix);
            shader.ShaderEffect.GetVariableByName("gView").AsMatrix()
                .SetMatrix(GraphicsManager.ActiveCamera.m_ViewMatrix);
            shader.ShaderEffect.GetVariableByName("gProj").AsMatrix().SetMatrix(GraphicsRenderer.ProjectionMatrix);

            GraphicsManager.ActiveLightSystem.UpdateShader(shader, modelMatrix);
            shader.ShaderEffect.GetVariableByName("CamPosObj").AsVector().Set(
                Vector4.Transform( new Vector4(GraphicsManager.ActiveCamera.CameraPosition, 1.0f), Matrix.Invert(modelMatrix)) );

            
            if (CurrentAnimationIndex != -1)
            {
                shader.ShaderEffect.GetVariableByName("boneTransforms")
                    .SetRawValue(boneTransformStream, MAX_BONES_PER_GEO * sizeof(float) * 16);
            }

            for (int i = 0; i < scene.MeshCount; i++)
            {
                MyMesh mesh = allMeshes[i];

                // pass vertices, normals, and indices into the shader
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(VertexLoc,
                    new VertexBufferBinding(mesh.VBOPositions, Vector3.SizeInBytes, 0));
                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(NormalLoc,
                    new VertexBufferBinding(mesh.VBONormals, Vector3.SizeInBytes, 0));

                GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetIndexBuffer(mesh.EBO, Format.R32_UInt, 0);
                
                // pass texture coordinates into the shader if applicable
                if (mesh.Materials.texCount > 0)
                {
                    // note that the raw parsed tex coords are in vec3, we just need the first 2 elements of the vector
                    GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(TexLoc,
                        new VertexBufferBinding(mesh.VBOTexCoords, Vector3.SizeInBytes, 0));
                }

                // pass bone IDs and weights if applicable
                shader.ShaderEffect.GetVariableByName("animationIndex").AsScalar().Set(CurrentAnimationIndex);
                if (CurrentAnimationIndex != -1)
                {
                    GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(BoneIdLoc,
                        new VertexBufferBinding(mesh.VBOBoneIDs, sizeof(int) * VertexBoneData.MAX_BONES_PER_VERTEX, 0));

                    GraphicsRenderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(BoneWeightLoc,
                        new VertexBufferBinding(mesh.VBOBoneWeights, sizeof(float) * VertexBoneData.MAX_BONES_PER_VERTEX, 0));
                    
                }

                // pass texture resource into the shader if applicable
                if (mesh.Materials.texSRV != null)
                {
                    shader.ShaderEffect.GetVariableByName("tex_diffuse").AsResource().SetResource(mesh.Materials.texSRV);
                }

                // pass material properties into the shader
                shader.ShaderEffect.GetVariableByName("Diffuse").AsVector().Set(mesh.Materials.diffuse);
                shader.ShaderEffect.GetVariableByName("Specular").AsVector().Set(mesh.Materials.specular);
                shader.ShaderEffect.GetVariableByName("Ambient").AsVector().Set(mesh.Materials.ambient);
                shader.ShaderEffect.GetVariableByName("Emissive").AsVector().Set(mesh.Materials.emissive);
                shader.ShaderEffect.GetVariableByName("Shininess").AsScalar().Set(mesh.Materials.shininess);
                shader.ShaderEffect.GetVariableByName("Opacity").AsScalar().Set(mesh.Materials.opacity);
                shader.ShaderEffect.GetVariableByName("texCount").AsScalar().Set(mesh.Materials.texCount);

                // Draw the object using the indices
                shader.ShaderPass.Apply(GraphicsRenderer.Device.ImmediateContext);
                GraphicsRenderer.Device.ImmediateContext.DrawIndexed(mesh.faceSize / sizeof(int), 0, 0);
            }
        }
    }
}
