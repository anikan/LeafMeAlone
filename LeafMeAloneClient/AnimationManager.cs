using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    static class AnimationManager
    {
        private static List<Model> _animationModels;

        public static void Init()
        {
            _animationModels = new List<Model>();
        }

        public static void Update()
        {

        }

        public static int AddAnimation(string path)
        {
            _animationModels.Add( new Model( path, true ) );
            return _animationModels.Count - 1;
        }

        public static Model SwitchAnimation(int Id, bool moonwalk = false)
        {
            _animationModels[Id].StopCurrentAnimation();
            _animationModels[Id].StartAnimationSequenceByIndex(1, true);
            return _animationModels[Id];
        }
    }
}
