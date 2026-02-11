using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Fetches level configurations from CloudScript (embedded WORLD_CONFIGS)
    /// on login, parses them into LevelDataSO instances, and caches them in
    /// memory. This is the authoritative source of level data —
    /// DefaultLevelFactory is only used as a fallback when the server is
    /// unavailable.
    /// </summary>
    public class ServerLevelLoader : MonoBehaviour
    {
        public static ServerLevelLoader Instance { get; private set; }

        /// <summary>True after level configs have been fetched and parsed.</summary>
        public bool IsLoaded { get; private set; }

        /// <summary>Fires when all world data has been loaded from server.</summary>
        public event Action OnLoaded;

        private const int MAX_WORLDS = 10;

        private readonly Dictionary<int, LevelDataSO> _levels = new();
        private readonly Dictionary<int, List<LevelDataSO>> _worldLevels = new();
        private readonly Dictionary<int, string> _worldNames = new();
        private int _worldCount;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Fetches all world configs from CloudScript's embedded WORLD_CONFIGS.
        /// Call after login succeeds.
        /// </summary>
        public void FetchAllWorlds()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[ServerLevelLoader] Not logged in, cannot fetch");
                OnLoaded?.Invoke();
                return;
            }

            PlayFabManager.Instance.ExecuteCloudScript("GetWorldConfigs", null,
                resultJson =>
                {
                    _levels.Clear();
                    _worldLevels.Clear();
                    _worldNames.Clear();
                    _worldCount = 0;

                    try
                    {
                        var worlds = JObject.Parse(resultJson);

                        foreach (var prop in worlds.Properties())
                        {
                            if (!int.TryParse(prop.Name, out int worldId)) continue;
                            var worldJson = prop.Value?.ToString();
                            if (string.IsNullOrEmpty(worldJson)) continue;

                            try
                            {
                                ParseWorld(worldId, worldJson);
                                _worldCount = Mathf.Max(_worldCount, worldId);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[ServerLevelLoader] Failed to parse World_{worldId}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ServerLevelLoader] Failed to parse GetWorldConfigs response: {ex.Message}");
                    }

                    IsLoaded = true;
                    Debug.Log($"[ServerLevelLoader] Loaded {_levels.Count} levels across {_worldCount} world(s)");
                    OnLoaded?.Invoke();
                },
                err =>
                {
                    Debug.LogWarning($"[ServerLevelLoader] GetWorldConfigs failed: {err}");
                    IsLoaded = true; // Mark loaded so game can fall back to local
                    OnLoaded?.Invoke();
                });
        }

        private void ParseWorld(int worldId, string json)
        {
            var worldObj = JObject.Parse(json);
            string worldName = worldObj["worldName"]?.Value<string>() ?? $"WORLD {worldId}";
            _worldNames[worldId] = worldName;

            var levelsObj = worldObj["levels"] as JObject;
            if (levelsObj == null) return;

            var worldList = new List<LevelDataSO>();

            foreach (var prop in levelsObj.Properties())
            {
                if (!int.TryParse(prop.Name, out int levelNumber)) continue;
                var cfg = prop.Value as JObject;
                if (cfg == null) continue;

                var level = ParseLevel(worldId, levelNumber, cfg);
                if (level != null)
                {
                    _levels[level.levelId] = level;
                    worldList.Add(level);
                }
            }

            worldList.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
            _worldLevels[worldId] = worldList;
        }

        private LevelDataSO ParseLevel(int worldId, int levelNumber, JObject cfg)
        {
            var level = ScriptableObject.CreateInstance<LevelDataSO>();
            level.levelId = levelNumber; // For World 1 this is the same as levelNumber
            level.worldId = worldId;
            level.levelNumber = levelNumber;
            level.levelName = cfg["name"]?.Value<string>() ?? $"Level {levelNumber}";
            level.timeLimitSeconds = cfg["time"]?.Value<float>() ?? 120f;
            level.orderSpawnInterval = cfg["orderInterval"]?.Value<float>() ?? 45f;
            level.maxActiveOrders = cfg["maxOrders"]?.Value<int>() ?? 3;
            level.initialOrders = cfg["initialOrders"]?.Value<int>() ?? 1;
            level.threshold1Star = cfg["star1"]?.Value<int>() ?? 300;
            level.threshold2Star = cfg["star2"]?.Value<int>() ?? 500;
            level.threshold3Star = cfg["star3"]?.Value<int>() ?? 700;
            level.unlimitedPlates = cfg["unlimitedPlates"]?.Value<bool>() ?? true;
            level.autoRemovePlates = cfg["autoRemovePlates"]?.Value<bool>() ?? true;
            level.requiresSink = cfg["requiresSink"]?.Value<bool>() ?? false;
            level.plateCount = cfg["plateCount"]?.Value<int>() ?? 0;
            level.entryCost = cfg["entryCost"]?.Value<int>() ?? DefaultLevelFactory.GetEntryCost(levelNumber);
            level.freeHeroRewardId = cfg["freeHeroRewardId"]?.Value<string>() ?? "";

            // Parse recipes
            level.availableRecipes = new List<RecipeSO>();
            var recipesArr = cfg["recipes"] as JArray;
            if (recipesArr != null)
            {
                foreach (var rToken in recipesArr)
                {
                    var rObj = rToken as JObject;
                    if (rObj == null) continue;

                    var recipe = ParseRecipe(rObj);
                    if (recipe != null)
                        level.availableRecipes.Add(recipe);
                }
            }

            // Auto-generate equipment layout from recipes
            KitchenLayoutGenerator.Generate(level);

            return level;
        }

        private RecipeSO ParseRecipe(JObject rObj)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeSO>();
            recipe.recipeName = rObj["name"]?.Value<string>() ?? "Unknown";
            recipe.pointsForCompletion = rObj["points"]?.Value<int>() ?? 100;
            recipe.timeLimitSeconds = rObj["time"]?.Value<float>() ?? 60f;
            recipe.difficultyTier = rObj["difficulty"]?.Value<int>() ?? 1;
            recipe.finalIngredients = new List<RecipeIngredient>();

            var ingsArr = rObj["ingredients"] as JArray;
            if (ingsArr != null)
            {
                foreach (var iToken in ingsArr)
                {
                    var iObj = iToken as JObject;
                    if (iObj == null) continue;

                    string typeName = iObj["type"]?.Value<string>() ?? "";
                    string stateName = iObj["state"]?.Value<string>() ?? "Raw";

                    if (Enum.TryParse<IngredientType>(typeName, true, out var ingType)
                        && Enum.TryParse<IngredientState>(stateName, true, out var ingState))
                    {
                        recipe.finalIngredients.Add(new RecipeIngredient
                        {
                            ingredientType = ingType,
                            requiredState = ingState
                        });
                    }
                    else
                    {
                        Debug.LogWarning($"[ServerLevelLoader] Unknown ingredient: {typeName}/{stateName}");
                    }
                }
            }

            return recipe;
        }

        /// <summary>Gets a level by its unique ID.</summary>
        public LevelDataSO GetLevel(int levelId)
        {
            return _levels.TryGetValue(levelId, out var level) ? level : null;
        }

        /// <summary>Gets all levels for a world, sorted by level number.</summary>
        public LevelDataSO[] GetLevelsForWorld(int worldId)
        {
            return _worldLevels.TryGetValue(worldId, out var list) ? list.ToArray() : Array.Empty<LevelDataSO>();
        }

        /// <summary>Gets the display name for a world.</summary>
        public string GetWorldName(int worldId)
        {
            return _worldNames.TryGetValue(worldId, out var name) ? name : null;
        }

        /// <summary>Gets the number of worlds loaded from server.</summary>
        public int GetWorldCount()
        {
            return _worldCount;
        }
    }
}
