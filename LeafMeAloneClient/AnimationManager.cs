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
        public bool Moonwalk;

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

        public static void Init()
        {
            _animationModels = new List<AnimationDescriptor>();
        }

        public static void Update(float delta_t)
        {

        }

        public static int AddAnimation(string path, Vector3 scale, float animationAcceleration = 0)
        {
            Model m;
            _animationModels.Add( new AnimationDescriptor( m = new Model(path, true, true), scale, animationAcceleration) );
            return _animationModels.Count - 1;
        }

        public static Model SwitchAnimation(int Id,  bool moonwalk = false)
        {
            _animationModels[Id].AnimModel.StopCurrentAnimation();
            _animationModels[Id].AnimModel.StartAnimationSequenceByIndex(0, true, moonwalk);
            _animationModels[Id].Moonwalk = moonwalk;

            return _animationModels[Id].AnimModel;
        }

        public static Vector3 GetScale(int id)
        {
            return _animationModels[id].Scale;
        }

        public static float GetAcceleration(int id)
        {
            return _animationModels[id].Acceleration;
        }

        public static void SetAltColor(int animId, Color3 color)
        {
            _animationModels[animId].AnimModel.UseAltColor(color);
        }

        public static void DisableAltColor(int animId)
        {
            _animationModels[animId].AnimModel.DisableAltColor();
        }
    }
}
