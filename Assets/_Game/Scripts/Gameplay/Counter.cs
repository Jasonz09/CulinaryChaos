using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Simple counter surface: hold and retrieve items.
    /// If a PlatedDish is on the counter, player can add ingredients to it.
    /// </summary>
    public class Counter : InteractiveObject
    {
        /// <summary>
        /// Handles player interaction to place, retrieve, or add items to a plated dish on the counter.
        /// </summary>
        /// <param name="player">The player interacting with this counter.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (player.IsCarrying)
            {
                // If counter holds a plate and player carries a cooked/chopped item, add to plate
                if (HeldItem != null && HeldItem.CurrentState == IngredientState.Plated
                    && player.CarriedItem.CurrentState != IngredientState.Plated)
                {
                    var incoming = player.CarriedItem;
                    if (incoming.CurrentState == IngredientState.Cooked ||
                        incoming.CurrentState == IngredientState.Chopped)
                    {
                        var released = player.ReleaseItem();
                        HeldItem.AddToPlate(released);
                        released.gameObject.SetActive(false);
                        return;
                    }
                }

                // Normal: place item on empty counter
                if (CanAcceptItem(player.CarriedItem))
                {
                    var item = player.ReleaseItem();
                    PlaceItem(item);
                }
            }
            else if (!player.IsCarrying && HeldItem != null)
            {
                // Pick up item from counter
                var item = RemoveItem();
                player.PickupItem(item);
            }
        }
    }
}
