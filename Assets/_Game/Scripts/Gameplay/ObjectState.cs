namespace IOChef.Gameplay
{
    /// <summary>
    /// State of an interactive kitchen object.
    /// </summary>
    public enum ObjectState
    {
        Empty,
        HasIngredient,
        Cooking,
        Complete,
        Dirty
    }
}
