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
        
        // Current player.
        public static PlayerClient ActivePlayer;

        // Converts a screen point to a world position.
        public static SlimDX.Vector3 ScreenToWorldPoint(SlimDX.Vector2 screenPos)
        {
            
            // Get the view and projection matrices
            SlimDX.Matrix viewMat = ActiveCamera.m_ViewMatrix;
            SlimDX.Matrix projectMat = GraphicsRenderer.ProjectionMatrix;

            // Multiply them together and take inverse
            SlimDX.Matrix resultMat = viewMat * projectMat;
            resultMat.Invert();

            // Args for a vector
            float[] args = new float[4];
            float winZ = 1.0f;

            // Calculate vector arguments for final vector
            args[0] = (2.0f * ((float)(screenPos.X - 0) / (GraphicsRenderer.Form.Width - 0))) - 1.0f;
            args[1] = 1.0f - (2.0f * ((float)(screenPos.Y - 0) / (GraphicsRenderer.Form.Height - 0)));
            args[2] = 2.0f * winZ - 1.0f;
            args[3] = 1.0f;

            // Fill vector with arguments and multiply the view/projection matrix inverse.
            SlimDX.Vector4 vArg = new SlimDX.Vector4(args[0], args[1], args[2], args[3]);
            SlimDX.Vector4 pos = SlimDX.Vector4.Transform(vArg, resultMat);

            pos.W = 1.0f / pos.W;

            // Multiply all args by last arg
            pos.X *= pos.W;
            pos.Y *= pos.W;
            pos.Z *= pos.W;

            // Return final position in world space.
            return new SlimDX.Vector3(pos.X, pos.Y, pos.Z);
        }   
    }
}
