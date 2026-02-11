using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using IOChef.Core;
using IOChef.Gameplay;

namespace IOChef.Economy
{
    /// <summary>
    /// Singleton manager for ingredient stock. Server-authoritative: purchases go
    /// through CloudScript, consumption is synced at end of level. Local stock is a
    /// cache populated from server on login.
    /// </summary>
    public class IngredientShopManager : MonoBehaviour
    {
        public static IngredientShopManager Instance { get; private set; }

        private Dictionary<IngredientType, int> _stock = new();
        private Dictionary<IngredientType, int> _consumedThisLevel = new();

        public const int BATCH_SIZE = 100;
        public const int MAX_STOCK = 500;
        private const string STOCK_KEY = "IngredientStock";
        private const string SERVER_STOCK_KEY = "IngredientStock";

        public event System.Action<IngredientType> OnIngredientPurchased;
        public event System.Action<IngredientType, int> OnStockChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStockCache();
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
        /// Pulls ingredient stock from server and overwrites local cache.
        /// If no data exists on server (new or reset player), triggers InitNewPlayer.
        /// </summary>
        private void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            Debug.Log("[IngredientShopManager] SyncFromServer: requesting stock data...");
            PlayFabManager.Instance.GetUserData(
                new List<string> { SERVER_STOCK_KEY },
                data =>
                {
                    if (data.TryGetValue(SERVER_STOCK_KEY, out string stockJson)
                        && !string.IsNullOrEmpty(stockJson))
                    {
                        ParseStockString(stockJson);
                        SaveStockCache();
                        Debug.Log($"[IngredientShopManager] Synced stock from server: {_stock.Count} types");
                    }
                    else
                    {
                        Debug.Log("[IngredientShopManager] No stock data on server. Triggering InitNewPlayer...");
                        PlayFabManager.Instance.ExecuteCloudScript("InitNewPlayer", null,
                            _ =>
                            {
                                Debug.Log("[IngredientShopManager] InitNewPlayer completed. Re-syncing...");
                                PlayFabManager.Instance?.RefreshCurrencies();
                                // Re-fetch after init
                                PlayFabManager.Instance?.GetUserData(
                                    new List<string> { SERVER_STOCK_KEY },
                                    data2 =>
                                    {
                                        if (data2.TryGetValue(SERVER_STOCK_KEY, out string json2)
                                            && !string.IsNullOrEmpty(json2))
                                        {
                                            ParseStockString(json2);
                                            SaveStockCache();
                                            Debug.Log($"[IngredientShopManager] Post-init sync: {_stock.Count} types");
                                        }
                                    },
                                    err2 => Debug.LogWarning($"[IngredientShopManager] Post-init sync failed: {err2}"));
                            },
                            err => Debug.LogWarning($"[IngredientShopManager] InitNewPlayer failed: {err}"));
                    }
                },
                err => Debug.LogWarning($"[IngredientShopManager] Server sync failed: {err}"));
        }

        public int GetIngredientPrice(IngredientType type)
        {
            return type switch
            {
                IngredientType.Lettuce or IngredientType.Tomato or IngredientType.Bread or IngredientType.Bun => 25,
                IngredientType.Meat or IngredientType.Cheese or IngredientType.Sausage or IngredientType.Vegetables => 50,
                IngredientType.Dough or IngredientType.Sauce or IngredientType.Pepperoni or IngredientType.Pasta
                    or IngredientType.Fish or IngredientType.Rice or IngredientType.Tortilla => 75,
                IngredientType.Seaweed or IngredientType.Broth or IngredientType.Seasoning or IngredientType.Noodles => 100,
                IngredientType.PlatedDish => 0,
                _ => 50,
            };
        }

        public int GetStock(IngredientType type)
        {
            if (type == IngredientType.PlatedDish) return MAX_STOCK;
            return _stock.TryGetValue(type, out int qty) ? qty : 0;
        }

        public bool IsIngredientUnlocked(IngredientType type) => GetStock(type) > 0;
        public bool CanBuyMore(IngredientType type) => GetStock(type) < MAX_STOCK;

