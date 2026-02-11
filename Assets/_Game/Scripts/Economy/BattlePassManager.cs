using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Economy
{
    public class BattlePassManager : MonoBehaviour
    {
        public static BattlePassManager Instance { get; private set; }

        [Header("Season Config")]
        [SerializeField] private int currentSeason = 1;
        [SerializeField] private int maxTier = 70;
        [SerializeField] private int xpPerTier = 1000;

        [Header("Rewards")]
        [SerializeField] private List<BattlePassTierReward> freeRewards;
        [SerializeField] private List<BattlePassTierReward> premiumRewards;

        private int _currentTier;
        private int _currentXP;
        private bool _isPremium;
        private int _premiumCost = 500;
        private System.DateTime _seasonEndUtc = System.DateTime.UtcNow.AddDays(30);
        private System.TimeSpan _serverTimeOffset = System.TimeSpan.Zero;
        private System.DateTime? _lastClaimUtc;
        private HashSet<int> _claimedFreeTiers = new();
        private HashSet<int> _claimedPremiumTiers = new();
        private Dictionary<int, BPTierRewardConfig> _tierConfigs = new();

        public int CurrentSeason => currentSeason;
        public int CurrentTier => _currentTier;
        public int CurrentXP => _currentXP;
        public int XPPerTier => xpPerTier;
        public bool IsPremiumPass => _isPremium;
        public int MaxTier => maxTier;
        public int PremiumCost => _premiumCost;
        public System.DateTime SeasonEndUtc => _seasonEndUtc;
        public System.DateTime ServerUtcNow => System.DateTime.UtcNow + _serverTimeOffset;
        public System.DateTime? LastClaimUtc => _lastClaimUtc;

        public event System.Action<int> OnTierUp;
        public event System.Action<BattlePassTierReward> OnRewardClaimed;
        public event System.Action OnPremiumStatusChanged;
        public event System.Action OnConfigFetched;

        private const string SERVER_BP_KEY = "BattlePassData";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (freeRewards == null) freeRewards = new List<BattlePassTierReward>();
            if (premiumRewards == null) premiumRewards = new List<BattlePassTierReward>();

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

        private void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.GetUserData(
                new List<string> { SERVER_BP_KEY },
                data =>
                {
                    if (data.TryGetValue(SERVER_BP_KEY, out string json)
                        && !string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            var obj = JObject.Parse(json);
                            int serverSeason = obj["season"]?.Value<int>() ?? 0;
                            if (serverSeason != currentSeason) return;

                            _currentTier = obj["tier"]?.Value<int>() ?? 0;
                            _currentXP = obj["xp"]?.Value<int>() ?? 0;
                            _isPremium = obj["premium"]?.Value<bool>() ?? false;

                            _claimedFreeTiers.Clear();
                            var freeTiers = obj["claimedFree"] as JArray;
                            if (freeTiers != null)
                                foreach (var t in freeTiers) _claimedFreeTiers.Add(t.Value<int>());

                            _claimedPremiumTiers.Clear();
                            var premTiers = obj["claimedPremium"] as JArray;
                            if (premTiers != null)
                                foreach (var t in premTiers) _claimedPremiumTiers.Add(t.Value<int>());

                            SaveProgressCache();
                            Debug.Log($"[BattlePassManager] Synced from server: tier={_currentTier}, premium={_isPremium}");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[BattlePassManager] Server sync parse error: {ex.Message}");
                        }
                    }
                },
                err => Debug.LogWarning($"[BattlePassManager] Server sync failed: {err}"));
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            PlayFabManager.Instance?.ExecuteCloudScript("AddBattlePassXP",
                new { amount, season = currentSeason },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        int newTier = result["tier"]?.Value<int>() ?? _currentTier;
                        int newXP = result["xp"]?.Value<int>() ?? _currentXP;
                        bool tieredUp = newTier > _currentTier;
                        _currentTier = newTier;
                        _currentXP = newXP;
                        SaveProgressCache();
                        if (tieredUp) OnTierUp?.Invoke(_currentTier);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[BattlePassManager] AddXP parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[BattlePassManager] AddXP failed: {err}"));
        }

        public void PurchasePremiumPass(System.Action<bool> onComplete = null)
        {
            PlayFabManager.Instance?.ExecuteCloudScript("PurchaseBattlePass",
                new { season = currentSeason },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _isPremium = true;
                            SaveProgressCache();
                            PlayFabManager.Instance?.RefreshCurrencies();
                            OnPremiumStatusChanged?.Invoke();
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            Debug.LogWarning($"[BattlePassManager] Purchase failed: {result["error"]}");
                            onComplete?.Invoke(false);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[BattlePassManager] PurchasePremiumPass parse error: {ex.Message}");
                        onComplete?.Invoke(false);
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[BattlePassManager] PurchasePremiumPass failed: {err}");
                    onComplete?.Invoke(false);
                });
        }

        public void FetchConfig(System.Action onDone = null)
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[BattlePassManager] FetchConfig: not logged in");
                onDone?.Invoke();
                return;
            }

            PlayFabManager.Instance.ExecuteCloudScript("GetBattlePassConfig",
                new { season = currentSeason },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);

                        _premiumCost = result["premiumCost"]?.Value<int>() ?? 500;
                        maxTier = result["maxTier"]?.Value<int>() ?? 70;
                        xpPerTier = result["xpPerTier"]?.Value<int>() ?? 1000;

                        string endStr = result["seasonEnd"]?.Value<string>();
                        if (!string.IsNullOrEmpty(endStr) && System.DateTime.TryParse(endStr, null,
                            System.Globalization.DateTimeStyles.RoundtripKind, out var parsedEnd))
                            _seasonEndUtc = parsedEnd.ToUniversalTime();

                        // Compute offset between server time and local time for accurate countdown
                        string serverTimeStr = result["serverTimeUtc"]?.Value<string>();
                        if (!string.IsNullOrEmpty(serverTimeStr) && System.DateTime.TryParse(serverTimeStr, null,
                            System.Globalization.DateTimeStyles.RoundtripKind, out var parsedServerTime))
                        {
                            _serverTimeOffset = parsedServerTime.ToUniversalTime() - System.DateTime.UtcNow;
                        }

                        var pd = result["playerData"] as JObject;
                        if (pd != null)
                        {
                            _currentTier = pd["tier"]?.Value<int>() ?? _currentTier;
                            _currentXP = pd["xp"]?.Value<int>() ?? _currentXP;
                            _isPremium = pd["premium"]?.Value<bool>() ?? _isPremium;

                            _claimedFreeTiers.Clear();
                            var freeTiers = pd["claimedFree"] as JArray;
                            if (freeTiers != null)
                                foreach (var t in freeTiers) _claimedFreeTiers.Add(t.Value<int>());

                            _claimedPremiumTiers.Clear();
                            var premTiers = pd["claimedPremium"] as JArray;
                            if (premTiers != null)
                                foreach (var t in premTiers) _claimedPremiumTiers.Add(t.Value<int>());

                            // Parse last claim time from server
                            string lastClaimStr = pd["lastClaimUtc"]?.Value<string>();
                            if (!string.IsNullOrEmpty(lastClaimStr) && System.DateTime.TryParse(lastClaimStr, null,
                                System.Globalization.DateTimeStyles.RoundtripKind, out var parsedClaim))
                                _lastClaimUtc = parsedClaim.ToUniversalTime();
                        }

                        var rewards = result["rewards"] as JObject;
                        _tierConfigs.Clear();
                        if (rewards != null)
                        {
                            foreach (var kv in rewards)
                            {
                                if (int.TryParse(kv.Key, out int tier))
                                {
                                    var r = kv.Value as JObject;
                                    _tierConfigs[tier] = new BPTierRewardConfig
                                    {
                                        freeCoins = r?["freeCoins"]?.Value<int>() ?? 0,
                                        freeGems = r?["freeGems"]?.Value<int>() ?? 0,
                                        premiumCoins = r?["premiumCoins"]?.Value<int>() ?? 0,
                                        premiumGems = r?["premiumGems"]?.Value<int>() ?? 0,
                                        premiumTokens = r?["premiumTokens"]?.Value<int>() ?? 0,
                                    };
                                }
                            }
                        }

                        SaveProgressCache();
                        Debug.Log($"[BattlePassManager] Config fetched: {_tierConfigs.Count} tiers, premium={_isPremium}, offset={_serverTimeOffset.TotalSeconds:F1}s");
                        OnConfigFetched?.Invoke();
                        onDone?.Invoke();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[BattlePassManager] FetchConfig parse error: {ex.Message}");
                        onDone?.Invoke();
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[BattlePassManager] FetchConfig failed: {err}");
                    onDone?.Invoke();
                });
        }

        public bool IsTierClaimed(int tier)
        {
            return _claimedFreeTiers.Contains(tier) ||
                   (_isPremium && _claimedPremiumTiers.Contains(tier));
        }

        public bool IsFreeTierClaimed(int tier) => _claimedFreeTiers.Contains(tier);
        public bool IsPremiumTierClaimed(int tier) => _claimedPremiumTiers.Contains(tier);

        public BPTierRewardConfig GetTierRewardConfig(int tier)
        {
            return _tierConfigs.TryGetValue(tier, out var config) ? config : null;
        }

        public bool HasTierConfigs() => _tierConfigs.Count > 0;

        public void ClaimTierReward(int tier, System.Action<bool, string> onComplete = null)
        {
            if (tier > _currentTier)
            {
                onComplete?.Invoke(false, "Tier not reached");
                return;
            }

            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[BattlePassManager] ClaimTierReward: not logged in");
                onComplete?.Invoke(false, "Not connected to server");
                return;
            }

            PlayFabManager.Instance.ExecuteCloudScript("ClaimBPReward",
                new { tier, season = currentSeason, isPremium = _isPremium },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            if (result["claimedFree"]?.Value<bool>() == true)
                            {
                                _claimedFreeTiers.Add(tier);
                                var fr = freeRewards?.Find(r => r.tier == tier);
                                if (fr != null) OnRewardClaimed?.Invoke(fr);
                            }
                            if (result["claimedPremium"]?.Value<bool>() == true)
                            {
                                _claimedPremiumTiers.Add(tier);
                                var pr = premiumRewards?.Find(r => r.tier == tier);
                                if (pr != null) OnRewardClaimed?.Invoke(pr);
                            }

                            // Update last claim time and server offset from response
                            string claimTimeStr = result["lastClaimUtc"]?.Value<string>();
                            if (!string.IsNullOrEmpty(claimTimeStr) && System.DateTime.TryParse(claimTimeStr, null,
                                System.Globalization.DateTimeStyles.RoundtripKind, out var parsedClaim))
                                _lastClaimUtc = parsedClaim.ToUniversalTime();

                            string serverTimeStr = result["serverTimeUtc"]?.Value<string>();
                            if (!string.IsNullOrEmpty(serverTimeStr) && System.DateTime.TryParse(serverTimeStr, null,
                                System.Globalization.DateTimeStyles.RoundtripKind, out var parsedServer))
                                _serverTimeOffset = parsedServer.ToUniversalTime() - System.DateTime.UtcNow;

                            SaveProgressCache();
                            PlayFabManager.Instance?.RefreshCurrencies();
                            onComplete?.Invoke(true, null);
                        }
                        else
                        {
                            string error = result["error"]?.Value<string>() ?? "Unknown error";
                            Debug.LogWarning($"[BattlePassManager] ClaimBPReward: {error}");
                            onComplete?.Invoke(false, error);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[BattlePassManager] ClaimBPReward parse error: {ex.Message}");
                        onComplete?.Invoke(false, "Server error");
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[BattlePassManager] ClaimBPReward failed: {err}");
                    onComplete?.Invoke(false, err);
                });
        }

        public List<BattlePassTierReward> GetFreeRewards() => freeRewards ?? new List<BattlePassTierReward>();
        public List<BattlePassTierReward> GetPremiumRewards() => premiumRewards ?? new List<BattlePassTierReward>();

        private void SaveProgressCache()
        {
            PlayerPrefs.SetInt("BP_Season", currentSeason);
            PlayerPrefs.SetInt("BP_Tier", _currentTier);
            PlayerPrefs.SetInt("BP_XP", _currentXP);
            PlayerPrefs.SetInt("BP_Premium", _isPremium ? 1 : 0);
            PlayerPrefs.SetString("BP_ClaimedFree", string.Join(",", _claimedFreeTiers));
            PlayerPrefs.SetString("BP_ClaimedPremium", string.Join(",", _claimedPremiumTiers));
            PlayerPrefs.Save();
        }

        private void LoadProgressCache()
        {
            int savedSeason = PlayerPrefs.GetInt("BP_Season", 0);
            if (savedSeason != currentSeason)
            {
                _currentTier = 0; _currentXP = 0; _isPremium = false;
                _claimedFreeTiers.Clear(); _claimedPremiumTiers.Clear();
                return;
            }
            _currentTier = PlayerPrefs.GetInt("BP_Tier", 0);
            _currentXP = PlayerPrefs.GetInt("BP_XP", 0);
            _isPremium = PlayerPrefs.GetInt("BP_Premium", 0) == 1;

            string freeStr = PlayerPrefs.GetString("BP_ClaimedFree", "");
            if (!string.IsNullOrEmpty(freeStr))
                foreach (string s in freeStr.Split(','))
                    if (int.TryParse(s, out int t)) _claimedFreeTiers.Add(t);

            string premStr = PlayerPrefs.GetString("BP_ClaimedPremium", "");
            if (!string.IsNullOrEmpty(premStr))
                foreach (string s in premStr.Split(','))
                    if (int.TryParse(s, out int t)) _claimedPremiumTiers.Add(t);
        }
    }
}
