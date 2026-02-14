using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using IOChef.Core;
using IOChef.Economy;
using IOChef.Gameplay;

namespace IOChef.UI
{
    /// <summary>
    /// Full-screen pre-round ingredient shop UI. Displayed when the player taps a level
    /// in <see cref="LevelSelectUI"/>. Shows required ingredients, allows purchasing,
    /// and gates level entry behind ingredient ownership and an entry cost.
    /// Fully programmatic – no Inspector refs or prefabs needed.
    /// </summary>
    public class IngredientShopUI : MonoBehaviour
    {
        // ─── Color palette ───

        /// <summary>
        /// Dark blue-grey background gradient top color.
        /// </summary>
        private static readonly Color COL_BG_TOP = new(0.18f, 0.22f, 0.30f);

        /// <summary>
        /// Darker blue-grey background gradient bottom color.
        /// </summary>
        private static readonly Color COL_BG_BOT = new(0.12f, 0.15f, 0.22f);

        /// <summary>
        /// Green play / start button face color.
        /// </summary>
        private static readonly Color COL_PLAY = new(0.30f, 0.75f, 0.30f);

        /// <summary>
        /// Green play / start button shadow color.
        /// </summary>
        private static readonly Color COL_PLAY_SHADOW = new(0.15f, 0.50f, 0.15f);

        /// <summary>
        /// Golden yellow general button face color.
        /// </summary>
        private static readonly Color COL_BTN = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Darker gold general button shadow color.
        /// </summary>
        private static readonly Color COL_BTN_SHADOW = new(0.82f, 0.62f, 0.08f);

        /// <summary>
        /// Dark brown button text color.
        /// </summary>
        private static readonly Color COL_BTN_TEXT = new(0.22f, 0.13f, 0.04f);

        /// <summary>
        /// Red quit / back button face color.
        /// </summary>
        private static readonly Color COL_QUIT = new(0.82f, 0.22f, 0.18f);

        /// <summary>
        /// Dark red quit / back button shadow color.
        /// </summary>
        private static readonly Color COL_QUIT_SHADOW = new(0.58f, 0.12f, 0.10f);

        /// <summary>
        /// Green color for owned ingredient labels.
        /// </summary>
        private static readonly Color COL_OWNED = new(0.30f, 0.75f, 0.30f);

        /// <summary>
        /// Grey color for locked / unavailable elements.
        /// </summary>
        private static readonly Color COL_LOCKED = new(0.50f, 0.48f, 0.45f);

        /// <summary>
        /// Golden coin icon / text accent color.
        /// </summary>
        private static readonly Color COL_COIN = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Blue gem icon / text accent color.
        /// </summary>
        private static readonly Color COL_GEM = new(0.40f, 0.70f, 1f);

        /// <summary>
        /// Slightly lighter dark-blue card background color.
        /// </summary>
        private static readonly Color COL_CARD_BG = new(0.25f, 0.28f, 0.35f);

        // ─── Runtime state ───

        /// <summary>
        /// Unique level identifier for the level being entered.
        /// </summary>
        private int _levelId;

        /// <summary>
        /// World identifier for the level being entered.
        /// </summary>
        private int _worldId;

        /// <summary>
        /// Level data ScriptableObject for the selected level.
        /// </summary>
        private LevelDataSO _levelData;

        /// <summary>
        /// Root canvas for this shop screen.
        /// </summary>
        private Canvas _canvas;

        /// <summary>
        /// Text element displaying the current coin balance.
        /// </summary>
        private TextMeshProUGUI _coinText;

        /// <summary>
        /// Text element displaying the current gem balance.
        /// </summary>
        private TextMeshProUGUI _gemText;

        /// <summary>
        /// The start button GameObject, enabled/disabled based on readiness.
        /// </summary>
        private GameObject _startBtn;

        /// <summary>
        /// Status text below the start button showing affordability info.
        /// </summary>
        private TextMeshProUGUI _statusText;

