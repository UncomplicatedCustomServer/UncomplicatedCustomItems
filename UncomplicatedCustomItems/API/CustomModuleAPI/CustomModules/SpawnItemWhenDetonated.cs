using InventorySystem.Items.Usables.Scp244;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class SpawnItemWhenDetonated : CustomModuleBase
    {
        public override string Name => "SpawnItemWhenDetonated";
        public override List<string> RequiredArguments =>
        [
            "ItemType",
            "ItemId",
            "TimeTillDespawn",
            "Chance",
            "Pickupable",
        ];

        public string ApiType { get; set; } = string.Empty;
        public uint ItemId { get; set; }
        public float TimeTillDespawn { get; set; }
        public float Chance { get; set; }
        public bool Pickupable { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<string>("ItemType", out var itemType))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ItemType is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<uint>("ItemId", out var itemId))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ItemId is not a valid uint!");
                    return;
                }

                if (!args.TryGetValue<float>("TimeTillDespawn", out var timeTillDespawn))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} TimeTillDespawn is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("Chance", out var chance))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Chance is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<bool>("Pickupable", out var pickupable))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Pickupable is not a valid Boolean!");
                    return;
                }

                ApiType = itemType;
                ItemId = itemId;
                TimeTillDespawn = timeTillDespawn;
                Chance = chance;
                Pickupable = pickupable;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is ProjectileExplodedEventArgs ev)
            {
                float chance = UnityEngine.Random.Range(0f, 101f);
                if (chance >= Chance)
                {
                    LogManager.Debug($"Loaded FlagSettings.");
                    if (ApiType.ToLower() == "uci")
                    {
                        if (Utilities.TryGetCustomItem((uint)ItemId, out ICustomItem itemToSpawn))
                        {
                            SummonedCustomItem summonedItem = new(itemToSpawn, ev.Position);
                            if (Pickupable == false)
                            {
                                summonedItem.Pickup?.Weight = 5000f;
                            }
                            if (TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                DespawnAfter(summonedItem.Serial, (float)TimeTillDespawn);
                            }
                        }
                        else
                            LogManager.Warn($"{ItemId} is not a UCI CustomItem ID!");
                    }
#if EXILED
                        else if (ApiType == "ECI" || ApiType == "eci")
                        {
                            if (Exiled.CustomItems.API.Features.CustomItem.TryGet((uint)ItemId, out Exiled.CustomItems.API.Features.CustomItem ExCustomItem))
                            {
                                Exiled.API.Features.Pickups.Pickup exCustomItem = ExCustomItem.Spawn(ev.Position);
                                if (!Pickupable)
                                    exCustomItem.Weight = 5000f;

                                if (TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    DespawnAfter(exCustomItem.Serial, (float)TimeTillDespawn);
                                }
                            }
                            else
                                LogManager.Warn($"{ItemId} is not a Exiled CustomItem ID!");
                        }
#endif
                    else if (ApiType.ToLower() == "normal")
                    {
                        if ((ItemType)ItemId == ItemType.SCP244a || (ItemType)ItemId == ItemType.SCP244b)
                        {
                            LogManager.Debug($"Item is SCP244a or SCP244b");
                            Scp244Pickup? scp244Pickup = (Scp244Pickup?)Scp244Pickup.Create((ItemType)ItemId, ev.Position);
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
                                DespawnAfter(scp244Pickup.Serial, (float)TimeTillDespawn);
                            }
                        }
                        else
                        {
                            Pickup? pickup = Pickup.Create((ItemType)ItemId, ev.Position);
                            if (pickup == null)
                                return;
                                
                            Vector3 vector3 = new(0f, 1f, 0f);
                            pickup.Transform.position = pickup.Transform.position + vector3;
                            pickup.Spawn();
                            if (!Pickupable)
                                pickup.Weight = 5000f;

                            if (TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                DespawnAfter(pickup.Serial, (float)TimeTillDespawn);
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
