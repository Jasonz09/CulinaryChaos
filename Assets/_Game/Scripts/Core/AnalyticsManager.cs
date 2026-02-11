using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Core
{
    /// <summary>
    /// Singleton manager that logs gameplay and monetization analytics events.
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        /// <summary>
        /// Global singleton instance of the AnalyticsManager.
        /// </summary>
        public static AnalyticsManager Instance { get; private set; }

        /// <summary>
        /// Initializes singleton and starts the session timer.
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

            try
            {
                LogEvent("session_start", new Dictionary<string, string>
                {
                    { "platform", Application.platform.ToString() },
                    { "version", Application.version }
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AnalyticsManager] Init error: {e.Message}");
            }
        }

        /// <summary>
        /// Logs a named analytics event with optional key-value parameters.
        /// </summary>
        /// <param name="eventName">The name of the analytics event.</param>
        /// <param name="parameters">Optional dictionary of event parameters.</param>
        public void LogEvent(string eventName, Dictionary<string, string> parameters = null)
        {
            // Integration point for Firebase, Unity Analytics, etc.
            string paramStr = "";
            if (parameters != null)
            {
                var parts = new List<string>();
                foreach (var kvp in parameters)
                    parts.Add($"{kvp.Key}={kvp.Value}");
                paramStr = string.Join(", ", parts);
            }

            Debug.Log($"[Analytics] {eventName}: {paramStr}");

            // TODO: Replace with actual analytics SDK calls
            // Firebase.Analytics.LogEvent(eventName, parameters);
        }

        /// <summary>
        /// Logs a level-completion analytics event.
        /// </summary>
        /// <param name="levelId">The identifier of the completed level.</param>
        /// <param name="score">The score the player achieved.</param>
        /// <param name="stars">The number of stars earned.</param>
        /// <param name="heroUsed">The identifier of the hero used.</param>
        public void LogLevelComplete(int levelId, int score, int stars, string heroUsed)
        {
            LogEvent("level_complete", new Dictionary<string, string>
            {
                { "level_id", levelId.ToString() },
                { "score", score.ToString() },
                { "stars", stars.ToString() },
                { "hero_used", heroUsed }
            });
        }

        /// <summary>
        /// Logs a purchase analytics event.
        /// </summary>
        /// <param name="itemId">The identifier of the purchased item.</param>
        /// <param name="price">The price paid for the item.</param>
        /// <param name="currencyType">The type of currency used.</param>
        public void LogPurchase(string itemId, float price, string currencyType)
        {
            LogEvent("purchase", new Dictionary<string, string>
            {
                { "item_id", itemId },
                { "price", price.ToString("F2") },
                { "currency", currencyType }
            });
        }

        /// <summary>
        /// Logs a hero-selection analytics event.
        /// </summary>
        /// <param name="heroId">The identifier of the selected hero.</param>
        public void LogHeroSelected(string heroId)
        {
            LogEvent("hero_selected", new Dictionary<string, string>
            {
                { "hero_id", heroId }
            });
        }

        /// <summary>
        /// Logs a cosmetic-purchase analytics event.
        /// </summary>
        /// <param name="cosmeticId">The identifier of the purchased cosmetic.</param>
        /// <param name="paymentType">The payment method used.</param>
        public void LogCosmeticPurchased(string cosmeticId, string paymentType)
        {
            LogEvent("cosmetic_purchased", new Dictionary<string, string>
            {
                { "cosmetic_id", cosmeticId },
                { "payment_type", paymentType }
            });
        }

        /// <summary>
        /// Logs a battle-pass tier claim analytics event.
        /// </summary>
        /// <param name="tier">The tier number that was claimed.</param>
        /// <param name="isPremium">Whether the claimed tier is a premium reward.</param>
        public void LogBattlePassTierClaimed(int tier, bool isPremium)
        {
            LogEvent("battlepass_tier_claimed", new Dictionary<string, string>
            {
                { "tier", tier.ToString() },
                { "is_premium", isPremium.ToString() }
            });
        }

        /// <summary>
        /// Logs the session end analytics event.
        /// </summary>
        private void OnApplicationQuit()
        {
            LogEvent("session_end");
        }
    }
}
