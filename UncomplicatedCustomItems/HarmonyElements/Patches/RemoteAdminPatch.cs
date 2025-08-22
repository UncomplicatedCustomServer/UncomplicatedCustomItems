using HarmonyLib;
using LabApi.Features.Wrappers;
using RemoteAdmin.Communication;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(RaPlayerList), nameof(RaPlayerList.GetPrefix))]
    internal static class RemoteAdminPatch
    {
        public static void Postfix(ref string __result, ReferenceHub hub, bool viewHiddenBadges = false, bool viewHiddenGlobalBadges = false)
        {
            Player player = Player.Get(hub);
            if (BadgeManager.devBadges.ContainsKey(player.UserId))
            {
                string badgeColor = BadgeManager.devBadges[player.UserId].Item2;
                if (BadgeManager.colorMap.TryGetValue(badgeColor, out string hexcolor))
                {
                    string icon = $"[<color={hexcolor}>💻</color>] ";
                    __result = icon + __result;
                }
            }
        }
    }
}