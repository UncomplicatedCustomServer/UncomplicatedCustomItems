using System.Linq;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.ShotEvents;
using InventorySystem.Items.Firearms;
using InventorySystem;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using UnityEngine;
using UncomplicatedCustomItems.API.Features;
using CustomPlayerEffects;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class PlayerExtensions
    {
        /// <summary>
        /// Checks whether the player has a keycard of a specific permission.
        /// </summary>
        /// <param name="player"><see cref="Player" /> trying to interact.</param>
        /// <param name="door"></param>
        /// <returns>Whether the player has the required keycard.</returns>
        internal static bool HasKeycardPermission(this Player player, IDoorPermissionRequester door) =>
            player.CurrentItem is KeycardItem keycard && player.CurrentItem.Base is IDoorPermissionProvider keycardProvider && door is IDoorPermissionRequester permissions && permissions.PermissionsPolicy.CheckPermissions(keycardProvider.GetPermissions(permissions));

        public static bool IsAimingDownWeapon(this Player player)
        {
            FirearmItem firearm = player.CurrentItem as FirearmItem;
            if (firearm.IsAiming())
                return true;
            else return false;
        }

        public static bool FlashLightModuleEnabled(this Player player)
        {
            FirearmItem firearm = player.CurrentItem as FirearmItem;
            if (firearm.FlashLightStatus())
                return true;
            else return false;
        
        }

        public static CommandSender GetSender(this Player player)
        {
            return player.ReferenceHub.queryProcessor._sender;
        }

        public static void Vaporize(this Player player, Player? attacker = null)
        {
            ParticleDisruptor tempDisruptor = Object.Instantiate(InventoryItemLoader.AvailableItems[ItemType.ParticleDisruptor]) as ParticleDisruptor;

            if (tempDisruptor != null)
            {
                if (attacker != null)
                    tempDisruptor.Owner = attacker.ReferenceHub;

                DisruptorShotEvent shotEvent = new(tempDisruptor, DisruptorActionModule.FiringState.FiringSingle);
                DisruptorDamageHandler damageHandler = new(shotEvent, Vector3.up, -1);
                player.ReferenceHub.playerStats.KillPlayer(damageHandler);

                Object.Destroy(tempDisruptor.gameObject);
            }
        }

        public static void GiveCustomItem(this Player player, CustomItem customitem) => new SummonedCustomItem(customitem, player);

        public static bool HasCustomItem(this Player player, bool currentitem = false)
        {
            if (!currentitem)
            {
                foreach (Item item in player.Items)
                {
                    if (item.IsCustomItem())
                        return true;
                }
            }
            else if (player.CurrentItem.IsCustomItem())
            {
                return true;
            }

            return false;
        }

        private static int GetEffectIndex(this Player player, StatusEffectBase effect)
        {
            PlayerEffectsController controller = player.ReferenceHub.playerEffectsController;
            for (int i = 0; i < controller.EffectsLength; i++)
            {
                if (ReferenceEquals(controller.AllEffects[i], effect))
                    return i;
            }

            return -1;
        }
    }
}