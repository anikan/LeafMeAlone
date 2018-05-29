using System.Drawing;
using System.Timers;
using AntTweakBar;
using SlimDX;
using SpriteTextRenderer;

namespace Client
{
    public class UITimer
    {
        public delegate void TimerCompleted();
        public TimerCompleted OnTimerCompleted;

        //time delta (in seconds)
        private const float tickDelta = 100.0f / 1000.0f;

        //Time remaining (in seconds)
        public float TimeRemaining { get; private set; }

        private readonly Timer t;

        private readonly DrawableString uiElem;

        public UITimer(float timeToCountInSeconds)
        {
            t = new Timer(tickDelta * 1000f);
            t.Elapsed += Timer_Tick;
            TimeRemaining = timeToCountInSeconds;
            uiElem = UIManagerSpriteRenderer.DrawTextContinuous("Time Remaining:" + TimeRemaining, UIManagerSpriteRenderer.TextType.NORMAL, 
                new RectangleF(0, 0, GraphicsRenderer.Form.Width, GraphicsRenderer.Form.Height), TextAlignment.Top | TextAlignment.Left, Color.White);
        }

        public void Timer_Tick(object o, ElapsedEventArgs elapsedEvent)
        {
            TimeRemaining -= tickDelta;
            uiElem.Text = "Time Remaining:" + TimeRemaining;

            if (TimeRemaining < 0.0f)
            {
                t.Stop();
                OnTimerCompleted?.Invoke();
            }
        }

        public void Restart(float timetoCountInSeconds)
        {
            TimeRemaining = timetoCountInSeconds;
            t.Start();
        }

        public void Start(float timeToCountInSeconds)
        {
            t.AutoReset = true;
            t.Enabled = true;
            TimeRemaining = timeToCountInSeconds;
            uiElem.Text = "Time Remaining:" + TimeRemaining;
            t.Start();
        }

    }
}