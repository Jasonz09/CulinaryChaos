using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Economy
{
    /// <summary>
    /// Manages cosmetic item purchasing, ownership, and equipping.
    /// Server-authoritative: all cosmetic ownership stored on server, synced on login.
    /// Local cache is populated from server data, never the source of truth.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        /// <summary>
        /// Global singleton instance.
        /// </summary>
        public static ShopManager Instance { get; private set; }

        [Header("Cosmetics Database")]
        /// <summary>
        /// Master list of all cosmetic items available in the shop.
        /// </summary>
        [SerializeField] private List<CosmeticItem> allCosmetics;

        /// <summary>
        /// Set of cosmetic IDs the player currently owns (cache from server).
        /// </summary>
        private HashSet<string> _ownedCosmetics = new();
        /// <summary>
        /// Currently equipped cosmetic ID for each cosmetic type (cache from server).
        /// </summary>
        private Dictionary<CosmeticType, string> _equippedCosmetics = new();

        /// <summary>
        /// PlayerPrefs key used to persist owned cosmetic IDs (cache only).
        /// </summary>
        private const string OWNED_KEY = "OwnedCosmetics";
        /// <summary>
        /// PlayerPrefs key used to persist equipped cosmetic mappings (cache only).
        /// </summary>
        private const string EQUIPPED_KEY = "EquippedCosmetics";
        /// <summary>
        /// PlayFab UserData key for cosmetic ownership and equipped state.
        /// </summary>
        private const string SERVER_KEY = "CosmeticData";

        /// <summary>
        /// Fires when a cosmetic is purchased.
        /// </summary>
        public event System.Action<CosmeticItem> OnCosmeticPurchased;

        /// <summary>
        /// Fires when a cosmetic is equipped.
        /// </summary>
        public event System.Action<CosmeticItem> OnCosmeticEquipped;

        /// <summary>
        /// Initializes singleton, persists across scenes, and loads owned cosmetics cache.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize list if null
            if (allCosmetics == null)
                allCosmetics = new List<CosmeticItem>();

            LoadOwnedCosmetics();
        }

        /// <summary>
        /// Syncs cosmetic ownership from the server on login.
        /// </summary>
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
        /// Fetches owned and equipped cosmetics from PlayFab UserData and overwrites local cache.
        /// Server is the authoritative source of cosmetic ownership.
        /// </summary>
        private void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.GetUserData(
                new List<string> { SERVER_KEY },
                data =>
                {
                    if (data.TryGetValue(SERVER_KEY, out string json) && !string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            var obj = JObject.Parse(json);
                            _ownedCosmetics.Clear();
                            _equippedCosmetics.Clear();

                            var ownedArr = obj["owned"] as JArray;
                            if (ownedArr != null)
                                foreach (var token in ownedArr)
                                    _ownedCosmetics.Add(token.ToString());

                            var equippedObj = obj["equipped"] as JObject;
                            if (equippedObj != null)
                                foreach (var prop in equippedObj.Properties())
                                    if (int.TryParse(prop.Name, out int typeInt) && prop.Value is JValue val)
                                        _equippedCosmetics[(CosmeticType)typeInt] = val.ToString();

                            SaveOwnedCosmetics();
                            Debug.Log($"[ShopManager] Synced from server: {_ownedCosmetics.Count} cosmetics owned");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[ShopManager] Server sync parse error: {ex.Message}");
                        }
                    }
                },
                err => Debug.LogWarning($"[ShopManager] Server sync failed: {err}"));
        }

        /// <summary>
        /// Returns all cosmetics, optionally filtered by type.
        /// </summary>
        /// <param name="typeFilter">Optional cosmetic type to filter by.</param>
        /// <returns>List of cosmetic items matching the filter.</returns>
        public List<CosmeticItem> GetAvailableCosmetics(CosmeticType? typeFilter = null)
        {
            if (allCosmetics == null) return new List<CosmeticItem>();
            if (typeFilter == null) return allCosmetics;
            return allCosmetics.FindAll(c => c.cosmeticType == typeFilter.Value);
        }

        /// <summary>
        /// Gets the featured cosmetic item. Returns the first item marked as featured,
        /// or falls back to the first unowned item, or any item if all are owned.
        /// </summary>
        /// <returns>The featured cosmetic item, or null if no cosmetics exist.</returns>
        public CosmeticItem GetFeaturedItem()
        {
            if (allCosmetics == null || allCosmetics.Count == 0) return null;

            // Try to find explicitly featured item
            var featured = allCosmetics.Find(c => c.isFeatured);
            if (featured != null) return featured;

            // Fallback to first unowned item
            featured = allCosmetics.Find(c => !_ownedCosmetics.Contains(c.cosmeticId));
            if (featured != null) return featured;

            // Fallback to any item
            return allCosmetics[0];
        }

        /// <summary>
        /// Checks whether the player owns a specific cosmetic.
        /// </summary>
        /// <param name="cosmeticId">The unique identifier of the cosmetic.</param>
        /// <returns>True if the cosmetic is owned, false otherwise.</returns>
        public bool IsCosmeticOwned(string cosmeticId) => _ownedCosmetics.Contains(cosmeticId);

        /// <summary>
        /// Attempts to purchase a cosmetic using soft currency via CloudScript validation.
        /// Server validates cost and grants cosmetic, then client syncs.
        /// </summary>
        /// <param name="cosmeticId">The unique identifier of the cosmetic to purchase.</param>
        /// <returns>True if the purchase was submitted, false if pre-flight checks fail.</returns>
        public bool PurchaseWithCredits(string cosmeticId)
        {
            if (allCosmetics == null) return false;
            var item = allCosmetics.Find(c => c.cosmeticId == cosmeticId);
            if (item == null || item.priceCredits <= 0) return false;
            if (_ownedCosmetics.Contains(cosmeticId)) return false;

            if (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAffordCoins(item.priceCredits))
                return false;

            PlayFabManager.Instance?.ExecuteCloudScript("PurchaseCosmetic",
                new { cosmeticId, price = item.priceCredits, currencyType = "credits" },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _ownedCosmetics.Add(cosmeticId);
                            item.isOwned = true;
                            SaveOwnedCosmetics();
                            OnCosmeticPurchased?.Invoke(item);
                            PlayFabManager.Instance?.RefreshCurrencies();
                            Debug.Log($"[ShopManager] Purchased cosmetic {cosmeticId} with credits");
                        }
                        else
                        {
                            Debug.LogWarning($"[ShopManager] Purchase failed: {result["error"]?.ToString() ?? "Unknown error"}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[ShopManager] Purchase parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[ShopManager] PurchaseCosmetic failed: {err}"));

            return true;
        }

        /// <summary>
        /// Attempts to purchase a cosmetic using premium currency via CloudScript validation.
        /// Server validates cost and grants cosmetic, then client syncs.
        /// </summary>
        /// <param name="cosmeticId">The unique identifier of the cosmetic to purchase.</param>
        /// <returns>True if the purchase was submitted, false if pre-flight checks fail.</returns>
        public bool PurchaseWithGems(string cosmeticId)
        {
            if (allCosmetics == null) return false;
            var item = allCosmetics.Find(c => c.cosmeticId == cosmeticId);
            if (item == null || item.priceGems <= 0) return false;
            if (_ownedCosmetics.Contains(cosmeticId)) return false;

            if (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAffordPremium(item.priceGems))
                return false;

            PlayFabManager.Instance?.ExecuteCloudScript("PurchaseCosmetic",
                new { cosmeticId, price = item.priceGems, currencyType = "gems" },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _ownedCosmetics.Add(cosmeticId);
                            item.isOwned = true;
                            SaveOwnedCosmetics();
                            OnCosmeticPurchased?.Invoke(item);
                            PlayFabManager.Instance?.RefreshCurrencies();
                            Debug.Log($"[ShopManager] Purchased cosmetic {cosmeticId} with gems");
                        }
                        else
                        {
                            Debug.LogWarning($"[ShopManager] Purchase failed: {result["error"]?.ToString() ?? "Unknown error"}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[ShopManager] Purchase parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[ShopManager] PurchaseCosmetic failed: {err}"));

            return true;
        }

        /// <summary>
        /// Equips an owned cosmetic via CloudScript validation, replacing any previously equipped cosmetic of the same type.
        /// </summary>
        /// <param name="cosmeticId">The unique identifier of the cosmetic to equip.</param>
        public void EquipCosmetic(string cosmeticId)
        {
            if (allCosmetics == null) return;
            var item = allCosmetics.Find(c => c.cosmeticId == cosmeticId);
            if (item == null || !_ownedCosmetics.Contains(cosmeticId)) return;

            PlayFabManager.Instance?.ExecuteCloudScript("EquipCosmetic",
                new { cosmeticId, cosmeticType = (int)item.cosmeticType },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _equippedCosmetics[item.cosmeticType] = cosmeticId;
                            item.isEquipped = true;
                            SaveOwnedCosmetics();
                            OnCosmeticEquipped?.Invoke(item);
                            Debug.Log($"[ShopManager] Equipped cosmetic {cosmeticId}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[ShopManager] Equip parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[ShopManager] EquipCosmetic failed: {err}"));
        }

        /// <summary>
        /// Gets the currently equipped cosmetic for a given type.
        /// </summary>
        /// <param name="type">The cosmetic type to query.</param>
        /// <returns>The cosmetic ID of the equipped item, or null if none is equipped.</returns>
        public string GetEquippedCosmetic(CosmeticType type)
        {
            return _equippedCosmetics.TryGetValue(type, out string id) ? id : null;
        }

        /// <summary>
        /// Persists owned and equipped cosmetic data to PlayerPrefs (cache only).
        /// </summary>
        private void SaveOwnedCosmetics()
        {
            PlayerPrefs.SetString(OWNED_KEY, string.Join(",", _ownedCosmetics));
            var equipped = new List<string>();
            foreach (var kvp in _equippedCosmetics)
                equipped.Add($"{(int)kvp.Key}:{kvp.Value}");
            PlayerPrefs.SetString(EQUIPPED_KEY, string.Join(",", equipped));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads owned and equipped cosmetic data from PlayerPrefs (cache only).
        /// This data is overwritten by SyncFromServer on login.
        /// </summary>
        private void LoadOwnedCosmetics()
        {
            try
            {
                // Initialize list if null
                if (allCosmetics == null)
                    allCosmetics = new List<CosmeticItem>();

                string owned = PlayerPrefs.GetString(OWNED_KEY, "");
                if (!string.IsNullOrEmpty(owned))
                {
                    foreach (string id in owned.Split(','))
                        if (!string.IsNullOrEmpty(id))
                            _ownedCosmetics.Add(id);
                }

                string equipped = PlayerPrefs.GetString(EQUIPPED_KEY, "");
                if (!string.IsNullOrEmpty(equipped))
                {
                    foreach (string pair in equipped.Split(','))
                    {
                        var parts = pair.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int typeInt))
                            _equippedCosmetics[(CosmeticType)typeInt] = parts[1];
                    }
                }

                // Sync owned state on items
                foreach (var item in allCosmetics)
                    item.isOwned = _ownedCosmetics.Contains(item.cosmeticId);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ShopManager] Failed to load cosmetics: {ex.Message}");
                allCosmetics = new List<CosmeticItem>();
            }
        }
    }
}
