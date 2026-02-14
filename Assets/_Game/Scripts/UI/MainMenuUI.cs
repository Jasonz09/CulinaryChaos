using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using IOChef.Core;
using IOChef.Economy;
using IOChef.Heroes;
using IOChef.Gameplay;
using System.Collections;
using System.Collections.Generic;

namespace IOChef.UI
{
    /// <summary>
    /// Overcooked-style Main Menu – fully programmatic, no Inspector refs needed.
    /// Chunky 3D-style buttons with dark bottom borders, warm food-truck palette.
    /// </summary>
    public partial class MainMenuUI : MonoBehaviour
    {
        // ─── Background Image ───
        [SerializeField] private Sprite backgroundSprite;

        // ─── Cached event delegates (so we can properly unsubscribe) ───
        private System.Action _refreshCurrencyHandler;
        private System.Action<int> _onLevelUpHandler;
        private System.Action<int> _onXPChangedHandler;
        private System.Action<int> _onTierUpHandler;
        private System.Action<CosmeticItem> _onCosmeticPurchasedHandler;

        // ─── Color Palette (Overcooked food-truck) ───

        /// <summary>
        /// Warm orange gradient top color.
        /// </summary>
        private static readonly Color COL_BG_TOP       = new(0.96f, 0.62f, 0.22f);

        /// <summary>
        /// Deep red-orange gradient bottom color.
        /// </summary>
        private static readonly Color COL_BG_BOT       = new(0.90f, 0.38f, 0.15f);

        /// <summary>
        /// Vivid green play button color.
        /// </summary>
        private static readonly Color COL_PLAY         = new(0.30f, 0.75f, 0.30f);

        /// <summary>
        /// Dark green play button shadow color.
        /// </summary>
        private static readonly Color COL_PLAY_SHADOW  = new(0.15f, 0.50f, 0.15f);

        /// <summary>
        /// Golden yellow standard button color.
        /// </summary>
        private static readonly Color COL_BTN          = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Darker gold standard button shadow color.
        /// </summary>
        private static readonly Color COL_BTN_SHADOW   = new(0.82f, 0.62f, 0.08f);

        /// <summary>
        /// Dark brown button text color.
        /// </summary>
        private static readonly Color COL_BTN_TEXT     = new(0.22f, 0.13f, 0.04f);

        /// <summary>
        /// Brown title text shadow color.
        /// </summary>
        private static readonly Color COL_TITLE_SHADOW = new(0.55f, 0.18f, 0.04f);

        /// <summary>
        /// Semi-transparent dark overlay panel background color.
        /// </summary>
        private static readonly Color COL_PANEL_BG     = new(0.12f, 0.08f, 0.04f, 0.88f);

        /// <summary>
        /// Red quit button color.
        /// </summary>
        private static readonly Color COL_QUIT         = new(0.82f, 0.22f, 0.18f);

        /// <summary>
        /// Dark red quit button shadow color.
        /// </summary>
        private static readonly Color COL_QUIT_SHADOW  = new(0.58f, 0.12f, 0.10f);

        /// <summary>
        /// Grey disabled button face color.
        /// </summary>
        private static readonly Color COL_DISABLED     = new(0.58f, 0.55f, 0.52f);

        /// <summary>
        /// Dark grey disabled button shadow color.
        /// </summary>
        private static readonly Color COL_DISABLED_SH  = new(0.40f, 0.38f, 0.35f);

        /// <summary>
        /// Light grey disabled button text color.
        /// </summary>
        private static readonly Color COL_DISABLED_TXT = new(0.85f, 0.83f, 0.80f);

        /// <summary>
        /// Orange badge accent color.
        /// </summary>
        private static readonly Color COL_BADGE        = new(0.95f, 0.38f, 0.10f);

        /// <summary>
        /// Warm semi-transparent tan mode card background color.
        /// </summary>
        private static readonly Color COL_MODE_BG      = new(0.92f, 0.72f, 0.42f, 0.55f);

        // ─── Runtime refs ───

        /// <summary>Cached 9-slice rounded rectangle sprite for modern buttons.</summary>
        private Sprite _cachedRoundedSprite;

        /// <summary>
        /// Root canvas for the main menu UI.
        /// </summary>
        private Canvas mainCanvas;

        /// <summary>
        /// RectTransform of the title text for bob animation.
        /// </summary>
        private RectTransform titleRT;

        /// <summary>
        /// Settings overlay panel root GameObject.
        /// </summary>
        private GameObject settingsPanel;

        /// <summary>
        /// Credits overlay panel root GameObject.
        /// </summary>
        private GameObject creditsPanel;

        /// <summary>
        /// Shop overlay panel root GameObject.
        /// </summary>
        private GameObject shopPanel;

        /// <summary>
        /// Heroes overlay panel root GameObject.
        /// </summary>
        private GameObject heroesPanel;

        private GameObject chestPanel;

        private GameObject battlePassPanel;

        // ── Confirmation Dialog ──
        private GameObject confirmDialog;
        private TMPro.TextMeshProUGUI confirmTitle;
        private TMPro.TextMeshProUGUI confirmMessage;
        private RectTransform confirmButtonArea;

        // ── Account Linking State ──
        private string _pendingAppleToken;
        private string _pendingGoogleAuthCode;
        private TMPro.TextMeshProUGUI linkAppleLabel;
        private TMPro.TextMeshProUGUI linkGoogleLabel;

        // ── Main Menu Currency Display ──
        private TextMeshProUGUI mainMenuCoinsLabel;
        private TextMeshProUGUI mainMenuGemsLabel;

        // ── Main Menu Player Profile ──
        private TextMeshProUGUI playerLevelLabel;
        private Image playerLevelFillBar;

        // ── Season Pass Sidebar Card ──
        private TextMeshProUGUI sidebarBPProgressLabel;
        private Image sidebarBPProgressFill;
        private TextMeshProUGUI sidebarBPBadgeLabel;

        // ── Featured Shop Item Card ──
        private TextMeshProUGUI featuredItemNameLabel;
        private TextMeshProUGUI featuredItemPriceLabel;
        private Image featuredItemIcon;

        /// <summary>
        /// Slider controlling music volume in settings.
        /// </summary>
        private Slider musicSlider;

        /// <summary>
        /// Slider controlling sound effects volume in settings.
        /// </summary>
        private Slider sfxSlider;

