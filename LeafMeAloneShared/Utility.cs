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
    public struct Transform
    {
        public Vector3 Position;  // location of the model in world coordinates
        public Vector3 Direction; // euler coordinate that represents the direction the object is facing
        public Vector3 Scale;     // scale of the model

        // check if the objects are logically equivalent to each other
        public override bool Equals(object other)
        {
            if (other == null || GetType() != other.GetType())
                return false;

            Transform other_prop = (Transform) other;
            return Position.Equals(other_prop.Position) &&
                Direction.Equals(other_prop.Direction) &&
                Scale.Equals(other_prop.Scale);

        }

        public void CopyToThis(Transform other)
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

        public Vector2 Get2dPosition()
        {
            return new Vector2(Position.X, Position.Y);
        }
    }
}