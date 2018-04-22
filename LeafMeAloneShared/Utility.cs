using SlimDX;

namespace Shared
{
    public static class Utility
    {
    
    }

    // model properties
    public class TransformProperties
    {
        public Vector3 Position;  // location of the model in world coordinates
        public Vector3 Direction; // unit vector pointing to the direction the model is facing
        public Vector3 Scale;     // scale of the model

        public Vector2 Get2dPosition()
        {
            return new Vector2(Position.X, Position.Y);
        }
    }
}