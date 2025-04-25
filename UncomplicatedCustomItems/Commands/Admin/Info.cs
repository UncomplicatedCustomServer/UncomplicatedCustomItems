using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Enums;
using UncomplicatedCustomItems.Extensions;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Info : ISubcommand
    {
        public string Name { get; } = "info";

        public string Description { get; } = "Get info on a summoned custom item";

        public string VisibleArgs { get; } = "<Item Id>";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.info";

        public string[] Aliases { get; } = ["info"];

        private string Color = null;
        private int Count = 0;

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            if (args.Count == 0)
            {
                response = $"usage: <Item Serial>";
                return false;
            }
            if (!ushort.TryParse(args[0], out ushort id) || !Utilities.TryGetCustomItem(id, out ICustomItem customItem))
            {
                response = $"CustomItem {args[0]} not found!";
                return false;
            }

            Dictionary<string, string> data = new()
            {
                { "<color=#00ffff>🔢</color> Id:", $"<b>{customItem.Id}</b>" },
                { "<color=#00ff00>🔪</color> Item:", $"<b>{customItem.Item}</b>" },
                { "<color=#00ff00>⚖</color> Scale:", $"<b>{customItem.Scale}</b>" },
                { "<color=#00ff00>⚖</color> Weight:", $"<b>{customItem.Weight}</b>" },
            };

            response = $"0\n<size=23><b>{customItem.Name} Info:</b></size>";

            if (customItem.Spawn is not null)
            {
                data.Add("<color=#632300>󾠬</color> Does It Spawn:", string.Join(", ", customItem.Spawn.DoSpawn));
                foreach (SummonedCustomItem SummonedCustomItem in SummonedCustomItem.List)
                {
                    if (SummonedCustomItem.CustomItem.Id == customItem.Id)
                    {
                        Count += 1;
                    }
                }
                data.Add("<color=#632300>📏</color> Amount Spawned:", string.Join(", ", Count));
                if (customItem.Spawn.Coords.Count >= 1)
                    data.Add("<color=#632300>󾠬</color> Spawn Coords:", string.Join(", ", customItem?.Spawn?.Coords));
                else if (customItem.Spawn.DynamicSpawn.Count >= 1)
                {
                    data.Add("<color=#632300>📂</color> Dynamic Spawn:", "");
                    foreach (DynamicSpawn DynamicSpawn in customItem.Spawn.DynamicSpawn)
                    {
                        data.Add("    <color=#632300>🎦</color> Spawn Rooms:", string.Join(", ", DynamicSpawn.Room));
                        data.Add("    <color=#632300>󾠬</color> Spawn Coords:", string.Join(", ", DynamicSpawn.Coords));
                        data.Add("    <color=#632300>🎲</color> Spawn Chance:", string.Join(", ", DynamicSpawn.Chance));
                    }
                }
                else if (customItem.Spawn.Zones.Count >= 1)
                    data.Add("<color=#632300>🇿</color> Spawn Zones:", string.Join(", ", customItem?.Spawn?.Zones));
            }
            if (customItem.FlagSettings.AudioSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.CustomSound))
            {
                data.Add("<color=#bf4eb6>📂</color> AudioSettings:", "");
                foreach (AudioSettings AudioSettings in customItem.FlagSettings.AudioSettings)
                {
                    data.Add("    <color=#bf4eb6>📏</color> Audible Distance:", string.Join(", ", AudioSettings.AudibleDistance));  
                    data.Add("    <color=#bf4eb6>📃</color> Audio Path:", string.Join(", ", AudioSettings.AudioPath));  
                    data.Add("    <color=#bf4eb6>🔉</color> Volume:", string.Join(", ", AudioSettings.SoundVolume));  
                }
            }
            if (customItem.FlagSettings.CantDropSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.CantDrop))
            {
                data.Add("<color=#bf4eb6>📂</color> CantDropSettings:", "");
                foreach (CantDropSettings CantDropSettings in customItem.FlagSettings.CantDropSettings)
                {
                    data.Add("    <color=#bf4eb6>💬</color> HintOrBroadcast:", string.Join(", ", CantDropSettings.HintOrBroadcast));
                    data.Add("    <color=#bf4eb6>💬</color> Message:", string.Join(", ", CantDropSettings.Message));
                    data.Add("    <color=#bf4eb6>🕛</color> Message Duration:", string.Join(", ", CantDropSettings.Duration));
                }
            }
            if (customItem.FlagSettings.ClusterSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.Cluster))
            {
                data.Add("<color=#bf4eb6>📂</color> ClusterSettings:", "");
                foreach (ClusterSettings ClusterSettings in customItem.FlagSettings.ClusterSettings)
                {
                    data.Add("    <color=#bf4eb6>#</color> Amount To Spawn:", string.Join(", ", ClusterSettings.AmountToSpawn));
                    data.Add("    <color=#bf4eb6>🕛</color> Fuse Time:", string.Join(", ", ClusterSettings.FuseTime));
                    data.Add("    <color=#bf4eb6>🔫</color> Items To Spawn:", string.Join(", ", ClusterSettings.ItemToSpawn));
                    data.Add("    <color=#bf4eb6>💥</color> Scp Damage Multiplier:", string.Join(", ", ClusterSettings.ScpDamageMultiplier));
                }
            }
            if (customItem.FlagSettings.DieOnDropSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.DieOnDrop))
            {
                data.Add("<color=#bf4eb6>📂</color> DieOnDropSettings:", "");
                foreach (DieOnDropSettings DieOnDropSettings in customItem.FlagSettings.DieOnDropSettings)
                {
                    data.Add("    <color=#bf4eb6>💬</color> Death Message:", string.Join(", ", DieOnDropSettings.DeathMessage));
                    data.Add("    <color=#bf4eb6>💦</color> Vaporize:", string.Join(", ", DieOnDropSettings.Vaporize));
                }
            }
            if (customItem.FlagSettings.EffectSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.EffectShot) || customItem.CustomFlags.Value.HasFlag(CustomFlags.EffectWhenEquiped) || customItem.CustomFlags.Value.HasFlag(CustomFlags.EffectWhenUsed))
            {
                data.Add("<color=#bf4eb6>📂</color> EffectSettings:", "");
                foreach (EffectSettings EffectSettings in customItem.FlagSettings.EffectSettings)
                {
                    data.Add("    <color=#bf4eb6>💻</color> Effect Event:", string.Join(", ", EffectSettings.EffectEvent));
                    data.Add("    <color=#bf4eb6>💉</color> Effect:", string.Join(", ", EffectSettings.Effect));
                    data.Add("    <color=#bf4eb6>📶</color> Effect Intensity:", string.Join(", ", EffectSettings.EffectIntensity));
                    data.Add("    <color=#bf4eb6>🕛</color> Effect Duration:", string.Join(", ", EffectSettings.EffectDuration));
                }
            }
            if (customItem.FlagSettings.ExplosiveBulletsSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.ExplosiveBullets))
            {
                data.Add("<color=#bf4eb6>📂</color> ExplosiveBulletsSettings:", "");
                foreach (ExplosiveBulletsSettings ExplosiveBulletsSettings in customItem.FlagSettings.ExplosiveBulletsSettings)
                {
                    data.Add("    <color=#bf4eb6>💥</color> Damage Radius:", string.Join(", ", ExplosiveBulletsSettings.DamageRadius));
                }
            }
            if (customItem.FlagSettings.ItemGlowSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.ItemGlow))
            {
                data.Add($"<color={Color}>📂</color> ItemGlowSettings:", "");
                foreach (ItemGlowSettings ItemGlowSettings in customItem.FlagSettings.ItemGlowSettings)
                {
                    data.Add($"    <color={Color}>🌟</color> Glow Color:", string.Join(", ", ItemGlowSettings.GlowColor));
                    Color = ItemGlowSettings.GlowColor;
                }
            }
            if (customItem.FlagSettings.LifeStealSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.LifeSteal))
            {
                data.Add("<color=#bf4eb6>📂</color> LifeStealSettings:", "");
                foreach (LifeStealSettings LifeStealSettings in customItem.FlagSettings.LifeStealSettings)
                {
                    data.Add("    <color=#bf4eb6>💊</color> LifeSteal Amount:", string.Join(", ", LifeStealSettings.LifeStealAmount));
                    data.Add("    <color=#bf4eb6>💊</color> LifeSteal Percentage:", string.Join(", ", LifeStealSettings.LifeStealPercentage));
                }
            }
            if (customItem.FlagSettings.SpawnItemWhenDetonatedSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.SpawnItemWhenDetonated))
            {
                data.Add("<color=#bf4eb6>📂</color> SpawnItemWhenDetonatedSettings:", "");
                foreach (SpawnItemWhenDetonatedSettings SpawnItemWhenDetonatedSettings in customItem.FlagSettings.SpawnItemWhenDetonatedSettings)
                {
                    data.Add("    <color=#bf4eb6>🔫</color> Item To Spawn:", string.Join(", ", SpawnItemWhenDetonatedSettings.ItemToSpawn));
                    data.Add("    <color=#bf4eb6>🎲</color> Chance:", string.Join(", ", SpawnItemWhenDetonatedSettings.Chance));
                    data.Add("    <color=#bf4eb6>🛠️</color> Pickupable:", string.Join(", ", SpawnItemWhenDetonatedSettings.Pickupable));
                    data.Add("    <color=#bf4eb6>🕛</color> TimeTillDespawn:", string.Join(", ", SpawnItemWhenDetonatedSettings.TimeTillDespawn));
                }
            }
            if (customItem.FlagSettings.SwitchRoleOnUseSettings != null && customItem.CustomFlags.Value.HasFlag(CustomFlags.SwitchRoleOnUse))
            {
                data.Add("<color=#bf4eb6>📂</color> SwitchRoleOnUseSettings:", "");
                foreach (SwitchRoleOnUseSettings SwitchRoleOnUseSettings in customItem.FlagSettings.SwitchRoleOnUseSettings)
                {
                    data.Add("    <color=#bf4eb6>🔂</color> Delay:", string.Join(", ", SwitchRoleOnUseSettings.Delay));
                    data.Add("    <color=#bf4eb6>🔒</color> Keep Location:", string.Join(", ", SwitchRoleOnUseSettings.KeepLocation));
                    data.Add("    <color=#bf4eb6>🆔</color> RoleId:", string.Join(", ", SwitchRoleOnUseSettings.RoleId));
                    data.Add("    <color=#bf4eb6>🚶</color> RoleType:", string.Join(", ", SwitchRoleOnUseSettings.RoleType));
                    data.Add("    <color=#bf4eb6>󾓦</color> SpawnFlags:", string.Join(", ", SwitchRoleOnUseSettings.SpawnFlags));                    
                }
            }
            if (customItem.CustomFlags.HasValue)
                data.Add("<color=#bf4eb6>📄</color> Custom flags:", string.Join(", ", customItem.CustomFlags.ToString()));
            foreach (KeyValuePair<string, string> kvp in data)
                response += $"\n{kvp.Key.GenerateWithBuffer(40)} {kvp.Value}";
            return true;
        }
    }
}
