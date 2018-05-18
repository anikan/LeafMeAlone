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
using Buffer = SlimDX.Direct3D11.Buffer;
using Format = SlimDX.DXGI.Format;
using Material = Assimp.Material;
using Mesh = Assimp.Mesh;
using Quaternion = SlimDX.Quaternion;

namespace Client
{
    /// <summary>
    /// used for storing information about the material properties of a mesh
    /// </summary>
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

        /// <summary>
        /// Create a new vertex object
        /// </summary>
        public VertexBoneData()
        {
            count = 0;
            BoneIndices = new int[4];
            BoneWeights = new float[4];
        }

        /// <summary>
        /// Add new bone weight data to this vertex
        /// </summary>
        /// <param name="boneIndex"> index of the bone </param>
        /// <param name="boneWeight"> weight of the bone to this vertex </param>
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

        /// <summary>
        /// to set the total weight to be ~1.0
        /// </summary>
        public void NormalizeBoneData()
        {
            float totalWeight = 0;
            for (int i = 0; i < MAX_BONES_PER_VERTEX; i++) totalWeight += BoneWeights[i];

            if (totalWeight < 0.1f)
            {
                BoneWeights[0] = 1.0f;
                BoneIndices[0] = 0;

                for (int i = 1; i < MAX_BONES_PER_VERTEX; i++) BoneWeights[i] = 0f;
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
        /// <summary>
        /// The number of vertices this mesh stores
        /// </summary>
        public int CountVertices;

        /// <summary>
        /// The various vertex buffer objects that need to be passed to the shader each frame
        /// </summary>
        public Buffer EBO, VBOPositions, VBONormals, VBOTexCoords, VBOBoneIDs, VBOBoneWeights;
        public int vertSize, normSize, faceSize, texSize, boneIDSize, boneWeightSize;
        public DataStream Vertices, Normals, Faces, TexCoords, DSBoneIDs, DSBoneWeights;
        public MyMaterial Materials;

        /// <summary>
        /// The vertex weight and indices
        /// </summary>
        public List<VertexBoneData> VertexBoneDatas;  // this is per vertex
        
    }

    /// <summary>
    /// To store information on each bone
    /// </summary>
    public class MyBone
    {
        /// <summary>
        /// The name of the bone, used for various dictionary references
        /// </summary>
        public string BoneName;

        /// <summary>
        /// Used for transforming from object space to bone space
        /// </summary>
        public Matrix BoneOffset;

        /// <summary>
        /// Stores the various transformation matrices of the bone
        /// </summary>
        public Matrix LocalTransform;
        public Matrix GlobalBindPoseTransform;
        public Matrix GlobalAnimatedTransform;
        public Matrix OriginalLocalTranform;

        /// <summary>
        /// Stores the hierarchy information
        /// </summary>
        public MyBone Parent;
        public List<MyBone> Children;

        /// <summary>
        /// Stores which indices this bone affects
        /// </summary>
        public List<int> MeshIndices;

        /// <summary>
        ///  passed into shader for transforming the bone vertices
        /// </summary>
        public Matrix BoneFrameTransformation;

        /// <summary>
        /// Constructor. Initializes Children list
        /// </summary>
        /// <param name="name"></param>
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
        /// <summary>
        /// Name of the node, as referenced by bone nodes too
        /// </summary>
        public String Name;

        /// <summary>
        /// The translation timing and vector of the animation
        /// </summary>
        public List<Vector3> Translations;
        public List<Double> TranslationTime;

        /// <summary>
        /// The rotation timing and vector of the animation
        /// </summary>
        public List<Quaternion> Rotations;
        public List<Double> RotationTime;

        /// <summary>
        /// The scaling timing and vector of the animation
        /// </summary>
        public List<Vector3> Scalings;
        public List<Double> ScalingTime;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
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
        private List<MyBone> _allBones;
        private Dictionary<string, MyBone> _allBoneLookup;
        private Dictionary<string, int> _allBoneMappings;

        /// <summary>
        /// Put the bone matrices into a list that is convenient to be passed into the shader
        /// </summary>
        private List<Matrix> _boneTransformList;

        /// <summary>
        /// A constant that is also defined in the shader
        /// </summary>
        public const int MAX_BONES_PER_GEO = 512;

        /// <summary>
        /// The root bone of the bone tree we are storing
        /// </summary>
        private MyBone _rootBone;

        /// <summary>
        /// A temp integer for creating unique strings
        /// </summary>
        private int _ubindex = 0;

        /// <summary>
        /// Recursively creates a bone tree based on the scenegraph returned by the importer
        /// </summary>
        /// <param name="node"> The node that is referenced to create the bone </param>
        /// <param name="parent"> The parent of the bone to be created </param>
        /// <returns> the bone that is created </returns>
        private MyBone CreateBoneTree(Node node, MyBone parent)
        {
            MyBone internalNode = new MyBone(node.Name)
            {
                Parent = parent
            };

            _allBoneLookup[internalNode.BoneName] = internalNode;

            internalNode.LocalTransform = node.Transform.ToMatrix();
            internalNode.OriginalLocalTranform = node.Transform.ToMatrix();

            internalNode.GlobalBindPoseTransform = CalculateBoneToWorldTransform(internalNode);
            internalNode.GlobalAnimatedTransform = CalculateBoneToWorldTransform(internalNode);

            internalNode.MeshIndices = node.MeshIndices.ToList();

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

        /// <summary>
        /// Calculate the current bone's global transform based on its
        /// local transform and the parent's global transform
        /// </summary>
        /// <param name="bone"> The bone whose global transform is to be calculated </param>
        /// <returns> global transformation matrix of the node </returns>
        private Matrix CalculateBoneToWorldTransform(MyBone bone)
        {
            Matrix global = bone.LocalTransform.Clone() ;
            MyBone parent = bone.Parent;
            global = parent == null ? global : global * parent.GlobalAnimatedTransform;
            return global;
        }

        /// <summary>
        /// Load the bone information for each vertex
        /// </summary>
        /// <param name="assimpMesh"> mesh that contains the information to be processed </param>
        /// <param name="mesh"> customized mesh that stores information for later use </param>
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
                //mesh.VertexBoneDatas[i].NormalizeBoneData();

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

        /// <summary>
        /// Update the list of matrices to be passed into the shader
        /// </summary>
        protected void UpdateBoneMatricesList()
        {

            for (int i = 0; i < MAX_BONES_PER_GEO; i++)
            {
                if (i >= _allBones.Count)
                {
                    break;
                }
                Matrix m = _allBones[i].BoneOffset * _allBones[i].GlobalAnimatedTransform * _inverseGlobalTransform;
                _boneTransformList[i] = m;
            }
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
                PostProcessSteps.LimitBoneWeights | PostProcessSteps.ValidateDataStructure );

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

            // do all the processing that rigging is required to have
            if (enableRigging)
            {
                _allBones = new List<MyBone>();
                _allBoneMappings = new Dictionary<string, int>();
                _allBoneLookup = new Dictionary<string, MyBone>();
                
                // set the animation related lookup tables
                AnimationIndices = new Dictionary<string, int>();
                _animationNodes = new List<Dictionary<string, MyAnimationNode>>(scene.AnimationCount);
                for (int i = 0; i < scene.AnimationCount; i++)
                {
                    AnimationIndices[scene.Animations[i].Name] = i;
                    _animationNodes.Add(new Dictionary<string, MyAnimationNode>());

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
                            myNode.Translations.Add(ch.PositionKeys[k].Value.ToVector3());
                            myNode.TranslationTime.Add(ch.PositionKeys[k].Time);
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
                
                // create and store the big scene tree
                _rootBone = CreateBoneTree(scene.RootNode, null);

                // set each bone offset
                foreach (var sceneMesh in scene.Meshes)
                {
                    foreach (var rawBone in sceneMesh.Bones)
                    {
                        MyBone found;
                        if (!_allBoneLookup.TryGetValue(rawBone.Name, out found))
                        {
                            Console.WriteLine("Cannot find bone: " + rawBone.Name);
                            continue;
                        }

                        found.BoneOffset = rawBone.OffsetMatrix.ToMatrix();
                        _allBones.Add(found);
                        _allBoneMappings[found.BoneName] = _allBones.IndexOf(found);
                    }
                }

                // for bones not inside the meshes...?
                foreach (var boneName in _allBoneLookup.Keys.Where(b =>
                    _allBones.All(b1 => b1.BoneName != b) && b.StartsWith("Bone")))
                {
                    _allBoneLookup[boneName].BoneOffset = _allBoneLookup[boneName].Parent.BoneOffset.Clone();
                    _allBones.Add(_allBoneLookup[boneName]);
                    _allBoneMappings[boneName] = _allBones.IndexOf(_allBoneLookup[boneName]);
                }

                // load the bone weights
                for (int idx = 0; idx < scene.MeshCount; idx++)
                {
                    LoadBoneWeights(scene.Meshes[idx], allMeshes[idx]);
                }

                //_boneTransformStream = new DataStream(MAX_BONES_PER_GEO * sizeof(float) * 16, true, true);
                _boneTransformList = new List<Matrix>(MAX_BONES_PER_GEO);
                for (int i = 0; i < MAX_BONES_PER_GEO; i++)
                {
                    _boneTransformList.Add(Matrix.Identity);
                }
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

            _inverseGlobalTransform = Matrix.Invert( scene.RootNode.Transform.ToMatrix() );
        }

        /// <summary>
        /// Use this function to set the Bone Hierarchy to have the correct transformation
        /// matrices for this frame
        /// </summary>
        /// <param name="AnimationIndex"> The index of the animation sequences to be played </param>
        /// <param name="TimeInSeconds"> The time since this animation is first started </param>
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
            ResetLocalTransforms();
            Evaluate(AnimationTime+1, CurrentAnimationIndex);
            UpdateTransforms(_rootBone);
        }

        /// <summary>
        /// Reset the local tranformation matrices to their original (bind pose) values
        /// </summary>
        private void ResetLocalTransforms()
        {
            foreach (var bone in _allBones)
            {
                bone.LocalTransform = bone.OriginalLocalTranform.Clone();
            }
        }

        /// <summary>
        /// Use this to recusively update the animation transform values
        /// </summary>
        /// <param name="bone"> The bone whose, and whose children's, global transformation matrices are to be calculated </param>
        private void UpdateTransforms(MyBone bone)
        {
            bone.GlobalAnimatedTransform = CalculateBoneToWorldTransform(bone);

            foreach (var child in bone.Children)
            {
                UpdateTransforms(child);
            }
        }

        /// <summary>
        /// Set the local tranformation matrices of the bone tree
        /// </summary>
        /// <param name="animationTime"> The time, in ticks, in the animation time domain </param>
        /// <param name="animationIndex"> The index of the animation sequence </param>
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

        /// <summary>
        /// Find the translation vector at the specified animation time for the specified animation sequence 
        /// </summary>
        /// <param name="animationTime"> The number of ticks in the animation time domain </param>
        /// <param name="animationNode"> The node to extract the translation vector from </param>
        /// <returns> The linearly interpolated translation vector that represents the translation at specified time </returns>
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

        /// <summary>
        /// Find the rotation quaternion at the specified animation time for the specified animation sequence 
        /// </summary>
        /// <param name="animationTime"> The number of ticks in the animation time domain </param>
        /// <param name="animationNode"> The node to extract the translation vector from </param>
        /// <returns> The linearly interpolated rotation quaternion that represents the rotation at specified time </returns>
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

        /// <summary>
        /// Find the scaling vector at the specified animation time for the specified animation sequence 
        /// </summary>
        /// <param name="animationTime"> The number of ticks in the animation time domain </param>
        /// <param name="animationNode"> The node to extract the translation vector from </param>
        /// <returns> The linearly interpolated scaling vector that represents the scaling at specified time </returns>
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

        /// <summary>
        /// Used to store information on the currently played animation sequences
        /// </summary>
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
                    UpdateBoneMatricesList();
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

        /// <summary>
        /// start playing the animation sequence as specified by its name
        /// </summary>
        /// <param name="animationName"> The name of the animation, specified by the artist </param>
        /// <param name="repeatAnimation"> State whether or not the animation is to be repeated infinitely </param>
        public void StartAnimationSequenceByName(string animationName, bool repeatAnimation = false)
        {
            if (!AnimationIndices.ContainsKey(animationName)) return;

            CurrentAnimationTime = 0;
            CurrentAnimationIndex = AnimationIndices.ContainsKey(animationName) ? AnimationIndices[animationName] : -1 ;
            RepeatAnimation = repeatAnimation;
            CurrentAnimationName = animationName;
        }

        /// <summary>
        /// start playing the animation sequence as specified by its index
        /// </summary>
        /// <param name="index"> The index of the animation, as interpreted by assimp </param>
        /// <param name="repeatAnimation"> State whether or not the animation is to be repeated infinitely </param>
        public void StartAnimationSequenceByIndex(int index, bool repeatAnimation = false)
        {
            if (index <= -1 || index > scene.AnimationCount - 1) return;

            CurrentAnimationTime = 0;
            CurrentAnimationIndex = index;
            CurrentAnimationName = scene.Animations[index].Name;
            RepeatAnimation = repeatAnimation;
        }

        /// <summary>
        ///  Stop whatever animation that is taking place
        /// </summary>
        public void StopCurrentAnimation()
        {
            CurrentAnimationIndex = -1;
            CurrentAnimationName = null;
        }

        /// <summary>
        /// find which animation is being set now
        /// </summary>
        /// <returns></returns>
        public int GetCurrentAnimationIndex()
        {
            return CurrentAnimationIndex;
        }

        /// <summary>
        /// find which animation is being played now
        /// </summary>
        /// <returns></returns>
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
                //shader.ShaderEffect.GetVariableByName("boneTransforms")
                //    .SetRawValue(_boneTransformStream, MAX_BONES_PER_GEO * sizeof(float) * 16);
                shader.ShaderEffect.GetVariableByName("boneTransforms").AsMatrix().SetMatrixArray(_boneTransformList.ToArray());
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
                    shader.ShaderEffect.GetVariableByName("meshTransform").AsMatrix().SetMatrix(_allBones[0].GlobalAnimatedTransform);
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
