using System;

namespace WizardMatch
{
    public class Timer
    {
        public float RemaingSeconds { get; private set; }
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