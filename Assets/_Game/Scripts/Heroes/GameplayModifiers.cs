namespace IOChef.Heroes
{
    /// <summary>
    /// Serializable set of hero-specific gameplay modifiers applied during a level.
    /// </summary>
    [System.Serializable]
    public struct GameplayModifiers
    {
        /// <summary>
        /// Multiplier applied to cooking duration.
        /// </summary>
        public float cookTimeMultiplier;

        /// <summary>
        /// Multiplier applied to the time before food burns.
        /// </summary>
        public float burnTimeMultiplier;

        /// <summary>
        /// Multiplier applied to earned score.
        /// </summary>
        public float scoreMultiplier;

        /// <summary>
        /// Multiplier applied to player movement speed.
        /// </summary>
        public float movementSpeedMultiplier;

        /// <summary>
        /// Multiplier applied to the player interaction radius.
        /// </summary>
        public float interactionRadiusMultiplier;

        /// <summary>
        /// Additional bonus time in seconds added to the level timer.
        /// </summary>
        public int bonusTimeSeconds;

        /// <summary>
        /// Maximum number of items the player can carry at once.
        /// </summary>
        public int maxCarryItems;

        /// <summary>
        /// Returns a default set of modifiers with all multipliers at 1x and no bonuses.
        /// </summary>
        public static GameplayModifiers Default => new GameplayModifiers
        {
            cookTimeMultiplier = 1f,
            burnTimeMultiplier = 1f,
            scoreMultiplier = 1f,
            movementSpeedMultiplier = 1f,
            interactionRadiusMultiplier = 1f,
            bonusTimeSeconds = 0,
            maxCarryItems = 1
        };
    }
}
