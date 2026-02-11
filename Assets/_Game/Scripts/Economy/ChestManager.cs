using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using IOChef.Heroes;
using Newtonsoft.Json.Linq;

namespace IOChef.Economy
{
    public class ChestManager : MonoBehaviour
    {
        public static ChestManager Instance { get; private set; }

        [SerializeField] private int bronzeChestCost = 0;
        [SerializeField] private int silverChestCost = 50;
        [SerializeField] private int goldChestCost = 150;

        public event System.Action<string, bool, string> OnChestOpened; // heroId, wasNew, heroRarity

        private bool _isOpening;
        private float _bronzeCooldownRemaining;

        public float BronzeCooldownRemaining => _bronzeCooldownRemaining;
        public bool IsBronzeOnCooldown => _bronzeCooldownRemaining > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_bronzeCooldownRemaining > 0)
                _bronzeCooldownRemaining -= Time.unscaledDeltaTime;
        }

        public int GetChestCost(ChestRarity rarity)
        {
            return rarity switch
            {
                ChestRarity.Bronze => bronzeChestCost,
                ChestRarity.Silver => silverChestCost,
                ChestRarity.Gold => goldChestCost,
                _ => bronzeChestCost,
            };
        }

        public bool CanAffordChest(ChestRarity rarity)
        {
            if (rarity == ChestRarity.Bronze) return !IsBronzeOnCooldown;
            return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAffordPremium(GetChestCost(rarity));
        }

        public void SyncCooldown()
        {
            PlayFabManager.Instance?.ExecuteCloudScript("GetPityCounters", new { },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        _bronzeCooldownRemaining = result["bronzeCooldownRemaining"]?.Value<float>() ?? 0;
                        Debug.Log($"[ChestManager] Cooldown synced: {_bronzeCooldownRemaining}s remaining");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[ChestManager] Cooldown sync error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[ChestManager] Cooldown sync failed: {err}"));
        }

        public void OpenChest(ChestRarity rarity, System.Action<string> onComplete = null)
        {
            if (_isOpening) { onComplete?.Invoke(null); return; }

            // Bronze: check cooldown instead of gem cost
            if (rarity == ChestRarity.Bronze && IsBronzeOnCooldown)
            {
                onComplete?.Invoke(null);
                return;
            }

            int cost = GetChestCost(rarity);
            if (cost > 0 && (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAffordPremium(cost)))
            {
                onComplete?.Invoke(null);
                return;
            }
            if (HeroManager.Instance == null) { onComplete?.Invoke(null); return; }

            _isOpening = true;
            PlayFabManager.Instance?.ExecuteCloudScript("OpenChest",
                new { rarity = rarity.ToString() },
                resultJson =>
                {
                    _isOpening = false;
                    try
                    {
                        var result = JObject.Parse(resultJson);

                        // Check for server-side error (cooldown, not enough gems, etc.)
                        string error = result["error"]?.ToString();
                        if (!string.IsNullOrEmpty(error))
                        {
                            if (result["cooldownRemaining"] != null)
                                _bronzeCooldownRemaining = result["cooldownRemaining"].Value<float>();
                            Debug.LogWarning($"[ChestManager] Server error: {error}");
                            onComplete?.Invoke(null);
                            return;
                        }

                        string heroId = result["heroId"]?.ToString();
                        bool wasNew = result["wasNew"]?.Value<bool>() ?? false;
                        string heroRarity = result["heroRarity"]?.ToString() ?? "Common";

                        // Start cooldown locally for bronze
                        if (rarity == ChestRarity.Bronze)
                            _bronzeCooldownRemaining = 86400f;

                        if (!string.IsNullOrEmpty(heroId))
                        {
                            if (wasNew)
                                HeroManager.Instance?.UnlockHero(heroId);

                            OnChestOpened?.Invoke(heroId, wasNew, heroRarity);
                            PlayFabManager.Instance?.RefreshCurrencies();
                            Debug.Log($"[ChestManager] Chest opened: {heroId} ({heroRarity}, new={wasNew})");
                        }
                        onComplete?.Invoke(heroId);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[ChestManager] Chest parse error: {ex.Message}");
                        onComplete?.Invoke(null);
                    }
                },
                err =>
                {
                    _isOpening = false;
                    Debug.LogWarning($"[ChestManager] OpenChest failed: {err}");
                    onComplete?.Invoke(null);
                });
        }

        public void OpenChestMulti(ChestRarity rarity, int count = 10,
            System.Action<List<ChestResult>> onComplete = null)
        {
            if (_isOpening) { onComplete?.Invoke(null); return; }

            // Bronze can't be multi-pulled
            if (rarity == ChestRarity.Bronze) { onComplete?.Invoke(null); return; }

            int totalCost = GetChestCost(rarity) * count;
            if (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAffordPremium(totalCost))
            {
                onComplete?.Invoke(null);
                return;
            }

            _isOpening = true;
            PlayFabManager.Instance?.ExecuteCloudScript("OpenChestMulti",
                new { rarity = rarity.ToString(), count },
                resultJson =>
                {
                    _isOpening = false;
                    try
                    {
                        var result = JObject.Parse(resultJson);

                        string error = result["error"]?.ToString();
                        if (!string.IsNullOrEmpty(error))
                        {
                            Debug.LogWarning($"[ChestManager] Multi-chest server error: {error}");
                            onComplete?.Invoke(null);
                            return;
                        }

                        var resultsArr = result["results"] as JArray;
                        var results = new List<ChestResult>();
                        if (resultsArr != null)
                        {
                            foreach (var r in resultsArr)
                            {
                                var cr = new ChestResult
                                {
                                    heroId = r["heroId"]?.ToString() ?? "",
                                    wasNew = r["wasNew"]?.Value<bool>() ?? false,
                                    heroRarity = r["heroRarity"]?.ToString() ?? "Common"
                                };
                                results.Add(cr);

                                if (cr.wasNew)
                                    HeroManager.Instance?.UnlockHero(cr.heroId);
                            }
                        }

                        PlayFabManager.Instance?.RefreshCurrencies();
                        Debug.Log($"[ChestManager] Multi-chest opened: {results.Count} results");
                        onComplete?.Invoke(results);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[ChestManager] Multi-chest parse error: {ex.Message}");
                        onComplete?.Invoke(null);
                    }
                },
                err =>
                {
                    _isOpening = false;
                    Debug.LogWarning($"[ChestManager] OpenChestMulti failed: {err}");
                    onComplete?.Invoke(null);
                });
        }
    }
}
