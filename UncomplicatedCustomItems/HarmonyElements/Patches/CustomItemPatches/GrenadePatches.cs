using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Extensions;
using Mirror;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{

    [HarmonyPatch(typeof(TimeGrenade), nameof(TimeGrenade.ServerActivate))]
    public static class GrenadePatches
    {
        [HarmonyPrefix]
        public static void TimeGrenadeServerActivatePrefix(TimeGrenade __instance)
        {
            if (!NetworkServer.active)
                return;

            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemId.SerialNumber, out var customItem) || !SummonedAPICustomItem.TryGet(__instance.ItemId.SerialNumber, out var summonItem))
                return;

            if (summonItem?.CustomItem is CustomExplosiveGrenade apiGrenade)
                ApplyFromApi(__instance as ExplosionGrenade, apiGrenade);

            if (summonItem?.CustomItem is CustomFlashGrenade apiFlash)
                ApplyFromFlashApi(__instance as FlashbangGrenade, apiFlash);

            if (customItem?.CustomItem.CustomData is ExplosiveGrenadeData data)
                ApplyFromData(__instance as ExplosionGrenade, data);

            if (customItem?.CustomItem.CustomData is FlashGrenadeData flashData)
                ApplyFromFlashData(__instance as FlashbangGrenade, flashData);        
        }

        private static void ApplyFromFlashData(FlashbangGrenade inst, FlashGrenadeData data)
        {
            if (inst == null || data == null)
                return;

            inst.BlindTime = data.AdditionalBlindedEffect;
            inst._fuseTime = data.FuseTime;
            inst._surfaceZoneDistanceIntensifier = data.SurfaceDistanceIntensifier;
            inst._minimalEffectDuration = data.MinimalDurationEffect;
        }

        private static void ApplyFromFlashApi(FlashbangGrenade inst, CustomFlashGrenade api)
        {
            if (inst == null || api == null)
                return;

            inst.BlindTime = api.AdditionalBlindedEffect;
            inst._fuseTime = api.FuseTime;
            inst._surfaceZoneDistanceIntensifier = api.SurfaceDistanceIntensifier;
            inst._minimalEffectDuration = api.MinimalDurationEffect;
        }


        private static void ApplyFromData(ExplosionGrenade inst, ExplosiveGrenadeData data)
        {
            if (inst == null || data == null)
                return;

            inst.MaxRadius = data.MaxRadius;
            inst.ScpDamageMultiplier = data.ScpDamageMultiplier;
            inst._burnedDuration = data.BurnDuration;
            inst._deafenedDuration = data.DeafenDuration;
            inst._concussedDuration = data.ConcussDuration;
            inst._fuseTime = data.FuseTime;
            inst._doorDamageOverDistance.Multiply(data.DoorDamageMultiplier);
            inst._playerDamageOverDistance.Multiply(data.PlayerDamageMultiplier);
        }

        private static void ApplyFromApi(ExplosionGrenade inst, CustomExplosiveGrenade api)
        {
            if (inst == null || api == null)
                return;

            inst.MaxRadius = api.MaxRadius;
            inst.ScpDamageMultiplier = api.ScpDamageMultiplier;
            inst._burnedDuration = api.BurnDuration;
            inst._deafenedDuration = api.DeafenDuration;
            inst._concussedDuration = api.ConcussDuration;
            inst._fuseTime = api.FuseTime;
            inst._doorDamageOverDistance.Multiply(api.DoorDamageMultiplier);
            inst._playerDamageOverDistance.Multiply(api.PlayerDamageMultiplier);
        }
    }
}
