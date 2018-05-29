using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    public class AnimationDescriptor
    {
        public Model AnimModel;
        public Vector3 Scale;
        public float Acceleration;

        public AnimationDescriptor(Model animModel, Vector3 scale, float acceleration)
        {
            AnimModel = animModel;
            Scale = scale;
            Acceleration = acceleration;
        }
    }
    static class AnimationManager
    {
        private static List<AnimationDescriptor> _animationModels;

        /// <summary>
        /// Initialize lists
        /// </summary>
        public static void Init()
        {
            _animationModels = new List<AnimationDescriptor>();
        }

        /// <summary>
        /// update, if necessary
        /// </summary>
        /// <param name="delta_t"></param>
        public static void Update(float delta_t)
        {

        }

        /// <summary>
        /// create a new animation
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scale"></param>
        /// <param name="animationAcceleration"></param>
        /// <returns></returns>
        public static int AddAnimation(string path, Vector3 scale, float animationAcceleration = 0)
        {
            Model m;
            _animationModels.Add( new AnimationDescriptor( m = new Model(path, true, true), scale, animationAcceleration) );
            return _animationModels.Count - 1;
        }

        /// <summary>
        /// get the specified animation model
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="repeat"></param>
        /// <param name="moonwalk"></param>
        /// <returns></returns>
        public static Model SwitchAnimation(int Id, bool repeat = true, bool moonwalk = false)
        {
            _animationModels[Id].AnimModel.StopCurrentAnimation();
            _animationModels[Id].AnimModel.StartAnimationSequenceByIndex(0, repeat, moonwalk);

            return _animationModels[Id].AnimModel;
        }

        /// <summary>
        /// get the scale for adjustment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Vector3 GetScale(int id)
        {
            return _animationModels[id].Scale;
        }

        /// <summary>
        /// get the animation acceleration rate
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static float GetAcceleration(int id)
        {
            return _animationModels[id].Acceleration;
        }

        /// <summary>
        /// set a tint for the model
        /// </summary>
        /// <param name="animId"></param>
        /// <param name="color"></param>
        public static void SetAltColor(int animId, Color3 color)
        {
            _animationModels[animId].AnimModel.UseAltColor(color);
        }

        /// <summary>
        /// disable tinting the model
        /// </summary>
        /// <param name="animId"></param>
        public static void DisableAltColor(int animId)
        {
            _animationModels[animId].AnimModel.DisableAltColor();
        }
    }
}
