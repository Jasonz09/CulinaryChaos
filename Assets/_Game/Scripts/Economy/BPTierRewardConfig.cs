namespace IOChef.Economy
{
    [System.Serializable]
    public class BPTierRewardConfig
    {
        public int freeCoins;
        public int freeGems;
        public int premiumCoins;
        public int premiumGems;
        public int premiumTokens;

        public string GetFreeDescription()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (freeCoins > 0) parts.Add($"{freeCoins} Coins");
            if (freeGems > 0) parts.Add($"{freeGems} Gems");
            return parts.Count > 0 ? string.Join(" + ", parts) : "—";
        }

        public string GetPremiumDescription()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (premiumCoins > 0) parts.Add($"{premiumCoins} Coins");
            if (premiumGems > 0) parts.Add($"{premiumGems} Gems");
            if (premiumTokens > 0) parts.Add($"{premiumTokens} Tokens");
            return parts.Count > 0 ? string.Join(" + ", parts) : "—";
        }
    }
}
