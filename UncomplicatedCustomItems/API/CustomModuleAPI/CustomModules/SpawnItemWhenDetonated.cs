using InventorySystem.Items.Usables.Scp244;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using System;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class SpawnItemWhenDetonated : CustomModuleBase
    {
        public override string Name => "SpawnItemWhenDetonated";

        public string ItemType { get; set; } = string.Empty;
        public uint ItemId { get; set; }
        public float TimeTillDespawn { get; set; }
        public float Chance { get; set; } = 100f;
        public bool Pickupable { get; set; } = true;

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is ProjectileExplodedEventArgs ev)
            {
                float roll = UnityEngine.Random.Range(0f, 100f);
                if (roll < Chance)
                {
                    LogManager.Debug($"SpawnItemWhenDetonated triggered.");
                    string apiType = ItemType.ToLower();

                    if (apiType == "uci")
                    {
                        if (Utilities.TryGetCustomItem(ItemId, out CustomItem itemToSpawn))
                        {
                            SummonedCustomItem summonedItem = new(itemToSpawn, ev.Position);
                            if (!Pickupable && summonedItem.Pickup != null)
                            {
                                summonedItem.Pickup.Weight = 5000f;
                            }
                            if (TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                DespawnAfter(summonedItem.Serial, TimeTillDespawn);
                            }
                        }
                        else
                        {
                            LogManager.Warn($"{ItemId} is not a UCI CustomItem ID!");
                        }
                    }
#if EXILED
                    else if (apiType == "eci")
                    {
                        if (Exiled.CustomItems.API.Features.CustomItem.TryGet(ItemId, out Exiled.CustomItems.API.Features.CustomItem? ExCustomItem) && ExCustomItem != null)
                        {
                            Exiled.API.Features.Pickups.Pickup? exCustomItem = ExCustomItem.Spawn(ev.Position);
                            if (exCustomItem == null)
                            {
                                LogManager.Warn($"{ItemId} failed to spawn an Exiled CustomItem pickup!");
                                return;
                            }

                            if (!Pickupable)
                                exCustomItem.Weight = 5000f;

                            if (TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                DespawnAfter(exCustomItem.Serial, TimeTillDespawn);
                            }
                        }
                        else
                            LogManager.Warn($"{ItemId} is not an Exiled CustomItem ID!");
                    }
#endif
                    else if (apiType == "normal")
                    {
                        ItemType enumType = (ItemType)ItemId;
                        if (enumType == global::ItemType.SCP244a || enumType == global::ItemType.SCP244b)
                        {
                            LogManager.Debug($"Item is SCP244a or SCP244b");
                            Scp244Pickup? scp244Pickup = (Scp244Pickup?)Scp244Pickup.Create(enumType, ev.Position);
                            if (scp244Pickup == null)
                                return;

                            scp244Pickup.Base.MaxDiameter = 0.1f;
                            scp244Pickup.State = Scp244State.Active;
                            scp244Pickup.Spawn();
                            if (!Pickupable)
                                scp244Pickup.Weight = 5000f;

                            if (TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                DespawnAfter(scp244Pickup.Serial, TimeTillDespawn);
                            }
                        }
                        else
                        {
                            Pickup? pickup = Pickup.Create(enumType, ev.Position);
                            if (pickup == null)
                                return;

                            pickup.Transform.position += Vector3.up;
                            pickup.Spawn();
                            if (!Pickupable)
                                pickup.Weight = 5000f;

                            if (TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                DespawnAfter(pickup.Serial, TimeTillDespawn);
                            }
                        }
                    }
                }
            }
        }

        public override void RegisterEvents()
        {
            ServerEvents.ProjectileExploded += Run;
        }

        public override void UnregisterEvents()
        {
            ServerEvents.ProjectileExploded -= Run;
        }

        public static void DespawnAfter(ushort serial, float time)
        {
            Timing.CallDelayed(Timing.WaitForSeconds(time), () =>
            {
                if (Pickup.TryGet(serial, out var pickup))
                {
                    pickup.Destroy();
                    LogManager.Debug($"Destroyed pickup. Type: {pickup.Type} Previous owner: {pickup.LastOwner} Serial: {pickup.Serial}");
                }
            });
        }
    }
}

