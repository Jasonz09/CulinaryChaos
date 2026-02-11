using System.Collections.Generic;

namespace IOChef.Heroes
{
    /// <summary>
    /// JSON serialization wrapper for hero progress dictionary.
    /// </summary>
    [System.Serializable]
    public class HeroProgressWrapper
    {
        /// <summary>
        /// Hero ID keys.
        /// </summary>
        public List<string> keys = new();

        /// <summary>
        /// Corresponding save data values.
        /// </summary>
        public List<HeroSaveData> values = new();

        public HeroProgressWrapper() { }

        /// <summary>
        /// Create wrapper from a dictionary.
        /// </summary>
        /// <param name="dict">Hero progress dictionary to serialize.</param>
        public HeroProgressWrapper(Dictionary<string, HeroSaveData> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        /// <summary>
        /// Convert back to a dictionary.
        /// </summary>
        /// <returns>Dictionary mapping hero IDs to save data.</returns>
        public Dictionary<string, HeroSaveData> ToDictionary()
        {
            var dict = new Dictionary<string, HeroSaveData>();
            if (keys == null || values == null) return dict;
            for (int i = 0; i < keys.Count && i < values.Count; i++)
                dict[keys[i]] = values[i];
            return dict;
        }
    }
}
