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
            SlimDX.Quaternion ret = new SlimDX.Quaternion();
            ret.X = q.X;
            ret.Y = q.Y;
            ret.Z = q.Z;
            ret.W = q.W;
            return ret;
        }

        public static Assimp.Quaternion ToQuaternion(this SlimDX.Quaternion q)
        {
            Assimp.Quaternion ret= new Assimp.Quaternion();
            ret.W = q.W;
            ret.X = q.X;
            ret.Y = q.Y;
            ret.Z = q.Z;
            return ret;
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

            ret = Matrix.Transpose(ret);
            return ret;
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

        public static Matrix Clone(this Matrix mat)
        {
            Matrix ret = new Matrix(  );
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    ret[i, j] = mat[i, j];
                }
            }

            return ret;
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
}