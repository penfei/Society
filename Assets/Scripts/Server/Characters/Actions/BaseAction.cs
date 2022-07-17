using System.Timers;

namespace Society.Characters
{
    public abstract class BaseAction
    {
        public delegate void ActionDelegate();

        public float Duration { get { return _duration; } }

        public event ActionDelegate OnActionEnd;
        public event ActionDelegate OnActionInterrupt;

        protected System.Random _random = new System.Random();
        protected float _duration;

        private Timer _timer;

        public virtual void Start(float hour)
        {
            _duration = (float)RandomizeDuration() * hour;

            _timer = new Timer(_duration);
            _timer.Elapsed += TimerEnd;
            _timer.AutoReset = false;
            _timer.Start();
        }

        public virtual void Interrupt()
        {
            OnActionInterrupt?.Invoke();
        }

        public virtual void End()
        {
            OnActionEnd?.Invoke();
        }

        protected virtual double RandomizeDuration()
        {
            return 0;
        }

        private void TimerEnd(object sender, ElapsedEventArgs e)
        {
            End();
        }
    }
}
