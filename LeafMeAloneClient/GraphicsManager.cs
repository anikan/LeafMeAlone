using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.UI;
using Shared;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;


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

        public static List<KeyValuePair<ParticleSystem,Transform>> DrawThisFrame = new List<KeyValuePair<ParticleSystem, Transform>>();


        /// <summary>
        /// ActiveCamera contains the currently active camera.
        /// </summary>
        public static Camera ActiveCamera;

        // Current player.
        public static PlayerClient ActivePlayer;

        // The list of particle systems
        public static List<ParticleSystem> ParticleSystems;

        // Converts a screen point to a world position.
        // Converts a screen point to a world position.
        public static Vector3 ScreenToWorldPoint(Vector2 screenPos)
        {

            // Get the view and projection matrices
            Matrix viewMat = ActiveCamera.m_ViewMatrix;
            Matrix projectMat = GraphicsRenderer.ProjectionMatrix;

            // Multiply them together and take inverse
            Matrix resultMat = viewMat * projectMat;
            resultMat.Invert();

            // Args for a vector
            float[] args = new float[4];
            float winZ = 1.0f;

            // Calculate vector arguments for final vector
            args[0] = (2.0f * ((float)(screenPos.X - 0) / (GraphicsRenderer.Form.ClientSize.Width - 0))) - 1.0f;
            args[1] = 1.0f - (2.0f * ((float)(screenPos.Y - 0) / (GraphicsRenderer.Form.ClientSize.Height - 0)));
            args[2] = 2.0f * winZ - 1.0f;
            args[3] = 1.0f;

            // Fill vector with arguments and multiply the view/projection matrix inverse.
            Vector4 vArg = new Vector4(args[0], args[1], args[2], args[3]);
            Vector4 pos = Vector4.Transform(vArg, resultMat);

            pos.W = 1.0f / pos.W;

            // Multiply all args by last arg
            pos.X *= pos.W;
            pos.Y *= pos.W;
            pos.Z *= pos.W;

            // Return final position in world space.
            return new Vector3(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Converts a world point to a screen position.
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoint(Vector3 worldPos)
        {

            // Get the view and projection matrices
            Matrix viewMat = ActiveCamera.m_ViewMatrix;
            Matrix projectMat = GraphicsRenderer.ProjectionMatrix;

            //multiply view,proj,and world pos to get the clip space pos
            Vector4 clipSpace = (viewMat * projectMat).Mult(new Vector4(worldPos, 1.0f));

            //normalize the clip space by dividing by w
            Vector3 normalDeviceCoordSpace = new Vector3(clipSpace.X / clipSpace.W, clipSpace.Y / clipSpace.W, clipSpace.Z / clipSpace.W);

            //transform into screen space by mult by window size.
            Vector2 screenSpace = new Vector2((normalDeviceCoordSpace.X + 1.0f) / 2.0f * GraphicsRenderer.Form.ClientSize.Width,
                (1.0f - normalDeviceCoordSpace.Y) / 2.0f * GraphicsRenderer.Form.ClientSize.Height);

            return screenSpace;
        }
        public static Vector2 WorldToViewportPoint(Vector3 worldPos)
        {

            // Get the view and projection matrices
            Matrix viewMat = ActiveCamera.m_ViewMatrix;
            Matrix projectMat = GraphicsRenderer.ProjectionMatrix;

            //multiply view,proj,and world pos to get the clip space pos
            Vector4 clipSpace = (viewMat * projectMat).Mult(new Vector4(worldPos, 1.0f));

            //normalize the clip space by dividing by w
            Vector3 normalDeviceCoordSpace = new Vector3(clipSpace.X / clipSpace.W, clipSpace.Y / clipSpace.W, clipSpace.Z / clipSpace.W);

            //transform into screen space by mult by window size.
            Vector2 viewPort = new Vector2((normalDeviceCoordSpace.X + 1.0f) / 2.0f, (1.0f - normalDeviceCoordSpace.Y) / 2.0f);

            return viewPort;
        }

        public static Shader ActiveShader;

        public static Light ActiveLightSystem;

        // the offset of the camera from the player. Can be changed anytime to update the camera
        public static Vector3 PlayerToCamOffset = GameClient.CAMERA_OFFSET;

        public static void Update(float delta_t)
        {

            // update the camera position based on the player position
            if (ActivePlayer != null)
            {
                ActiveCamera.MoveCameraAbsolute(ActivePlayer.Transform.Position + PlayerToCamOffset,
                    ActivePlayer.Transform.Position);
            }
            LeafClient.Fire?.Update(delta_t);

        }

        public static void Draw()
        {

            foreach (ParticleSystem particleSystem in ParticleSystems)
            {
                particleSystem.Draw();
            }

            foreach (var particleSystem in DrawThisFrame)
            {
                particleSystem.Key.DrawTransform(particleSystem.Value);
            }
            DrawThisFrame.Clear();
        }

        public static void DrawParticlesThisFrame(ParticleSystem p,Transform t)
        {
            DrawThisFrame.Add(new KeyValuePair<ParticleSystem,Transform>(p,t));
        }


        /// <summary>
        /// Initialize the graphics manager
        /// </summary>
        public static void Init(Camera activeCamera)
        {
            ActiveCamera = activeCamera;
            ParticleSystemManager.Init();
            UIManagerSpriteRenderer.Init();
            // initialize with 20 lights; to change the number of lights, need to change it in the shader manually too
            ActiveLightSystem = new Light(20);

            {
                LightParameters light0 = ActiveLightSystem.GetLightParameters(0);
                light0.UseDirectionalPreset();
                light0.intensities = new Vector4(1.3f, 1.2f, 1.0f, 0);
                light0.status = LightParameters.STATUS_ON;
                light0.position = Vector4.Normalize(new Vector4(-1, 1, 0, 0));
            }
            {
                LightParameters light1 = ActiveLightSystem.GetLightParameters(1);
                light1.UseDirectionalPreset();
                light1.status = LightParameters.STATUS_ON;
                light1.intensities = new Vector4(0.8f, 0.8f, 0.8f, 0);
                light1.position = Vector4.Normalize(new Vector4(1, -1, 0, 0));
            }
            {
                LightParameters light2 = ActiveLightSystem.GetLightParameters(2);
                light2.UseDirectionalPreset();
                light2.status = LightParameters.STATUS_ON;
                light2.intensities = new Vector4(1.2f, 1.2f, 1.2f, 0);
                light2.position = Vector4.Normalize(new Vector4(-1, 0, 1, 0));
            }
            {
                LightParameters light3 = ActiveLightSystem.GetLightParameters(3);
                light3.UseDirectionalPreset();
                light3.status = LightParameters.STATUS_ON;
                light3.intensities = new Vector4(1.3f, 1.3f, 1.3f, 0);
                light3.position = Vector4.Normalize(new Vector4(1, 0, -1, 0));
            }

            LoadAllShaders();

            ParticleSystems = new List<ParticleSystem>();

        }

        /// <summary>
        /// Initialize all the default shaders
        /// </summary>
        private static void LoadAllShaders()
        {
            // add more argument to each list as needed
            List<string> allShaderPaths = new List<string>(new string[]
            {
                Constants.DefaultShader
            });

            List<string> allShaderVSName = new List<string>(new string[]
            {
                "VS"
            });

            List<string> allShaderPSName = new List<string>(new string[]
            {
                "PS"
            });

            List<InputElement[]> allShaderElements = new List<InputElement[]>();
            allShaderElements.Add(new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 1),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 2),
                new InputElement("BONE_ID", 0, Format.R32G32B32A32_UInt, 3),
                new InputElement("BONE_WEIGHT", 0, Format.R32G32B32A32_Float, 4)
            });

            for (int i = 0; i < allShaderPaths.Count; i++)
            {
                DictShader[allShaderPaths[i]] = new Shader(allShaderPaths[i], allShaderVSName[i], allShaderPSName[i], allShaderElements[i]);
            }
        }
    }
}
