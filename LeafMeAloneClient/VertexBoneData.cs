using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
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
}
