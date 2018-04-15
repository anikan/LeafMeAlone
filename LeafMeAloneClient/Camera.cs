using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{
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
        public Vector3 m_CameraLookAt;
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
        public Vector3 m_CameraPosition;
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

        private Matrix m_ViewMatrix;

        public Camera( Vector3 pos, Vector3 lookat, Vector3 up)
        {
            m_CameraUp = up;
            m_CameraPosition = pos;
            m_CameraLookAt = lookat;
            UpdateCameraView();
        }

        public void UpdateCameraView()
        {
            m_ViewMatrix = Matrix.LookAtLH(m_CameraPosition, m_CameraLookAt, m_CameraUp);
        }

        // move the camera by delta
        public void MoveCamera( Vector3 delta )
        {
            m_CameraPosition += delta;
            m_CameraLookAt += delta;
            UpdateCameraView();
        }
    }
}
