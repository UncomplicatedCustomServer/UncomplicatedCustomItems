using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using EventSource = LabApi.Events.Handlers.PlayerEvents;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using MapEventSource = LabApi.Events.Handlers.ServerEvents;
using InventorySystem.Items.Firearms.Modules.Scp127;
using MEC;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using UncomplicatedCustomItems.Extensions;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Wrappers;
using PlayerRoles;

namespace UncomplicatedCustomItems.Events.Internal
{
    internal static class Player
    {   //EventSource.EVENT += EVENTNAME
        public static void Register()
        {
            EventSource.PickedUpItem += ShowItemInfoOnItemAdded;
            EventSource.DroppedItem += DroppedItemEvent;
            EventSource.ChangedItem += ChangeItemInHand;
            EventSource.ChangingItem += ChangingItemInHand;
            EventSource.UsedItem += OnItemUsingCompleted;
            EventSource.ToggledNoclip += NoclipButton;
            EventSource.Dying += DeathEvent;
            EventSource.ChangingRole += RoleChangeEvent;
            EventSource.ThrewProjectile += ThrownProjectile;
            EventSource.Hurting += Damaged;
            MapEventSource.ProjectileExploded += GrenadeExploded;
        }
        // EventSource.EVENT -= EVENTNAME 
        public static void Unregister()
        {
            EventSource.PickedUpItem -= ShowItemInfoOnItemAdded;
            EventSource.DroppedItem -= DroppedItemEvent;
            EventSource.ChangedItem -= ChangeItemInHand;
            EventSource.ChangingItem -= ChangingItemInHand;
            EventSource.UsedItem -= OnItemUsingCompleted;
            EventSource.Dying -= DeathEvent;
            EventSource.ChangingRole -= RoleChangeEvent;
            EventSource.ThrewProjectile -= ThrownProjectile;
            EventSource.Hurting -= Damaged;
            EventSource.ToggledNoclip -= NoclipButton;
            MapEventSource.ProjectileExploded -= GrenadeExploded;
        }

        public static void Damaged(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker.CurrentItem == null || ev.Player == null || ev.Player.Role == RoleTypeId.Destroyed || ev.Player.Role == RoleTypeId.Spectator)
                return;
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
                {
                    IWeaponData WeaponData = CustomItem.CustomItem.CustomData as IWeaponData;
                    if (CustomItem.Item.Type.IsWeapon())
                    {
                        if (WeaponData.EnableFriendlyFire)
                        {
                            if (ev.Player != null)
                            {
                                ev.Player.Damage(WeaponData.Damage, ev.Attacker);
                                ev.Attacker.SendHitMarker();
                            }
                        }
                    }
                }
        }

