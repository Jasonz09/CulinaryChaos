using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Manages the countdown timer for a kitchen level.
    /// </summary>
    public class GameTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        /// <summary>
        /// Total time in seconds allocated for the level.
        /// </summary>
        [SerializeField] private float totalTime = 300f; // 5 minutes

        /// <summary>
        /// Seconds remaining on the countdown timer.
        /// </summary>
        private float _remainingTime;
        /// <summary>
        /// Whether the timer is currently counting down.
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Gets the number of seconds remaining on the timer.
        /// </summary>
        public float RemainingTime => _remainingTime;

        /// <summary>
        /// Gets the total time allocated for this level in seconds.
        /// </summary>
        public float TotalTime => totalTime;

        /// <summary>
        /// Gets the remaining time as a normalized ratio from 0 to 1.
        /// </summary>
        public float TimeRatio => Mathf.Clamp01(_remainingTime / totalTime);

        /// <summary>
        /// Gets whether the timer is currently counting down.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Gets whether the remaining time is below 60 seconds.
        /// </summary>
        public bool IsLowTime => _remainingTime < 60f;

        /// <summary>
        /// Gets whether the remaining time is below 30 seconds.
        /// </summary>
        public bool IsCriticalTime => _remainingTime < 30f;

        /// <summary>
        /// Raised when the timer starts counting down.
        /// </summary>
        public event System.Action OnTimerStarted;

        /// <summary>
        /// Raised when the timer reaches zero.
        /// </summary>
        public event System.Action OnTimerExpired;

        /// <summary>
        /// Raised each frame with the current remaining time in seconds.
        /// </summary>
        public event System.Action<float> OnTimerUpdated;

        /// <summary>
        /// Initializes the timer with the specified duration, including any hero bonus time.
        /// </summary>
        /// <param name="time">The base time in seconds for this level.</param>
        public void Initialize(float time)
        {
            totalTime = time;

            // Apply hero bonus time
            int bonusTime = 0;
            if (Heroes.HeroManager.Instance != null)
            {
                var mods = Heroes.HeroManager.Instance.GetActiveModifiers();
                bonusTime = mods.bonusTimeSeconds;
            }

            _remainingTime = totalTime + bonusTime;
            _isRunning = false;
        }

        /// <summary>
        /// Starts the countdown timer.
        /// </summary>
        public void StartTimer()
        {
            _isRunning = true;
            OnTimerStarted?.Invoke();
        }

        /// <summary>
        /// Pauses the countdown timer.
        /// </summary>
        public void PauseTimer()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Resumes the countdown timer after a pause.
        /// </summary>
        public void ResumeTimer()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Adds bonus seconds to the remaining time.
        /// </summary>
        /// <param name="seconds">The number of seconds to add.</param>
        public void AddTime(float seconds)
        {
            _remainingTime += seconds;
        }

        /// <summary>
        /// Decrements the timer each frame and fires expiry event at zero.
        /// </summary>
        private void Update()
        {
            if (!_isRunning) return;

            _remainingTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(_remainingTime);

            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                _isRunning = false;
                OnTimerExpired?.Invoke();
            }
        }

        /// <summary>
        /// Returns the remaining time formatted as MM:SS.
        /// </summary>
        /// <returns>A string in the format "MM:SS".</returns>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(_remainingTime / 60f);
            int seconds = Mathf.FloorToInt(_remainingTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
