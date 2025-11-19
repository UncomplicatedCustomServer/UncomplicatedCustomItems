using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using MEC;
using System.Collections.Generic;
using System.Linq;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomCandy : APICustomItem
    {
        public static List<(CustomCandy, ushort, int)> Candyidx = [];

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
        public virtual bool AllowSpawningAsItem { get; set; }

        // TODO:
        // Test events.
        public override void RegisterEvents()
        {
            PlayerEvent.UsingItem += new LabApi.Events.LabEventHandler<PlayerUsingItemEventArgs>(InternalOnUsingItem);
            PlayerEvent.ItemUsageEffectsApplying += new LabApi.Events.LabEventHandler<PlayerItemUsageEffectsApplyingEventArgs>(InternalOnEffectsApplying);

            base.RegisterEvents();
        }

        public override void UnregisterEvents()
        {
            PlayerEvent.UsingItem -= new LabApi.Events.LabEventHandler<PlayerUsingItemEventArgs>(InternalOnUsingItem);
            PlayerEvent.ItemUsageEffectsApplying -= new LabApi.Events.LabEventHandler<PlayerItemUsageEffectsApplyingEventArgs>(InternalOnEffectsApplying);

            base.UnregisterEvents();
        }

        private void InternalOnUsingItem(PlayerUsingItemEventArgs ev)
        {
            if (ev.UsableItem.Base is Scp330Bag bag)
            {
                List<APICustomItem> candies = List.Where(c => c is CustomCandy candyData && c.Spawn).ToList();
                APICustomItem item = candies.RandomItem();

                if (candies.Count > 0 && item is CustomCandy data && UnityEngine.Random.Range(0f, 101f) >= data.Chance && bag.Candies[bag.SelectedCandyId] == data.CandyType)
                {
                    int index = bag.SelectedCandyId;
                    if (index >= 0 && index <= bag.Candies.Count)
                    {
                        OnUsing(ev);
                        Timing.CallDelayed(Timing.WaitForOneFrame, () => OnUsed(ev));
                    }
                }
            }
        }

        private void InternalOnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            if (ev.UsableItem.Base is Scp330Bag bag)
            {
                int index = bag.SelectedCandyId;
                if (index >= 0 && index <= bag.Candies.Count)
                {
                    List<APICustomItem> candies = List.Where(c => c is CustomCandy candyData && c.Spawn).ToList();
                    if (candies.Count() >= 1)
                    {
                        APICustomItem item = candies.RandomItem();

                        if (Candyidx.Any(i => i.Item2 == bag.ItemSerial && bag.Candies[index] == CandyType))
                        {
                            if (!ApplyEffects)
                            {
                                InventorySystem.Items.Usables.UsableItemsController.GetHandler(ev.Player.ReferenceHub).CurrentUsable.Item?.OnUsingCancelled();
                                ev.Player.Connection.Send(new InventorySystem.Items.Usables.StatusMessage(InventorySystem.Items.Usables.StatusMessage.StatusType.Cancel, bag.ItemSerial), 0);
                                ev.IsAllowed = false;
                                ev.ContinueProcess = false;
                                return;
                            }


                            OnEffectsApplying(ev);
                            Timing.CallDelayed(Timing.WaitForOneFrame, () => OnEffectsApplied(ev));

                            if (DestroyOnUse)
                            {
                                (ev.UsableItem.Base as Scp330Bag)?.TryRemove(index);
                                Candyidx.Remove((this, bag.ItemSerial, index));
                            }

                            ev.Player.SendHint(EatingMessage, EatingMessageDuration);
                        }
                        else if (candies.Count > 0 && item is CustomCandy data && UnityEngine.Random.Range(0f, 101f) >= data.Chance && bag.Candies[index] == data.CandyType)
                        {
                            if (!ApplyEffects)
                            {
                                InventorySystem.Items.Usables.UsableItemsController.GetHandler(ev.Player.ReferenceHub).CurrentUsable.Item?.OnUsingCancelled();
                                ev.Player.Connection.Send(new InventorySystem.Items.Usables.StatusMessage(InventorySystem.Items.Usables.StatusMessage.StatusType.Cancel, bag.ItemSerial), 0);
                                ev.IsAllowed = false;
                                ev.ContinueProcess = false;
                                return;
                            }

                            OnEffectsApplying(ev);
                            Timing.CallDelayed(Timing.WaitForOneFrame, () => OnEffectsApplied(ev));

                            if (!DestroyOnUse)
                                Candyidx.Add((this, bag.ItemSerial, index));

                            if (data.DestroyOnUse)
                                (ev.UsableItem.Base as Scp330Bag)?.TryRemove(index);

                            ev.Player.SendHint(data.EatingMessage, data.EatingMessageDuration);
                        }   
                    }
                }
            }
        }

        /// <summary>
        /// Called after <see cref="OnUsing"/> setting ev.IsAllowed will do nothing
        /// </summary>
        /// <param name="ev"></param>
        protected virtual void OnUsed(PlayerUsingItemEventArgs ev) { }
        protected virtual void OnUsing(PlayerUsingItemEventArgs ev) { }
        protected virtual void OnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev) { }

        /// <summary>
        /// Called after <see cref="OnEffectsApplying"/> setting ev.IsAllowed will do nothing
        /// </summary>
        /// <param name="ev"></param>
        protected virtual void OnEffectsApplied(PlayerItemUsageEffectsApplyingEventArgs ev) { }
    }
}