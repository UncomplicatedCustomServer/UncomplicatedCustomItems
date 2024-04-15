using Exiled.Events.EventArgs.Player;

namespace UncomplicatedCustomItems.Events.Internal
{
    internal class Player
    {
        public void OnItemDropping(DroppingItemEventArgs Dropping)
        {
            Dropping.IsAllowed = Helper.Helper.HandleItemEvent(ItemEvents.Drop, Dropping, Dropping.Player);
        }

        public void OnItemUsing(UsingItemEventArgs Using)
        {
            Using.IsAllowed = Helper.Helper.HandleItemEvent(ItemEvents.Drop, Using, Using.Player);
        }

        public void OnItemPickup(ItemAddedEventArgs PickingUp)
        {
            if (!Helper.Helper.HandleItemEvent(ItemEvents.Drop, PickingUp, PickingUp.Player))
            {
                PickingUp.Player.DropItem(PickingUp.Item);
            }
        }
    }
}
