using UnityEngine;
using System.Collections.Generic;
using IOChef.Gameplay;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Creates an Overcooked-style 2D top-down kitchen at runtime.
    /// Now data-driven: reads equipment layout from LevelDataSO
    /// and adapts grid size, camera, and stations per level.
    /// </summary>
    public class KitchenVisualSetup : MonoBehaviour
    {
        // ─── Palette (Overcooked 2 inspired) ───

        /// <summary>
        /// Floor tile color (light variant).
        /// </summary>
        private static readonly Color COL_TILE_A     = new(0.78f, 0.68f, 0.56f);

        /// <summary>
        /// Floor tile color (dark variant).
        /// </summary>
        private static readonly Color COL_TILE_B     = new(0.72f, 0.62f, 0.50f);

        /// <summary>
        /// Counter body color.
        /// </summary>
        private static readonly Color COL_COUNTER    = new(0.75f, 0.18f, 0.15f);

        /// <summary>
        /// Counter edge accent color.
        /// </summary>
        private static readonly Color COL_COUNTER_ED = new(0.60f, 0.12f, 0.10f);

        /// <summary>
        /// Counter top surface color.
        /// </summary>
        private static readonly Color COL_COUNTER_TP = new(0.88f, 0.82f, 0.72f);

        /// <summary>
        /// Wood accent color (light variant).
        /// </summary>
        private static readonly Color COL_WOOD       = new(0.55f, 0.38f, 0.22f);

        /// <summary>
        /// Wood accent color (dark variant).
        /// </summary>
        private static readonly Color COL_WOOD_DARK  = new(0.40f, 0.26f, 0.14f);

        /// <summary>
        /// Wall background color.
        /// </summary>
        private static readonly Color COL_WALL       = new(0.58f, 0.46f, 0.34f);

        /// <summary>
        /// Wall trim highlight color.
        /// </summary>
        private static readonly Color COL_WALL_TRIM  = new(0.72f, 0.42f, 0.18f);

        /// <summary>
        /// Player character body color.
        /// </summary>
        private static readonly Color COL_PLAYER     = new(0.25f, 0.70f, 0.92f);

        /// <summary>
        /// Chef hat color.
        /// </summary>
        private static readonly Color COL_CHEF_HAT   = new(0.98f, 0.98f, 0.96f);

        /// <summary>
        /// Chef skin tone color.
        /// </summary>
        private static readonly Color COL_CHEF_SKIN  = new(0.96f, 0.82f, 0.68f);

        /// <summary>
        /// Drop shadow color with transparency.
        /// </summary>
        private static readonly Color COL_SHADOW     = new(0f, 0f, 0f, 0.18f);

        // Station colors

        /// <summary>
        /// Cutting station surface color.
        /// </summary>
        private static readonly Color COL_CUT        = new(0.88f, 0.85f, 0.78f);

        /// <summary>
        /// Cooktop station surface color.
        /// </summary>
        private static readonly Color COL_COOK       = new(0.35f, 0.35f, 0.40f);

        /// <summary>
        /// Cooktop flame indicator color.
        /// </summary>
        private static readonly Color COL_COOK_FLAME = new(1f, 0.55f, 0.15f);

        /// <summary>
        /// Plating station surface color.
        /// </summary>
        private static readonly Color COL_PLATE      = new(0.95f, 0.95f, 0.90f);

        /// <summary>
        /// Serve point station color.
        /// </summary>
        private static readonly Color COL_SERVE      = new(0.20f, 0.72f, 0.35f);

        /// <summary>
        /// Serve point arrow indicator color.
        /// </summary>
        private static readonly Color COL_SERVE_ARR  = new(1f, 1f, 1f);

        /// <summary>
        /// Trash bin station color.
        /// </summary>
        private static readonly Color COL_TRASH      = new(0.50f, 0.50f, 0.50f);

        /// <summary>
        /// Sink station color.
        /// </summary>
        private static readonly Color COL_SINK       = new(0.55f, 0.75f, 0.88f);

        /// <summary>
        /// Mapping of ingredient types to their visual color and display label for source crates.
        /// </summary>
        private static readonly Dictionary<IngredientType, (Color color, string label)> SourceVisuals = new()
        {
            { IngredientType.Lettuce, (new Color(0.40f, 0.72f, 0.20f), "LETTUCE") },
            { IngredientType.Tomato,  (new Color(0.90f, 0.22f, 0.15f), "TOMATO") },
            { IngredientType.Meat,    (new Color(0.72f, 0.30f, 0.22f), "MEAT") },
        };

        /// <summary>
        /// Cell size in world units for the kitchen grid.
        /// </summary>
        private const float CS = 1f;

        /// <summary>
        /// Cached square sprite used for rectangular visual elements.
        /// </summary>
        private static Sprite _sqSprite;

        /// <summary>
        /// Cached circle sprite used for round visual elements.
        /// </summary>
        private static Sprite _circSprite;

        /// <summary>
        /// Width of the kitchen grid in cells.
        /// </summary>
        private int _gridW;

        /// <summary>
        /// Height of the kitchen grid in cells.
        /// </summary>
        private int _gridH;

        /// <summary>
        /// Creates shared sprite assets on initialization.
        /// </summary>
        private void Awake()
        {
            CreateSprites();
        }

        /// <summary>
        /// Build the kitchen from level data. Called by LevelController after level data is loaded.
        /// </summary>
        /// <param name="levelData">The level configuration containing grid size and equipment layout.</param>
        public void BuildKitchen(LevelDataSO levelData)
        {
            _gridW = levelData.gridWidth;
            _gridH = levelData.gridHeight;

            Debug.Log($"[KitchenVisualSetup] Building {_gridW}x{_gridH} kitchen for '{levelData.levelName}' " +
                      $"({levelData.equipment.Count} equipment spawns)");

            CreateSprites();
            SetupCamera();
            DrawFloor();
            DrawWalls();
            LayoutKitchenFromData(levelData);
            SetupPlayer();
        }

        // ═══════════════════════════════════════════════════════
        //  SPRITES
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Generates the square and circle sprites used by all kitchen visuals.
        /// </summary>
        private static void CreateSprites()
        {
            if (_sqSprite != null) return;

            var sqTex = new Texture2D(4, 4);
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            sqTex.SetPixels(px);
            sqTex.filterMode = FilterMode.Point;
            sqTex.Apply();
            _sqSprite = Sprite.Create(sqTex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

            int sz = 32;
            var cTex = new Texture2D(sz, sz);
            float r = sz / 2f - 1;
            float cx = sz / 2f, cy = sz / 2f;
            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    cTex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            cTex.filterMode = FilterMode.Bilinear;
            cTex.Apply();
            _circSprite = Sprite.Create(cTex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), (float)sz);
        }

        // ═══════════════════════════════════════════════════════
        //  CAMERA (adapts to grid size)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Configures the camera for the kitchen grid dimensions.
        /// </summary>
        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = _gridH * 0.7f;
            cam.backgroundColor = new Color(0.12f, 0.10f, 0.08f);
            cam.transform.position = new Vector3(_gridW * CS / 2f, _gridH * CS / 2f - 0.3f, -10f);
        }

        // ═══════════════════════════════════════════════════════
        //  FLOOR (adapts to grid size)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws the checkerboard floor tiles and border.
        /// </summary>
        private void DrawFloor()
        {
            var parent = new GameObject("Floor").transform;
            for (int x = 0; x < _gridW; x++)
                for (int y = 0; y < _gridH; y++)
                {
                    Color c = ((x + y) % 2 == 0) ? COL_TILE_A : COL_TILE_B;
                    MkSq(parent, $"T{x}_{y}", x + 0.5f, y + 0.5f, CS * 0.98f, CS * 0.98f, c, -10);
                }

            MkSq(null, "FloorBorder", _gridW / 2f, _gridH / 2f, _gridW + 0.4f, _gridH + 0.4f,
                new Color(0.32f, 0.24f, 0.16f), -11);
        }

        // ═══════════════════════════════════════════════════════
        //  WALLS (adapts to grid size)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws the wall panels and collision boundaries.
        /// </summary>
        private void DrawWalls()
        {
            var parent = new GameObject("Walls").transform;

            MkSq(parent, "WallTop", _gridW / 2f, _gridH + 0.25f, _gridW + 0.4f, 0.5f, COL_WALL, -5);
            MkSq(parent, "WallTrim", _gridW / 2f, _gridH - 0.02f, _gridW + 0.4f, 0.15f, COL_WALL_TRIM, -4);
            MkSq(parent, "WallL", -0.15f, _gridH / 2f, 0.3f, _gridH + 0.9f, COL_WALL, -5);
            MkSq(parent, "WallR", _gridW + 0.15f, _gridH / 2f, 0.3f, _gridH + 0.9f, COL_WALL, -5);
            MkSq(parent, "WallBot", _gridW / 2f, -0.25f, _gridW + 0.4f, 0.5f, COL_WOOD_DARK, -5);

            AddWallCollider(parent, "WC_Top",   _gridW / 2f, _gridH + 0.3f, _gridW + 1f, 1f);
            AddWallCollider(parent, "WC_Bot",   _gridW / 2f, -0.3f,         _gridW + 1f, 1f);
            AddWallCollider(parent, "WC_Left",  -0.3f,       _gridH / 2f,   1f, _gridH + 2f);
            AddWallCollider(parent, "WC_Right", _gridW + 0.3f, _gridH / 2f, 1f, _gridH + 2f);
        }

        /// <summary>
        /// Creates a wall collision box at the specified position.
        /// </summary>
        /// <param name="parent">Parent transform for the collider.</param>
        /// <param name="name">Name of the collider GameObject.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        /// <param name="w">Width of the collider.</param>
        /// <param name="h">Height of the collider.</param>
        private static void AddWallCollider(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, y, 0);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            col.isTrigger = false;
        }

        // ═══════════════════════════════════════════════════════
        //  DATA-DRIVEN KITCHEN LAYOUT
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Places all equipment from the level data onto the kitchen grid.
        /// </summary>
        /// <param name="levelData">The level configuration with equipment layout.</param>
        private void LayoutKitchenFromData(LevelDataSO levelData)
        {
            var counters = new GameObject("Counters").transform;
            var stations = new GameObject("Stations").transform;

            foreach (var eq in levelData.equipment)
            {
                float x = eq.gridX + 0.5f;
                float y = eq.gridY + 0.5f;

                // Draw counter visual at every equipment position
                DrawCounter(counters, x, y);

                // Create the station component on top
                switch (eq.equipmentType)
                {
                    case EquipmentType.IngredientSource:
                        CreateSource(x, y, eq.sourceIngredient, stations);
                        break;
                    case EquipmentType.CuttingBoard:
                        CreateStation<CuttingBoard>(x, y, stations, COL_CUT, "CUT", DrawCuttingIcon);
                        break;
                    case EquipmentType.Cooktop:
                        CreateStation<Cooktop>(x, y, stations, COL_COOK, "COOK", DrawStoveIcon);
                        break;
                    case EquipmentType.PlatingStation:
                        CreateStation<PlatingStation>(x, y, stations, COL_PLATE, "PLATE", DrawPlateIcon);
                        break;
                    case EquipmentType.ServePoint:
                        CreateStation<ServePoint>(x, y, stations, COL_SERVE, "SERVE", DrawServeIcon);
                        break;
                    case EquipmentType.TrashBin:
                        CreateStation<TrashBin>(x, y, stations, COL_TRASH, "BIN", DrawTrashIcon);
                        break;
                    case EquipmentType.Sink:
                        CreateStation<Sink>(x, y, stations, COL_SINK, "SINK", DrawSinkIcon);
                        break;
                    case EquipmentType.Counter:
                        CreateWorkCounter(x, y);
                        break;
                }
            }
        }

        /// <summary>
        /// Draws a counter visual with collision at the grid position.
        /// </summary>
        /// <param name="parent">Parent transform for the counter.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void DrawCounter(Transform parent, float x, float y)
        {
            MkSq(parent, "C", x, y, 0.92f, 0.92f, COL_COUNTER, 0);
            MkSq(parent, "CE", x, y - 0.38f, 0.92f, 0.16f, COL_COUNTER_ED, 1);
            MkSq(parent, "CT", x, y + 0.08f, 0.80f, 0.65f, COL_COUNTER_TP, 2);

            var blocker = new GameObject("Block");
            blocker.transform.SetParent(parent);
            blocker.transform.position = new Vector3(x, y, 0);
            var bc = blocker.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(0.82f, 0.82f);
            bc.isTrigger = false;
        }

        // ═══════════════════════════════════════════════════════
        //  STATION CREATION (programmatic, no FindAnyObjectByType)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Creates a station game object with collider, visual, and label.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour type of the station component.</typeparam>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        /// <param name="parent">Parent transform for the station icon.</param>
        /// <param name="stationColor">Fallback color for the station icon.</param>
        /// <param name="label">Display label for the station.</param>
        /// <param name="drawIcon">Callback to draw the station-specific icon.</param>
        /// <returns>The created station MonoBehaviour component.</returns>
        private T CreateStation<T>(float x, float y, Transform parent, Color stationColor,
            string label, System.Action<Transform, float, float> drawIcon) where T : MonoBehaviour
        {
            var go = new GameObject($"Station_{label}");
            go.transform.position = new Vector3(x, y, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sqSprite;
            sr.color = Color.clear;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.9f);

            var station = go.AddComponent<T>();

            // Station icon on counter
            var iconParent = new GameObject($"Icon_{label}").transform;
            iconParent.SetParent(parent);
            iconParent.position = new Vector3(x, y + 0.08f, 0);

            if (drawIcon != null)
                drawIcon(iconParent, x, y + 0.08f);
            else
                MkSq(iconParent, "Ico", x, y + 0.08f, 0.55f, 0.55f, stationColor, 4);

            // Label below counter
            if (!string.IsNullOrEmpty(label))
            {
                var lgo = new GameObject($"Lbl_{label}");
                lgo.transform.SetParent(iconParent);
                lgo.transform.position = new Vector3(x, y - 0.58f, 0);
                lgo.transform.localScale = new Vector3(0.08f, 0.08f, 1f);
                var tm = lgo.AddComponent<TextMesh>();
                tm.text = label;
                tm.fontSize = 48;
                tm.anchor = TextAnchor.UpperCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = new Color(1, 1, 1, 0.85f);
                tm.characterSize = 0.4f;
                lgo.GetComponent<MeshRenderer>().sortingOrder = 15;
            }

            return station;
        }

        /// <summary>
        /// Creates an ingredient source station with crate visual.
        /// </summary>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        /// <param name="type">The ingredient type this source provides.</param>
        /// <param name="parent">Parent transform for the source icon.</param>
        private void CreateSource(float x, float y, IngredientType type, Transform parent)
        {
            var (color, label) = SourceVisuals.ContainsKey(type)
                ? SourceVisuals[type]
                : (new Color(0.75f, 0.70f, 0.50f), type.ToString().ToUpper());

            var go = new GameObject($"Source_{label}");
            go.transform.position = new Vector3(x, y, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sqSprite;
            sr.color = Color.clear;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.9f);

            var source = go.AddComponent<IngredientSource>();
            source.SetSourceType(type);

            // Crate icon
            var icon = new GameObject($"Icon_{label}").transform;
            icon.SetParent(parent);
            icon.position = new Vector3(x, y + 0.08f, 0);
            DrawCrateIcon(icon, x, y + 0.08f, color);

            // Label
            var lgo = new GameObject($"Lbl_{label}");
            lgo.transform.SetParent(icon);
            lgo.transform.position = new Vector3(x, y - 0.58f, 0);
            lgo.transform.localScale = new Vector3(0.08f, 0.08f, 1f);
            var tm = lgo.AddComponent<TextMesh>();
            tm.text = label;
            tm.fontSize = 48;
            tm.anchor = TextAnchor.UpperCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1, 1, 1, 0.85f);
            tm.characterSize = 0.4f;
            lgo.GetComponent<MeshRenderer>().sortingOrder = 15;
        }

        /// <summary>
        /// Creates a plain work counter station.
        /// </summary>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void CreateWorkCounter(float x, float y)
        {
            var go = new GameObject($"WorkCounter");
            go.transform.position = new Vector3(x, y, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sqSprite;
            sr.color = Color.clear;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.9f);

            go.AddComponent<Counter>();
        }

        // ═══════════════════════════════════════════════════════
        //  STATION ICONS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws a crate icon for ingredient sources.
        /// </summary>
        /// <param name="p">Parent transform for the icon.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        /// <param name="c">Color of the crate.</param>
        private void DrawCrateIcon(Transform p, float x, float y, Color c)
        {
            MkSq(p, "Box", x, y, 0.55f, 0.50f, c, 4);
            Color dark = c * 0.7f; dark.a = 1;
            MkSq(p, "BoxD", x, y - 0.18f, 0.55f, 0.10f, dark, 5);
            Color stripe = c * 0.85f; stripe.a = 1;
            MkSq(p, "Str", x, y + 0.04f, 0.45f, 0.06f, stripe, 5);
        }

        /// <summary>
        /// Draws a cutting board and knife icon.
        /// </summary>
        /// <param name="p">Parent transform for the icon.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void DrawCuttingIcon(Transform p, float x, float y)
        {
            MkSq(p, "Board", x, y, 0.55f, 0.40f, new Color(0.76f, 0.60f, 0.38f), 4);
            MkSq(p, "Knife", x + 0.12f, y + 0.05f, 0.06f, 0.35f, new Color(0.80f, 0.80f, 0.82f), 5);
            MkSq(p, "KHandle", x + 0.12f, y - 0.12f, 0.08f, 0.12f, COL_WOOD, 5);
        }

        /// <summary>
        /// Draws a cooktop with flames icon.
        /// </summary>
        /// <param name="p">Parent transform for the icon.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void DrawStoveIcon(Transform p, float x, float y)
        {
            MkSq(p, "Stove", x, y, 0.58f, 0.50f, COL_COOK, 4);
            MkCirc(p, "Burn1", x - 0.12f, y + 0.06f, 0.16f, new Color(0.25f, 0.25f, 0.28f), 5);
            MkCirc(p, "Burn2", x + 0.14f, y + 0.06f, 0.16f, new Color(0.25f, 0.25f, 0.28f), 5);
            MkCirc(p, "Fl1", x - 0.12f, y + 0.06f, 0.08f, COL_COOK_FLAME, 6);
            MkCirc(p, "Fl2", x + 0.14f, y + 0.06f, 0.08f, COL_COOK_FLAME, 6);
        }

        /// <summary>
        /// Draws a plate icon.
        /// </summary>
        /// <param name="p">Parent transform for the icon.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void DrawPlateIcon(Transform p, float x, float y)
        {
            MkCirc(p, "Plate", x, y, 0.42f, COL_PLATE, 4);
            MkCirc(p, "PlateIn", x, y, 0.30f, new Color(0.90f, 0.88f, 0.85f), 5);
        }

        /// <summary>
        /// Draws a serve window with arrow icon.
        /// </summary>
        /// <param name="p">Parent transform for the icon.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void DrawServeIcon(Transform p, float x, float y)
        {
            MkSq(p, "Win", x, y, 0.58f, 0.50f, COL_SERVE, 4);
            MkSq(p, "Arr", x, y + 0.02f, 0.08f, 0.25f, COL_SERVE_ARR, 5);
            MkSq(p, "ArrL", x - 0.08f, y + 0.08f, 0.20f, 0.08f, COL_SERVE_ARR, 5);
        }

        /// <summary>
        /// Draws a trash bin icon.
        /// </summary>
        /// <param name="p">Parent transform for the icon.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void DrawTrashIcon(Transform p, float x, float y)
        {
            MkSq(p, "Bin", x, y - 0.04f, 0.40f, 0.42f, COL_TRASH, 4);
            MkSq(p, "BinT", x, y + 0.18f, 0.46f, 0.08f, new Color(0.42f, 0.42f, 0.42f), 5);
            MkSq(p, "Lid", x, y + 0.24f, 0.35f, 0.06f, new Color(0.55f, 0.55f, 0.55f), 5);
        }

        /// <summary>
        /// Draws a sink basin with faucet icon.
        /// </summary>
        /// <param name="p">Parent transform for the icon.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        private void DrawSinkIcon(Transform p, float x, float y)
        {
            MkSq(p, "Basin", x, y, 0.52f, 0.42f, COL_SINK, 4);
            MkSq(p, "Water", x, y - 0.04f, 0.38f, 0.28f, new Color(0.40f, 0.65f, 0.85f), 5);
            MkSq(p, "Fauc", x, y + 0.24f, 0.08f, 0.14f, new Color(0.70f, 0.70f, 0.72f), 5);
            MkSq(p, "FaucH", x + 0.06f, y + 0.28f, 0.14f, 0.06f, new Color(0.70f, 0.70f, 0.72f), 5);
        }

        // ═══════════════════════════════════════════════════════
        //  PLAYER (round chef with hat, position adapts to grid)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Positions the player character and creates the chef visual.
        /// </summary>
        private void SetupPlayer()
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player == null) return;

            // Position in the lower walkable corridor, centered horizontally
            float playerX = _gridW / 2f + 0.5f;
            float playerY;
            if (_gridH >= 7)
                playerY = 2.5f;     // Full tier: corridor at y=2
            else if (_gridH >= 6)
                playerY = 1.5f;     // Medium tier: corridor at y=1
            else
                playerY = 1.5f;     // Small tier: corridor at y=1

            player.transform.position = new Vector3(playerX, playerY, 0);
            player.transform.localScale = Vector3.one;

            var sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = _circSprite;
                sr.color = COL_PLAYER;
                sr.sortingOrder = 10;
            }

            // Drop shadow
            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(player.transform);
            shadow.transform.localPosition = new Vector3(0.03f, -0.08f, 0);
            shadow.transform.localScale = new Vector3(0.9f, 0.5f, 1f);
            var ssr = shadow.AddComponent<SpriteRenderer>();
            ssr.sprite = _circSprite;
            ssr.color = COL_SHADOW;
            ssr.sortingOrder = 9;

            // Chef hat
            var hat = new GameObject("ChefHat");
            hat.transform.SetParent(player.transform);
            hat.transform.localPosition = new Vector3(0, 0.28f, 0);
            hat.transform.localScale = new Vector3(0.65f, 0.55f, 1f);
            var hsr = hat.AddComponent<SpriteRenderer>();
            hsr.sprite = _circSprite;
            hsr.color = COL_CHEF_HAT;
            hsr.sortingOrder = 12;

            // Hat poof
            var poof = new GameObject("HatPoof");
            poof.transform.SetParent(player.transform);
            poof.transform.localPosition = new Vector3(0, 0.42f, 0);
            poof.transform.localScale = new Vector3(0.45f, 0.40f, 1f);
            var psr = poof.AddComponent<SpriteRenderer>();
            psr.sprite = _circSprite;
            psr.color = COL_CHEF_HAT;
            psr.sortingOrder = 13;

            // Face
            var face = new GameObject("Face");
            face.transform.SetParent(player.transform);
            face.transform.localPosition = new Vector3(0, 0.05f, 0);
            face.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            var fsr = face.AddComponent<SpriteRenderer>();
            fsr.sprite = _circSprite;
            fsr.color = COL_CHEF_SKIN;
            fsr.sortingOrder = 11;
        }

        // ═══════════════════════════════════════════════════════
        //  DRAW HELPERS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Creates a square sprite renderer at the given position.
        /// </summary>
        /// <param name="parent">Parent transform (can be null).</param>
        /// <param name="name">Name of the GameObject.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        /// <param name="w">Width in world units.</param>
        /// <param name="h">Height in world units.</param>
        /// <param name="c">Sprite color.</param>
        /// <param name="order">Sorting order for rendering.</param>
        /// <returns>The created SpriteRenderer component.</returns>
        private static SpriteRenderer MkSq(Transform parent, string name, float x, float y,
            float w, float h, Color c, int order)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, y, 0);
            go.transform.localScale = new Vector3(w, h, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sqSprite;
            sr.color = c;
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>
        /// Creates a circle sprite renderer at the given position.
        /// </summary>
        /// <param name="parent">Parent transform (can be null).</param>
        /// <param name="name">Name of the GameObject.</param>
        /// <param name="x">World X position.</param>
        /// <param name="y">World Y position.</param>
        /// <param name="size">Diameter of the circle in world units.</param>
        /// <param name="c">Sprite color.</param>
        /// <param name="order">Sorting order for rendering.</param>
        /// <returns>The created SpriteRenderer component.</returns>
        private static SpriteRenderer MkCirc(Transform parent, string name, float x, float y,
            float size, Color c, int order)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, y, 0);
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _circSprite;
            sr.color = c;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
