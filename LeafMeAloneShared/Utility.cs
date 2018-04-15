using Assimp;
using SlimDX;

namespace Shared
{
    public static class Utility
    {
        public static Vector3 ToVector3(this Vector3D vector)
        {
            return new Vector3(vector.X,vector.Y,vector.Z);
        }
    }

    // model properties
    public struct TransformProperties
    {
        public Vector3 Position;  // location of the model in world coordinates
        public Vector3 Direction; // unit vector pointing to the direction the model is facing
        public Vector3 Scale;     // scale of the model
    }
}