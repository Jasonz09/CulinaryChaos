namespace IOChef.Gameplay
{
    /// <summary>
    /// End-of-level results snapshot.
    /// </summary>
    [System.Serializable]
    public struct LevelResults
    {
        /// <summary>
        /// Final accumulated score.
        /// </summary>
        public int finalScore;

        /// <summary>
        /// Star rating earned (0-3).
        /// </summary>
        public int starRating;

        /// <summary>
        /// Total orders successfully completed.
        /// </summary>
        public int ordersCompleted;

        /// <summary>
        /// Total orders that expired or failed.
        /// </summary>
        public int ordersFailed;

        /// <summary>
        /// Highest combo streak achieved.
        /// </summary>
        public int bestCombo;
    }
}