        // ─── Shop panel dynamic refs (in MainMenuUI.Shop.cs) ───
        // ─── Heroes panel dynamic refs (in MainMenuUI.Heroes.cs) ───

        // ─── Title bob ───

        /// <summary>
        /// Original anchored position of the title before bob offset.
        /// </summary>
        private Vector2 titleOrigPos;

        /// <summary>
        /// Speed multiplier for the title bob sine wave.
        /// </summary>
        private readonly float bobSpeed = 1.8f;

        /// <summary>
        /// Pixel amplitude of the title bob animation.
        /// </summary>
        private readonly float bobAmount = 10f;

        // ═══════════════════════════════════════════════════════
        //  LIFECYCLE
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Builds the main menu UI and starts the entrance animation.
        /// </summary>
        private void Awake()
        {
            Debug.Log("[MainMenuUI] v4 Awake - flat face buttons, no child Face panel");
            try { EnsureEventSystem(); BuildUI(); }
            catch (System.Exception e)
            { Debug.LogError($"[MainMenuUI] {e.Message}\n{e.StackTrace}"); }
        }

        private void Start()
        {
            RefreshMainMenuCurrency();
            RefreshPlayerProfile();
            RefreshSidebarBattlePass();
            RefreshFeaturedItem();

            // Cache delegates so OnDestroy can properly unsubscribe
            _refreshCurrencyHandler = RefreshMainMenuCurrency;
            _onLevelUpHandler = (level) => RefreshPlayerProfile();
            _onXPChangedHandler = (xp) => RefreshPlayerProfile();
            _onTierUpHandler = (tier) => RefreshSidebarBattlePass();
            _onCosmeticPurchasedHandler = (item) => RefreshFeaturedItem();

            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrenciesRefreshed += _refreshCurrencyHandler;
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnLevelUp += _onLevelUpHandler;
                PlayerLevelManager.Instance.OnXPChanged += _onXPChangedHandler;
            }
            if (BattlePassManager.Instance != null)
            {
                BattlePassManager.Instance.OnTierUp += _onTierUpHandler;
            }
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnCosmeticPurchased += _onCosmeticPurchasedHandler;
            }
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null && _refreshCurrencyHandler != null)
                CurrencyManager.Instance.OnCurrenciesRefreshed -= _refreshCurrencyHandler;
            if (PlayerLevelManager.Instance != null)
            {
                if (_onLevelUpHandler != null)
                    PlayerLevelManager.Instance.OnLevelUp -= _onLevelUpHandler;
                if (_onXPChangedHandler != null)
                    PlayerLevelManager.Instance.OnXPChanged -= _onXPChangedHandler;
            }
            if (BattlePassManager.Instance != null && _onTierUpHandler != null)
            {
                BattlePassManager.Instance.OnTierUp -= _onTierUpHandler;
            }
            if (ShopManager.Instance != null && _onCosmeticPurchasedHandler != null)
            {
                ShopManager.Instance.OnCosmeticPurchased -= _onCosmeticPurchasedHandler;
            }
        }

        private void RefreshMainMenuCurrency()
        {
            if (CurrencyManager.Instance != null)
            {
                if (mainMenuCoinsLabel != null)
                    mainMenuCoinsLabel.text = $"{CurrencyManager.Instance.Coins}";
                if (mainMenuGemsLabel != null)
                    mainMenuGemsLabel.text = $"{CurrencyManager.Instance.Gems}";
            }
        }

        private void RefreshPlayerProfile()
        {
            if (PlayerLevelManager.Instance != null)
            {
                if (playerLevelLabel != null)
                    playerLevelLabel.text = $"{PlayerLevelManager.Instance.CurrentLevel}";
                if (playerLevelFillBar != null)
                    playerLevelFillBar.fillAmount = PlayerLevelManager.Instance.XPProgress;
            }
        }

        private void RefreshSidebarBattlePass()
        {
            if (BattlePassManager.Instance != null)
            {
                int currentTier = BattlePassManager.Instance.CurrentTier;
                if (sidebarBPProgressLabel != null)
                    sidebarBPProgressLabel.text = $"PROGRESS: {currentTier}/4";
                if (sidebarBPBadgeLabel != null)
                    sidebarBPBadgeLabel.text = currentTier.ToString();
                if (sidebarBPProgressFill != null)
                {
                    float progress = (float)currentTier / 70f;
                    var rt = sidebarBPProgressFill.rectTransform;
                    rt.anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
                }
            }
        }

        private void RefreshFeaturedItem()
        {
            if (ShopManager.Instance == null) return;

            // Get featured cosmetic from shop using the helper method
            var featured = ShopManager.Instance.GetFeaturedItem();

            if (featured != null)
            {
                if (featuredItemNameLabel != null)
                    featuredItemNameLabel.text = featured.displayName.ToUpper();

                if (featuredItemPriceLabel != null)
                {
                    if (featured.priceGems > 0)
                        featuredItemPriceLabel.text = $"{featured.priceGems} ◆";
                    else if (featured.priceCredits > 0)
                        featuredItemPriceLabel.text = $"{featured.priceCredits} ◉";
                    else
                        featuredItemPriceLabel.text = "FREE";
                }

                if (featuredItemIcon != null && featured.previewImage != null)
                {
                    featuredItemIcon.sprite = featured.previewImage;
                    featuredItemIcon.color = Color.white;
                    featuredItemIcon.type = Image.Type.Simple;
                }
                else if (featuredItemIcon != null)
                {
                    // Fallback to rounded sprite if no preview image
                    featuredItemIcon.sprite = GetRoundedSprite();
                    featuredItemIcon.color = new Color(0, 0, 0, 0.3f);
                }
            }
        }

        /// <summary>
        /// Animates the title bob effect.
        /// </summary>
        private void Update()
        {
            if (titleRT != null)
            {
                float y = titleOrigPos.y + Mathf.Sin(Time.unscaledTime * bobSpeed) * bobAmount;
                titleRT.anchoredPosition = new Vector2(titleOrigPos.x, y);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  BUILD
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Constructs the full main menu layout.
        /// </summary>
        private void BuildUI()
        {
            Debug.Log("[MainMenuUI] Starting BuildUI...");
            var cgo = new GameObject("MainMenuCanvas");
            cgo.transform.SetParent(transform);
            mainCanvas = cgo.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 10;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();
            var root = cgo.GetComponent<RectTransform>();

            BuildBackground(root);
            BuildModernHUD(root);
            
            settingsPanel = SafeBuildPanel(root, "Settings", BuildSettingsPanel);
            creditsPanel  = SafeBuildPanel(root, "Credits", BuildCreditsPanel);
            shopPanel     = SafeBuildPanel(root, "Shop", BuildShopPanel);
            heroesPanel   = SafeBuildPanel(root, "Heroes", BuildHeroesPanel);
            chestPanel    = SafeBuildPanel(root, "Chest", BuildChestPanel);
            battlePassPanel = SafeBuildPanel(root, "BattlePass", BuildBattlePassPanel);
            confirmDialog = SafeBuildPanel(root, "ConfirmDialog", BuildConfirmDialog);
        }

        /// <summary>
        /// Safely builds a panel: if an exception occurs, the partially-built panel
        /// is hidden so it doesn't corrupt the menu visually.
        /// </summary>
        private GameObject SafeBuildPanel(RectTransform root, string name, System.Func<RectTransform, GameObject> builder)
        {
            try
            {
                return builder(root);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainMenuUI] Failed to build {name} panel: {e.Message}\n{e.StackTrace}");
                // Try to find and hide any partially-built panel
                var panelTransform = root.Find(name + "Panel");
                if (panelTransform != null)
                    panelTransform.gameObject.SetActive(false);
                return null;
            }
        }

        // ─── Background ───
        /// <summary>
        /// Loads the user-provided background image and stretches it full-screen.
        /// Falls back to a warm solid color if image not found.
        /// </summary>
        private void BuildBackground(RectTransform p)
        {
            var bgSprite = Resources.Load<Sprite>("UI/MainMenuBackground");
            if (bgSprite != null)
            {
                Debug.Log("[MainMenuUI] Background image loaded successfully");
                var bgImg = MakePanel(p, "Bg", Color.white);
                Stretch(bgImg);
                var img = bgImg.GetComponent<Image>();
                img.sprite = bgSprite;
                img.preserveAspect = false;
                img.raycastTarget = false;
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Background image not found at Resources/UI/MainMenuBackground");
                var bg = MakePanel(p, "Bg", new Color(0.18f, 0.12f, 0.08f));
                Stretch(bg);
            }

        }

        // ─── Title ─── (Deprecated)
        private void BuildTitle(RectTransform p)
        {
        }

        // ─── Currency Bar ─── (Deprecated)
        private void BuildCurrencyBar(RectTransform p)
        {
        }

        // ─── Modern HUD ───
        private void BuildModernHUD(RectTransform root)
        {
            // =================================================================================
            // 1. HEADER BAR - Clean, full-width navigation bar
            // =================================================================================

            // Solid dark header background
            var topBar = MakePanel(root, "TopBar", new Color(0.10f, 0.11f, 0.14f, 1f));
            topBar.anchorMin = new Vector2(0, 1);
            topBar.anchorMax = new Vector2(1, 1);
            topBar.pivot = new Vector2(0.5f, 1);
            topBar.sizeDelta = new Vector2(0, 56);
            topBar.anchoredPosition = Vector2.zero;

            // Thin bottom accent line
            var barLine = MakePanel(topBar, "BottomLine", new Color(0.22f, 0.24f, 0.28f, 1f));
            barLine.anchorMin = new Vector2(0, 0); barLine.anchorMax = new Vector2(1, 0);
            barLine.sizeDelta = new Vector2(0, 1); barLine.anchoredPosition = Vector2.zero;


            // ─── LEFT: Game Title ───
            var titleArea = MakePanel(topBar, "TitleArea", Color.clear);
            titleArea.anchorMin = new Vector2(0, 0); titleArea.anchorMax = new Vector2(0.16f, 1);
            titleArea.offsetMin = new Vector2(18, 0); titleArea.offsetMax = Vector2.zero;
            var titleTxt = MakeText(titleArea, "Title", "CULINARY CHAOS", 15, new Color(1f, 0.88f, 0.55f), FontStyles.Bold);
            Stretch(titleTxt);
            var titleTMP = titleTxt.GetComponent<TextMeshProUGUI>();
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            titleTMP.textWrappingMode = TextWrappingModes.NoWrap;


            // ─── CENTER: Navigation Buttons ───
            var navContainer = MakePanel(topBar, "NavContainer", Color.clear);
            navContainer.anchorMin = new Vector2(0.22f, 0); navContainer.anchorMax = new Vector2(0.68f, 1);
            navContainer.offsetMin = new Vector2(0, 0); navContainer.offsetMax = new Vector2(0, 0);

            var navHL = navContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            navHL.childAlignment = TextAnchor.MiddleCenter;
            navHL.spacing = 6;
            navHL.childControlWidth = true; navHL.childForceExpandWidth = true;
            navHL.childControlHeight = true; navHL.childForceExpandHeight = true;
            navHL.padding = new RectOffset(4, 4, 0, 0);

            // ── Nav Button Builder (tab-style with orange underline for active) ──
            void MakeNavBtn(string label, System.Action onClick, bool active, bool disabled)
            {
                Color txtColor;
                if (active) {
                    txtColor = Color.white;
                } else if (disabled) {
                    txtColor = new Color(0.45f, 0.47f, 0.52f);
                } else {
                    txtColor = new Color(0.72f, 0.74f, 0.78f);
                }

                // Transparent background — no pill shape
                var tab = MakePanel(navContainer, label + "Btn", Color.clear);

                var txt = MakeText(tab, "Lbl", label, 14, txtColor, FontStyles.Bold);
                Stretch(txt);
                var tmp = txt.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.textWrappingMode = TextWrappingModes.NoWrap;

                // Orange underline indicator for active tab
                if (active)
                {
                    var underline = MakePanel(tab, "Underline", new Color(1f, 0.55f, 0.1f, 1f));
                    underline.anchorMin = new Vector2(0.15f, 0);
                    underline.anchorMax = new Vector2(0.85f, 0);
                    underline.pivot = new Vector2(0.5f, 0);
                    underline.sizeDelta = new Vector2(0, 3);
                    underline.anchoredPosition = new Vector2(0, 4);
                    underline.GetComponent<Image>().sprite = GetRoundedSprite();
                }

                var btn = tab.gameObject.AddComponent<Button>();
                btn.targetGraphic = tab.GetComponent<Image>();
                btn.transition = Selectable.Transition.ColorTint;
                btn.interactable = !disabled;

                if (!disabled)
                {
                    btn.onClick.AddListener(() => onClick?.Invoke());
                    ColorBlock cb = btn.colors;
                    cb.normalColor = Color.white;
                    cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
                    cb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
                    cb.selectedColor = Color.white;
                    cb.fadeDuration = 0.1f;
                    cb.colorMultiplier = 1f;
                    btn.colors = cb;
                }
                else
                {
                    ColorBlock cb = btn.colors;
                    cb.disabledColor = Color.white;
                    cb.colorMultiplier = 1f;
                    btn.colors = cb;
                }
            }

            MakeNavBtn("HEROES",  OnHeroesClicked,     true,  false);
            MakeNavBtn("PETS",    () => { },            false, true);   // disabled
            MakeNavBtn("RECIPES", OnRecipeBookClicked,  false, false);
            MakeNavBtn("SHOP",    OnShopClicked,        false, false);


            // ─── RIGHT: Currency + Level + Settings ───
            var profileArea = MakePanel(topBar, "ProfileArea", Color.clear);
            profileArea.anchorMin = new Vector2(0.68f, 0); profileArea.anchorMax = new Vector2(1, 1);
            profileArea.offsetMin = new Vector2(0, 0);
            profileArea.offsetMax = new Vector2(-12, 0);

            var profHL = profileArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            profHL.childAlignment = TextAnchor.MiddleRight;
            profHL.spacing = 8;
            profHL.childControlWidth = false; profHL.childForceExpandWidth = false;
            profHL.padding = new RectOffset(0, 0, 6, 6);

            // ── Coins chip ──
            var coinChip = MakePanel(profileArea, "CoinChip", new Color(0.16f, 0.17f, 0.20f, 1f));
            coinChip.GetComponent<Image>().sprite = GetRoundedSprite();
            var coinChipLE = coinChip.gameObject.AddComponent<LayoutElement>();
            coinChipLE.preferredWidth = 100; coinChipLE.preferredHeight = 34;

            var coinHL = coinChip.gameObject.AddComponent<HorizontalLayoutGroup>();
            coinHL.spacing = 5; coinHL.childAlignment = TextAnchor.MiddleCenter;
            coinHL.childControlWidth = false; coinHL.childForceExpandWidth = false;
            coinHL.padding = new RectOffset(10, 10, 0, 0);

            var coinDot = MakePanel(coinChip, "Dot", new Color(1f, 0.78f, 0.2f, 1f));
            coinDot.GetComponent<Image>().sprite = GetRoundedSprite();
            var cdLE = coinDot.gameObject.AddComponent<LayoutElement>();
            cdLE.preferredWidth = 14; cdLE.preferredHeight = 14;

            mainMenuCoinsLabel = MakeText(coinChip, "Val", "1,050", 13, new Color(1f, 0.92f, 0.6f), FontStyles.Bold).GetComponent<TextMeshProUGUI>();
            mainMenuCoinsLabel.alignment = TextAlignmentOptions.MidlineLeft;
            mainMenuCoinsLabel.textWrappingMode = TextWrappingModes.NoWrap;
            var cValLE = mainMenuCoinsLabel.gameObject.AddComponent<LayoutElement>();
            cValLE.preferredWidth = 60;

            // ── Gems chip ──
            var gemChip = MakePanel(profileArea, "GemChip", new Color(0.16f, 0.17f, 0.20f, 1f));
            gemChip.GetComponent<Image>().sprite = GetRoundedSprite();
            var gemChipLE = gemChip.gameObject.AddComponent<LayoutElement>();
            gemChipLE.preferredWidth = 90; gemChipLE.preferredHeight = 34;

            var gemHL = gemChip.gameObject.AddComponent<HorizontalLayoutGroup>();
            gemHL.spacing = 5; gemHL.childAlignment = TextAnchor.MiddleCenter;
            gemHL.childControlWidth = false; gemHL.childForceExpandWidth = false;
            gemHL.padding = new RectOffset(10, 10, 0, 0);

            var gemDot = MakePanel(gemChip, "Dot", new Color(0.4f, 0.7f, 1f, 1f));
            gemDot.GetComponent<Image>().sprite = GetRoundedSprite();
            var gdLE = gemDot.gameObject.AddComponent<LayoutElement>();
            gdLE.preferredWidth = 14; gdLE.preferredHeight = 14;

            mainMenuGemsLabel = MakeText(gemChip, "Val", "100", 13, new Color(0.7f, 0.9f, 1f), FontStyles.Bold).GetComponent<TextMeshProUGUI>();
            mainMenuGemsLabel.alignment = TextAlignmentOptions.MidlineLeft;
            mainMenuGemsLabel.textWrappingMode = TextWrappingModes.NoWrap;
            var gValLE = mainMenuGemsLabel.gameObject.AddComponent<LayoutElement>();
            gValLE.preferredWidth = 50;

            // ── Level badge ──
            var levelBadge = MakePanel(profileArea, "LevelBadge", new Color(0.16f, 0.17f, 0.20f, 1f));
            levelBadge.GetComponent<Image>().sprite = GetRoundedSprite();
            var lbLE = levelBadge.gameObject.AddComponent<LayoutElement>();
            lbLE.preferredWidth = 56; lbLE.preferredHeight = 34;

            var lbHL = levelBadge.gameObject.AddComponent<HorizontalLayoutGroup>();
            lbHL.childAlignment = TextAnchor.MiddleCenter;
            lbHL.spacing = 2;
            lbHL.padding = new RectOffset(6, 6, 0, 0);
            lbHL.childControlWidth = false; lbHL.childForceExpandWidth = false;

            var lvlLbl = MakeText(levelBadge, "Lbl", "LV", 10, new Color(0.55f, 0.58f, 0.65f), FontStyles.Bold);
            var lvlLblLE = lvlLbl.gameObject.AddComponent<LayoutElement>();
            lvlLblLE.preferredWidth = 18;
            lvlLbl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineRight;

            playerLevelLabel = MakeText(levelBadge, "Num", "69", 16, new Color(0.5f, 0.85f, 1f), FontStyles.Bold).GetComponent<TextMeshProUGUI>();
            playerLevelLabel.alignment = TextAlignmentOptions.MidlineLeft;
            playerLevelLabel.textWrappingMode = TextWrappingModes.NoWrap;
            var plNumLE = playerLevelLabel.gameObject.AddComponent<LayoutElement>();
            plNumLE.preferredWidth = 26;

            // Hidden fill bar reference (kept for compatibility)
            var hiddenFill = MakePanel(levelBadge, "Fill", Color.clear);
            hiddenFill.gameObject.SetActive(false);
            playerLevelFillBar = hiddenFill.GetComponent<Image>();

            // ── Settings button ──
            var setBtn = MakePanel(profileArea, "SettingsBtn", new Color(0.16f, 0.17f, 0.20f, 1f));
            setBtn.GetComponent<Image>().sprite = GetRoundedSprite();
            var sBtnLE = setBtn.gameObject.AddComponent<LayoutElement>();
            sBtnLE.preferredWidth = 36; sBtnLE.preferredHeight = 34;
            var setBtnIcon = MakeText(setBtn, "Icon", "SET", 10, new Color(0.65f, 0.68f, 0.72f), FontStyles.Bold);
            Stretch(setBtnIcon);
            setBtnIcon.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            var setBtnBtn = setBtn.gameObject.AddComponent<Button>();
            setBtnBtn.targetGraphic = setBtn.GetComponent<Image>();
            setBtnBtn.onClick.AddListener(OnSettingsClicked);
            setBtnBtn.transition = Selectable.Transition.ColorTint;

            ColorBlock scb = setBtnBtn.colors;
            scb.normalColor = Color.white;
            scb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            scb.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            scb.selectedColor = Color.white;
            scb.colorMultiplier = 1f;
            scb.fadeDuration = 0.1f;
            setBtnBtn.colors = scb;

            // Add spring effect to settings button
            setBtn.gameObject.AddComponent<ButtonSpringEffect>();


            // ─── END HEADER ───


            // =================================================================================
            // 2. LEFT SIDEBAR & CONTENT
            // =================================================================================
            var leftBar = MakePanel(root, "LeftSidebar", Color.clear);
            leftBar.anchorMin = new Vector2(0, 0.12f);
            leftBar.anchorMax = new Vector2(0.25f, 0.82f); // Lowered top to clear header
            leftBar.offsetMin = new Vector2(30, 0); leftBar.offsetMax = new Vector2(-10, 0);
            
            var lblg = leftBar.gameObject.AddComponent<VerticalLayoutGroup>();
            lblg.spacing = 16; lblg.childAlignment = TextAnchor.UpperLeft; 
            lblg.childControlHeight = false; lblg.childForceExpandHeight = false;
            lblg.childControlWidth = true; lblg.childForceExpandWidth = true;

            // ═══ Season Pass Card — Modern Clean Design (Clickable) ═══
            // Main Container
            var passCard = MakePanel(leftBar, "SeasonPassCard", new Color(0.12f, 0.14f, 0.18f, 0.98f));
            var passCardLE = passCard.gameObject.AddComponent<LayoutElement>();
            passCardLE.minHeight = 110; passCardLE.preferredHeight = 110; passCardLE.flexibleWidth = 1;
            
            // Make it clickable
            var passCardBtn = passCard.gameObject.AddComponent<Button>();
            passCardBtn.targetGraphic = passCard.GetComponent<Image>();
            passCardBtn.onClick.AddListener(OnSeasonPassClicked);
            passCardBtn.transition = Selectable.Transition.ColorTint;
            ColorBlock passCardCB = passCardBtn.colors;
            passCardCB.normalColor = Color.white;
            passCardCB.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            passCardCB.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            passCardCB.selectedColor = Color.white;
            passCardCB.colorMultiplier = 1f;
            passCardCB.fadeDuration = 0.1f;
            passCardBtn.colors = passCardCB;
            
            // Layout Group for Card
            var passVL = passCard.gameObject.AddComponent<VerticalLayoutGroup>();
            passVL.padding = new RectOffset(16, 16, 14, 14);
            passVL.spacing = 8;
            passVL.childControlHeight = true; passVL.childControlWidth = true;
            passVL.childForceExpandHeight = false; passVL.childForceExpandWidth = true;
            passVL.childAlignment = TextAnchor.UpperLeft;

            // Border
            var outline = passCard.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.1f);
            outline.effectDistance = new Vector2(1, -1);

            // 1. Header Row (Guaranteed Height)
            var headerRow = MakePanel(passCard, "HeaderRow", Color.clear);
            var hLE = headerRow.gameObject.AddComponent<LayoutElement>();
            hLE.minHeight = 18; hLE.preferredHeight = 18; hLE.flexibleWidth = 1;
            
            var headerTxt = MakeText(headerRow, "Title", "SEASON PASS", 12, new Color(0.6f, 0.6f, 0.7f), FontStyles.Bold);
            // Force text to fill the header row and NOT wrap
            var hRect = headerTxt.GetComponent<RectTransform>();
            hRect.anchorMin = Vector2.zero; hRect.anchorMax = Vector2.one;
            hRect.offsetMin = hRect.offsetMax = Vector2.zero;
            headerTxt.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.NoWrap;
            headerTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // 2. Main Body Row (Split: Stats | Badge)
            var bodyRow = MakePanel(passCard, "BodyRow", Color.clear);
            var bLE = bodyRow.gameObject.AddComponent<LayoutElement>();
            bLE.flexibleHeight = 1; bLE.flexibleWidth = 1;

            var bodyHL = bodyRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            bodyHL.spacing = 12; bodyHL.childControlWidth = true; bodyHL.childForceExpandWidth = false;
            bodyHL.childControlHeight = true; bodyHL.childForceExpandHeight = true;

            // Left Col: Level Text & Bar
            var leftCol = MakePanel(bodyRow, "LeftCol", Color.clear);
            var lcLE = leftCol.gameObject.AddComponent<LayoutElement>();
            lcLE.flexibleWidth = 1; 

            var leftVL = leftCol.gameObject.AddComponent<VerticalLayoutGroup>();
            leftVL.spacing = 6; leftVL.childAlignment = TextAnchor.MiddleLeft;
            leftVL.childControlHeight = true; leftVL.childForceExpandHeight = false;
            
            // Level Text
            int currentBPTier = BattlePassManager.Instance?.CurrentTier ?? 14;
            sidebarBPProgressLabel = MakeText(leftCol, "Level", $"LEVEL {currentBPTier}", 18, new Color(1f, 0.75f, 0.2f), FontStyles.Bold).GetComponent<TextMeshProUGUI>();
            var pLE = sidebarBPProgressLabel.gameObject.AddComponent<LayoutElement>();
            pLE.minHeight = 24;

            // Progress Bar Container (FIXED HEIGHT)
            var barContainer = MakePanel(leftCol, "BarContainer", new Color(0.08f,0.08f,0.1f,0.9f));
            var barBgLE = barContainer.gameObject.AddComponent<LayoutElement>();
            barBgLE.minHeight = 12; barBgLE.preferredHeight = 12; barBgLE.flexibleWidth = 1;
            barContainer.GetComponent<Image>().sprite = GetRoundedSprite();

            // Progress Fill
            sidebarBPProgressFill = MakePanel(barContainer, "Fill", new Color(1f, 0.7f, 0.15f)).GetComponent<Image>();
            float bpProgress = BattlePassManager.Instance != null ? (float)BattlePassManager.Instance.CurrentTier / 70f : 0.2f;
            sidebarBPProgressFill.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            sidebarBPProgressFill.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Clamp01(bpProgress), 1);
            sidebarBPProgressFill.GetComponent<RectTransform>().offsetMin = sidebarBPProgressFill.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            sidebarBPProgressFill.sprite = GetRoundedSprite();

            // Next Reward Label
            var nextTxt = MakeText(leftCol, "Next", "NEXT 100 GEMS", 9, new Color(0.5f, 0.5f, 0.55f), FontStyles.Normal);
            
            // Right Col: Badge
            var rightCol = MakePanel(bodyRow, "RightCol", Color.clear);
            var rcLE = rightCol.gameObject.AddComponent<LayoutElement>();
            rcLE.minWidth = 54; rcLE.preferredWidth = 54;
            
            var badge = MakePanel(rightCol, "Badge", new Color(0.15f, 0.16f, 0.2f));
            CenterBox(badge.GetComponent<RectTransform>(), 52, 52);
            badge.GetComponent<Image>().sprite = GetRoundedSprite();
            
            var bOutline = badge.gameObject.AddComponent<UnityEngine.UI.Outline>();
            bOutline.effectColor = new Color(1f, 0.75f, 0.25f);
            bOutline.effectDistance = new Vector2(2, -2);
            
            int badgeTier = BattlePassManager.Instance?.CurrentTier ?? 14;
            var bVal = MakeText(badge, "Val", badgeTier.ToString(), 22, new Color(1f, 0.8f, 0.3f), FontStyles.Bold);
            sidebarBPBadgeLabel = bVal.GetComponent<TextMeshProUGUI>();
            Stretch(bVal); 
            sidebarBPBadgeLabel.alignment = TextAlignmentOptions.Center;


            // ═══ Featured Shop Item — Clean Horizontal Card ═══
            var featCard = MakePanel(leftBar, "FeaturedCard", new Color(0.12f, 0.14f, 0.18f, 0.98f));
            var fcLE = featCard.gameObject.AddComponent<LayoutElement>();
            fcLE.minHeight = 120; fcLE.preferredHeight = 120; fcLE.flexibleWidth = 1;

            var featHL = featCard.gameObject.AddComponent<HorizontalLayoutGroup>();
            featHL.padding = new RectOffset(12, 12, 12, 12);
            featHL.spacing = 16;
            featHL.childControlWidth = true; featHL.childControlHeight = true;
            featHL.childForceExpandWidth = false; featHL.childForceExpandHeight = true;
            
            // Icon Box (Square)
            var iconBox = MakePanel(featCard, "IconBox", new Color(0,0,0,0.3f));
            var ibLE = iconBox.gameObject.AddComponent<LayoutElement>();
            ibLE.minWidth = 96; ibLE.preferredWidth = 96; // Square-ish with height
            featuredItemIcon = iconBox.GetComponent<Image>();
            featuredItemIcon.sprite = GetRoundedSprite();

            var ibIcon = MakeText(iconBox, "I", "⚔️", 40, Color.white, FontStyles.Normal);
            Stretch(ibIcon); ibIcon.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Info Column
            var featInfo = MakePanel(featCard, "Info", Color.clear);
            var fiLE = featInfo.gameObject.AddComponent<LayoutElement>();
            fiLE.flexibleWidth = 1; 

            var featVL = featInfo.gameObject.AddComponent<VerticalLayoutGroup>();
            featVL.spacing = 4; featVL.childAlignment = TextAnchor.UpperLeft;
            featVL.childControlHeight = true; featVL.childForceExpandHeight = false;

            // Title
            var fTitle = MakeText(featInfo, "H", "FEATURED ITEM", 9, new Color(0.6f, 0.6f, 0.65f), FontStyles.Bold);
            var ftLE = fTitle.gameObject.AddComponent<LayoutElement>();
            ftLE.preferredHeight = 12;
            
            // Name
            var fName = MakeText(featInfo, "N", "EPIC KNIFE", 14, Color.white, FontStyles.Bold);
            var fNameLE = fName.gameObject.AddComponent<LayoutElement>();
            fNameLE.minHeight = 20;
            featuredItemNameLabel = fName.GetComponent<TextMeshProUGUI>();

            // Spacer
            var fSpacer = MakePanel(featInfo, "Sp", Color.clear);
            fSpacer.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

            // Price Button
            var priceBtn = MakePanel(featInfo, "Btn", new Color(0.2f, 0.7f, 0.4f));
            var priceBtnLE = priceBtn.gameObject.AddComponent<LayoutElement>();
            priceBtnLE.minHeight = 30; priceBtnLE.preferredHeight = 30; priceBtnLE.preferredWidth = 110;
            priceBtn.GetComponent<Image>().sprite = GetRoundedSprite();
            
            var pVal = MakeText(priceBtn, "P", "1200", 16, Color.white, FontStyles.Bold);
            featuredItemPriceLabel = pVal.GetComponent<TextMeshProUGUI>();
            Stretch(pVal); featuredItemPriceLabel.alignment = TextAlignmentOptions.Center;

            // ═══ 3. Play Button (Bottom Right) — HEAVY Skew ═══
            var playContainer = MakePanel(root, "PlayContainer", Color.clear);
            playContainer.anchorMin = new Vector2(1, 0); playContainer.anchorMax = new Vector2(1, 0);
            playContainer.pivot = new Vector2(1, 0);
            playContainer.anchoredPosition = new Vector2(-40, 40);
            playContainer.sizeDelta = new Vector2(240, 80);

            // Use the new Skewed Helper
            // Skew X = -0.3 for aggressive slant
            var playBtn = MakeSkewedButton(
                playContainer, 
                "PLAY", 
                new Color(1.0f, 0.45f, 0.0f), // Face
                new Color(0.6f, 0.18f, 0.0f), // Shadow
                Color.white, 
                36, 
                240, 
                80, 
                OnPlayClicked,
                -0.3f
            );
            // Ensure button fills container
            Stretch(playBtn);
        }

        // ─── Recipe Book Click Handler ───
        private void OnRecipeBookClicked()
        {
            Debug.Log("[MainMenuUI] Recipe Book clicked - Not yet implemented");
            // TODO: Implement recipe book feature
        }

        // ─── Settings Panel ───
        /// <summary>
        /// Creates the settings panel with volume sliders.
        /// </summary>
        /// <param name="p">Parent RectTransform for the settings panel.</param>
        /// <returns>The settings panel GameObject.</returns>
        private GameObject BuildSettingsPanel(RectTransform p)
        {
            var panel = MakePanel(p, "SettingsPanel", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "SBox", new Color(0.95f, 0.88f, 0.75f));
            CenterBox(box, 620, 780);

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 22; vl.childAlignment = TextAnchor.MiddleCenter;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;
            vl.childControlHeight = true; vl.childControlWidth = true;
            vl.padding = new RectOffset(34, 34, 34, 34);

            AddLayoutText(box, "SH", "SETTINGS", 42, COL_BTN_TEXT, FontStyles.Bold, 52);
            AddLayoutText(box, "ML", "Music", 24, COL_BTN_TEXT, FontStyles.Normal, 32);

            musicSlider = MakeSlider(box, "MSlider",
                AudioManager.Instance != null ? AudioManager.Instance.musicVolume : 0.7f);
            musicSlider.onValueChanged.AddListener(v =>
            { if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v); });

            AddLayoutText(box, "SL", "Sound Effects", 24, COL_BTN_TEXT, FontStyles.Normal, 32);

            sfxSlider = MakeSlider(box, "SSlider",
                AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 1f);
            sfxSlider.onValueChanged.AddListener(v =>
            { if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(v); });

            // ── Account Linking ──
            AddLayoutText(box, "LinkH", "ACCOUNT LINKING", 24,
                COL_BTN_TEXT, FontStyles.Bold, 34);
            AddLayoutText(box, "LinkD", "Link to save progress across devices", 16,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Italic, 22);

            Color appleBg = new(0.15f, 0.15f, 0.15f);
            Color appleSh = new(0.05f, 0.05f, 0.05f);
            linkAppleLabel = MakeChunkyButtonWithLabel(box, "LINK APPLE ACCOUNT", appleBg, appleSh,
                Color.white, 20, 58, OnLinkAppleClicked);

            Color googleBg = new(0.26f, 0.52f, 0.96f);
            Color googleSh = new(0.15f, 0.35f, 0.72f);
            linkGoogleLabel = MakeChunkyButtonWithLabel(box, "LINK GOOGLE ACCOUNT", googleBg, googleSh,
                Color.white, 20, 58, OnLinkGoogleClicked);

            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 26, 58,
                () => panel.gameObject.SetActive(false));

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        // ─── Credits Panel ───
        /// <summary>
        /// Creates the credits panel with attribution text.
        /// </summary>
        /// <param name="p">Parent RectTransform for the credits panel.</param>
        /// <returns>The credits panel GameObject.</returns>
        private GameObject BuildCreditsPanel(RectTransform p)
        {
            var panel = MakePanel(p, "CreditsPanel", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "CBox", new Color(0.95f, 0.88f, 0.75f));
            CenterBox(box, 620, 440);

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 16; vl.childAlignment = TextAnchor.MiddleCenter;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;
            vl.childControlHeight = true; vl.childControlWidth = true;
            vl.padding = new RectOffset(34, 34, 34, 34);

            AddLayoutText(box, "CH", "CREDITS", 42, COL_BTN_TEXT, FontStyles.Bold, 52);
            AddLayoutText(box, "CB",
                "IOChef - A Cooking Game\n\nDeveloped with Unity\nInspired by Overcooked\n\nThank you for playing!",
                20, COL_BTN_TEXT, FontStyles.Normal, 170);

            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 26, 58,
                () => panel.gameObject.SetActive(false));

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        // ─── Confirmation Dialog ───

        private GameObject BuildConfirmDialog(RectTransform p)
        {
            var panel = MakePanel(p, "ConfirmDialog", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "CBox", new Color(0.95f, 0.88f, 0.75f));
            CenterBox(box, 640, 480);

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 14; vl.childAlignment = TextAnchor.MiddleCenter;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;
            vl.childControlHeight = true; vl.childControlWidth = true;
            vl.padding = new RectOffset(30, 30, 30, 30);

            // Title
            var titleRT = MakeText(box, "Title", "Confirm", 32, COL_BTN_TEXT, FontStyles.Bold);
            AddLE(titleRT.gameObject, 44);
            confirmTitle = titleRT.GetComponent<TextMeshProUGUI>();

            // Message
            var msgRT = MakeText(box, "Msg", "", 18, new Color(0.3f, 0.2f, 0.1f), FontStyles.Normal);
            AddLE(msgRT.gameObject, 180);
            confirmMessage = msgRT.GetComponent<TextMeshProUGUI>();

            // Button area — buttons are added dynamically by ShowConfirmDialog
            var btnArea = MakePanel(box, "Buttons", Color.clear);
            AddLE(btnArea.gameObject, 140);
            var btnVL = btnArea.gameObject.AddComponent<VerticalLayoutGroup>();
            btnVL.spacing = 10; btnVL.childAlignment = TextAnchor.MiddleCenter;
            btnVL.childForceExpandWidth = true; btnVL.childForceExpandHeight = false;
            btnVL.childControlHeight = true; btnVL.childControlWidth = true;
            btnVL.padding = new RectOffset(10, 10, 4, 4);
            confirmButtonArea = btnArea;

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        private void ShowConfirmDialog(string title, string message,
            string btn1Label, Color btn1Color, System.Action btn1Action,
            string btn2Label = null, Color? btn2Color = null, System.Action btn2Action = null)
        {
            if (confirmDialog == null) return;

            confirmTitle.text = title;
            confirmMessage.text = message;

            // Clear old buttons
            for (int i = confirmButtonArea.childCount - 1; i >= 0; i--)
                Destroy(confirmButtonArea.GetChild(i).gameObject);

            // Primary button
            MakeChunkyButton(confirmButtonArea, btn1Label, btn1Color,
                btn1Color * 0.7f, Color.white, 20, 54, () =>
                {
                    confirmDialog.SetActive(false);
                    btn1Action?.Invoke();
                });

            // Secondary button (optional)
            if (btn2Label != null)
            {
                Color b2c = btn2Color ?? COL_BTN;
                MakeChunkyButton(confirmButtonArea, btn2Label, b2c,
                    b2c * 0.7f, Color.white, 20, 54, () =>
                    {
                        confirmDialog.SetActive(false);
                        btn2Action?.Invoke();
                    });
            }

            confirmDialog.SetActive(true);
        }

        private void HideConfirmDialog()
        {
            if (confirmDialog != null) confirmDialog.SetActive(false);
        }

        // BuildShopPanel moved to MainMenuUI.Shop.cs

        // BuildHeroesPanel moved to MainMenuUI.Heroes.cs

        // ─── Version Text ───
        /// <summary>
        /// Creates the version number text. (Deprecated, now handled in BuildButtonGroup)
        /// </summary>
        private void BuildVersionText(RectTransform p)
        {
            // Disabled to prevent duplicate overlapping text
        }

        // ═══════════════════════════════════════════════════════
        //  CALLBACKS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Navigates to the level select screen.
        /// </summary>
        private void OnPlayClicked()
        { if (GameManager.Instance != null) GameManager.Instance.LoadLevelSelect(); }

        private void ShowPanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(true);
        }

        private void OnBattlePassClicked()
        {
            if (battlePassPanel != null) battlePassPanel.SetActive(true);
        }

        /// <summary>
        /// Toggles the settings panel visibility.
        /// </summary>
        private void OnSettingsClicked()
        { if (settingsPanel != null) settingsPanel.SetActive(true); }

        /// <summary>
        /// Toggles the credits panel visibility.
        /// </summary>
        private void OnCreditsClicked()
        { if (creditsPanel != null) creditsPanel.SetActive(true); }

        /// <summary>
        /// Opens the shop overlay panel.
        /// </summary>
        private void OnShopClicked()
        {
            if (shopPanel != null)
            {
                RefreshShopPanel();
                shopPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Opens the heroes overlay panel.
        /// </summary>
        private void OnHeroesClicked()
        {
            if (heroesPanel != null)
            {
                RefreshHeroesPanel();
                heroesPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Opens the dedicated chests panel.
        /// </summary>
        private void OnChestsClicked()
        {
            if (chestPanel != null)
            {
                RefreshChestPanel();
                chestPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Opens the season pass panel.
        /// </summary>
        private void OnSeasonPassClicked()
        {
            if (battlePassPanel != null)
            {
                RefreshBattlePassPanel();
                battlePassPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ═══════════════════════════════════════════════════════
        //  ACCOUNT LINKING — Full Conflict Resolution
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Initiates Apple Sign-In and attempts to link the Apple account.
        /// Handles all conflict scenarios with user confirmation dialogs.
        /// </summary>
        private void OnLinkAppleClicked()
        {
            Debug.Log("[MainMenuUI] Link Apple Account clicked");

            // TODO: Replace this block with actual Apple Sign-In plugin call:
            //   AppleAuthManager.LoginWithAppleId(credential => {
            //       string identityToken = Encoding.UTF8.GetString(credential.IdentityToken);
            //       AttemptLinkApple(identityToken);
            //   }, error => { ShowConfirmDialog("Sign-In Failed", ...); });

            // For now, show that the flow is wired but needs the sign-in plugin:
            ShowConfirmDialog("Apple Sign-In Required",
                "The Apple Sign-In plugin is not yet integrated.\n\n" +
                "Once integrated, this will:\n" +
                "1. Open Apple Sign-In\n" +
                "2. Link your Apple ID to this account\n" +
                "3. Let you recover this account on any device",
                "OK", COL_BTN, null);
        }

        /// <summary>
        /// Initiates Google Sign-In and attempts to link the Google account.
        /// Handles all conflict scenarios with user confirmation dialogs.
        /// </summary>
        private void OnLinkGoogleClicked()
        {
            Debug.Log("[MainMenuUI] Link Google Account clicked");

            // TODO: Replace this block with actual Google Sign-In plugin call:
            //   GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task => {
            //       string authCode = task.Result.AuthCode;
            //       AttemptLinkGoogle(authCode);
            //   });

            ShowConfirmDialog("Google Sign-In Required",
                "The Google Sign-In plugin is not yet integrated.\n\n" +
                "Once integrated, this will:\n" +
                "1. Open Google Sign-In\n" +
                "2. Link your Google account to this game\n" +
                "3. Let you recover this account on any device",
                "OK", COL_BTN, null);
        }

        /// <summary>
        /// Attempts to link an Apple identity token. On conflict, shows
        /// a confirmation dialog explaining what will happen.
        ///
        /// Scenarios handled:
        /// 1. Success — Apple ID linked to current account.
        /// 2. LinkedAccountAlreadyClaimed — Apple ID is already linked to a
        ///    DIFFERENT PlayFab account. User can force-link (steals the Apple
        ///    link from the other account) or cancel.
        /// 3. AccountAlreadyLinked — Current PlayFab account already has an
        ///    Apple ID linked. User can overwrite or cancel.
        /// 4. Other errors — shown as-is with a dismiss button.
        /// </summary>
        private void AttemptLinkApple(string identityToken)
        {
            Debug.Log("[MainMenuUI] AttemptLinkApple called (placeholder)");
            ShowConfirmDialog("Link Apple (Placeholder)", "Apple linking is not implemented in this build.", "OK", COL_BTN, null);
        }

        private void AttemptLinkGoogle(string authCode)
        {
            Debug.Log("[MainMenuUI] AttemptLinkGoogle called (placeholder)");
            ShowConfirmDialog("Link Google (Placeholder)", "Google linking is not implemented in this build.", "OK", COL_BTN, null);
        }

    }

}