        /// <summary>
        /// Purchases ingredients via CloudScript (server validates coin cost and stock limits).
        /// </summary>
        public void PurchaseIngredient(IngredientType type, System.Action<bool> onComplete = null)
        {
            int current = GetStock(type);
            if (current >= MAX_STOCK) { onComplete?.Invoke(false); return; }

            int price = GetIngredientPrice(type);
            if (price <= 0) { onComplete?.Invoke(false); return; }
            if (!CurrencyManager.Instance.CanAffordCoins(price)) { onComplete?.Invoke(false); return; }

            int toAdd = Mathf.Min(BATCH_SIZE, MAX_STOCK - current);

            PlayFabManager.Instance?.ExecuteCloudScript("PurchaseIngredient",
                new { type = type.ToString(), batchSize = toAdd, price },
                resultJson =>
                {
                    try
                    {
                        var result = Newtonsoft.Json.Linq.JObject.Parse(resultJson);
                        bool success = result["success"] != null && bool.Parse(result["success"].ToString());
                        if (success)
                        {
                            int newStock = int.TryParse(result["newStock"]?.ToString(), out int ns) ? ns : current + toAdd;
                            _stock[type] = newStock;
                            SaveStockCache();
                            OnIngredientPurchased?.Invoke(type);
                            OnStockChanged?.Invoke(type, newStock);
                            PlayFabManager.Instance?.RefreshCurrencies();
                            onComplete?.Invoke(true);
                        }
                        else { onComplete?.Invoke(false); }
                    }
                    catch { onComplete?.Invoke(false); }
                },
                err => { Debug.LogWarning($"[IngredientShopManager] Purchase failed: {err}"); onComplete?.Invoke(false); });
        }

        /// <summary>
        /// Deducts 1 unit locally during gameplay. Tracked for server sync at level end.
        /// </summary>
        public bool ConsumeIngredient(IngredientType type)
        {
            if (type == IngredientType.PlatedDish) return true;
            int current = GetStock(type);
            if (current <= 0) return false;

            _stock[type] = current - 1;
            SaveStockCache();
            OnStockChanged?.Invoke(type, _stock[type]);

            if (!_consumedThisLevel.ContainsKey(type))
                _consumedThisLevel[type] = 0;
            _consumedThisLevel[type]++;

            return true;
        }

        /// <summary>
        /// Syncs consumed ingredients to server at end of level via CloudScript.
        /// </summary>
        public void FlushToServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;
            if (_consumedThisLevel.Count == 0) return;

            var consumed = new Dictionary<string, int>();
            foreach (var kvp in _consumedThisLevel)
                consumed[kvp.Key.ToString()] = kvp.Value;

            PlayFabManager.Instance.ExecuteCloudScript("SyncIngredientStock",
                new { consumed },
                onSuccess: _ => { _consumedThisLevel.Clear(); },
                onError: err => Debug.LogWarning($"[IngredientShopManager] FlushToServer failed: {err}"));
        }

        public List<IngredientType> GetIngredientsForRecipes(List<RecipeSO> recipes)
        {
            var types = new HashSet<IngredientType>();
            if (recipes == null) return new List<IngredientType>();
            foreach (var recipe in recipes)
            {
                if (recipe.finalIngredients == null) continue;
                foreach (var ing in recipe.finalIngredients)
                    if (ing.ingredientType != IngredientType.PlatedDish)
                        types.Add(ing.ingredientType);
            }
            return types.ToList();
        }

        public bool HasAllIngredientsForLevel(LevelDataSO level)
        {
            if (level == null || level.availableRecipes == null) return true;
            var needed = GetIngredientsForRecipes(level.availableRecipes);
            return needed.All(t => GetStock(t) > 0);
        }

        public List<IngredientType> GetMissingIngredientsForLevel(LevelDataSO level)
        {
            if (level == null || level.availableRecipes == null) return new List<IngredientType>();
            var needed = GetIngredientsForRecipes(level.availableRecipes);
            return needed.Where(t => GetStock(t) <= 0).ToList();
        }

        private string SerializeStock()
        {
            var entries = _stock.Select(kvp => $"{(int)kvp.Key}:{kvp.Value}");
            return string.Join(",", entries);
        }

        private void ParseStockString(string data)
        {
            _stock.Clear();
            if (string.IsNullOrEmpty(data)) return;

            // Server stores JSON object: {"Lettuce":100,"Tomato":100,...}
            // Local cache uses "enumInt:qty,enumInt:qty" format
            string trimmed = data.Trim();
            if (trimmed.StartsWith("{"))
            {
                try
                {
                    var json = Newtonsoft.Json.Linq.JObject.Parse(trimmed);
                    foreach (var prop in json.Properties())
                    {
                        if (System.Enum.TryParse<IngredientType>(prop.Name, out var ingType)
                            && int.TryParse(prop.Value.ToString(), out int qty))
                        {
                            _stock[ingType] = qty;
                        }
                    }
                    return;
                }
                catch { /* Fall through to legacy format */ }
            }

            // Legacy local cache format: "0:100,1:100"
            foreach (string pair in data.Split(','))
            {
                var parts = pair.Split(':');
                if (parts.Length == 2
                    && int.TryParse(parts[0], out int typeInt)
                    && int.TryParse(parts[1], out int qty))
                {
                    _stock[(IngredientType)typeInt] = qty;
                }
            }
        }

        private void SaveStockCache()
        {
            PlayerPrefs.SetString(STOCK_KEY, SerializeStock());
            PlayerPrefs.Save();
        }

        private void LoadStockCache()
        {
            string data = PlayerPrefs.GetString(STOCK_KEY, "");
            ParseStockString(data);
        }
    }
}
