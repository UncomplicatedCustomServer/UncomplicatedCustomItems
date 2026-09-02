using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.ShotEvents;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class PlayerExtensions
    {
        public static Dictionary<Player, int> PlayerKills { get; set; } = [];
        
        public static CommandSender GetSender(this Player player) => player.ReferenceHub.queryProcessor._sender;

        private static ParticleDisruptor? DisruptorCache = null;

        public static void Vaporize(this Player player, Player? attacker = null)
        {
            if (DisruptorCache == null)
            {
                if (!InventoryItemLoader.TryGetItem(ItemType.ParticleDisruptor, out ParticleDisruptor disruptor))
                    return;

                DisruptorCache = disruptor;
            }

            if (attacker != null)
                DisruptorCache.Owner = attacker.ReferenceHub;

            DisruptorShotEvent shotEvent = new(DisruptorCache, DisruptorActionModule.FiringState.FiringSingle);
            DisruptorDamageHandler damageHandler = new(shotEvent, Vector3.up, -1);
            player.ReferenceHub.playerStats.KillPlayer(damageHandler);
        }

        public static void GiveCustomItem(this Player player, CustomItem customitem) => new SummonedCustomItem(customitem, player);

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
            else if (player.CurrentItem?.IsSummonedCustomItem() ?? false)
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