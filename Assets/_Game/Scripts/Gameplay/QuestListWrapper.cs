using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// JSON serialization wrapper for a list of quests.
    /// </summary>
    [System.Serializable]
    public class QuestListWrapper
    {
        /// <summary>
        /// List of quest instances.
        /// </summary>
        public List<Quest> quests = new();
    }
}
