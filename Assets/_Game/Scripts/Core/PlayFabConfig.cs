using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// ScriptableObject holding PlayFab configuration. Create via Assets > Create > IOChef > PlayFab Config.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayFabConfig", menuName = "IOChef/PlayFab Config")]
    public class PlayFabConfig : ScriptableObject
    {
        [Header("PlayFab Settings")]
        [Tooltip("Your PlayFab Title ID from the Game Manager dashboard.")]
        public string titleId = "180D84";

        [Header("Currency Codes")]
        [Tooltip("Virtual currency code for coins (max 2 characters).")]
        public string coinsCurrencyCode = "CO";

        [Tooltip("Virtual currency code for gems (max 2 characters).")]
        public string gemsCurrencyCode = "GM";

        [Tooltip("Virtual currency code for hero tokens (max 2 characters).")]
        public string heroTokensCurrencyCode = "HT";
    }
}
