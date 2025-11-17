using System;
using HarmonyLib;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(MapGeneration.Distributors.Locker))]
    internal static class LockerSpawningItemPrefix
    {
        [HarmonyPatch("FillChamber")]
        public static bool Prefix(MapGeneration.Distributors.Locker __instance)
        {
            foreach (ICustomItem item in CustomItem.List)
            {
                if (!item.Spawn.DoSpawn)
                    continue;

                foreach (SpawnData data in item.Spawn.SpawnSettings)
                {
                    if (!data.LockerSettings.Enable)
                        continue;

                    if (!Room.TryGetRoomAtPosition(__instance.gameObject.transform.position, out Room lockerRoom))
                        continue;

                    Room customItemRoom = Utilities.GetRoomFromName(data.LockerSettings.Room);

                    float chance = UnityEngine.Random.Range(0f, 101f);
                    if (chance >= data.Chance)
                        continue;

                    if (!string.Equals(customItemRoom.Name.ToString(), lockerRoom.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (lockerRoom.Zone != data.LockerSettings.Zone)
                        continue;

                    Locker locker = Locker.Get(__instance);

                    bool spawned = HandleLockerSpawn(data, locker, item);

                    return spawned;
                }
            }

            return true;
        }

        internal static bool HandleLockerSpawn(SpawnData data, Locker locker, ICustomItem item)
        {

            switch (data.LockerSettings.LockerType)
            {
                case LockerType.SCPPedestal:
                    if (locker is PedestalLocker pedestalLocker)
                    {
                        LogManager.Silent($"{nameof(LockerSpawningItemPrefix)}: Spawning in SCP Pedestal");
                        new SummonedCustomItem(item, pedestalLocker.AddItem(item.Item));
                    }

                    break;
                case LockerType.RifleRack:
                    if (locker is RifleRackLocker riflerack)
                    {
                        LogManager.Silent($"{nameof(LockerSpawningItemPrefix)}: Spawning in Rifle Rack");
                        switch (data.LockerSettings.Chamber)
                        {
                            case 1:
                                new SummonedCustomItem(item, riflerack.MainChamber.AddItem(item.Item));
                                break;
                            case 2:
                                new SummonedCustomItem(item, riflerack.Bullet1.AddItem(item.Item));
                                break;
                            case 3:
                                new SummonedCustomItem(item, riflerack.Bullet2.AddItem(item.Item));
                                break;
                            case 4:
                                new SummonedCustomItem(item, riflerack.Bullet3.AddItem(item.Item));
                                break;
                            case 5:
                                new SummonedCustomItem(item, riflerack.Bullet4.AddItem(item.Item));
                                break;
                            case 6:
                                new SummonedCustomItem(item, riflerack.HeGrenade1.AddItem(item.Item));
                                break;
                            case 7:
                                new SummonedCustomItem(item, riflerack.HeGrenade2.AddItem(item.Item));
                                break;
                            default:
                                new SummonedCustomItem(item, riflerack.MainChamber.AddItem(item.Item));
                                break;
                        }
                        return false;
                    }
                    break;

                case LockerType.WallCabinet:
                    if (locker is WallCabinet wallCabinet)
                    {
                        LogManager.Silent($"{nameof(LockerSpawningItemPrefix)}: Spawning in WallCabinet");
                        switch (data.LockerSettings.Chamber)
                        {
                            case 1:
                                new SummonedCustomItem(item, wallCabinet.MainChamber.AddItem(item.Item));
                                break;
                            case 2:
                                new SummonedCustomItem(item, wallCabinet.LowerShelf.AddItem(item.Item));
                                break;
                            case 3:
                                new SummonedCustomItem(item, wallCabinet.UpperShelf.AddItem(item.Item));
                                break;
                            default:
                                new SummonedCustomItem(item, wallCabinet.MainChamber.AddItem(item.Item));
                                break;
                        }
                        return false;
                    }
                    break;

                case LockerType.StandardLocker:
                    if (locker is StandardLocker standardLocker)
                    {
                        LogManager.Silent($"{nameof(LockerSpawningItemPrefix)}: Spawning in Standard Locker");
                        switch (data.LockerSettings.Chamber)
                        {
                            case 1:
                                new SummonedCustomItem(item, standardLocker.BottomLeft.AddItem(item.Item));
                                break;
                            case 2:
                                new SummonedCustomItem(item, standardLocker.BottomMiddle.AddItem(item.Item));
                                break;
                            case 3:
                                new SummonedCustomItem(item, standardLocker.BottomRight.AddItem(item.Item));
                                break;
                            case 4:
                                new SummonedCustomItem(item, standardLocker.MainLeft.AddItem(item.Item));
                                break;
                            case 5:
                                new SummonedCustomItem(item, standardLocker.MainMiddle.AddItem(item.Item));
                                break;
                            case 6:
                                new SummonedCustomItem(item, standardLocker.MainRight.AddItem(item.Item));
                                break;
                            default:
                                new SummonedCustomItem(item, standardLocker.MainMiddle.AddItem(item.Item));
                                break;
                        }
                        return false;
                    }
                    break;

                case LockerType.LargeLocker:
                    if (locker is LargeLocker largeLocker)
                    {
                        LogManager.Silent($"{nameof(LockerSpawningItemPrefix)}: Spawning in Large Locker");
                        switch (data.LockerSettings.Chamber)
                        {
                            case 1:
                                new SummonedCustomItem(item, largeLocker.BottomLeft.AddItem(item.Item));
                                break;
                            case 2:
                                new SummonedCustomItem(item, largeLocker.BottomMiddle.AddItem(item.Item));
                                break;
                            case 3:
                                new SummonedCustomItem(item, largeLocker.BottomRight.AddItem(item.Item));
                                break;
                            case 4:
                                new SummonedCustomItem(item, largeLocker.MiddleLeft.AddItem(item.Item));
                                break;
                            case 5:
                                new SummonedCustomItem(item, largeLocker.MiddleRight.AddItem(item.Item));
                                break;
                            case 6:
                                new SummonedCustomItem(item, largeLocker.TopLeft.AddItem(item.Item));
                                break;
                            case 7:
                                new SummonedCustomItem(item, largeLocker.TopMiddle.AddItem(item.Item));
                                break;
                            case 8:
                                new SummonedCustomItem(item, largeLocker.TopRight.AddItem(item.Item));
                                break;
                            default:
                                new SummonedCustomItem(item, largeLocker.TopMiddle.AddItem(item.Item));
                                break;
                        }
                        return false;
                    }
                    break;
            }
            return true;
        }
    }
}