using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Economy
{
    public class DailyDealsManager : MonoBehaviour
    {
        public static DailyDealsManager Instance { get; private set; }

        private List<DailyDealData> _todayDeals = new();
        private HashSet<string> _purchasedToday = new();
        private float _secondsUntilReset;

        public event System.Action OnDealsRefreshed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_secondsUntilReset > 0)
                _secondsUntilReset -= Time.unscaledDeltaTime;
        }

        public void FetchDeals(System.Action onDone = null)
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[DailyDealsManager] FetchDeals: not logged in");
                onDone?.Invoke();
                return;
            }

            PlayFabManager.Instance.ExecuteCloudScript("GetShopData", new { },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        _secondsUntilReset = result["secondsUntilReset"]?.Value<float>() ?? 0;

                        var dealsArr = result["dailyDeals"] as JArray;
                        _todayDeals.Clear();
                        if (dealsArr != null)
                        {
                            foreach (var d in dealsArr)
                            {
                                _todayDeals.Add(new DailyDealData
                                {
                                    dealId = d["dealId"]?.ToString() ?? "",
                                    type = d["type"]?.ToString() ?? "",
                                    amount = d["amount"]?.Value<int>() ?? 0,
                                    normalGemCost = d["normalGemCost"]?.Value<int>() ?? 0,
                                    dealGemCost = d["dealGemCost"]?.Value<int>() ?? 0,
                                    isFree = d["isFree"]?.Value<bool>() ?? false
                                });
                            }
                        }

                        _purchasedToday.Clear();
                        var purchArr = result["purchasedDeals"] as JArray;
                        if (purchArr != null)
                        {
                            foreach (var p in purchArr)
                                _purchasedToday.Add(p.ToString());
                        }

                        Debug.Log($"[DailyDealsManager] Fetched {_todayDeals.Count} deals, resets in {_secondsUntilReset}s");
                        OnDealsRefreshed?.Invoke();
                        onDone?.Invoke();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[DailyDealsManager] Parse error: {ex.Message}");
                        onDone?.Invoke();
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[DailyDealsManager] Fetch failed: {err}");
                    onDone?.Invoke();
                });
        }

        public void PurchaseDeal(string dealId, System.Action<bool> onComplete = null)
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[DailyDealsManager] PurchaseDeal: not logged in");
                onComplete?.Invoke(false);
                return;
            }

            PlayFabManager.Instance.ExecuteCloudScript("PurchaseDailyDeal",
                new { dealId },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _purchasedToday.Add(dealId);
                            PlayFabManager.Instance?.RefreshCurrencies();
                            OnDealsRefreshed?.Invoke();
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            Debug.LogWarning($"[DailyDealsManager] Purchase failed: {result["error"]}");
                            onComplete?.Invoke(false);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[DailyDealsManager] PurchaseDeal parse error: {ex.Message}");
                        onComplete?.Invoke(false);
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[DailyDealsManager] Purchase error: {err}");
                    onComplete?.Invoke(false);
                });
        }

        public List<DailyDealData> GetTodayDeals() => _todayDeals;
        public bool IsDealPurchased(string dealId) => _purchasedToday.Contains(dealId);
        public float GetSecondsUntilReset() => _secondsUntilReset;
    }
}
