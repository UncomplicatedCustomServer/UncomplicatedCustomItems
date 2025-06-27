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

namespace UncomplicatedCustomItems.Extensions
{
    public static class PlayerExtensions
    {
        /// <summary>
        /// Checks whether the player has a keycard of a specific permission.
        /// </summary>
        /// <param name="player"><see cref="Player" /> trying to interact.</param>
        /// <param name="permissions">The permission that's gonna be searched for.</param>
        /// <param name="door"></param>
        /// <returns>Whether the player has the required keycard.</returns>
        internal static bool HasKeycardPermission(this Player player, DoorPermissionFlags permissions, IDoorPermissionRequester door)
        {
            return player.Items.Any(item => item is LabApi.Features.Wrappers.KeycardItem keycard && permissions == keycard.Base.GetPermissions(door));
        }
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
            ParticleDisruptor tempDisruptor = UnityEngine.Object.Instantiate(InventoryItemLoader.AvailableItems[ItemType.ParticleDisruptor]) as ParticleDisruptor;

            if (tempDisruptor != null)
            {
                if (attacker != null)
                {
                    tempDisruptor.Owner = attacker.ReferenceHub;
                }

                DisruptorShotEvent shotEvent = new DisruptorShotEvent(tempDisruptor, DisruptorActionModule.FiringState.FiringSingle);
                DisruptorDamageHandler damageHandler = new DisruptorDamageHandler(shotEvent, Vector3.up, -1);
                player.ReferenceHub.playerStats.KillPlayer(damageHandler);

                UnityEngine.Object.Destroy(tempDisruptor.gameObject);
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
    }
}