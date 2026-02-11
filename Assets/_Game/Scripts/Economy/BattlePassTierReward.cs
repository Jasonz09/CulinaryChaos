using UnityEngine;

namespace IOChef.Economy
{
    /// <summary>
    /// Defines a reward at a specific battle pass tier.
    /// </summary>
    [System.Serializable]
    public class BattlePassTierReward
    {
        /// <summary>
        /// Tier number this reward is at.
        /// </summary>
        public int tier;

        /// <summary>
        /// Whether this reward is on the free track.
        /// </summary>
        public bool isFreeTrack;

        /// <summary>
        /// Type of reward granted.
        /// </summary>
        public RewardType rewardType;

        /// <summary>
        /// ID of the rewarded item (cosmetic ID, hero ID, etc.).
        /// </summary>
        public string rewardId;

        /// <summary>
        /// Amount for currency rewards.
        /// </summary>
        public int rewardAmount;

        /// <summary>
        /// Icon displayed in the reward UI.
        /// </summary>
        public Sprite rewardIcon;

        /// <summary>
        /// Player-facing reward description.
        /// </summary>
        public string description;
    }
}
