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

        public static void ChangeAppearance(this Player player, RoleTypeId type, bool skipJump = false, byte unitId = 0) => ChangeAppearance(player, type, Player.List.Where(x => x != player), skipJump, unitId);

        /// <summary>
        /// Change <see cref="Player"/> character model for appearance.
        /// It will continue until <see cref="Player"/>'s <see cref="RoleTypeId"/> changes.
        /// </summary>
        /// <param name="player">Player to change.</param>
        /// <param name="type">Model type.</param>
        /// <param name="playersToAffect">The players who should see the changed appearance.</param>
        /// <param name="skipJump">Whether to skip the little jump that works around an invisibility issue.</param>
        /// <param name="unitId">The UnitNameId to use for the player's new role, if the player's new role uses unit names. (is NTF).</param>
        public static void ChangeAppearance(this Player player, RoleTypeId type, IEnumerable<Player> playersToAffect, bool skipJump = false, byte unitId = 0)
        {
            RelativePosition relativePos = new RelativePosition(player.Position);
            if (player.GameObject == null || !PlayerRoleLoader.TryGetRoleTemplate(type, out PlayerRoleBase roleBase))
                return;

            bool isRisky = type.GetTeam() == Team.Dead || player.Role == RoleTypeId.Spectator || player.Role == RoleTypeId.Destroyed;

            NetworkWriterPooled writer = NetworkWriterPool.Get();
            writer.WriteUShort(38952);
            writer.WriteUInt(player.NetworkId);
            writer.WriteRoleType(type);

            if (roleBase is HumanRole humanRole && humanRole.UsesUnitNames)
            {
                if (player.Role.GetRoleBase() is not HumanRole)
                    isRisky = true;
                writer.WriteByte(unitId);
            }

            if (roleBase is ZombieRole)
            {
                if (player.Role.GetRoleBase() is not ZombieRole)
                    isRisky = true;

                writer.WriteUShort((ushort)Mathf.Clamp(Mathf.CeilToInt(player.MaxHealth), ushort.MinValue, ushort.MaxValue));
                writer.WriteBool(true);
            }

            if (roleBase is Scp1507Role)
            {
                if (player.Role.GetRoleBase() is not Scp1507Role)
                    isRisky = true;

                writer.WriteByte((byte)player.RoleBase.ServerSpawnReason);
            }

            if (roleBase is FpcStandardRoleBase fpc)
            {
                if (player.Role.GetRoleBase() is not FpcStandardRoleBase playerfpc)
                    isRisky = true;
                else
                    fpc = playerfpc;

                ushort value = 0;
                fpc?.FpcModule.MouseLook.GetSyncValues(0, out value, out ushort _);
                writer.WriteRelativePosition(relativePos);
                writer.WriteUShort(value);
            }

            foreach (Player target in playersToAffect)
            {
                if (target != player || !isRisky)
                    target.Connection.Send(writer.ToArraySegment());
                else
                    LogManager.Error($"Prevent Seld-Desync of {player.Nickname} with {type}");
            }

            NetworkWriterPool.Return(writer);

            // To counter a bug that makes the player invisible until they move after changing their appearance, we will teleport them upwards slightly to force a new position update for all clients.
            if (!skipJump)
                player.Position += Vector3.up * 0.25f;
        }
    }
}