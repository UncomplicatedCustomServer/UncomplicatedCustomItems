using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.YamlObjects;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AttachmentsUtils), nameof(AttachmentsUtils.AttachmentsValue))]
    public static class AttachmentParameterPatch
    {
        public static void Postfix(Firearm firearm, AttachmentParam param, ref float __result)
        {
            if (!SummonedCustomItem.TryGet(firearm.ItemSerial, out var item) || item == null)
                return;

            if (item.CustomItem.CustomData is not WeaponData data)
                return;

            if (param == AttachmentParam.MagazineCapacityModifier)
                __result = 0f;

            if (data.ParameterModifiers is null)
                return;

            foreach (ParameterObject? paramobj in data.ParameterModifiers)
            {
                if (paramobj is null)
                    continue;

                if (param != paramobj.Parameter)
                    continue;

                switch (paramobj.Type)
                {
                    case MathType.Multiplier:
                        __result *= paramobj.Value;
                        break;

                    case MathType.Division:
                        __result /= paramobj.Value;
                        break;

                    case MathType.Addition:
                        __result += paramobj.Value;
                        break;

                    case MathType.Subtraction:
                        __result -= paramobj.Value;
                        break;
                }
            }
        }
    }
}