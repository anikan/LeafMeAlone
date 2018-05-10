using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;

namespace Shared
{
    public class FPSTracker
    {
        public float CurrentFps;
        private readonly Stopwatch stopwatch;
        private readonly FloatVariable fps;

        public FPSTracker(Bar b)
        {
            CurrentFps = 0;
            stopwatch = new Stopwatch();

            fps = new FloatVariable(b) { ReadOnly = true, Label = "FPS" };
        }

        public void Start()
        {
            stopwatch.Start();
        }


        public void Next()
        {
            CurrentFps = 1000.0f/stopwatch.ElapsedTicks * 100;
            fps.Value = CurrentFps;
            stopwatch.Restart();
        }
    }
}
