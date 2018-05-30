using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;
using SlimDX.Direct3D11;

namespace Client
{
    // A wrapper of the Geometry class; used to manage the mesh that is used for rendering
    // Also manages the Model class
    public class Model
    {
        private static Vector3 defaultDirection = new Vector3(0, 0, 0);

        // Whether the model is drawn
        public bool Enabled = true;
        
        public Vector3 Tint = new Vector3(1, 1, 1);

        // public float burningGeoColor;
        // public bool burningGeoColorEnabled;
        // active geometry and shader in use
        private Geometry m_ActiveGeo;
        private Shader m_ActiveShader;
        private string m_ActiveShaderPath;

        // model matrix used for the rendering
        private Matrix m_ModelMatrix;

        // holds the transformation properties of the model
        public Transform m_Properties;

        // This is a duplicate used to check if there is a need to update the matrix
        private Transform m_PrevProperties;

        /// <summary>
        /// creates a new model; duplicate filepath will be used to detect
        /// if a geometry already exists. A default shader will be used if not specified
        /// </summary>
        /// <param name="filePath"> the file path of the model to be loaded </param>
        /// <param name="enableRigging"> specify if rigging is enabled or not </param>
        public Model(string filePath, bool enableRigging = false, bool burningEnabled = false)
        {
            //confirm the file exists
            System.Diagnostics.Debug.Assert(File.Exists(filePath));

            Load(filePath, enableRigging);
            m_ModelMatrix = Matrix.Identity;

            m_Properties = new Transform();
            m_PrevProperties = new Transform();

            m_PrevProperties.Rotation = new Vector3(0, 0, 0);
            m_PrevProperties.Position = new Vector3(0, 0, 0);
            m_PrevProperties.Scale = new Vector3(1, 1, 1);
            Update(0);

            //burningGeoColorEnabled = burningEnabled;
            setShader(Constants.DefaultShader);

            m_ActiveShader.ShaderEffect.GetVariableByName("Tint").AsVector().Set(Tint);
        }

        /// <summary>
        /// Create a new model and specify some particular shader to use for this model
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="shaderPath"></param>
        public Model(string filepath, string shaderPath, bool enableRigging = false) : this(filepath, enableRigging)
        {
            setShader(shaderPath);
        }

        /// <summary>
        /// Set the shader for this model. Creates a new shader with default settings if necessary
        /// </summary>
        /// <param name="shaderPath"> filepath to the shader to be set to </param>
        public void setShader(string shaderPath)
        {
            // initialize shader if necessary
            if (GraphicsManager.DictShader.ContainsKey(shaderPath))
            {
                m_ActiveShader = GraphicsManager.DictShader[shaderPath];
            }
            else
            {
                // by default the VS_name is "VS", PS_name is "PS", and we have the position,normal,texcoord element layout
                m_ActiveShader = new Shader(shaderPath);
                GraphicsManager.DictShader[shaderPath] = m_ActiveShader;
            }
        }

        /// <summary>
        /// Can be used to set the active shader to some custom shader that does not use the default setting
        /// </summary>
        /// <param name="shader"> shader to be set to </param>
        public void setShader(Shader shader)
        {
            m_ActiveShader = shader;
        }

        /// <summary>
        ///  load the geometry if it is not available, or find
        ///  reference to the existing geometry if it is found
        /// </summary>
        /// <param name="filePath"> the filepath to the model file </param>
        /// <param name="enableRigging"> whether or not rigging should be enabled </param>
        public void Load(string filePath, bool enableRigging)
        {
            if (GraphicsManager.DictGeometry.ContainsKey(filePath))
            {
                m_ActiveGeo = GraphicsManager.DictGeometry[filePath];
            }
            else
            {
                m_ActiveGeo = new Geometry(filePath, enableRigging);
                GraphicsManager.DictGeometry[filePath] = m_ActiveGeo;
            }
        }

        /// <summary>
        /// Used to store information on the currently played animation sequences
        /// </summary>
        private double CurrentAnimationTime = 0;
        private int CurrentAnimationIndex = -1;
        private string CurrentAnimationName = null;
        private bool RepeatAnimation = false;
        private bool PauseAnimation = false;

