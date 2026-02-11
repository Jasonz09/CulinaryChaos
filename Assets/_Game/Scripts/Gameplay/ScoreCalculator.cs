using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Tracks score, combo streaks, and order statistics during a level.
    /// </summary>
    public class ScoreCalculator : MonoBehaviour
    {
        /// <summary>
        /// Time ratio threshold above which a speed bonus is awarded.
        /// </summary>
        [Header("Scoring Settings")]
        [SerializeField] private float speedBonusThreshold = 0.5f; // > 50% time left = speed bonus

        /// <summary>
        /// Combo multiplier increase per consecutive completed order.
        /// </summary>
        private const float COMBO_INCREMENT = 0.10f; // +10% per consecutive order

        /// <summary>
        /// Maximum combo multiplier cap.
        /// </summary>
        private const float COMBO_MAX = 2.0f;        // cap at 2x

        /// <summary>
        /// Accumulated score for the current level.
        /// </summary>
        private int _currentScore;

        /// <summary>
        /// Number of consecutive orders completed without failure.
        /// </summary>
        private int _comboStreak;

        /// <summary>
        /// Current score multiplier based on combo streak.
        /// </summary>
        private float _comboMultiplier = 1f;

        /// <summary>
        /// Total number of orders successfully completed.
        /// </summary>
        private int _ordersCompleted;

        /// <summary>
        /// Total number of orders that expired or failed.
        /// </summary>
        private int _ordersFailed;

        /// <summary>
        /// Highest combo streak achieved during this level.
        /// </summary>
        private int _bestCombo;

        /// <summary>
        /// Current accumulated score.
        /// </summary>
        public int CurrentScore => _currentScore;

        /// <summary>
        /// Current consecutive order streak.
        /// </summary>
        public int ComboStreak => _comboStreak;

        /// <summary>
        /// Active combo multiplier.
        /// </summary>
        public float ComboMultiplier => _comboMultiplier;

        /// <summary>
        /// Total orders completed this level.
        /// </summary>
        public int OrdersCompleted => _ordersCompleted;

        /// <summary>
        /// Total orders failed this level.
        /// </summary>
        public int OrdersFailed => _ordersFailed;

        /// <summary>
        /// Highest combo streak achieved.
        /// </summary>
        public int BestCombo => _bestCombo;

        /// <summary>
        /// Fires when the score changes.
        /// </summary>
        public event System.Action<int> OnScoreChanged;

        /// <summary>
        /// Fires when the combo streak changes.
        /// </summary>
        public event System.Action<int> OnComboChanged;

        /// <summary>
        /// Reset all scoring state for a new level.
        /// </summary>
        public void Initialize()
        {
            _currentScore = 0;
            _comboStreak = 0;
            _comboMultiplier = 1f;
            _ordersCompleted = 0;
            _ordersFailed = 0;
            _bestCombo = 0;
        }

        /// <summary>
        /// Process a completed order, awarding points and updating combo.
        /// </summary>
        /// <param name="order">The completed order.</param>
        public void OrderCompleted(Order order)
        {
            _ordersCompleted++;

            // Base points
            int points = order.pointsReward;

            // Speed bonus
            float timeRatio = order.TimeRatio;
            if (timeRatio > speedBonusThreshold)
            {
                int speedBonus = Mathf.RoundToInt(order.recipe.bonusForSpeed * timeRatio);
                points += speedBonus;
            }

            // Hero score modifier
            if (Heroes.HeroManager.Instance != null)
            {
                var mods = Heroes.HeroManager.Instance.GetActiveModifiers();
                float scoreMult = mods.scoreMultiplier;

                // ScoreBoost ability: adds extra percentage on top
                if (Heroes.HeroManager.Instance.GetActiveSpecialAbilityType() == Heroes.SpecialAbilityType.ScoreBoost)
                    scoreMult += Heroes.HeroManager.Instance.GetActiveSpecialAbilityValue();

                points = Mathf.RoundToInt(points * scoreMult);
            }

            // Combo multiplier (capped)
            _comboStreak++;
            _comboMultiplier = Mathf.Min(1f + (_comboStreak - 1) * COMBO_INCREMENT, COMBO_MAX);
            points = Mathf.RoundToInt(points * _comboMultiplier);

            if (_comboStreak > _bestCombo)
                _bestCombo = _comboStreak;

            _currentScore += points;
            OnScoreChanged?.Invoke(_currentScore);
            OnComboChanged?.Invoke(_comboStreak);
        }

        /// <summary>
        /// Process a failed order, applying penalty and breaking combo.
        /// </summary>
        public void OrderFailed()
        {
            _ordersFailed++;

            // Check hero ability: some heroes don't lose combo on fail
            bool keepCombo = false;
            if (Heroes.HeroManager.Instance != null)
            {
                var hero = Heroes.HeroManager.Instance.SelectedHero;
                if (hero != null && hero.specialAbilityType == Heroes.SpecialAbilityType.ComboKeep)
                {
                    keepCombo = true;
                }
            }

            if (!keepCombo)
            {
                _comboStreak = 0;
                _comboMultiplier = 1f;
                OnComboChanged?.Invoke(_comboStreak);
            }

            // Penalty
            int penalty = 100;
            _currentScore = Mathf.Max(0, _currentScore - penalty);
            OnScoreChanged?.Invoke(_currentScore);
        }

        /// <summary>
        /// Get star rating for current score.
        /// </summary>
        /// <param name="levelData">Level data with star thresholds.</param>
        /// <returns>Star count (0-3).</returns>
        public int GetStarRating(LevelDataSO levelData)
        {
            return levelData.GetStarRating(_currentScore);
        }

        /// <summary>
        /// Build end-of-level results snapshot.
        /// </summary>
        /// <param name="levelData">Level data with star thresholds.</param>
        /// <returns>LevelResults struct with final stats.</returns>
        public LevelResults GetResults(LevelDataSO levelData)
        {
            return new LevelResults
            {
                finalScore = _currentScore,
                starRating = GetStarRating(levelData),
                ordersCompleted = _ordersCompleted,
                ordersFailed = _ordersFailed,
                bestCombo = _bestCombo
            };
        }
    }
}
