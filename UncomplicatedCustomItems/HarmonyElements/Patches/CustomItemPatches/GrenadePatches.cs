using System;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Features.Helper;
using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;
using Footprinting;
using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using Mirror;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UnityEngine;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    /*
    // TODO:
    // Test the PinPullTime and Repickable patches
    // Test SCP018 FuseTime, FriendlyFireTime and Repickupable patches
    [HarmonyPatch(typeof(ThrowableItem))]
    internal static class ThrowableItemPatches
    {
        [HarmonyPatch(typeof(ThrowableItem), nameof(ThrowableItem.ServerThrow), new[] { typeof(float), typeof(float), typeof(Vector3), typeof(Vector3) })]
        public static bool Prefix(ThrowableItem __instance, float forceAmount, float upwardFactor, Vector3 torque, Vector3 startVel, ref ThrownProjectile __result)
        {
            try
            {
                if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item))
                {
                    switch (item.CustomItem.CustomData, item.CustomItem.CustomItemType)
                    {
                        case (ExplosiveGrenadeData data, CustomItemType.ExplosiveGrenade):
                            GameObject go = new("UCI_Custom_ExplosionGrenade");
                            ExplosionGrenade grenade = go.AddComponent<ExplosionGrenade>();
                            
                            grenade.MaxRadius = data.MaxRadius;
                            grenade.ScpDamageMultiplier = data.ScpDamageMultiplier;
                            grenade._burnedDuration = data.BurnDuration;
                            grenade._concussedDuration = data.ConcussDuration;
                            grenade._deafenedDuration = data.DeafenDuration;
                            grenade._fuseTime = data.FuseTime;
                            grenade._doorDamageOverDistance.Multiply(data.DoorDamageMultiplier);
                            grenade._playerDamageOverDistance.Multiply(data.PlayerDamageMultiplier);

                            Transform camera = __instance.Owner.PlayerCameraReference;
                            grenade.transform.position = camera.position;
                            grenade.transform.rotation = camera.rotation;
                            grenade.gameObject.SetActive(true);

                            PickupSyncInfo networkInfo = new(__instance.ItemTypeId, __instance.Weight, __instance.ItemSerial);
                            networkInfo.Locked = !data.Repickable;

                            if (grenade is ThrownProjectile thrown)
                            {
                                thrown.NetworkInfo = networkInfo;
                                thrown.PreviousOwner = new Footprint(__instance.Owner);
                                NetworkServer.Spawn(thrown.gameObject);

                                Transform playerCamera = __instance.Owner.PlayerCameraReference;
                                float num = 1f - Mathf.Abs(Vector3.Dot(playerCamera.forward, Vector3.up));
                                Vector3 camVec = playerCamera.forward + playerCamera.up * upwardFactor * num;
                                Vector3 velocityVector = camVec * forceAmount + ThrowableNetworkHandler.GetLimitedVelocity(startVel);

                                if (thrown.TryGetComponent<Rigidbody>(out var rb))
                                {
                                    rb.centerOfMass = Vector3.zero;
                                    rb.angularVelocity = torque;
                                    rb.velocity = velocityVector;
                                }

                                thrown.ServerOnThrown(torque, velocityVector);
                                thrown.ServerActivate();

                                __result = thrown;
                                return false;
                            }

                            break;
                            
                        case (FlashGrenadeData flashdata, CustomItemType.FlashGrenade):
                            break;

                        case (SCP018Data scpdata, CustomItemType.SCPItem):
                            break;
                    }
                }
                else if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiItem))
                {
                    switch (apiItem.CustomItem)
                    {
                        case CustomExplosiveGrenade apidata:
                            GameObject go = new("UCI_API_ExplosionGrenade");
                            ExplosionGrenade grenade = go.AddComponent<ExplosionGrenade>();

                            grenade.MaxRadius = apidata.MaxRadius;
                            grenade.ScpDamageMultiplier = apidata.ScpDamageMultiplier;
                            grenade._burnedDuration = apidata.BurnDuration;
                            grenade._concussedDuration = apidata.ConcussDuration;
                            grenade._deafenedDuration = apidata.DeafenDuration;
                            grenade._fuseTime = apidata.FuseTime;
                            grenade._doorDamageOverDistance.Multiply(apidata.DoorDamageMultiplier);
                            grenade._playerDamageOverDistance.Multiply(apidata.PlayerDamageMultiplier);

                            Transform camera = __instance.Owner.PlayerCameraReference;
                            grenade.transform.position = camera.position;
                            grenade.transform.rotation = camera.rotation;
                            grenade.gameObject.SetActive(true);

                            PickupSyncInfo networkInfo = new(__instance.ItemTypeId, __instance.Weight, __instance.ItemSerial);
                            networkInfo.Locked = !apidata.Repickable;

                            if (grenade is ThrownProjectile thrown)
                            {
                                thrown.NetworkInfo = networkInfo;
                                thrown.PreviousOwner = new Footprint(__instance.Owner);
                                NetworkServer.Spawn(thrown.gameObject);

                                Transform playerCamera = __instance.Owner.PlayerCameraReference;
                                float num = 1f - Mathf.Abs(Vector3.Dot(playerCamera.forward, Vector3.up));
                                Vector3 camVec = playerCamera.forward + playerCamera.up * upwardFactor * num;
                                Vector3 velocityVector = camVec * forceAmount + ThrowableNetworkHandler.GetLimitedVelocity(startVel);

                                if (thrown.TryGetComponent<Rigidbody>(out var rb))
                                {
                                    rb.centerOfMass = Vector3.zero;
                                    rb.angularVelocity = torque;
                                    rb.velocity = velocityVector;
                                }

                                thrown.ServerOnThrown(torque, velocityVector);
                                thrown.ServerActivate();

                                __result = thrown;
                                return false;
                            }

                            break;

                        case CustomFlashGrenade:
                            break;
                            
                        case CustomSCP018:
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"CustomGrenade_Prefix exception: {ex}");
                return true;
            }
        }
    }
        */
}