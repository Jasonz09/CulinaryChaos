using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Base class for all interactive kitchen equipment.
    /// </summary>
    public abstract class InteractiveObject : MonoBehaviour
    {
        [Header("Interactive Object")]
        /// <summary>
        /// Sprite renderer used to display this object's visual appearance.
        /// </summary>
        [SerializeField] protected SpriteRenderer spriteRenderer;
        /// <summary>
        /// Transform where held ingredients are positioned on this station.
        /// </summary>
        [SerializeField] protected Transform itemHoldPoint;

        /// <summary>
        /// Current object state.
        /// </summary>
        public ObjectState CurrentState { get; protected set; } = ObjectState.Empty;

        /// <summary>
        /// Ingredient currently on this station.
        /// </summary>
        public Ingredient HeldItem { get; protected set; }

        /// <summary>
        /// Handle player interaction.
        /// </summary>
        /// <param name="player">The interacting player.</param>
        public abstract void OnPlayerInteract(PlayerController player);

        /// <summary>
        /// Check if this object can accept an ingredient.
        /// </summary>
        /// <param name="item">Ingredient to check.</param>
        /// <returns>True if the item can be placed.</returns>
        public virtual bool CanAcceptItem(Ingredient item)
        {
            return CurrentState == ObjectState.Empty && HeldItem == null;
        }

        /// <summary>
        /// Places an ingredient onto this station's hold point.
        /// </summary>
        /// <param name="item">The ingredient to place on the station.</param>
        protected void PlaceItem(Ingredient item)
        {
            HeldItem = item;
            if (itemHoldPoint == null)
            {
                var hp = new GameObject("HoldPoint");
                hp.transform.SetParent(transform);
                hp.transform.localPosition = new Vector3(0, 0.25f, 0);
                itemHoldPoint = hp.transform;
            }
            if (item != null)
            {
                item.transform.SetParent(itemHoldPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one; // ensure full size on station
                item.gameObject.SetActive(true);
            }
            CurrentState = ObjectState.HasIngredient;
        }

        /// <summary>
        /// Removes and returns the held ingredient from this station.
        /// </summary>
        /// <returns>The ingredient that was held, or null if empty.</returns>
        protected Ingredient RemoveItem()
        {
            var item = HeldItem;
            HeldItem = null;
            if (item != null)
                item.transform.SetParent(null);
            CurrentState = ObjectState.Empty;
            return item;
        }
    }
}
