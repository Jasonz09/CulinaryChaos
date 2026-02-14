using UnityEngine;

namespace IOChef.Economy
{
    /// <summary>
    /// Data for a purchasable cosmetic item.
    /// </summary>
    [System.Serializable]
    public class CosmeticItem
    {
        /// <summary>
        /// Unique cosmetic identifier.
        /// </summary>
        public string cosmeticId;

        /// <summary>
        /// Display name shown to the player.
        /// </summary>
        public string displayName;

        /// <summary>
        /// Category of this cosmetic.
        /// </summary>
        public CosmeticType cosmeticType;

        /// <summary>
        /// Rarity tier.
        /// </summary>
        public CosmeticRarity rarity;

        /// <summary>
        /// Preview image for the shop.
        /// </summary>
        public Sprite previewImage;

        /// <summary>
        /// Soft currency price (0 = not purchasable with credits).
        /// </summary>
        public int priceCredits;

        /// <summary>
        /// Premium currency price (0 = not purchasable with gems).
        /// </summary>
        public int priceGems;

        /// <summary>
        /// Whether the player owns this cosmetic.
        /// </summary>
        public bool isOwned;

        /// <summary>
        /// Whether this cosmetic is currently equipped.
        /// </summary>
        public bool isEquipped;

        /// <summary>
        /// Hero ID this cosmetic applies to (for hero skins).
        /// </summary>
        public string targetHeroId;

        /// <summary>
        /// Whether this item is featured in the shop sidebar.
        /// </summary>
        public bool isFeatured;
    }
}
