using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using IOChef.Gameplay;

namespace IOChef.UI
{
    /// <summary>
    /// Fully programmatic Results screen – Overcooked style, no Inspector refs.
    /// Shows current run stats, star thresholds, NEW BEST badge.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        // ─── Color palette ───

        /// <summary>
        /// Background gradient top color.
        /// </summary>
        private static readonly Color COL_BG_TOP       = new(0.96f, 0.62f, 0.22f);

        /// <summary>
        /// Background gradient bottom color.
        /// </summary>
        private static readonly Color COL_BG_BOT       = new(0.90f, 0.38f, 0.15f);

        /// <summary>
        /// Play / next-level button face color.
        /// </summary>
        private static readonly Color COL_PLAY         = new(0.30f, 0.75f, 0.30f);

        /// <summary>
        /// Play button shadow color.
        /// </summary>
        private static readonly Color COL_PLAY_SHADOW  = new(0.15f, 0.50f, 0.15f);

        /// <summary>
        /// Generic button face color.
        /// </summary>
        private static readonly Color COL_BTN          = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Generic button shadow color.
        /// </summary>
        private static readonly Color COL_BTN_SHADOW   = new(0.82f, 0.62f, 0.08f);

        /// <summary>
        /// Generic button text color.
        /// </summary>
        private static readonly Color COL_BTN_TEXT     = new(0.22f, 0.13f, 0.04f);

        /// <summary>
        /// Quit button face color.
        /// </summary>
        private static readonly Color COL_QUIT         = new(0.82f, 0.22f, 0.18f);

        /// <summary>
        /// Quit button shadow color.
        /// </summary>
        private static readonly Color COL_QUIT_SHADOW  = new(0.58f, 0.12f, 0.10f);

        /// <summary>
        /// Title text shadow color.
        /// </summary>
        private static readonly Color COL_TITLE_SHADOW = new(0.55f, 0.18f, 0.04f);

        /// <summary>
        /// Earned star fill color.
        /// </summary>
        private static readonly Color COL_STAR_ON      = new(1f, 0.84f, 0.22f);

        /// <summary>
        /// Unearned star color.
        /// </summary>
        private static readonly Color COL_STAR_OFF     = new(0.45f, 0.40f, 0.35f);

        /// <summary>
        /// Score card background color.
        /// </summary>
        private static readonly Color COL_CARD_BG      = new(0.92f, 0.72f, 0.42f, 0.55f);

        /// <summary>
        /// Stat label text color.
        /// </summary>
        private static readonly Color COL_STAT_LABEL   = new(0.50f, 0.38f, 0.22f, 0.85f);

        /// <summary>
        /// Stat value text color.
        /// </summary>
        private static readonly Color COL_STAT_VALUE   = new(0.22f, 0.13f, 0.04f);

        /// <summary>
        /// New best score highlight color.
        /// </summary>
        private static readonly Color COL_NEW_BEST     = new(1f, 0.30f, 0.15f);

        /// <summary>
        /// Star threshold text color.
        /// </summary>
        private static readonly Color COL_THRESHOLD    = new(0.55f, 0.42f, 0.28f, 0.7f);

        /// <summary>
        /// World map button face color.
        /// </summary>
        private static readonly Color COL_MAP_BTN      = new(0.25f, 0.55f, 0.80f);

        /// <summary>
        /// World map button shadow color.
        /// </summary>
        private static readonly Color COL_MAP_BTN_SH   = new(0.15f, 0.38f, 0.58f);

        /// <summary>
        /// Builds the results screen UI on load.
        /// </summary>
        private void Awake()
        {
            Debug.Log("[ResultsScreenUI] v3 Awake - stats + thresholds");
            var existing = GetComponent<Canvas>();
            if (existing != null) existing.enabled = false;

            try { EnsureEventSystem(); BuildUI(); }
            catch (System.Exception e)
            { Debug.LogError($"[ResultsScreenUI] {e.Message}\n{e.StackTrace}"); }
        }

