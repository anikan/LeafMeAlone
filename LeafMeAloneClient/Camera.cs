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
        public Vector3 CameraUp;
        public Vector3 CameraLookAt;
        public Vector3 CameraPosition;
        public Matrix ViewMatrix;

        public Camera( Vector3 pos, Vector3 lookat, Vector3 up)
        {

        }
    }
}
