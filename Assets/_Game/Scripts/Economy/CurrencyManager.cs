using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;

namespace IOChef.Economy
{
    /// <summary>
    /// Singleton manager that tracks coin, gem, and hero-token currencies.
    /// Server-authoritative: local values are a read-only cache. All mutations
    /// happen through CloudScript; after each mutation, RefreshCurrencies() syncs
    /// the cache from server balances. PlayerPrefs is written for fast UI display
    /// on next launch but is always overwritten by server data on login.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        private const string COINS_KEY = "Coins";
        private const string GEMS_KEY = "Gems";
        private const string HERO_TOKENS_KEY = "HeroTokens";
        private const int GEMS_TO_COINS_RATE = 100;

        /// <summary>Current coin balance (local cache).</summary>
        public long Coins { get; private set; }

        /// <summary>Current gem balance (local cache).</summary>
        public long Gems { get; private set; }

        /// <summary>Current hero-token balance (local cache).</summary>
        public long HeroTokens { get; private set; }

        /// <summary>Backward-compatible alias for Coins.</summary>
        public long SoftCurrency => Coins;

        /// <summary>Backward-compatible alias for Gems.</summary>
        public long PremiumCurrency => Gems;

        public event System.Action<long> OnCoinsChanged;
        public event System.Action<long> OnGemsChanged;
        public event System.Action<long> OnHeroTokensChanged;
        public event System.Action OnCurrenciesRefreshed;

        public event System.Action<long> OnSoftCurrencyChanged
        {
            add => OnCoinsChanged += value;
            remove => OnCoinsChanged -= value;
        }

        public event System.Action<long> OnPremiumCurrencyChanged
        {
            add => OnGemsChanged += value;
            remove => OnGemsChanged -= value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load cached values for immediate UI display
            LoadCurrencyCache();
        }

        private void Start()
        {
            // Sync from server on login
            if (PlayFabManager.Instance != null)
            {
                if (PlayFabManager.Instance.IsLoggedIn)
                    SyncFromServer();
                else
                    PlayFabManager.Instance.OnLoginSuccess += SyncFromServer;
            }
        }

        /// <summary>
        /// Pulls currency balances from the server and overwrites local cache.
        /// Called on login and can be called anytime to refresh.
        /// </summary>
        public void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.GetVirtualCurrencies(
                currencies => UpdateFromServer(currencies),
                err => Debug.LogWarning($"[CurrencyManager] Server sync failed: {err}"));
        }

        /// <summary>
        /// Updates the local cache from a server-provided currency dictionary.
        /// Called by PlayFabManager.RefreshCurrencies() after CloudScript calls.
        /// </summary>
        public void UpdateFromServer(Dictionary<string, int> currencies)
        {
            if (PlayFabManager.Instance == null) return;

            if (currencies.TryGetValue(PlayFabManager.Instance.CoinsCurrencyCode, out int coins))
            {
                Coins = coins;
                OnCoinsChanged?.Invoke(Coins);
            }
            if (currencies.TryGetValue(PlayFabManager.Instance.GemsCurrencyCode, out int gems))
            {
                Gems = gems;
                OnGemsChanged?.Invoke(Gems);
            }
            if (currencies.TryGetValue(PlayFabManager.Instance.HeroTokensCurrencyCode, out int tokens))
            {
                HeroTokens = tokens;
                OnHeroTokensChanged?.Invoke(HeroTokens);
            }
            SaveCurrencyCache();
            OnCurrenciesRefreshed?.Invoke();
            Debug.Log($"[CurrencyManager] Updated from server: {Coins}c, {Gems}g, {HeroTokens}t");
        }

        // ──────────────────────────── Coins ────────────────────────────

        /// <summary>Checks whether the player can afford a coin purchase (from cache).</summary>
        public bool CanAffordCoins(long amount) => Coins >= amount;

        /// <summary>Backward-compatible alias.</summary>
        public bool CanAffordSoft(long amount) => CanAffordCoins(amount);

        // ──────────────────────────── Gems ─────────────────────────────

        /// <summary>Checks whether the player can afford a gem purchase (from cache).</summary>
        public bool CanAffordGems(long amount) => Gems >= amount;

        /// <summary>Backward-compatible alias.</summary>
        public bool CanAffordPremium(long amount) => CanAffordGems(amount);

        // ─────────────────────────── Hero Tokens ───────────────────────

        /// <summary>Checks whether the player can afford a hero-token purchase (from cache).</summary>
        public bool CanAffordHeroTokens(long amount) => HeroTokens >= amount;

        // ─────────────────────────── Conversion ────────────────────────

        /// <summary>
        /// Converts gems into coins via CloudScript (server-side atomic conversion).
        /// Returns false if the cached balance is insufficient.
        /// </summary>
        public bool ConvertGemToCoins(int gemAmount)
        {
            if (gemAmount <= 0 || Gems < gemAmount) return false;

            PlayFabManager.Instance?.ExecuteCloudScript("ConvertGemToCoins",
                new { gemAmount },
                resultJson =>
                {
                    PlayFabManager.Instance?.RefreshCurrencies();
                },
                err => Debug.LogWarning($"[CurrencyManager] ConvertGemToCoins failed: {err}"));

            return true;
        }

        // ──────────────────────── Backward-Compatible Wrappers ───────────
        // These exist so callers (DailyLoginManager, etc.) compile.
        // They should only be called from CloudScript response handlers
        // or SyncFromServer. They do NOT write to the server.

        /// <summary>Updates local cache only. Use only from server response callbacks.</summary>
        public void AddCoins(long amount) { /* No-op: server handles currency */ }

        /// <summary>Updates local cache only. Use only from server response callbacks.</summary>
        public void AddGems(long amount) { /* No-op: server handles currency */ }

        /// <summary>Updates local cache only. Use only from server response callbacks.</summary>
        public void AddHeroTokens(long amount) { /* No-op: server handles currency */ }

        /// <summary>No-op: server handles currency via CloudScript.</summary>
        public bool SpendCoins(long amount) => false;

        /// <summary>No-op: server handles currency via CloudScript.</summary>
        public bool SpendGems(long amount) => false;

        /// <summary>No-op: server handles currency via CloudScript.</summary>
        public bool SpendHeroTokens(long amount) => false;

        /// <summary>Backward-compatible wrapper.</summary>
        public void AddSoftCurrency(long amount) => AddCoins(amount);

        /// <summary>Backward-compatible wrapper.</summary>
        public bool SpendSoftCurrency(long amount) => SpendCoins(amount);

        /// <summary>Backward-compatible wrapper.</summary>
        public void AddPremiumCurrency(long amount) => AddGems(amount);

        /// <summary>Backward-compatible wrapper.</summary>
        public bool SpendPremiumCurrency(long amount) => SpendGems(amount);

        // ──────────────────────── Cache Persistence ──────────────────────

        private void SaveCurrencyCache()
        {
            PlayerPrefs.SetString(COINS_KEY, Coins.ToString());
            PlayerPrefs.SetString(GEMS_KEY, Gems.ToString());
            PlayerPrefs.SetString(HERO_TOKENS_KEY, HeroTokens.ToString());
            PlayerPrefs.Save();
        }

        private void LoadCurrencyCache()
        {
            long.TryParse(PlayerPrefs.GetString(COINS_KEY, "0"), out long coins);
            long.TryParse(PlayerPrefs.GetString(GEMS_KEY, "0"), out long gems);
            long.TryParse(PlayerPrefs.GetString(HERO_TOKENS_KEY, "0"), out long heroTokens);

            Coins = coins;
            Gems = gems;
            HeroTokens = heroTokens;
        }
    }
}
