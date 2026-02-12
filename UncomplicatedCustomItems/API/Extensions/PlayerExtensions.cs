using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.ShotEvents;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class PlayerExtensions
    {
        public static Dictionary<Player, int> PlayerKills { get; set; } = [];

        /// <summary>
        /// Checks whether the player has a keycard of a specific permission.
        /// </summary>
        /// <param name="player"><see cref="Player" /> trying to interact.</param>
        /// <param name="door"></param>
        /// <returns>Whether the player has the required keycard.</returns>
        public static bool HasKeycardPermission(this Player player, IDoorPermissionRequester door) =>
            player.CurrentItem is KeycardItem keycard && player.CurrentItem.Base is IDoorPermissionProvider keycardProvider && door is IDoorPermissionRequester permissions && permissions.PermissionsPolicy.CheckPermissions(keycardProvider.GetPermissions(permissions));

        public static CommandSender GetSender(this Player player) => player.ReferenceHub.queryProcessor._sender;

        public static void Vaporize(this Player player, Player? attacker = null)
        {
            if (!InventoryItemLoader.TryGetItem(ItemType.ParticleDisruptor, out ParticleDisruptor disruptor))
                return;

            if (attacker != null)
                disruptor.Owner = attacker.ReferenceHub;

            DisruptorShotEvent shotEvent = new(disruptor, DisruptorActionModule.FiringState.FiringSingle);
            DisruptorDamageHandler damageHandler = new(shotEvent, Vector3.up, -1);
            player.ReferenceHub.playerStats.KillPlayer(damageHandler);
        }

        public static void GiveCustomItem(this Player player, ICustomItem customitem) => new SummonedCustomItem(customitem, player);

        public static bool HasCustomItem(this Player player, bool currentitem = false)
        {
            if (!currentitem)
            {
                foreach (Item item in player.Items)
                {
                    if (item.IsSummonedCustomItem())
                        return true;
                }
            }
            else if (player.CurrentItem.IsSummonedCustomItem())
                return true;

            return false;
        }

        public static int TotalKills(this Player player)
        {
            PlayerKills.TryGetValue(player, out int kills);
            return kills;
        }

        public static IEnumerable<Player> RealList(this IEnumerable<Player> players) => players.Where(p => p.IsReady && p.IsPlayer && !p.IsHost && !p.IsDummy && !p.IsNpc);
    }
}