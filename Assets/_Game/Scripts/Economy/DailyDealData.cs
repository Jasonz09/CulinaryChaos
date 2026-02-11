namespace IOChef.Economy
{
    [System.Serializable]
    public class DailyDealData
    {
        public string dealId;
        public string type;
        public int amount;
        public int normalGemCost;
        public int dealGemCost;
        public bool isFree;
    }
}
