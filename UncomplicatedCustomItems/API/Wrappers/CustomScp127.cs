using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using HarmonyLib;
using InventorySystem.Items.Firearms.Extensions;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using System;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Wrappers
{
    public class CustomScp127 : IWrapper<Firearm>
    {
        /// <summary>
        /// Gets the <see cref="InventorySystem.Items.Firearms.Firearm"/> that this class is encapsulating.
        /// </summary>
        public new Firearm Base { get; }

        public CustomScp127(Firearm firearm)
        {
            if (firearm.Type != ItemType.GunSCP127)
            {
                LogManager.Warn($"{firearm.Type} is not Scp127!");
                return;
            }
            Base = firearm;
            foreach (ModuleBase moduleBase in firearm.Base.Modules)
            {
                switch (moduleBase)
                {
                    case Scp127HumeModule hume when Scp127HumeModule == null:
                        Scp127HumeModule = hume;
                        break;

                    case Scp127Hitscan hitscan when Scp127Hitscan == null:
                        Scp127Hitscan = hitscan;
                        break;

                    case Scp127ActionModule action when Scp127ActionModule == null:
                        Scp127ActionModule = action;
                        break;

                    case Scp127TierManagerModule tier when Scp127TierManagerModule == null:
                        Scp127TierManagerModule = tier;
                        break;

                    case Scp127MagazineModule mag when Scp127MagazineModule == null:
                        Scp127MagazineModule = mag;
                        break;
                }
            }

        }

        /// <summary>
        /// Gets the HitRegistration module of the Scp127 instance.
        /// </summary>
        public Scp127Hitscan Scp127Hitscan { get; private set; }

        /// <summary>
        /// Gets the HumeShield module of the Scp127 instance.
        /// </summary>
        public Scp127HumeModule Scp127HumeModule { get; private set; }

        /// <summary>
        /// Gets the Action module of the Scp127 instance.
        /// </summary>
        public Scp127ActionModule Scp127ActionModule { get; private set; }

        /// <summary>
        /// Gets the TierManager module of the Scp127 instance.
        /// </summary>
        public Scp127TierManagerModule Scp127TierManagerModule { get; private set; }

        /// <summary>
        /// Gets the Magazine module of the Scp127 instance.
        /// </summary>
        public Scp127MagazineModule Scp127MagazineModule { get; private set; }

        public bool GiveHumeShield { get; set; } = false;

        [HarmonyPatch(typeof(Scp127HumeModule), nameof(Scp127HumeModule.HsMax), MethodType.Getter)]
        public static class MaxHumeShieldPatch
        {
            public static void Postfix(Scp127HumeModule __instance, ref float __result)
            {
                if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out SummonedCustomItem SCI) && SCI.CustomItem.CustomItemType == CustomItemType.SCPItem && SCI.Item.Type == ItemType.GunSCP127)
                {
                    try
                    {
                        ISCP127Data data = SCI.CustomItem.CustomData as ISCP127Data;
                        if (!data.GiveHumeShield)
                        {
                            __result = 0f;
                        }
                        else
                        {
                            __result = Scp127TierManagerModule.GetTierForItem(__instance.Item)
                            switch
                            {
                                Scp127Tier.Tier1 => data.Tier1HumeShieldAmount,
                                Scp127Tier.Tier2 => data.Tier2HumeShieldAmount,
                                Scp127Tier.Tier3 => data.Tier3HumeShieldAmount,
                                _ => __result
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(MaxHumeShieldPatch)}: {ex.Message}\n{ex.StackTrace}");
                    }

                }
            }
        }

        [HarmonyPatch(typeof(Scp127ActionModule), nameof(Scp127ActionModule.BaseFireRate), MethodType.Getter)]
        public static class FireRatePatch
        {
            public static void Postfix(Scp127ActionModule __instance, ref float __result)
            {
                if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out SummonedCustomItem SCI) && SCI.CustomItem.CustomItemType == CustomItemType.SCPItem && SCI.Item.Type == ItemType.GunSCP127)
                {
                    try
                    {
                        ISCP127Data data = SCI.CustomItem.CustomData as ISCP127Data;
                        __result = Scp127TierManagerModule.GetTierForItem(__instance.Item)
                        switch
                        {
                            Scp127Tier.Tier1 => data.Tier1BulletFireRate,
                            Scp127Tier.Tier2 => data.Tier2BulletFireRate,
                            Scp127Tier.Tier3 => data.Tier3BulletFireRate,
                            _ => __result
                        };
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(FireRatePatch)}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Scp127MagazineModule), "ActiveSettings", MethodType.Getter)]
        public static class BulletRegenSettingsPatch
        {
            public static void Postfix(Scp127MagazineModule __instance, ref object __result)
            {
                if (__result is not Scp127MagazineModule.RegenerationSettings original)
                    return;

                if (!Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out SummonedCustomItem SCI) && SCI.CustomItem.CustomItemType == CustomItemType.SCPItem && SCI.Item.Type == ItemType.GunSCP127)
                    return;
                try
                {
                    Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(__instance.Item);
                    ISCP127Data data = SCI.CustomItem.CustomData as ISCP127Data;

                    __result = new Scp127MagazineModule.RegenerationSettings
                    {
                        BulletsPerSecond = tier
                        switch
                        {
                            Scp127Tier.Tier1 => data.Tier1BulletRegenRate,
                            Scp127Tier.Tier2 => data.Tier2BulletRegenRate,
                            Scp127Tier.Tier3 => data.Tier3BulletRegenRate,
                            _ => original.BulletsPerSecond
                        },
                        PostFireDelay = tier
                        switch
                        {
                            Scp127Tier.Tier1 => data.Tier1BulletRegenPostFireDelay,
                            Scp127Tier.Tier2 => data.Tier2BulletRegenPostFireDelay,
                            Scp127Tier.Tier3 => data.Tier3BulletRegenPostFireDelay,
                            _ => original.PostFireDelay
                        }
                    };
                }
                catch (Exception ex)
                {
                    LogManager.Error($"{nameof(BulletRegenSettingsPatch)}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
        [HarmonyPatch(typeof(Scp127HumeModule), nameof(Scp127HumeModule.ShieldRegenRate), MethodType.Getter)] // Dosent work
        public static class ShieldRegenRatePatch
        {
            public static void Postfix(Scp127HumeModule __instance, ref float __result)
            {
                if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out SummonedCustomItem SCI) && SCI.CustomItem.CustomItemType == CustomItemType.SCPItem && SCI.Item.Type == ItemType.GunSCP127)
                {
                    try
                    {
                        ISCP127Data data = SCI.CustomItem.CustomData as ISCP127Data;
                        if (data.GiveHumeShield)
                        {
                            __result = Scp127TierManagerModule.GetTierForItem(__instance.Item)
                            switch
                            {
                                Scp127Tier.Tier1 => data.Tier3ShieldRegenRate,
                                Scp127Tier.Tier2 => data.Tier2ShieldRegenRate,
                                Scp127Tier.Tier3 => data.Tier3ShieldRegenRate,
                                _ => __result
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(ShieldRegenRatePatch)}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Scp127HumeModule), nameof(Scp127HumeModule.ShieldDecayRate), MethodType.Getter)] // Dosent work
        public static class ShieldDecayRatePatch
        {
            public static void Postfix(Scp127HumeModule __instance, ref float __result)
            {
                if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out SummonedCustomItem SCI) && SCI.CustomItem.CustomItemType == CustomItemType.SCPItem && SCI.Item.Type == ItemType.GunSCP127)
                {
                    try
                    {
                        ISCP127Data data = SCI.CustomItem.CustomData as ISCP127Data;
                        if (data.GiveHumeShield)
                        {
                            __result = Scp127TierManagerModule.GetTierForItem(__instance.Item)
                            switch
                            {
                                Scp127Tier.Tier1 => data.Tier1ShieldDecayRate,
                                Scp127Tier.Tier2 => data.Tier2ShieldDecayRate,
                                Scp127Tier.Tier3 => data.Tier3ShieldDecayRate,
                                _ => __result
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(ShieldDecayRatePatch)}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Scp127HumeModule), nameof(Scp127HumeModule.ShieldOnDamagePause), MethodType.Getter)] // Dosent work
        public static class ShieldOnDamagePausePatch
        {
            public static void Postfix(Scp127HumeModule __instance, ref float __result)
            {
                if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out SummonedCustomItem SCI) && SCI.CustomItem.CustomItemType == CustomItemType.SCPItem && SCI.Item.Type == ItemType.GunSCP127)
                {
                    try
                    {
                        ISCP127Data data = SCI.CustomItem.CustomData as ISCP127Data;
                        if (data.GiveHumeShield)
                        {
                            __result = Scp127TierManagerModule.GetTierForItem(__instance.Item)
                            switch
                            {
                                Scp127Tier.Tier1 => data.Tier1ShieldOnDamagePause,
                                Scp127Tier.Tier2 => data.Tier2ShieldOnDamagePause,
                                Scp127Tier.Tier3 => data.Tier3ShieldOnDamagePause,
                                _ => __result
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(ShieldOnDamagePausePatch)}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }
    }
}
