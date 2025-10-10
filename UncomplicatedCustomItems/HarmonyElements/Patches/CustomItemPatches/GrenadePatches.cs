using System.Collections.Concurrent;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using LabApi.Features.Wrappers;
using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;
using MEC;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    // TODO:
    // Test the PinPullTime and Repickable patches
    // Test SCP018 FuseTime, FriendlyFireTime and Repickupable patches
    [HarmonyPatch(typeof(ThrowableItem))]
    internal static class ThrowableItemPatches
    {

        private static readonly ConcurrentDictionary<ushort, IData> _cachedCustomData = new();

        [HarmonyPatch(nameof(ThrowableItem.OnAdded))]
        [HarmonyPostfix]
        public static void OnAddedCacheCustomItem(ThrowableItem __instance)
        {
            _cachedCustomData.GetOrAdd(__instance.ItemSerial, key =>
            {
                if (Utilities.TryGetSummonedCustomItem(key, out var summoned))
                {
                    LogManager.Debug($"Caching summoned custom item: {summoned.CustomItem.Name} ({summoned.CustomItem.Id}) for serial {key}");
                    return summoned.CustomItem.CustomData;
                }

                return null;
            });
        }

        [HarmonyPatch(nameof(ThrowableItem.OnRemoved))]
        [HarmonyPostfix]
        public static void OnRemovedClearCache(ThrowableItem __instance) =>
            _cachedCustomData.TryRemove(__instance.ItemSerial, out _);

        [HarmonyPatch(nameof(ThrowableItem.OnHolstered))]
        [HarmonyPostfix]
        public static void OnHolsteredClearCache(ThrowableItem __instance) =>
            _cachedCustomData.TryRemove(__instance.ItemSerial, out _);

        [HarmonyPatch(nameof(ThrowableItem.UpdateServer))]
        [HarmonyPrefix]
        public static void UpdateServerApplyCustomValues(ThrowableItem __instance)
        {
            if (_cachedCustomData.TryGetValue(__instance.ItemSerial, out var entry) && entry != null)
            {
                ApplyItemValues(__instance, entry);
                return;
            }

            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var summoned))
            {
                IData cd = summoned.CustomItem.CustomData;
                if (cd != null)
                {
                    _cachedCustomData[__instance.ItemSerial] = cd;
                    LogManager.Debug($"{nameof(UpdateServerApplyCustomValues)}: Resolved & cached CustomData for serial {__instance.ItemSerial} - name: {summoned.CustomItem.Name}");
                    ApplyItemValues(__instance, cd);
                }
                else
                    LogManager.Debug($"{nameof(UpdateServerApplyCustomValues)}: Summoned found for serial {__instance.ItemSerial} - name: {summoned.CustomItem.Name} but CustomData is NULL. Will retry later.");
            }
            else
                LogManager.Debug($"{nameof(UpdateServerApplyCustomValues)}: No summoned custom item for serial {__instance.ItemSerial}. Will retry later.");
        }

        private static void ApplyItemValues(ThrowableItem instance, object data)
        {
            switch (data)
            {
                case IExplosiveGrenadeData grenadeData:
                    instance._pinPullTime = grenadeData.PinPullTime;
                    instance._repickupable = grenadeData.Repickable;
                    break;

                case IFlashGrenadeData flashData:
                    instance._pinPullTime = flashData.PinPullTime;
                    instance._repickupable = flashData.Repickable;
                    break;

                case ISCP018Data scp018Data:
                    instance._pinPullTime = scp018Data.ThrowTime;
                    instance._repickupable = scp018Data.Repickable;
                    break;

                default:
                    if (Utilities.TryGetSummonedCustomItem(instance.ItemSerial, out var summoned) && summoned.CustomItem.CustomData is not ItemData)
                        LogManager.Info($"{nameof(UpdateServerApplyCustomValues)}: CustomItem - {summoned.CustomItem.Name} is not a supported ThrowableItem data type.");
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(TimeGrenade), nameof(TimeGrenade.ServerActivate))]
    internal static class GrenadePatches
    {
        [HarmonyPostfix]
        public static void TimeGrenadeServerActivatePostfix(TimeGrenade __instance)
        {
            LogManager.Debug("Called");
            if (SummonedAPICustomItem.TryGet(__instance.ItemId.SerialNumber, out var summonItem))
            {
                switch (summonItem.CustomItem)
                {
                    case CustomExplosiveGrenade apiGrenade when __instance is ExplosionGrenade grenade:
                        LogManager.Debug($"Applying CustomExplosiveGrenade values to grenade with serial {__instance.ItemId.SerialNumber}");
                        LabApi.Features.Wrappers.ThrowableItem.Get(Item.Get(__instance.Info.Serial).Base as ThrowableItem).Base._repickupable = apiGrenade.Repickable;
                        ApplyFromApi(ExplosiveGrenadeProjectile.Get(grenade), apiGrenade);
                        break;
                    case CustomFlashGrenade apiFlash when __instance is FlashbangGrenade flash:
                        LogManager.Debug($"Applying CustomFlashGrenade values to grenade with serial {__instance.ItemId.SerialNumber}");
                        LabApi.Features.Wrappers.ThrowableItem.Get(Item.Get(__instance.Info.Serial).Base as ThrowableItem).Base._repickupable = apiFlash.Repickable;
                        ApplyFromFlashApi(FlashbangProjectile.Get(flash), apiFlash);
                        break;
                    case CustomSCP018 scp018 when __instance is InventorySystem.Items.ThrowableProjectiles.Scp018Projectile scp018proj:
                        LogManager.Debug($"Applying CustomSCP018 values to grenade with serial {__instance.ItemId.SerialNumber}");
                        LabApi.Features.Wrappers.ThrowableItem.Get(Item.Get(__instance.Info.Serial).Base as ThrowableItem).Base._repickupable = scp018.Repickable;
                        ApplyFrom018Api(LabApi.Features.Wrappers.Scp018Projectile.Get(scp018proj), scp018);
                        break;
                    default:
                        LogManager.Debug($"Couldnt assign with serial {__instance.ItemId.SerialNumber}");
                        break;
                };
            }
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemId.SerialNumber, out var customItem))
            {
                switch (customItem.CustomItem.CustomData)
                {
                    case ExplosiveGrenadeData grenadedata when __instance is ExplosionGrenade grenade:
                        LogManager.Debug($"Applying CustomExplosiveGrenade values to grenade with serial {__instance.ItemId.SerialNumber}");
                        LabApi.Features.Wrappers.ThrowableItem.Get(Item.Get(__instance.Info.Serial).Base as ThrowableItem).Base._repickupable = grenadedata.Repickable;
                        ApplyFromData(ExplosiveGrenadeProjectile.Get(grenade), grenadedata);
                        break;
                    case FlashGrenadeData flashData when __instance is FlashbangGrenade flash:
                        LogManager.Debug($"Applying CustomFlashGrenade values to grenade with serial {__instance.ItemId.SerialNumber}");
                        LabApi.Features.Wrappers.ThrowableItem.Get(Item.Get(__instance.Info.Serial).Base as ThrowableItem).Base._repickupable = flashData.Repickable;
                        ApplyFromFlashData(FlashbangProjectile.Get(flash), flashData);
                        break;
                    case SCP018Data scp018data when __instance is InventorySystem.Items.ThrowableProjectiles.Scp018Projectile scp018proj:
                        LogManager.Debug($"Applying CustomSCP018 values to grenade with serial {__instance.ItemId.SerialNumber}");
                        LabApi.Features.Wrappers.ThrowableItem.Get(Item.Get(__instance.Info.Serial).Base as ThrowableItem).Base._repickupable = scp018data.Repickable;
                        ApplyFrom018Data(LabApi.Features.Wrappers.Scp018Projectile.Get(scp018proj), scp018data);
                        break;
                    default:
                        LogManager.Debug($"Couldnt assign with serial {__instance.ItemId.SerialNumber}");
                        break;
                };
            }
        }

        private static void ApplyFrom018Data(LabApi.Features.Wrappers.Scp018Projectile inst, SCP018Data data)
        {
            if (inst == null || data == null)
                return;

            inst.Base._friendlyFireTime = data.FriendlyFireTime;
            inst.RemainingTime = data.FuseTime;
        }

        private static void ApplyFrom018Api(LabApi.Features.Wrappers.Scp018Projectile instance, CustomSCP018 api)
        {
            if (instance == null || api == null)
                return;

            instance.Base._friendlyFireTime = api.FriendlyFireTime;
            instance.RemainingTime = api.FuseTime;
        }

        private static void ApplyFromFlashData(FlashbangProjectile instance, FlashGrenadeData data)
        {
            if (instance == null || data == null)
                return;

            instance.BaseBlindTime = data.AdditionalBlindedEffect;
            instance.RemainingTime = data.FuseTime;
            instance.Base._surfaceZoneDistanceIntensifier = data.SurfaceDistanceIntensifier;
            instance.Base._minimalEffectDuration = data.MinimalDurationEffect;
        }

        private static void ApplyFromFlashApi(FlashbangProjectile instance, CustomFlashGrenade api)
        {
            if (instance == null || api == null)
                return;

            instance.BaseBlindTime = api.AdditionalBlindedEffect;
            instance.RemainingTime = api.FuseTime;
            instance.Base._surfaceZoneDistanceIntensifier = api.SurfaceDistanceIntensifier;
            instance.Base._minimalEffectDuration = api.MinimalDurationEffect;
        }

        private static void ApplyFromData(ExplosiveGrenadeProjectile instance, ExplosiveGrenadeData data)
        {
            if (instance == null || data == null)
                return;

            instance.MaxRadius = data.MaxRadius;
            instance.ScpDamageMultiplier = data.ScpDamageMultiplier;
            instance.Base._burnedDuration = data.BurnDuration;
            instance.Base._deafenedDuration = data.DeafenDuration;
            instance.Base._concussedDuration = data.ConcussDuration;
            instance.RemainingTime = data.FuseTime;
            instance.Base._doorDamageOverDistance.Multiply(data.DoorDamageMultiplier);
            instance.Base._playerDamageOverDistance.Multiply(data.PlayerDamageMultiplier);
        }

        private static void ApplyFromApi(ExplosiveGrenadeProjectile instance, CustomExplosiveGrenade api)
        {
            if (instance == null || api == null)
                return;

            instance.MaxRadius = api.MaxRadius;
            instance.ScpDamageMultiplier = api.ScpDamageMultiplier;
            instance.Base._burnedDuration = api.BurnDuration;
            instance.Base._deafenedDuration = api.DeafenDuration;
            instance.Base._concussedDuration = api.ConcussDuration;
            instance.RemainingTime = api.FuseTime;
            instance.Base._doorDamageOverDistance.Multiply(api.DoorDamageMultiplier);
            instance.Base._playerDamageOverDistance.Multiply(api.PlayerDamageMultiplier);
        }
    }
}
