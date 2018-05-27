using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    static class AnimationManager
    {
        private static List<Model> _animationModels;
        private static List<Vector3> _scaleFactors;

        public static void Init()
        {
            _animationModels = new List<Model>();
        }

        public static void Update()
        {

        }

        public static int AddAnimation(string path, Vector3 scale)
        {
            _animationModels.Add( new Model( path, true ) );
            _scaleFactors.Add( scale );
            return _animationModels.Count - 1;
        }

        public static Model SwitchAnimation(int Id, Transform transform, bool moonwalk = false)
        {
            _animationModels[Id].StopCurrentAnimation();
            _animationModels[Id].StartAnimationSequenceByIndex(0, true);
            _animationModels[Id].m_Properties.CopyToThis(transform);

            _animationModels[Id].m_Properties.Scale.X *= _scaleFactors[Id].X;
            _animationModels[Id].m_Properties.Scale.Y *= _scaleFactors[Id].Y;
            _animationModels[Id].m_Properties.Scale.Z *= _scaleFactors[Id].Z;

            return _animationModels[Id];
        }
    }
}
