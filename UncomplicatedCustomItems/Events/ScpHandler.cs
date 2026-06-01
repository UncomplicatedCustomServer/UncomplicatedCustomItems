using LabApi.Events.Arguments.Scp127Events;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;
using Scp914Event = LabApi.Events.Handlers.Scp914Events;

namespace UncomplicatedCustomItems.Events
{
    internal class ScpHandler
    {
        public static void Register()
        {
            Scp914Event.ProcessingPickup += OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem += OnItemUpgrade;
            Scp127Events.Talking += On127Talking;
            Scp127Events.GainingExperience += On127GainingExperience;
        }

        public static void Unregister()
        {
            Scp914Event.ProcessingPickup -= OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem -= OnItemUpgrade;
            Scp127Events.Talking -= On127Talking;
            Scp127Events.GainingExperience -= On127GainingExperience;
        }

        public static void OnPickupUpgrade(Scp914ProcessingPickupEventArgs ev)
        {
            if (ev.Pickup.IsCustomItem() || ev.Pickup.IsSummonedAPICustomItem())
            {
                ev.IsAllowed = false;
                ev.Pickup.Position = ev.NewPosition;
            }
        }

        public static void OnItemUpgrade(Scp914ProcessingInventoryItemEventArgs ev)
        {
            if (ev.Item.IsSummonedCustomItem() || ev.Item.IsSummonedAPICustomItem())
                ev.IsAllowed = false;
        }

        private static void On127Talking(Scp127TalkingEventArgs ev)
        {
            if (SummonedAPICustomItem.TryGet(ev.Scp127Item.Serial, out var api) && api != null && api.CustomItem is CustomSCP127 customSCP127 && customSCP127.MuteVoiceLines)
                ev.IsAllowed = false;

            if (Utilities.TryGetSummonedCustomItem(ev.Scp127Item.Serial, out var item) && item != null && item.CustomItem.CustomItemType is CustomItemType.SCPItem && item.CustomItem.CustomData is SCP127Data scp127Data && scp127Data.MuteVoiceLines)
                ev.IsAllowed = false;
        }

        private static void On127GainingExperience(Scp127GainingExperienceEventArgs ev)
        {
            if (SummonedAPICustomItem.TryGet(ev.Scp127Item.Serial, out var api) && api != null && api.CustomItem is CustomSCP127 customSCP127 && !customSCP127.AllowXPGain)
                ev.IsAllowed = false;

            if (Utilities.TryGetSummonedCustomItem(ev.Scp127Item.Serial, out var item) && item != null && item.CustomItem.CustomItemType is CustomItemType.SCPItem && item.CustomItem.CustomData is SCP127Data scp127Data && !scp127Data.AllowXPGain)
                ev.IsAllowed = false;
        }
    }
}