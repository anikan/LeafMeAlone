using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    public class Transform
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

        public Transform()
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        // check if the objects are logically equivalent to each other
        public override bool Equals(object other)
        {
            if (other == null || GetType() != other.GetType())
                return false;

            Transform other_prop = (Transform)other;
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

        public override string ToString()
        {
            return base.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
