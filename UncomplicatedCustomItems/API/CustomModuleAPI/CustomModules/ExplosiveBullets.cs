using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ExplosiveBullets : CustomModuleBase
    {
        public override string Name => "ExplosiveBullets";

        public float DamageRadius { get; set; }
        public float SCPDamageMultiplier { get; set; }
        public float DoorDamageMultiplier { get; set; }
        public float PlayerDamageMultiplier { get; set; }

        public void Prescan(Player player, Item item, Ray ray, HitscanResult result)
        {
            if (!Check(item))
                return;

            foreach (HitRayPair pair in result.Obstacles)
            {
                ExplosiveGrenadeProjectile? grenade = (ExplosiveGrenadeProjectile?)TimedGrenadeProjectile.SpawnActive(pair.Hit.point, ItemType.GrenadeHE, player, 0.2);
                if (grenade != null)
                {
                    grenade.MaxRadius = DamageRadius;
                    grenade.Base.ScpDamageMultiplier = SCPDamageMultiplier;
                    grenade.Base._playerDamageOverDistance.Multiply(PlayerDamageMultiplier);
                    grenade.Base._doorDamageOverDistance.Multiply(DoorDamageMultiplier);
                    grenade.FuseEnd();
                }
            }
        }

        public void Prescan(Player player, Item item, DestructibleHitPair pair, HitscanResult result)
        {
            if (!Check(item))
                return;

                ExplosiveGrenadeProjectile? grenade = (ExplosiveGrenadeProjectile?)TimedGrenadeProjectile.SpawnActive(pair.Hit.point, ItemType.GrenadeHE, player, 0.2);
                if (grenade != null)
                {
                    grenade.MaxRadius = DamageRadius;
                    grenade.Base.ScpDamageMultiplier = SCPDamageMultiplier;
                    grenade.Base._playerDamageOverDistance.Multiply(PlayerDamageMultiplier);
                    grenade.Base._doorDamageOverDistance.Multiply(DoorDamageMultiplier);
                    grenade.FuseEnd();
                }
        }

        public override void RegisterEvents()
        {
            HitscanHitregModuleBasePatch.OnDamageDestructible += Prescan;
            HitscanHitregModuleBasePatch.OnAppendPrescan += Prescan;
        }

        public override void UnregisterEvents()
        {
            HitscanHitregModuleBasePatch.OnDamageDestructible -= Prescan;
            HitscanHitregModuleBasePatch.OnAppendPrescan -= Prescan;
        }
    }
}