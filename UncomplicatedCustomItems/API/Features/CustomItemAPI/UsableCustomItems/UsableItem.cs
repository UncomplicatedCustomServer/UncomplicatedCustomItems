using LabApi.Events.Arguments.PlayerEvents;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class UsableItem : APICustomItem
    {
        public override void RegisterEvents()
        {
            PlayerEvent.CancellingUsingItem += new LabApi.Events.LabEventHandler<PlayerCancellingUsingItemEventArgs>(InternalOnUsingCancelling);
            PlayerEvent.CancelledUsingItem += new LabApi.Events.LabEventHandler<PlayerCancelledUsingItemEventArgs>(InternalOnUsingCancelled);
            PlayerEvent.UsingItem += new LabApi.Events.LabEventHandler<PlayerUsingItemEventArgs>(InternalOnUsing);
            PlayerEvent.UsedItem += new LabApi.Events.LabEventHandler<PlayerUsedItemEventArgs>(InternalOnUsed);
            PlayerEvent.ItemUsageEffectsApplying += new LabApi.Events.LabEventHandler<PlayerItemUsageEffectsApplyingEventArgs>(InternalOnEffectsApplying);

            base.RegisterEvents();
        }

        public override void UnregisterEvents()
        {
            PlayerEvent.CancellingUsingItem -= new LabApi.Events.LabEventHandler<PlayerCancellingUsingItemEventArgs>(InternalOnUsingCancelling);
            PlayerEvent.CancelledUsingItem -= new LabApi.Events.LabEventHandler<PlayerCancelledUsingItemEventArgs>(InternalOnUsingCancelled);
            PlayerEvent.UsingItem -= new LabApi.Events.LabEventHandler<PlayerUsingItemEventArgs>(InternalOnUsing);
            PlayerEvent.UsedItem -= new LabApi.Events.LabEventHandler<PlayerUsedItemEventArgs>(InternalOnUsed);
            PlayerEvent.ItemUsageEffectsApplying -= new LabApi.Events.LabEventHandler<PlayerItemUsageEffectsApplyingEventArgs>(InternalOnEffectsApplying);

            base.UnregisterEvents();
        }

        private void InternalOnUsingCancelling(PlayerCancellingUsingItemEventArgs ev)
        {
            if (Check(ev.UsableItem))
                OnUsingCancelling(ev);
        }

        private void InternalOnUsingCancelled(PlayerCancelledUsingItemEventArgs ev)
        {
            if (Check(ev.UsableItem))
                OnUsingCancelled(ev);
        }


        private void InternalOnUsing(PlayerUsingItemEventArgs ev)
        {
            if (Check(ev.UsableItem))
                OnUsing(ev);
        }

        private void InternalOnUsed(PlayerUsedItemEventArgs ev)
        {
            if (Check(ev.UsableItem))
                OnUsed(ev);
        }

        private void InternalOnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            if (Check(ev.UsableItem))
                OnEffectsApplying(ev);
        }

        protected virtual void OnUsingCancelling(PlayerCancellingUsingItemEventArgs ev) { }
        protected virtual void OnUsingCancelled(PlayerCancelledUsingItemEventArgs ev) { }
        protected virtual void OnUsing(PlayerUsingItemEventArgs ev) { }
        protected virtual void OnUsed(PlayerUsedItemEventArgs ev) { }
        protected virtual void OnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev) { }
    }
}