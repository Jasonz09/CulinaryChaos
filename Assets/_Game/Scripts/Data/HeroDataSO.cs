using UnityEngine;

namespace IOChef.Heroes
{
    /// <summary>
    /// ScriptableObject defining a hero's stats, abilities, and progression.
    /// Stats scale between base and max values based on hero level.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHero", menuName = "IOChef/Hero Data")]
    public class HeroDataSO : ScriptableObject
    {
        [Header("Hero Info")]
        public string heroId;
        public string heroName;
        [TextArea] public string description;
        public HeroRarity rarity;
        public Sprite heroArt;
        public Sprite heroPortrait;

        [Header("Acquisition")]
        public bool isFreeHero;
        public float purchasePrice;
        public string acquisitionMethod;

        [Header("Ability")]
        public string abilityName;
        [TextArea] public string abilityDescription;

        [Header("Gameplay Modifiers — Base (Level 1)")]
        public float baseCookTimeMultiplier = 1f;
        public float baseBurnTimeMultiplier = 1f;
        public float baseScoreMultiplier = 1f;
        public float baseMovementSpeedMultiplier = 1f;
        public float baseInteractionRadiusMultiplier = 1f;
        public float baseBonusTimeSeconds = 0f;

        [Header("Gameplay Modifiers — Max (Level 10)")]
        public float maxCookTimeMultiplier = 0.95f;
        public float maxBurnTimeMultiplier = 1.08f;
        public float maxScoreMultiplier = 1.05f;
        public float maxMovementSpeedMultiplier = 1.05f;
        public float maxInteractionRadiusMultiplier = 1.05f;
        public float maxBonusTimeSeconds = 3f;

        [Header("Special Ability")]
        public SpecialAbilityType specialAbilityType;
        public float baseSpecialAbilityValue;
        public float maxSpecialAbilityValue;

        [Header("Progression")]
        public int maxLevel = 10;
        public AnimationCurve xpCurve;

        [Header("Cosmetics")]
        public RuntimeAnimatorController animatorOverride;
        public GameObject specialEffect;
        public string[] skinIds;

        // Legacy flat fields kept for backward compat (hidden in Inspector)
        [HideInInspector] public float cookTimeMultiplier = 1f;
        [HideInInspector] public float burnTimeMultiplier = 1f;
        [HideInInspector] public float scoreMultiplier = 1f;
        [HideInInspector] public float movementSpeedMultiplier = 1f;
        [HideInInspector] public float interactionRadiusMultiplier = 1f;
        [HideInInspector] public int bonusTimeSeconds = 0;
        [HideInInspector] public int maxCarryItems = 1;
        [HideInInspector] public float specialAbilityValue;

        /// <summary>
        /// Interpolation factor: 0.0 at level 1, 1.0 at maxLevel.
        /// </summary>
        private float LevelT(int level)
        {
            if (maxLevel <= 1) return 0f;
            return Mathf.Clamp01((float)(level - 1) / (maxLevel - 1));
        }

        /// <summary>
        /// Convert hero stats to gameplay modifiers, scaled by hero level.
        /// </summary>
        public GameplayModifiers ToModifiers(int level)
        {
            float t = LevelT(level);
            return new GameplayModifiers
            {
                cookTimeMultiplier = Mathf.Lerp(baseCookTimeMultiplier, maxCookTimeMultiplier, t),
                burnTimeMultiplier = Mathf.Lerp(baseBurnTimeMultiplier, maxBurnTimeMultiplier, t),
                scoreMultiplier = Mathf.Lerp(baseScoreMultiplier, maxScoreMultiplier, t),
                movementSpeedMultiplier = Mathf.Lerp(baseMovementSpeedMultiplier, maxMovementSpeedMultiplier, t),
                interactionRadiusMultiplier = Mathf.Lerp(baseInteractionRadiusMultiplier, maxInteractionRadiusMultiplier, t),
                bonusTimeSeconds = Mathf.RoundToInt(Mathf.Lerp(baseBonusTimeSeconds, maxBonusTimeSeconds, t)),
                maxCarryItems = 1
            };
        }

        /// <summary>
        /// Convert hero stats to gameplay modifiers at level 1 (backward compat).
        /// </summary>
        public GameplayModifiers ToModifiers()
        {
            return ToModifiers(1);
        }

        /// <summary>
        /// Returns the special ability value scaled by hero level.
        /// </summary>
        public float GetSpecialAbilityValue(int level)
        {
            float t = LevelT(level);
            return Mathf.Lerp(baseSpecialAbilityValue, maxSpecialAbilityValue, t);
        }
    }
}
