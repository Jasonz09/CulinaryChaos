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
            BuildButtonGroup(root);
            BuildVersionText(root);

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

        // ─── Title ───
        /// <summary>
        /// Creates a bold, modern CULINARY CHAOS title with layered depth.
        /// </summary>
        private void BuildTitle(RectTransform p)
        {
            // Title backing plate for readability
            var plate = MakePanel(p, "TitlePlate", new Color(0f, 0f, 0f, 0.45f));
            plate.anchorMin = new Vector2(0, 0.84f);
            plate.anchorMax = new Vector2(1, 1f);
            plate.offsetMin = plate.offsetMax = Vector2.zero;
            plate.GetComponent<Image>().raycastTarget = false;

            // Deep shadow layer
            var sh2 = MakeText(p, "TitleShadow2", "CULINARY CHAOS", 72, new Color(0.20f, 0.06f, 0.01f, 0.7f), FontStyles.Bold);
            AnchorTop(sh2, new Vector2(900, 100), new Vector2(5, -28));
            sh2.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Primary shadow
            var sh = MakeText(p, "TitleShadow", "CULINARY CHAOS", 72, new Color(0.55f, 0.15f, 0.02f), FontStyles.Bold);
            AnchorTop(sh, new Vector2(900, 100), new Vector2(3, -26));
            sh.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Main title
            titleRT = MakeText(p, "Title", "CULINARY CHAOS", 72, new Color(1f, 0.92f, 0.50f), FontStyles.Bold);
            AnchorTop(titleRT, new Vector2(900, 100), new Vector2(0, -24));
            titleOrigPos = titleRT.anchoredPosition;
            var titleTMP = titleRT.GetComponent<TextMeshProUGUI>();
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.characterSpacing = 6;

            var outline = titleRT.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.65f, 0.22f, 0.05f);
            outline.effectDistance = new Vector2(3, -3);

            var outline2 = titleRT.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline2.effectColor = new Color(0.40f, 0.10f, 0.02f, 0.5f);
            outline2.effectDistance = new Vector2(2, -2);

            // Decorative lines beside title (pure ASCII, no emoji)
            var decoL = MakeText(p, "DecoL", "< < <", 28, new Color(1f, 0.75f, 0.25f, 0.7f), FontStyles.Bold);
            AnchorTop(decoL, new Vector2(100, 50), new Vector2(-380, -36));

            var decoR = MakeText(p, "DecoR", "> > >", 28, new Color(1f, 0.75f, 0.25f, 0.7f), FontStyles.Bold);
            AnchorTop(decoR, new Vector2(100, 50), new Vector2(380, -36));

            // Tagline
            var tagline = MakeText(p, "Tagline", "-- Ready to cook? --", 18,
                new Color(1, 1, 1, 0.70f), FontStyles.Italic);
            AnchorTop(tagline, new Vector2(400, 30), new Vector2(0, -120));
        }



        // ─── Currency Bar ───
        /// <summary>
        /// Creates a top-right currency display bar.
        /// </summary>
        private void BuildCurrencyBar(RectTransform p)
        {
            var bar = MakePanel(p, "CurrencyBar", new Color(0.12f, 0.08f, 0.04f, 0.70f));
            bar.anchorMin = new Vector2(1f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(1f, 1f);
            bar.sizeDelta = new Vector2(280, 50);
            bar.anchoredPosition = new Vector2(-16, -12);

            var hl = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = true;
            hl.padding = new RectOffset(14, 14, 6, 6);

            // Coin dot + label
            MakeColorDot(bar, new Color(1f, 0.84f, 0.22f), 18);
            mainMenuCoinsLabel = MakeText(bar, "CoinVal", "0", 20, Color.white, FontStyles.Bold).GetComponent<TextMeshProUGUI>();

            // Divider
            MakeText(bar, "Div", "|", 18, new Color(1, 1, 1, 0.3f), FontStyles.Normal);

            // Gem dot + label
            MakeColorDot(bar, new Color(0.30f, 0.65f, 0.95f), 18);
            mainMenuGemsLabel = MakeText(bar, "GemVal", "0", 20, Color.white, FontStyles.Bold).GetComponent<TextMeshProUGUI>();
        }

        // ─── Button Group ───
        /// <summary>
        /// Modernized button group with icons, gradients, and a "juicier" layout.
        /// </summary>
        private void BuildButtonGroup(RectTransform p)
        {
            // Center container for the main menu stack
            var menuPanel = MakePanel(p, "MenuPanel", Color.clear);
            menuPanel.anchorMin = new Vector2(0.30f, 0.15f); // Moved up slightly
            menuPanel.anchorMax = new Vector2(0.70f, 0.65f);
            menuPanel.offsetMin = menuPanel.offsetMax = Vector2.zero;
            menuPanel.GetComponent<Image>().raycastTarget = false;

            var vlg = menuPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24; // Increased spacing
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // 1. START COOKING - The Hero Button
            // Orange-Red Gradient for high energy/action
            Color startTop = new Color(1f, 0.5f, 0.2f);   // Bright Orange
            Color startBot = new Color(0.9f, 0.2f, 0.1f); // Deep Red-Orange
            MakeModernButton(menuPanel, "START COOKING", "UI/btn_play", startTop, startBot, 100, 36, OnPlayClicked);

            // 2. RECIPE BOOK
            // Golden-Yellow Gradient
            Color bookTop = new Color(1f, 0.85f, 0.3f);   // Gold
            Color bookBot = new Color(1f, 0.65f, 0.1f);   // Amber
            MakeModernButton(menuPanel, "RECIPE BOOK", "UI/btn_menu", bookTop, bookBot, 80, 28, OnRecipeBookClicked);

            // 3. STORE & OPTIONS (Side by Side)
            var row = MakePanel(menuPanel, "BottomRow", Color.clear);
            row.GetComponent<Image>().raycastTarget = false;

            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            AddLE(row.gameObject, 70); // Row height

            // Purple/Blue style for Store, Gray/Metallic for Options
            Color storeTop = new Color(0.6f, 0.4f, 1f);
            Color storeBot = new Color(0.4f, 0.2f, 0.9f);
            MakeModernButton(row, "STORE", "UI/btn_shop", storeTop, storeBot, 0, 24, OnShopClicked);

            Color optTop = new Color(0.4f, 0.45f, 0.5f);
            Color optBot = new Color(0.25f, 0.3f, 0.35f);
            MakeModernButton(row, "OPTIONS", "UI/btn_settings", optTop, optBot, 0, 24, OnSettingsClicked);

            // ── Bottom-Right Pills (Heroes, Chests, Pass) ──
            // Kept minimal to not clutter main view
            var extras = MakePanel(p, "Extras", Color.clear);
            extras.anchorMin = new Vector2(0.98f, 0.02f);
            extras.anchorMax = new Vector2(0.98f, 0.02f);
            extras.pivot = new Vector2(1f, 0f);
            extras.sizeDelta = new Vector2(400, 48);
            
            var ehlg = extras.gameObject.AddComponent<HorizontalLayoutGroup>();
            ehlg.spacing = 10;
            ehlg.childAlignment = TextAnchor.MiddleRight;
            ehlg.childForceExpandWidth = false;
            ehlg.childControlWidth = true;
            
            // Dark semi-transparent pills
            Color pillCol = new Color(0,0,0,0.6f);
            MakeModernPill(extras, "HEROES", "UI/icon_gems", pillCol, OnHeroesClicked);
            MakeModernPill(extras, "CHESTS", "UI/icon_credits", pillCol, OnChestsClicked); // using credits icon as placeholder for chest
            MakeModernPill(extras, "PASS", "UI/star_filled", pillCol, OnSeasonPassClicked);
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
        /// Creates the version number text at the bottom.
        /// </summary>
        /// <param name="p">Parent RectTransform for the version text.</param>
        private void BuildVersionText(RectTransform p)
        {
            var v = MakeText(p, "Ver", $"v{Application.version}", 16,
                new Color(1, 1, 1, 0.40f), FontStyles.Normal);
            v.anchorMin = new Vector2(1, 0); v.anchorMax = new Vector2(1, 0);
            v.pivot = new Vector2(1, 0);
            v.anchoredPosition = new Vector2(-14, 12);
            v.sizeDelta = new Vector2(180, 26);
            v.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;
        }

        // ═══════════════════════════════════════════════════════
        //  CALLBACKS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Navigates to the level select screen.
        /// </summary>
        private void OnPlayClicked()
        { if (GameManager.Instance != null) GameManager.Instance.LoadLevelSelect(); }

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
        private void AttemptLinkApple(string identityToken, bool forceLink = false)
        {
            if (linkAppleLabel != null) linkAppleLabel.text = "LINKING...";

            PlayFabManager.Instance?.LinkAppleAccount(identityToken, forceLink,
                onSuccess: () =>
                {
                    Debug.Log("[MainMenuUI] Apple account linked successfully");
                    if (linkAppleLabel != null) linkAppleLabel.text = "APPLE LINKED";
                    ShowConfirmDialog("Account Linked",
                        "Your Apple ID has been linked to this account.\n\n" +
                        "You can now recover your progress by signing in with Apple on any device.",
                        "OK", COL_PLAY, null);
                },
                onError: err =>
                {
                    string errorName = PlayFabManager.GetErrorName(err);

                    if (errorName == PlayFabManager.ERR_LINKED_ACCOUNT_ALREADY_CLAIMED)
                    {
                        // This Apple ID belongs to another account
                        _pendingAppleToken = identityToken;
                        ShowConfirmDialog(
                            "Apple ID Already In Use",
                            "This Apple ID is already linked to a different account.\n\n" +
                            "If you continue, the Apple ID will be moved to YOUR current " +
                            "account. The other account will lose its Apple Sign-In link " +
                            "and may become unrecoverable.\n\n" +
                            "Are you sure you want to steal this Apple ID link?",
                            "LINK ANYWAY", new Color(0.85f, 0.25f, 0.2f),
                            () => AttemptLinkApple(_pendingAppleToken, forceLink: true),
                            "CANCEL", COL_BTN, () =>
                            {
                                if (linkAppleLabel != null) linkAppleLabel.text = "LINK APPLE ACCOUNT";
                            });
                    }
                    else if (errorName == PlayFabManager.ERR_ACCOUNT_ALREADY_LINKED)
                    {
                        // This PlayFab account already has an Apple ID
                        _pendingAppleToken = identityToken;
                        ShowConfirmDialog(
                            "Already Linked",
                            "This game account already has an Apple ID linked.\n\n" +
                            "Linking a new Apple ID will replace the existing one. " +
                            "You will no longer be able to recover this account " +
                            "with the old Apple ID.\n\n" +
                            "Do you want to replace it?",
                            "REPLACE", new Color(0.85f, 0.55f, 0.1f),
                            () => AttemptLinkApple(_pendingAppleToken, forceLink: true),
                            "KEEP CURRENT", COL_BTN, () =>
                            {
                                if (linkAppleLabel != null) linkAppleLabel.text = "APPLE LINKED";
                            });
                    }
                    else
                    {
                        // Unknown/network error
                        if (linkAppleLabel != null) linkAppleLabel.text = "LINK APPLE ACCOUNT";
                        ShowConfirmDialog("Link Failed",
                            $"Could not link Apple account.\n\n{err}",
                            "OK", COL_BTN, null);
                    }
                });
        }

        /// <summary>
        /// Attempts to link a Google auth code. On conflict, shows
        /// a confirmation dialog explaining what will happen.
        ///
        /// Scenarios handled:
        /// 1. Success — Google account linked to current account.
        /// 2. LinkedAccountAlreadyClaimed — Google account is already linked to a
        ///    DIFFERENT PlayFab account. User can force-link or cancel.
        /// 3. AccountAlreadyLinked — Current PlayFab account already has a
        ///    Google account linked. User can overwrite or cancel.
        /// 4. Other errors — shown as-is with a dismiss button.
        /// </summary>
        private void AttemptLinkGoogle(string serverAuthCode, bool forceLink = false)
        {
            if (linkGoogleLabel != null) linkGoogleLabel.text = "LINKING...";

            PlayFabManager.Instance?.LinkGoogleAccount(serverAuthCode, forceLink,
                onSuccess: () =>
                {
                    Debug.Log("[MainMenuUI] Google account linked successfully");
                    if (linkGoogleLabel != null) linkGoogleLabel.text = "GOOGLE LINKED";
                    ShowConfirmDialog("Account Linked",
                        "Your Google account has been linked to this account.\n\n" +
                        "You can now recover your progress by signing in with Google on any device.",
                        "OK", COL_PLAY, null);
                },
                onError: err =>
                {
                    string errorName = PlayFabManager.GetErrorName(err);

                    if (errorName == PlayFabManager.ERR_LINKED_ACCOUNT_ALREADY_CLAIMED)
                    {
                        // This Google account belongs to another account
                        _pendingGoogleAuthCode = serverAuthCode;
                        ShowConfirmDialog(
                            "Google Account Already In Use",
                            "This Google account is already linked to a different account.\n\n" +
                            "If you continue, the Google link will be moved to YOUR current " +
                            "account. The other account will lose its Google Sign-In link " +
                            "and may become unrecoverable.\n\n" +
                            "Are you sure you want to steal this Google link?",
                            "LINK ANYWAY", new Color(0.85f, 0.25f, 0.2f),
                            () => AttemptLinkGoogle(_pendingGoogleAuthCode, forceLink: true),
                            "CANCEL", COL_BTN, () =>
                            {
                                if (linkGoogleLabel != null) linkGoogleLabel.text = "LINK GOOGLE ACCOUNT";
                            });
                    }
                    else if (errorName == PlayFabManager.ERR_ACCOUNT_ALREADY_LINKED)
                    {
                        // This PlayFab account already has a Google account
                        _pendingGoogleAuthCode = serverAuthCode;
                        ShowConfirmDialog(
                            "Already Linked",
                            "This game account already has a Google account linked.\n\n" +
                            "Linking a new Google account will replace the existing one. " +
                            "You will no longer be able to recover this account " +
                            "with the old Google account.\n\n" +
                            "Do you want to replace it?",
                            "REPLACE", new Color(0.85f, 0.55f, 0.1f),
                            () => AttemptLinkGoogle(_pendingGoogleAuthCode, forceLink: true),
                            "KEEP CURRENT", COL_BTN, () =>
                            {
                                if (linkGoogleLabel != null) linkGoogleLabel.text = "GOOGLE LINKED";
                            });
                    }
                    else
                    {
                        // Unknown/network error
                        if (linkGoogleLabel != null) linkGoogleLabel.text = "LINK GOOGLE ACCOUNT";
                        ShowConfirmDialog("Link Failed",
                            $"Could not link Google account.\n\n{err}",
                            "OK", COL_BTN, null);
                    }
                });
        }

        // ═══════════════════════════════════════════════════════
        //  REFRESH
        // ═══════════════════════════════════════════════════════

        // RefreshShopPanel moved to MainMenuUI.Shop.cs

        // BuildHeroesPanel and RefreshHeroesPanel moved to MainMenuUI.Heroes.cs

        // ═══════════════════════════════════════════════════════
        //  ANIMATION
        // ═══════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════
        //  FACTORY HELPERS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Creates a small colored circle — used for currency icons instead of emoji.
        /// </summary>
        private void MakeColorDot(RectTransform parent, Color color, int size)
        {
            var go = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = size;
            le.preferredHeight = size;
            le.flexibleWidth = 0;
        }

        /// <summary>
        /// Creates an EventSystem if none exists in the scene.
        /// </summary>
        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var g = new GameObject("EventSystem");
                g.AddComponent<EventSystem>();
                g.AddComponent<StandaloneInputModule>();
            }
        }

        /// <summary>
        /// Creates a UI panel with an Image component.
        /// </summary>
        /// <param name="parent">Parent RectTransform to attach the panel to.</param>
        /// <param name="name">Name of the panel GameObject.</param>
        /// <param name="c">Background color of the panel.</param>
        /// <returns>The RectTransform of the created panel.</returns>
        private RectTransform MakePanel(RectTransform parent, string name, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = c;
            return go.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Creates a TextMeshProUGUI text element.
        /// </summary>
        /// <param name="parent">Parent RectTransform to attach the text to.</param>
        /// <param name="name">Name of the text GameObject.</param>
        /// <param name="text">Text content to display.</param>
        /// <param name="size">Font size.</param>
        /// <param name="c">Text color.</param>
        /// <param name="style">Font style (bold, italic, etc.).</param>
        /// <returns>The RectTransform of the created text element.</returns>
        private RectTransform MakeText(RectTransform parent, string name, string text,
            int size, Color c, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.color = c;
            t.fontStyle = style; t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = true;
            return go.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Adds a text element with a layout element for vertical layouts.
        /// </summary>
        /// <param name="parent">Parent RectTransform to attach the text to.</param>
        /// <param name="name">Name of the text GameObject.</param>
        /// <param name="text">Text content to display.</param>
        /// <param name="size">Font size.</param>
        /// <param name="c">Text color.</param>
        /// <param name="style">Font style (bold, italic, etc.).</param>
        /// <param name="prefH">Preferred height for the layout element.</param>
        private void AddLayoutText(RectTransform parent, string name, string text,
            int size, Color c, FontStyles style, float prefH)
        {
            var rt = MakeText(parent, name, text, size, c, style);
            AddLE(rt.gameObject, prefH);
        }

        /// <summary>
        /// Adds a LayoutElement with a preferred height to a GameObject.
        /// </summary>
        /// <param name="go">The GameObject to add the LayoutElement to.</param>
        /// <param name="prefH">Preferred height for the layout element.</param>
        private static void AddLE(GameObject go, float prefH)
        {
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = prefH;
            le.flexibleHeight  = 0;
        }

        private static void AddLE(GameObject go, float prefH, float prefW)
        {
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = prefH;
            le.flexibleHeight  = 0;
            if (prefW > 0) le.preferredWidth = prefW;
        }

        // ─── Currency Icon Helpers ───

        private RectTransform MakeCurrencyIcon(RectTransform parent, string currencyType, int size = 22)
        {
            Color iconColor = currencyType switch
            {
                "coins"  => new Color(1f, 0.84f, 0.22f),
                "gems"   => new Color(0.20f, 0.55f, 0.85f),
                "tokens" => new Color(0.30f, 0.75f, 0.30f),
                _        => new Color(0.70f, 0.70f, 0.70f),
            };

            var iconGO = new GameObject($"Icon_{currencyType}", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(parent, false);
            var img = iconGO.GetComponent<Image>();
            img.color = iconColor;
            img.raycastTarget = false;
            var rt = iconGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);

            var le = iconGO.AddComponent<LayoutElement>();
            le.preferredWidth = size;
            le.preferredHeight = size;
            le.flexibleWidth = 0;
            return rt;
        }

        private TextMeshProUGUI MakeCurrencyLabel(RectTransform parent, string name,
            string currencyType, string text, int fontSize, Color textColor, float prefH)
        {
            var row = MakePanel(parent, name, Color.clear);
            AddLE(row.gameObject, prefH);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 6;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;
            hl.childControlWidth = true;
            hl.childControlHeight = true;

            MakeCurrencyIcon(row, currencyType);

            var txtRT = MakeText(row, $"{name}Txt", text, fontSize, textColor, FontStyles.Bold);
            var txtLE = txtRT.gameObject.AddComponent<LayoutElement>();
            txtLE.flexibleWidth = 1;
            txtLE.preferredHeight = prefH;
            return txtRT.GetComponent<TextMeshProUGUI>();
        }

        // ─── Purchase Feedback Animation ───

        private void ShowPurchaseFeedback(RectTransform parent, string text, Color color)
        {
            StartCoroutine(PurchaseFeedbackCoroutine(parent, text, color));
        }

        private IEnumerator PurchaseFeedbackCoroutine(RectTransform parent, string text, Color color)
        {
            var go = new GameObject("Feedback", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 50);
            Vector2 startPos = Vector2.zero;
            rt.anchoredPosition = startPos;

            float duration = 1f;
            float timer = 0f;
            while (timer < duration)
            {
                float t = timer / duration;
                rt.anchoredPosition = startPos + new Vector2(0, t * 100f);
                tmp.color = new Color(color.r, color.g, color.b, 1f - t);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            Destroy(go);
        }

        /// <summary>
        /// Simple button: Image = face color, shadow strip child behind label.
        /// No child Images blocking raycasts. No color tinting.
        /// </summary>
        /// <param name="parent">Parent RectTransform.</param>
        /// <param name="label">Button label text.</param>
        /// <param name="face">Face color.</param>
        /// <param name="shadow">Shadow strip color.</param>
        /// <param name="txt">Text color.</param>
        /// <param name="size">Font size.</param>
        /// <param name="h">Button height.</param>
        /// <param name="onClick">Click callback.</param>
        private void MakeChunkyButton(RectTransform parent, string label, Color face, Color shadow,
            Color txt, int size, int h, UnityEngine.Events.UnityAction onClick)
        {
            int borderH = Mathf.Max(5, h / 10);

            // Button GO — Image IS the face color
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            AddLE(go, h);

            var img = go.GetComponent<Image>();
            img.color = face;  // green, yellow, red etc — displayed directly

            // Bottom shadow strip (non-interactive, behind label)
            var shGO = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shGO.transform.SetParent(go.transform, false);
            var shImg = shGO.GetComponent<Image>();
            shImg.color = shadow;
            shImg.raycastTarget = false;  // clicks pass through to button
            var shRT = shGO.GetComponent<RectTransform>();
            shRT.anchorMin = new Vector2(0, 0);
            shRT.anchorMax = new Vector2(1, 0);
            shRT.pivot = new Vector2(0.5f, 0);
            shRT.offsetMin = Vector2.zero;
            shRT.offsetMax = Vector2.zero;
            shRT.sizeDelta = new Vector2(0, borderH);

            // Button setup — NO color tinting
            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            // Label
            var lbl = new GameObject("Lbl", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(go.transform, false);
            var tmp = lbl.GetComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = size; tmp.color = txt;
            tmp.fontStyle = FontStyles.Bold; tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;  // clicks pass through to button
            var lblRT = lbl.GetComponent<RectTransform>();
            Stretch(lblRT);
            lblRT.offsetMin = new Vector2(8, borderH);
            lblRT.offsetMax = new Vector2(-8, -2);

            go.AddComponent<ButtonBounceEffect>();
        }

        private TextMeshProUGUI MakeChunkyButtonWithLabel(RectTransform parent, string label, Color face, Color shadow,
            Color txt, int size, int h, UnityEngine.Events.UnityAction onClick)
        {
            MakeChunkyButton(parent, label, face, shadow, txt, size, h, onClick);
            // Return label TMP from the button we just created (last child of parent)
            var btnGO = parent.GetChild(parent.childCount - 1);
            return btnGO.Find("Lbl")?.GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Disabled button with badge. Same flat structure — no child Face panel.
        /// </summary>
        /// <param name="parent">Parent RectTransform.</param>
        /// <param name="label">Button label text.</param>
        /// <param name="badge">Badge text.</param>
        /// <param name="face">Face color.</param>
        /// <param name="shadow">Shadow strip color.</param>
        /// <param name="txt">Text color.</param>
        /// <param name="size">Font size.</param>
        /// <param name="h">Button height.</param>
        private void MakeDisabledBadgeButton(RectTransform parent, string label, string badge,
            Color face, Color shadow, Color txt, int size, int h)
        {
            int borderH = Mathf.Max(4, h / 14);

            var go = new GameObject($"Btn_{label}_Off", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            AddLE(go, h);

            var img = go.GetComponent<Image>();
            img.color = face;  // grey for disabled

            // Bottom shadow strip
            var shGO = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shGO.transform.SetParent(go.transform, false);
            var shImg = shGO.GetComponent<Image>();
            shImg.color = shadow;
            shImg.raycastTarget = false;
            var shRT = shGO.GetComponent<RectTransform>();
            shRT.anchorMin = new Vector2(0, 0);
            shRT.anchorMax = new Vector2(1, 0);
            shRT.pivot = new Vector2(0.5f, 0);
            shRT.offsetMin = Vector2.zero;
            shRT.offsetMax = Vector2.zero;
            shRT.sizeDelta = new Vector2(0, borderH);

            // Disabled, no tinting
            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.interactable = false;
            btn.targetGraphic = img;

            // Label
            var lbl = new GameObject("Lbl", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(go.transform, false);
            var tmp = lbl.GetComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = size; tmp.color = txt;
            tmp.fontStyle = FontStyles.Bold; tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            var lblRT = lbl.GetComponent<RectTransform>();
            Stretch(lblRT);
            lblRT.offsetMin = new Vector2(8, borderH);
            lblRT.offsetMax = new Vector2(-8, -2);

            // Badge (top-right)
            var badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(go.transform, false);
            var bImg = badgeGO.GetComponent<Image>();
            bImg.color = COL_BADGE;
            bImg.raycastTarget = false;
            var brt = badgeGO.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(1, 1);
            brt.pivot = new Vector2(1, 1);
            brt.anchoredPosition = new Vector2(10, 10);
            brt.sizeDelta = new Vector2(160, 26);

            var btx = new GameObject("BTx", typeof(RectTransform), typeof(TextMeshProUGUI));
            btx.transform.SetParent(badgeGO.transform, false);
            var bt = btx.GetComponent<TextMeshProUGUI>();
            bt.text = badge; bt.fontSize = 13; bt.color = Color.white;
            bt.fontStyle = FontStyles.Bold; bt.alignment = TextAlignmentOptions.Center;
            bt.raycastTarget = false;
            Stretch(btx.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Creates a styled slider control.
        /// </summary>
        /// <param name="parent">Parent RectTransform to attach the slider to.</param>
        /// <param name="name">Name of the slider GameObject.</param>
        /// <param name="val">Initial value of the slider (0-1).</param>
        /// <returns>The created Slider component.</returns>
        private Slider MakeSlider(RectTransform parent, string name, float val)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            AddLE(go, 42);

            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            bg.GetComponent<Image>().color = new Color(0.68f, 0.62f, 0.52f);
            var bgrt = bg.GetComponent<RectTransform>(); Stretch(bgrt);
            bgrt.offsetMin = new Vector2(8, 15); bgrt.offsetMax = new Vector2(-8, -15);

            var fa = new GameObject("FA", typeof(RectTransform));
            fa.transform.SetParent(go.transform, false);
            var fart = fa.GetComponent<RectTransform>(); Stretch(fart);
            fart.offsetMin = new Vector2(8, 15); fart.offsetMax = new Vector2(-8, -15);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fa.transform, false);
            fill.GetComponent<Image>().color = COL_PLAY;
            var frt = fill.GetComponent<RectTransform>(); Stretch(frt);

            var ha = new GameObject("HA", typeof(RectTransform));
            ha.transform.SetParent(go.transform, false);
            var hart = ha.GetComponent<RectTransform>(); Stretch(hart);
            hart.offsetMin = new Vector2(8, 0); hart.offsetMax = new Vector2(-8, 0);

            var handle = new GameObject("H", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(ha.transform, false);
            handle.GetComponent<Image>().color = Color.white;
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(26, 26);

            var s = go.GetComponent<Slider>();
            s.fillRect = frt; s.handleRect = handle.GetComponent<RectTransform>();
            s.minValue = 0; s.maxValue = 1; s.value = val;
            return s;
        }

        // ─── Modern Button System ───

        /// <summary>
        /// Returns (or generates) a high-quality 9-slice rounded-rectangle sprite
        /// with crisp anti-aliased edges. Smaller radius = sleek modern look.
        /// </summary>
        // ─── Sprite Generators ───

        private Dictionary<string, Sprite> _gradientCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Generates a rounded rectangle sprite with a vertical gradient fill.
        /// </summary>
        private Sprite GeneratGradientSprite(int w, int h, int r, Color top, Color bottom)
        {
            string key = $"{w}x{h}_{r}_{top}_{bottom}";
            if (_gradientCache.ContainsKey(key) && _gradientCache[key] != null) return _gradientCache[key];

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float vf = (float)y / (h - 1);
                Color rowColor = Color.Lerp(bottom, top, vf); // Gradient from bottom to top

                for (int x = 0; x < w; x++)
                {
                    // SDF Alpha for rounded corners
                    float dist = SdfRoundedRect(x, y, w, h, r);
                    float alpha = Mathf.Clamp01(1f - dist);
                    
                    Color final = rowColor;
                    final.a *= alpha;
                    pixels[y * w + x] = final;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f, 0.5f));
            _gradientCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Simple white rounded rect for shadows/tinting.
        /// </summary>
        private Sprite GetRoundedRectSprite(int radius)
        {
             // Radius 20, White-White gradient
             return GeneratGradientSprite(128, 128, radius, Color.white, Color.white);
        }

        /// <summary>Signed-distance field for a rounded rectangle.</summary>
        private static float SdfRoundedRect(int px, int py, int w, int h, int r)
        {
            float x = px - w * 0.5f;
            float y = py - h * 0.5f;
            float hw = w * 0.5f - r;
            float hh = h * 0.5f - r;
            float dx = Mathf.Max(Mathf.Abs(x) - hw, 0);
            float dy = Mathf.Max(Mathf.Abs(y) - hh, 0);
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        /// <summary>
        /// Creates a high-quality modern gradient button with icon.
        /// </summary>
        private void MakeModernButton(RectTransform parent, string label, string iconPath, 
            Color topColor, Color botColor, float height, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var btnObj = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(CanvasGroup), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            
            // Layout Element if height is specified
            if (height > 0)
            {
                var le = btnObj.AddComponent<LayoutElement>();
                le.preferredHeight = height;
                le.minHeight = height;
            }

            // 1. Shadow/Depth Layer (The "3D" part)
            var shadowImg = CreateImage(btnObj, "Shadow");
            shadowImg.sprite = GetRoundedRectSprite(20);
            shadowImg.color = new Color(0, 0, 0, 0.3f);
            shadowImg.type = Image.Type.Simple; // Generated texture is non-sliced unless updated
            // Adjust scaling to standard
            var shadowRT = shadowImg.rectTransform;
            shadowRT.anchorMin = Vector2.zero; shadowRT.anchorMax = Vector2.one;
            shadowRT.offsetMin = new Vector2(2, -4); // Push shadow down
            shadowRT.offsetMax = new Vector2(-2, -4);

            // 2. Main Gradient Face
            var faceImg = CreateImage(btnObj, "Face");
            faceImg.sprite = GeneratGradientSprite(256, 128, 20, topColor, botColor); // Wide texture for scaling
            faceImg.type = Image.Type.Simple; 
            
            var faceRT = faceImg.rectTransform;
            faceRT.anchorMin = Vector2.zero; faceRT.anchorMax = Vector2.one;
            faceRT.offsetMin = new Vector2(0, 0); 
            faceRT.offsetMax = new Vector2(0, 0);

            // 3. Highlight/Glaze (Top rim) - Optional polish
            var glazeObj = CreateImage(btnObj, "Glaze");
            glazeObj.sprite = GetRoundedRectSprite(20); // Reuse white sprite
            glazeObj.color = new Color(1, 1, 1, 0.15f); // Subtle white tint
            glazeObj.type = Image.Type.Simple;
            var glazeRT = glazeObj.rectTransform;
            glazeRT.anchorMin = new Vector2(0, 0.5f); glazeRT.anchorMax = Vector2.one;
            glazeRT.offsetMin = new Vector2(4, 0); glazeRT.offsetMax = new Vector2(-4, -2);
            glazeObj.raycastTarget = false;

            // 4. Content Container (Horizontal Layout for Icon + Text)
            var contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(btnObj.transform, false);
            Stretch(contentObj.GetComponent<RectTransform>());
            var hlg = contentObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(24, 24, 10, 10);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; 
            hlg.childForceExpandWidth = false;

            // 5. Icon
            if (!string.IsNullOrEmpty(iconPath))
            {
                Sprite iconSprite = Resources.Load<Sprite>(iconPath);
                if (iconSprite != null)
                {
                    var iconImg = CreateImage(contentObj, "Icon");
                    iconImg.sprite = iconSprite;
                    iconImg.preserveAspect = true;
                    var le = iconImg.gameObject.AddComponent<LayoutElement>();
                    le.preferredWidth = height > 0 ? height * 0.6f : 32;
                    le.preferredHeight = height > 0 ? height * 0.6f : 32;
                }
            }

            // 6. Text
            var lblObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(contentObj.transform, false);
            var tmp = lblObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Left; // Align left next to icon
            tmp.characterSpacing = 2f; 
            tmp.enableWordWrapping = false;
            
            // Button Logic
            var btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = faceImg;
            btn.transition = Selectable.Transition.None; // We use custom spring
            if (onClick != null) btn.onClick.AddListener(onClick);

            btnObj.AddComponent<ButtonSpringEffect>();
        }

        private void MakeModernPill(RectTransform parent, string label, string iconPath, Color color, UnityEngine.Events.UnityAction action)
        {
            // Similar to button but smaller/simpler
            var go = new GameObject($"Pill_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            
            var img = go.GetComponent<Image>();
            img.sprite = GetRoundedRectSprite(16);
            img.type = Image.Type.Simple;
            img.color = color;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            if (action != null) btn.onClick.AddListener(action);

            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; 

            // Icon
            if (!string.IsNullOrEmpty(iconPath))
            {
                Sprite ico = Resources.Load<Sprite>(iconPath);
                if (ico != null)
                {
                    var i = CreateImage(go, "Icon");
                    i.sprite = ico;
                    i.preserveAspect = true;
                    var le = i.gameObject.AddComponent<LayoutElement>();
                    le.preferredWidth = 20; le.preferredHeight = 20;
                }
            }

            // Text
            var t = MakeText(go.GetComponent<RectTransform>(), "Txt", label, 14, Color.white, FontStyles.Bold);
            t.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
        }

        private Image CreateImage(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            return go.GetComponent<Image>();
        }

        /* 
           Preserving existing positioning helpers below 
        */

        // ─── Positioning helpers ───
        /// <summary>
        /// Stretches a RectTransform to fill its parent.
        /// </summary>
        /// <param name="rt">The RectTransform to stretch.</param>
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Anchors a RectTransform to the top-center with given size and position.
        /// </summary>
        /// <param name="rt">The RectTransform to anchor.</param>
        /// <param name="size">Width and height of the element.</param>
        /// <param name="pos">Anchored position offset from the top-center.</param>
        private static void AnchorTop(RectTransform rt, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = size; rt.anchoredPosition = pos;
        }

        /// <summary>
        /// Centers a RectTransform with the given width and height.
        /// </summary>
        /// <param name="rt">The RectTransform to center.</param>
        /// <param name="w">Width of the element.</param>
        /// <param name="h">Height of the element.</param>
        private static void CenterBox(RectTransform rt, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
        }
    }
}
