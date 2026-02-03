using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace UncomplicatedCustomItems.API.Enums
{
    public enum CommonTypes
    {
        [CommonTypes(typeof(Vector2))]
        Vector2,

        [CommonTypes(typeof(Vector3))]
        Vector3,

        [CommonTypes(typeof(Vector4))]
        Vector4,

        [CommonTypes(typeof(Quaternion))]
        Quaternion,

        [CommonTypes(typeof(Transform))]
        Transform,

        [CommonTypes(typeof(GameObject))]
        GameObject,

        [CommonTypes(typeof(Timing))]
        Timing,

        [CommonTypes(typeof(Player))]
        StaticPlayer,

        [CommonTypes(typeof(Logger))]
        Log,

        [CommonTypes(typeof(Logger))]
        Logger,

        [CommonTypes(typeof(Room))]
        Room,

        [CommonTypes(typeof(RoleTypeId))]
        RoleTypeId,

        [CommonTypes(typeof(Item))]
        Item,

        [CommonTypes(typeof(SummonedCustomItem))]
        SummonedCustomItem,

        [CommonTypes(typeof(CustomItem))]
        CustomItem,

        [CommonTypes(typeof(CustomAction))]
        CustomAction,

        [CommonTypes(typeof(Announcer))]
        Cassie,

        [CommonTypes(typeof(Announcer))]
        Announcer,

        [CommonTypes(typeof(Elevator))]
        Elevator,

        [CommonTypes(typeof(Pickup))]
        StaticPickup,

        [CommonTypes(typeof(Map))]
        Map,

        [CommonTypes(typeof(Projectile))]
        Projectile,

        [CommonTypes(typeof(Round))]
        Round,

        [CommonTypes(typeof(Server))]
        Server,

        [CommonTypes(typeof(Physics))]
        Physics,

        [CommonTypes(typeof(Ragdoll))]
        Ragdoll,

        [CommonTypes(typeof(RespawnWave))]
        RespawnWave,

        [CommonTypes(typeof(LabApi.Features.Wrappers.Scp914))]
        Scp914,

        [CommonTypes(typeof(Tesla))]
        Tesla,

        [CommonTypes(typeof(ThrowableItem))]
        ThrowableItem,

        [CommonTypes(typeof(Workstation))]
        Workstation,

        [CommonTypes(typeof(Door))]
        Door,

        [CommonTypes(typeof(BreakableDoor))]
        BreakableDoor,

        [CommonTypes(typeof(Gate))]
        Gate,

        [CommonTypes(typeof(ElevatorDoor))]
        ElevatorDoor,

        [CommonTypes(typeof(BulkheadDoor))]
        BulkheadDoor,
        
        [CommonTypes(typeof(DummyDoor))]
        DummyDoor,

        [CommonTypes(typeof(DoorCrusher))]
        DoorCrusher,
    }
}