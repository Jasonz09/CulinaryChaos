using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using IOChef.Gameplay;

namespace IOChef.UI
{
    /// <summary>
    /// Overcooked-style gameplay HUD.
    /// Bottom bar: blue coin/score on left, green timer on right.
    /// Top-left: order cards. Top-right: pause button.
    /// Fully programmatic – no Inspector refs needed.
    /// </summary>
    public class HUDCanvas : MonoBehaviour
    {
        // ─── Overcooked palette ───

        /// <summary>
        /// Score bar fill color.
        /// </summary>
        private static readonly Color COL_SCORE_BAR  = new(0.15f, 0.40f, 0.75f);

        /// <summary>
        /// Score bar dark edge color.
        /// </summary>
        private static readonly Color COL_SCORE_DARK = new(0.10f, 0.28f, 0.55f);

        /// <summary>
        /// Timer bar fill color at normal time.
        /// </summary>
        private static readonly Color COL_TIMER_BAR  = new(0.18f, 0.62f, 0.25f);

        /// <summary>
        /// Timer bar dark edge color.
        /// </summary>
        private static readonly Color COL_TIMER_DARK = new(0.12f, 0.44f, 0.16f);

        /// <summary>
        /// Timer bar fill color when time is low.
        /// </summary>
        private static readonly Color COL_TIMER_LOW  = new(0.90f, 0.70f, 0.12f);

        /// <summary>
        /// Timer bar fill color when time is critical.
        /// </summary>
        private static readonly Color COL_TIMER_CRIT = new(0.90f, 0.20f, 0.15f);

        /// <summary>
        /// Background color for the bottom bar strip.
        /// </summary>
        private static readonly Color COL_BAR_BG     = new(0.10f, 0.08f, 0.06f, 0.90f);

        /// <summary>
        /// Gold coin icon color.
        /// </summary>
        private static readonly Color COL_COIN       = new(1f, 0.84f, 0.20f);

        /// <summary>
        /// Shared white color constant.
        /// </summary>
        private static readonly Color COL_WHITE      = new(1f, 1f, 1f);

        /// <summary>
        /// Order card background color.
        /// </summary>
        private static readonly Color COL_ORDER_BG   = new(0.95f, 0.92f, 0.85f, 0.95f);

        /// <summary>
        /// Order card edge accent color.
        /// </summary>
        private static readonly Color COL_ORDER_EDGE = new(0.72f, 0.42f, 0.18f);

        /// <summary>
        /// Menu button background color.
        /// </summary>
        private static readonly Color COL_MENU_BG     = new(0.80f, 0.22f, 0.18f);

        /// <summary>
        /// Full-screen pause overlay color.
        /// </summary>
        private static readonly Color COL_OVERLAY     = new(0f, 0f, 0f, 0.65f);

        /// <summary>
        /// Pause menu panel background color.
        /// </summary>
        private static readonly Color COL_MENU_PANEL  = new(0.95f, 0.92f, 0.85f, 0.97f);

        /// <summary>
        /// Pause menu button face color.
        /// </summary>
        private static readonly Color COL_MENU_BTN    = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Pause menu button shadow color.
        /// </summary>
        private static readonly Color COL_MENU_BTN_SH = new(0.82f, 0.62f, 0.08f);

        /// <summary>
        /// Pause menu button text color.
        /// </summary>
        private static readonly Color COL_MENU_TXT    = new(0.22f, 0.13f, 0.04f);

        /// <summary>
        /// Cancel button face color.
        /// </summary>
        private static readonly Color COL_CANCEL      = new(0.50f, 0.48f, 0.45f);

        /// <summary>
        /// Cancel button shadow color.
        /// </summary>
        private static readonly Color COL_CANCEL_SH   = new(0.35f, 0.33f, 0.30f);

        /// <summary>
        /// Retry button face color.
        /// </summary>
        private static readonly Color COL_RETRY       = new(0.30f, 0.75f, 0.30f);

        /// <summary>
        /// Retry button shadow color.
        /// </summary>
        private static readonly Color COL_RETRY_SH    = new(0.15f, 0.50f, 0.15f);

