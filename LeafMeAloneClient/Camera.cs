using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{
    /// <summary>
    /// A class that stores the information necessary to construct the View matrix
    /// CameraPosition: Location of the Camera
    /// CameraLookAt: Location of the target that the camera is looking at
    /// CameraUp: Orientation of the camera; typically (0,0,1)
    /// </summary>
    class Camera
    {
        private Vector3 m_CameraUp;
        public Vector3 CameraUp {
            get
            {
                return m_CameraUp; 
            }
            set
            {
                m_CameraUp = value;
                UpdateCameraView();

            }
        }
        private Vector3 m_CameraLookAt;
        public Vector3 CameraLookAt
        {
            get
            {
                return m_CameraLookAt;
            }
            set
            {
                m_CameraLookAt = value;
                UpdateCameraView();

            }
        }
        private Vector3 m_CameraPosition;
        public Vector3 CameraPosition
        {
            get
            {
                return m_CameraPosition;
            }
            set
            {
                m_CameraPosition = value;
                UpdateCameraView();

            }
        }

        public Matrix m_ViewMatrix;

        /// <summary>
        /// Initialize the Camera parameters and the camera matrix
        /// </summary>
        /// <param name="pos"> position of the camera </param>
        /// <param name="lookat"> target that the camera is looking at </param>
        /// <param name="up"> orientation of the camera </param>
        public Camera( Vector3 pos, Vector3 lookat, Vector3 up)
        {
            m_CameraUp = up;
            m_CameraPosition = pos;
            m_CameraLookAt = lookat;
            UpdateCameraView();
        }

        /// <summary>
        /// Update the Camera matrix
        /// </summary>
        public void UpdateCameraView()
        {
            m_ViewMatrix = Matrix.LookAtLH(m_CameraPosition, m_CameraLookAt, m_CameraUp);
        }

        /// <summary>
        /// move the camera by delta (by default)
        /// can also specify the camera absolute position
        /// </summary>
        /// <param name="delta"> move the camera by delta. If it is isRelative, the change is additve; otherwise it is absolute </param>
        /// <param name="isRelative"> set whether or not the move is relative; true by default </param>
        public void MoveCamera( Vector3 delta, bool isRelative=true )
        {
            if (isRelative)
            {
                m_CameraPosition += delta;
                m_CameraLookAt += delta;
            }
            else
            {
                Vector3 diff = delta - m_CameraPosition;
                m_CameraPosition = delta;
                m_CameraLookAt = m_CameraLookAt + diff;
            }
            UpdateCameraView();
        }

        /// <summary>
        /// rotate the camera around the centerOfRotation by _ angle towards _ direction
        /// Note that the input direction is in terms of camera space coordinate
        /// </summary>
        /// <param name="centerOfRotation"> the center around which the camera should rotate... (tested with (0,0,0) )</param>
        /// <param name="direction"> The direction (cartesian vector) the camera should rotate towards in the </param>
        /// <param name="angle"> The angle of rotation </param>
        public void RotateCamera(Vector3 centerOfRotation, Vector3 direction, float angle)
        {
            Vector3 rotationAxis = Vector3.Cross(new Vector3(0, 0, -1), Vector3.Normalize(direction));
            
            Matrix rotationMatrix = Matrix.RotationAxis(rotationAxis, angle);

            Vector3 offset = -1.0f * centerOfRotation;
            m_CameraPosition += offset;
            m_CameraPosition = Vector3.TransformCoordinate(m_CameraPosition, rotationMatrix);
            m_CameraPosition -= offset;

            UpdateCameraView();
        }
    }
}
