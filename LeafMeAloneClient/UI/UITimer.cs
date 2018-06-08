using System;
using System.Diagnostics;
using System.Drawing;
using System.Timers;
using Shared;
using SpriteTextRenderer;

namespace Client.UI
{
    public class UITimer : UI
    {
        public delegate void TimerCompleted();
        public TimerCompleted OnTimerCompleted;

        public const string TimeFormatting = "mm\\:ss\\:f";

        //time delta (in seconds)
        private const float tickDelta = .1f;

        //Time remaining (in seconds)
        public float TimeRemaining { get; private set; }

        private readonly Timer t;
        private Stopwatch actualTime = new Stopwatch();

        private float startingTime;

        public UITimer(float timeToCountInSeconds) : base(TimeSpan.FromSeconds(timeToCountInSeconds).ToString(TimeFormatting), UIManagerSpriteRenderer.TextType.BOLD,
            new RectangleF(0, 0, 0, 0), TextAlignment.Top | TextAlignment.HorizontalCenter, Color.White)
        {
            t = new Timer(tickDelta * 1000f);
         
            t.Elapsed += Timer_Tick;
            TimeRemaining = timeToCountInSeconds;
        }

        //Tick every 100 milliseconds.
        public void Timer_Tick(object o, ElapsedEventArgs elapsedEvent)
        {
            TimeRemaining = (float)(startingTime - actualTime.Elapsed.TotalSeconds);
            var time = TimeSpan.FromSeconds(TimeRemaining);
            UIText.Text = time.ToString(TimeFormatting);

            if (TimeRemaining < 0.0f)
            {
                t.Stop();
                OnTimerCompleted?.Invoke();
            }
        }
        //Restart ElapsedTime.
        public void Restart(float timeToCountInSeconds)
        {
            actualTime.Restart();

            TimeRemaining = timeToCountInSeconds;
            startingTime = timeToCountInSeconds;
            t.Start();
        }

        /// <summary>
        /// Reset the ElapsedTime
        /// </summary>
        public void End()
        {
            TimeRemaining = 0;
            UIText.Text = "Match Over";
            t.Stop();
        }

        /// <summary>
        /// Start the ElapsedTime
        /// </summary>
        /// <param name="timeToCountInSeconds">Amount to start at</param>
        public void Start(float timeToCountInSeconds)
        {
            t.AutoReset = true;
            t.Enabled = true;

            actualTime.Restart();

            startingTime = timeToCountInSeconds;

            TimeRemaining = (float)(startingTime - actualTime.Elapsed.TotalSeconds);
            var time = TimeSpan.FromSeconds(TimeRemaining);
            UIText.Text = time.ToString(TimeFormatting);
            t.Start();
        }

    }
}