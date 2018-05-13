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
        public double CurrentFps;
        private readonly Stopwatch stopwatch;
        private readonly DoubleVariable fps;


        private double totalFrames = 0;
        private double totalTime = 0.0f;

        public UIFramesPersecond(Size size, Point location)
        {
            CurrentFps = 0;
            stopwatch = Stopwatch.StartNew();
            fps = new DoubleVariable(UIManager.Create("FPS", size, location))
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
            {
                totalFrames++;
                totalTime+= stopwatch.ElapsedMilliseconds / 1000.0;
                if (totalTime > 1.0f)
                {
                    CurrentFps = totalFrames;
                    totalFrames = 0;
                    totalTime = 0;
                }

            }

            fps.Value = CurrentFps;
            stopwatch.Reset();
        }
    }
}
