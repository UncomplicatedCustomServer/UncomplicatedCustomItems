using Exiled.Events.EventArgs.Item;
using MEC;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems.Events
{
    internal class ItemHandler
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Item.ChargingJailbird += OnCharge;
            Exiled.Events.Handlers.Item.ChangingAttachments += OnChangingAttachments;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Item.ChargingJailbird -= OnCharge;
            Exiled.Events.Handlers.Item.ChangingAttachments -= OnChangingAttachments;
        }

        public static void OnCharge(ChargingJailbirdEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;

            if (CustomItem.HasModule(CustomFlags.NoCharge))
            {
                if (ev.Item != null)
                {
                    ev.IsAllowed = false;
                    Timing.CallDelayed(0.1f, () =>
                    {
                        ev.Player.CurrentItem = ev.Item;
                    });
                }
            }

            PlayerHandler.ChargeAttack = true;
            if (CustomItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                AudioApi AudioApi = new();
                if (ev.Item != null)
                {
                    LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {CustomItem.CustomItem.Name}.");
                    AudioApi.PlayAudio(CustomItem, ev.Player.Position);
                }
            }
            else return;
        }

        public static void OnChangingAttachments(ChangingAttachmentsEventArgs ev)
        {
            if (ev.Item == null || ev.Player == null || ev.Firearm == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.WorkstationBan))
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
            }
            else return;
        }
    }
}
