using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Economy
{
    /// <summary>
    /// Singleton manager for equipment and hero skin ownership and equipping.
    /// Server-authoritative: all skin ownership stored on server, synced on login.
    /// Local cache is populated from server data, never the source of truth.
    /// </summary>
    public class SkinManager : MonoBehaviour
    {
        /// <summary>
        /// Global singleton instance.
        /// </summary>
        public static SkinManager Instance { get; private set; }

        /// <summary>
        /// Set of owned skin IDs (cache from server).
        /// </summary>
        private HashSet<string> _ownedSkins = new();

        /// <summary>
        /// Currently equipped skin ID for each skin type (cache from server).
        /// </summary>
        private Dictionary<SkinType, string> _equippedSkins = new();

        /// <summary>
        /// PlayerPrefs key for owned skins (cache only).
        /// </summary>
        private const string OWNED_KEY = "OwnedSkins";

        /// <summary>
        /// PlayerPrefs key for equipped skins (cache only).
        /// </summary>
        private const string EQUIPPED_KEY = "EquippedSkins_V2";

        /// <summary>
        /// PlayFab UserData key for skin ownership and equipped state.
        /// </summary>
        private const string SERVER_KEY = "SkinData";

        /// <summary>
        /// Fires when a skin is purchased.
        /// </summary>
        public event System.Action<string> OnSkinPurchased;

        /// <summary>
        /// Fires when a skin is equipped.
        /// </summary>
        public event System.Action<string> OnSkinEquipped;

        /// <summary>
        /// Initializes singleton, persists across scenes, and loads state from cache.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSkins();
        }

        /// <summary>
        /// Syncs skin ownership from the server on login.
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
        /// Fetches owned and equipped skins from PlayFab UserData and overwrites local cache.
        /// Server is the authoritative source of skin ownership.
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
                            _ownedSkins.Clear();
                            _equippedSkins.Clear();

                            var ownedArr = obj["owned"] as JArray;
                            if (ownedArr != null)
                                foreach (var token in ownedArr)
                                    _ownedSkins.Add(token.ToString());

                            var equippedObj = obj["equipped"] as JObject;
                            if (equippedObj != null)
                                foreach (var prop in equippedObj.Properties())
                                    if (int.TryParse(prop.Name, out int typeInt) && prop.Value is JValue val)
                                        _equippedSkins[(SkinType)typeInt] = val.ToString();

                            SaveSkins();
                            Debug.Log($"[SkinManager] Synced from server: {_ownedSkins.Count} skins owned");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[SkinManager] Server sync parse error: {ex.Message}");
                        }
                    }
                },
                err => Debug.LogWarning($"[SkinManager] Server sync failed: {err}"));
        }

        /// <summary>
        /// Checks whether a skin is owned.
        /// </summary>
        /// <param name="skinId">The skin identifier to check.</param>
        /// <returns>True if owned.</returns>
        public bool IsSkinOwned(string skinId)
        {
            return _ownedSkins.Contains(skinId);
        }

        /// <summary>
        /// Unlocks a skin via CloudScript (server-side validation).
        /// Called by CloudScript handlers when rewards are granted.
        /// </summary>
        /// <param name="skinId">The skin identifier to unlock.</param>
        public void UnlockSkin(string skinId)
        {
            _ownedSkins.Add(skinId);
            SaveSkins();
            Debug.Log($"[SkinManager] Unlocked skin {skinId}");
        }

        /// <summary>
        /// Attempts to purchase a skin using coins or gems via CloudScript validation.
        /// Server validates cost and grants skin, then client syncs.
        /// </summary>
        /// <param name="skin">The skin data to purchase.</param>
        /// <returns>True if the purchase was submitted, false if pre-flight checks fail.</returns>
        public bool PurchaseSkin(SkinData skin)
        {
            if (skin == null || _ownedSkins.Contains(skin.skinId)) return false;
            if (CurrencyManager.Instance == null) return false;

            bool canAffordCoins = skin.priceCoin > 0 && CurrencyManager.Instance.CanAffordCoins(skin.priceCoin);
            bool canAffordGems = skin.priceGems > 0 && CurrencyManager.Instance.CanAffordPremium(skin.priceGems);

            if (!canAffordCoins && !canAffordGems) return false;

            string currencyType = canAffordCoins ? "coins" : "gems";
            int price = canAffordCoins ? skin.priceCoin : skin.priceGems;

            PlayFabManager.Instance?.ExecuteCloudScript("PurchaseSkin",
                new { skinId = skin.skinId, price, currencyType },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _ownedSkins.Add(skin.skinId);
                            SaveSkins();
                            OnSkinPurchased?.Invoke(skin.skinId);
                            PlayFabManager.Instance?.RefreshCurrencies();
                            Debug.Log($"[SkinManager] Purchased skin {skin.skinId} with {currencyType}");
                        }
                        else
                        {
                            Debug.LogWarning($"[SkinManager] Purchase failed: {result["error"]?.ToString() ?? "Unknown error"}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[SkinManager] Purchase parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[SkinManager] PurchaseSkin failed: {err}"));

            return true;
        }

        /// <summary>
        /// Equips an owned skin for the given type via CloudScript validation.
        /// </summary>
        /// <param name="skinId">The skin identifier to equip.</param>
        /// <param name="type">The skin type category.</param>
        public void EquipSkin(string skinId, SkinType type)
        {
            if (!_ownedSkins.Contains(skinId)) return;

            PlayFabManager.Instance?.ExecuteCloudScript("EquipSkin",
                new { skinId, skinType = (int)type },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _equippedSkins[type] = skinId;
                            SaveSkins();
                            OnSkinEquipped?.Invoke(skinId);
                            Debug.Log($"[SkinManager] Equipped skin {skinId}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[SkinManager] Equip parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[SkinManager] EquipSkin failed: {err}"));
        }

        /// <summary>
        /// Returns the equipped skin ID for the given type.
        /// </summary>
        /// <param name="type">The skin type to query.</param>
        /// <returns>The equipped skin ID, or null if none.</returns>
        public string GetEquippedSkin(SkinType type)
        {
            return _equippedSkins.TryGetValue(type, out string id) ? id : null;
        }

        /// <summary>
        /// Persists skin data to PlayerPrefs (cache only).
        /// </summary>
        private void SaveSkins()
        {
            PlayerPrefs.SetString(OWNED_KEY, string.Join(",", _ownedSkins));
            var equipped = new List<string>();
            foreach (var kvp in _equippedSkins)
                equipped.Add($"{(int)kvp.Key}:{kvp.Value}");
            PlayerPrefs.SetString(EQUIPPED_KEY, string.Join(",", equipped));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads skin data from PlayerPrefs (cache only).
        /// This data is overwritten by SyncFromServer on login.
        /// </summary>
        private void LoadSkins()
        {
            string owned = PlayerPrefs.GetString(OWNED_KEY, "");
            if (!string.IsNullOrEmpty(owned))
                foreach (string id in owned.Split(','))
                    if (!string.IsNullOrEmpty(id))
                        _ownedSkins.Add(id);

            string equipped = PlayerPrefs.GetString(EQUIPPED_KEY, "");
            if (!string.IsNullOrEmpty(equipped))
                foreach (string pair in equipped.Split(','))
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int typeInt))
                        _equippedSkins[(SkinType)typeInt] = parts[1];
                }
        }
    }
}
