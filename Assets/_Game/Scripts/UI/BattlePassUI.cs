using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace IOChef.UI
{
    /// <summary>
    /// Displays the battle pass UI with tier progression, XP bar, and premium upgrade option.
    /// </summary>
    public class BattlePassUI : MonoBehaviour
    {
        /// <summary>
        /// Text element displaying the current season name.
        /// </summary>
        [Header("Pass Info")]
        [SerializeField] private TextMeshProUGUI seasonNameText;

        /// <summary>
        /// Text element displaying the player's current tier.
        /// </summary>
        [SerializeField] private TextMeshProUGUI currentTierText;

        /// <summary>
        /// Slider showing XP progress towards the next tier.
        /// </summary>
        [SerializeField] private Slider xpProgressBar;

        /// <summary>
        /// Text element displaying the current XP value.
        /// </summary>
        [SerializeField] private TextMeshProUGUI xpText;

        /// <summary>
        /// Scroll rect for browsing through tier cards.
        /// </summary>
        [Header("Tier Display")]
        [SerializeField] private ScrollRect tierScroll;

        /// <summary>
        /// Container transform for spawned tier card instances.
        /// </summary>
        [SerializeField] private Transform tierCardContainer;

        /// <summary>
        /// Prefab used to instantiate tier cards.
        /// </summary>
        [SerializeField] private GameObject tierCardPrefab;

        /// <summary>
        /// Button to upgrade the battle pass to premium.
        /// </summary>
        [Header("Premium")]
        [SerializeField] private Button upgradeToPremiumButton;

        /// <summary>
        /// Text element displaying the premium upgrade price.
        /// </summary>
        [SerializeField] private TextMeshProUGUI premiumPriceText;

        /// <summary>
        /// Badge GameObject shown when the player has the premium pass.
        /// </summary>
        [SerializeField] private GameObject premiumBadge;

        /// <summary>
        /// Button to navigate back to the previous screen.
        /// </summary>
        [Header("Navigation")]
        [SerializeField] private Button backButton;

        /// <summary>
        /// Initializes the battle pass display and populates tier cards.
        /// </summary>
        private void Start()
        {
            if (upgradeToPremiumButton != null)
                upgradeToPremiumButton.onClick.AddListener(OnUpgradePremium);
            if (backButton != null)
                backButton.onClick.AddListener(() => Core.GameManager.Instance?.LoadMainMenu());

            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes all battle pass UI elements to reflect current progression state.
        /// </summary>
        public void RefreshDisplay()
        {
            var bp = Economy.BattlePassManager.Instance;
            if (bp == null) return;

            if (seasonNameText != null)
                seasonNameText.text = $"Season {bp.CurrentSeason}";
            if (currentTierText != null)
                currentTierText.text = $"Tier {bp.CurrentTier}";

            bool isPremium = bp.IsPremiumPass;
            if (premiumBadge != null)
                premiumBadge.SetActive(isPremium);
            if (upgradeToPremiumButton != null)
                upgradeToPremiumButton.gameObject.SetActive(!isPremium);

            PopulateTiers();
        }

        /// <summary>
        /// Creates tier cards for each battle pass tier.
        /// </summary>
        private void PopulateTiers()
        {
            if (tierCardContainer == null || tierCardPrefab == null) return;

            // Clear existing
            foreach (Transform child in tierCardContainer)
                Destroy(child.gameObject);

            var bp = Economy.BattlePassManager.Instance;
            if (bp == null) return;

            for (int tier = 1; tier <= 70; tier++)
            {
                var card = Instantiate(tierCardPrefab, tierCardContainer);

                var tierNum = card.transform.Find("TierNumber")?.GetComponent<TextMeshProUGUI>();
                if (tierNum != null)
                    tierNum.text = tier.ToString();

                bool isCompleted = tier <= bp.CurrentTier;
                bool isClaimed = bp.IsTierClaimed(tier);

                var claimButton = card.transform.Find("ClaimButton")?.GetComponent<Button>();
                if (claimButton != null)
                {
                    claimButton.gameObject.SetActive(isCompleted && !isClaimed);
                    int t = tier;
                    claimButton.onClick.AddListener(() => ClaimTier(t));
                }
            }
        }

        /// <summary>
        /// Claims the reward for the specified tier.
        /// </summary>
        /// <param name="tier">The tier number to claim.</param>
        private void ClaimTier(int tier)
        {
            Economy.BattlePassManager.Instance?.ClaimTierReward(tier);
            RefreshDisplay();
        }

        /// <summary>
        /// Handles the premium upgrade button click.
        /// </summary>
        private void OnUpgradePremium()
        {
            Economy.BattlePassManager.Instance?.PurchasePremiumPass(success =>
            {
                RefreshDisplay();
            });
        }
    }
}
