using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;
using Shared;
using SlimDX;
using SpriteTextRenderer;

namespace Client
{
    public class UIFramesPersecond
    {
        //Current FPS
        public double CurrentFps;

        //Stopwatch for counting.
        private readonly Stopwatch stopwatch;
        private readonly DrawableString fps;

        private double totalFrames = 0.0f;
        private double totalTime = 0.0f;
        private DrawableTexture t;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UIFramesPersecond()
        {
            CurrentFps = 0.0f;
            stopwatch = Stopwatch.StartNew();
            fps = UIManagerSpriteRenderer.DrawTextContinuous("0", UIManagerSpriteRenderer.TextType.BOLD,
                new RectangleF(0, 0, GraphicsRenderer.Form.ClientSize.Width, GraphicsRenderer.Form.ClientSize.Height), TextAlignment.Right | TextAlignment.Top, Color.Red);

           
        }

        /// <summary>
        /// Start Timing.
        /// </summary>
        public void Start()
        {
            stopwatch.Start();
        }

        /// <summary>
        /// Stop Timing.
        /// </summary>
        public void StopAndCalculateFps()
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 1)
            {
                totalFrames++;
                totalTime += (double)stopwatch.ElapsedMilliseconds / 1000.0;
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