        private static void GrenadeExploded(ProjectileExplodedEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.TimedGrenade.Serial, out SummonedCustomItem Item))
                return;

            Item?.HandleEvent(ev.Player, ItemEvents.Detonation, ev.TimedGrenade.Serial); // Untested
        }

        private static void DroppedItemEvent(PlayerDroppedItemEventArgs ev)
        {
            if (ev.Pickup == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem Item))
                return;

            Item?.OnDrop(ev);
            Item.ResetBadge(ev.Player);
            if (Item.HasModule(Enums.CustomFlags.ToolGun))
            {
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
                EventHandler.StopRelativePosCoroutine(ev.Player);
            }
            EventHandler.StopHumeShieldRegen(ev.Player);
        }

        /// <summary>
        /// Show item name if it is custom item
        /// </summary>
        /// <param name="ev"></param>
        private static void ShowItemInfoOnItemAdded(PlayerPickedUpItemEventArgs ev)
        {
            if (ev.Item is null)
                return;
            if (ev.Player is null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
            {
                Item.OnPickup(ev);
                Item.HandlePickedUpDisplayHint();
            }
        }

        private static void OnItemUsingCompleted(PlayerUsedItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.UsableItem == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem Item))
                return;

            if (Item is null)
                return;
            
            Item.HandleEvent(ev.Player, ItemEvents.Use, ev.UsableItem.Serial);

            Item?.ResetBadge(ev.Player);

            if (Item.CustomItem.Reusable)
                new SummonedCustomItem(Item.CustomItem, ev.Player);
        }

        private static void ChangeItemInHand(PlayerChangedItemEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (ev.NewItem is null)
                return;

            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.NewItem.Serial, out SummonedCustomItem item))
                return;

            item.HandleSelectedDisplayHint();
            item.LoadBadge(ev.Player);
        }

        private static void ChangingItemInHand(PlayerChangingItemEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
            {
                if (EventHandler.EquipedKeycards.ContainsKey(ev.Player.CurrentItem.Serial))
                    EventHandler.EquipedKeycards.Remove(ev.Player.CurrentItem.Serial);
                return;
            }
            if (EventHandler.EquipedKeycards.ContainsKey(item.Serial))
                EventHandler.EquipedKeycards.Remove(item.Serial);

            item.ResetBadge(ev.Player);

            if (item.HasModule(Enums.CustomFlags.ToolGun))
            {
                EventHandler.StopRelativePosCoroutine(ev.Player);
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
            }

            if (item.Item.Type == ItemType.GunSCP127 && item.CustomItem.CustomItemType == CustomItemType.SCPItem)
            {
                ISCP127Data data = item.CustomItem.CustomData as ISCP127Data;
                Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(item.Item.Base);
                if (tier == Scp127Tier.Tier1)
                {
                    ev.Player.HumeShieldRegenRate = 0f;
                    Timing.RunCoroutine(DecayRate(ev.Player, data.Tier1ShieldDecayRate));
                }
                else if (tier == Scp127Tier.Tier2)
                {
                    ev.Player.HumeShieldRegenRate = 0f;
                    Timing.RunCoroutine(DecayRate(ev.Player, data.Tier2ShieldDecayRate));
                }
                else if (tier == Scp127Tier.Tier3)
                {
                    ev.Player.HumeShieldRegenRate = 0f;
                    Timing.RunCoroutine(DecayRate(ev.Player, data.Tier3ShieldDecayRate));
                }
                else
                    LogManager.Error($"{item.CustomItem.Name} - {item.Serial} has no tier?");
                EventHandler.StopHumeShieldRegen(ev.Player);
            }
        }

        internal static IEnumerator<float> DecayRate(LabApi.Features.Wrappers.Player player, float DecayRate)
        {
            for (; ; )
            {
                if (player.HumeShield >= 0)
                {
                    player.HumeShield -= Time.deltaTime * DecayRate;
                    yield return Timing.WaitForOneFrame;
                }
                else
                {
                    yield break;
                }
            }
        }

        private static void DeathEvent(PlayerDyingEventArgs ev)
        {
            if (!ev.Player.Connection.isReady)
                return;

            if (ev.Player == null)
                return;

            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customitem))
                {
                    customitem.OnDied(ev, customitem);
                    customitem?.ResetBadge(ev.Player);
                    if (customitem.HasModule(Enums.CustomFlags.ToolGun))
                    {
                        SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
                        EventHandler.StopRelativePosCoroutine(ev.Player);
                    }

                }
            }
            EventHandler.StopHumeShieldRegen(ev.Player);
        }

        private static void RoleChangeEvent(PlayerChangingRoleEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!ev.Player.Connection.isReady)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
            if (item.HasModule(Enums.CustomFlags.ToolGun))
            {
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
                EventHandler.StopRelativePosCoroutine(ev.Player);
            }
            EventHandler.StopHumeShieldRegen(ev.Player);
        }

        private static void ThrownProjectile(PlayerThrewProjectileEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
        }

        private static void NoclipButton(PlayerToggledNoclipEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
                return;

            Item?.HandleEvent(ev.Player, ItemEvents.Noclip, ev.Player.CurrentItem.Serial);
        }
    }
}