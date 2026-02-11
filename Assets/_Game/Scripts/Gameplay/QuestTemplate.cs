namespace IOChef.Gameplay
{
    /// <summary>
    /// Template defining a quest's requirements and reward.
    /// </summary>
    [System.Serializable]
    public class QuestTemplate
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
        /// Credits awarded on completion.
        /// </summary>
        public int creditReward;
    }
}
