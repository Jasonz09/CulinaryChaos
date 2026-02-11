using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Daily login bonus tracker. Server-authoritative: all date checks and
    /// reward grants happen via CloudScript using server UTC time.
    /// No client-side DateTime.UtcNow usage.
    /// </summary>
    public class DailyLoginManager : MonoBehaviour
    {
        public static DailyLoginManager Instance { get; private set; }

        public int CurrentStreak { get; private set; }
        public int CurrentDay { get; private set; }
        public bool HasClaimedToday { get; private set; }

        public event System.Action<int, int> OnLoginRewardAvailable;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load cached state for immediate UI display
            LoadCache();
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
        /// Pulls daily login state from server. Server provides all date/time logic.
        /// </summary>
        private void SyncFromServer()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.GetUserData(
                new List<string> { "DailyLoginData" },
                data =>
                {
                    if (data.TryGetValue("DailyLoginData", out string json)
                        && !string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            var obj = JObject.Parse(json);
                            CurrentStreak = obj["streak"]?.Value<int>() ?? 0;
                            CurrentDay = obj["day"]?.Value<int>() ?? 0;
                            HasClaimedToday = obj["claimedToday"]?.Value<bool>() ?? false;
                            SaveCache();

                            if (!HasClaimedToday && CurrentDay > 0)
                            {
                                int reward = GetRewardForDay(CurrentDay);
                                OnLoginRewardAvailable?.Invoke(CurrentDay, reward);
                            }

                            Debug.Log($"[DailyLoginManager] Synced: streak={CurrentStreak}, day={CurrentDay}, claimed={HasClaimedToday}");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[DailyLoginManager] Server sync parse error: {ex.Message}");
                        }
                    }
                    else
                    {
                        // New player - no login data yet, reward will be available after first claim
                        int reward = GetRewardForDay(1);
                        OnLoginRewardAvailable?.Invoke(1, reward);
                    }
                },
                err => Debug.LogWarning($"[DailyLoginManager] Server sync failed: {err}"));
        }

        public int GetRewardForDay(int day)
        {
            return day switch
            {
                1 => 50,
                7 => 200,
                14 => 500,
                21 => 50,
                30 => 100,
                _ => 25 + (day * 5)
            };
        }

        public bool IsGemReward(int day) => day == 21 || day == 30;

        /// <summary>
        /// Claims the daily login reward via CloudScript. Server validates using UTC time,
        /// calculates streak, grants currency, and returns updated state.
        /// </summary>
        public void ClaimDailyReward()
        {
            if (HasClaimedToday) return;

            PlayFabManager.Instance?.ExecuteCloudScript("ClaimDailyLogin",
                new { day = CurrentDay, streak = CurrentStreak },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        bool success = result["success"]?.Value<bool>() ?? false;
                        if (success)
                        {
                            HasClaimedToday = true;
                            CurrentDay = result["day"]?.Value<int>() ?? CurrentDay;
                            CurrentStreak = result["streak"]?.Value<int>() ?? CurrentStreak;
                            SaveCache();
                            PlayFabManager.Instance?.RefreshCurrencies();
                            Debug.Log($"[DailyLoginManager] Claimed: day={CurrentDay}, streak={CurrentStreak}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[DailyLoginManager] Claim parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[DailyLoginManager] ClaimDailyLogin failed: {err}"));
        }

        private void SaveCache()
        {
            PlayerPrefs.SetInt("LoginStreak", CurrentStreak);
            PlayerPrefs.SetInt("LoginDay", CurrentDay);
            PlayerPrefs.Save();
        }

        private void LoadCache()
        {
            CurrentStreak = PlayerPrefs.GetInt("LoginStreak", 0);
            CurrentDay = PlayerPrefs.GetInt("LoginDay", 0);
        }
    }
}
