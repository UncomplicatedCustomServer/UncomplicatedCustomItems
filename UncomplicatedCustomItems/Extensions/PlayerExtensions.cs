using System.Collections.Generic;
using System.Linq;
using GameCore;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp1507;
using PlayerRoles;
using UnityEngine;
using UncomplicatedCustomItems.API.Features.Helper;
using RelativePositioning;
using static UnityEngine.UI.GridLayoutGroup;
using CustomPlayerEffects;
using System;

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
    }
}