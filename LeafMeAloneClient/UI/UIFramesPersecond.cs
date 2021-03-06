﻿using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using SpriteTextRenderer;

namespace Client.UI
{
    public class UIFramesPersecond : UI
    {
        //Current FPS
        public double CurrentFps;

        //Stopwatch for counting.
        private readonly Stopwatch stopwatch;

        private double totalFrames = 0.0f;
        private double totalTime = 0.0f;
        private DrawableTexture t;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UIFramesPersecond() : base("0", UIManagerSpriteRenderer.TextType.BOLD,
            RectangleF.Empty, TextAlignment.Right | TextAlignment.Top, Color.Red)
        {
            CurrentFps = 0.0f;
            stopwatch = Stopwatch.StartNew();

           
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

            UIText.Text = CurrentFps.ToString(CultureInfo.InvariantCulture);
            stopwatch.Reset();
        }

        public override void Update()
        {
            
        }
    }
}
