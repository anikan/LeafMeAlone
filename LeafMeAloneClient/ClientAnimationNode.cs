using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{

    /// <summary>
    /// Copies over animation node information
    /// </summary>
    public class ClientAnimationNode
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
        public ClientAnimationNode(String name)
        {
            Name = name;
        }
    }
}
