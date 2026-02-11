using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Trash bin: dispose of unwanted or burned items.
    /// </summary>
    public class TrashBin : InteractiveObject
    {
        /// <summary>
        /// Handles player interaction to dispose of a carried item.
        /// </summary>
        /// <param name="player">The player interacting with this trash bin.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (player.IsCarrying)
            {
                var item = player.ReleaseItem();
                Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// Determines whether this trash bin can accept the given ingredient.
        /// </summary>
        /// <param name="item">The ingredient to check.</param>
        /// <returns>True if the item is not null.</returns>
        public override bool CanAcceptItem(Ingredient item)
        {
            return item != null;
        }
    }
}