        /// <summary>
        /// Constructs the full results screen layout with stats and buttons.
        /// </summary>
        private void BuildUI()
        {
            var cgo = new GameObject("ResultsCanvas_Prog");
            cgo.transform.SetParent(transform);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();
            var root = cgo.GetComponent<RectTransform>();

            // Background
            var bg = MakePanel(root, "Bg", COL_BG_TOP); Stretch(bg);
            var bgBot = MakePanel(root, "BgBot", new Color(COL_BG_BOT.r, COL_BG_BOT.g, COL_BG_BOT.b, 0.55f));
            Stretch(bgBot);
            bgBot.anchorMin = new Vector2(0, 0);
            bgBot.anchorMax = new Vector2(1, 0.35f);
            bgBot.offsetMin = bgBot.offsetMax = Vector2.zero;

            // Get results from static pass-through (current run data)
            int levelId = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentLevelId : 1;
            var results = LevelController.LastRunResults;
            int score = results.finalScore;
            int stars = results.starRating;
            bool isNewBest = LevelController.LastRunWasNewBest;

            var levelData = DefaultLevelFactory.Create(levelId);

            // Main content panel
            var content = MakePanel(root, "Content", Color.clear);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.55f);
            content.sizeDelta = new Vector2(580, 0);

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title
            string titleText = stars >= 1 ? "LEVEL COMPLETE!" : "TIME'S UP!";
            var shadowTx = MakeText(root, "TitleSh", titleText, 54, COL_TITLE_SHADOW, FontStyles.Bold);
            AnchorTop(shadowTx, new Vector2(700, 80), new Vector2(3, -42));
            var titleTx = MakeText(root, "Title", titleText, 54, Color.white, FontStyles.Bold);
            AnchorTop(titleTx, new Vector2(700, 80), new Vector2(0, -38));

            // Level name
            string levelLabel = levelData != null
                ? $"Level {levelData.DisplayId}: {levelData.levelName}"
                : $"Level {levelId}";
            AddLayoutText(content, "LL", levelLabel, 20, new Color(1, 1, 1, 0.7f), FontStyles.Italic, 28);

            // Stars row
            var starsRow = MakePanel(content, "StarsRow", Color.clear);
            AddLE(starsRow.gameObject, 80);
            var shlg = starsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            shlg.spacing = 20;
            shlg.childAlignment = TextAnchor.MiddleCenter;
            shlg.childForceExpandWidth = false;
            shlg.childForceExpandHeight = false;
            shlg.childControlWidth = true;
            shlg.childControlHeight = true;

            for (int i = 1; i <= 3; i++)
            {
                Color c = i <= stars ? COL_STAR_ON : COL_STAR_OFF;
                var star = MakeText(starsRow, $"Star{i}", "*", 64, c, FontStyles.Bold);
                AddLE(star.gameObject, 70, 70);
            }

            // Star thresholds
            if (levelData != null)
            {
                string thresholds = $"* {levelData.threshold1Star}   ** {levelData.threshold2Star}   *** {levelData.threshold3Star}";
                AddLayoutText(content, "Thresh", thresholds, 16, COL_THRESHOLD, FontStyles.Normal, 22);
            }

            // NEW BEST badge
            if (isNewBest)
            {
                AddLayoutText(content, "NewBest", "NEW BEST!", 28, COL_NEW_BEST, FontStyles.Bold, 36);
            }

            // Score card
            var card = MakePanel(content, "ScoreCard", COL_CARD_BG);
            AddLE(card.gameObject, 240);
            var cvlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            cvlg.spacing = 4;
            cvlg.childAlignment = TextAnchor.MiddleCenter;
            cvlg.childForceExpandWidth = true;
            cvlg.childForceExpandHeight = false;
            cvlg.childControlWidth = true;
            cvlg.childControlHeight = true;
            cvlg.padding = new RectOffset(20, 20, 16, 16);

            AddLayoutText(card, "SL", "FINAL SCORE", 18, COL_STAT_LABEL, FontStyles.Normal, 24);
            AddLayoutText(card, "SV", score.ToString("N0"), 52, COL_STAT_VALUE, FontStyles.Bold, 60);

            // Stats row
            var statsRow = MakePanel(card, "StatsRow", Color.clear);
            AddLE(statsRow.gameObject, 70);
            var statsHlg = statsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            statsHlg.spacing = 8;
            statsHlg.childAlignment = TextAnchor.MiddleCenter;
            statsHlg.childForceExpandWidth = true;
            statsHlg.childForceExpandHeight = false;
            statsHlg.childControlWidth = true;
            statsHlg.childControlHeight = true;

            BuildStatBox(statsRow, "ORDERS", results.ordersCompleted.ToString(), COL_PLAY);
            BuildStatBox(statsRow, "FAILED", results.ordersFailed.ToString(), COL_QUIT);
            BuildStatBox(statsRow, "COMBO", $"x{results.bestCombo}", COL_BTN);

