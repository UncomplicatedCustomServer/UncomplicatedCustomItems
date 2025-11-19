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
        private static List<Locker> usedLockers = [];
        private static Dictionary<uint, uint> spawnedAmounts = [];

        [HarmonyPatch(nameof(MapGeneration.Distributors.Locker.FillChamber))]
        public static bool Prefix(MapGeneration.Distributors.Locker __instance)
        {
            foreach (ICustomItem item in CustomItem.List)
            {
                if (!item.Spawn.DoSpawn)
                    continue;

                if (!spawnedAmounts.ContainsKey(item.Id))
                    spawnedAmounts.Add(item.Id, 0);

                foreach (SpawnData data in item.Spawn.SpawnSettings)
                {
                    if (!data.LockerSettings.Enable)
                        continue;

                    if (!Room.TryGetRoomAtPosition(__instance.gameObject.transform.position, out Room lockerRoom))
                        continue;

                    Room customItemRoom = Utilities.GetRoomFromName(data.LockerSettings.Room);

                    float chance = UnityEngine.Random.Range(0f, 101f);
                    if (chance > data.Chance)
                        continue;

                    if (!string.Equals(customItemRoom.Name.ToString(), lockerRoom.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        LogManager.Debug($"{customItemRoom.Name.ToString()}, {lockerRoom.Name.ToString()}");
                        continue;
                    }

                    if (lockerRoom.Zone != data.LockerSettings.Zone)
                        continue;

                    Locker locker = Locker.Get(__instance);
                    if (usedLockers.Contains(locker))
                        continue;

                    return HandleLockerSpawn(data, locker, item);
                }
            }

            return true;
        }

        internal static bool HandleLockerSpawn(SpawnData data, Locker locker, ICustomItem item)
        {
            if (usedLockers.Contains(locker))
                return true;

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
                        usedLockers.Add(locker);
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;
                    
                case LockerType.RifleRack:
                    if (locker is RifleRackLocker riflerack)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Rifle Rack");
                        switch (Enum.Parse(typeof(RifleRackLockerChambers), data.LockerSettings.Chamber))
                        {
                            case RifleRackLockerChambers.MainChamber:
                                summoned = new SummonedCustomItem(item, riflerack.MainChamber.AddItem(item.Item));
                                break;
                            case RifleRackLockerChambers.Bullet1:
                                summoned = new SummonedCustomItem(item, riflerack.Bullet1.AddItem(item.Item));
                                break;
                            case RifleRackLockerChambers.Bullet2:
                                summoned = new SummonedCustomItem(item, riflerack.Bullet2.AddItem(item.Item));
                                break;
                            case RifleRackLockerChambers.Bullet3:
                                summoned = new SummonedCustomItem(item, riflerack.Bullet3.AddItem(item.Item));
                                break;
                            case RifleRackLockerChambers.Bullet4:
                                summoned = new SummonedCustomItem(item, riflerack.Bullet4.AddItem(item.Item));
                                break;
                            case RifleRackLockerChambers.HeGrenade1:
                                summoned = new SummonedCustomItem(item, riflerack.HeGrenade1.AddItem(item.Item));
                                break;
                            case RifleRackLockerChambers.HeGrenade2:
                                summoned = new SummonedCustomItem(item, riflerack.HeGrenade2.AddItem(item.Item));
                                break;
                            default:
                                summoned = new SummonedCustomItem(item, riflerack.MainChamber.AddItem(item.Item));
                                break;
                        }

                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        usedLockers.Add(locker);
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.WallCabinet:
                    if (locker is WallCabinet wallCabinet)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in WallCabinet");
                        switch (Enum.Parse(typeof(WallCabinetChambers), data.LockerSettings.Chamber))
                        {
                            case WallCabinetChambers.MainChamber:
                                summoned = new SummonedCustomItem(item, wallCabinet.MainChamber.AddItem(item.Item));
                                break;
                            case WallCabinetChambers.LowerShelf:
                                summoned = new SummonedCustomItem(item, wallCabinet.LowerShelf.AddItem(item.Item));
                                break;
                            case WallCabinetChambers.UpperShelf:
                                summoned = new SummonedCustomItem(item, wallCabinet.UpperShelf.AddItem(item.Item));
                                break;
                            default:
                                summoned = new SummonedCustomItem(item, wallCabinet.MainChamber.AddItem(item.Item));
                                break;
                        }

                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        usedLockers.Add(locker);
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.StandardLocker:
                    if (locker is StandardLocker standardLocker)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Standard Locker");
                        switch (Enum.Parse(typeof(StandardLockerChambers), data.LockerSettings.Chamber))
                        {
                            case StandardLockerChambers.BottomLeft:
                                summoned = new SummonedCustomItem(item, standardLocker.BottomLeft.AddItem(item.Item));
                                break;
                            case StandardLockerChambers.BottomMiddle:
                                summoned = new SummonedCustomItem(item, standardLocker.BottomMiddle.AddItem(item.Item));
                                break;
                            case StandardLockerChambers.BottomRight:
                                summoned = new SummonedCustomItem(item, standardLocker.BottomRight.AddItem(item.Item));
                                break;
                            case StandardLockerChambers.MainLeft:
                                summoned = new SummonedCustomItem(item, standardLocker.MainLeft.AddItem(item.Item));
                                break;
                            case StandardLockerChambers.MainMiddle:
                                summoned = new SummonedCustomItem(item, standardLocker.MainMiddle.AddItem(item.Item));
                                break;
                            case StandardLockerChambers.MainRight:
                                summoned = new SummonedCustomItem(item, standardLocker.MainRight.AddItem(item.Item));
                                break;
                            default:
                                summoned = new SummonedCustomItem(item, standardLocker.MainMiddle.AddItem(item.Item));
                                break;
                        }

                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        usedLockers.Add(locker);
                        spawnedAmounts[item.Id]++;
                        return false;
                    }
                    break;

                case LockerType.LargeLocker:
                    if (locker is LargeLocker largeLocker)
                    {
                        SummonedCustomItem summoned;
                        LogManager.Debug($"{nameof(LockerSpawningItemPrefix)}: Spawning in Large Locker");
                        switch (Enum.Parse(typeof(LargeLockerChambers), data.LockerSettings.Chamber))
                        {
                            case LargeLockerChambers.BottomLeft:
                                summoned = new SummonedCustomItem(item, largeLocker.BottomLeft.AddItem(item.Item));
                                break;
                            case LargeLockerChambers.BottomMiddle:
                                summoned = new SummonedCustomItem(item, largeLocker.BottomMiddle.AddItem(item.Item));
                                break;
                            case LargeLockerChambers.BottomRight:
                                summoned = new SummonedCustomItem(item, largeLocker.BottomRight.AddItem(item.Item));
                                break;
                            case LargeLockerChambers.MiddleLeft:
                                summoned = new SummonedCustomItem(item, largeLocker.MiddleLeft.AddItem(item.Item));
                                break;
                            case LargeLockerChambers.MiddleRight:
                                summoned = new SummonedCustomItem(item, largeLocker.MiddleRight.AddItem(item.Item));
                                break;
                            case LargeLockerChambers.TopLeft:
                                summoned = new SummonedCustomItem(item, largeLocker.TopLeft.AddItem(item.Item));
                                break;
                            case LargeLockerChambers.TopMiddle:
                                summoned = new SummonedCustomItem(item, largeLocker.TopMiddle.AddItem(item.Item));
                                break;
                            case LargeLockerChambers.TopRight:
                                summoned = new SummonedCustomItem(item, largeLocker.TopRight.AddItem(item.Item));
                                break;
                            default:
                                summoned = new SummonedCustomItem(item, largeLocker.TopMiddle.AddItem(item.Item));
                                break;
                        }

                        summoned.Pickup.Position += data.LockerSettings.Offset;
                        usedLockers.Add(locker);
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