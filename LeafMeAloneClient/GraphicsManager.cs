using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    static class GraphicsManager
    {

        /// <summary>
        /// DictGeometry contains all the previously loaded geometries.
        /// </summary>
        public static Dictionary<string, Geometry> DictGeometry = new Dictionary<string, Geometry>();

        /// <summary>
        /// DictShader contains all the previously loaded Shaders.
        /// </summary>
        public static Dictionary<string, Shader> DictShader = new Dictionary<string, Shader>();

        /// <summary>
        /// DictLight contains all the previously loaded Lights.
        /// </summary>
        public static Dictionary<string, Light> DictLight = new Dictionary<string, Light>();

        /// <summary>
        /// ActiveCamera contains the currently active camera.
        /// </summary>
        public static Camera ActiveCamera;

        public static PlayerClient ActivePlayer;

        public static SlimDX.Vector3 ScreenToWorldPoint(SlimDX.Vector2 screenPos)
        {

            
            SlimDX.Matrix viewMat = ActiveCamera.m_ViewMatrix;
            SlimDX.Matrix projectMat = GraphicsRenderer.ProjectionMatrix;

            /*
            float x = 2.0f * screenPos.X / GraphicsRenderer.Form.Width - 1.0f;
            float y = -2.0f * screenPos.Y / GraphicsRenderer.Form.Height + 1.0f;
            SlimDX.Matrix viewProjectionInverse = (projectMat * viewMat);
            viewProjectionInverse.Invert();

            SlimDX.Vector3 point = new SlimDX.Vector3(x, y, 0);
            SlimDX.Vector3 result = SlimDX.Vector3.TransformCoordinate(point, viewProjectionInverse);
            return result;
            */

            


            SlimDX.Matrix resultMat = viewMat * projectMat;
            resultMat.Invert();

            float[] args = new float[4];
            float winZ = 1.0f;

            args[0] = (2.0f * ((float)(screenPos.X - 0) / (GraphicsRenderer.Form.Width - 0))) - 1.0f;
            args[1] = 1.0f - (2.0f * ((float)(screenPos.Y - 0) / (GraphicsRenderer.Form.Height - 0)));
            args[2] = 2.0f * winZ - 1.0f;
            args[3] = 1.0f;

            SlimDX.Vector4 vArg = new SlimDX.Vector4(args[0], args[1], args[2], args[3]);
            SlimDX.Vector4 pos = SlimDX.Vector4.Transform(vArg, resultMat);

            pos.W = 1.0f / pos.W;

            pos.X *= pos.W;
            pos.Y *= pos.W;
            pos.Z *= pos.W;

            return new SlimDX.Vector3(pos.X, pos.Y, pos.Z);
            

        }   
    }
}
