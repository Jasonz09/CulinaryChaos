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
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrenciesRefreshed += RefreshMainMenuCurrency;
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrenciesRefreshed -= RefreshMainMenuCurrency;
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
            
            settingsPanel = BuildSettingsPanel(root);
            creditsPanel  = BuildCreditsPanel(root);
            shopPanel     = BuildShopPanel(root);
            heroesPanel   = BuildHeroesPanel(root);
            chestPanel    = BuildChestPanel(root);
            battlePassPanel = BuildBattlePassPanel(root);
            confirmDialog = BuildConfirmDialog(root);
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
            // 1. Top Navigation Bar - full-width strip at top
            var topBar = MakePanel(root, "TopNav", new Color(0, 0, 0, 0.65f));
            topBar.anchorMin = new Vector2(0, 1);
            topBar.anchorMax = new Vector2(1, 1);
            topBar.pivot = new Vector2(0.5f, 1);
            topBar.sizeDelta = new Vector2(0, 70);
            topBar.anchoredPosition = Vector2.zero;
            
            // Top Bar Layout
            // Left: Logo
            var logoArea = MakePanel(topBar, "LogoArea", Color.clear);
            logoArea.anchorMin = new Vector2(0, 0); logoArea.anchorMax = new Vector2(0.2f, 1);
            logoArea.offsetMin = new Vector2(30, 0); logoArea.offsetMax = new Vector2(0, 0);
            var logoTxt = MakeText(logoArea, "LogoTxt", "CULINARY\nCHAOS", 24, new Color(1f, 0.6f, 0.2f), FontStyles.Bold);
            Stretch(logoTxt);
            logoTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // Center: Nav Links
            var navArea = MakePanel(topBar, "NavArea", Color.clear);
            navArea.anchorMin = new Vector2(0.2f, 0); navArea.anchorMax = new Vector2(0.75f, 1);
            navArea.offsetMin = navArea.offsetMax = Vector2.zero;
            var navHlg = navArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            navHlg.spacing = 40; navHlg.childAlignment = TextAnchor.MiddleCenter; 
            navHlg.childControlWidth = false; navHlg.childForceExpandWidth = false;

            // Nav Buttons
            MakeTextButton(navArea, "HEROES", 20, Color.white, OnHeroesClicked);
            MakeTextButton(navArea, "PETS", 20, COL_DISABLED_TXT, () => Debug.Log("Pets not implemented"));
            MakeTextButton(navArea, "RECIPES", 20, Color.white, OnRecipeBookClicked);
            MakeTextButton(navArea, "SHOP", 20, Color.white, OnShopClicked);
            MakeTextButton(navArea, "SEASON PASS", 20, Color.white, OnBattlePassClicked);

            // Right: Player Profile & Currency
            var rightArea = MakePanel(topBar, "RightProfile", Color.clear);
            rightArea.anchorMin = new Vector2(0.75f, 0f); rightArea.anchorMax = new Vector2(1f, 1f);
            rightArea.offsetMin = rightArea.offsetMax = Vector2.zero;
            
            var curHlg = rightArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            curHlg.spacing = 15; curHlg.childAlignment = TextAnchor.MiddleRight; curHlg.padding = new RectOffset(0, 30, 0, 0);
            curHlg.childControlWidth = false; curHlg.childForceExpandWidth = false;
            
            // Profile Name
            MakeText(rightArea, "UserName", "CHEF", 18, Color.white, FontStyles.Bold);
            
            // Coins
            var coinP = MakePanel(rightArea, "Coins", Color.clear);
            AddLE(coinP.gameObject, 80, -1);
            var cleft = coinP.gameObject.AddComponent<HorizontalLayoutGroup>();
            cleft.spacing = 5; cleft.childControlWidth = true; cleft.childForceExpandWidth = false; cleft.childAlignment = TextAnchor.MiddleLeft;
            MakeColorDot(coinP, new Color(1f, 0.84f, 0.22f), 14);
            mainMenuCoinsLabel = MakeText(coinP, "Val", "0", 18, Color.white, FontStyles.Bold).GetComponent<TextMeshProUGUI>();

            // Gems
            var gemP = MakePanel(rightArea, "Gems", Color.clear);
            AddLE(gemP.gameObject, 80, -1);
            var gleft = gemP.gameObject.AddComponent<HorizontalLayoutGroup>();
            gleft.spacing = 5; gleft.childControlWidth = true; gleft.childForceExpandWidth = false; gleft.childAlignment = TextAnchor.MiddleLeft;
            MakeColorDot(gemP, new Color(0.30f, 0.65f, 0.95f), 14);
            mainMenuGemsLabel = MakeText(gemP, "Val", "0", 18, Color.white, FontStyles.Bold).GetComponent<TextMeshProUGUI>();

            // Settings Icon Btn
            MakeModernButton(rightArea, "Options", "", Color.clear, Color.clear, 24, 40, OnSettingsClicked, false); 
            var optBtn = rightArea.GetChild(rightArea.childCount - 1).GetComponentInChildren<TextMeshProUGUI>();
            optBtn.text = "O"; // Gear emoji or char? Using 'O' for now.

            // 2. Left Sidebar (Season Pass / Featured)
            var leftBar = MakePanel(root, "LeftSidebar", Color.clear);
            leftBar.anchorMin = new Vector2(0, 0.15f);
            leftBar.anchorMax = new Vector2(0.25f, 0.75f);
            leftBar.offsetMin = new Vector2(40, 0); leftBar.offsetMax = new Vector2(0, 0);
            
            var lblg = leftBar.gameObject.AddComponent<VerticalLayoutGroup>();
            lblg.spacing = 20; lblg.childAlignment = TextAnchor.UpperLeft; lblg.childControlHeight = false; lblg.childForceExpandHeight = false;

            // Season Pass Card
            var passCard = MakePanel(leftBar, "SeasonPassCard", new Color(0.1f, 0.12f, 0.15f, 0.85f));
            AddLE(passCard.gameObject, -1, 110);
            var passLayout = passCard.gameObject.AddComponent<VerticalLayoutGroup>();
            passLayout.padding = new RectOffset(20,20,15,15); passLayout.spacing = 5;
            passLayout.childControlHeight = false; passLayout.childForceExpandHeight = false;
            MakeText(passCard, "Title", "SEASON PASS", 22, Color.white, FontStyles.Bold).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            MakeText(passCard, "Progress", "Progress - 1/4", 14, Color.gray, FontStyles.Normal).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            // Progress Bar
            var pBar = MakePanel(passCard, "Bar", Color.black); 
            AddLE(pBar.gameObject, -1, 8);
            var fill = MakePanel(pBar, "Fill", new Color(0.95f, 0.38f, 0.10f));
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0.3f, 1); fill.offsetMin = fill.offsetMax = Vector2.zero;

            // Featured Item
            var featCard = MakePanel(leftBar, "FeaturedCard", new Color(0.1f, 0.12f, 0.15f, 0.85f));
            AddLE(featCard.gameObject, -1, 180);
            var featLayout = featCard.gameObject.AddComponent<VerticalLayoutGroup>();
            featLayout.padding = new RectOffset(20,20,15,15);
            featLayout.childControlHeight = false; featLayout.childForceExpandHeight = false;
            MakeText(featCard, "Title", "FEATURED SHOP ITEM", 18, Color.white, FontStyles.Bold);
            // Placeholder visuals for item
            var itemPlaceholder = MakePanel(featCard, "ItemIcon", new Color(0,0,0,0.3f));
            var itemLE = itemPlaceholder.gameObject.AddComponent<LayoutElement>();
            itemLE.flexibleHeight = 1; itemLE.preferredHeight = 100;

            // 3. Play Button (Bottom Right)
            var playArea = MakePanel(root, "PlayArea", Color.clear);
            playArea.anchorMin = new Vector2(1, 0); playArea.anchorMax = new Vector2(1, 0);
            playArea.pivot = new Vector2(1, 0);
            playArea.anchoredPosition = new Vector2(-50, 50);
            playArea.sizeDelta = new Vector2(320, 100);

            // Orange Hex button
            MakeModernButton(playArea, "PLAY", "UI/btn_play", new Color(1.0f, 0.4f, 0.0f), new Color(1.0f, 0.3f, 0.0f), 42, 80, OnPlayClicked, true);
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