        // Runtime refs

        /// <summary>
        /// Text element displaying the remaining time.
        /// </summary>
        private TextMeshProUGUI timerText;

        /// <summary>
        /// Text element displaying the current score.
        /// </summary>
        private TextMeshProUGUI scoreText;

        /// <summary>
        /// Text element displaying the combo streak.
        /// </summary>
        private TextMeshProUGUI comboText;

        /// <summary>
        /// Image used as the timer bar foreground fill.
        /// </summary>
        private Image timerFillFg;

        /// <summary>
        /// Image used as the timer bar background.
        /// </summary>
        private Image timerBarBg;

        /// <summary>
        /// Image used as the score bar background.
        /// </summary>
        private Image scoreBarBg;

        /// <summary>
        /// Text elements for displaying active order cards.
        /// </summary>
        private TextMeshProUGUI[] orderTexts = new TextMeshProUGUI[4];

        /// <summary>
        /// Parent container for order card elements.
        /// </summary>
        private RectTransform orderWrap;

        /// <summary>
        /// Root game object of the pause menu overlay.
        /// </summary>
        private GameObject menuOverlay;

        /// <summary>
        /// Reference to the GameTimer component.
        /// </summary>
        private GameTimer gameTimer;

        /// <summary>
        /// Reference to the ScoreCalculator component.
        /// </summary>
        private ScoreCalculator scoreCalculator;

        /// <summary>
        /// Reference to the OrderQueue component.
        /// </summary>
        private OrderQueue orderQueue;

        /// <summary>
        /// Canvas used to render the HUD.
        /// </summary>
        private Canvas hudCanvas;

