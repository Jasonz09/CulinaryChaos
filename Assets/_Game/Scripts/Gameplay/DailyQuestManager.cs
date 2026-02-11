using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Daily quest system. Server-authoritative: quest generation uses server UTC
    /// date, progress updates go through CloudScript, rewards are granted server-side.
    /// No client-side DateTime.UtcNow usage.
    /// </summary>
    public class DailyQuestManager : MonoBehaviour
    {
        public static DailyQuestManager Instance { get; private set; }

        [Header("Quest Pool")]
        [SerializeField] private List<QuestTemplate> easyQuests;
        [SerializeField] private List<QuestTemplate> mediumQuests;
        [SerializeField] private List<QuestTemplate> hardQuests;

        private List<Quest> _activeQuests = new();
        private int _rerollsUsedToday;

        public IReadOnlyList<Quest> ActiveQuests => _activeQuests;

        private const string QUEST_DATA_KEY = "DailyQuestData";
        private const string SERVER_QUEST_KEY = "DailyQuestData";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (easyQuests == null) easyQuests = new List<QuestTemplate>();
            if (mediumQuests == null) mediumQuests = new List<QuestTemplate>();
            if (hardQuests == null) hardQuests = new List<QuestTemplate>();

            LoadQuestsCache();
        }

        private void Start()
        {
            if (PlayFabManager.Instance != null)
            {
                if (PlayFabManager.Instance.IsLoggedIn)
                    SyncFromServer();
                else
                    PlayFabManager.Instance.OnLoginSuccess += SyncFromServer;
            }
        }

        /// <summary>
        /// Asks server to check/generate daily quests using server UTC date.
        /// Server returns today's quest list (generates new ones if it's a new day).
        /// </summary>
        private void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            // Build quest pool summary for server-side generation
            var questPool = new List<object>();
            if (easyQuests != null)
                foreach (var q in easyQuests)
                    questPool.Add(new { q.questId, q.description, q.targetCount, q.creditReward, difficulty = "easy" });
            if (mediumQuests != null)
                foreach (var q in mediumQuests)
                    questPool.Add(new { q.questId, q.description, q.targetCount, q.creditReward, difficulty = "medium" });
            if (hardQuests != null)
                foreach (var q in hardQuests)
                    questPool.Add(new { q.questId, q.description, q.targetCount, q.creditReward, difficulty = "hard" });

            PlayFabManager.Instance.ExecuteCloudScript("CheckDailyQuests",
                new { questPool },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        var questsArray = result["quests"] as JArray;
                        if (questsArray != null)
                        {
                            _activeQuests.Clear();
                            foreach (var qToken in questsArray)
                            {
                                _activeQuests.Add(new Quest
                                {
                                    questId = qToken["questId"]?.Value<string>() ?? "",
                                    description = qToken["description"]?.Value<string>() ?? "",
                                    targetCount = qToken["targetCount"]?.Value<int>() ?? 0,
                                    currentCount = qToken["currentCount"]?.Value<int>() ?? 0,
                                    creditReward = qToken["creditReward"]?.Value<int>() ?? 0,
                                    isCompleted = qToken["isCompleted"]?.Value<bool>() ?? false,
                                    isClaimed = qToken["isClaimed"]?.Value<bool>() ?? false
                                });
                            }
                            _rerollsUsedToday = result["rerolls"]?.Value<int>() ?? 0;
                            SaveQuestsCache();
                            Debug.Log($"[DailyQuestManager] Synced {_activeQuests.Count} quests from server");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[DailyQuestManager] CheckDailyQuests parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[DailyQuestManager] CheckDailyQuests failed: {err}"));
        }

        /// <summary>
        /// Updates progress on matching quests via CloudScript.
        /// Server validates and persists the update.
        /// </summary>
        public void UpdateQuestProgress(string questType, int amount = 1)
        {
            if (amount <= 0) return;

            PlayFabManager.Instance?.ExecuteCloudScript("UpdateQuestProgress",
                new { questType, amount },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        var questsArray = result["quests"] as JArray;
                        if (questsArray != null)
                        {
                            _activeQuests.Clear();
                            foreach (var qToken in questsArray)
                            {
                                _activeQuests.Add(new Quest
                                {
                                    questId = qToken["questId"]?.Value<string>() ?? "",
                                    description = qToken["description"]?.Value<string>() ?? "",
                                    targetCount = qToken["targetCount"]?.Value<int>() ?? 0,
                                    currentCount = qToken["currentCount"]?.Value<int>() ?? 0,
                                    creditReward = qToken["creditReward"]?.Value<int>() ?? 0,
                                    isCompleted = qToken["isCompleted"]?.Value<bool>() ?? false,
                                    isClaimed = qToken["isClaimed"]?.Value<bool>() ?? false
                                });
                            }
                            SaveQuestsCache();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[DailyQuestManager] UpdateQuestProgress parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[DailyQuestManager] UpdateQuestProgress failed: {err}"));
        }

        /// <summary>
        /// Claims the reward for a completed quest via CloudScript.
        /// Server validates completion, marks claimed, and grants coins.
        /// </summary>
        public void ClaimReward(int questIndex, System.Action<bool> onComplete = null)
        {
            if (questIndex < 0 || questIndex >= _activeQuests.Count) { onComplete?.Invoke(false); return; }
            var quest = _activeQuests[questIndex];
            if (!quest.isCompleted || quest.isClaimed) { onComplete?.Invoke(false); return; }

            PlayFabManager.Instance?.ExecuteCloudScript("ClaimQuestReward",
                new { questIndex, questId = quest.questId, reward = quest.creditReward },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            quest.isClaimed = true;
                            SaveQuestsCache();
                            PlayFabManager.Instance?.RefreshCurrencies();
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            onComplete?.Invoke(false);
                        }
                    }
                    catch { onComplete?.Invoke(false); }
                },
                err => { Debug.LogWarning($"[DailyQuestManager] ClaimQuestReward failed: {err}"); onComplete?.Invoke(false); });
        }

        private void SaveQuestsCache()
        {
            string json = JsonUtility.ToJson(new QuestListWrapper { quests = _activeQuests });
            PlayerPrefs.SetString(QUEST_DATA_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadQuestsCache()
        {
            string json = PlayerPrefs.GetString(QUEST_DATA_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<QuestListWrapper>(json);
                    if (wrapper?.quests != null)
                        _activeQuests = wrapper.quests;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[DailyQuestManager] Cache load error: {ex.Message}");
                }
            }
        }
    }
}
