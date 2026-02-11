using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using IOChef.Core;
using IOChef.Gameplay;
using IOChef.Economy;

namespace IOChef.UI
{
    /// <summary>
    /// Mario-style overworld level select with snake path, world navigation,
    /// pinch-to-zoom, drag-to-pan, and auto-focus on current level.
    /// Fully programmatic – no Inspector refs needed.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        // ─── Color palette (matches MainMenuUI) ───

        /// <summary>
        /// Top background gradient color.
        /// </summary>
        private static readonly Color COL_BG_TOP       = new(0.96f, 0.62f, 0.22f);

        /// <summary>
        /// Bottom background gradient color.
        /// </summary>
        private static readonly Color COL_BG_BOT       = new(0.90f, 0.38f, 0.15f);

        /// <summary>
        /// Play button face color.
        /// </summary>
        private static readonly Color COL_PLAY         = new(0.30f, 0.75f, 0.30f);

        /// <summary>
        /// Play button shadow color.
        /// </summary>
        private static readonly Color COL_PLAY_SHADOW  = new(0.15f, 0.50f, 0.15f);

        /// <summary>
        /// General button face color.
        /// </summary>
        private static readonly Color COL_BTN          = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// General button shadow color.
        /// </summary>
        private static readonly Color COL_BTN_SHADOW   = new(0.82f, 0.62f, 0.08f);

        /// <summary>
        /// Button text color.
        /// </summary>
        private static readonly Color COL_BTN_TEXT     = new(0.22f, 0.13f, 0.04f);

        /// <summary>
        /// Title shadow color.
        /// </summary>
        private static readonly Color COL_TITLE_SHADOW = new(0.55f, 0.18f, 0.04f);

        /// <summary>
        /// Quit button face color.
        /// </summary>
        private static readonly Color COL_QUIT         = new(0.82f, 0.22f, 0.18f);

        /// <summary>
        /// Quit button shadow color.
        /// </summary>
        private static readonly Color COL_QUIT_SHADOW  = new(0.58f, 0.12f, 0.10f);

        /// <summary>
        /// Locked node face color.
        /// </summary>
        private static readonly Color COL_LOCKED       = new(0.50f, 0.48f, 0.45f);

        /// <summary>
        /// Locked node shadow color.
        /// </summary>
        private static readonly Color COL_LOCKED_SH    = new(0.35f, 0.33f, 0.30f);

        /// <summary>
        /// Earned star color.
        /// </summary>
        private static readonly Color COL_STAR_ON      = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Unearned star color.
        /// </summary>
        private static readonly Color COL_STAR_OFF     = new(0.55f, 0.50f, 0.45f, 0.5f);

        /// <summary>
        /// Path segment color.
        /// </summary>
        private static readonly Color COL_PATH         = new(0.55f, 0.35f, 0.18f, 0.8f);

        /// <summary>
        /// Open (unlocked) node color.
        /// </summary>
        private static readonly Color COL_NODE_OPEN    = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Completed node color.
        /// </summary>
        private static readonly Color COL_NODE_DONE    = new(0.30f, 0.75f, 0.30f);

        /// <summary>
        /// Disabled arrow color.
        /// </summary>
        private static readonly Color COL_ARROW_DIS    = new(0.60f, 0.55f, 0.50f, 0.4f);

        /// <summary>
        /// Number of columns in the snake grid layout.
        /// </summary>
        private const int COLS = 5;

        /// <summary>
        /// Size of each level node in pixels.
        /// </summary>
        private const float NODE_SIZE = 110f;

        // Zoom constants

        /// <summary>
        /// Minimum allowed zoom level.
        /// </summary>
        private const float MIN_ZOOM = 0.6f;

        /// <summary>
        /// Maximum allowed zoom level.
        /// </summary>
        private const float MAX_ZOOM = 2.5f;

        /// <summary>
        /// Default zoom level on open.
        /// </summary>
        private const float DEFAULT_ZOOM = 1.5f;

        /// <summary>
        /// Zoom speed multiplier for scroll wheel input.
        /// </summary>
        private const float SCROLL_ZOOM_SPEED = 0.15f;

        // Content sizing — large enough to spread nodes out for zoom