        /// <summary>Creates the HUD canvas and builds all UI elements.</summary>
        private void Awake()
        {
            Debug.Log("[HUDCanvas] v4 Awake - root-level overlay");
            try
            {
                // Destroy any existing Canvas components on this GO
                // (nesting a child Canvas inside a disabled Canvas prevents rendering)
                var existingCanvas = GetComponent<Canvas>();
                if (existingCanvas != null) DestroyImmediate(existingCanvas);
                var existingScaler = GetComponent<CanvasScaler>();
                if (existingScaler != null) DestroyImmediate(existingScaler);
                var existingRaycaster = GetComponent<GraphicRaycaster>();
                if (existingRaycaster != null) DestroyImmediate(existingRaycaster);

                gameTimer = FindAnyObjectByType<GameTimer>();
                scoreCalculator = FindAnyObjectByType<ScoreCalculator>();
                orderQueue = FindAnyObjectByType<OrderQueue>();
                EnsureEventSystem();
                BuildUI();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HUDCanvas] {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>Subscribes to timer, score, and combo update events.</summary>
        private void OnEnable()
        {
            if (gameTimer != null) gameTimer.OnTimerUpdated += UpdateTimer;
            if (scoreCalculator != null)
            {
                scoreCalculator.OnScoreChanged += UpdateScore;
                scoreCalculator.OnComboChanged += UpdateCombo;
            }
        }

        /// <summary>Unsubscribes from timer, score, and combo update events.</summary>
        private void OnDisable()
        {
            if (gameTimer != null) gameTimer.OnTimerUpdated -= UpdateTimer;
            if (scoreCalculator != null)
            {
                scoreCalculator.OnScoreChanged -= UpdateScore;
                scoreCalculator.OnComboChanged -= UpdateCombo;
            }
        }

        /// <summary>Finds gameplay references and triggers initial order refresh.</summary>
        private void Start()
        {
            UpdateScore(scoreCalculator != null ? scoreCalculator.CurrentScore : 0);
            UpdateCombo(0);
        }

        // ═══════════════════════════════════════════════════════
        //  BUILD UI
        // ═══════════════════════════════════════════════════════

        /// <summary>Constructs the full HUD layout under the root canvas.</summary>
        private void BuildUI()
        {
            // Create as root-level GO so no parent Canvas can interfere
            var cgo = new GameObject("HUD_OC");
            hudCanvas = cgo.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 20;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();
            var root = cgo.GetComponent<RectTransform>();

            BuildBottomBar(root);
            BuildMenuButton(root);
            BuildOrderCards(root);
            BuildControlsHint(root);
        }

        // ───────────────────────────────────────────
        //  BOTTOM BAR (Overcooked style)
        //  Left: [coin icon] score   |   Right: timer [hourglass icon]
        // ───────────────────────────────────────────

        /// <summary>Creates the bottom bar with timer, score, and combo displays.</summary>
        /// <param name="root">The root RectTransform to parent the bottom bar under.</param>
        private void BuildBottomBar(RectTransform root)
        {
            // Full-width dark strip at bottom
            var strip = MakePanel(root, "BotStrip", COL_BAR_BG);
            SetAnchors(strip, 0, 0, 1, 0);
            strip.pivot = new Vector2(0.5f, 0);
            strip.sizeDelta = new Vector2(0, 100);

            // ── SCORE BAR (left) ──
            scoreBarBg = MakePanel(strip, "ScoreBar", COL_SCORE_BAR).GetComponent<Image>();
            var srt = scoreBarBg.GetComponent<RectTransform>();
            SetAnchors(srt, 0, 0, 0, 1);
            srt.pivot = new Vector2(0, 0.5f);
            srt.anchoredPosition = new Vector2(16, 0);
            srt.sizeDelta = new Vector2(280, -16);

            // Blue bar edge (bottom darker strip)
            var sEdge = MakePanel(srt, "ScoreEdge", COL_SCORE_DARK);
            SetAnchors(sEdge, 0, 0, 1, 0);
            sEdge.sizeDelta = new Vector2(0, 8);

            // Rounded feel: just use the bar with a coin + text inside
            // Coin icon (gold circle using UI Image)
            var coinRT = MakePanel(srt, "Coin", COL_COIN);
            SetAnchors(coinRT, 0, 0.5f, 0, 0.5f);
            coinRT.anchoredPosition = new Vector2(30, 0);
            coinRT.sizeDelta = new Vector2(36, 36);
            // Dollar sign on coin
            var coinTxt = MakeTMP(coinRT, "CoinTxt", "$", 20, new Color(0.75f, 0.58f, 0.08f), FontStyles.Bold);
            Stretch(coinTxt.GetComponent<RectTransform>());

            // Score text
            scoreText = MakeTMP(srt, "ScoreTxt", "0", 36, COL_WHITE, FontStyles.Bold);
            var scRT = scoreText.GetComponent<RectTransform>();
            SetAnchors(scRT, 0, 0, 1, 1);
            scRT.offsetMin = new Vector2(60, 4);
            scRT.offsetMax = new Vector2(-8, -4);
            scoreText.alignment = TextAlignmentOptions.Left;

            // Combo text (overlays score area, shows when combo > 1)
            comboText = MakeTMP(srt, "ComboTxt", "", 18, COL_COIN, FontStyles.Bold);
            var cmRT = comboText.GetComponent<RectTransform>();
            SetAnchors(cmRT, 1, 0, 1, 1);
            cmRT.pivot = new Vector2(1, 0.5f);
            cmRT.anchoredPosition = new Vector2(-8, 0);
            cmRT.sizeDelta = new Vector2(100, 0);
            comboText.alignment = TextAlignmentOptions.Right;
            comboText.gameObject.SetActive(false);

            // ── TIMER BAR (right) ──
            timerBarBg = MakePanel(strip, "TimerBar", COL_TIMER_BAR).GetComponent<Image>();
            var trt = timerBarBg.GetComponent<RectTransform>();
            SetAnchors(trt, 1, 0, 1, 1);
            trt.pivot = new Vector2(1, 0.5f);
            trt.anchoredPosition = new Vector2(-16, 0);
            trt.sizeDelta = new Vector2(320, -16);

            // Green bar edge
            var tEdge = MakePanel(trt, "TimerEdge", COL_TIMER_DARK);
            SetAnchors(tEdge, 0, 0, 1, 0);
            tEdge.sizeDelta = new Vector2(0, 8);

            // Timer fill bar (shrinks from right as time runs out)
            var fillBg = MakePanel(trt, "FillBg", new Color(0, 0, 0, 0.25f));
            SetAnchors(fillBg, 0.02f, 0.15f, 0.98f, 0.85f);
            fillBg.offsetMin = fillBg.offsetMax = Vector2.zero;

            var fillFg = MakePanel(fillBg, "FillFg", new Color(0.30f, 0.80f, 0.35f, 0.6f));
            SetAnchors(fillFg, 0, 0, 1, 1);
            fillFg.offsetMin = fillFg.offsetMax = Vector2.zero;
            timerFillFg = fillFg.GetComponent<Image>();

            // Hourglass icon (right side)
            var hourRT = MakePanel(trt, "Hour", Color.clear);
            SetAnchors(hourRT, 1, 0.5f, 1, 0.5f);
            hourRT.anchoredPosition = new Vector2(-26, 0);
            hourRT.sizeDelta = new Vector2(30, 36);
            var hourTxt = MakeTMP(hourRT, "HourTxt", "\u29D6", 22, COL_WHITE, FontStyles.Normal);
            Stretch(hourTxt.GetComponent<RectTransform>());

            // Timer text (centered)
            timerText = MakeTMP(trt, "TimerTxt", "03:00", 36, COL_WHITE, FontStyles.Bold);
            var ttRT = timerText.GetComponent<RectTransform>();
            SetAnchors(ttRT, 0, 0, 1, 1);
            ttRT.offsetMin = new Vector2(8, 4);
            ttRT.offsetMax = new Vector2(-40, -4);
            timerText.alignment = TextAlignmentOptions.Center;

            // ── LEVEL NAME (center bottom) ──
            int levelId = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentLevelId : 1;
            var levelData = Gameplay.DefaultLevelFactory.Create(levelId);
            string levelLabel = levelData != null ? levelData.DisplayId : $"Level {levelId}";
            var lvlTxt = MakeTMP(strip.GetComponent<RectTransform>(), "LvlName",
                levelLabel, 22, new Color(1, 1, 1, 0.6f), FontStyles.Normal);
            var lvlRT = lvlTxt.GetComponent<RectTransform>();
            SetAnchors(lvlRT, 0.5f, 0, 0.5f, 1);
            lvlRT.sizeDelta = new Vector2(200, 0);
            lvlTxt.alignment = TextAlignmentOptions.Center;
        }

        // ───────────────────────────────────────────
        //  MENU BUTTON (top-right, opens overlay)
        // ───────────────────────────────────────────

        /// <summary>Creates the pause menu button in the top corner.</summary>
        /// <param name="root">The root RectTransform to parent the menu button under.</param>
        private void BuildMenuButton(RectTransform root)
        {
            var btnGO = new GameObject("MenuBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(root, false);
            var rt = btnGO.GetComponent<RectTransform>();
            SetAnchors(rt, 1, 1, 1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-16, -16);
            rt.sizeDelta = new Vector2(56, 56);
            btnGO.GetComponent<Image>().color = COL_MENU_BG;

            var btn = btnGO.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(OnMenuClicked);

            var lbl = MakeTMP(rt, "Lbl", "\u2261", 30, COL_WHITE, FontStyles.Bold);
            Stretch(lbl.GetComponent<RectTransform>());
        }

        /// <summary>Creates the full-screen pause menu overlay with buttons.</summary>
        /// <param name="root">The root RectTransform to parent the overlay under.</param>
        private void BuildMenuOverlay(RectTransform root)
        {
            // Full-screen dark overlay
            menuOverlay = new GameObject("MenuOverlay");
            menuOverlay.transform.SetParent(root, false);
            var overlayRT = menuOverlay.AddComponent<RectTransform>();
            Stretch(overlayRT);
            var overlayImg = menuOverlay.AddComponent<Image>();
            overlayImg.color = COL_OVERLAY;
            overlayImg.raycastTarget = true;

            // Center panel
            var panel = MakePanel(overlayRT, "Panel", COL_MENU_PANEL);
            SetAnchors(panel, 0.5f, 0.5f, 0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(420, 380);

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Title
            var title = MakeTMP(panel, "Title", "PAUSED", 32, COL_ORDER_EDGE, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Center;
            var tle = title.gameObject.AddComponent<LayoutElement>();
            tle.preferredHeight = 44;

            // Buttons
            MakeMenuButton(panel, "RETRY", COL_RETRY, COL_RETRY_SH, COL_WHITE, OnRetryClicked);
            MakeMenuButton(panel, "WORLD MAP", COL_MENU_BTN, COL_MENU_BTN_SH, COL_MENU_TXT, OnWorldMapClicked);
            MakeMenuButton(panel, "MAIN MENU", COL_MENU_BTN, COL_MENU_BTN_SH, COL_MENU_TXT, OnMainMenuClicked);
            MakeMenuButton(panel, "CANCEL", COL_CANCEL, COL_CANCEL_SH, COL_WHITE, OnCancelClicked);
        }

        /// <summary>Creates a styled button in the pause menu.</summary>
        /// <param name="parent">The parent RectTransform to attach the button to.</param>
        /// <param name="label">The text label displayed on the button.</param>
        /// <param name="face">The button face color.</param>
        /// <param name="shadow">The button shadow color.</param>
        /// <param name="txt">The button text color.</param>
        /// <param name="onClick">The callback invoked when the button is clicked.</param>
        private void MakeMenuButton(RectTransform parent, string label, Color face, Color shadow,
            Color txt, UnityEngine.Events.UnityAction onClick)
        {
            int h = 56;
            int borderH = 5;

            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = h;

            var img = go.GetComponent<Image>();
            img.color = face;

            var shGO = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shGO.transform.SetParent(go.transform, false);
            shGO.GetComponent<Image>().color = shadow;
            shGO.GetComponent<Image>().raycastTarget = false;
            var shRT = shGO.GetComponent<RectTransform>();
            shRT.anchorMin = new Vector2(0, 0);
            shRT.anchorMax = new Vector2(1, 0);
            shRT.pivot = new Vector2(0.5f, 0);
            shRT.offsetMin = shRT.offsetMax = Vector2.zero;
            shRT.sizeDelta = new Vector2(0, borderH);

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var lbl = MakeTMP(go.GetComponent<RectTransform>(), "Lbl", label, 24, txt, FontStyles.Bold);
            var lblRT = lbl.GetComponent<RectTransform>();
            Stretch(lblRT);
            lblRT.offsetMin = new Vector2(8, borderH);
            lblRT.offsetMax = new Vector2(-8, -2);
        }

        // ───────────────────────────────────────────
        //  ORDER CARDS (top-left, dynamic)
        // ───────────────────────────────────────────

        /// <summary>Creates the order card display area.</summary>
        /// <param name="root">The root RectTransform to parent the order cards under.</param>
        private void BuildOrderCards(RectTransform root)
        {
            orderWrap = MakePanel(root, "Orders", new Color(0.06f, 0.05f, 0.04f, 0.8f));
            SetAnchors(orderWrap, 0, 1, 0, 1);
            orderWrap.pivot = new Vector2(0, 1);
            orderWrap.anchoredPosition = new Vector2(12, -12);
            orderWrap.sizeDelta = new Vector2(320, 260);

            var vlg = orderWrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Title
            var title = MakeTMP(orderWrap, "Title", "\u25B6 ORDERS", 20, COL_ORDER_EDGE, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Left;
            var tle = title.gameObject.AddComponent<LayoutElement>();
            tle.preferredHeight = 28;

            // 4 order slots
            for (int i = 0; i < 4; i++)
            {
                orderTexts[i] = MakeTMP(orderWrap, $"Ord{i}", "", 17, COL_WHITE, FontStyles.Normal);
                orderTexts[i].alignment = TextAlignmentOptions.Left;
                orderTexts[i].enableWordWrapping = true;
                var le = orderTexts[i].gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 44;
                orderTexts[i].gameObject.SetActive(false);
            }
        }

        // ───────────────────────────────────────────
        //  CONTROLS HINT
        // ───────────────────────────────────────────

        /// <summary>Creates the keyboard controls hint text.</summary>
        /// <param name="root">The root RectTransform to parent the controls hint under.</param>
        private void BuildControlsHint(RectTransform root)
        {
            var hint = MakeTMP(root, "Controls",
                "WASD: Move  |  Space/E: Interact  |  Click: Interact",
                16, new Color(1, 1, 1, 0.45f), FontStyles.Normal);
            var hrt = hint.GetComponent<RectTransform>();
            SetAnchors(hrt, 0.5f, 0, 0.5f, 0);
            hrt.pivot = new Vector2(0.5f, 0);
            hrt.anchoredPosition = new Vector2(0, 106);
            hrt.sizeDelta = new Vector2(500, 24);
            hint.alignment = TextAlignmentOptions.Center;
        }

        // ═══════════════════════════════════════════════════════
        //  UPDATE
        // ═══════════════════════════════════════════════════════

        /// <summary>Updates timer bar fill, color, and text.</summary>
        /// <param name="timeRemaining">The remaining time in seconds.</param>
        private void UpdateTimer(float timeRemaining)
        {
            RefreshOrders();

            if (timerText != null)
            {
                int m = Mathf.FloorToInt(timeRemaining / 60f);
                int s = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"{m:00}:{s:00}";

                // Color the whole timer bar based on urgency
                if (timeRemaining < 30f)
                {
                    timerText.color = COL_WHITE;
                    if (timerBarBg != null) timerBarBg.color = COL_TIMER_CRIT;
                }
                else if (timeRemaining < 60f)
                {
                    timerText.color = COL_WHITE;
                    if (timerBarBg != null) timerBarBg.color = COL_TIMER_LOW;
                }
                else
                {
                    timerText.color = COL_WHITE;
                    if (timerBarBg != null) timerBarBg.color = COL_TIMER_BAR;
                }
            }

            if (timerFillFg != null && gameTimer != null)
            {
                float ratio = gameTimer.TimeRatio;
                timerFillFg.rectTransform.anchorMax = new Vector2(ratio, 1);
            }
        }

        /// <summary>Updates the score text display.</summary>
        /// <param name="score">The current score value.</param>
        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
        }

        /// <summary>Updates the combo streak text display.</summary>
        /// <param name="combo">The current combo streak count.</param>
        private void UpdateCombo(int combo)
        {
            if (comboText != null)
            {
                comboText.gameObject.SetActive(combo > 1);
                comboText.text = $"x{combo}!";
            }
        }

        /// <summary>Refreshes the order card texts from the active order queue.</summary>
        private void RefreshOrders()
        {
            if (orderQueue == null) orderQueue = FindAnyObjectByType<Gameplay.OrderQueue>();
            if (orderQueue == null) return;

            var orders = orderQueue.ActiveOrders;
            for (int i = 0; i < 4; i++)
            {
                if (orderTexts[i] == null) continue;
                if (i < orders.Count)
                {
                    orderTexts[i].gameObject.SetActive(true);
                    var o = orders[i];
                    int sec = Mathf.CeilToInt(o.remainingTime);
                    string timeStr = sec >= 60 ? $"{sec / 60}:{sec % 60:D2}" : $"{sec}s";
                    bool urgent = o.TimeRatio < 0.3f;
                    orderTexts[i].color = urgent ? COL_TIMER_CRIT : COL_WHITE;

                    // Show ingredients needed clearly
                    string ingredients = "";
                    if (o.recipe.finalIngredients != null)
                    {
                        foreach (var ing in o.recipe.finalIngredients)
                        {
                            string state = ing.requiredState switch
                            {
                                Gameplay.IngredientState.Chopped => "Chop",
                                Gameplay.IngredientState.Cooked => "Cook",
                                _ => ing.requiredState.ToString()
                            };
                            if (ingredients.Length > 0) ingredients += " + ";
                            ingredients += $"{state} {ing.ingredientType}";
                        }
                    }
                    orderTexts[i].text = $"\u2022 {o.recipe.recipeName} ({timeStr})\n  \u279C {ingredients}";
                }
                else
                {
                    orderTexts[i].gameObject.SetActive(false);
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  CALLBACKS
        // ═══════════════════════════════════════════════════════

        /// <summary>Shows the pause menu overlay and pauses the game.</summary>
        private void OnMenuClicked()
        {
            if (Core.GameManager.Instance == null) return;

            if (Core.GameManager.Instance.CurrentGameState == Core.GameState.Playing)
            {
                Core.GameManager.Instance.PauseGame();
                BuildMenuOverlay(hudCanvas.GetComponent<RectTransform>());
            }
        }

        /// <summary>Hides the pause menu overlay and resumes the game.</summary>
        private void OnCancelClicked()
        {
            if (menuOverlay != null) Destroy(menuOverlay);
            if (Core.GameManager.Instance != null)
                Core.GameManager.Instance.ResumeGame();
        }

        /// <summary>Reloads the current scene to restart the level.</summary>
        private void OnRetryClicked()
        {
            Time.timeScale = 1f;
            if (Core.GameManager.Instance != null)
                Core.GameManager.Instance.LoadLevel(Core.GameManager.Instance.CurrentLevelId);
        }

        /// <summary>Returns to the level select screen.</summary>
        private void OnWorldMapClicked()
        {
            Time.timeScale = 1f;
            if (Core.GameManager.Instance != null)
                Core.GameManager.Instance.LoadLevelSelect();
        }

        /// <summary>Returns to the main menu.</summary>
        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            if (Core.GameManager.Instance != null)
                Core.GameManager.Instance.LoadMainMenu();
        }

        // ═══════════════════════════════════════════════════════
        //  FACTORY HELPERS
        // ═══════════════════════════════════════════════════════

        /// <summary>Creates an EventSystem if none exists in the scene.</summary>
        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var g = new GameObject("EventSystem");
                g.AddComponent<EventSystem>();
                g.AddComponent<StandaloneInputModule>();
            }
        }

        /// <summary>Creates a UI panel with an Image component.</summary>
        /// <param name="parent">The parent RectTransform to attach the panel to.</param>
        /// <param name="name">The name of the panel GameObject.</param>
        /// <param name="c">The background color of the panel.</param>
        /// <returns>The RectTransform of the created panel.</returns>
        private static RectTransform MakePanel(RectTransform parent, string name, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = c;
            return go.GetComponent<RectTransform>();
        }

        /// <summary>Creates a TextMeshProUGUI element.</summary>
        /// <param name="parent">The parent RectTransform to attach the text to.</param>
        /// <param name="name">The name of the text GameObject.</param>
        /// <param name="text">The initial text content.</param>
        /// <param name="size">The font size.</param>
        /// <param name="c">The text color.</param>
        /// <param name="style">The font style.</param>
        /// <returns>The created TextMeshProUGUI component.</returns>
        private static TextMeshProUGUI MakeTMP(RectTransform parent, string name, string text,
            int size, Color c, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = c;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            return tmp;
        }

        /// <summary>Sets the min and max anchors on a RectTransform.</summary>
        /// <param name="rt">The RectTransform to modify.</param>
        /// <param name="ax">The minimum anchor X value.</param>
        /// <param name="ay">The minimum anchor Y value.</param>
        /// <param name="bx">The maximum anchor X value.</param>
        /// <param name="by">The maximum anchor Y value.</param>
        private static void SetAnchors(RectTransform rt, float ax, float ay, float bx, float by)
        {
            rt.anchorMin = new Vector2(ax, ay);
            rt.anchorMax = new Vector2(bx, by);
        }

        /// <summary>Stretches a RectTransform to fill its parent.</summary>
        /// <param name="rt">The RectTransform to stretch.</param>
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
