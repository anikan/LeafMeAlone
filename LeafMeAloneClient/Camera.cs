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
        private Vector3 cameraUp;
        public Vector3 CameraUp {
            get
            {
                return cameraUp; 
            }
            set
            {
                cameraUp = value;
                UpdateCameraView();

            }
        }
        public Vector3 CameraLookAt;
        public Vector3 CameraPosition;
        public Matrix ViewMatrix;

        public Camera( Vector3 pos, Vector3 lookat, Vector3 up)
        {

        }
    }
}
