using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;
using SlimDX;

namespace Client
{
    public class UIFramesPersecond
    {
        public double CurrentFps;
        private readonly Stopwatch stopwatch;
        private readonly DrawableString fps;

        private double totalFrames = 0.0f;
        private double totalTime = 0.0f;

        public UIFramesPersecond(Size size, Point location)
        {
            CurrentFps = 0.0f;
            stopwatch = Stopwatch.StartNew();
            fps = UIManager2.DrawTextContinuous("0", UIManager2.TextType.BOLD,
                new Vector2(GraphicsRenderer.Form.Width-40, 0), Color.Red);
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
                totalTime+= (double)stopwatch.ElapsedMilliseconds / 1000.0;
                if (totalTime > 1.0f)
                {
                    CurrentFps = totalFrames;
                    totalFrames = 0;
                    totalTime = 0;
                }

            }

            fps.Text = CurrentFps.ToString();
            stopwatch.Reset();
        }
    }
}
