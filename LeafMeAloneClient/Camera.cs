using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
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
        //Added frustum to camera, because we need to recalculate view frustum every time we change the view.
        public ViewFrustum Frustum;

        private Vector3 m_CameraUp;

        /// <summary>
        /// For screenshake 
        /// </summary>
        private bool _enableScreenShake = false;
        private float _screenShakeMagnitude = 0f;
        private Random rng;

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

        //
        public void StartScreenShake(float intensity)
        {
            _enableScreenShake = true;
            _screenShakeMagnitude = intensity;
        }

        public void StopScreenShake()
        {
            _enableScreenShake = false;
        }

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
            rng = new Random();
            UpdateCameraView();
        }

        /// <summary>
        /// Update the Camera matrix
        /// </summary>
        public void UpdateCameraView()
        {
            if (!_enableScreenShake)
            {
                m_ViewMatrix = Matrix.LookAtLH(m_CameraPosition, m_CameraLookAt, m_CameraUp);
            }
            else
            {
                Vector3 perp = Vector3.Normalize(Vector3.Cross(m_CameraLookAt - m_CameraPosition, m_CameraUp));
                Vector3 offset = perp * _screenShakeMagnitude * (rng.NextFloat() / 2f + .5f);
                Matrix rotation = Matrix.RotationAxis(perp, rng.NextFloat() * 2f * (float) Math.PI);
                offset = Vector3.TransformCoordinate(offset, rotation);
                m_ViewMatrix = Matrix.LookAtLH(m_CameraPosition + offset, m_CameraLookAt + offset, m_CameraUp);
            }

            //every time the cam view is updated, refresh the view frustum.
            Frustum = new ViewFrustum(m_ViewMatrix,GraphicsRenderer.ProjectionMatrix);
        }

        /// <summary>
        /// move the camera by delta (by default)
        /// can also specify the camera absolute position
        /// </summary>
        /// <param name="delta"> move the camera by delta without changing the angle </param>
        public void MoveCameraRelative ( Vector3 delta )
        {
            m_CameraPosition += delta;
            m_CameraLookAt += delta;
            UpdateCameraView();
        }

        /// <summary>
        /// Move the camera to an absolute position, looking at another absolute position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="lookat"></param>
        public void MoveCameraAbsolute(Vector3 pos, Vector3 lookat)
        {
            m_CameraPosition = pos;
            m_CameraLookAt = lookat;
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
