using System.Collections.Generic;

namespace IOChef.Economy
{
    [System.Serializable]
    public class BundleData
    {
        public string bundleId;
        public string name;
        public int gemCost;
        public float valueMultiplier;
        public List<BundleContent> contents = new();
    }

    [System.Serializable]
    public class BundleContent
    {
        public string type;
        public int amount;
    }
}
