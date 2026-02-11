using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// ScriptableObject defining level configuration, timing, and scoring.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevel", menuName = "IOChef/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Level Info")]
        /// <summary>
        /// Unique level identifier.
        /// </summary>
        public int levelId;

        /// <summary>
        /// World this level belongs to.
        /// </summary>
        public int worldId = 1;

        /// <summary>
        /// Level number within its world.
        /// </summary>
        public int levelNumber = 1;

        /// <summary>
        /// Display name of the level.
        /// </summary>
        public string levelName;

        /// <summary>
        /// Thumbnail sprite for level select screen.
        /// </summary>
        public Sprite levelThumbnail;

        /// <summary>
        /// Formatted display ID (e.g. "1-3").
        /// </summary>
        public string DisplayId => $"{worldId}-{levelNumber}";

        [Header("Grid")]
        /// <summary>
        /// Kitchen grid width in cells.
        /// </summary>
        public int gridWidth = 8;

        /// <summary>
        /// Kitchen grid height in cells.
        /// </summary>
        public int gridHeight = 6;

        [Header("Equipment Layout")]
        /// <summary>
        /// Equipment placements on the grid.
        /// </summary>
        public List<EquipmentSpawn> equipment;

        [Header("Recipes")]
        /// <summary>
        /// Recipes available in this level.
        /// </summary>
        public List<RecipeSO> availableRecipes;

        [Header("Timing")]
        /// <summary>
        /// Total time limit in seconds.
        /// </summary>
        public float timeLimitSeconds = 300f;

        /// <summary>
        /// Seconds between new order spawns.
        /// </summary>
        public float orderSpawnInterval = 60f;

        /// <summary>
        /// Maximum simultaneous active orders.
        /// </summary>
        public int maxActiveOrders = 4;

        /// <summary>
        /// Number of orders at level start.
        /// </summary>
        public int initialOrders = 1;

        [Header("Scoring")]
        /// <summary>
        /// Score threshold for 1 star.
        /// </summary>
        public int threshold1Star = 300;

        /// <summary>
        /// Score threshold for 2 stars.
        /// </summary>
        public int threshold2Star = 500;

        /// <summary>
        /// Score threshold for 3 stars.
        /// </summary>
        public int threshold3Star = 700;

        [Header("Dish Management")]
        /// <summary>
        /// Whether plates are unlimited.
        /// </summary>
        public bool unlimitedPlates = true;

        /// <summary>
        /// Whether served plates are auto-removed.
        /// </summary>
        public bool autoRemovePlates = true;

        /// <summary>
        /// Whether this level requires a sink station.
        /// </summary>
        public bool requiresSink = false;

        /// <summary>
        /// Number of plates available (when not unlimited).
        /// </summary>
        public int plateCount = 0;

        [Header("Difficulty")]
        /// <summary>
        /// Difficulty tier (1 = easiest).
        /// </summary>
        public int difficultyLevel = 1;

        [Header("Economy")]
        /// <summary>
        /// Coin cost to start this level. Zero means free.
        /// </summary>
        public int entryCost = 0;

        /// <summary>
        /// Hero ID unlocked for free upon first completion of this level.
        /// Empty means no hero reward.
        /// </summary>
        public string freeHeroRewardId = "";

        /// <summary>
        /// Get the star rating for a given score.
        /// </summary>
        /// <param name="score">Player's final score.</param>
        /// <returns>Star count (0-3).</returns>
        public int GetStarRating(int score)
        {
            if (score >= threshold3Star) return 3;
            if (score >= threshold2Star) return 2;
            if (score >= threshold1Star) return 1;
            return 0;
        }
    }
}
