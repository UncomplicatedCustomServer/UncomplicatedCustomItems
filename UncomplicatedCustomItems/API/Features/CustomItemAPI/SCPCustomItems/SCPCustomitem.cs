using LabApi.Events.Arguments.PlayerEvents;
using MEC;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class SCPCustomItem : APICustomItem
    {
        public override void RegisterEvents()
        {
            PlayerEvent.UsedItem += new LabApi.Events.LabEventHandler<PlayerUsedItemEventArgs>(InternalOnUsedItem);
            PlayerEvent.UsingItem += new LabApi.Events.LabEventHandler<PlayerUsingItemEventArgs>(InternalOnUsingItem);
            PlayerEvent.ItemUsageEffectsApplying += new LabApi.Events.LabEventHandler<PlayerItemUsageEffectsApplyingEventArgs>(InternalOnEffectsApplying);

            base.RegisterEvents();
        }

        public override void UnregisterEvents()
        {
            PlayerEvent.UsedItem -= new LabApi.Events.LabEventHandler<PlayerUsedItemEventArgs>(InternalOnUsedItem);
            PlayerEvent.UsingItem -= new LabApi.Events.LabEventHandler<PlayerUsingItemEventArgs>(InternalOnUsingItem);
            PlayerEvent.ItemUsageEffectsApplying -= new LabApi.Events.LabEventHandler<PlayerItemUsageEffectsApplyingEventArgs>(InternalOnEffectsApplying);

            base.UnregisterEvents();
        }

        private void InternalOnUsedItem(PlayerUsedItemEventArgs ev)
        {
            if (Check(ev.UsableItem))
                OnUsed(ev);
        }

        private void InternalOnUsingItem(PlayerUsingItemEventArgs ev)
        {
            if (Check(ev.UsableItem))
                OnUsing(ev);
        }

        private void InternalOnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            if (Check(ev.UsableItem))
            {
                OnEffectsApplying(ev);
                Timing.CallDelayed(Timing.WaitForOneFrame, () => OnEffectsApplied(ev));
            }
        }

        public virtual void OnUsed(PlayerUsedItemEventArgs ev) { }
        public virtual void OnUsing(PlayerUsingItemEventArgs ev) { }
        public virtual void OnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev) { }
        public virtual void OnEffectsApplied(PlayerItemUsageEffectsApplyingEventArgs ev) { }
    }
}