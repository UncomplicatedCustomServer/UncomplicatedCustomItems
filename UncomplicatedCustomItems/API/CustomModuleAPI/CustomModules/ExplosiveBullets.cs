using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ExplosiveBullets : CustomModuleBase
    {
        public override string Name => "ExplosiveBullets";

        public float DamageRadius { get; set; }

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
                    grenade.FuseEnd();
                }
            }
        }

        public override void RegisterEvents()
        {
            AppendPrescanPatch.OnAppendPrescan += Prescan;
        }

        public override void UnregisterEvents()
        {
            AppendPrescanPatch.OnAppendPrescan -= Prescan;
        }
    }
}