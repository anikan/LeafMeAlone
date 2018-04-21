using System;
using System.Runtime.CompilerServices;
using Assimp;
using SlimDX;
using SlimDX.X3DAudio;

namespace Shared
{
    public static class Utility
    {
        public static Vector3 ToVector3(this Vector3D vector)
        {
            return new Vector3(vector.X,vector.Y,vector.Z);
        }

        public static float ToRadians(this float degrees)
        {
            return degrees * ((float)Math.PI / 180.0f);
        }
    }

    // model properties
    public struct TransformProperties
    {
        public Vector3 Position;  // location of the model in world coordinates
        public Vector3 Direction; // unit vector pointing to the direction the model is facing
        public Vector3 Scale;     // scale of the model

        // check if the objects are logically equivalent to each other
        public override bool Equals(object other)
        {
            if (other == null || GetType() != other.GetType())
                return false;

            TransformProperties other_prop = (TransformProperties) other;
            return Position.Equals(other_prop.Position) &&
                Direction.Equals(other_prop.Direction) &&
                Scale.Equals(other_prop.Scale);

        }

        public void copyToThis(TransformProperties other)
        {
            Position.X = other.Position.X;
            Position.Y = other.Position.Y;
            Position.Z = other.Position.Z;

            Direction.X = other.Direction.X;
            Direction.Y = other.Direction.Y;
            Direction.Z = other.Direction.Z;

            Scale.X = other.Scale.X;
            Scale.Y = other.Scale.Y;
            Scale.Z = other.Scale.Z;

        }
        

    }
}