using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// An descriptor attached to a model for animation
    /// </summary>
    public class AnimationDescriptor
    {
        public Model AnimModel;
        public Vector3 Scale;

        /// <summary>
        /// Only for the manually handled timer in the AnimationManager
        /// </summary>
        public float ElapsedTimePostAnimation = 0f;
        public float EndTime = 0f;

        public AnimationDescriptor(Model animModel, Vector3 scale, float timescale)
        {
            AnimModel = animModel;
            Scale = scale;
            AnimModel.SetAnimationTimeScale(timescale);
        }
    }

    /// <summary>
    /// Animation manager to help keep track of the various states of animation
    /// </summary>
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
            foreach (var des in _animationModels)
            {
                if (des.ElapsedTimePostAnimation < des.EndTime && des.AnimModel.HasAnimationEnded())
                {
                    des.ElapsedTimePostAnimation += delta_t;
                }
            }
        }

        /// <summary>
        /// create a new animation
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scale"></param>
        /// <param name="timeScale"></param>
        /// <returns></returns>
        public static int AddAnimation(string path, Vector3 scale, float timeScale = 1.0f)
        {
            Model m;
            _animationModels.Add( new AnimationDescriptor( m = new Model(path, true, true), scale, timeScale) );
            return _animationModels.Count - 1;
        }

        /// <summary>
        /// get the specified animation model
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="repeat"></param>
        /// <param name="moonwalk"></param>
        /// <returns></returns>
        public static Model GetAnimatedModel(int Id, bool repeat = true, bool moonwalk = false, int index = 0)
        {
            _animationModels[Id].AnimModel.StartAnimationSequenceByIndex(index, repeat, moonwalk);
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
        /// start a new animation timer
        /// </summary>
        /// <param name="id"></param>
        public static void StartPostAnimationTimer(int id, float endTimer)
        {
            _animationModels[id].ElapsedTimePostAnimation = 0f;
            _animationModels[id].EndTime = endTimer;
        }

        /// <summary>
        /// check if the animation timer is over
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsPostTimerEnded(int id)
        {
            return _animationModels[id].ElapsedTimePostAnimation >= _animationModels[id].EndTime;
        }

        public static float GetPostTimeElapsed(int id)
        {
            return _animationModels[id].ElapsedTimePostAnimation;
        }
    }
}