        /// <summary>
        /// Width of the scrollable map content area.
        /// </summary>
        private const float CONTENT_W = 1800f;

        /// <summary>
        /// Height of the scrollable map content area.
        /// </summary>
        private const float CONTENT_H = 2800f;

        // Snap to focus level immediately (before animation)

        /// <summary>
        /// Zoom level used when snapping to the focus level.
        /// </summary>
        private const float FOCUS_ZOOM = 1.8f;

        /// <summary>
        /// Root canvas for the level select screen.
        /// </summary>
        private Canvas mainCanvas;

        /// <summary>
        /// RectTransform of the title text for bob animation.
        /// </summary>
        private RectTransform titleRT;

        /// <summary>
        /// Original anchored position of the title before bobbing.
        /// </summary>
        private Vector2 titleOrigPos;

        /// <summary>
        /// Speed of the title bob animation.
        /// </summary>
        private readonly float bobSpeed = 1.8f;

        /// <summary>
        /// Vertical amplitude of the title bob animation.
        /// </summary>
        private readonly float bobAmount = 8f;

        /// <summary>
        /// Currently selected world identifier.
        /// </summary>
        private int _currentWorldId = 1;

        /// <summary>
        /// Total number of worlds available.
        /// </summary>
        private int _worldCount = 1;

        /// <summary>
        /// Level data entries for the current world.
        /// </summary>
        private LevelDataSO[] _worldLevels;

        // Zoom/pan state

        /// <summary>
        /// RectTransform of the zoomable map content.
        /// </summary>
        private RectTransform _mapContent;

        /// <summary>
        /// RectTransform of the map viewport (masked area).
        /// </summary>
        private RectTransform _viewport;

        /// <summary>
        /// ScrollRect used for drag-to-pan navigation.
        /// </summary>
        private ScrollRect _scrollRect;

        /// <summary>
        /// Current zoom scale factor.
        /// </summary>
        private float _currentZoom = DEFAULT_ZOOM;

        /// <summary>
        /// Previous frame pinch distance, or -1 if not pinching.
        /// </summary>
        private float _prevPinchDist = -1f;

        /// <summary>
        /// Index of the level node to auto-focus on.
        /// </summary>
        private int _focusLevelIndex = 0;

        /// <summary>
        /// Cached positions for each level node in content space.
        /// </summary>
        private Vector2[] _nodePositions;

        /// <summary>Builds the level select UI and starts the entrance animation.</summary>
        private void Awake()
        {
            Debug.Log("[LevelSelectUI] v4 Awake - zoomable overworld");
            try { EnsureEventSystem(); BuildUI(); }
            catch (System.Exception e)
            { Debug.LogError($"[LevelSelectUI] {e.Message}\n{e.StackTrace}"); }
        }

        /// <summary>Animates the title bob and handles zoom input each frame.</summary>
        private void Update()
        {
            // Title bob
            if (titleRT != null)
            {
                float y = titleOrigPos.y + Mathf.Sin(Time.unscaledTime * bobSpeed) * bobAmount;
                titleRT.anchoredPosition = new Vector2(titleOrigPos.x, y);
            }

            // Zoom input
            HandleZoomInput();
        }

        // ═══════════════════════════════════════════════════════
        //  ZOOM INPUT
        // ═══════════════════════════════════════════════════════

        /// <summary>Processes scroll wheel and pinch-to-zoom input.</summary>
        private void HandleZoomInput()
        {
            if (_mapContent == null || _scrollRect == null) return;

            // Mouse scroll wheel (desktop)
            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                float newZoom = _currentZoom + scrollDelta * SCROLL_ZOOM_SPEED;
                ApplyZoom(Mathf.Clamp(newZoom, MIN_ZOOM, MAX_ZOOM));
            }

            // Pinch-to-zoom (touch)
            if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                float curDist = Vector2.Distance(t0.position, t1.position);

                if (_prevPinchDist > 0f)
                {
                    float pinchDelta = curDist - _prevPinchDist;
                    float zoomChange = pinchDelta * 0.005f;
                    float newZoom = Mathf.Clamp(_currentZoom + zoomChange, MIN_ZOOM, MAX_ZOOM);
                    ApplyZoom(newZoom);
                }

