using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// An active customer order to be fulfilled.
    /// </summary>
    [System.Serializable]
    public class Order
    {
        /// <summary>
        /// Recipe this order requires.
        /// </summary>
        public RecipeSO recipe;

        /// <summary>
        /// Points awarded on completion.
        /// </summary>
        public int pointsReward;

        /// <summary>
        /// Total time allowed for this order.
        /// </summary>
        public float timeLimit;

        /// <summary>
        /// Seconds remaining before expiry.
        /// </summary>
        public float remainingTime;

        /// <summary>
        /// Fraction of time remaining (0-1).
        /// </summary>
        public float TimeRatio => Mathf.Clamp01(remainingTime / timeLimit);
    }
}
