using System;
using HarmonyLib;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Enums.LockerChambers;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(MapGeneration.Distributors.Locker))]
    internal static class LockerSpawningItemPrefix
    {
        private static Dictionary<uint, uint> spawnedAmounts = [];

        [HarmonyPatch(nameof(MapGeneration.Distributors.Locker.FillChamber))]
        public static bool Prefix(MapGeneration.Distributors.Locker __instance)
        {
            foreach (ICustomItem item in CustomItem.List)
            {
                if (!spawnedAmounts.ContainsKey(item.Id))
                    spawnedAmounts.Add(item.Id, 0);

                foreach (SpawnData data in item.Spawn.SpawnSettings)
                {
                    if (!data.LockerSettings.Enable)
                        continue;

                    if (!Room.TryGetRoomAtPosition(__instance.gameObject.transform.position, out Room lockerRoom))
                        continue;

                    Room customItemRoom = Utilities.GetRoomFromName(data.LockerSettings.Room);

                    if (UnityEngine.Random.Range(0f, 101f) > data.Chance)
                        continue;

                    if (!string.Equals(customItemRoom.Name.ToString(), lockerRoom.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        LogManager.Silent($"{customItemRoom.Name}, {lockerRoom.Name}");
                        continue;
                    }

                    if (lockerRoom.Zone != data.LockerSettings.Zone)
                        continue;

                    Locker locker = Locker.Get(__instance);
                    return HandleLockerSpawn(data, locker, item);
                }
            }

            return true;
        }

        internal static bool HandleLockerSpawn(SpawnData data, Locker locker, ICustomItem item)
        {
            if (spawnedAmounts[item.Id] >= item.Spawn.Count)
                return true;

            switch (data.LockerSettings.LockerType)
            {
                case LockerType.SCPPedestal:
                    if (locker is PedestalLocker pedestalLocker)
                    {
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in SCP Pedestal");
                        SummonedCustomItem summoned = new(item, pedestalLocker.AddItem(item.Item));
                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.RifleRack:
                    if (locker is RifleRackLocker riflerack)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Rifle Rack");

                        LockerChamber targetChamber = null;

                        switch (Enum.Parse(typeof(RifleRackLockerChambers), data.LockerSettings.Chamber))
                        {
                            case RifleRackLockerChambers.MainChamber:
                                targetChamber = riflerack.MainChamber;
                                break;
                            case RifleRackLockerChambers.Bullet1:
                                targetChamber = riflerack.Bullet1;
                                break;
                            case RifleRackLockerChambers.Bullet2:
                                targetChamber = riflerack.Bullet2;
                                break;
                            case RifleRackLockerChambers.Bullet3:
                                targetChamber = riflerack.Bullet3;
                                break;
                            case RifleRackLockerChambers.Bullet4:
                                targetChamber = riflerack.Bullet4;
                                break;
                            case RifleRackLockerChambers.HeGrenade1:
                                targetChamber = riflerack.HeGrenade1;
                                break;
                            case RifleRackLockerChambers.HeGrenade2:
                                targetChamber = riflerack.HeGrenade2;
                                break;
                            default:
                                targetChamber = riflerack.MainChamber;
                                break;
                        }
                        
                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.WallCabinet:
                    if (locker is WallCabinet wallCabinet)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in WallCabinet");

                        LockerChamber targetChamber = null;

                        switch (Enum.Parse(typeof(WallCabinetChambers), data.LockerSettings.Chamber))
                        {
                            case WallCabinetChambers.MainChamber:
                                targetChamber = wallCabinet.MainChamber;
                                break;
                            case WallCabinetChambers.LowerShelf:
                                targetChamber = wallCabinet.LowerShelf;
                                break;
                            case WallCabinetChambers.UpperShelf:
                                targetChamber = wallCabinet.UpperShelf;
                                break;
                            default:
                                targetChamber = wallCabinet.MainChamber;
                                break;
                        }

                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.StandardLocker:
                    if (locker is StandardLocker standardLocker)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Standard Locker");

                        LockerChamber targetChamber = null;

                        switch (Enum.Parse(typeof(StandardLockerChambers), data.LockerSettings.Chamber))
                        {
                            case StandardLockerChambers.BottomLeft:
                                targetChamber = standardLocker.BottomLeft;
                                break;
                            case StandardLockerChambers.BottomMiddle:
                                targetChamber = standardLocker.BottomMiddle;
                                break;
                            case StandardLockerChambers.BottomRight:
                                targetChamber = standardLocker.BottomRight;
                                break;
                            case StandardLockerChambers.MainLeft:
                                targetChamber = standardLocker.MainLeft;
                                break;
                            case StandardLockerChambers.MainMiddle:
                                targetChamber = standardLocker.MainMiddle;
                                break;
                            case StandardLockerChambers.MainRight:
                                targetChamber = standardLocker.MainRight;
                                break;
                            default:
                                targetChamber = standardLocker.MainMiddle;
                                break;
                        }

                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.LargeLocker:
                    if (locker is LargeLocker largeLocker)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Large Locker");

                        LockerChamber targetChamber = null;

                        switch (Enum.Parse(typeof(LargeLockerChambers), data.LockerSettings.Chamber))
                        {
                            case LargeLockerChambers.BottomLeft:
                                targetChamber = largeLocker.BottomLeft;
                                break;
                            case LargeLockerChambers.BottomMiddle:
                                targetChamber = largeLocker.BottomMiddle;
                                break;
                            case LargeLockerChambers.BottomRight:
                                targetChamber = largeLocker.BottomRight;
                                break;
                            case LargeLockerChambers.MiddleLeft:
                                targetChamber = largeLocker.MiddleLeft;
                                break;
                            case LargeLockerChambers.MiddleRight:
                                targetChamber = largeLocker.MiddleRight;
                                break;
                            case LargeLockerChambers.TopLeft:
                                targetChamber = largeLocker.TopLeft;
                                break;
                            case LargeLockerChambers.TopMiddle:
                                targetChamber = largeLocker.TopMiddle;
                                break;
                            case LargeLockerChambers.TopRight:
                                targetChamber = largeLocker.TopRight;
                                break;
                            default:
                                targetChamber = largeLocker.TopMiddle;
                                break;
                        }

                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }
    }
}