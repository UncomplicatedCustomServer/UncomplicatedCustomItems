using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.HarmonyElements.Patches;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomCandy : APICustomItem
    {
        /// <summary>
        /// Set when the user gets the candy
        /// </summary>
        public CandyInstance CandyInstance { get; internal set; }

        /// <summary>
        /// Gets or sets the Candy Type to spawn as
        /// </summary>
        public abstract CandyKindID CandyType { get; set; }

        /// <summary>
        /// Gets or sets the hint message shown when the candy is eaten
        /// </summary>
        public virtual string EatingMessage { get; set; }

        /// <summary>
        /// Gets or sets the hint message duration
        /// </summary>
        public virtual float EatingMessageDuration { get; set; }

        /// <summary>
        /// Gets or sets whether the candy will be destroyed when the player uses it.
        /// </summary>
        public abstract bool DestroyOnUse { get; set; }

        /// <summary>
        /// Gets or sets the chance for the candy to spawn.
        /// </summary>
        public abstract float Chance { get; set; }

        /// <summary>
        /// Gets or sets whether the candy will apply its effects to the player.
        /// </summary>
        public abstract bool ApplyEffects { get; set; }

        /// <summary>
        /// Gets or sets whether the candy can spawn as a candy bag on the map
        /// </summary>
        public abstract bool AllowSpawningAsItem { get; set; }

        // TODO:
        // Test events.
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
            if (ev.UsableItem.Base is Scp330Bag bag && bag.IsCandySelected && Scp330CandyInstancePatch.TryGetCandyInstances(bag, out List<CandyInstance> instances))
            {
                int index = bag.SelectedCandyId;
                if (index >= 0 && index < instances.Count)
                {
                    CandyInstance instance = instances[index];

                    if (instance.APICustomItem != null && CustomItems.TryGetValue(instance.APICustomItem.Id, out APICustomItem baseItem) && baseItem is CustomCandy customCandy && customCandy == this)
                        OnUsed(ev);
                }
            }
        }

        private void InternalOnUsingItem(PlayerUsingItemEventArgs ev)
        {
            if (ev.UsableItem.Base is Scp330Bag bag && bag.IsCandySelected && Scp330CandyInstancePatch.TryGetCandyInstances(bag, out List<CandyInstance> instances))
            {
                int index = bag.SelectedCandyId;
                if (index >= 0 && index < instances.Count)
                {
                    CandyInstance instance = instances[index];

                    if (instance.APICustomItem != null && CustomItems.TryGetValue(instance.APICustomItem.Id, out APICustomItem baseItem) && baseItem is CustomCandy customCandy && customCandy == this)
                        OnUsing(ev);
                }
            }
        }

        private void InternalOnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            if (ev.UsableItem.Base is Scp330Bag bag && bag.IsCandySelected && Scp330CandyInstancePatch.TryGetCandyInstances(bag, out List<CandyInstance> instances))
            {
                int index = bag.SelectedCandyId;
                if (index >= 0 && index < instances.Count)
                {
                    CandyInstance instance = instances[index];

                    if (instance.APICustomItem != null && CustomItems.TryGetValue(instance.APICustomItem.Id, out APICustomItem baseItem) && baseItem is CustomCandy customCandy && customCandy == this)
                    {
                        if (!ApplyEffects)
                        {
                            ev.ContinueProcess = false;
                            ev.IsAllowed = false;
                            ev.Player.CurrentItem = null;
                            return;
                        }

                        OnEffectsApplying(ev);
                        Timing.CallDelayed(Timing.WaitForOneFrame, () => OnEffectsApplied(ev));

                        if (customCandy.DestroyOnUse)
                            (ev.UsableItem.Base as Scp330Bag)?.TryRemove(index);

                        ev.Player.SendHint(customCandy.EatingMessage, customCandy.EatingMessageDuration);
                    }
                }
            }
        }

        public virtual void OnUsed(PlayerUsedItemEventArgs ev) { }
        public virtual void OnUsing(PlayerUsingItemEventArgs ev) { }
        public virtual void OnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev) { }
        public virtual void OnEffectsApplied(PlayerItemUsageEffectsApplyingEventArgs ev) { }
    }
}