using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Economy
{
    public class BundleManager : MonoBehaviour
    {
        public static BundleManager Instance { get; private set; }

        private List<BundleData> _availableBundles = new();

        public event System.Action OnBundlesRefreshed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void FetchBundles(System.Action onDone = null)
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[BundleManager] FetchBundles: not logged in");
                onDone?.Invoke();
                return;
            }

            PlayFabManager.Instance.ExecuteCloudScript("GetShopData", new { },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        var bundlesArr = result["bundles"] as JArray;
                        _availableBundles.Clear();
                        if (bundlesArr != null)
                        {
                            foreach (var b in bundlesArr)
                            {
                                var bd = new BundleData
                                {
                                    bundleId = b["bundleId"]?.ToString() ?? "",
                                    name = b["name"]?.ToString() ?? "",
                                    gemCost = b["gemCost"]?.Value<int>() ?? 0,
                                    valueMultiplier = b["valueMultiplier"]?.Value<float>() ?? 1f,
                                    contents = new List<BundleContent>()
                                };
                                var contArr = b["contents"] as JArray;
                                if (contArr != null)
                                {
                                    foreach (var c in contArr)
                                    {
                                        bd.contents.Add(new BundleContent
                                        {
                                            type = c["type"]?.ToString() ?? "",
                                            amount = c["amount"]?.Value<int>() ?? 0
                                        });
                                    }
                                }
                                _availableBundles.Add(bd);
                            }
                        }
                        Debug.Log($"[BundleManager] Fetched {_availableBundles.Count} bundles");
                        OnBundlesRefreshed?.Invoke();
                        onDone?.Invoke();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[BundleManager] Parse error: {ex.Message}");
                        onDone?.Invoke();
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[BundleManager] Fetch failed: {err}");
                    onDone?.Invoke();
                });
        }

        public void PurchaseBundle(string bundleId, System.Action<bool> onComplete = null)
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[BundleManager] PurchaseBundle: not logged in");
                onComplete?.Invoke(false);
                return;
            }

            PlayFabManager.Instance.ExecuteCloudScript("PurchaseBundle",
                new { bundleId },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        if (result["success"]?.Value<bool>() == true)
                        {
                            _availableBundles.RemoveAll(b => b.bundleId == bundleId);
                            PlayFabManager.Instance?.RefreshCurrencies();
                            OnBundlesRefreshed?.Invoke();
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            Debug.LogWarning($"[BundleManager] Purchase failed: {result["error"]}");
                            onComplete?.Invoke(false);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[BundleManager] PurchaseBundle parse error: {ex.Message}");
                        onComplete?.Invoke(false);
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[BundleManager] Purchase error: {err}");
                    onComplete?.Invoke(false);
                });
        }

        public List<BundleData> GetAvailableBundles() => _availableBundles;
    }
}
