using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Economy
{
    /// <summary>
    /// Singleton manager tracking player account level and XP progression.
    /// Server-authoritative: XP is added via CloudScript AddPlayerXP handler.
    /// Local state is a cache populated from server on login.
    /// </summary>
    public class PlayerLevelManager : MonoBehaviour
    {
        public static PlayerLevelManager Instance { get; private set; }

        private int _currentLevel = 1;
        private int _currentXP;

        public int CurrentLevel => _currentLevel;
        public int CurrentXP => _currentXP;
        public int XPToNextLevel => GetXPForLevel(_currentLevel);
        public float XPProgress => Mathf.Clamp01((float)_currentXP / Mathf.Max(1, XPToNextLevel));

        public event System.Action<int> OnLevelUp;
        public event System.Action<int> OnXPChanged;

        private const string SERVER_LEVEL_KEY = "PlayerLevelData";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgressCache();
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
        /// Pulls level progress from server and overwrites local cache.
        /// </summary>
        private void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.GetUserData(
                new List<string> { SERVER_LEVEL_KEY },
                data =>
                {
                    if (data.TryGetValue(SERVER_LEVEL_KEY, out string json)
                        && !string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            var wrapper = JsonUtility.FromJson<PlayerLevelData>(json);
                            if (wrapper != null)
                            {
                                _currentLevel = wrapper.level;
                                _currentXP = wrapper.xp;
                                SaveProgressCache();
                                OnXPChanged?.Invoke(_currentXP);
                                Debug.Log($"[PlayerLevelManager] Synced from server: level {_currentLevel}, xp {_currentXP}");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[PlayerLevelManager] Server sync parse error: {ex.Message}");
                        }
                    }
                },
                err => Debug.LogWarning($"[PlayerLevelManager] Server sync failed: {err}"));
        }

        public int GetXPForLevel(int level) => 100 * level;

        /// <summary>
        /// Adds XP via CloudScript. Server processes level-ups and returns new state.
        /// </summary>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            PlayFabManager.Instance?.ExecuteCloudScript("AddPlayerXP",
                new { amount },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        int newLevel = result["level"]?.Value<int>() ?? _currentLevel;
                        int newXP = result["xp"]?.Value<int>() ?? _currentXP;

                        bool leveled = newLevel > _currentLevel;
                        _currentLevel = newLevel;
                        _currentXP = newXP;
                        SaveProgressCache();

                        OnXPChanged?.Invoke(_currentXP);
                        if (leveled) OnLevelUp?.Invoke(_currentLevel);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[PlayerLevelManager] AddXP parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[PlayerLevelManager] AddXP failed: {err}"));
        }

        private void SaveProgressCache()
        {
            PlayerPrefs.SetInt("PlayerLevel", _currentLevel);
            PlayerPrefs.SetInt("PlayerXP", _currentXP);
            PlayerPrefs.Save();
        }

        private void LoadProgressCache()
        {
            _currentLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            _currentXP = PlayerPrefs.GetInt("PlayerXP", 0);
        }

        [System.Serializable]
        private class PlayerLevelData
        {
            public int level;
            public int xp;
        }
    }
}
