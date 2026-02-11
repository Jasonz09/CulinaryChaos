using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace IOChef.UI
{
    /// <summary>
    /// Manages the in-game shop UI with tabs for cosmetics, heroes, battle pass, and currency.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        /// <summary>
        /// Button to switch to the cosmetics shop tab.
        /// </summary>
        [Header("Tab Buttons")]
        [SerializeField] private Button cosmeticsTabButton;

        /// <summary>
        /// Button to switch to the heroes shop tab.
        /// </summary>
        [SerializeField] private Button heroesTabButton;

        /// <summary>
        /// Button to switch to the battle pass shop tab.
        /// </summary>
        [SerializeField] private Button battlePassTabButton;

        /// <summary>
        /// Button to switch to the currency shop tab.
        /// </summary>
        [SerializeField] private Button currencyTabButton;

        /// <summary>
        /// Panel displaying the cosmetics shop content.
        /// </summary>
        [Header("Tab Panels")]
        [SerializeField] private GameObject cosmeticsPanel;

        /// <summary>
        /// Panel displaying the heroes shop content.
        /// </summary>
        [SerializeField] private GameObject heroesPanel;

        /// <summary>
        /// Panel displaying the battle pass shop content.
        /// </summary>
        [SerializeField] private GameObject battlePassPanel;

        /// <summary>
        /// Panel displaying the currency shop content.
        /// </summary>
        [SerializeField] private GameObject currencyPanel;

        /// <summary>
        /// Text element displaying the player's current credits balance.
        /// </summary>
        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI creditsText;

        /// <summary>
        /// Text element displaying the player's current gems balance.
        /// </summary>
        [SerializeField] private TextMeshProUGUI gemsText;

        /// <summary>
        /// Container transform for spawned cosmetic card instances.
        /// </summary>
        [Header("Cosmetics")]
        [SerializeField] private Transform cosmeticCardContainer;

        /// <summary>
        /// Prefab used to instantiate cosmetic shop cards.
        /// </summary>
        [SerializeField] private GameObject cosmeticCardPrefab;

        /// <summary>
        /// Container transform for spawned hero card instances.
        /// </summary>
        [Header("Heroes")]
        [SerializeField] private Transform heroCardContainer;

        /// <summary>
        /// Prefab used to instantiate hero shop cards.
        /// </summary>
        [SerializeField] private GameObject heroShopCardPrefab;

        /// <summary>
        /// Button to navigate back to the main menu.
        /// </summary>
        [Header("Navigation")]
        [SerializeField] private Button backButton;

        /// <summary>
        /// Initializes tab buttons, populates shop content, and updates currency display.
        /// </summary>
        private void Start()
        {
            cosmeticsTabButton?.onClick.AddListener(() => ShowTab(0));
            heroesTabButton?.onClick.AddListener(() => ShowTab(1));
            battlePassTabButton?.onClick.AddListener(() => ShowTab(2));
            currencyTabButton?.onClick.AddListener(() => ShowTab(3));
            backButton?.onClick.AddListener(OnBackClicked);

            ShowTab(0);
            UpdateCurrencyDisplay();
            PopulateHeroShop();
        }

        /// <summary>
        /// Switches the visible shop tab panel.
        /// </summary>
        /// <param name="tabIndex">Zero-based index of the tab to display.</param>
        private void ShowTab(int tabIndex)
        {
            cosmeticsPanel?.SetActive(tabIndex == 0);
            heroesPanel?.SetActive(tabIndex == 1);
            battlePassPanel?.SetActive(tabIndex == 2);
            currencyPanel?.SetActive(tabIndex == 3);
        }

        /// <summary>
        /// Refreshes the credits and gems text from CurrencyManager.
        /// </summary>
        private void UpdateCurrencyDisplay()
        {
            if (Economy.CurrencyManager.Instance == null) return;

            if (creditsText != null)
                creditsText.text = Economy.CurrencyManager.Instance.SoftCurrency.ToString("N0");
            if (gemsText != null)
                gemsText.text = Economy.CurrencyManager.Instance.PremiumCurrency.ToString("N0");
        }

        /// <summary>
        /// Fills the hero shop panel with cards for all available heroes.
        /// </summary>
        private void PopulateHeroShop()
        {
            if (Heroes.HeroManager.Instance == null || heroCardContainer == null || heroShopCardPrefab == null)
                return;

            var allHeroes = Heroes.HeroManager.Instance.GetAllHeroes();
            foreach (var hero in allHeroes)
            {
                var card = Instantiate(heroShopCardPrefab, heroCardContainer);

                var nameText = card.transform.Find("HeroName")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null) nameText.text = hero.heroName;

                var descText = card.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
                if (descText != null) descText.text = hero.abilityDescription;

                var priceText = card.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
                if (priceText != null)
                {
                    if (hero.isFreeHero)
                        priceText.text = "Free";
                    else if (Heroes.HeroManager.Instance.IsHeroUnlocked(hero.heroId))
                        priceText.text = "Owned";
                    else
                        priceText.text = $"${hero.purchasePrice:F2}";
                }

                var img = card.transform.Find("HeroImage")?.GetComponent<Image>();
                if (img != null && hero.heroArt != null)
                    img.sprite = hero.heroArt;
            }
        }

        /// <summary>
        /// Navigates back to the main menu.
        /// </summary>
        private void OnBackClicked()
        {
            Core.GameManager.Instance?.LoadMainMenu();
        }
    }
}
