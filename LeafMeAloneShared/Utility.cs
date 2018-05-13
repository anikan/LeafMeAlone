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
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static SlimDX.Quaternion ToQuaternion(this Assimp.Quaternion q)
        {
            return new SlimDX.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static Matrix ToMatrix(this Matrix4x4 mat)
        {
            Matrix ret = new Matrix();

            ret.M11 = mat.A1;
            ret.M12 = mat.A2;
            ret.M13 = mat.A3;
            ret.M14 = mat.A4;

            ret.M21 = mat.B1;
            ret.M22 = mat.B2;
            ret.M23 = mat.B3;
            ret.M24 = mat.B4;

            ret.M31 = mat.C1;
            ret.M32 = mat.C2;
            ret.M33 = mat.C3;
            ret.M34 = mat.C4;
            
            ret.M41 = mat.D1;
            ret.M42 = mat.D2;
            ret.M43 = mat.D3;
            ret.M44 = mat.D4;

            //return ret;
            return Matrix.Transpose(ret);
        }

        public static float ToRadians(this float degrees)
        {
            return degrees * ((float) Math.PI / 180.0f);
        }

        public static void Copy(this Vector4 dest, Vector4 src)
        {
            dest.X = src.X;
            dest.Y = src.Y;
            dest.Z = src.Z;
            dest.W = src.W;
        }

        public static void Copy(this Vector3 dest, Vector3 src)
        {
            dest.X = src.X;
            dest.Y = src.Y;
            dest.Z = src.Z;
        }

        public static void Copy(this Vector4 dest, Vector3 src)
        {
            dest.X = src.X;
            dest.Y = src.Y;
            dest.Z = src.Z;
            dest.W = 1;
        }

        public static void Copy(this Vector3 dest, Vector4 src)
        {
            dest.X = src.X;
            dest.Y = src.Y;
            dest.Z = src.Z;
        }

        public static float NextFloat(this Random r)
        {
            return (float) r.NextDouble();
        }

        public static float Range(this Random r, float max)
        {
            return r.NextFloat() * max;
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

        public Vector3 Forward  // getter returns the unit direction vector, based on the Rotation vector
        {
            get
            {
                Vector3 retVec = new Vector3(0, 0, 1);

                // set the rotation based on the three directions
                Matrix m_ModelMatrix = Matrix.RotationX(Rotation.X) *
                                Matrix.RotationY(Rotation.Y) *
                                Matrix.RotationZ(Rotation.Z);

                retVec = Vector3.Normalize(Vector3.TransformCoordinate(retVec, m_ModelMatrix));

             //   Console.WriteLine("Forward is: " + retVec);

                return retVec;
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
            return new Vector2(Position.X, Position.Z);
        }
    }
}