        /// <summary>
        /// Whether the first-purchase tutorial arrow is currently active.
        /// </summary>
        private bool _tutorialActive;

        /// <summary>
        /// RectTransform of the pulsing tutorial arrow indicator.
        /// </summary>
        private RectTransform _tutorialArrow;

        /// <summary>
        /// Cached list of ingredient cards for refresh after purchase.
        /// </summary>
        private readonly List<RectTransform> _cardRoots = new();

        /// <summary>
        /// Cached ingredient types in display order, parallel to <see cref="_cardRoots"/>.
        /// </summary>
        private readonly List<IngredientType> _cardTypes = new();

        /// <summary>
        /// The scrollable content parent that holds ingredient rows.
        /// </summary>
        private RectTransform _gridContent;

        // ═══════════════════════════════════════════════════════
        //  STATIC ENTRY POINT
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Creates and shows the ingredient shop screen for the specified level.
        /// </summary>
        /// <param name="worldId">World identifier of the target level.</param>
        /// <param name="levelId">Level identifier of the target level.</param>
        /// <param name="levelData">Level data ScriptableObject describing recipes and entry cost.</param>
        public static void Show(int worldId, int levelId, LevelDataSO levelData)
        {
            var go = new GameObject("IngredientShopUI");
            var ui = go.AddComponent<IngredientShopUI>();
            ui._worldId = worldId;
            ui._levelId = levelId;
            ui._levelData = levelData;
            ui.BuildUI();
        }

        // ═══════════════════════════════════════════════════════
        //  LIFECYCLE
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Unsubscribes from currency change events when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnCoinsChanged -= OnCoinsChanged;
                CurrencyManager.Instance.OnGemsChanged -= OnGemsChanged;
            }