        /// <summary>
        /// pass the model matrix to the shader and draw the active geometry
        /// </summary>
        public void Draw()
        {
            if (Enabled) {
                m_ActiveShader.ShaderEffect.GetVariableByName("Tint").AsVector().Set(Tint);
                if (CurrentAnimationIndex != -1)
                {

                    m_ActiveGeo.CurrentAnimationTime = CurrentAnimationTime;
                    m_ActiveGeo.CurrentAnimationName = CurrentAnimationName;
                    m_ActiveGeo.CurrentAnimationIndex = CurrentAnimationIndex;
                    m_ActiveGeo.RepeatAnimation = RepeatAnimation;
                    m_ActiveGeo.UpdateAnimation();
                }

                m_ActiveShader.UseShader();
                m_ActiveGeo.Draw(m_ModelMatrix, m_ActiveShader);
                
            }
        }

        /// <summary>
        /// Update the model matrix based on the properties
        /// Assume by default the model is facing (0, 0, -1)
        /// Also updates the animation sequence if available
        /// </summary>
        /// <param name="delta_time"> the time advanced since the previous frame </param>
        public void Update(float delta_time)
        {
            // update the matrix only if the properties has changes
            if (!m_Properties.Equals(m_PrevProperties))
            {
                // prev properties = current properties
                m_PrevProperties.CopyToThis(m_Properties);

                m_ModelMatrix = Matrix.Scaling(m_Properties.Scale); // set the scaling of the model

                // set the rotation based on the three directions
                m_ModelMatrix = m_ModelMatrix * Matrix.RotationX(m_Properties.Rotation.X) *
                                Matrix.RotationY(m_Properties.Rotation.Y) *
                                Matrix.RotationZ(m_Properties.Rotation.Z);

                // set the translation based on the position
                m_ModelMatrix = m_ModelMatrix * Matrix.Translation(m_Properties.Position);
            }

            if (!PauseAnimation && CurrentAnimationIndex != -1)
            {
                CurrentAnimationTime += delta_time;
            }
        }

        /// <summary>
        /// start playing the animation sequence as specified by its name
        /// </summary>
        /// <param name="animationName"> The name of the animation, specified by the artist </param>
        /// <param name="repeatAnimation"> State whether or not the animation is to be repeated infinitely </param>
        /// <returns> true if succeeded in starting the animation, false if else </returns>
        public bool StartAnimationSequenceByName(string animationName, bool repeatAnimation)
        {
            if (!m_ActiveGeo.AnimationIndices.ContainsKey(animationName)) return false;

            CurrentAnimationTime = 0;
            CurrentAnimationIndex = m_ActiveGeo.AnimationIndices[animationName];
            RepeatAnimation = repeatAnimation;
            CurrentAnimationName = animationName;
            PauseAnimation = false;
            return true;
        }

        /// <summary>
        /// start playing the animation sequence as specified by its index
        /// </summary>
        /// <param name="index"> The index of the animation, specified by the artist </param>
        /// <param name="repeatAnimation"> State whether or not the animation is to be repeated infinitely </param>
        /// <returns> true if succeeded in starting the animation, false if else </returns>
        public bool StartAnimationSequenceByIndex(int index, bool repeatAnimation)
        {
            if (index < 0 || index >= m_ActiveGeo.GetAnimationCount()) return false;

            CurrentAnimationTime = 0;
            CurrentAnimationIndex = index;
            CurrentAnimationName = m_ActiveGeo.GetAnimationNameByIndex(index);
            RepeatAnimation = repeatAnimation;
            PauseAnimation = false;
            return true;
        }

        /// <summary>
        ///  Stop whatever animation that is taking place
        /// </summary>
        public void StopCurrentAnimation()
        {
            CurrentAnimationIndex = -1;
            CurrentAnimationName = null;
        }

        /// <summary>
        /// find which animation is being set now
        /// </summary>
        /// <returns> the index of currently playing animation </returns>
        public int GetCurrentAnimationIndex()
        {
            return CurrentAnimationIndex;
        }

        /// <summary>
        /// find which animation is being played now
        /// </summary>
        /// <returns> the name of the currently playing animation </returns>
        public string GetCurrentAnimationName()
        {
            return CurrentAnimationName;
        }

        /// <summary>
        /// Pause the current animation sequence
        /// </summary>
        public void PauseCurrentAnimation()
        {
            PauseAnimation = true;
        }

        /// <summary>
        /// Resume the current animation sequence
        /// </summary>
        public void ResumeCurrentAnimation()
        {
            PauseAnimation = false;
        }

        /// <summary>
        /// Check if the animation sequence is paused or not
        /// </summary>
        /// <returns> true if paused, false otherwise </returns>
        public bool IsAnimationPaused()
        {
            return PauseAnimation;
        }
    }
}
