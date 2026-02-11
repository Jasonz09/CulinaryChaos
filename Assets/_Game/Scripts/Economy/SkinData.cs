namespace IOChef.Economy
{
    /// <summary>
    /// Data model for an equippable skin item.
    /// </summary>
    [System.Serializable]
    public class SkinData
    {
        /// <summary>
        /// Unique identifier for this skin.
        /// </summary>
        public string skinId;

        /// <summary>
        /// Display name of the skin.
        /// </summary>
        public string displayName;

        /// <summary>
        /// Category of this skin.
        /// </summary>
        public SkinType skinType;

        /// <summary>
        /// Cost in coins to purchase this skin.
        /// </summary>
        public int priceCoin;

        /// <summary>
        /// Cost in gems to purchase this skin.
        /// </summary>
        public int priceGems;

        /// <summary>
        /// Associated hero ID if this is a hero skin.
        /// </summary>
        public string heroId;
    }
}
