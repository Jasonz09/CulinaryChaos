namespace IOChef.Heroes
{
    /// <summary>
    /// Persistent save data for a single hero.
    /// </summary>
    [System.Serializable]
    public class HeroSaveData
    {
        /// <summary>
        /// Whether this hero has been unlocked.
        /// </summary>
        public bool isUnlocked;

        /// <summary>
        /// Current hero level.
        /// </summary>
        public int currentLevel = 1;

        /// <summary>
        /// Accumulated XP toward the next level.
        /// </summary>
        public float currentXP;
    }
}
