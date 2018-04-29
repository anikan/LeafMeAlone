using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// ActiveCamera contains the currently active camera.
        /// </summary>
        public static Camera ActiveCamera;

        public static PlayerClient ActivePlayer;

        public static Shader ActiveShader;

        public static Light ActiveLightSystem;

        public static void Update()
        {

        }

        /// <summary>
        /// Initialize the graphics manager
        /// </summary>
        public static void Init()
        {
            ActiveCamera = new Camera(new Vector3(0, 0, -10), Vector3.Zero, Vector3.UnitY);
            ActiveLightSystem = new Light(20);
            LightParameters light0 = ActiveLightSystem.GetLightParameters(0);
            {
                light0.UseDirectionalPreset();
                light0.intensities = new Vector4(1.3f,1.2f,1.0f,0);
                light0.status = LightParameters.STATUS_ON;
            }
            LightParameters light1 = ActiveLightSystem.GetLightParameters(1);
            {
                light1.UseDirectionalPreset();
                light1.status = LightParameters.STATUS_ON;
                light1.intensities = new Vector4(0.8f,0.8f,0.8f,0);
                light1.position = Vector4.Normalize(new Vector4(0,-1,0,0));
            }

            LightParameters light2 = ActiveLightSystem.GetLightParameters(2);
            {
                light2.UseDirectionalPreset();
                light2.status = LightParameters.STATUS_ON;
                light2.intensities = new Vector4(0.8f, 0.8f, 0.8f, 0);
                light2.position = Vector4.Normalize(new Vector4(0, 1, 0, 0));
            }

            LoadAllShaders();
        }

        /// <summary>
        /// Initialize all the default shaders
        /// </summary>
        private static void LoadAllShaders()
        {
            // add more argument to each list as needed
            List <string> allShaderPaths = new List<string>( new string[]
            {
                @"../../Shaders/defaultShader.fx"
            } );

            List <string> allShaderVSName = new List<string>(new string[]
            {
                "VS"
            } );

            List<string> allShaderPSName = new List<string>(new string[]
            {
                "PS"
            } );

            List<InputElement[]> allShaderElements = new List<InputElement[]>();
            allShaderElements.Add(new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 1),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 2)
            });

            for (int i = 0; i < allShaderPaths.Count; i++)
            {
                DictShader[allShaderPaths[i]] = new Shader(allShaderPaths[i], allShaderVSName[i], allShaderPSName[i], allShaderElements[i]);
            }
        }
    }
}