            // Spacer
            var sp = MakePanel(content, "Sp", Color.clear);
            AddLE(sp.gameObject, 10);

            // Buttons
            MakeChunkyButton(content, "RETRY", COL_BTN, COL_BTN_SHADOW, COL_BTN_TEXT, 28, 72, OnRetryClicked);

            if (stars > 0)
                MakeChunkyButton(content, "NEXT LEVEL", COL_PLAY, COL_PLAY_SHADOW, Color.white, 28, 72, OnNextLevelClicked);

            MakeChunkyButton(content, "WORLD MAP", COL_MAP_BTN, COL_MAP_BTN_SH, Color.white, 24, 60, OnWorldMapClicked);
            MakeChunkyButton(content, "MAIN MENU", COL_QUIT, COL_QUIT_SHADOW, Color.white, 22, 56, OnMenuClicked);
        }

        /// <summary>
        /// Creates an individual stat display box.
        /// </summary>
        /// <param name="parent">Parent RectTransform for the stat box.</param>
        /// <param name="label">Label text for the stat.</param>
        /// <param name="value">Display value of the stat.</param>
        /// <param name="accent">Accent color for the value text.</param>
        private void BuildStatBox(RectTransform parent, string label, string value, Color accent)
        {
            var box = MakePanel(parent, $"Stat_{label}", new Color(0, 0, 0, 0.12f));
            var le = box.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.preferredHeight = 65;

            var boxVlg = box.gameObject.AddComponent<VerticalLayoutGroup>();
            boxVlg.spacing = 2;
            boxVlg.childAlignment = TextAnchor.MiddleCenter;
            boxVlg.childForceExpandWidth = true;
            boxVlg.childForceExpandHeight = false;
            boxVlg.childControlWidth = true;
            boxVlg.childControlHeight = true;
            boxVlg.padding = new RectOffset(4, 4, 6, 6);

            AddLayoutText(box, "V", value, 28, accent, FontStyles.Bold, 34);
            AddLayoutText(box, "L", label, 13, COL_STAT_LABEL, FontStyles.Normal, 18);
        }

        // ─── Callbacks ───
        /// <summary>
        /// Reloads the current level scene.
        /// </summary>
        private void OnRetryClicked()
        {
            int levelId = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentLevelId : 1;
            Core.GameManager.Instance?.LoadLevel(levelId);
        }

        /// <summary>
        /// Advances to the next level and loads it.
        /// </summary>
        private void OnNextLevelClicked()
        {
            int currentId = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentLevelId : 1;
            var nextLevel = DefaultLevelFactory.Create(currentId + 1);
            if (nextLevel != null)
            {
                Core.GameManager.Instance?.SetCurrentWorld(nextLevel.worldId);
                Core.GameManager.Instance?.LoadLevel(nextLevel.levelId);
            }
            else
            {
                Core.GameManager.Instance?.LoadLevelSelect();
            }
        }

        /// <summary>
        /// Returns to the level select screen.
        /// </summary>
        private void OnWorldMapClicked()
        {
            Core.GameManager.Instance?.LoadLevelSelect();
        }

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        private void OnMenuClicked()
        {
            Core.GameManager.Instance?.LoadMainMenu();
        }

        // ─── Factory helpers ───
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
        /// Adds a text element with a layout element.
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
        /// Adds a LayoutElement with preferred dimensions.
        /// </summary>
        /// <param name="go">The GameObject to add the LayoutElement to.</param>
        /// <param name="prefH">Preferred height for the layout element.</param>
        /// <param name="prefW">Preferred width for the layout element (-1 to skip).</param>
        private static void AddLE(GameObject go, float prefH, float prefW = -1)
        {
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = prefH;
            le.flexibleHeight = 0;
            if (prefW > 0) le.preferredWidth = prefW;
        }

        /// <summary>
        /// Creates a styled chunky button with shadow.
        /// </summary>
        /// <param name="parent">Parent RectTransform to attach the button to.</param>
        /// <param name="label">Button label text.</param>
        /// <param name="face">Face color of the button.</param>
        /// <param name="shadow">Shadow strip color.</param>
        /// <param name="txt">Text color.</param>
        /// <param name="size">Font size.</param>
        /// <param name="h">Button height.</param>
        /// <param name="onClick">Click callback action.</param>
        private void MakeChunkyButton(RectTransform parent, string label, Color face, Color shadow,
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
        /// Anchors a RectTransform to the top-center.
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
    }
}
