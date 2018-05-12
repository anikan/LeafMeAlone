using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;

namespace Client
{
    public class UIFramesPersecond
    {
        public float CurrentFps;
        private readonly Stopwatch stopwatch;
        private readonly FloatVariable fps;

        public UIFramesPersecond(Size size, Point location)
        {
            CurrentFps = 0;
            stopwatch = Stopwatch.StartNew();
            fps = new FloatVariable(UIManager.Create("FPS", size, location))
            {
                ReadOnly = true,
                Label = "FPS",
                Precision = 2,
                Value = 0
            };
        }

        public void Start()
        {
            stopwatch.Start();
        }


        public void StopAndCalculateFps()
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 1)
                CurrentFps = 1000.0f / stopwatch.ElapsedMilliseconds;

            fps.Value = CurrentFps;
            stopwatch.Reset();
        }
    }
}
