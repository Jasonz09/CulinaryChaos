using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// An active quest instance with progress tracking.
    /// </summary>
    [System.Serializable]
    public class Quest
    {
        /// <summary>
        /// Unique quest identifier.
        /// </summary>
        public string questId;

        /// <summary>
        /// Player-facing description.
        /// </summary>
        public string description;

        /// <summary>
        /// Number of actions required to complete.
        /// </summary>
        public int targetCount;

        /// <summary>
        /// Number of actions completed so far.
        /// </summary>
        public int currentCount;

        /// <summary>
        /// Credits awarded on completion.
        /// </summary>
        public int creditReward;

        /// <summary>
        /// Whether the quest goal has been met.
        /// </summary>
        public bool isCompleted;

        /// <summary>
        /// Whether the reward has been claimed.
        /// </summary>
        public bool isClaimed;

        /// <summary>
        /// Completion progress as a fraction (0-1).
        /// </summary>
        public float Progress => Mathf.Clamp01((float)currentCount / targetCount);
    }
}
