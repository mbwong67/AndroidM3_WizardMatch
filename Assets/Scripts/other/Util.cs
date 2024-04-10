using System;

namespace WizardMatch
{
    public class Timer
    {
        /// <summary>
        /// Remaining seconds left on this timer.
        /// </summary>
        public float RemaingSeconds { get; private set; }
        /// <summary>
        /// The maximum duration set during either construction or via Timer.SetTimer(float value)
        /// </summary>
        public float MaxDuration { get; private set; }

        public Timer(float duration)
        {
            RemaingSeconds = duration;
            MaxDuration = duration;
        }

        public event Action OnTimerStart; // <-- currently unused
        public event Action OnTimerEnd;
        public void Tick(float deltaTime)
        {
            if (RemaingSeconds == 0f) return;
            //Debug.Log(RemaingSeconds);
            RemaingSeconds -= deltaTime;

            CheckForTimerEnd();
        }
        public void SetTimer(float value)
        {
            RemaingSeconds = value;
            MaxDuration = value;
        }
        private void CheckForTimerEnd()
        {
            if (RemaingSeconds > 0f) { return; }
            RemaingSeconds = 0;
            OnTimerEnd?.Invoke();
        }
        public float GetRemaingingSeconds()
        {
            return RemaingSeconds;
        }
    }
}