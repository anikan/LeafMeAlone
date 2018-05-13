using System.Drawing;
using System.Timers;
using AntTweakBar;

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

        private System.Timers.Timer t;

        public FloatVariable TimeRemaining_UI;

        public UITimer(float timeToCountInSeconds, Size size, Point location)
        {
            t = new Timer(tickDelta * 1000f);
            t.Elapsed += Timer_Tick;
            TimeRemaining = timeToCountInSeconds;
            TimeRemaining_UI =
                new FloatVariable(UIManager.Create("Timer", size, location))
                {
                    ReadOnly = true,
                    Label = "Time Remaining: ",
                    Precision = 2,
                    Value = timeToCountInSeconds
                };
        }

        public void Timer_Tick(object o, ElapsedEventArgs elapsedEvent)
        {
            TimeRemaining -= tickDelta;
            TimeRemaining_UI.Value = TimeRemaining;

            if (TimeRemaining < 0.0f)
            {
                t.Stop();
                TimeRemaining_UI.Value = 0;
                OnTimerCompleted?.Invoke();
            }
        }

        public void Restart(float timetoCountInSeconds)
        {
            TimeRemaining = timetoCountInSeconds;
            TimeRemaining_UI.Value = timetoCountInSeconds;
            t.Start();
        }

        public void Start(float timeToCountInSeconds)
        {
            t.AutoReset = true;
            t.Enabled = true;
            TimeRemaining = timeToCountInSeconds;
            TimeRemaining_UI.Value = timeToCountInSeconds;
            t.Start();
        }

    }
}