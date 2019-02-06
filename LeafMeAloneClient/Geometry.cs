﻿/* Author: Yiming, Nick
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


namespace Client { 

    class Geometry
    {
        public const int VertexLoc = 0, NormalLoc = 1, TexLoc = 2, BoneIdLoc = 3, BoneWeightLoc = 4;

        //Bounding boxes for all the meshes.
        public List<BoundingBox> BoundingBoxes { get; private set; } = new List<BoundingBox>();

        /// <summary>
        /// Store information on all the meshes
        /// </summary>
        protected List<ClientMesh> allMeshes;

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
        private List< Dictionary<String, ClientAnimationNode> > _animationNodes;

        /// <summary>
        /// For inverting from the root node
        /// </summary>
        private Matrix _inverseGlobalTransform;

        /// <summary>
        /// Store information on the bones here
        /// </summary>
        private List<ClientBone> _allBones;
        private Dictionary<string, ClientBone> _allBoneLookup;
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
        private ClientBone _rootBone;

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
        private ClientBone CreateBoneTree(Node node, ClientBone parent)
        {
            ClientBone internalNode = new ClientBone(node.Name)
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
                ClientBone child = CreateBoneTree(node.Children[i], internalNode);
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
        private Matrix CalculateBoneToWorldTransform(ClientBone bone)
        {
            Matrix global = bone.LocalTransform.Clone() ;
            ClientBone parent = bone.Parent;
            global = parent == null ? global : global * parent.GlobalAnimatedTransform;
            return global;
        }

        /// <summary>
        /// Load the bone information for each vertex
        /// </summary>
        /// <param name="assimpMesh"> mesh that contains the information to be processed </param>
        /// <param name="mesh"> customized mesh that stores information for later use </param>
        protected void LoadBoneWeights(Mesh assimpMesh, ClientMesh mesh)
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
            allMeshes = new List<ClientMesh>(scene.MeshCount);

            //loop through and store sizes 
            for (int idx = 0; idx < scene.MeshCount; idx++)
            {
                ClientMesh mesh = new ClientMesh();
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
                _allBones = new List<ClientBone>();
                _allBoneMappings = new Dictionary<string, int>();
                _allBoneLookup = new Dictionary<string, ClientBone>();
                
                // set the animation related lookup tables
                AnimationIndices = new Dictionary<string, int>();
                _animationNodes = new List<Dictionary<string, ClientAnimationNode>>(scene.AnimationCount);
                for (int i = 0; i < scene.AnimationCount; i++)
                {
                    AnimationIndices[scene.Animations[i].Name] = i;
                    _animationNodes.Add(new Dictionary<string, ClientAnimationNode>());

                    for (int j = 0; j < scene.Animations[i].NodeAnimationChannelCount; j++)
                    {

                        NodeAnimationChannel ch = scene.Animations[i].NodeAnimationChannels[j];
                        ClientAnimationNode myNode = new ClientAnimationNode(ch.NodeName);
                        
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
                        ClientBone found;
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

                // for bones not inside the meshes .......?
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
                ClientMesh mesh = allMeshes[idx];

                //create new datastreams.
                mesh.Vertices = new DataStream(mesh.vertSize, true, true);
                mesh.Normals = new DataStream(mesh.normSize, true, true);
                mesh.Faces = new DataStream(mesh.faceSize, true, true);

                // create a new material
                mesh.Materials = new ClientMaterial();

                //min and max bounds
                var min = new Vector3(float.MaxValue);
                var max = new Vector3(float.MinValue);
                // copy the buffers
                scene.Meshes[idx].Vertices.ForEach(vertex =>
                {
                    mesh.Vertices.Write(vertex.ToVector3());

                    //keep track of min and max for obj boundaries.
                    min = Vector3.Minimize(min, vertex.ToVector3());
                    max = Vector3.Maximize(max, vertex.ToVector3());
                });
                BoundingBoxes.Add(new BoundingBox(min,max));


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
        protected void SetBoneTransform(int AnimationIndex, double TimeInSeconds, bool reverse = false)
        {
            float offset = 0;

            // number of ticks per second
            double TicksPerSecond = scene.Animations[AnimationIndex].TicksPerSecond;
            if (TicksPerSecond <= 0f) TicksPerSecond = 25.0f;

            // current animated time in ticks
            double TimeInTicks = TimeInSeconds * scene.Animations[AnimationIndex].TicksPerSecond;

            // animation time in ticks
            double AnimationTime = TimeInTicks % (scene.Animations[AnimationIndex].DurationInTicks - offset);

            if (reverse)
            {
                AnimationTime = scene.Animations[AnimationIndex].DurationInTicks - AnimationTime - offset;
            }

            // read node hierarchy
            ResetLocalTransforms();
            Evaluate(AnimationTime + offset, CurrentAnimationIndex);
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
        private void UpdateTransforms(ClientBone bone)
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
            Dictionary<string, ClientAnimationNode> currentChannels = _animationNodes[animationIndex];
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
        private Vector3 CalcInterpolateTranslation(double animationTime, ClientAnimationNode animationNode)
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
        private Quaternion CalcInterpolateRotation(double animationTime, ClientAnimationNode animationNode)
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
        private Vector3 CalcInterpolateScaling(double animationTime, ClientAnimationNode animationNode)
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
        public double CurrentAnimationTime = 0;
        public int CurrentAnimationIndex = -1;
        public string CurrentAnimationName = null;
        public bool RepeatAnimation = false;
        public bool ReverseAnimation = false;

        // need to be called in order to use the skeletal animation
        public void UpdateAnimation()
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
                    SetBoneTransform(CurrentAnimationIndex, CurrentAnimationTime, ReverseAnimation);
                    UpdateBoneMatricesList();
                }
                // stop the animation if it is done
                else
                {
                    CurrentAnimationIndex = -1;
                }
            }
            else CurrentAnimationIndex = -1;
        }

        /// <summary>
        /// Check for the duration of the animation in seconds
        /// </summary>
        /// <param name="index"></param>
        public double GetAnimationDuration(int index)
        {
            return scene.Animations[index].DurationInTicks / scene.Animations[index].TicksPerSecond;
        }

        /// <summary>
        /// Find the name of an animation by name
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetAnimationNameByIndex(int index)
        {
            return scene.Animations[index].Name;
        }

        /// <summary>
        /// Return the total number of animations stored
        /// </summary>
        /// <returns> number of animation sequences </returns>
        public int GetAnimationCount()
        {
            return _animationNodes.Count;
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
        /// Helper method, used to transfer information from Material to ClientMaterial
        /// </summary>
        /// <param name="mat"> the source Material </param>
        /// <param name="myMat"> the destination ClientMaterial </param>
        protected void ApplyMaterial(Material mat, ClientMaterial myMat)
        {
            if (mat.GetMaterialTextureCount(TextureType.Diffuse) > 0)
            {
                TextureSlot tex;
                if (mat.GetMaterialTexture(TextureType.Diffuse, 0, out tex))
                {
                    ShaderResourceView temp;
                    myMat.setDiffuseTexture( temp = CreateTexture(Path.Combine(Path.GetDirectoryName(sourceFileName), Path.GetFileName(tex.FilePath))) );
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

        public Vector4 altDiffuseColor;
        public bool useAltColor = false;

        public void UseAltColor(Color3 color)
        {
            altDiffuseColor.X = color.Red;
            altDiffuseColor.Y = color.Green;
            altDiffuseColor.Z = color.Blue;
            altDiffuseColor.W = 0;
            useAltColor = true;
        }

        public void DisableAltColor()
        {
            useAltColor = false;
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
                ClientMesh mesh = allMeshes[i];

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
                if (!useAltColor || mesh.Materials.texCount > 0)
                {
                    shader.ShaderEffect.GetVariableByName("Diffuse").AsVector().Set(mesh.Materials.diffuse);
                }
                else
                {
                    shader.ShaderEffect.GetVariableByName("Diffuse").AsVector().Set(mesh.Materials.diffuse.ScalarMultiply(altDiffuseColor));
                }

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
