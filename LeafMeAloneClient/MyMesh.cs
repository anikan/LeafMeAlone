using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Client
{

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
}
