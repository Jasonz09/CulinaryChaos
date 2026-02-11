using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;

namespace IOChef.Heroes
{
    /// <summary>
    /// Manages hero selection, unlocking, progression, and save data.
    /// Server-authoritative: all writes go through CloudScript. Local state is a
    /// read-only cache populated from server on login.
    /// </summary>
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        [Header("Hero Database")]
        [SerializeField] private List<HeroDataSO> allHeroes;

        [Header("Runtime State")]
        [SerializeField] private HeroDataSO selectedHero;

        private Dictionary<string, HeroSaveData> _heroProgress = new();
        private Dictionary<string, ServerHeroEntry> _serverCatalog = new();

        /// <summary>
        /// Lightweight server catalog entry for hero metadata validation.
        /// </summary>
        private struct ServerHeroEntry
        {
            public string heroId;
            public string heroName;
            public string rarity;
            public bool isFreeHero;
            public int maxLevel;
        }

        public HeroDataSO SelectedHero => selectedHero;

        private const string SERVER_HERO_KEY = "HeroProgress";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHeroProgress();
        }

        private void Start()
        {
            if (PlayFabManager.Instance != null)
            {
                if (PlayFabManager.Instance.IsLoggedIn)
                {
                    SyncFromServer();
                    SyncHeroCatalog();
                }
                else
                {
                    PlayFabManager.Instance.OnLoginSuccess += SyncFromServer;
                    PlayFabManager.Instance.OnLoginSuccess += SyncHeroCatalog;
                }
            }
        }

        /// <summary>
        /// Pulls hero progress from server and overwrites local cache.
        /// </summary>
        private void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.GetUserData(
                new List<string> { SERVER_HERO_KEY },
                data =>
                {
                    if (data.TryGetValue(SERVER_HERO_KEY, out string json)
                        && !string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            var wrapper = JsonUtility.FromJson<HeroProgressWrapper>(json);
                            if (wrapper != null)
                                _heroProgress = wrapper.ToDictionary();
                            SaveHeroProgress();
                            Debug.Log("[HeroManager] Synced hero progress from server");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[HeroManager] Server sync parse error: {ex.Message}");
                        }
                    }
                },
                err => Debug.LogWarning($"[HeroManager] Server sync failed: {err}"));
        }

        /// <summary>
        /// Fetches the server-authoritative hero catalog via CloudScript.
        /// Used to validate hero metadata (rarity, maxLevel) matches local ScriptableObjects.
        /// </summary>
        private void SyncHeroCatalog()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.ExecuteCloudScript("GetHeroCatalog", null,
                resultJson =>
                {
                    try
                    {
                        var result = Newtonsoft.Json.Linq.JObject.Parse(resultJson);
                        var heroesArray = result["heroes"] as Newtonsoft.Json.Linq.JArray;
                        if (heroesArray != null)
                        {
                            _serverCatalog.Clear();
                            foreach (var h in heroesArray)
                            {
                                var entry = new ServerHeroEntry
                                {
                                    heroId = h["heroId"]?.ToString() ?? "",
                                    heroName = h["heroName"]?.ToString() ?? "",
                                    rarity = h["rarity"]?.ToString() ?? "Common",
                                    isFreeHero = h["isFreeHero"]?.ToObject<bool>() ?? false,
                                    maxLevel = int.TryParse(h["maxLevel"]?.ToString(), out int ml) ? ml : 10
                                };
                                _serverCatalog[entry.heroId] = entry;
                            }
                            Debug.Log($"[HeroManager] Synced hero catalog: {_serverCatalog.Count} heroes");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[HeroManager] Catalog sync parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[HeroManager] Catalog sync failed: {err}"));
        }

        /// <summary>
        /// Returns the server max level for a hero, falling back to the ScriptableObject value.
        /// </summary>
        public int GetServerMaxLevel(string heroId)
        {
            if (_serverCatalog.TryGetValue(heroId, out var entry))
                return entry.maxLevel;
            var hero = allHeroes?.Find(h => h.heroId == heroId);
            return hero != null ? hero.maxLevel : 10;
        }

        public GameplayModifiers GetActiveModifiers()
        {
            if (selectedHero == null) return GameplayModifiers.Default;
            int level = GetHeroLevel(selectedHero.heroId);
            return selectedHero.ToModifiers(level);
        }

        /// <summary>
        /// Returns the active hero's special ability type, or None if no hero selected.
        /// </summary>
        public SpecialAbilityType GetActiveSpecialAbilityType()
        {
            if (selectedHero == null) return SpecialAbilityType.None;
            return selectedHero.specialAbilityType;
        }

        /// <summary>
        /// Returns the active hero's special ability value scaled by their current level.
        /// </summary>
        public float GetActiveSpecialAbilityValue()
        {
            if (selectedHero == null) return 0f;
            int level = GetHeroLevel(selectedHero.heroId);
            return selectedHero.GetSpecialAbilityValue(level);
        }

        public void SelectHero(string heroId)
        {
            if (allHeroes == null) return;
            var hero = allHeroes.Find(h => h.heroId == heroId);
            if (hero != null && IsHeroUnlocked(heroId))
                selectedHero = hero;
        }

        public bool IsHeroUnlocked(string heroId)
        {
            if (allHeroes == null) return false;
            var hero = allHeroes.Find(h => h.heroId == heroId);
            if (hero == null) return false;
            if (hero.isFreeHero) return true;
            return _heroProgress.ContainsKey(heroId) && _heroProgress[heroId].isUnlocked;
        }

        /// <summary>
        /// Updates local cache when server confirms hero unlock.
        /// Called from CloudScript response handlers (OpenChest, CompleteLevel).
        /// </summary>
        public void UnlockHero(string heroId)
        {
            if (!_heroProgress.ContainsKey(heroId))
                _heroProgress[heroId] = new HeroSaveData();
            _heroProgress[heroId].isUnlocked = true;
            SaveHeroProgress();
        }

        public int GetHeroLevel(string heroId)
        {
            return _heroProgress.ContainsKey(heroId) ? _heroProgress[heroId].currentLevel : 1;
        }

        public int GetUpgradeCost(string heroId)
        {
            if (allHeroes == null) return -1;
            var hero = allHeroes.Find(h => h.heroId == heroId);
            if (hero == null) return -1;
            int level = GetHeroLevel(heroId);
            if (level >= hero.maxLevel) return -1;
            return 10 * level;
        }

        /// <summary>
        /// Upgrades a hero via CloudScript. Server validates token cost and max level.
        /// </summary>
        public void UpgradeHero(string heroId, System.Action<bool> onComplete = null)
        {
            int cost = GetUpgradeCost(heroId);
            if (cost < 0) { onComplete?.Invoke(false); return; }
            if (!Economy.CurrencyManager.Instance.CanAffordHeroTokens(cost)) { onComplete?.Invoke(false); return; }

            PlayFabManager.Instance?.ExecuteCloudScript("UpgradeHero",
                new { heroId },
                resultJson =>
                {
                    try
                    {
                        var result = Newtonsoft.Json.Linq.JObject.Parse(resultJson);
                        bool success = result["success"] != null && bool.Parse(result["success"].ToString());
                        if (success)
                        {
                            int newLevel = int.TryParse(result["newLevel"]?.ToString(), out int nl) ? nl : GetHeroLevel(heroId) + 1;
                            if (!_heroProgress.ContainsKey(heroId))
                                _heroProgress[heroId] = new HeroSaveData();
                            _heroProgress[heroId].currentLevel = newLevel;
                            SaveHeroProgress();
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
                err => { Debug.LogWarning($"[HeroManager] UpgradeHero failed: {err}"); onComplete?.Invoke(false); });
        }

        public List<HeroDataSO> GetAllHeroes() => allHeroes ?? new List<HeroDataSO>();

        public HeroDataSO GetHeroById(string heroId)
        {
            return allHeroes?.Find(h => h.heroId == heroId);
        }

        public List<HeroDataSO> GetUnlockedHeroes()
        {
            if (allHeroes == null) return new List<HeroDataSO>();
            return allHeroes.FindAll(h => IsHeroUnlocked(h.heroId));
        }

        private void SaveHeroProgress()
        {
            string json = JsonUtility.ToJson(new HeroProgressWrapper(_heroProgress));
            PlayerPrefs.SetString("HeroProgress", json);
            PlayerPrefs.Save();
        }

        private void LoadHeroProgress()
        {
            try
            {
                string json = PlayerPrefs.GetString("HeroProgress", "");
                if (!string.IsNullOrEmpty(json))
                {
                    var wrapper = JsonUtility.FromJson<HeroProgressWrapper>(json);
                    if (wrapper != null)
                        _heroProgress = wrapper.ToDictionary();
                }
                if (allHeroes == null || allHeroes.Count == 0)
                {
                    var loaded = Resources.LoadAll<HeroDataSO>("Heroes");
                    allHeroes = new List<HeroDataSO>(loaded);
                    Debug.Log($"[HeroManager] Loaded {allHeroes.Count} heroes from Resources");
                }
                if (selectedHero == null && allHeroes.Count > 0) selectedHero = allHeroes[0];
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HeroManager] Failed to load hero progress: {ex.Message}");
                allHeroes = new List<HeroDataSO>();
            }
        }
    }
}