                _prevPinchDist = curDist;

                // Disable ScrollRect drag while pinching
                _scrollRect.enabled = false;
            }
            else
            {
                _prevPinchDist = -1f;
                if (!_scrollRect.enabled)
                    _scrollRect.enabled = true;
            }
        }

        /// <summary>Applies the given zoom level to the map content.</summary>
        /// <param name="newZoom">The new zoom scale factor to apply.</param>
        private void ApplyZoom(float newZoom)
        {
            if (_mapContent == null || _viewport == null) return;

            Vector2 viewportSize = _viewport.rect.size;
            Vector2 oldPos = _mapContent.anchoredPosition;
            float oldZoom = _currentZoom;

            // Find the point in content space that's currently at viewport center
            // viewportCenter = anchoredPosition + nodeScaled => nodeScaled = viewportCenter - anchoredPosition
            float centerContentX = (viewportSize.x / 2f - oldPos.x) / oldZoom;
            float centerContentY = (viewportSize.y / 2f - oldPos.y) / oldZoom;

            _currentZoom = newZoom;
            _mapContent.localScale = Vector3.one * _currentZoom;

            // Reposition to keep the same content point centered
            float newX = viewportSize.x / 2f - centerContentX * _currentZoom;
            float newY = viewportSize.y / 2f - centerContentY * _currentZoom;

            // Clamp
            float contentScaledW = CONTENT_W * _currentZoom;
            float contentScaledH = CONTENT_H * _currentZoom;
            newX = Mathf.Clamp(newX, viewportSize.x - contentScaledW, 0f);
            newY = Mathf.Clamp(newY, viewportSize.y - contentScaledH, 0f);

            _mapContent.anchoredPosition = new Vector2(newX, newY);
        }

        // ═══════════════════════════════════════════════════════
        //  BUILD
        // ═══════════════════════════════════════════════════════

        /// <summary>Constructs the full level select layout.</summary>
        private void BuildUI()
        {
            // Use server level data when available, fall back to local factory
            if (ServerLevelLoader.Instance != null && ServerLevelLoader.Instance.IsLoaded
                && ServerLevelLoader.Instance.GetLevelsForWorld(_currentWorldId).Length > 0)
            {
                _worldLevels = ServerLevelLoader.Instance.GetLevelsForWorld(_currentWorldId);
                _worldCount = ServerLevelLoader.Instance.GetWorldCount();
            }
            else
            {
                _worldLevels = DefaultLevelFactory.CreateWorld(_currentWorldId);
                _worldCount = DefaultLevelFactory.WorldCount;
            }

            var cgo = new GameObject("LevelSelectCanvas_Prog");
            cgo.transform.SetParent(transform);
            mainCanvas = cgo.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 10;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();
            var root = cgo.GetComponent<RectTransform>();

            BuildBackground(root);
            BuildWorldHeader(root);
            BuildOverworldPath(root);
            BuildBackButton(root);

            SnapToFocusLevel(_focusLevelIndex);
        }

        // ─── Background ───
        /// <summary>Creates the gradient background panels.</summary>
        /// <param name="p">The parent RectTransform to build the background under.</param>
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

        // ─── World Header with navigation arrows ───
        /// <summary>Creates the world title and navigation arrows.</summary>
        /// <param name="p">The parent RectTransform to build the header under.</param>
        private void BuildWorldHeader(RectTransform p)
        {
            string worldName = ServerLevelLoader.Instance != null && ServerLevelLoader.Instance.IsLoaded
                ? (ServerLevelLoader.Instance.GetWorldName(_currentWorldId)
                   ?? DefaultLevelFactory.GetWorldName(_currentWorldId))
                : DefaultLevelFactory.GetWorldName(_currentWorldId);
            string headerText = $"WORLD {_currentWorldId} - {worldName}";

            var sh = MakeText(p, "TitleShadow", headerText, 48, COL_TITLE_SHADOW, FontStyles.Bold);
            AnchorTop(sh, new Vector2(900, 80), new Vector2(4, -42));

            titleRT = MakeText(p, "Title", headerText, 48, Color.white, FontStyles.Bold);
            AnchorTop(titleRT, new Vector2(900, 80), new Vector2(0, -38));
            titleOrigPos = titleRT.anchoredPosition;

            // World nav arrows
            bool canLeft = _currentWorldId > 1;
            bool canRight = _currentWorldId < _worldCount;

            BuildWorldArrow(p, "<", canLeft, -380, () => SwitchWorld(_currentWorldId - 1));
            BuildWorldArrow(p, ">", canRight, 380, () => SwitchWorld(_currentWorldId + 1));

            // Subtitle
            var sub = MakeText(p, "Sub", $"{_worldLevels.Length} Levels", 22,
                new Color(1, 1, 1, 0.70f), FontStyles.Italic);
            AnchorTop(sub, new Vector2(600, 30), new Vector2(0, -120));
        }

        /// <summary>Creates a world navigation arrow button.</summary>
        /// <param name="p">The parent RectTransform to attach the arrow to.</param>
        /// <param name="label">The arrow label text.</param>
        /// <param name="enabled">Whether the arrow is interactable.</param>
        /// <param name="xOffset">The horizontal offset from center.</param>
        /// <param name="onClick">The callback invoked when the arrow is clicked.</param>
        private void BuildWorldArrow(RectTransform p, string label, bool enabled, float xOffset,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject($"Arrow_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(p, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(xOffset, -38);
            rt.sizeDelta = new Vector2(60, 60);

            var img = go.GetComponent<Image>();
            img.color = enabled ? COL_BTN : COL_ARROW_DIS;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.interactable = enabled;
            if (enabled && onClick != null)
            {
                btn.onClick.AddListener(onClick);
                go.AddComponent<ButtonBounceEffect>();
            }

            var lbl = MakeText(rt, "Lbl", label, 32, enabled ? COL_BTN_TEXT : COL_LOCKED, FontStyles.Bold);
            Stretch(lbl);
        }

        /// <summary>Switches the displayed world and rebuilds the path.</summary>
        /// <param name="newWorldId">The world identifier to switch to.</param>
        private void SwitchWorld(int newWorldId)
        {
            if (newWorldId < 1 || newWorldId > _worldCount) return;
            _currentWorldId = newWorldId;
            Destroy(mainCanvas.gameObject);
            BuildUI();
        }

        // ─── Overworld Snake Path (scrollable + zoomable) ───
        /// <summary>Creates the snake path with level nodes and path segments.</summary>
        /// <param name="p">The parent RectTransform to build the path under.</param>
        private void BuildOverworldPath(RectTransform p)
        {
            // Viewport — the visible window for the map (masked)
            var viewportGO = new GameObject("MapViewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(p, false);
            _viewport = viewportGO.GetComponent<RectTransform>();
            _viewport.anchorMin = new Vector2(0.02f, 0.12f);
            _viewport.anchorMax = new Vector2(0.98f, 0.87f);
            _viewport.offsetMin = _viewport.offsetMax = Vector2.zero;

            var vpImg = viewportGO.GetComponent<Image>();
            vpImg.color = new Color(1, 1, 1, 0.01f); // Nearly invisible but needed for Mask
            vpImg.raycastTarget = true;

            var mask = viewportGO.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content — the full map that gets scrolled/zoomed
            var contentGO = new GameObject("MapContent", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            _mapContent = contentGO.GetComponent<RectTransform>();
            _mapContent.anchorMin = _mapContent.anchorMax = new Vector2(0f, 0f);
            _mapContent.pivot = new Vector2(0f, 0f);
            _mapContent.sizeDelta = new Vector2(CONTENT_W, CONTENT_H);

            // ScrollRect for drag panning
            _scrollRect = viewportGO.AddComponent<ScrollRect>();
            _scrollRect.content = _mapContent;
            _scrollRect.horizontal = true;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            _scrollRect.elasticity = 0.1f;
            _scrollRect.inertia = true;
            _scrollRect.decelerationRate = 0.12f;
            _scrollRect.scrollSensitivity = 0f; // Disable scroll wheel on ScrollRect (we handle it for zoom)
            _scrollRect.viewport = _viewport;

            // Calculate node positions (snake pattern, bottom to top)
            int levelCount = _worldLevels.Length;
            int rows = Mathf.CeilToInt((float)levelCount / COLS);

            _nodePositions = new Vector2[levelCount];
            float xSpacing = CONTENT_W / (COLS + 1);
            float ySpacing = CONTENT_H / (rows + 1);

            for (int i = 0; i < levelCount; i++)
            {
                int row = i / COLS;
                int col = i % COLS;
                if (row % 2 == 1) col = (COLS - 1) - col;

                _nodePositions[i] = new Vector2(
                    xSpacing * (col + 1),
                    ySpacing * (row + 1)
                );
            }

            // Draw path segments first (behind nodes)
            for (int i = 0; i < levelCount - 1; i++)
                BuildPathSegment(_mapContent, _nodePositions[i], _nodePositions[i + 1]);

            // Draw level nodes on top and determine focus level
            _focusLevelIndex = 0;
            for (int i = 0; i < levelCount; i++)
            {
                var level = _worldLevels[i];
                bool unlocked = IsLevelUnlocked(level);
                BuildLevelNode(_mapContent, level, _nodePositions[i], unlocked);

                if (unlocked)
                    _focusLevelIndex = i;
            }

            // Apply initial zoom centered on focus level
            _currentZoom = FOCUS_ZOOM;
            _mapContent.localScale = Vector3.one * _currentZoom;
        }

        /// <summary>Draws a path line between two node positions.</summary>
        /// <param name="parent">The parent RectTransform to attach the path segment to.</param>
        /// <param name="from">The starting position of the path segment.</param>
        /// <param name="to">The ending position of the path segment.</param>
        private void BuildPathSegment(RectTransform parent, Vector2 from, Vector2 to)
        {
            var go = new GameObject("PathSeg", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = COL_PATH;
            img.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            Vector2 mid = (from + to) / 2f;
            Vector2 diff = to - from;
            float length = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = mid;
            rt.sizeDelta = new Vector2(length, 10f);
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            // Small circle at connection point
            var dot = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(parent, false);
            dot.GetComponent<Image>().color = COL_PATH;
            dot.GetComponent<Image>().raycastTarget = false;
            var drt = dot.GetComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = Vector2.zero;
            drt.pivot = new Vector2(0.5f, 0.5f);
            drt.anchoredPosition = from;
            drt.sizeDelta = new Vector2(16, 16);
        }

        /// <summary>Creates a level node with stars, number, and click handler.</summary>
        /// <param name="parent">The parent RectTransform to attach the node to.</param>
        /// <param name="level">The level data for this node.</param>
        /// <param name="position">The position of the node in content space.</param>
        /// <param name="unlocked">Whether this level is unlocked.</param>
        private void BuildLevelNode(RectTransform parent, LevelDataSO level, Vector2 position, bool unlocked)
        {
            int bestStars = PlayerPrefs.GetInt($"Level_{level.levelId}_Stars", 0);
            bool hasStars = bestStars > 0;

            Color face = !unlocked ? COL_LOCKED
                       : hasStars  ? COL_NODE_DONE
                       : COL_NODE_OPEN;
            Color shadow = !unlocked ? COL_LOCKED_SH
                         : hasStars  ? COL_PLAY_SHADOW
                         : COL_BTN_SHADOW;

            var go = new GameObject($"Node_{level.levelId}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(NODE_SIZE, NODE_SIZE);

            var img = go.GetComponent<Image>();
            img.color = face;

            // Shadow strip
            int borderH = 6;
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

            // Level display ID (e.g., "1-3")
            var numTxt = MakeText(rt, "Num", level.DisplayId, 28, Color.white, FontStyles.Bold);
            numTxt.anchorMin = new Vector2(0, 0.35f);
            numTxt.anchorMax = new Vector2(1, 1);
            numTxt.offsetMin = numTxt.offsetMax = Vector2.zero;
            numTxt.GetComponent<TextMeshProUGUI>().raycastTarget = false;

            // Star row
            var starRow = new GameObject("Stars", typeof(RectTransform));
            starRow.transform.SetParent(rt, false);
            var srRT = starRow.GetComponent<RectTransform>();
            srRT.anchorMin = new Vector2(0, 0);
            srRT.anchorMax = new Vector2(1, 0.38f);
            srRT.offsetMin = new Vector2(0, borderH);
            srRT.offsetMax = Vector2.zero;

            var hlg = starRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 2;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            for (int s = 1; s <= 3; s++)
            {
                Color starC = (unlocked && s <= bestStars) ? COL_STAR_ON : COL_STAR_OFF;
                var star = MakeText(srRT, $"S{s}", "*", 20, starC, FontStyles.Bold);
                star.GetComponent<TextMeshProUGUI>().raycastTarget = false;
                var le = star.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 24;
                le.preferredWidth = 24;
            }

            // Level name label below node
            var nameLbl = MakeText(parent, $"Name_{level.levelId}", level.levelName,
                16, new Color(1, 1, 1, 0.75f), FontStyles.Normal);
            nameLbl.anchorMin = nameLbl.anchorMax = Vector2.zero;
            nameLbl.pivot = new Vector2(0.5f, 1f);
            nameLbl.anchoredPosition = new Vector2(position.x, position.y - NODE_SIZE / 2 - 4);
            nameLbl.sizeDelta = new Vector2(140, 22);
            nameLbl.GetComponent<TextMeshProUGUI>().raycastTarget = false;
            nameLbl.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
            nameLbl.GetComponent<TextMeshProUGUI>().fontSizeMin = 10;
            nameLbl.GetComponent<TextMeshProUGUI>().fontSizeMax = 16;

            // Button behavior
            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = img;

            if (unlocked)
            {
                int levelId = level.levelId;
                int worldId = level.worldId;
                btn.onClick.AddListener(() => OnLevelClicked(worldId, levelId));
                go.AddComponent<ButtonBounceEffect>();
            }
            else
            {
                btn.interactable = false;
            }
        }

        /// <summary>Checks if a level is unlocked based on saved progress.</summary>
        /// <param name="level">The level data to check.</param>
        /// <returns>True if the level is unlocked; otherwise, false.</returns>
        private bool IsLevelUnlocked(LevelDataSO level)
        {
            if (level.levelNumber == 1 && level.worldId == 1) return true;

            // Primary gate: server-synced MaxUnlockedLevel
            int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
            if (level.levelId > maxUnlocked) return false;

            // Secondary: previous level must have stars
            int prevLevelId = level.levelId - 1;
            int prevStars = PlayerPrefs.GetInt($"Level_{prevLevelId}_Stars", 0);
            return prevStars > 0;
        }

        // ─── Back Button ───
        /// <summary>Creates the back-to-menu button.</summary>
        /// <param name="p">The parent RectTransform to build the button under.</param>
        private void BuildBackButton(RectTransform p)
        {
            var wrap = MakePanel(p, "BackWrap", Color.clear);
            wrap.anchorMin = new Vector2(0.25f, 0.02f);
            wrap.anchorMax = new Vector2(0.75f, 0.10f);
            wrap.offsetMin = wrap.offsetMax = Vector2.zero;

            var vlg = wrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            MakeChunkyButton(wrap, "BACK", COL_QUIT, COL_QUIT_SHADOW, Color.white, 28, 70, OnBackClicked);
        }

        // ═══════════════════════════════════════════════════════
        //  CALLBACKS
        // ═══════════════════════════════════════════════════════

        /// <summary>Handles a level node click, opening the ingredient shop for the selected level.</summary>
        /// <param name="worldId">The world identifier of the clicked level.</param>
        /// <param name="levelId">The level identifier of the clicked level.</param>
        private void OnLevelClicked(int worldId, int levelId)
        {
            Debug.Log($"[LevelSelectUI] Opening shop for World {worldId} Level {levelId}");
            LevelDataSO levelData = null;

            if (ServerLevelLoader.Instance != null && ServerLevelLoader.Instance.IsLoaded)
                levelData = ServerLevelLoader.Instance.GetLevel(levelId);

            levelData ??= DefaultLevelFactory.Create(levelId);

            if (levelData != null)
                IngredientShopUI.Show(worldId, levelId, levelData);
        }

        /// <summary>Returns to the main menu scene.</summary>
        private void OnBackClicked()
        {
            Debug.Log("[LevelSelectUI] Back to main menu");
            if (GameManager.Instance != null)
                GameManager.Instance.LoadMainMenu();
        }

        // ═══════════════════════════════════════════════════════
        //  ANIMATION + AUTO-FOCUS
        // ═══════════════════════════════════════════════════════


        /// <summary>Scrolls the map to center on the specified level node.</summary>
        /// <param name="levelIndex">The index of the level node to center on.</param>
        private void SnapToFocusLevel(int levelIndex)
        {
            if (_mapContent == null || _viewport == null || _nodePositions == null) return;
            if (levelIndex < 0 || levelIndex >= _nodePositions.Length) return;

            Canvas.ForceUpdateCanvases();

            Vector2 targetPos = GetContentPositionForNode(levelIndex);
            _mapContent.anchoredPosition = targetPos;
        }

        /// <summary>Calculates the scroll position to center on a node.</summary>
        /// <param name="index">The index of the node to center on.</param>
        /// <returns>The content anchored position that centers the node in the viewport.</returns>
        private Vector2 GetContentPositionForNode(int index)
        {
            Vector2 nodePos = _nodePositions[index];
            Vector2 viewportSize = _viewport.rect.size;

            // Node position in scaled space (from content bottom-left)
            float nodeScaledX = nodePos.x * _currentZoom;
            float nodeScaledY = nodePos.y * _currentZoom;

            // Content total scaled size
            float contentScaledW = CONTENT_W * _currentZoom;
            float contentScaledH = CONTENT_H * _currentZoom;

            // With anchor at (0,0) and pivot at (0,0):
            // anchoredPosition (0,0) means content bottom-left is at viewport bottom-left
            // To center nodePos in viewport:
            // viewport center = viewportSize / 2
            // We need: anchoredPosition.x + nodeScaledX = viewportSize.x / 2
            // So: anchoredPosition.x = viewportSize.x / 2 - nodeScaledX
            float targetX = viewportSize.x / 2f - nodeScaledX;
            float targetY = viewportSize.y / 2f - nodeScaledY;

            // Clamp so content edges don't go past viewport edges
            float minX = viewportSize.x - contentScaledW; // most we can scroll left
            float maxX = 0f;                                // content left at viewport left
            float minY = viewportSize.y - contentScaledH;
            float maxY = 0f;

            targetX = Mathf.Clamp(targetX, minX, maxX);
            targetY = Mathf.Clamp(targetY, minY, maxY);

            return new Vector2(targetX, targetY);
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
        private RectTransform MakePanel(RectTransform parent, string name, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = c;
            return go.GetComponent<RectTransform>();
        }

        /// <summary>Creates a TextMeshProUGUI text element.</summary>
        /// <param name="parent">The parent RectTransform to attach the text to.</param>
        /// <param name="name">The name of the text GameObject.</param>
        /// <param name="text">The initial text content.</param>
        /// <param name="size">The font size.</param>
        /// <param name="c">The text color.</param>
        /// <param name="style">The font style.</param>
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

        /// <summary>Creates a styled chunky button with shadow.</summary>
        /// <param name="parent">The parent RectTransform to attach the button to.</param>
        /// <param name="label">The text label displayed on the button.</param>
        /// <param name="face">The button face color.</param>
        /// <param name="shadow">The button shadow color.</param>
        /// <param name="txt">The button text color.</param>
        /// <param name="size">The font size.</param>
        /// <param name="h">The button height in pixels.</param>
        /// <param name="onClick">The callback invoked when the button is clicked.</param>
        private void MakeChunkyButton(RectTransform parent, string label, Color face, Color shadow,
            Color txt, int size, int h, UnityEngine.Events.UnityAction onClick)
        {
            int borderH = Mathf.Max(5, h / 10);

            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = h;
            le.flexibleHeight = 0;

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
        }

        /// <summary>Stretches a RectTransform to fill its parent.</summary>
        /// <param name="rt">The RectTransform to stretch.</param>
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// <summary>Anchors a RectTransform to the top-center with given size and position.</summary>
        /// <param name="rt">The RectTransform to anchor.</param>
        /// <param name="size">The size of the RectTransform.</param>
        /// <param name="pos">The anchored position offset from the top-center.</param>
        private static void AnchorTop(RectTransform rt, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = size; rt.anchoredPosition = pos;
        }
    }
}
