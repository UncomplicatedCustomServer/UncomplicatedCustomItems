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

                    if (!Room.TryGetRoomAtPosition(__instance.gameObject.transform.position, out Room? lockerRoom))
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
                        summoned.Pickup?.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.RifleRack:
                    if (locker is RifleRackLocker riflerack)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Rifle Rack");

                        LockerChamber targetChamber = null!;

                        targetChamber = Enum.Parse(typeof(RifleRackLockerChambers), data.LockerSettings.Chamber) switch
                        {
                            RifleRackLockerChambers.MainChamber => riflerack.MainChamber,
                            RifleRackLockerChambers.Bullet1 => riflerack.Bullet1,
                            RifleRackLockerChambers.Bullet2 => riflerack.Bullet2,
                            RifleRackLockerChambers.Bullet3 => riflerack.Bullet3,
                            RifleRackLockerChambers.Bullet4 => riflerack.Bullet4,
                            RifleRackLockerChambers.HeGrenade1 => riflerack.HeGrenade1,
                            RifleRackLockerChambers.HeGrenade2 => riflerack.HeGrenade2,
                            _ => riflerack.MainChamber,
                        };

                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup?.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.WallCabinet:
                    if (locker is WallCabinet wallCabinet)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in WallCabinet");

                        LockerChamber targetChamber = null!;

                        targetChamber = Enum.Parse(typeof(WallCabinetChambers), data.LockerSettings.Chamber) switch
                        {
                            WallCabinetChambers.MainChamber => wallCabinet.MainChamber,
                            WallCabinetChambers.LowerShelf => wallCabinet.LowerShelf,
                            WallCabinetChambers.UpperShelf => wallCabinet.UpperShelf,
                            _ => wallCabinet.MainChamber,
                        };

                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup?.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.StandardLocker:
                    if (locker is StandardLocker standardLocker)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Standard Locker");

                        LockerChamber targetChamber = null!;

                        targetChamber = Enum.Parse(typeof(StandardLockerChambers), data.LockerSettings.Chamber) switch
                        {
                            StandardLockerChambers.BottomLeft => standardLocker.BottomLeft,
                            StandardLockerChambers.BottomMiddle => standardLocker.BottomMiddle,
                            StandardLockerChambers.BottomRight => standardLocker.BottomRight,
                            StandardLockerChambers.MainLeft => standardLocker.MainLeft,
                            StandardLockerChambers.MainMiddle => standardLocker.MainMiddle,
                            StandardLockerChambers.MainRight => standardLocker.MainRight,
                            _ => standardLocker.MainMiddle,
                        };

                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup?.Position += data.LockerSettings.Offset;
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.LargeLocker:
                    if (locker is LargeLocker largeLocker)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Large Locker");

                        LockerChamber targetChamber = null!;

                        targetChamber = Enum.Parse(typeof(LargeLockerChambers), data.LockerSettings.Chamber) switch
                        {
                            LargeLockerChambers.BottomLeft => largeLocker.BottomLeft,
                            LargeLockerChambers.BottomMiddle => largeLocker.BottomMiddle,
                            LargeLockerChambers.BottomRight => largeLocker.BottomRight,
                            LargeLockerChambers.MiddleLeft => largeLocker.MiddleLeft,
                            LargeLockerChambers.MiddleRight => largeLocker.MiddleRight,
                            LargeLockerChambers.TopLeft => largeLocker.TopLeft,
                            LargeLockerChambers.TopMiddle => largeLocker.TopMiddle,
                            LargeLockerChambers.TopRight => largeLocker.TopRight,
                            _ => largeLocker.TopMiddle,
                        };

                        summoned = new SummonedCustomItem(item, targetChamber.AddItem(item.Item));
                        summoned.Pickup?.Position += data.LockerSettings.Offset;
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