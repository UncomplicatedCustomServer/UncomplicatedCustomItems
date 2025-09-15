using System;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules.Scp127;
using Mirror;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Scp127VoiceTriggerBase), nameof(Scp127VoiceTriggerBase.ServerPlayVoiceLine), [typeof(AudioClip), typeof(Action<NetworkWriter>), typeof(Scp127VoiceTriggerBase.VoiceLinePriority)])]
    public static class VoiceTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Scp127VoiceTriggerBase __instance, AudioClip clip, Action<NetworkWriter> extraData, Scp127VoiceTriggerBase.VoiceLinePriority priority)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out var item))
            {
                if (item.CustomItem.CustomItemType == CustomItemType.SCPItem && item.Item.Type == ItemType.GunSCP127)
                {
                    try
                    {
                        SCP127Data data = item.CustomItem.CustomData as SCP127Data;
                        if (data.MuteVoiceLines)
                            return false;
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(VoiceTriggerPatch)} Error checking MuteVoiceLines for item {__instance.Item.ItemSerial}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            return true;
        }
    }
}