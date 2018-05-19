using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{


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
}
