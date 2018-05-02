﻿using System;
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

        public static Vector4 Mult(this Matrix m, Vector4 multBy)
        {
            return Vector4.Transform(multBy, Matrix.Transpose(m));
        }
    }

    // model properties
    public struct Transform
    {
        public Vector3 Position;  // location of the model in world coordinates
        public Vector3 Rotation; // euler coordinate that represents the direction the object is facing
        public Vector3 Scale;     // scale of the model

        public Vector3 Forward
        {
            get
            {
                float X = (float)Math.Cos(Rotation.Y) * (float)Math.Cos(Rotation.X);
                float Y = (float)Math.Sin(Rotation.Y) * (float)Math.Cos(Rotation.X);
                float Z = (float)Math.Sin(Rotation.X);
                return new Vector3(X, Y, Z);
            }
        }

        // check if the objects are logically equivalent to each other
        public override bool Equals(object other)
        {
            if (other == null || GetType() != other.GetType())
                return false;

            Transform other_prop = (Transform) other;
            return Position.Equals(other_prop.Position) &&
                Rotation.Equals(other_prop.Rotation) &&
                Scale.Equals(other_prop.Scale);

        }

        public void CopyToThis(Transform other)
        {
            Position.X = other.Position.X;
            Position.Y = other.Position.Y;
            Position.Z = other.Position.Z;

            Rotation.X = other.Rotation.X;
            Rotation.Y = other.Rotation.Y;
            Rotation.Z = other.Rotation.Z;

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