            if (IngredientShopManager.Instance != null)
                IngredientShopManager.Instance.OnIngredientPurchased -= OnIngredientPurchasedExternal;
        }

        // ═══════════════════════════════════════════════════════
        //  BUILD
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Constructs the full ingredient shop layout including header, currency bar,
        /// ingredient grid, start button, and back button.
        /// </summary>
        private void BuildUI()
        {
            EnsureEventSystem();

            var cgo = new GameObject("IngredientShopCanvas");
            cgo.transform.SetParent(transform);
            _canvas = cgo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();
            var root = cgo.GetComponent<RectTransform>();

            BuildBackground(root);
            BuildHeader(root);
            BuildCurrencyBar(root);
            BuildIngredientGrid(root);
            BuildBottomButtons(root);

            // Subscribe to currency and purchase events
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnCoinsChanged += OnCoinsChanged;
                CurrencyManager.Instance.OnGemsChanged += OnGemsChanged;
            }

            if (IngredientShopManager.Instance != null)
                IngredientShopManager.Instance.OnIngredientPurchased += OnIngredientPurchasedExternal;

            UpdateCurrencyDisplay();
            UpdateStartButton();
            CheckTutorial();
        }

        // ─── Background ───

        /// <summary>
        /// Creates the dark gradient background panels.
        /// </summary>
        /// <param name="p">Parent RectTransform for the background elements.</param>
        private void BuildBackground(RectTransform p)
        {
            var bg = MakePanel(p, "Bg", COL_BG_TOP); Stretch(bg);

            var ov = MakePanel(p, "BgBot", COL_BG_BOT); Stretch(ov);
            ov.GetComponent<Image>().color = new Color(COL_BG_BOT.r, COL_BG_BOT.g, COL_BG_BOT.b, 0.55f);
            ov.anchorMin = new Vector2(0, 0);
            ov.anchorMax = new Vector2(1, 0.35f);
            ov.offsetMin = ov.offsetMax = Vector2.zero;

            var stripe = MakePanel(p, "Stripe", new Color(1f, 1f, 1f, 0.06f));
            stripe.anchorMin = new Vector2(0, 0.90f);
            stripe.anchorMax = new Vector2(1, 0.93f);
            stripe.offsetMin = stripe.offsetMax = Vector2.zero;
        }

        // ─── Header ───

        /// <summary>
        /// Creates the level name and entry cost header text.
        /// </summary>
        /// <param name="p">Parent RectTransform for the header elements.</param>
        private void BuildHeader(RectTransform p)
        {
            string displayId = _levelData != null ? _levelData.DisplayId : $"{_worldId}-?";
            string levelName = _levelData != null ? _levelData.levelName : "Unknown";
            int entryCost = _levelData != null ? _levelData.entryCost : 0;

            string headerText = entryCost > 0
                ? $"Level {displayId}: {levelName} - Entry: {entryCost} coins"
                : $"Level {displayId}: {levelName}";

            var sh = MakeText(p, "HeaderShadow", headerText, 36, new Color(0, 0, 0, 0.5f), FontStyles.Bold);
            AnchorTop(sh, new Vector2(1000, 60), new Vector2(3, -18));

            var title = MakeText(p, "Header", headerText, 36, Color.white, FontStyles.Bold);
            AnchorTop(title, new Vector2(1000, 60), new Vector2(0, -15));
        }

        // ─── Currency bar ───

        /// <summary>
        /// Creates the coin and gem balance display at the top-right and subscribes
        /// to <see cref="CurrencyManager.OnCoinsChanged"/> for live updates.
        /// </summary>
        /// <param name="p">Parent RectTransform for the currency bar elements.</param>
        private void BuildCurrencyBar(RectTransform p)
        {
            // Container anchored top-right
            var bar = MakePanel(p, "CurrencyBar", new Color(0, 0, 0, 0.35f));
            bar.anchorMin = new Vector2(1f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(1f, 1f);
            bar.anchoredPosition = new Vector2(-16, -80);
            bar.sizeDelta = new Vector2(320, 45);

            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(12, 12, 4, 4);

            // Coin icon + text
            var coinLabel = MakeText(bar, "CoinIcon", "o", 20, COL_COIN, FontStyles.Bold);
            AddLE(coinLabel.gameObject, 36, 24);

            var coinRt = MakeText(bar, "CoinText", "0", 22, COL_COIN, FontStyles.Normal);
            coinRt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            AddLE(coinRt.gameObject, 36, 80);
            _coinText = coinRt.GetComponent<TextMeshProUGUI>();

            // Gem icon + text
            var gemLabel = MakeText(bar, "GemIcon", "<>", 18, COL_GEM, FontStyles.Bold);
            AddLE(gemLabel.gameObject, 36, 28);

            var gemRt = MakeText(bar, "GemText", "0", 22, COL_GEM, FontStyles.Normal);
            gemRt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            AddLE(gemRt.gameObject, 36, 70);
            _gemText = gemRt.GetComponent<TextMeshProUGUI>();
        }

        // ─── Ingredient Grid ───

        /// <summary>
        /// Creates a vertically scrollable grid of ingredient cards laid out in two columns.
        /// Each row uses a <see cref="HorizontalLayoutGroup"/> inside a <see cref="VerticalLayoutGroup"/>.
        /// </summary>
        /// <param name="p">Parent RectTransform for the grid viewport and scroll view.</param>
        private void BuildIngredientGrid(RectTransform p)
        {
            // Viewport
            var viewportGO = new GameObject("GridViewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(p, false);
            var viewport = viewportGO.GetComponent<RectTransform>();
            viewport.anchorMin = new Vector2(0.04f, 0.18f);
            viewport.anchorMax = new Vector2(0.96f, 0.85f);
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;

            var vpImg = viewportGO.GetComponent<Image>();
            vpImg.color = new Color(1, 1, 1, 0.01f);
            vpImg.raycastTarget = true;

            var mask = viewportGO.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Scrollable content
            var contentGO = new GameObject("GridContent", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            _gridContent = contentGO.GetComponent<RectTransform>();
            _gridContent.anchorMin = new Vector2(0, 1);
            _gridContent.anchorMax = new Vector2(1, 1);
            _gridContent.pivot = new Vector2(0.5f, 1f);
            _gridContent.offsetMin = new Vector2(0, 0);
            _gridContent.offsetMax = new Vector2(0, 0);

            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.padding = new RectOffset(8, 8, 8, 8);

            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ScrollRect
            var scroll = viewportGO.AddComponent<ScrollRect>();
            scroll.content = _gridContent;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
            scroll.viewport = viewport;

            // Gather required ingredient types
            List<IngredientType> ingredients = new();
            if (IngredientShopManager.Instance != null && _levelData != null)
                ingredients = IngredientShopManager.Instance.GetIngredientsForRecipes(_levelData.availableRecipes);

            // Build cards in rows of 2
            _cardRoots.Clear();
            _cardTypes.Clear();

            RectTransform currentRow = null;
            int colIndex = 0;

            for (int i = 0; i < ingredients.Count; i++)
            {
                if (colIndex == 0)
                {
                    currentRow = CreateRow(_gridContent);
                    colIndex = 0;
                }

                BuildIngredientCard(currentRow, ingredients[i]);
                colIndex++;

                if (colIndex >= 2) colIndex = 0;
            }

            // If odd number of ingredients, add a spacer to keep the last card half-width
            if (ingredients.Count % 2 == 1 && currentRow != null)
            {
                var spacer = MakePanel(currentRow, "Spacer", Color.clear);
                var sLE = spacer.gameObject.AddComponent<LayoutElement>();
                sLE.flexibleWidth = 1;
                sLE.preferredHeight = 180;
            }
        }

        /// <summary>
        /// Creates a horizontal row container for ingredient cards.
        /// </summary>
        /// <param name="parent">Parent vertical layout content transform.</param>
        /// <returns>The RectTransform of the new row.</returns>
        private RectTransform CreateRow(RectTransform parent)
        {
            var rowGO = new GameObject("Row", typeof(RectTransform));
            rowGO.transform.SetParent(parent, false);
            var rowRT = rowGO.GetComponent<RectTransform>();

            var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.UpperCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var le = rowGO.AddComponent<LayoutElement>();
            le.preferredHeight = 180;
            le.flexibleHeight = 0;

            return rowRT;
        }

        /// <summary>
        /// Creates a single ingredient card showing name, stock quantity, price, and buy button.
        /// </summary>
        /// <param name="parent">Parent RectTransform (a horizontal row) to attach the card to.</param>
        /// <param name="type">The ingredient type this card represents.</param>
        private void BuildIngredientCard(RectTransform parent, IngredientType type)
        {
            int stock = IngredientShopManager.Instance != null
                ? IngredientShopManager.Instance.GetStock(type) : 0;
            bool atMax = stock >= IngredientShopManager.MAX_STOCK;
            int price = IngredientShopManager.Instance != null
                ? IngredientShopManager.Instance.GetIngredientPrice(type) : 0;

            // Card background
            var card = MakePanel(parent, $"Card_{type}", COL_CARD_BG);
            var cardLE = card.gameObject.AddComponent<LayoutElement>();
            cardLE.flexibleWidth = 1;
            cardLE.preferredHeight = 180;

            // Internal vertical layout
            var vl = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 4;
            vl.childAlignment = TextAnchor.MiddleCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.padding = new RectOffset(10, 10, 8, 8);

            // Ingredient name
            var nameRt = MakeText(card, "Name", type.ToString(), 24, Color.white, FontStyles.Bold);
            AddLE(nameRt.gameObject, 30);

            // Stock quantity line
            Color stockColor = stock > 0 ? COL_OWNED : new Color(1f, 0.4f, 0.4f);
            string stockStr = $"x{stock} / {IngredientShopManager.MAX_STOCK}";
            var stockRt = MakeText(card, "Stock", stockStr, 20, stockColor, FontStyles.Normal);
            AddLE(stockRt.gameObject, 26);

            // Price line
            string priceStr = $"{price} coins / 100";
            var priceRt = MakeText(card, "Price", priceStr, 16, COL_COIN, FontStyles.Italic);
            AddLE(priceRt.gameObject, 22);

            // Button: BUY 100 or MAX
            if (atMax)
            {
                var maxRt = MakeText(card, "Max", "MAX", 20, COL_LOCKED, FontStyles.Bold);
                AddLE(maxRt.gameObject, 50);
            }
            else
            {
                bool canAfford = CurrencyManager.Instance != null
                    && CurrencyManager.Instance.CanAffordCoins(price);
                Color btnFace = canAfford ? COL_BTN : COL_LOCKED;
                Color btnSh = canAfford ? COL_BTN_SHADOW : new Color(0.35f, 0.33f, 0.30f);
                Color btnTxt = canAfford ? COL_BTN_TEXT : Color.white;

                int willAdd = Mathf.Min(IngredientShopManager.BATCH_SIZE,
                    IngredientShopManager.MAX_STOCK - stock);
                string buyLabel = $"BUY {willAdd}";

                IngredientType capturedType = type;
                MakeChunkyButton(card, buyLabel, btnFace, btnSh, btnTxt, 18, 50,
                    () => OnBuyIngredient(capturedType));
            }

            _cardRoots.Add(card);
            _cardTypes.Add(type);
        }

        // ─── Bottom buttons ───

        /// <summary>
        /// Creates the START and BACK buttons at the bottom of the screen.
        /// </summary>
        /// <param name="p">Parent RectTransform for the bottom button area.</param>
        private void BuildBottomButtons(RectTransform p)
        {
            var wrap = MakePanel(p, "BottomWrap", Color.clear);
            wrap.anchorMin = new Vector2(0.08f, 0.02f);
            wrap.anchorMax = new Vector2(0.92f, 0.16f);
            wrap.offsetMin = wrap.offsetMax = Vector2.zero;

            var vlg = wrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.padding = new RectOffset(0, 0, 0, 0);

            // Status text (shows "Need X more coins" etc.)
            var statusRt = MakeText(wrap, "StatusText", "", 20,
                new Color(1, 0.6f, 0.6f), FontStyles.Italic);
            AddLE(statusRt.gameObject, 28);
            _statusText = statusRt.GetComponent<TextMeshProUGUI>();

            // Button row
            var row = MakePanel(wrap, "BtnRow", Color.clear);
            AddLE(row.gameObject, 75);

            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Back button
            MakeChunkyButton(row, "BACK", COL_QUIT, COL_QUIT_SHADOW, Color.white, 26, 70, OnBackClicked);

            // Start button
            var startGO = MakeChunkyButtonGO(row, "START", COL_PLAY, COL_PLAY_SHADOW,
                Color.white, 30, 70, OnStartClicked);
            _startBtn = startGO;
        }

        // ═══════════════════════════════════════════════════════
        //  TUTORIAL
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Checks whether the shop tutorial has been completed. If not, shows a pulsing
        /// yellow arrow pointing at the first unpurchased ingredient card.
        /// </summary>
        private void CheckTutorial()
        {
            if (PlayerPrefs.GetInt("ShopTutorialDone", 0) != 0)
            {
                _tutorialActive = false;
                return;
            }

            // Find first unpurchased card
            RectTransform targetCard = null;
            for (int i = 0; i < _cardTypes.Count; i++)
            {
                if (IngredientShopManager.Instance != null
                    && !IngredientShopManager.Instance.IsIngredientUnlocked(_cardTypes[i]))
                {
                    targetCard = _cardRoots[i];
                    break;
                }
            }

            if (targetCard == null)
            {
                _tutorialActive = false;
                return;
            }

            _tutorialActive = true;

            // Arrow pointing down at the card
            var arrowGO = new GameObject("TutorialArrow", typeof(RectTransform), typeof(TextMeshProUGUI));
            arrowGO.transform.SetParent(targetCard, false);
            _tutorialArrow = arrowGO.GetComponent<RectTransform>();
            _tutorialArrow.anchorMin = new Vector2(0.5f, 1f);
            _tutorialArrow.anchorMax = new Vector2(0.5f, 1f);
            _tutorialArrow.pivot = new Vector2(0.5f, 0f);
            _tutorialArrow.anchoredPosition = new Vector2(0, 5);
            _tutorialArrow.sizeDelta = new Vector2(200, 60);

            var tmp = arrowGO.GetComponent<TextMeshProUGUI>();
            tmp.text = "v\nBuy this ingredient\nto start cooking!";
            tmp.fontSize = 16;
            tmp.color = COL_BTN;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Bottom;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;

            StartCoroutine(PulseTutorialArrow());
        }

        /// <summary>
        /// Coroutine that oscillates the tutorial arrow scale to draw attention.
        /// </summary>
        /// <returns>An enumerator for the coroutine.</returns>
        private System.Collections.IEnumerator PulseTutorialArrow()
        {
            while (_tutorialActive && _tutorialArrow != null)
            {
                float scale = 1f + 0.15f * Mathf.Sin(Time.unscaledTime * 4f);
                _tutorialArrow.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }

        /// <summary>
        /// Dismisses the tutorial and marks it as completed in PlayerPrefs.
        /// </summary>
        private void DismissTutorial()
        {
            _tutorialActive = false;
            PlayerPrefs.SetInt("ShopTutorialDone", 1);
            PlayerPrefs.Save();

            if (_tutorialArrow != null)
                Destroy(_tutorialArrow.gameObject);
        }

        // ═══════════════════════════════════════════════════════
        //  CALLBACKS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Handles purchasing an ingredient. Calls <see cref="IngredientShopManager.PurchaseIngredient"/>
        /// and refreshes the entire grid if successful.
        /// </summary>
        /// <param name="type">The ingredient type the player wants to buy.</param>
        private void OnBuyIngredient(IngredientType type)
        {
            if (IngredientShopManager.Instance == null) return;

            IngredientShopManager.Instance.PurchaseIngredient(type, success =>
            {
                if (success)
                {
                    Debug.Log($"[IngredientShopUI] Purchased {type}");

                    if (_tutorialActive)
                        DismissTutorial();

                    RefreshGrid();
                    UpdateCurrencyDisplay();
                    UpdateStartButton();
                }
                else
                {
                    Debug.Log($"[IngredientShopUI] Cannot afford {type}");
                }
            });
        }

        /// <summary>
        /// Handles external ingredient purchase events (from other UI) to keep this display in sync.
        /// </summary>
        /// <param name="type">The ingredient type that was purchased.</param>
        private void OnIngredientPurchasedExternal(IngredientType type)
        {
            RefreshGrid();
            UpdateStartButton();
        }

        /// <summary>
        /// Calls the StartLevel CloudScript handler to validate and charge entry cost
        /// server-side, then loads the level on success.
        /// </summary>
        private void OnStartClicked()
        {
            // Disable button to prevent double-clicks
            if (_startBtn != null)
            {
                var btn = _startBtn.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }

            Debug.Log($"[IngredientShopUI] Requesting StartLevel for World {_worldId} Level {_levelId}");

            PlayFabManager.Instance?.ExecuteCloudScript("StartLevel",
                new { levelId = _levelId },
                resultJson =>
                {
                    try
                    {
                        var result = Newtonsoft.Json.Linq.JObject.Parse(resultJson);
                        bool success = result["success"] != null && bool.Parse(result["success"].ToString());

                        if (success)
                        {
                            // Refresh currencies after entry cost deduction
                            PlayFabManager.Instance?.RefreshCurrencies();

                            Debug.Log($"[IngredientShopUI] Starting World {_worldId} Level {_levelId}");
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.SetCurrentWorld(_worldId);
                                GameManager.Instance.LoadLevel(_levelId);
                            }
                        }
                        else
                        {
                            string error = result["error"]?.ToString() ?? "Unknown error";
                            Debug.LogWarning($"[IngredientShopUI] StartLevel failed: {error}");
                            ReEnableStartButton();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[IngredientShopUI] StartLevel parse error: {ex.Message}");
                        ReEnableStartButton();
                    }
                },
                err =>
                {
                    Debug.LogWarning($"[IngredientShopUI] StartLevel CloudScript error: {err}");
                    ReEnableStartButton();
                });
        }

        private void ReEnableStartButton()
        {
            if (_startBtn != null)
            {
                var btn = _startBtn.GetComponent<Button>();
                if (btn != null) btn.interactable = true;
            }
        }

        /// <summary>
        /// Destroys this UI and returns the player to the level select screen.
        /// </summary>
        private void OnBackClicked()
        {
            Debug.Log("[IngredientShopUI] Back to level select");
            Destroy(gameObject);
        }

        /// <summary>
        /// Handles coin balance change events from <see cref="CurrencyManager"/>.
        /// </summary>
        /// <param name="newBalance">The updated coin balance.</param>
        private void OnCoinsChanged(long newBalance)
        {
            UpdateCurrencyDisplay();
            UpdateStartButton();
        }

        /// <summary>
        /// Handles gem balance change events from <see cref="CurrencyManager"/>.
        /// </summary>
        /// <param name="newBalance">The updated gem balance.</param>
        private void OnGemsChanged(long newBalance)
        {
            UpdateCurrencyDisplay();
        }

        // ═══════════════════════════════════════════════════════
        //  REFRESH
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Refreshes the coin and gem text from <see cref="CurrencyManager"/>.
        /// </summary>
        private void UpdateCurrencyDisplay()
        {
            if (CurrencyManager.Instance == null) return;

            if (_coinText != null)
                _coinText.text = CurrencyManager.Instance.Coins.ToString("N0");
            if (_gemText != null)
                _gemText.text = CurrencyManager.Instance.Gems.ToString("N0");
        }

        /// <summary>
        /// Checks all conditions and enables or disables the start button.
        /// Conditions: all required ingredients must be owned and the player must
        /// be able to afford the entry cost.
        /// </summary>
        private void UpdateStartButton()
        {
            if (_startBtn == null) return;

            bool hasAllIngredients = IngredientShopManager.Instance != null
                && IngredientShopManager.Instance.HasAllIngredientsForLevel(_levelData);

            int entryCost = _levelData != null ? _levelData.entryCost : 0;
            bool canAffordEntry = entryCost <= 0
                || (CurrencyManager.Instance != null && CurrencyManager.Instance.CanAffordCoins(entryCost));

            bool canStart = hasAllIngredients && canAffordEntry;

            var btn = _startBtn.GetComponent<Button>();
            if (btn != null) btn.interactable = canStart;

            var img = _startBtn.GetComponent<Image>();
            if (img != null) img.color = canStart ? COL_PLAY : COL_LOCKED;

            // Update status text
            if (_statusText != null)
            {
                if (!hasAllIngredients)
                {
                    var missing = IngredientShopManager.Instance != null
                        ? IngredientShopManager.Instance.GetMissingIngredientsForLevel(_levelData)
                        : new List<IngredientType>();
                    _statusText.text = $"Purchase {missing.Count} more ingredient{(missing.Count != 1 ? "s" : "")} to start";
                    _statusText.color = new Color(1f, 0.6f, 0.6f);
                }
                else if (!canAffordEntry)
                {
                    long currentCoins = CurrencyManager.Instance != null ? CurrencyManager.Instance.Coins : 0;
                    long deficit = entryCost - currentCoins;
                    _statusText.text = $"Need {deficit} more coins";
                    _statusText.color = new Color(1f, 0.6f, 0.6f);
                }
                else
                {
                    _statusText.text = "Ready to cook!";
                    _statusText.color = COL_OWNED;
                }
            }
        }

        /// <summary>
        /// Destroys all current ingredient cards and rebuilds the grid.
        /// Called after a purchase to reflect the new ownership state.
        /// </summary>
        private void RefreshGrid()
        {
            if (_gridContent == null) return;

            // Destroy existing rows
            for (int i = _gridContent.childCount - 1; i >= 0; i--)
                Destroy(_gridContent.GetChild(i).gameObject);

            _cardRoots.Clear();
            _cardTypes.Clear();

            // Rebuild
            List<IngredientType> ingredients = new();
            if (IngredientShopManager.Instance != null && _levelData != null)
                ingredients = IngredientShopManager.Instance.GetIngredientsForRecipes(_levelData.availableRecipes);

            RectTransform currentRow = null;
            int colIndex = 0;

            for (int i = 0; i < ingredients.Count; i++)
            {
                if (colIndex == 0)
                {
                    currentRow = CreateRow(_gridContent);
                    colIndex = 0;
                }

                BuildIngredientCard(currentRow, ingredients[i]);
                colIndex++;

                if (colIndex >= 2) colIndex = 0;
            }

            if (ingredients.Count % 2 == 1 && currentRow != null)
            {
                var spacer = MakePanel(currentRow, "Spacer", Color.clear);
                var sLE = spacer.gameObject.AddComponent<LayoutElement>();
                sLE.flexibleWidth = 1;
                sLE.preferredHeight = 180;
            }

            CheckTutorial();
        }

        // ═══════════════════════════════════════════════════════
        //  FACTORY HELPERS
        // ═══════════════════════════════════════════════════════

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
            t.textWrappingMode = TextWrappingModes.Normal;
            return go.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Creates a styled chunky button with shadow, identical to <see cref="MainMenuUI"/> style.
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
            MakeChunkyButtonGO(parent, label, face, shadow, txt, size, h, onClick);
        }

        /// <summary>
        /// Creates a styled chunky button with shadow and returns the root GameObject.
        /// </summary>
        /// <param name="parent">Parent RectTransform.</param>
        /// <param name="label">Button label text.</param>
        /// <param name="face">Face color.</param>
        /// <param name="shadow">Shadow strip color.</param>
        /// <param name="txt">Text color.</param>
        /// <param name="size">Font size.</param>
        /// <param name="h">Button height.</param>
        /// <param name="onClick">Click callback.</param>
        /// <returns>The root GameObject of the created button.</returns>
        private GameObject MakeChunkyButtonGO(RectTransform parent, string label, Color face, Color shadow,
            Color txt, int size, int h, UnityEngine.Events.UnityAction onClick)
        {
            int borderH = Mathf.Max(5, h / 10);

            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            AddLE(go, h);

            var img = go.GetComponent<Image>();
            img.color = face;

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

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

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

            go.AddComponent<ButtonBounceEffect>();

            return go;
        }

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
        /// Adds a LayoutElement with a preferred height to a GameObject.
        /// </summary>
        /// <param name="go">The GameObject to add the LayoutElement to.</param>
        /// <param name="prefH">Preferred height for the layout element.</param>
        private static void AddLE(GameObject go, float prefH)
        {
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = prefH;
            le.flexibleHeight = 0;
        }

        /// <summary>
        /// Adds a LayoutElement with preferred height and width to a GameObject.
        /// </summary>
        /// <param name="go">The GameObject to add the LayoutElement to.</param>
        /// <param name="prefH">Preferred height for the layout element.</param>
        /// <param name="prefW">Preferred width for the layout element.</param>
        private static void AddLE(GameObject go, float prefH, float prefW)
        {
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = prefH;
            le.preferredWidth = prefW;
            le.flexibleHeight = 0;
        }
    }
}
