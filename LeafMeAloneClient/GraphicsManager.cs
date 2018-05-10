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
        
        // Current player.
        public static PlayerClient ActivePlayer;

        // The list of particle systems
        private static List<ParticleSystem> p_systems;

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
            args[0] = (2.0f * ((float)(screenPos.X - 0) / (GraphicsRenderer.Form.Width - 0))) - 1.0f;
            args[1] = 1.0f - (2.0f * ((float)(screenPos.Y - 0) / (GraphicsRenderer.Form.Height - 0)));
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

        public static Shader ActiveShader;

        public static Light ActiveLightSystem;

        // the offset of the camera from the player. Can be changed anytime to update the camera
        public static Vector3 PlayerToCamOffset = new Vector3(0, 50, -30);

        // the offset of the framethrower from the player
        public static Vector3 PlayerToFlamethrowerOffset = new Vector3(1.8f,3.85f,3.0f);
        public static float FlameInitSpeed = 40.0f, FlameAcceleration = 15.0f;
        public static float WindInitSpeed = 60.0f, WindAcceleration = -30.0f, WindStopDistance = 60.0f;
        public static Vector3 WindDirection = Vector3.UnitX;
        private static int counter = 0;

        public static void Update()
        {
            // update the camera position based on the player position
            ActiveCamera.MoveCameraAbsolute( ActivePlayer.Transform.Position + PlayerToCamOffset, ActivePlayer.Transform.Position );

            // set the rotation based on the three directions
            Matrix mat = Matrix.RotationX(ActivePlayer.Transform.Rotation.X) *
                                   Matrix.RotationY(ActivePlayer.Transform.Rotation.Y) *
                                   Matrix.RotationZ(ActivePlayer.Transform.Rotation.Z);
            
            // if all rendering is stopped, reset and enable
            if (!p_systems[0].IsRendering())
            {
                p_systems[0].ResetSystem();
                p_systems[0].EnableGeneration(true);
            }
            // manually toggle off after 6000 frames
            else if ((counter = ++counter % 6000) == 0)
            {
                p_systems[0].EnableGeneration(false);
            }

            // flame throwing particle system update
            p_systems[0].SetOrigin(ActivePlayer.Transform.Position + Vector3.TransformCoordinate(PlayerToFlamethrowerOffset, mat) );
            p_systems[0].SetVelocity(ActivePlayer.Transform.Forward * FlameInitSpeed);
            p_systems[0].SetAcceleration(ActivePlayer.Transform.Forward * FlameAcceleration);
            p_systems[0].Update();

            if (counter % 1000 == 0)
            {
                WindDirection = Vector3.TransformCoordinate(WindDirection, Matrix.RotationY(0.1f));
            }

            p_systems[1].SetVelocity(WindDirection * WindInitSpeed);
            p_systems[1].SetAcceleration(WindDirection * WindAcceleration);
            p_systems[1].Update();

        }

        public static void Draw()
        {
            p_systems[0].Draw();
            p_systems[1].Draw();

        }

        /// <summary>
        /// Initialize the graphics manager
        /// </summary>
        public static void Init()
        {
            ActiveCamera = new Camera(new Vector3(0, 50, -30), Vector3.Zero, Vector3.UnitY);

            // initialize with 20 lights; to change the number of lights, need to change it in the shader manually too
            ActiveLightSystem = new Light(20);  
            LightParameters light0 = ActiveLightSystem.GetLightParameters(0);
            {
                light0.UseDirectionalPreset();
                light0.intensities = new Vector4(1.3f,1.2f,1.0f,0);
                light0.status = LightParameters.STATUS_ON;
                light0.position = Vector4.Normalize(new Vector4(-1, 0, 0, 0));
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

            p_systems = new List<ParticleSystem>();
            
            // set the rotation based on the three directions
            Matrix mat = Matrix.RotationX(ActivePlayer.Transform.Rotation.X) *
                         Matrix.RotationY(ActivePlayer.Transform.Rotation.Y) *
                         Matrix.RotationZ(ActivePlayer.Transform.Rotation.Z);

            // Flame thrower settings
            p_systems.Add(new ParticleSystem(ParticleSystemType.FIRE,
                ActivePlayer.Transform.Position +
                Vector3.TransformCoordinate(PlayerToFlamethrowerOffset, mat), // origin
                ActivePlayer.Transform.Forward * FlameInitSpeed, // acceleration
                ActivePlayer.Transform.Forward * FlameAcceleration, // initial speed
                    false,          // cutoff all colors
                    false,          // no backward particle prevention
                    320.0f,   // cone radius, may need to adjust whenever acceleration changes
                    1.0f,    // initial delta size
                    10f,     // cutoff distance
                    0.2f,     // cutoff speed
                    0.075f      // enlarge speed
                )
            );

            // Wind blower settings
            p_systems.Add(new ParticleSystem(ParticleSystemType.WIND,
                    new Vector3(-10, -10, 0),   // origin
                    WindDirection * WindAcceleration,  // acceleration
                    WindDirection * WindInitSpeed,    // initial speed
                    true,          // cutoff alpha only
                    true,          // prevent backward flow 
                    800.0f,   // cone radius
                    1.0f,    // initial delta size
                    0f,     // cutoff distance
                    0.5f,     // cutoff speed
                    0.1f,      // enlarge speed
                    WindStopDistance      // stop dist
                )
            );

            // camera effect...?
            p_systems.Add(new ParticleSystem(ParticleSystemType.WIND,
                    new Vector3(-10, -10, 0),   // origin
                    new Vector3(-30.0f, 0f, 0f),  // acceleration
                    new Vector3(100f, 0f, 0f),    // initial speed
                    true,          // cutoff alpha only
                    false,           // dont prevent backward flow
                    10000.0f,   // cone radius
                    1.0f,    // initial delta size
                    2f,     // cutoff distance
                    0.5f,     // cutoff speed
                    0.2f      // enlarge speed
                )
            );
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
