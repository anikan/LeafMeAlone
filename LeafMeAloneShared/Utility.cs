using SlimDX;

namespace Shared
{
    public static class Utility
    {
    
    }

    // model properties
    public struct TransformProperties
    {
        Vector3 Position;  // location of the model in world coordinates
        Vector3 Direction; // unit vector pointing to the direction the model is facing
        Vector3 Scale;     // scale of the model
    }
}