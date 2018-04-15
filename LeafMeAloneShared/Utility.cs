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
}