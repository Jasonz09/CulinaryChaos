using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// ScriptableObject database holding all level definitions for the game.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "IOChef/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        /// <summary>
        /// List of all level data assets in the game.
        /// </summary>
        [SerializeField] private List<LevelDataSO> levels = new();

        /// <summary>
        /// The total number of levels in the database.
        /// </summary>
        public int LevelCount => levels.Count;

        /// <summary>
        /// Retrieves a level by its unique identifier.
        /// </summary>
        /// <param name="levelId">The unique level identifier to search for.</param>
        /// <returns>The matching LevelDataSO, or null if not found.</returns>
        public LevelDataSO GetLevel(int levelId)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelId == levelId)
                    return levels[i];
            }
            return null;
        }

        /// <summary>
        /// Retrieves a level by its list index.
        /// </summary>
        /// <param name="index">The zero-based index into the levels list.</param>
        /// <returns>The LevelDataSO at the given index, or null if out of range.</returns>
        public LevelDataSO GetLevelByIndex(int index)
        {
            if (index < 0 || index >= levels.Count) return null;
            return levels[index];
        }

        /// <summary>
        /// Returns all levels belonging to the specified world, sorted by level number.
        /// </summary>
        /// <param name="worldId">The world identifier to filter by.</param>
        /// <returns>A sorted list of levels in the given world.</returns>
        public List<LevelDataSO> GetLevelsForWorld(int worldId)
        {
            var result = new List<LevelDataSO>();
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].worldId == worldId)
                    result.Add(levels[i]);
            }
            result.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
            return result;
        }

        /// <summary>
        /// Counts the number of distinct worlds across all levels.
        /// </summary>
        /// <returns>The total number of unique world identifiers.</returns>
        public int GetWorldCount()
        {
            var worlds = new HashSet<int>();
            for (int i = 0; i < levels.Count; i++)
                worlds.Add(levels[i].worldId);
            return worlds.Count;
        }
    }
}
